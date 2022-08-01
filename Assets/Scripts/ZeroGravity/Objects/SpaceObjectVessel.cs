using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.Objects
{
	public abstract class SpaceObjectVessel : ArtificialBody, IMapMainObject
	{
		public enum VesselObjectType
		{
			SubSystem = 1,
			Generator = 2,
			RoomTrigger = 3,
			ResourceContainer = 4,
			Door = 5,
			SceneTriggerExecuter = 6,
			AttachPoint = 7,
			DockingPort = 8,
			SpawnPoint = 9,
			NameTag = 10,
			RepairPoint = 11
		}

		[CompilerGenerated]
		private sealed class _003CUpdateDynamicObjects_003Ec__AnonStorey0
		{
			internal DynamicObject obj;

			internal bool _003C_003Em__0(DynamicObjectDetails m)
			{
				return m.GUID == obj.GUID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CUpdateCharacters_003Ec__AnonStorey1
		{
			internal OtherPlayer opl;

			internal bool _003C_003Em__0(CharacterDetails m)
			{
				return m.GUID == opl.GUID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CUpdateCorpses_003Ec__AnonStorey2
		{
			internal Corpse obj;

			internal bool _003C_003Em__0(CorpseDetails m)
			{
				return m.GUID == obj.GUID;
			}
		}

		public GameObject RootObject;

		public Dictionary<int, SubSystem> SubSystems = new Dictionary<int, SubSystem>();

		public Dictionary<int, Generator> Generators = new Dictionary<int, Generator>();

		public Dictionary<int, SceneTriggerRoom> RoomTriggers = new Dictionary<int, SceneTriggerRoom>();

		public Dictionary<int, ResourceContainer> ResourceContainers = new Dictionary<int, ResourceContainer>();

		public Dictionary<int, SceneDoor> Doors = new Dictionary<int, SceneDoor>();

		public Dictionary<int, SceneTriggerExecuter> SceneTriggerExecuters = new Dictionary<int, SceneTriggerExecuter>();

		public Dictionary<int, BaseSceneAttachPoint> AttachPoints = new Dictionary<int, BaseSceneAttachPoint>();

		public Dictionary<int, SceneDockingPort> DockingPorts = new Dictionary<int, SceneDockingPort>();

		public Dictionary<int, SceneSpawnPoint> SpawnPoints = new Dictionary<int, SceneSpawnPoint>();

		public Dictionary<int, SceneNameTag> NameTags = new Dictionary<int, SceneNameTag>();

		public Dictionary<int, VesselRepairPoint> RepairPoints = new Dictionary<int, VesselRepairPoint>();

		public List<SceneVesselEmblem> Emblems = new List<SceneVesselEmblem>();

		public List<DamagePointData> DamagePoints = new List<DamagePointData>();

		public float Mass;

		public SpaceObjectVessel DockedToMainVessel;

		public SpaceObjectVessel DockedToVessel;

		public List<SpaceObjectVessel> DockedVessels = new List<SpaceObjectVessel>();

		public Vector3 RelativePosition = Vector3.zero;

		public Quaternion RelativeRotation = Quaternion.identity;

		public float Temperature;

		public float Health;

		public float Armor;

		public AnimationCurve DamagePointEffectFrequency;

		public float? SelfDestructTimer;

		public SubSystemEngine Engine;

		public SubSystemRCS RCS;

		public SubSystemFTL FTLEngine;

		public GeneratorCapacitor Capacitor;

		public SceneCargoBay CargoBay;

		public List<SpaceObjectVessel> AllDockedVessels = new List<SpaceObjectVessel>();

		public VesselBaseSystem VesselBaseSystem;

		public List<DockedVesselData> DummyDockedVessels = new List<DockedVesselData>();

		public static double VesselDecayRateMultiplier = 1.0;

		protected List<Collider> OptimizationColliders;

		protected List<Collider> DontOptimizeColliders;

		public Quaternion RotationCorrection = Quaternion.identity;

		public bool DockingControlsDisabled;

		public bool SecurityPanelsLocked;

		public OrbitParameters LastKnownMapOrbit;

		public VesselData VesselData;

		public bool ActivateGeometry;

		private float posDifferenceCheck = 3.33E-05f;

		private float rotDifferenceCheck = 1E-08f;

		[CompilerGenerated]
		private static Func<DockedVesselData, bool> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<SpaceObjectVessel, bool> _003C_003Ef__am_0024cache1;

		[CompilerGenerated]
		private static Func<DockedVesselData, bool> _003C_003Ef__am_0024cache2;

		[CompilerGenerated]
		private static Func<SpaceObjectVessel, bool> _003C_003Ef__am_0024cache3;

		[CompilerGenerated]
		private static Func<DockedVesselData, bool> _003C_003Ef__am_0024cache4;

		[CompilerGenerated]
		private static Func<SpaceObjectVessel, bool> _003C_003Ef__am_0024cache5;

		[CompilerGenerated]
		private static Func<SpaceObjectVessel, float> _003C_003Ef__am_0024cache6;

		public GameScenes.SceneID SceneID
		{
			get
			{
				return (VesselData == null) ? GameScenes.SceneID.None : VesselData.SceneID;
			}
		}

		public float MaxHealth { get; protected set; }

		public bool EngineOnLine
		{
			get
			{
				return Engine != null && Engine.Status == SystemStatus.OnLine;
			}
		}

		public bool HasPower
		{
			get
			{
				return VesselBaseSystem == null || VesselBaseSystem.Status == SystemStatus.OnLine;
			}
		}

		public bool OptimizationEnabled { get; private set; }

		public bool IsWarpOnline { get; protected set; }

		public SubSystemRadar RadarSystem { get; protected set; }

		public bool IsDocked
		{
			get
			{
				return DockedToMainVessel != null;
			}
		}

		public bool IsMainVessel
		{
			get
			{
				return MainVessel == this;
			}
		}

		public SpaceObjectVessel MainVessel
		{
			get
			{
				return (!(DockedToMainVessel != null)) ? this : DockedToMainVessel;
			}
		}

		public override CelestialBody ParentCelesitalBody
		{
			get
			{
				return base.ParentCelesitalBody;
			}
		}

		public virtual string Name
		{
			get
			{
				return (VesselData == null) ? base.name : (VesselData.VesselRegistration + " " + VesselData.VesselName);
			}
		}

		public virtual string CustomName
		{
			get
			{
				if (VesselData != null)
				{
					if (VesselData.VesselName.IsNullOrEmpty())
					{
						return VesselData.VesselRegistration;
					}
					return VesselData.VesselName;
				}
				return Localization.UnknownObject;
			}
		}

		public string CommandVesselName
		{
			get
			{
				VesselData commandVesselData = CommandVesselData;
				if (commandVesselData != null)
				{
					if (commandVesselData.VesselName.IsNullOrEmpty())
					{
						return commandVesselData.VesselRegistration;
					}
					return commandVesselData.VesselName;
				}
				return Localization.UnknownObject;
			}
		}

		public VesselData CommandVesselData
		{
			get
			{
				if (MainVessel.IsDummyObject)
				{
					List<DockedVesselData> dummyDockedVessels = MainVessel.DummyDockedVessels;
					if (_003C_003Ef__am_0024cache0 == null)
					{
						_003C_003Ef__am_0024cache0 = _003Cget_CommandVesselData_003Em__0;
					}
					DockedVesselData dockedVesselData = dummyDockedVessels.FirstOrDefault(_003C_003Ef__am_0024cache0);
					if (dockedVesselData != null)
					{
						return dockedVesselData.Data;
					}
				}
				else
				{
					List<SpaceObjectVessel> allDockedVessels = MainVessel.AllDockedVessels;
					if (_003C_003Ef__am_0024cache1 == null)
					{
						_003C_003Ef__am_0024cache1 = _003Cget_CommandVesselData_003Em__1;
					}
					SpaceObjectVessel spaceObjectVessel = allDockedVessels.FirstOrDefault(_003C_003Ef__am_0024cache1);
					if (spaceObjectVessel != null)
					{
						return spaceObjectVessel.VesselData;
					}
				}
				return MainVessel.VesselData;
			}
		}

		public List<SpaceObjectVessel> AllVessels
		{
			get
			{
				List<SpaceObjectVessel> list = new List<SpaceObjectVessel>();
				list.Add(MainVessel);
				list.AddRange(MainVessel.AllDockedVessels);
				return list;
			}
		}

		public override Vector3D Position
		{
			get
			{
				if (IsMainVessel)
				{
					return base.Position;
				}
				return MainVessel.Position + Quaternion.LookRotation(MainVessel.Forward, MainVessel.Up).ToQuaternionD() * (base.transform.position - MainVessel.transform.position).ToVector3D();
			}
		}

		public override Vector3D Velocity
		{
			get
			{
				if (IsMainVessel)
				{
					return base.Velocity;
				}
				return MainVessel.Velocity;
			}
		}

		public override bool IsDistressSignalActive
		{
			get
			{
				return (VesselData != null && VesselData.IsDistressSignalActive) ? true : false;
			}
		}

		public override bool IsAlwaysVisible
		{
			get
			{
				return (VesselData != null && VesselData.IsAlwaysVisible) ? true : false;
			}
		}

		public bool IsDebrisFragment
		{
			get
			{
				if (VesselData == null)
				{
					return true;
				}
				return VesselData.IsDebrisFragment;
			}
		}

		public override Vector3 Forward
		{
			get
			{
				return base.Forward;
			}
			set
			{
				base.Forward = value;
				if (!IsMainVessel)
				{
					return;
				}
				foreach (SpaceObjectVessel allDockedVessel in AllDockedVessels)
				{
					if ((((object)allDockedVessel != null) ? allDockedVessel.gameObject : null) != null)
					{
						allDockedVessel.Forward = Quaternion.LookRotation(Forward, Up) * Quaternion.LookRotation(allDockedVessel.transform.forward, allDockedVessel.transform.up) * Vector3.forward;
					}
				}
			}
		}

		public override Vector3 Up
		{
			get
			{
				return base.Up;
			}
			set
			{
				base.Up = value;
				if (!IsMainVessel)
				{
					return;
				}
				foreach (SpaceObjectVessel allDockedVessel in AllDockedVessels)
				{
					if ((((object)allDockedVessel != null) ? allDockedVessel.gameObject : null) != null)
					{
						allDockedVessel.Up = Quaternion.LookRotation(Forward, Up) * Quaternion.LookRotation(allDockedVessel.transform.forward, allDockedVessel.transform.up) * Vector3.up;
					}
				}
			}
		}

		public bool IsStation
		{
			get
			{
				if (base.IsDummyObject)
				{
					int result;
					if (DummyDockedVessels.Count > 0)
					{
						List<DockedVesselData> dummyDockedVessels = DummyDockedVessels;
						if (_003C_003Ef__am_0024cache2 == null)
						{
							_003C_003Ef__am_0024cache2 = _003Cget_IsStation_003Em__2;
						}
						result = ((dummyDockedVessels.FirstOrDefault(_003C_003Ef__am_0024cache2) != null) ? 1 : 0);
					}
					else
					{
						result = 0;
					}
					return (byte)result != 0;
				}
				int result2;
				if (AllDockedVessels.Count > 0)
				{
					List<SpaceObjectVessel> allDockedVessels = AllDockedVessels;
					if (_003C_003Ef__am_0024cache3 == null)
					{
						_003C_003Ef__am_0024cache3 = _003Cget_IsStation_003Em__3;
					}
					result2 = ((allDockedVessels.FirstOrDefault(_003C_003Ef__am_0024cache3) != null) ? 1 : 0);
				}
				else
				{
					result2 = 0;
				}
				return (byte)result2 != 0;
			}
		}

		public bool IsOutpost
		{
			get
			{
				if (base.IsDummyObject)
				{
					int result;
					if (DummyDockedVessels.Count > 0)
					{
						List<DockedVesselData> dummyDockedVessels = DummyDockedVessels;
						if (_003C_003Ef__am_0024cache4 == null)
						{
							_003C_003Ef__am_0024cache4 = _003Cget_IsOutpost_003Em__4;
						}
						result = ((dummyDockedVessels.FirstOrDefault(_003C_003Ef__am_0024cache4) == null) ? 1 : 0);
					}
					else
					{
						result = 0;
					}
					return (byte)result != 0;
				}
				int result2;
				if (AllDockedVessels.Count > 0)
				{
					List<SpaceObjectVessel> allDockedVessels = AllDockedVessels;
					if (_003C_003Ef__am_0024cache5 == null)
					{
						_003C_003Ef__am_0024cache5 = _003Cget_IsOutpost_003Em__5;
					}
					result2 = ((allDockedVessels.FirstOrDefault(_003C_003Ef__am_0024cache5) == null) ? 1 : 0);
				}
				else
				{
					result2 = 0;
				}
				return (byte)result2 != 0;
			}
		}

		public bool IsOutpostOrStation
		{
			get
			{
				return (base.IsDummyObject && DummyDockedVessels.Count > 0) || (!base.IsDummyObject && AllDockedVessels.Count > 0);
			}
		}

		public float ExposureDamage
		{
			get
			{
				return Client.Instance.GetVesselExposureDamage(MainVessel.Orbit.Position.Magnitude);
			}
		}

		public override double RadarSignature
		{
			get
			{
				if (!IsMainVessel)
				{
					return MainVessel.RadarSignature;
				}
				if (VesselData == null)
				{
					return 0.0;
				}
				double num = 1.0;
				for (OrbitParameters parent = Orbit.Parent; parent != null; parent = parent.Parent)
				{
					CelestialBody celestialBody = parent.CelestialBody;
					num *= (double)celestialBody.GetRadarSignatureModifier(this);
				}
				foreach (DebrisField debrisField in Client.Instance.DebrisFields)
				{
					if (debrisField.CheckObject(this))
					{
						num *= (double)debrisField.RadarSignatureMultiplier;
					}
				}
				return VesselData.RadarSignature * num;
			}
		}

		protected override bool ShouldSetLocalTransform
		{
			get
			{
				return !IsDocked && (MyPlayer.Instance == null || !(MyPlayer.Instance.Parent is SpaceObjectVessel) || (MyPlayer.Instance.Parent as SpaceObjectVessel).MainVessel != this);
			}
		}

		protected override bool ShouldUpdateTransform
		{
			get
			{
				return !IsDocked;
			}
		}

		public bool SoundObjectsEnabled { get; private set; }

		private object GetVesselObject(VesselObjectType objectType, int inSceneID)
		{
			try
			{
				switch (objectType)
				{
				case VesselObjectType.SubSystem:
					return SubSystems[inSceneID];
				case VesselObjectType.Generator:
					return Generators[inSceneID];
				case VesselObjectType.RoomTrigger:
					return RoomTriggers[inSceneID];
				case VesselObjectType.ResourceContainer:
					return ResourceContainers[inSceneID];
				case VesselObjectType.Door:
					return Doors[inSceneID];
				case VesselObjectType.SceneTriggerExecuter:
					return SceneTriggerExecuters[inSceneID];
				case VesselObjectType.AttachPoint:
					return AttachPoints[inSceneID];
				case VesselObjectType.DockingPort:
					return DockingPorts[inSceneID];
				case VesselObjectType.SpawnPoint:
					return SpawnPoints[inSceneID];
				case VesselObjectType.NameTag:
					return NameTags[inSceneID];
				case VesselObjectType.RepairPoint:
					return RepairPoints[inSceneID];
				default:
					Dbg.Error("Cannot get structure object. Unsupported object type.", objectType, inSceneID);
					return null;
				}
			}
			catch
			{
				Dbg.Error("Cannot get structure object. Object ID not found.", objectType, inSceneID);
				return null;
			}
		}

		public T GetVesselObject<T>(int inSceneID)
		{
			try
			{
				if (typeof(SubSystem).IsAssignableFrom(typeof(T)))
				{
					return (T)GetVesselObject(VesselObjectType.SubSystem, inSceneID);
				}
				if (typeof(Generator).IsAssignableFrom(typeof(T)))
				{
					return (T)GetVesselObject(VesselObjectType.Generator, inSceneID);
				}
				if (typeof(SceneTriggerRoom).IsAssignableFrom(typeof(T)))
				{
					return (T)GetVesselObject(VesselObjectType.RoomTrigger, inSceneID);
				}
				if (typeof(ResourceContainer).IsAssignableFrom(typeof(T)))
				{
					return (T)GetVesselObject(VesselObjectType.ResourceContainer, inSceneID);
				}
				if (typeof(SceneDoor).IsAssignableFrom(typeof(T)))
				{
					return (T)GetVesselObject(VesselObjectType.Door, inSceneID);
				}
				if (typeof(SceneTriggerExecuter).IsAssignableFrom(typeof(T)))
				{
					return (T)GetVesselObject(VesselObjectType.SceneTriggerExecuter, inSceneID);
				}
				if (typeof(BaseSceneAttachPoint).IsAssignableFrom(typeof(T)))
				{
					return (T)GetVesselObject(VesselObjectType.AttachPoint, inSceneID);
				}
				if (typeof(SceneDockingPort).IsAssignableFrom(typeof(T)))
				{
					return (T)GetVesselObject(VesselObjectType.DockingPort, inSceneID);
				}
				if (typeof(SceneSpawnPoint).IsAssignableFrom(typeof(T)))
				{
					return (T)GetVesselObject(VesselObjectType.SpawnPoint, inSceneID);
				}
				if (typeof(SceneNameTag).IsAssignableFrom(typeof(T)))
				{
					return (T)GetVesselObject(VesselObjectType.NameTag, inSceneID);
				}
				if (typeof(VesselRepairPoint).IsAssignableFrom(typeof(T)))
				{
					return (T)GetVesselObject(VesselObjectType.RepairPoint, inSceneID);
				}
				return default(T);
			}
			catch (Exception ex)
			{
				Dbg.Error(ex.Message, ex.StackTrace);
				return default(T);
			}
		}

		public T GetStructureObject<T>(int inSceneID)
		{
			return GetVesselObject<T>(inSceneID);
		}

		public List<SceneTriggerRoom> GetRoomTriggers()
		{
			return new List<SceneTriggerRoom>(RoomTriggers.Values);
		}

		public abstract void ChangeStats(Vector3? thrust = null, Vector3? rotation = null, Vector3? autoStabilize = null, float? engineThrustPercentage = null, SubSystemDetails subSystem = null, GeneratorDetails generator = null, RoomDetails roomTrigger = null, DoorDetails door = null, SceneTriggerExecuterDetails sceneTriggerExecuter = null, SceneDockingPortDetails dockingPort = null, AttachPointDetails attachPoint = null, long? stabilizationTarget = null, SpawnPointStats spawnPoint = null, float? selfDestructTime = null, string emblemId = null);

		public void RecreateDockedVesselsTree()
		{
			DockedToMainVessel = null;
			DockedToVessel = null;
			RecreateDockedVesselsTree(this, null);
		}

		private void RecreateDockedVesselsTree(SpaceObjectVessel mainVessel, SpaceObjectVessel parentVessel)
		{
			AllDockedVessels.Clear();
			DockedVessels.Clear();
			foreach (SceneDockingPort value in DockingPorts.Values)
			{
				if (value.DockedToPort != null && value.DockedToPort.ParentShip != parentVessel)
				{
					SpaceObjectVessel parentShip = value.DockedToPort.ParentShip;
					parentShip.DockedToMainVessel = mainVessel;
					parentShip.DockedToVessel = this;
					mainVessel.AllDockedVessels.Add(parentShip);
					DockedVessels.Add(parentShip);
					parentShip.RecreateDockedVesselsTree(mainVessel, this);
				}
			}
		}

		public void ResetDockedToVessel()
		{
			foreach (SpaceObjectVessel dockedVessel in DockedVessels)
			{
				dockedVessel.DockedToVessel = this;
				dockedVessel.ResetDockedToVessel();
			}
		}

		public void SetMainVesselForChldren(SpaceObjectVessel newMainVessel)
		{
			foreach (SpaceObjectVessel dockedVessel in DockedVessels)
			{
				if (!newMainVessel.AllDockedVessels.Contains(dockedVessel))
				{
					dockedVessel.DockedToMainVessel = newMainVessel;
					newMainVessel.AllDockedVessels.Add(dockedVessel);
					dockedVessel.SetMainVesselForChldren(newMainVessel);
				}
			}
		}

		public void DbgLogDockedVesseslTree()
		{
			SpaceObjectVessel spaceObjectVessel = null;
			if (IsDocked)
			{
				spaceObjectVessel = DockedToMainVessel;
			}
			else if (AllDockedVessels.Count > 0)
			{
				spaceObjectVessel = this;
			}
			if (spaceObjectVessel != null)
			{
				spaceObjectVessel.DbgLogDockedVesslesTreeWorker(1);
			}
		}

		private void DbgLogDockedVesslesTreeWorker(int padding)
		{
			foreach (SpaceObjectVessel dockedVessel in DockedVessels)
			{
				dockedVessel.DbgLogDockedVesslesTreeWorker(padding + 1);
			}
		}

		public void AllowOtherCharacterMovement()
		{
			SpaceObjectVessel spaceObjectVessel = this;
			if (spaceObjectVessel.DockedToMainVessel != null)
			{
				spaceObjectVessel = spaceObjectVessel.DockedToMainVessel;
			}
			OtherPlayer[] componentsInChildren = spaceObjectVessel.TransferableObjectsRoot.GetComponentsInChildren<OtherPlayer>();
			foreach (OtherPlayer otherPlayer in componentsInChildren)
			{
				otherPlayer.tpsController.UpdateMovementPosition = true;
			}
			foreach (SpaceObjectVessel allDockedVessel in spaceObjectVessel.AllDockedVessels)
			{
				OtherPlayer[] componentsInChildren2 = allDockedVessel.TransferableObjectsRoot.GetComponentsInChildren<OtherPlayer>();
				foreach (OtherPlayer otherPlayer2 in componentsInChildren2)
				{
					otherPlayer2.tpsController.UpdateMovementPosition = true;
				}
			}
		}

		protected void UpdateDynamicObjects(List<DynamicObjectDetails> dynamicObjectDetails)
		{
			if (dynamicObjectDetails == null)
			{
				return;
			}
			List<GameObject> list = new List<GameObject>();
			using (IEnumerator<DynamicObject> enumerator = TransferableObjectsRoot.GetComponentsInChildren<DynamicObject>().Where(_003CUpdateDynamicObjects_003Em__6).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					_003CUpdateDynamicObjects_003Ec__AnonStorey0 _003CUpdateDynamicObjects_003Ec__AnonStorey = new _003CUpdateDynamicObjects_003Ec__AnonStorey0();
					_003CUpdateDynamicObjects_003Ec__AnonStorey.obj = enumerator.Current;
					if (_003CUpdateDynamicObjects_003Ec__AnonStorey.obj != null)
					{
						DynamicObjectDetails dynamicObjectDetails2 = dynamicObjectDetails.Find(_003CUpdateDynamicObjects_003Ec__AnonStorey._003C_003Em__0);
						if (dynamicObjectDetails2 != null)
						{
							dynamicObjectDetails.Remove(dynamicObjectDetails2);
						}
						else
						{
							list.Add(_003CUpdateDynamicObjects_003Ec__AnonStorey.obj.gameObject);
						}
					}
				}
			}
			if (list.Count > 0)
			{
				foreach (GameObject item in list)
				{
					UnityEngine.Object.Destroy(item);
				}
			}
			if (dynamicObjectDetails.Count <= 0)
			{
				return;
			}
			foreach (DynamicObjectDetails dynamicObjectDetail in dynamicObjectDetails)
			{
				if (Client.Instance.GetDynamicObject(dynamicObjectDetail.GUID) == null)
				{
					DynamicObject.SpawnDynamicObject(dynamicObjectDetail, this);
				}
			}
		}

		protected virtual void UpdateCharacters(List<CharacterDetails> characterDetails)
		{
			if (characterDetails == null)
			{
				return;
			}
			List<GameObject> list = new List<GameObject>();
			OtherPlayer[] componentsInChildren = TransferableObjectsRoot.GetComponentsInChildren<OtherPlayer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_003CUpdateCharacters_003Ec__AnonStorey1 _003CUpdateCharacters_003Ec__AnonStorey = new _003CUpdateCharacters_003Ec__AnonStorey1();
				_003CUpdateCharacters_003Ec__AnonStorey.opl = componentsInChildren[i];
				if (_003CUpdateCharacters_003Ec__AnonStorey.opl != null)
				{
					CharacterDetails characterDetails2 = characterDetails.Find(_003CUpdateCharacters_003Ec__AnonStorey._003C_003Em__0);
					if (characterDetails2 != null)
					{
						characterDetails.Remove(characterDetails2);
					}
					else
					{
						list.Add(_003CUpdateCharacters_003Ec__AnonStorey.opl.gameObject);
					}
				}
			}
			if (list.Count > 0)
			{
				foreach (GameObject item in list)
				{
					UnityEngine.Object.Destroy(item);
				}
			}
			if (characterDetails.Count <= 0)
			{
				return;
			}
			foreach (CharacterDetails characterDetail in characterDetails)
			{
				if (Client.Instance.GetPlayer(characterDetail.GUID) == null)
				{
					OtherPlayer.SpawnPlayer(characterDetail, this);
				}
			}
		}

		protected void UpdateCorpses(List<CorpseDetails> corpseDetails)
		{
			if (corpseDetails == null)
			{
				return;
			}
			List<GameObject> list = new List<GameObject>();
			Corpse[] componentsInChildren = TransferableObjectsRoot.GetComponentsInChildren<Corpse>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_003CUpdateCorpses_003Ec__AnonStorey2 _003CUpdateCorpses_003Ec__AnonStorey = new _003CUpdateCorpses_003Ec__AnonStorey2();
				_003CUpdateCorpses_003Ec__AnonStorey.obj = componentsInChildren[i];
				if (_003CUpdateCorpses_003Ec__AnonStorey.obj != null)
				{
					CorpseDetails corpseDetails2 = corpseDetails.Find(_003CUpdateCorpses_003Ec__AnonStorey._003C_003Em__0);
					if (corpseDetails2 != null)
					{
						corpseDetails.Remove(corpseDetails2);
					}
					else
					{
						list.Add(_003CUpdateCorpses_003Ec__AnonStorey.obj.gameObject);
					}
				}
			}
			if (list.Count > 0)
			{
				foreach (GameObject item in list)
				{
					UnityEngine.Object.Destroy(item);
				}
			}
			if (corpseDetails.Count <= 0)
			{
				return;
			}
			foreach (CorpseDetails corpseDetail in corpseDetails)
			{
				if (Client.Instance.GetCorpse(corpseDetail.GUID) == null)
				{
					Corpse.SpawnCorpse(corpseDetail, null);
				}
			}
		}

		public virtual void OnSceneLoaded()
		{
		}

		public override void Unsubscribe()
		{
			base.Unsubscribe();
			if (!IsMainVessel)
			{
				return;
			}
			foreach (SpaceObjectVessel allDockedVessel in AllDockedVessels)
			{
				allDockedVessel.Unsubscribe();
			}
		}

		public override void Subscribe()
		{
			base.Subscribe();
			if (!IsMainVessel)
			{
				return;
			}
			foreach (SpaceObjectVessel allDockedVessel in AllDockedVessels)
			{
				allDockedVessel.Subscribe();
			}
		}

		public virtual bool IsPlayerAuthorized(Player pl)
		{
			return true;
		}

		public virtual bool IsPlayerAuthorizedOrNoSecurity(Player pl)
		{
			return true;
		}

		public virtual bool IsPlayerAuthorizedOrFreeSecurity(Player pl)
		{
			return true;
		}

		private void RecalculateDifferenceChecks()
		{
			if (!(MyPlayer.Instance == null) && !(MyPlayer.Instance.transform == null))
			{
				float num = (base.transform.position - MyPlayer.Instance.transform.position).magnitude - (float)base.Radius;
				if (num < 100f)
				{
					posDifferenceCheck = 1E-08f;
				}
				else if (num < 1000f)
				{
					posDifferenceCheck = 0.001f;
				}
				else if (num < 1500f)
				{
					posDifferenceCheck = num / 1000f;
				}
				else
				{
					posDifferenceCheck = num / 500f;
				}
			}
		}

		protected override bool PositionAndRotationPhysicsCheck(ref Vector3? nextPos, ref Quaternion? nextRot)
		{
			return (nextPos.HasValue && !nextPos.Value.IsEpsilonEqual(base.transform.localPosition, posDifferenceCheck)) || (nextRot.HasValue && !nextRot.Value.IsEpsilonEqual(base.transform.localRotation, rotDifferenceCheck));
		}

		public override void UpdateArtificialBodyPosition(bool updateChildren)
		{
			if (ArtificalRigidbody != null && GeometryPlaceholder != null)
			{
				GeometryRoot.transform.position = GeometryPlaceholder.transform.position;
				GeometryRoot.transform.rotation = GeometryPlaceholder.transform.rotation;
				ArtificalRigidbody.position = GeometryPlaceholder.transform.position;
				ArtificalRigidbody.rotation = GeometryPlaceholder.transform.rotation;
			}
			if (!updateChildren || AllDockedVessels == null || AllDockedVessels.Count <= 0)
			{
				return;
			}
			foreach (SpaceObjectVessel allDockedVessel in AllDockedVessels)
			{
				if (allDockedVessel.ArtificalRigidbody != null && allDockedVessel.GeometryPlaceholder != null)
				{
					allDockedVessel.GeometryRoot.transform.position = allDockedVessel.GeometryPlaceholder.transform.position;
					allDockedVessel.GeometryRoot.transform.rotation = allDockedVessel.GeometryPlaceholder.transform.rotation;
					allDockedVessel.ArtificalRigidbody.position = allDockedVessel.GeometryPlaceholder.transform.position;
					allDockedVessel.ArtificalRigidbody.rotation = allDockedVessel.GeometryPlaceholder.transform.rotation;
				}
			}
		}

		protected override void UpdatePositionAndRotation(bool setLocalPositionAndRotation)
		{
			if (!IsDocked && setLocalPositionAndRotation)
			{
				RecalculateDifferenceChecks();
			}
			base.UpdatePositionAndRotation(setLocalPositionAndRotation);
			ZeroOcclusion.CheckOcclusionFor(this, true);
		}

		public void ActivateSelfDestruct(float time)
		{
			if (time >= 0f)
			{
				SelfDestructTimer = time;
				float? selfDestructTime = time;
				ChangeStats(null, null, null, null, null, null, null, null, null, null, null, null, null, selfDestructTime);
			}
		}

		public void CancelSelfDestruct()
		{
			SelfDestructTimer = null;
			ChangeStats(null, null, null, null, null, null, null, null, null, null, null, null, null, -1f);
		}

		public override void LoadGeometry()
		{
			base.IsDummyObject = false;
			if (VesselData == null || IsMainVessel)
			{
				RequestSpawn();
			}
		}

		public override void DestroyGeometry()
		{
			base.DestroyGeometry();
		}

		public virtual void ToggleOptimization(bool optimizationEnabled)
		{
			OptimizationEnabled = optimizationEnabled;
			if (OptimizationColliders != null && OptimizationColliders.Count > 0)
			{
				foreach (Collider optimizationCollider in OptimizationColliders)
				{
					if (optimizationCollider != null)
					{
						optimizationCollider.enabled = !optimizationEnabled;
					}
				}
			}
			bool flag = true;
			if (DontOptimizeColliders != null && DontOptimizeColliders.Count > 0)
			{
				foreach (Collider dontOptimizeCollider in DontOptimizeColliders)
				{
					if (dontOptimizeCollider != null)
					{
						dontOptimizeCollider.enabled = flag;
					}
				}
			}
			DynamicObject[] componentsInChildren = TransferableObjectsRoot.GetComponentsInChildren<DynamicObject>();
			foreach (DynamicObject dynamicObject in componentsInChildren)
			{
				if (!dynamicObject.IsAttached)
				{
					dynamicObject.ToggleKinematic(optimizationEnabled);
				}
			}
		}

		public string GetDescription()
		{
			SpaceObjectVessel mainVessel = MainVessel;
			int num = ((!base.IsDummyObject) ? mainVessel.AllDockedVessels.Count : DummyDockedVessels.Count);
			if (num == 0)
			{
				if (GameScenes.Ranges.IsShip(mainVessel.SceneID))
				{
					return Localization.Ship + " - " + mainVessel.SceneID.ToLocalizedString();
				}
				return Localization.Module + " - " + mainVessel.SceneID.ToLocalizedString();
			}
			if (IsStation)
			{
				if (num < 3)
				{
					return Localization.SmallStation;
				}
				if (num < 8)
				{
					return Localization.MediumStation;
				}
				return Localization.LargeStation;
			}
			if (num < 3)
			{
				return Localization.SmallOutpost;
			}
			if (num < 6)
			{
				return Localization.MediumOutpost;
			}
			return Localization.LargeOutpost;
		}

		protected void SmoothRotation(float deltaTime)
		{
			SpaceObject spaceObject = ((!(MyPlayer.Instance.Parent is SpaceObjectVessel)) ? MyPlayer.Instance.Parent : (MyPlayer.Instance.Parent as SpaceObjectVessel).MainVessel);
			if (TargetRotation.HasValue)
			{
				Quaternion quaternion = Quaternion.LookRotation(Forward, Up);
				Quaternion quaternion2;
				if (!ShouldSetLocalTransform)
				{
					quaternion2 = ((!Client.VESSEL_ROTATION_LERP_UNCLAMPED) ? Quaternion.Slerp(Quaternion.Slerp(TargetRotation.Value, quaternion, deltaTime), Quaternion.Slerp(quaternion, TargetRotation.Value, deltaTime), Client.VESSEL_ROTATION_LERP_VALUE) : Quaternion.Slerp(Quaternion.SlerpUnclamped(TargetRotation.Value, quaternion, deltaTime), Quaternion.SlerpUnclamped(quaternion, TargetRotation.Value, deltaTime), Client.VESSEL_ROTATION_LERP_VALUE));
				}
				else
				{
					float num = Quaternion.Angle(quaternion, TargetRotation.Value);
					quaternion2 = quaternion * Quaternion.Euler(RotationVec.ToVector3() * deltaTime);
					float num2 = Quaternion.Angle(quaternion2, TargetRotation.Value);
					quaternion2 = quaternion * Quaternion.Euler(RotationVec.ToVector3() * deltaTime * (1f + (float)((!(num < num2)) ? 1 : (-1)) * num / 100f));
				}
				Forward = quaternion2 * Vector3.forward;
				Up = quaternion2 * Vector3.up;
				if (ShouldSetLocalTransform)
				{
					base.transform.rotation = Quaternion.LookRotation(spaceObject.Forward, spaceObject.Up).Inverse() * Quaternion.LookRotation(Forward, Up);
					Debug.DrawRay(base.transform.position, base.transform.rotation * Vector3.forward * 50f, Color.blue);
					Debug.DrawRay(base.transform.position, Quaternion.LookRotation(spaceObject.Forward, spaceObject.Up).Inverse() * TargetRotation.Value * Vector3.forward * 50f, Color.red);
				}
				else if (Client.IsGameBuild)
				{
					MyPlayer.Instance.UpdateCameraPositions();
				}
			}
		}

		public double GetCompoundMass()
		{
			List<SpaceObjectVessel> allVessels = MainVessel.AllVessels;
			if (_003C_003Ef__am_0024cache6 == null)
			{
				_003C_003Ef__am_0024cache6 = _003CGetCompoundMass_003Em__7;
			}
			return allVessels.Sum(_003C_003Ef__am_0024cache6);
		}

		public void SetLastKnownMapOrbit()
		{
			LastKnownMapOrbit = new OrbitParameters();
			LastKnownMapOrbit.CopyDataFrom(Orbit, Client.Instance.SolarSystem.CurrentTime, true);
		}

		public void ResetLastKnownMapOrbit()
		{
			LastKnownMapOrbit = null;
		}

		[CompilerGenerated]
		private static bool _003Cget_CommandVesselData_003Em__0(DockedVesselData m)
		{
			return m.Data.SceneID == GameScenes.SceneID.AltCorp_Command_Module;
		}

		[CompilerGenerated]
		private static bool _003Cget_CommandVesselData_003Em__1(SpaceObjectVessel m)
		{
			return m.SceneID == GameScenes.SceneID.AltCorp_Command_Module;
		}

		[CompilerGenerated]
		private static bool _003Cget_IsStation_003Em__2(DockedVesselData m)
		{
			return m.Data.SceneID == GameScenes.SceneID.AltCorp_Command_Module;
		}

		[CompilerGenerated]
		private static bool _003Cget_IsStation_003Em__3(SpaceObjectVessel m)
		{
			return m.SceneID == GameScenes.SceneID.AltCorp_Command_Module;
		}

		[CompilerGenerated]
		private static bool _003Cget_IsOutpost_003Em__4(DockedVesselData m)
		{
			return m.Data.SceneID == GameScenes.SceneID.AltCorp_Command_Module;
		}

		[CompilerGenerated]
		private static bool _003Cget_IsOutpost_003Em__5(SpaceObjectVessel m)
		{
			return m.SceneID == GameScenes.SceneID.AltCorp_Command_Module;
		}

		[CompilerGenerated]
		private bool _003CUpdateDynamicObjects_003Em__6(DynamicObject m)
		{
			return m.Parent == this;
		}

		[CompilerGenerated]
		private static float _003CGetCompoundMass_003Em__7(SpaceObjectVessel m)
		{
			return m.Mass;
		}
	}
}
