using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Effects;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.LevelDesign
{
	public static class SceneHelper
	{
		[CompilerGenerated]
		private sealed class _003CFillSubSystems_003Ec__AnonStorey0
		{
			internal SubSystem subSys;

			internal bool _003C_003Em__0(SubSystemDetails m)
			{
				return m.InSceneID == subSys.InSceneID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CFillGenerators_003Ec__AnonStorey1
		{
			internal Generator generator;

			internal bool _003C_003Em__0(GeneratorDetails m)
			{
				return m.InSceneID == generator.InSceneID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CFillRoomTriggers_003Ec__AnonStorey2
		{
			internal SceneTriggerRoom str;

			internal bool _003C_003Em__0(RoomDetails m)
			{
				return m.InSceneID == str.InSceneID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CFillNameTags_003Ec__AnonStorey3
		{
			internal SceneNameTag obj;

			internal bool _003C_003Em__0(NameTagData m)
			{
				return m.InSceneID == obj.InSceneID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CFillRepairPoints_003Ec__AnonStorey4
		{
			internal VesselRepairPoint obj;

			internal bool _003C_003Em__0(VesselRepairPointDetails m)
			{
				return m.InSceneID == obj.InSceneID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CFillMiningPoints_003Ec__AnonStorey5
		{
			internal AsteroidMiningPoint obj;

			internal bool _003C_003Em__0(AsteroidMiningPointDetails m)
			{
				return m.InSceneID == obj.InSceneID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CFillResourceContainers_003Ec__AnonStorey6
		{
			internal ResourceContainer rc;

			internal bool _003C_003Em__0(ResourceContainerDetails m)
			{
				return m.InSceneID == rc.InSceneID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CFillDoors_003Ec__AnonStorey7
		{
			internal SceneDoor sd;

			internal bool _003C_003Em__0(DoorDetails m)
			{
				return m.InSceneID == sd.InSceneID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CFillSceneTriggerExecuters_003Ec__AnonStorey8
		{
			internal SceneTriggerExecuter ste;

			internal bool _003C_003Em__0(SceneTriggerExecuterDetails m)
			{
				return m.InSceneID == ste.InSceneID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CFillAttachPoints_003Ec__AnonStorey9
		{
			internal BaseSceneAttachPoint bsap;

			internal bool _003C_003Em__0(AttachPointDetails m)
			{
				return m.InSceneID == bsap.InSceneID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CFillSceneDockingPorts_003Ec__AnonStoreyA
		{
			internal SceneDockingPort obj;

			internal bool _003C_003Em__0(SceneDockingPortDetails m)
			{
				return m.ID.InSceneID == obj.InSceneID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CFillSpawnWithChanceData_003Ec__AnonStoreyB
		{
			internal SpawnObjectsWithChanceScene sow;

			internal bool _003C_003Em__0(SpawnObjectsWithChanceDetails m)
			{
				return m.InSceneID == sow.InSceneID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CFillCargoBays_003Ec__AnonStoreyC
		{
			internal SceneCargoBay cargoBay;

			internal bool _003C_003Em__0(CargoBayDetails m)
			{
				return m.InSceneID == cargoBay.InSceneID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CFillSpawnPoints_003Ec__AnonStoreyD
		{
			internal SceneSpawnPoint obj;

			internal bool _003C_003Em__0(SpawnPointStats m)
			{
				return m.InSceneID == obj.InSceneID;
			}
		}

		[CompilerGenerated]
		private static Func<GameObject, bool> _003C_003Ef__am_0024cache0;

		public static List<StructureSceneConnectionData> GetStructureConnectionData(GameObject rootObject)
		{
			return new List<StructureSceneConnectionData>();
		}

		public static List<SpawnPointData> GetSpawnPointsData(GameObject rootObject)
		{
			return new List<SpawnPointData>();
		}

		public static List<SubSystemData> GetSubSystemsData(GameObject rootObject)
		{
			return new List<SubSystemData>();
		}

		public static List<GeneratorData> GetGeneratorsData(GameObject rootObject)
		{
			return new List<GeneratorData>();
		}

		public static List<NameTagData> GetNameTagsData(GameObject rootObject)
		{
			return new List<NameTagData>();
		}

		public static List<VesselRepairPointData> GetVesselRepaitPointsData(GameObject rootObject)
		{
			return new List<VesselRepairPointData>();
		}

		public static List<AsteroidMiningPointData> GetMiningPointsData(GameObject rootObject)
		{
			return new List<AsteroidMiningPointData>();
		}

		public static List<ResourceContainerData> GetResourceContainersData(GameObject rootObject)
		{
			return new List<ResourceContainerData>();
		}

		public static List<DynamicObjectSceneData> GetDynamicObjectsData(GameObject rootObject)
		{
			return new List<DynamicObjectSceneData>();
		}

		public static List<BaseAttachPointData> GetAttachPointData(GameObject rootObject)
		{
			return new List<BaseAttachPointData>();
		}

		public static List<SceneDockingPortData> GetSceneDockingPortData(GameObject rootObject)
		{
			return new List<SceneDockingPortData>();
		}

		public static List<SpawnObjectsWithChanceData> GetSpawnObjectsWithChanceData(GameObject rootObject)
		{
			return new List<SpawnObjectsWithChanceData>();
		}

		public static CargoBayData GetSceneCargoBayData(GameObject rootObject)
		{
			return null;
		}

		public static List<RoomData> GetRoomTriggersData(GameObject rootObject)
		{
			return new List<RoomData>();
		}

		public static float VolumeOfGameObject(GameObject gobj)
		{
			bool flag = gobj.GetComponent<SceneTriggerRoom>() != null;
			float num = 0f;
			Collider[] componentsInChildren = gobj.GetComponentsInChildren<Collider>();
			foreach (Collider collider in componentsInChildren)
			{
				if (collider != null && (!flag || (flag && (collider.gameObject.GetComponent<SceneTriggerRoomSegment>() != null || collider.gameObject == gobj))))
				{
					MeshFilter component = collider.GetComponent<MeshFilter>();
					if (component == null || collider is BoxCollider)
					{
						Vector3 size = collider.bounds.size;
						num += size.x * size.y * size.z * collider.transform.lossyScale.x * collider.transform.lossyScale.y * collider.transform.lossyScale.z;
					}
					else
					{
						num += VolumeOfMesh(component.sharedMesh) * collider.transform.lossyScale.x * collider.transform.lossyScale.y * collider.transform.lossyScale.z;
					}
				}
			}
			return num;
		}

		public static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
		{
			float num = p3.x * p2.y * p1.z;
			float num2 = p2.x * p3.y * p1.z;
			float num3 = p3.x * p1.y * p2.z;
			float num4 = p1.x * p3.y * p2.z;
			float num5 = p2.x * p1.y * p3.z;
			float num6 = p1.x * p2.y * p3.z;
			return 1f / 6f * (0f - num + num2 + num3 - num4 - num5 + num6);
		}

		public static float VolumeOfMesh(Mesh mesh)
		{
			float num = 0f;
			Vector3[] vertices = mesh.vertices;
			int[] triangles = mesh.triangles;
			for (int i = 0; i < mesh.triangles.Length; i += 3)
			{
				Vector3 p = vertices[triangles[i]];
				Vector3 p2 = vertices[triangles[i + 1]];
				Vector3 p3 = vertices[triangles[i + 2]];
				num += SignedVolumeOfTriangle(p, p2, p3);
			}
			return Mathf.Abs(num);
		}

		private static AirDoorParticleToggler CreateDoorAirParticles(SceneDoor door, SceneTriggerRoom room, UnityEngine.Object prefabObject)
		{
			return null;
		}

		public static List<DoorData> GetDoorsData(GameObject rootObject, string doorAirParticlesPrefabPath = "Assets/Prefabs/FX/Particles/AirDoorParticles.prefab", bool isAutoSaveAll = false)
		{
			return new List<DoorData>();
		}

		public static List<SceneTriggerExecuterData> GetSceneTriggerExecutersData(GameObject rootObject)
		{
			return new List<SceneTriggerExecuterData>();
		}

		public static ServerCollisionData GetCollisionData(GameObject rootObject)
		{
			ServerCollisionData serverCollisionData = new ServerCollisionData();
			serverCollisionData.MeshCollidersData = new List<MeshData>();
			serverCollisionData.PrimitiveCollidersData = new List<PrimitiveColliderData>();
			return serverCollisionData;
		}

		public static SecuritySystem GetSecuritySystem(GameObject sceneRoot)
		{
			return null;
		}

		public static void FillSubSystems(GameObject sceneRoot, Dictionary<int, SubSystem> subSystemsDict, List<SubSystemDetails> subSystemsDetails)
		{
			SubSystem[] componentsInChildren = sceneRoot.GetComponentsInChildren<SubSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_003CFillSubSystems_003Ec__AnonStorey0 _003CFillSubSystems_003Ec__AnonStorey = new _003CFillSubSystems_003Ec__AnonStorey0();
				_003CFillSubSystems_003Ec__AnonStorey.subSys = componentsInChildren[i];
				subSystemsDict[_003CFillSubSystems_003Ec__AnonStorey.subSys.InSceneID] = _003CFillSubSystems_003Ec__AnonStorey.subSys;
				_003CFillSubSystems_003Ec__AnonStorey.subSys.SetParentVessel(sceneRoot.GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel);
				if (subSystemsDetails != null)
				{
					SubSystemDetails subSystemDetails = subSystemsDetails.Find(_003CFillSubSystems_003Ec__AnonStorey._003C_003Em__0);
					if (subSystemDetails != null)
					{
						_003CFillSubSystems_003Ec__AnonStorey.subSys.SetDetails(subSystemDetails, true);
					}
				}
			}
		}

		public static void FillGenerators(GameObject sceneRoot, Dictionary<int, Generator> generatorsDict, List<GeneratorDetails> generatorsDetails)
		{
			Generator[] componentsInChildren = sceneRoot.GetComponentsInChildren<Generator>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_003CFillGenerators_003Ec__AnonStorey1 _003CFillGenerators_003Ec__AnonStorey = new _003CFillGenerators_003Ec__AnonStorey1();
				_003CFillGenerators_003Ec__AnonStorey.generator = componentsInChildren[i];
				generatorsDict[_003CFillGenerators_003Ec__AnonStorey.generator.InSceneID] = _003CFillGenerators_003Ec__AnonStorey.generator;
				_003CFillGenerators_003Ec__AnonStorey.generator.SetParentVessel(sceneRoot.GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel);
				if (generatorsDetails != null)
				{
					GeneratorDetails generatorDetails = generatorsDetails.Find(_003CFillGenerators_003Ec__AnonStorey._003C_003Em__0);
					if (generatorDetails != null)
					{
						_003CFillGenerators_003Ec__AnonStorey.generator.SetDetails(generatorDetails);
					}
				}
			}
		}

		public static void FillRoomTriggers(SpaceObjectVessel vessel, GameObject sceneRoot, Dictionary<int, SceneTriggerRoom> roomTriggersDict, List<RoomDetails> roomTriggersDetails)
		{
			SceneTriggerRoom[] componentsInChildren = sceneRoot.GetComponentsInChildren<SceneTriggerRoom>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_003CFillRoomTriggers_003Ec__AnonStorey2 _003CFillRoomTriggers_003Ec__AnonStorey = new _003CFillRoomTriggers_003Ec__AnonStorey2();
				_003CFillRoomTriggers_003Ec__AnonStorey.str = componentsInChildren[i];
				roomTriggersDict[_003CFillRoomTriggers_003Ec__AnonStorey.str.InSceneID] = _003CFillRoomTriggers_003Ec__AnonStorey.str;
				_003CFillRoomTriggers_003Ec__AnonStorey.str.ParentVessel = vessel;
				if (roomTriggersDetails != null)
				{
					RoomDetails roomDetails = roomTriggersDetails.Find(_003CFillRoomTriggers_003Ec__AnonStorey._003C_003Em__0);
					if (roomDetails != null)
					{
						_003CFillRoomTriggers_003Ec__AnonStorey.str.UseGravity = roomDetails.UseGravity;
						_003CFillRoomTriggers_003Ec__AnonStorey.str.AirPressure = roomDetails.AirPressure;
						_003CFillRoomTriggers_003Ec__AnonStorey.str.AirQuality = roomDetails.AirQuality;
					}
				}
			}
		}

		public static void FillNameTags(Ship ship, GameObject sceneRoot, Dictionary<int, SceneNameTag> nameTags, List<NameTagData> nameTagDetails)
		{
			SceneNameTag[] componentsInChildren = sceneRoot.GetComponentsInChildren<SceneNameTag>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_003CFillNameTags_003Ec__AnonStorey3 _003CFillNameTags_003Ec__AnonStorey = new _003CFillNameTags_003Ec__AnonStorey3();
				_003CFillNameTags_003Ec__AnonStorey.obj = componentsInChildren[i];
				if (_003CFillNameTags_003Ec__AnonStorey.obj.Local)
				{
					continue;
				}
				nameTags[_003CFillNameTags_003Ec__AnonStorey.obj.InSceneID] = _003CFillNameTags_003Ec__AnonStorey.obj;
				_003CFillNameTags_003Ec__AnonStorey.obj.ParentVessel = ship;
				if (nameTagDetails != null)
				{
					NameTagData nameTagData = nameTagDetails.Find(_003CFillNameTags_003Ec__AnonStorey._003C_003Em__0);
					if (nameTagData != null)
					{
						_003CFillNameTags_003Ec__AnonStorey.obj.NameTagText = nameTagData.NameTagText;
					}
				}
			}
		}

		public static void FillRepairPoints(Ship ship, GameObject sceneRoot, Dictionary<int, VesselRepairPoint> repairPoints, List<VesselRepairPointDetails> repairPointsDetails)
		{
			VesselRepairPoint[] componentsInChildren = sceneRoot.GetComponentsInChildren<VesselRepairPoint>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_003CFillRepairPoints_003Ec__AnonStorey4 _003CFillRepairPoints_003Ec__AnonStorey = new _003CFillRepairPoints_003Ec__AnonStorey4();
				_003CFillRepairPoints_003Ec__AnonStorey.obj = componentsInChildren[i];
				repairPoints[_003CFillRepairPoints_003Ec__AnonStorey.obj.InSceneID] = _003CFillRepairPoints_003Ec__AnonStorey.obj;
				_003CFillRepairPoints_003Ec__AnonStorey.obj.ParentVessel = ship;
				if (repairPointsDetails != null)
				{
					VesselRepairPointDetails vesselRepairPointDetails = repairPointsDetails.Find(_003CFillRepairPoints_003Ec__AnonStorey._003C_003Em__0);
					if (vesselRepairPointDetails != null)
					{
						_003CFillRepairPoints_003Ec__AnonStorey.obj.MaxHealth = vesselRepairPointDetails.MaxHealth;
						_003CFillRepairPoints_003Ec__AnonStorey.obj.Health = vesselRepairPointDetails.Health;
						_003CFillRepairPoints_003Ec__AnonStorey.obj.SecondaryDamageActive = vesselRepairPointDetails.SecondaryDamageActive;
					}
				}
			}
		}

		public static void FillMiningPoints(Asteroid asteroid, GameObject sceneRoot, Dictionary<int, AsteroidMiningPoint> repairPoints, List<AsteroidMiningPointDetails> miningPointsDetails)
		{
			AsteroidMiningPoint[] componentsInChildren = sceneRoot.GetComponentsInChildren<AsteroidMiningPoint>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_003CFillMiningPoints_003Ec__AnonStorey5 _003CFillMiningPoints_003Ec__AnonStorey = new _003CFillMiningPoints_003Ec__AnonStorey5();
				_003CFillMiningPoints_003Ec__AnonStorey.obj = componentsInChildren[i];
				repairPoints[_003CFillMiningPoints_003Ec__AnonStorey.obj.InSceneID] = _003CFillMiningPoints_003Ec__AnonStorey.obj;
				_003CFillMiningPoints_003Ec__AnonStorey.obj.ParentVessel = asteroid;
				if (miningPointsDetails != null)
				{
					AsteroidMiningPointDetails asteroidMiningPointDetails = miningPointsDetails.Find(_003CFillMiningPoints_003Ec__AnonStorey._003C_003Em__0);
					if (asteroidMiningPointDetails != null)
					{
						_003CFillMiningPoints_003Ec__AnonStorey.obj.SetDetails(asteroidMiningPointDetails);
					}
				}
			}
		}

		public static void FillResourceContainers(GameObject sceneRoot, Dictionary<int, ResourceContainer> resourceContainersDict, List<ResourceContainerDetails> resourceContainersDetails)
		{
			ResourceContainer[] componentsInChildren = sceneRoot.GetComponentsInChildren<ResourceContainer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_003CFillResourceContainers_003Ec__AnonStorey6 _003CFillResourceContainers_003Ec__AnonStorey = new _003CFillResourceContainers_003Ec__AnonStorey6();
				_003CFillResourceContainers_003Ec__AnonStorey.rc = componentsInChildren[i];
				resourceContainersDict[_003CFillResourceContainers_003Ec__AnonStorey.rc.InSceneID] = _003CFillResourceContainers_003Ec__AnonStorey.rc;
				if (resourceContainersDetails != null)
				{
					ResourceContainerDetails resourceContainerDetails = resourceContainersDetails.Find(_003CFillResourceContainers_003Ec__AnonStorey._003C_003Em__0);
					if (resourceContainerDetails != null)
					{
						_003CFillResourceContainers_003Ec__AnonStorey.rc.SetDetails(resourceContainerDetails);
					}
				}
			}
		}

		public static void FillDoors(SpaceObjectVessel vessel, GameObject sceneRoot, Dictionary<int, SceneDoor> doorsDict, List<DoorDetails> doorsDetails)
		{
			SceneDoor[] componentsInChildren = sceneRoot.GetComponentsInChildren<SceneDoor>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_003CFillDoors_003Ec__AnonStorey7 _003CFillDoors_003Ec__AnonStorey = new _003CFillDoors_003Ec__AnonStorey7();
				_003CFillDoors_003Ec__AnonStorey.sd = componentsInChildren[i];
				doorsDict[_003CFillDoors_003Ec__AnonStorey.sd.InSceneID] = _003CFillDoors_003Ec__AnonStorey.sd;
				_003CFillDoors_003Ec__AnonStorey.sd.ParentVessel = vessel;
				if (doorsDetails != null)
				{
					DoorDetails doorDetails = doorsDetails.Find(_003CFillDoors_003Ec__AnonStorey._003C_003Em__0);
					if (doorDetails != null)
					{
						_003CFillDoors_003Ec__AnonStorey.sd.SetDoorDetails(doorDetails, true);
					}
				}
			}
		}

		public static void FillSceneTriggerExecuters(SpaceObjectVessel vessel, GameObject sceneRoot, Dictionary<int, SceneTriggerExecuter> sceneTriggerExecuters, List<SceneTriggerExecuterDetails> sceneTriggersExecuterDetails)
		{
			SceneTriggerExecuter[] componentsInChildren = sceneRoot.GetComponentsInChildren<SceneTriggerExecuter>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_003CFillSceneTriggerExecuters_003Ec__AnonStorey8 _003CFillSceneTriggerExecuters_003Ec__AnonStorey = new _003CFillSceneTriggerExecuters_003Ec__AnonStorey8();
				_003CFillSceneTriggerExecuters_003Ec__AnonStorey.ste = componentsInChildren[i];
				sceneTriggerExecuters[_003CFillSceneTriggerExecuters_003Ec__AnonStorey.ste.InSceneID] = _003CFillSceneTriggerExecuters_003Ec__AnonStorey.ste;
				_003CFillSceneTriggerExecuters_003Ec__AnonStorey.ste.ParentVessel = vessel;
				if (sceneTriggersExecuterDetails != null)
				{
					SceneTriggerExecuterDetails sceneTriggerExecuterDetails = sceneTriggersExecuterDetails.Find(_003CFillSceneTriggerExecuters_003Ec__AnonStorey._003C_003Em__0);
					if (sceneTriggerExecuterDetails != null)
					{
						_003CFillSceneTriggerExecuters_003Ec__AnonStorey.ste.SetExecuterDetails(sceneTriggerExecuterDetails, true);
					}
				}
			}
		}

		public static void FillAttachPoints(SpaceObjectVessel vessel, GameObject sceneRoot, Dictionary<int, BaseSceneAttachPoint> attachPointDict, List<AttachPointDetails> AttachPointsDetails)
		{
			BaseSceneAttachPoint[] componentsInChildren = sceneRoot.GetComponentsInChildren<BaseSceneAttachPoint>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_003CFillAttachPoints_003Ec__AnonStorey9 _003CFillAttachPoints_003Ec__AnonStorey = new _003CFillAttachPoints_003Ec__AnonStorey9();
				_003CFillAttachPoints_003Ec__AnonStorey.bsap = componentsInChildren[i];
				attachPointDict[_003CFillAttachPoints_003Ec__AnonStorey.bsap.InSceneID] = _003CFillAttachPoints_003Ec__AnonStorey.bsap;
				_003CFillAttachPoints_003Ec__AnonStorey.bsap.ParentVessel = vessel;
				if (AttachPointsDetails != null && _003CFillAttachPoints_003Ec__AnonStorey.bsap is SceneMachineryPartSlot)
				{
					AttachPointDetails attachPointDetails = AttachPointsDetails.Find(_003CFillAttachPoints_003Ec__AnonStorey._003C_003Em__0);
					if (attachPointDetails != null && attachPointDetails.AuxDetails != null)
					{
						(_003CFillAttachPoints_003Ec__AnonStorey.bsap as SceneMachineryPartSlot).SetActive((attachPointDetails.AuxDetails as MachineryPartSlotAuxDetails).IsActive, false);
					}
				}
			}
		}

		public static void FillSceneDockingPorts(Ship ship, GameObject sceneRoot, Dictionary<int, SceneDockingPort> sceneDockingPorts, List<SceneDockingPortDetails> sceneDockingPortDetails)
		{
			SceneDockingPort[] componentsInChildren = sceneRoot.GetComponentsInChildren<SceneDockingPort>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_003CFillSceneDockingPorts_003Ec__AnonStoreyA _003CFillSceneDockingPorts_003Ec__AnonStoreyA = new _003CFillSceneDockingPorts_003Ec__AnonStoreyA();
				_003CFillSceneDockingPorts_003Ec__AnonStoreyA.obj = componentsInChildren[i];
				sceneDockingPorts[_003CFillSceneDockingPorts_003Ec__AnonStoreyA.obj.InSceneID] = _003CFillSceneDockingPorts_003Ec__AnonStoreyA.obj;
				_003CFillSceneDockingPorts_003Ec__AnonStoreyA.obj.ParentShip = ship;
				if (sceneDockingPortDetails != null)
				{
					SceneDockingPortDetails sceneDockingPortDetails2 = sceneDockingPortDetails.Find(_003CFillSceneDockingPorts_003Ec__AnonStoreyA._003C_003Em__0);
					if (sceneDockingPortDetails2 != null)
					{
						_003CFillSceneDockingPorts_003Ec__AnonStoreyA.obj.SetDetails(sceneDockingPortDetails2, true);
					}
				}
			}
		}

		public static void FillSpawnWithChanceData(GameObject sceneRoot, List<SpawnObjectsWithChanceDetails> SpawnChanceDetails)
		{
			SpawnObjectsWithChanceScene[] componentsInChildren = sceneRoot.GetComponentsInChildren<SpawnObjectsWithChanceScene>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_003CFillSpawnWithChanceData_003Ec__AnonStoreyB _003CFillSpawnWithChanceData_003Ec__AnonStoreyB = new _003CFillSpawnWithChanceData_003Ec__AnonStoreyB();
				_003CFillSpawnWithChanceData_003Ec__AnonStoreyB.sow = componentsInChildren[i];
				SpawnObjectsWithChanceDetails spawnObjectsWithChanceDetails = SpawnChanceDetails.Find(_003CFillSpawnWithChanceData_003Ec__AnonStoreyB._003C_003Em__0);
				if (spawnObjectsWithChanceDetails != null)
				{
					_003CFillSpawnWithChanceData_003Ec__AnonStoreyB.sow.SetDetails(spawnObjectsWithChanceDetails);
				}
			}
		}

		public static void FillCargoBays(GameObject sceneRoot, Dictionary<int, SceneCargoBay> cargoBaysDict, List<CargoBayDetails> cargoBaysDetails)
		{
			SceneCargoBay[] componentsInChildren = sceneRoot.GetComponentsInChildren<SceneCargoBay>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_003CFillCargoBays_003Ec__AnonStoreyC _003CFillCargoBays_003Ec__AnonStoreyC = new _003CFillCargoBays_003Ec__AnonStoreyC();
				_003CFillCargoBays_003Ec__AnonStoreyC.cargoBay = componentsInChildren[i];
				cargoBaysDict[_003CFillCargoBays_003Ec__AnonStoreyC.cargoBay.InSceneID] = _003CFillCargoBays_003Ec__AnonStoreyC.cargoBay;
				if (cargoBaysDetails != null)
				{
					CargoBayDetails cargoBayDetails = cargoBaysDetails.Find(_003CFillCargoBays_003Ec__AnonStoreyC._003C_003Em__0);
					if (cargoBayDetails != null)
					{
						_003CFillCargoBays_003Ec__AnonStoreyC.cargoBay.SetDetails(cargoBayDetails);
					}
				}
			}
		}

		public static void FillSpawnPoints(Ship sh, GameObject sceneRoot, Dictionary<int, SceneSpawnPoint> sceneSpawnPoints, List<SpawnPointStats> sceneSpawnPointStats)
		{
			SceneSpawnPoint[] componentsInChildren = sceneRoot.GetComponentsInChildren<SceneSpawnPoint>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_003CFillSpawnPoints_003Ec__AnonStoreyD _003CFillSpawnPoints_003Ec__AnonStoreyD = new _003CFillSpawnPoints_003Ec__AnonStoreyD();
				_003CFillSpawnPoints_003Ec__AnonStoreyD.obj = componentsInChildren[i];
				sceneSpawnPoints[_003CFillSpawnPoints_003Ec__AnonStoreyD.obj.InSceneID] = _003CFillSpawnPoints_003Ec__AnonStoreyD.obj;
				_003CFillSpawnPoints_003Ec__AnonStoreyD.obj.ParentVessel = sh;
				if (sceneSpawnPointStats != null)
				{
					SpawnPointStats spawnPointStats = sceneSpawnPointStats.Find(_003CFillSpawnPoints_003Ec__AnonStoreyD._003C_003Em__0);
					if (spawnPointStats != null)
					{
						_003CFillSpawnPoints_003Ec__AnonStoreyD.obj.SetStats(spawnPointStats);
					}
				}
			}
		}

		public static void FillCubemapProbes(GameObject sceneRoot)
		{
			CubemapProbe[] componentsInChildren = sceneRoot.GetComponentsInChildren<CubemapProbe>();
			foreach (CubemapProbe cubemapProbe in componentsInChildren)
			{
				Client.Instance.CubemapRenderer.AddReflectionProbe(cubemapProbe.GetComponent<ReflectionProbe>());
			}
		}

		public static void RemoveCubemapProbes(GameObject sceneRoot)
		{
			CubemapProbe[] componentsInChildren = sceneRoot.GetComponentsInChildren<CubemapProbe>();
			foreach (CubemapProbe cubemapProbe in componentsInChildren)
			{
				Client.Instance.CubemapRenderer.RemoveReflectionProbe(cubemapProbe.GetComponent<ReflectionProbe>());
			}
		}

		public static void SerializeOcclusionObjects(GameObject sceneRoot)
		{
			ZeroOccluder[] componentsInChildren = sceneRoot.GetComponentsInChildren<ZeroOccluder>(true);
			foreach (ZeroOccluder zeroOccluder in componentsInChildren)
			{
				zeroOccluder.SerializeOcclusionObjects();
			}
		}

		public static bool CompareTags(string sceneTags, string objectTags)
		{
			if (objectTags == null)
			{
				objectTags = string.Empty;
			}
			if (sceneTags == null)
			{
				sceneTags = string.Empty;
			}
			if (sceneTags == objectTags)
			{
				return true;
			}
			string[] array = objectTags.Split(';');
			string[] array2 = sceneTags.Split(';');
			foreach (string text in array2)
			{
				string[] array3 = array;
				foreach (string text2 in array3)
				{
					if (text == text2)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static void FillEmblems(GameObject sceneRoot, SpaceObjectVessel vessel)
		{
			SceneVesselEmblem[] componentsInChildren = sceneRoot.GetComponentsInChildren<SceneVesselEmblem>(true);
			foreach (SceneVesselEmblem item in componentsInChildren)
			{
				vessel.Emblems.Add(item);
			}
		}

		public static void FillDamagePoints(GameObject sceneRoot, SpaceObjectVessel vessel)
		{
			vessel.DamagePoints = new List<DamagePointData>();
			SceneDamagePoint[] componentsInChildren = sceneRoot.GetComponentsInChildren<SceneDamagePoint>(true);
			foreach (SceneDamagePoint sceneDamagePoint in componentsInChildren)
			{
				List<GameObject> effects = sceneDamagePoint.Effects;
				if (_003C_003Ef__am_0024cache0 == null)
				{
					_003C_003Ef__am_0024cache0 = _003CFillDamagePoints_003Em__0;
				}
				sceneDamagePoint.Effects = effects.Where(_003C_003Ef__am_0024cache0).ToList();
				if (sceneDamagePoint.Effects.Count > 0)
				{
					sceneDamagePoint.transform.parent.gameObject.Activate(true);
					vessel.DamagePoints.Add(new DamagePointData
					{
						VisibilityThreshold = sceneDamagePoint.VisibilityThreshold,
						UseOcclusion = sceneDamagePoint.UseOcclusion,
						ParentTransform = sceneDamagePoint.transform.parent,
						Position = sceneDamagePoint.transform.localPosition,
						Rotation = sceneDamagePoint.transform.localRotation,
						Scale = sceneDamagePoint.transform.localScale,
						Effects = sceneDamagePoint.Effects
					});
				}
				UnityEngine.Object.Destroy(sceneDamagePoint.gameObject);
			}
		}

		public static void CheckTags(GameObject sceneRoot, string sceneTags)
		{
			SceneObjectTag[] componentsInChildren = sceneRoot.GetComponentsInChildren<SceneObjectTag>(true);
			foreach (SceneObjectTag sceneObjectTag in componentsInChildren)
			{
				if (sceneObjectTag.TagAction != 0)
				{
					bool flag = CompareTags(sceneTags, SceneTagObject.TagsToString(sceneObjectTag.Tags));
					sceneObjectTag.gameObject.SetActive((sceneObjectTag.TagAction == TagAction.DisableIfTagIs && !flag) || (sceneObjectTag.TagAction == TagAction.EnableIfTagIs && flag));
				}
			}
			SceneTriggerExecuter[] componentsInChildren2 = sceneRoot.GetComponentsInChildren<SceneTriggerExecuter>(true);
			foreach (SceneTriggerExecuter sceneTriggerExecuter in componentsInChildren2)
			{
				if (sceneTriggerExecuter.TagAction == TagAction.None)
				{
					continue;
				}
				bool flag2 = CompareTags(sceneTags, sceneTriggerExecuter.Tags);
				sceneTriggerExecuter.enabled = (sceneTriggerExecuter.TagAction == TagAction.DisableIfTagIs && !flag2) || (sceneTriggerExecuter.TagAction == TagAction.EnableIfTagIs && flag2);
				SceneTrigger[] componentsInChildren3 = sceneRoot.GetComponentsInChildren<SceneTrigger>(true);
				foreach (SceneTrigger sceneTrigger in componentsInChildren3)
				{
					if (sceneTrigger.Executer == sceneTriggerExecuter)
					{
						Collider[] components = sceneTrigger.GetComponents<Collider>();
						foreach (Collider collider in components)
						{
							collider.enabled = sceneTriggerExecuter.enabled;
						}
						sceneTrigger.enabled = sceneTriggerExecuter.enabled;
					}
				}
			}
			SceneSpawnPoint[] componentsInChildren4 = sceneRoot.GetComponentsInChildren<SceneSpawnPoint>(true);
			foreach (SceneSpawnPoint sceneSpawnPoint in componentsInChildren4)
			{
				if (sceneSpawnPoint.TagAction != 0)
				{
					bool flag3 = CompareTags(sceneTags, sceneSpawnPoint.Tags);
					sceneSpawnPoint.enabled = (sceneSpawnPoint.TagAction == TagAction.DisableIfTagIs && !flag3) || (sceneSpawnPoint.TagAction == TagAction.EnableIfTagIs && flag3);
				}
			}
		}

		[CompilerGenerated]
		private static bool _003CFillDamagePoints_003Em__0(GameObject m)
		{
			return m != null;
		}
	}
}
