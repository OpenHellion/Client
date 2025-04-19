using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OpenHellion;
using OpenHellion.Net;
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

		public override SpaceObjectType Type => SpaceObjectType.Asteroid;

		public override void ChangeStats(Vector3? thrust = null, Vector3? rotation = null,
			Vector3? autoStabilize = null, float? engineThrustPercentage = null, SubSystemDetails subSystem = null,
			GeneratorDetails distributionSystem = null, RoomDetails roomTrigger = null, DoorDetails door = null,
			SceneTriggerExecutorDetails sceneTriggerExecutor = null, SceneDockingPortDetails dockingPort = null,
			AttachPointDetails attachPoint = null, long? stabilizationTarget = null, SpawnPointStats spawnPoint = null,
			float? selfDestructTime = null, string emblemId = null)
		{
		}

		public override void DestroyGeometry()
		{
			base.DestroyGeometry();
			IsDummyObject = true;
			sceneLoadStarted = false;
			SceneObjectsLoaded = false;
		}

		public static Asteroid Create(ObjectTransform trans, VesselData data, bool isMainObject)
		{
			Asteroid asteroid =
				ArtificialBody.CreateImpl(SpaceObjectType.Asteroid, trans.GUID, trans, isMainObject) as Asteroid;
			if (data != null)
			{
				asteroid.VesselData = data;
			}

			asteroid.Radius = 1000.0;
			asteroid.IsDummyObject = true;
			asteroid.SceneObjectsLoaded = false;
			World.Map.InitialiseMapObject(asteroid);
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
			World.SolarSystem.RemoveArtificialBody(this);
			World.Map.RemoveMapObject(this);
			SceneHelper.RemoveCubemapProbes(gameObject, World);
			World.ActiveVessels.Remove(Guid);
		}

		public void ConnectMessageListeners()
		{
			EventSystem.AddListener(typeof(InitializeSpaceObjectMessage), InitializeSpaceObjectMessageListener);
		}

		public void DisconnectMessageListeners()
		{
			EventSystem.RemoveListener(typeof(InitializeSpaceObjectMessage), InitializeSpaceObjectMessageListener);
		}

		private void InitializeSpaceObjectMessageListener(NetworkData data)
		{
			InitializeSpaceObjectMessage initializeSpaceObjectMessage = data as InitializeSpaceObjectMessage;
			if (initializeSpaceObjectMessage.GUID == Guid)
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
			Radius = spawnAsteroidResponseData.Radius;
			if (!sceneLoadStarted && !SceneObjectsLoaded && !IsDummyObject)
			{
				sceneLoadStarted = true;
				gameObject.SetActive(true);
				LoadAsync(spawnAsteroidResponseData.MiningPoints).Forget();
			}
		}

		private async UniTask LoadAsteroidsAsync(Transform rootTransform)
		{
			await Globals.SceneLoader.LoadSceneAsync(SceneLoader.SceneType.CelestialBody, (long)SceneID);
			GameObject sceneRoot =
				Globals.SceneLoader.GetLoadedScene(SceneLoader.SceneType.CelestialBody, SceneID);
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
			World.ActiveVessels[Guid] = this;
			sceneRoot.SetActive(true);
			sceneRoot.SetActive(true);
			SceneHelper.FillCubemapProbes(sceneRoot, World);
			SceneHelper.CheckTags(sceneRoot, VesselData == null ? string.Empty : VesselData.Tag);
		}

		public async UniTask LoadAsync(List<AsteroidMiningPointDetails> miningPoints)
		{
			World.InGameGUI.ToggleBusyLoading(true);
			await LoadAsteroidsAsync(GeometryRoot.transform);
			SceneHelper.FillMiningPoints(this, gameObject, MiningPoints, miningPoints);
			IsDummyObject = false;
			SceneObjectsLoaded = true;
			World.InGameGUI.ToggleBusyLoading(false);
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
