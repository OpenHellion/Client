using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class Asteroid : SpaceObjectVessel
	{
		public Dictionary<int, AsteroidMiningPoint> MiningPoints = new Dictionary<int, AsteroidMiningPoint>();

		private List<ResourceType> level1Resources;

		private bool sceneLoadStarted;

		public override SpaceObjectType Type
		{
			get
			{
				return SpaceObjectType.Asteroid;
			}
		}

		public override void ChangeStats(Vector3? thrust = null, Vector3? rotation = null, Vector3? autoStabilize = null, float? engineThrustPercentage = null, SubSystemDetails subSystem = null, GeneratorDetails distributionSystem = null, RoomDetails roomTrigger = null, DoorDetails door = null, SceneTriggerExecuterDetails sceneTriggerExecuter = null, SceneDockingPortDetails dockingPort = null, AttachPointDetails attachPoint = null, long? stabilizationTarget = null, SpawnPointStats spawnPoint = null, float? selfDestructTime = null, string emblemId = null)
		{
		}

		public override void DestroyGeometry()
		{
			base.DestroyGeometry();
			base.IsDummyObject = true;
			sceneLoadStarted = false;
			base.SceneObjectsLoaded = false;
		}

		public static Asteroid Create(ObjectTransform trans, VesselData data, bool isMainObject)
		{
			Asteroid asteroid = ArtificialBody.Create(SpaceObjectType.Asteroid, trans.GUID, trans, isMainObject) as Asteroid;
			if (data != null)
			{
				asteroid.VesselData = data;
			}
			asteroid.Radius = 1000.0;
			asteroid.IsDummyObject = true;
			asteroid.SceneObjectsLoaded = false;
			Client.Instance.Map.InitializeMapObject(asteroid);
			return asteroid;
		}

		private void Start()
		{
			ConnectMessageListeners();
		}

		private void FixedUpdate()
		{
			SmoothRotation(Time.fixedDeltaTime);
		}

		private void OnDestroy()
		{
			DisconnectMessageListeners();
			Client.Instance.SolarSystem.RemoveArtificialBody(this);
			Client.Instance.Map.RemoveMapObject(this);
			SceneHelper.RemoveCubemapProbes(base.gameObject);
			Client.Instance.ActiveVessels.Remove(base.GUID);
		}

		public void ConnectMessageListeners()
		{
			Client.Instance.NetworkController.EventSystem.AddListener(typeof(InitializeSpaceObjectMessage), InitializeSpaceObjectMessageListener);
		}

		public void DisconnectMessageListeners()
		{
			Client.Instance.NetworkController.EventSystem.RemoveListener(typeof(InitializeSpaceObjectMessage), InitializeSpaceObjectMessageListener);
		}

		private void InitializeSpaceObjectMessageListener(NetworkData data)
		{
			InitializeSpaceObjectMessage initializeSpaceObjectMessage = data as InitializeSpaceObjectMessage;
			if (initializeSpaceObjectMessage.GUID == base.GUID)
			{
				UpdateDynamicObjects(initializeSpaceObjectMessage.DynamicObjects);
				UpdateCharacters(initializeSpaceObjectMessage.Characters);
				UpdateCorpses(initializeSpaceObjectMessage.Corpses);
			}
		}

		public override void ParseSpawnData(SpawnObjectResponseData data)
		{
			base.ParseSpawnData(data);
			SpawnAsteroidResponseData spawnAsteroidResponseData = data as SpawnAsteroidResponseData;
			VesselData = spawnAsteroidResponseData.Data;
			base.Radius = spawnAsteroidResponseData.Radius;
			if (!sceneLoadStarted && !base.SceneObjectsLoaded && !base.IsDummyObject)
			{
				sceneLoadStarted = true;
				base.gameObject.SetActive(true);
				StartCoroutine(LoadScenesCoroutine(spawnAsteroidResponseData.MiningPoints));
			}
		}

		private IEnumerator LoadScenes(Transform rootTransform)
		{
			CreateArtificalRigidbody();
			yield return StartCoroutine(Client.Instance.SceneLoader.LoadSceneCoroutine(SceneLoader.SceneType.Asteroid, (long)base.SceneID));
			GameObject sceneRoot = Client.Instance.SceneLoader.GetLoadedScene(SceneLoader.SceneType.Asteroid, (long)base.SceneID);
			sceneRoot.transform.SetParent(rootTransform);
			sceneRoot.transform.Reset();
			RootObject = sceneRoot;
			if (GeometryRoot != null)
			{
				DestructionEffects = GeometryRoot.GetComponentInChildren<VesselDestructionEffects>(true);
				if (DestructionEffects != null)
				{
					DestructionEffects.gameObject.SetActive(false);
				}
			}
			if (TargetRotation.HasValue)
			{
				SetTargetPositionAndRotation(null, TargetRotation.Value, true);
			}
			sceneRoot.GetComponentInParent<SpaceObjectVessel>().ActivateGeometry = true;
			Client.Instance.ActiveVessels[base.GUID] = this;
			sceneRoot.SetActive(true);
			sceneRoot.SetActive(true);
			SceneHelper.FillCubemapProbes(sceneRoot);
			SceneHelper.CheckTags(sceneRoot, (VesselData == null) ? string.Empty : VesselData.Tag);
		}

		public IEnumerator LoadScenesCoroutine(List<AsteroidMiningPointDetails> miningPoints)
		{
			Client.Instance.CanvasManager.ToggleBusyLoading(true);
			yield return StartCoroutine(LoadScenes(GeometryRoot.transform));
			SceneHelper.FillMiningPoints(this, base.gameObject, MiningPoints, miningPoints);
			base.IsDummyObject = false;
			base.SceneObjectsLoaded = true;
			Client.Instance.CanvasManager.ToggleBusyLoading(false);
		}

		private void Update()
		{
			if (ActivateGeometry)
			{
				if (RootObject.activeInHierarchy)
				{
					ActivateGeometry = false;
				}
				else
				{
					RootObject.SetActive(true);
				}
			}
		}
	}
}
