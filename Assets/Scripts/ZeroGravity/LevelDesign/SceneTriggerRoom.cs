using System;
using System.Collections.Generic;
using System.Linq;
using OpenHellion.Networking;
using UnityEngine;
using UnityEngine.Serialization;
using ZeroGravity.CharacterMovement;
using ZeroGravity.Data;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerRoom : BaseSceneTrigger, ISceneObject
	{
		[Serializable]
		public class LightItemObject
		{
			public Light Light;
		}

		public string RoomName;

		private List<MonoBehaviour> BehaviourScripts = new List<MonoBehaviour>();

		[SerializeField]
		private int _inSceneID;

		public Vector3 GravityForce;

		[SerializeField]
		[FormerlySerializedAs("UseGravity")]
		private bool _UseGravity = true;

		public bool GravityAutoToggle;

		public bool AirFiltering = true;

		[SerializeField]
		private bool _disablePlayerInsideOccluder;

		[HideInInspector]
		public SpaceObjectVessel ParentVessel;

		[HideInInspector]
		public List<SceneDoor> Doors;

		[Tooltip("Actual volume in cubic meters. ")]
		public float Volume = 1f;

		[Tooltip("Parent SceneTriggerRoom object")]
		public SceneTriggerRoom ParentRoom;

		[SerializeField]
		private float _AirPressure = 1f;

		[SerializeField]
		private float _AirQuality = 1f;

		[Space(10f)]
		public float PressurizeSpeed = 3f;

		public float DepressurizeSpeed = 3f;

		public float VentSpeed = 5f;

		[NonSerialized]
		public float AirPressureChangeRate;

		[NonSerialized]
		public float AirQualityChangeRate;

		[NonSerialized]
		public RoomPressurizationStatus PressurizationStatus;

		private bool isApplicationQuitting;

		public PressureAmbientGameObjects PressureGOsp;

		[Space(10f)]
		[Title("EFFECTS")]
		public List<ParticleSystem> PressurizationParticles;

		public List<ParticleSystem> DepressurizationParticles;

		[Title("SOUND")]
		public List<SoundEffect> PressurizationSounds;

		[HideInInspector]
		public string EnvironmentReverb = string.Empty;

		[HideInInspector]
		public uint EnvironmentReverbId;

		[HideInInspector]
		public string Ambience = string.Empty;

		[NonSerialized]
		public bool Fire;

		[NonSerialized]
		public bool Breach;

		[NonSerialized]
		public bool GravityMalfunction;

		[NonSerialized]
		public short CompoundRoomID;

		[NonSerialized]
		public List<SceneTriggerRoom> ConnectedRooms;

		private float oldPressure;

		private float lastAirPressureTime;

		public override bool ExclusivePlayerLocking => false;

		public override bool CameraMovementAllowed => false;

		public override SceneTriggerType TriggerType => SceneTriggerType.Room;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.DontCheck;

		public override List<ItemType> PlayerHandsItemType => null;

		public override bool IsNearTrigger => true;

		public int InSceneID
		{
			get
			{
				return _inSceneID;
			}
			set
			{
				_inSceneID = value;
			}
		}

		public override bool IsInteractable => false;

		public float Breathability => MathHelper.Clamp(AirQuality * AirPressure / 0.2f, 0f, 1f);

		public bool IsAirOk => Breathability + float.Epsilon >= 1f;

		private bool CritAir => IsAirOk && AirQuality * AirPressure <= 0.3f;

		public bool FireCanBurn => AirQuality * AirPressure >= 0.25f;

		public bool DisablePlayerInsideOccluder => _disablePlayerInsideOccluder;

		public bool UseGravity
		{
			get
			{
				return !GravityMalfunction && _UseGravity;
			}
			set
			{
				_UseGravity = value;
			}
		}

		public float AirQuality
		{
			get
			{
				return _AirQuality;
			}
			set
			{
				_AirQuality = value;
			}
		}

		public float AirPressure
		{
			get
			{
				return _AirPressure;
			}
			set
			{
				_AirPressure = value;
				lastAirPressureTime = Time.time;
				if (MyPlayer.Instance != null && MyPlayer.Instance.CurrentRoomTrigger == this)
				{
					MyPlayer.Instance.Pressure = value;
				}
				if (value <= 0.5f && !oldPressure.IsEpsilonEqual(value, 0.03f))
				{
					oldPressure = value;
				}
			}
		}

		public float InterpolatedAirQuality => _AirQuality;

		public float InterpolatedAirPressure => Mathf.Clamp01(_AirPressure + (AirPressureChangeRate * (Time.time - lastAirPressureTime) - AirPressureChangeRate));

		public void AddBehaviourScript(MonoBehaviour script)
		{
			if (script != null && !BehaviourScripts.Contains(script))
			{
				BehaviourScripts.Add(script);
			}
		}

		public void ExecuteBehaviourScripts()
		{
			foreach (MonoBehaviour behaviourScript in BehaviourScripts)
			{
				if (!(behaviourScript == null))
				{
					if (behaviourScript is SceneDoor)
					{
						(behaviourScript as SceneDoor).UpdateDoorUI();
					}
					else if (behaviourScript is DoorEnviormentPanel)
					{
						(behaviourScript as DoorEnviormentPanel).DoorEnviormentUpdateUI();
					}
					else if (behaviourScript is AirLockControls)
					{
						(behaviourScript as AirLockControls).UpdateUI();
					}
					else if (behaviourScript is VesselRepairPoint)
					{
						(behaviourScript as VesselRepairPoint).UpdateEffects();
					}
				}
			}
			if (PressureGOsp != null)
			{
				PressureGOsp.CheckPressureForAmbient(AirPressure, IsAirOk, CritAir);
			}
			UpdatePressurizationEffects();
		}

		public void UpdatePressurizationEffects()
		{
			if (PressurizationStatus == RoomPressurizationStatus.Pressurize)
			{
				if (DepressurizationParticles != null && DepressurizationParticles.Count != 0)
				{
					foreach (ParticleSystem depressurizationParticle in DepressurizationParticles)
					{
						if (depressurizationParticle.isPlaying)
						{
							depressurizationParticle.Stop();
						}
					}
				}
				if (PressurizationParticles != null && PressurizationParticles.Count != 0)
				{
					foreach (ParticleSystem pressurizationParticle in PressurizationParticles)
					{
						if (!pressurizationParticle.isPlaying)
						{
							pressurizationParticle.Play();
						}
					}
				}
				{
					foreach (SoundEffect pressurizationSound in PressurizationSounds)
					{
						if (!pressurizationSound.SoundEvents[0].IsPlaying)
						{
							pressurizationSound.Play(0);
						}
					}
					return;
				}
			}
			if (PressurizationStatus == RoomPressurizationStatus.Depressurize)
			{
				if (PressurizationParticles != null && PressurizationParticles.Count != 0)
				{
					foreach (ParticleSystem pressurizationParticle2 in PressurizationParticles)
					{
						if (pressurizationParticle2.isPlaying)
						{
							pressurizationParticle2.Stop();
						}
					}
				}
				if (DepressurizationParticles != null && DepressurizationParticles.Count != 0)
				{
					foreach (ParticleSystem depressurizationParticle2 in DepressurizationParticles)
					{
						if (!depressurizationParticle2.isPlaying)
						{
							depressurizationParticle2.Play();
						}
					}
				}
				{
					foreach (SoundEffect pressurizationSound2 in PressurizationSounds)
					{
						if (!pressurizationSound2.SoundEvents[0].IsPlaying)
						{
							pressurizationSound2.Play(0);
						}
					}
					return;
				}
			}
			if (PressurizationStatus == RoomPressurizationStatus.Vent)
			{
				foreach (SoundEffect pressurizationSound3 in PressurizationSounds)
				{
					if (!pressurizationSound3.SoundEvents[2].IsPlaying)
					{
						pressurizationSound3.Play(2);
					}
				}
				return;
			}
			if (PressurizationParticles != null && PressurizationParticles.Count != 0)
			{
				foreach (ParticleSystem pressurizationParticle3 in PressurizationParticles)
				{
					if (pressurizationParticle3.isPlaying)
					{
						pressurizationParticle3.Stop();
					}
				}
			}
			if (DepressurizationParticles != null && DepressurizationParticles.Count != 0)
			{
				foreach (ParticleSystem depressurizationParticle3 in DepressurizationParticles)
				{
					if (depressurizationParticle3.isPlaying)
					{
						depressurizationParticle3.Stop();
					}
				}
			}
			foreach (SoundEffect pressurizationSound4 in PressurizationSounds)
			{
				if (pressurizationSound4.IsPlaying)
				{
					pressurizationSound4.Play(1);
				}
			}
		}

		public void ChangeAirPressure(float pressure)
		{
			NetworkController.Instance.SendToGameServer(new RoomPressureMessage
			{
				ID = new VesselObjectID(ParentVessel.GUID, InSceneID),
				TargetPressure = pressure
			});
		}

		public void ChangeAirPressure(SceneTriggerRoom room)
		{
			NetworkController.Instance.SendToGameServer(new RoomPressureMessage
			{
				ID = new VesselObjectID(ParentVessel.GUID, InSceneID),
				TargetRoomID = ((!(room == null)) ? new VesselObjectID(room.ParentVessel.GUID, room.InSceneID) : null)
			});
		}

		private void Awake()
		{
			if (PressureGOsp != null)
			{
				PressureGOsp.Initialize();
			}
		}

		protected override void Start()
		{
			base.Start();
			if (ParentVessel == null)
			{
				ParentVessel = GetComponentInParent<SpaceObjectVessel>();
			}
			ConnectedRooms = GetConnectedRooms();
			if (!(ParentVessel != null))
			{
				return;
			}
			SceneDoor[] componentsInChildren = ParentVessel.GeometryRoot.GetComponentsInChildren<SceneDoor>();
			foreach (SceneDoor sceneDoor in componentsInChildren)
			{
				if (sceneDoor.Room1 == this || sceneDoor.Room2 == this)
				{
					Doors.Add(sceneDoor);
				}
			}
		}

		public void ToggleGravityOn()
		{
			ToggleGravity(true);
		}

		public void ToggleGravityOff()
		{
			ToggleGravity(false);
		}

		public void ToggleGravity(bool? enabled = null)
		{
			SpaceObjectVessel parentVessel = ParentVessel;
			RoomDetails roomTrigger = new RoomDetails
			{
				InSceneID = InSceneID,
				UseGravity = ((!enabled.HasValue) ? (!UseGravity) : (enabled == true)),
				AirFiltering = AirFiltering
			};
			parentVessel.ChangeStats(null, null, null, null, null, null, roomTrigger);
		}

		public void ToggleAirFilteringOn()
		{
			ToggleAirFiltering(true);
		}

		public void ToggleAirFilteringOff()
		{
			ToggleAirFiltering(false);
		}

		public void ToggleAirFiltering(bool? enabled = null)
		{
			SpaceObjectVessel parentVessel = ParentVessel;
			RoomDetails roomTrigger = new RoomDetails
			{
				InSceneID = InSceneID,
				UseGravity = UseGravity,
				AirFiltering = ((!enabled.HasValue) ? (!AirFiltering) : (enabled == true))
			};
			parentVessel.ChangeStats(null, null, null, null, null, null, roomTrigger);
		}

		private void Update()
		{
		}

		private void OnDisable()
		{
			if (isApplicationQuitting)
			{
				return;
			}
			if (MyPlayer.Instance != null)
			{
				TransitionTriggerHelper componentInChildren = MyPlayer.Instance.GetComponentInChildren<TransitionTriggerHelper>();
				if (componentInChildren != null)
				{
					componentInChildren.ExitTriggers(GetComponentsInChildren<Collider>());
				}
			}
			if (ParentVessel != null)
			{
				TransitionTriggerHelper[] componentsInChildren = ParentVessel.GeometryRoot.GetComponentsInChildren<TransitionTriggerHelper>();
				foreach (TransitionTriggerHelper transitionTriggerHelper in componentsInChildren)
				{
					transitionTriggerHelper.ExitTriggers(GetComponentsInChildren<Collider>());
				}
			}
		}

		private void OnApplicationQuit()
		{
			isApplicationQuitting = true;
		}

		private List<SceneTriggerRoom> GetConnectedRooms(HashSet<SceneTriggerRoom> traversedRooms = null)
		{
			if (traversedRooms == null)
			{
				traversedRooms = new HashSet<SceneTriggerRoom>();
			}
			List<SceneTriggerRoom> list = new List<SceneTriggerRoom>();
			if (traversedRooms.Add(this))
			{
				list.Add(this);
				if (ParentVessel != null)
				{
					List<SceneTriggerRoom> list2 = ParentVessel.RoomTriggers.Values.Where((SceneTriggerRoom m) => m.ParentRoom == this && m.RoomName.IsNullOrEmpty()).ToList();
					if (ParentRoom != null && ParentRoom.RoomName.IsNullOrEmpty())
					{
						list2.Add(ParentRoom);
					}
					{
						foreach (SceneTriggerRoom r in list2)
						{
							SceneDoor sceneDoor = ParentVessel.Doors.Values.FirstOrDefault((SceneDoor m) => (m.Room1 == this && m.Room2 == r) || (m.Room1 == r && m.Room2 == this));
							if (sceneDoor == null || !sceneDoor.IsSealable)
							{
								list.AddRange(r.GetConnectedRooms(traversedRooms));
							}
						}
						return list;
					}
				}
			}
			return list;
		}
	}
}
