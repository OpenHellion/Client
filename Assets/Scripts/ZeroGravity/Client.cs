using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using TriInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.Effects;
using ZeroGravity.LevelDesign;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;
using ZeroGravity.UI;
using OpenHellion.ProviderSystem;
using OpenHellion.Networking;
using OpenHellion.Networking.Message.MainServer;
using OpenHellion.Networking.Message;
using OpenHellion.Util;
using UnityEngine.InputSystem;

namespace ZeroGravity
{
	public class Client : MonoBehaviour
	{
		public enum SPGameMode
		{
			Standard,
			Sandbox
		}

		public enum SceneLoadTypeValue
		{
			Simple,
			PreloadWithCopy
		}

		private readonly string _spServerPath = Directory.GetCurrentDirectory() + "\\Hellion_Data\\HELLION_SP";

		private const string _spServerFileName = "HELLION_SP.exe";

		private CharacterData _newCharacterData;

		public bool ExperimentalBuild;

		[Multiline(2)]
		public string ExperimentalText;

		public GameObject ExperimentalGameObject;

		public bool AllowLoadingOldSaveGames;

		[NonSerialized]
		public Network.Gender CurrentGender;

		public InputField CharacterInputField;

		public GameObject CreateCharacterPanel;

		public Text CurrentGenderText;

		public GameObject PasswordEnterPanel;

		public InputField PasswordInputField;

		public MainMenuSceneController MainMenuSceneController;

		public bool USE_PHYSICS_POSITION_FIX = true;

		public float RCS_THRUST_SENSITIVITY = 0.5f;

		public float RCS_ROTATION_SENSITIVITY = 5f;

		public float RCS_THRUST_TIME_MULTIPLIER = 2f;

		public float RCS_ROTATION_TIME_MULTIPLIER = 2f;

		public static double CELESTIAL_BODY_RADIUS_MULTIPLIER = 1.0;

		public static float DROP_THRESHOLD = 0.2f;

		public static float DROP_MIN_FORCE = 0f;

		public static float DROP_MAX_FORCE = 8f;

		public static float DROP_MAX_TIME = 3f;

		public static float VESSEL_ROTATION_LERP_VALUE = 0.9f;

		public static bool VESSEL_ROTATION_LERP_UNCLAMPED = false;

		public static float VESSEL_TRANSLATION_LERP_VALUE = 0.8f;

		public static bool VESSEL_TRANSLATION_LERP_UNCLAMPED = false;

		public static uint NetworkDataHash = ClassHasher.GetClassHashCode(typeof(NetworkData));

		public static uint SceneDataHash = ClassHasher.GetClassHashCode(typeof(ISceneData));

		public static uint CombinedHash = NetworkDataHash * SceneDataHash;

		public static float MouseSpeedOnPanels = 30f;

		public static SceneLoadTypeValue SceneLoadType = SceneLoadTypeValue.PreloadWithCopy;

		public static int ControlsVersion = 1;

		public static volatile bool IsRunning = false;

		public SceneLoader SceneLoader;

		public SolarSystem SolarSystem;

		public CanvasManager CanvasManager;

		public static SignInRequest LastSignInRequest = null;

		public static SignInResponse LastSignInResponse = null;

		[NonSerialized]
		public Dictionary<long, OtherPlayer> Players = new Dictionary<long, OtherPlayer>();

		[NonSerialized]
		public Dictionary<long, DynamicObject> DynamicObjects = new Dictionary<long, DynamicObject>();

		[NonSerialized]
		public Dictionary<long, Corpse> Corpses = new Dictionary<long, Corpse>();

		[NonSerialized]
		public Dictionary<long, SpaceObjectVessel> ActiveVessels = new Dictionary<long, SpaceObjectVessel>();

		public GameObject SolarSystemRoot;

		public GameObject ShipExteriorRoot;

		public Transform ShipExteriorRotation;

		public Transform SunCameraRootTransform;

		public Transform SunCameraTransform;

		public Transform PlanetsCameraRootTransform;

		public Transform PlanetsCameraTransform;

		public Transform ShipSunLightTransform;

		public Transform PlanetsRootTransform;

		public Transform PlanetsSunLightTransform;

		public RenderToCubeMap CubemapRenderer;

		public Map Map;

		public SoundEffect AmbientSounds;

		/// <summary>
		/// 	If we have loaded up a save or joined a server, and is in game.<br/>
		/// 	True when game is started.
		/// </summary>
		[ReadOnly]
		public bool IsInGame;

		[ReadOnly]
		public bool HasFocus;

		public bool InvertMouseWhileDriving;

		public bool InvertedMouse;

		public bool IsChatOpened;

		public volatile bool LogoutRequestSent;

		public volatile bool LogInResponseReceived;

		private bool _gameExitWanted;

		private float _maxSecondsToWaitForExit = 3f;

		public bool EnvironmentReady;

		private bool _receivedSignInResponse;

		private const int autosaveInterval = 600;

		[NonSerialized]
		public List<ItemIngredientsData> ItemsIngredients;

		[NonSerialized]
		public List<Quest> Quests = new List<Quest>();

		public QuestCollectionObject QuestCollection;

		[NonSerialized]
		public float LastMovementMessageTime;

		[NonSerialized]
		public int LoadingScenesCount;

		public IEnumerator<string> ShuffledTexts;

		public float LastLoadingTipsChangeTime;

		private float headbobStrength;

		private int antialiasingOption;

		private bool volumetricOption;

		public bool OffSpeedHelper = true;

		public static volatile int MainThreadID;

		private static int defaultLayerMaskValue = -1;

		public static float DefaultCameraFov = 75f;

		public static float SpecialCameraFov = 40f;

		private bool openMainSceneStarted;

		public GameObject MainMenuRoot;

		[NonSerialized]
		public Dictionary<long, CharacterInteractionState> CharacterInteractionStatesQueue = new Dictionary<long, CharacterInteractionState>();

		[ReadOnly]
		public bool SignInFailed;

		private int collisionLayerMask;

		public static volatile bool ForceRespawn = false;

		public static volatile bool IsLogout = false;

		public static volatile bool IsDisconected = false;

		public GameObject PreloadingScreen;

		public int CurrentLanguageIndex;

		private IEnumerator _processInvitationCoroutine;

		private IEnumerator _inviteCoroutine;

		public GameObject InviteScreen;

		public List<DebrisField> DebrisFields = new List<DebrisField>();

		public ExteriorParticles ExtParticles;

		public DebrisFieldEffect DebrisEffect;

		public EffectPrefabs EffectPrefabs;

		public SpriteManager SpriteManager;

		public InGameCanvasManager InGamePanels;

		[NonSerialized]
		public DateTime? ServerRestartTime;

		private static Process _spServerProcess = null;

		private float lastSPAutosaveTime;

		[NonSerialized]
		public bool SinglePlayerMode;

		public static bool SinglePlayerQuickLoad;

		public static bool SinglePlayerRespawn;

		public static SPGameMode SinglePlayerGameMode;

		[NonSerialized]
		public double ExposureRange;

		[NonSerialized]
		public float[] VesselExposureValues;

		[NonSerialized]
		public float[] PlayerExposureValues;

		private float _lastLatencyMessageTime = -1f;

		private int _latencyMs;

		[Title("Cursor")]
		public Texture2D DefaultCursor;

		public Texture2D HoverCursor;

		public static bool ReconnectAutomatically = false;

		public static string LastConnectedServerPass = null;

		public static ServerData LastConnectedServer;

		public static string InvitedToServerId;

		public static string InvitedToServerPassword = null;

		public static VesselObjectID InvitedToServerSpawnPointId = null;

		private IEnumerator _connectToServerCoroutine;

		private Task _loadingFinishedTask;

		private Task _restoreMapDetailsTask;

		private static Client _instance = null;
		public static Client Instance => _instance;

		public static bool IsGameBuild => Instance != null;

		public float HeadbobStrength
		{
			get
			{
				return headbobStrength;
			}
			set
			{
				headbobStrength = value;
				if (MyPlayer.Instance != null)
				{
					MyPlayer.Instance.FpsController.HeadBobStrength = headbobStrength;
				}
			}
		}

		public int AntialiasingOption
		{
			get
			{
				return antialiasingOption;
			}
			set
			{
				antialiasingOption = value;
				if (MyPlayer.Instance != null)
				{
					MyPlayer.Instance.InitializeCameraEffects();
				}
			}
		}

		public bool VolumetricOption
		{
			get
			{
				return volumetricOption;
			}
			set
			{
				volumetricOption = value;
				if (MyPlayer.Instance != null)
				{
					MyPlayer.Instance.InitializeCameraEffects();
				}
			}
		}

		public static int DefaultLayerMask
		{
			get
			{
				if (defaultLayerMaskValue == -1)
				{
					defaultLayerMaskValue = 1 << LayerMask.NameToLayer("Default");
				}
				return defaultLayerMaskValue;
			}
		}

		public int LatencyMs
		{
			get
			{
				if (_lastLatencyMessageTime < 0f)
				{
					return 0;
				}
				float num = Time.realtimeSinceStartup - _lastLatencyMessageTime;
				if (_latencyMs < 0 || num > 5f)
				{
					return (int)(num * 1000f);
				}
				return _latencyMs;
			}
		}


		private void Awake()
		{
			// Used to save input controls when we change the defaults.
			// TODO: Make this an editor feature.
			//InputController.SaveDefaultJSON();

			Texture[] source = Resources.LoadAll<Texture>("Emblems");
			SceneVesselEmblem.Textures = source.ToDictionary((Texture x) => x.name, (Texture y) => y);

			RCS_THRUST_SENSITIVITY = Properties.GetProperty("rcs_thrust_sensitivity", RCS_THRUST_SENSITIVITY);
			RCS_ROTATION_SENSITIVITY = Properties.GetProperty("rcs_rotation_sensitivity", RCS_ROTATION_SENSITIVITY);
			CELESTIAL_BODY_RADIUS_MULTIPLIER = Properties.GetProperty("celestial_body_radius_multiplier", CELESTIAL_BODY_RADIUS_MULTIPLIER);
			DROP_THRESHOLD = Properties.GetProperty("drop_threshold", DROP_THRESHOLD);
			DROP_MIN_FORCE = Properties.GetProperty("drop_min_force", DROP_MIN_FORCE);
			DROP_MAX_FORCE = Properties.GetProperty("drop_max_force", DROP_MAX_FORCE);
			DROP_MAX_TIME = Properties.GetProperty("drop_max_time", DROP_MAX_TIME);
			VESSEL_ROTATION_LERP_VALUE = Properties.GetProperty("vessel_rotation_lerp_value", VESSEL_ROTATION_LERP_VALUE);
			VESSEL_ROTATION_LERP_UNCLAMPED = Properties.GetProperty("vessel_rotation_lerp_unclamped", VESSEL_ROTATION_LERP_UNCLAMPED);
			VESSEL_TRANSLATION_LERP_VALUE = Properties.GetProperty("vessel_translation_lerp_value", VESSEL_TRANSLATION_LERP_VALUE);
			VESSEL_TRANSLATION_LERP_UNCLAMPED = Properties.GetProperty("vessel_translation_lerp_unclamped", VESSEL_TRANSLATION_LERP_UNCLAMPED);

			StaticData.LoadData();
			Application.runInBackground = true;
			MainThreadID = Thread.CurrentThread.ManagedThreadId;
			_instance = this;
			IsRunning = true;
			openMainSceneStarted = false;

			// Only load simple scenes if we have little available memory, regardless of settings.
			if (SystemInfo.systemMemorySize < 6000 || Application.isEditor)
			{
				SceneLoadType = SceneLoadTypeValue.Simple;
			}
			else
			{
				int property = Properties.GetProperty("load_type", (int)SceneLoadType);
				if (Enum.IsDefined(typeof(SceneLoadTypeValue), property))
				{
					SceneLoadType = (SceneLoadTypeValue)property;
				}
			}
		}

		private void Start()
		{
			ShuffledTexts = Localization.PreloadText.OrderBy((string m) => MathHelper.RandomNextDouble()).ToList().GetEnumerator();
			if (ExperimentalGameObject != null)
			{
				if (ExperimentalBuild)
				{
					ExperimentalGameObject.SetActive(value: true);
					ExperimentalGameObject.GetComponentInChildren<Text>().text = ExperimentalText.Trim() + " " + Application.version;
					ProviderManager.MainProvider.SetAchievement(AchievementID.other_testing_squad_member);
				}
				else
				{
					ExperimentalGameObject.SetActive(value: false);
				}
			}
			string property = Properties.GetProperty("custom_localization_file", string.Empty);
			if (Properties.GetProperty("save_default_localization_file", defaultValue: false))
			{
				string fileName = "localization_default.txt";
				Localization.SaveToFile(fileName);
			}

			if (Localization.LocalizationFiles.TryGetValue(CurrentLanguageIndex, out var value))
			{
				try
				{
					Localization.ImportFromString(Resources.Load<TextAsset>(value).text);
				}
				catch
				{
					Localization.RevertToDefault();
				}
			}
			else if (property != string.Empty)
			{
				Localization.ImportFromFile(property);
			}
			else
			{
				Localization.RevertToDefault();
			}
			InGamePanels.LocalizePanels();
			EventSystem.AddListener(typeof(LogInResponse), LogInResponseListener);

			collisionLayerMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("PlayerCollision"));
			EventSystem.AddListener(typeof(KillPlayerMessage), KillPlayerMessageListener);
			EventSystem.AddListener(typeof(LogOutResponse), LogOutResponseListener);
			EventSystem.AddListener(typeof(DestroyObjectMessage), DestroyObjectMessageListener);
			EventSystem.AddListener(typeof(PlayerSpawnResponse), PlayerSpawnResponseListener);
			EventSystem.AddListener(typeof(SpawnObjectsResponse), SpawnObjectsReponseListener);
			EventSystem.AddListener(typeof(MovementMessage), MovementMessageListener);
			EventSystem.AddListener(typeof(DynamicObjectsInfoMessage), DynamicObjectsInfoMessageListener);
			EventSystem.AddListener(typeof(PlayersOnServerResponse), PlayersOnServerResponseListener);
			EventSystem.AddListener(typeof(AvailableSpawnPointsResponse), AvailableSpawnPointsResponseListener);
			EventSystem.AddListener(typeof(ConsoleMessage), ConsoleMessageListener);
			EventSystem.AddListener(typeof(ShipCollisionMessage), ShipCollisionMessageListener);
			EventSystem.AddListener(typeof(UpdateVesselDataMessage), UpdateVesselDataMessageListener);
			EventSystem.AddListener(EventSystem.InternalEventType.ShowMessageBox, ShowMessageBoxListener);
			EventSystem.AddListener(EventSystem.InternalEventType.OpenMainScreen, OpenMainScreenListener);
			EventSystem.AddListener(EventSystem.InternalEventType.ReconnectAuto, ReconnectAutoListener);
			EventSystem.AddListener(EventSystem.InternalEventType.RemoveLoadingCanvas, RemoveLoadingCanvasListener);
			EventSystem.AddListener(EventSystem.InternalEventType.ConnectionFailed, ConnectionFailedListener);
			EventSystem.AddListener(EventSystem.InternalEventType.CloseAllLoadingScreens, CloseAllLoadingScreensListener);

			if (InvitedToServerId != null)
			{
				_inviteCoroutine = ConnectToInvite();
				StartCoroutine(_inviteCoroutine);
			}
			else
			{
				InvitedToServerPassword = null;
				InvitedToServerSpawnPointId = null;
				if (IsLogout || IsDisconected)
				{
					CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.ConnectingToMain);
					CanvasManager.SelectScreen(CanvasManager.Screen.CharacterSelect);
				}
				if (ReconnectAutomatically)
				{
					if (SinglePlayerRespawn)
					{
						SinglePlayerQuickLoad = false;
						PlayNewSPGame();
					}
					else
					{
						CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.ConnectingToGame);
						EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.ReconnectAuto));
					}
				}
			}
			if (Settings.Instance != null)
			{
				Settings.Instance.LoadSettings(Settings.SettingsType.Game);
			}
			if (SinglePlayerQuickLoad)
			{
				PlayNewSPGame();
			}
		}

		public string GetInviteString(VesselObjectID spawnPointId)
		{
			InviteMessage inviteMessage = new InviteMessage
			{
				Time = Time.time,
				ServerId = LastConnectedServer.Id,
				Password = LastConnectedServerPass,
				SpawnPointId = spawnPointId
			};
			return JsonSerialiser.Serialize(inviteMessage);
		}

		private IEnumerator LoadMainMenuScene()
		{
			yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			GameObject[] rootGameObjects = SceneManager.GetSceneByName("MainMenu").GetRootGameObjects();
			foreach (GameObject gameObject in rootGameObjects)
			{
				gameObject.transform.parent = MainMenuRoot.transform;
			}
		}

		private void RemoveLoadingCanvasListener(EventSystem.InternalEventData data)
		{
			CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.None);
		}

		private void DynamicObjectsInfoMessageListener(NetworkData data)
		{
			DynamicObjectsInfoMessage dynamicObjectsInfoMessage = data as DynamicObjectsInfoMessage;
			foreach (DynamicObjectInfo info in dynamicObjectsInfoMessage.Infos)
			{
				DynamicObject dynamicObject = GetObject(info.GUID, SpaceObjectType.DynamicObject) as DynamicObject;
				if (dynamicObject != null && dynamicObject.Item != null)
				{
					dynamicObject.Item.ProcesStatsData(info.Stats);
				}
			}
		}

		private void SpawnObjectsReponseListener(NetworkData data)
		{
			SpawnObjectsResponse spawnObjectsResponse = data as SpawnObjectsResponse;
			foreach (SpawnObjectResponseData datum in spawnObjectsResponse.Data)
			{
				try
				{
					if (datum.Type == SpaceObjectType.Ship || datum.Type == SpaceObjectType.Asteroid)
					{
						SpaceObjectVessel spaceObjectVessel = GetObject(datum.GUID, datum.Type) as SpaceObjectVessel;
						spaceObjectVessel.ParseSpawnData(datum);
						if (spaceObjectVessel.IsMainVessel && spaceObjectVessel.Orbit.Parent != null)
						{
							Map.InitializeMapObject(spaceObjectVessel);
						}
					}
					else if (datum.Type == SpaceObjectType.DynamicObject && GetDynamicObject(datum.GUID) == null)
					{
						DynamicObject.SpawnDynamicObject(datum);
					}
					else if (datum.Type == SpaceObjectType.Corpse && GetCorpse(datum.GUID) == null)
					{
						Corpse.SpawnCorpse(datum);
					}
					else if (datum.Type == SpaceObjectType.Player && GetPlayer(datum.GUID) == null)
					{
						OtherPlayer.SpawnPlayer(datum);
					}
				}
				catch (Exception ex)
				{
					Dbg.Error(ex.Message, ex.StackTrace);
				}
			}
			if (_restoreMapDetailsTask != null)
			{
				_restoreMapDetailsTask.RunSynchronously();
				_restoreMapDetailsTask = null;
			}
		}

		private void ShipCollisionMessageListener(NetworkData data)
		{
			ShipCollisionMessage shipCollisionMessage = data as ShipCollisionMessage;
			if ((!(GetVessel(shipCollisionMessage.ShipOne) == MyPlayer.Instance.Parent) && (shipCollisionMessage.ShipTwo == -1 || !(GetVessel(shipCollisionMessage.ShipTwo) == MyPlayer.Instance.Parent))) || !(shipCollisionMessage.CollisionVelocity > float.Epsilon))
			{
				return;
			}
			MyPlayer.Instance.FpsController.CameraController.cameraShakeController.CamShake(0.8f, 0.3f, 15f, 15f, useSparks: true);
			VesselHealthSounds[] componentsInChildren = GetVessel(shipCollisionMessage.ShipOne).GeometryRoot.GetComponentsInChildren<VesselHealthSounds>();
			foreach (VesselHealthSounds vesselHealthSounds in componentsInChildren)
			{
				vesselHealthSounds.PlaySounds();
			}
			if (shipCollisionMessage.ShipTwo != -1)
			{
				VesselHealthSounds[] componentsInChildren2 = GetVessel(shipCollisionMessage.ShipTwo).GeometryRoot.GetComponentsInChildren<VesselHealthSounds>();
				foreach (VesselHealthSounds vesselHealthSounds2 in componentsInChildren2)
				{
					vesselHealthSounds2.PlaySounds();
				}
			}
		}

		private void PlayerSpawnResponseListener(NetworkData data)
		{
			PlayerSpawnResponse s = data as PlayerSpawnResponse;
			if (s.Response == ResponseResult.Success)
			{
				CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.Loading);
				SolarSystemRoot.SetActive(value: true);
				MainMenuRoot.SetActive(value: false);
				if (s.HomeGUID.HasValue)
				{
					MyPlayer.Instance.HomeStationGUID = s.HomeGUID.Value;
				}
				SceneLoader.LoadScenesWithIDs(s.Scenes);
				if (s.ParentType == SpaceObjectType.Ship)
				{
					Ship ship = Ship.Create(s.MainVesselID, s.VesselData, s.ParentTransform, isMainObject: true);
					ship.gameObject.SetActive(value: true);
					if (s.DockedVessels != null && s.DockedVessels.Count > 0)
					{
						foreach (DockedVesselData dockedVessel in s.DockedVessels)
						{
							Ship ship2 = Ship.Create(dockedVessel.GUID, dockedVessel.Data, s.ParentTransform, isMainObject: true);
							ship2.gameObject.SetActive(value: true);
							ship2.DockedToMainVessel = ship;
						}
					}
					MyPlayer.Instance.Parent = GetVessel(s.ParentID);
					Dbg.Info("Starting main scene load, Ship");
					StartCoroutine(LoadMainScenesCoroutine(s, ship, s.VesselObjects));
				}
				else if (s.ParentType == SpaceObjectType.Asteroid)
				{
					Asteroid asteroid = Asteroid.Create(s.ParentTransform, s.VesselData, isMainObject: true);
					asteroid.gameObject.SetActive(value: true);
					MyPlayer.Instance.Parent = asteroid;
					Dbg.Info("Starting main scene load, Asteroid");
					StartCoroutine(LoadMainScenesCoroutine(s, asteroid));
				}
				else if (s.ParentType == SpaceObjectType.PlayerPivot)
				{
					Pivot parent = Pivot.Create(SpaceObjectType.PlayerPivot, s.ParentTransform, isMainObject: true);
					MyPlayer.Instance.Parent = parent;
					_loadingFinishedTask = new Task(delegate
					{
						OnLoadingComplete(s);
					});
				}
				else
				{
					Dbg.Error("Unknown player parent", s.ParentType, s.ParentID);
					ShowMessageBox(Localization.SpawnErrorTitle, Localization.SpawnErrorMessage);
					CanvasManager.CanChooseSpawn = true;
				}
				if (s.TimeUntilServerRestart.HasValue)
				{
					ServerRestartTime = DateTime.UtcNow.AddSeconds(s.TimeUntilServerRestart.Value);
				}
				else
				{
					ServerRestartTime = null;
				}
				if (s.Quests != null)
				{
					foreach (QuestDetails quest in s.Quests)
					{
						MyPlayer.Instance.SetQuestDetails(quest, showNotifications: false, playCutScenes: false);
					}
				}
				if (s.Blueprints != null)
				{
					MyPlayer.Instance.Blueprints = s.Blueprints;
				}
				if (s.NavMapDetails == null)
				{
					return;
				}
				_restoreMapDetailsTask = new Task(delegate
				{
					foreach (UnknownMapObjectDetails det in s.NavMapDetails.Unknown)
					{
						SpaceObjectVessel spaceObjectVessel = null;
						if (det.SpawnRuleID != 0)
						{
							spaceObjectVessel = SolarSystem.ArtificialBodies.FirstOrDefault((ArtificialBody m) => m is SpaceObjectVessel && (m as SpaceObjectVessel).IsMainVessel && (m as SpaceObjectVessel).VesselData != null && (m as SpaceObjectVessel).VesselData.SpawnRuleID == det.SpawnRuleID) as SpaceObjectVessel;
						}
						if (spaceObjectVessel == null)
						{
							spaceObjectVessel = GetVessel(det.GUID);
						}
						if (spaceObjectVessel != null)
						{
							spaceObjectVessel.RadarVisibilityType = RadarVisibilityType.Unknown;
							spaceObjectVessel.LastKnownMapOrbit = new OrbitParameters();
							spaceObjectVessel.LastKnownMapOrbit.ParseNetworkData(det.LastKnownOrbit);
							if (spaceObjectVessel.VesselData != null && spaceObjectVessel.VesselData.SpawnRuleID != 0)
							{
								Map.UnknownVisibilityOrbits[spaceObjectVessel.VesselData.SpawnRuleID] = spaceObjectVessel.LastKnownMapOrbit;
							}
						}
					}
					SolarSystem.ArtificialBodiesVisiblityModified();
				});
			}
			else
			{
				ShowMessageBox(Localization.SpawnErrorTitle, Localization.SpawnErrorMessage);
				CanvasManager.CanChooseSpawn = true;
			}
		}

		private IEnumerator LoadMainScenesCoroutine(PlayerSpawnResponse s, Asteroid ast)
		{
			yield return StartCoroutine(ast.LoadScenesCoroutine(s.MiningPoints));
			_loadingFinishedTask = new Task(delegate
			{
				OnLoadingComplete(s);
			});
		}

		private IEnumerator LoadMainScenesCoroutine(PlayerSpawnResponse s, Ship sh, VesselObjects shipObjects)
		{
			if (sh != null)
			{
				yield return StartCoroutine(sh.LoadShipScenesCoroutine(isMainShip: true, shipObjects));
				if (s.DockedVessels != null && s.DockedVessels.Count > 0)
				{
					foreach (DockedVesselData dockVess in s.DockedVessels)
					{
						Ship childShip = Instance.GetVessel(dockVess.GUID) as Ship;
						yield return StartCoroutine(childShip.LoadShipScenesCoroutine(isMainShip: true, dockVess.VesselObjects));
						childShip.transform.parent = sh.ConnectedObjectsRoot.transform;
					}
				}
			}
			if (shipObjects.DockingPorts != null)
			{
				SceneDockingPort[] componentsInChildren = sh.GeometryRoot.GetComponentsInChildren<SceneDockingPort>(includeInactive: true);
				foreach (SceneDockingPort dp in componentsInChildren)
				{
					SceneDockingPortDetails sceneDockingPortDetails = shipObjects.DockingPorts.Find((SceneDockingPortDetails m) => m.ID.InSceneID == dp.InSceneID);
					if (sceneDockingPortDetails != null)
					{
						dp.SetDetails(sceneDockingPortDetails, isInitialize: true);
					}
				}
			}
			if (s.DockedVessels != null && s.DockedVessels.Count > 0)
			{
				foreach (DockedVesselData dockedVessel in s.DockedVessels)
				{
					if (dockedVessel.VesselObjects.DockingPorts == null)
					{
						continue;
					}
					Ship ship = Instance.GetVessel(dockedVessel.GUID) as Ship;
					SceneDockingPort[] componentsInChildren2 = ship.GeometryRoot.GetComponentsInChildren<SceneDockingPort>(includeInactive: true);
					foreach (SceneDockingPort dport in componentsInChildren2)
					{
						SceneDockingPortDetails sceneDockingPortDetails2 = dockedVessel.VesselObjects.DockingPorts.Find((SceneDockingPortDetails m) => m.ID.InSceneID == dport.InSceneID);
						if (sceneDockingPortDetails2 != null)
						{
							dport.SetDetails(sceneDockingPortDetails2, isInitialize: true);
						}
					}
				}
			}
			DynamicObject[] componentsInChildren3 = sh.TransferableObjectsRoot.GetComponentsInChildren<DynamicObject>();
			foreach (DynamicObject dynamicObject in componentsInChildren3)
			{
				dynamicObject.ToggleEnabled(isEnabled: true, toggleColliders: true);
				dynamicObject.CheckRoomTrigger(null);
			}
			Corpse[] componentsInChildren4 = sh.TransferableObjectsRoot.GetComponentsInChildren<Corpse>();
			foreach (Corpse corpse in componentsInChildren4)
			{
				corpse.CheckRoomTrigger(null);
			}
			_loadingFinishedTask = new Task(delegate
			{
				OnLoadingComplete(s);
			});
		}

		private void OnLoadingComplete(PlayerSpawnResponse s)
		{
			_loadingFinishedTask = null;
			MyPlayer.Instance.ActivatePlayer(s);
		}

		public void OpenMainScreenListener(EventSystem.InternalEventData data)
		{
			if (!CanvasManager.DeadScreen.activeInHierarchy)
			{
				if (!LogoutRequestSent && !SinglePlayerQuickLoad)
				{
					CanvasManager.TogglePlayerDisconect(val: true);
				}
				else
				{
					OpenMainScreen();
				}
			}
		}

		/// <summary>
		/// 	Listen for connection drops and attempt to reconnect.
		/// </summary>
		public void ReconnectAutoListener(EventSystem.InternalEventData data)
		{
			try
			{
				Reconnect();
			}
			catch
			{
				ReconnectAutomatically = false;
				OpenMainScreen();
			}
		}

		private void ShowMessageBoxListener(EventSystem.InternalEventData data)
		{
			ShowMessageBox((string)data.Objects[0], (string)data.Objects[1]);
		}

		private void SendLogoutRequest()
		{
			if (!LogoutRequestSent)
			{
				LogoutRequestSent = true;
				NetworkController.Instance.SendToGameServer(new LogOutRequest());
			}
		}

		public void ShowSpawnPointSelection(List<SpawnPointDetails> spawnPoints, bool canContinue)
		{
			CanvasManager.SelectScreen(CanvasManager.Screen.StartingPoint);
			if (spawnPoints == null)
			{
				spawnPoints = new List<SpawnPointDetails>();
			}
			if (SinglePlayerMode && SinglePlayerGameMode == SPGameMode.Sandbox)
			{
				spawnPoints.Add(new SpawnPointDetails
				{
					SpawnSetupType = SpawnSetupType.FreeRoamSteropes,
					IsPartOfCrew = false,
					PlayersOnShip = new List<string>()
				});
				spawnPoints.Add(new SpawnPointDetails
				{
					SpawnSetupType = SpawnSetupType.MiningSteropes,
					IsPartOfCrew = false,
					PlayersOnShip = new List<string>()
				});
				spawnPoints.Add(new SpawnPointDetails
				{
					SpawnSetupType = SpawnSetupType.SteropesNearRandomStation,
					IsPartOfCrew = false,
					PlayersOnShip = new List<string>()
				});
				spawnPoints.Add(new SpawnPointDetails
				{
					SpawnSetupType = SpawnSetupType.SteropesNearDoomedOutpost,
					IsPartOfCrew = false,
					PlayersOnShip = new List<string>()
				});
				spawnPoints.Add(new SpawnPointDetails
				{
					SpawnSetupType = SpawnSetupType.FreeRoamArges,
					IsPartOfCrew = false,
					PlayersOnShip = new List<string>()
				});
				spawnPoints.Add(new SpawnPointDetails
				{
					SpawnSetupType = SpawnSetupType.MiningArges,
					IsPartOfCrew = false,
					PlayersOnShip = new List<string>()
				});
			}
			CanvasManager.ShowSpawnPoints(spawnPoints, SendSpawnRequest, canContinue);
		}

		public void SendSpawnRequest(SpawnPointDetails details)
		{
			if (CanvasManager.CanChooseSpawn)
			{
				CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.Loading);
				CanvasManager.CanChooseSpawn = false;
				PlayerSpawnRequest playerSpawnRequest = new PlayerSpawnRequest
				{
					SpawnSetupType = details.SpawnSetupType,
					SpawPointParentID = details.SpawnPointParentID
				};
				NetworkController.Instance.SendToGameServer(playerSpawnRequest);
			}
		}

		private void LogOutResponseListener(NetworkData data)
		{
			LogOutResponse logOutResponse = data as LogOutResponse;
			if (logOutResponse.Response == ResponseResult.Error)
			{
				Dbg.Error("Failed to log out properly");
			}

			NetworkController.Instance.Disconnect();
			if (SinglePlayerMode)
			{
				KillAllSPProcesses();
			}
			if (_gameExitWanted)
			{
				QuitApplication();
			}
			else
			{
				OpenMainScreen();
			}
		}

		private void DestroyObjectMessageListener(NetworkData data)
		{
			DestroyObjectMessage destroyObjectMessage = data as DestroyObjectMessage;
			SpaceObject obj = GetObject(destroyObjectMessage.ID, destroyObjectMessage.ObjectType);
			if (obj != null && obj.Type != SpaceObjectType.PlayerPivot && obj.Type != SpaceObjectType.DynamicObjectPivot && obj.Type != SpaceObjectType.CorpsePivot)
			{
				obj.DestroyGeometry();
				if (obj is DynamicObject && (obj as DynamicObject).Item != null && (obj as DynamicObject).Item.AttachPoint != null)
				{
					(obj as DynamicObject).Item.AttachPoint.DetachItem((obj as DynamicObject).Item);
				}
				if (MyPlayer.Instance != null && MyPlayer.Instance.CurrentActiveItem != null && MyPlayer.Instance.CurrentActiveItem.GUID == obj.GUID)
				{
					MyPlayer.Instance.Inventory.RemoveItemFromHands(resetStance: true);
				}
				UnityEngine.Object.Destroy(obj.gameObject);
			}
		}

		public void LogOut()
		{
			if (IsInGame)
			{
				CanvasManager.InGameMenuCanvas.SetActive(value: false);
				CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.Loading);
				IsLogout = true;
				SendLogoutRequest();
			}
			else if (SinglePlayerMode)
			{
				OpenMainScreen();
			}
			else
			{
				CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.Loading);
				OpenMainScreen();
			}
		}

		/// <summary>
		/// 	Minor reset of game state. Reloads scene and opens main menu.
		/// </summary>
		public void OpenMainScreen()
		{
			if (!openMainSceneStarted)
			{
				openMainSceneStarted = true;
				InGamePanels.Detach();
				ToggleCursor(true);
				if (MyPlayer.Instance != null)
				{
					Destroy(MyPlayer.Instance.gameObject);
				}
				SceneManager.LoadScene("Client", LoadSceneMode.Single);
				if (MainMenuSceneController != null)
				{
					MainMenuSceneController.gameObject.SetActive(value: true);
				}
			}
		}

		public void ConnectionFailedListener(EventSystem.InternalEventData data)
		{
			CanvasManager.SelectScreen(CanvasManager.Screen.CharacterSelect);
			ReconnectAutomatically = false;
			SignInResponseListener(LastSignInResponse);
		}

		public void CloseAllLoadingScreensListener(EventSystem.InternalEventData data)
		{
			if (CanvasManager.StartingPointScreen.activeInHierarchy)
			{
				CanvasManager.ExitSpawnOptionsScreen();
			}
			else
			{
				CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.None);
			}
		}

		public IEnumerator ConnectToInvite()
		{
			CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.ConnectingToGame);
			_receivedSignInResponse = false;
			InviteScreen.SetActive(value: true);

			SignIn();
			CanvasManager.Disclamer.SetActive(value: false);

			yield return new WaitUntil(() => _receivedSignInResponse);

			InvitedToServerId = null;
			if (LastConnectedServer == null)
			{
				StopInviteCoroutine();
				ShowMessageBox(Localization.ConnectionError, Localization.ConnectionToGameBroken);
				yield break;
			}

			ConnectToServer(LastConnectedServer, InvitedToServerPassword);
			InviteScreen.SetActive(value: false);
			_inviteCoroutine = null;
		}

		public void StopInviteCoroutine()
		{
			if (_inviteCoroutine != null)
			{
				StopCoroutine(_inviteCoroutine);
				_inviteCoroutine = null;
			}
			InviteScreen.SetActive(value: false);
		}

		private void OnApplicationQuit()
		{
			// Kill the single player server.
			if (SinglePlayerMode)
			{
				KillAllSPProcesses();
			}

			// If we didn't tell the game explicitly that we want to exit.
			if (!_gameExitWanted)
			{
				// Prevent the app from quitting...
				Application.wantsToQuit += () => { return false; };

				// ...and exit safely instead.
				ExitGame();
			}
			else
			{
				NetworkController.Instance.Disconnect();
			}
		}

		/// <summary>
		/// 	Method to exit the game safely. Handles shutting down connections to servers/clients properly, and saves settings.
		/// </summary>
		public void ExitGame()
		{
			_gameExitWanted = true;
			QuitApplication();
		}

		private void QuitApplication()
		{
			if (IsInGame) LogOut();
			IsRunning = false;
			OnDestroy();
			NetworkController.Instance.Disconnect();
#if UNITY_EDITOR
			EditorApplication.ExitPlaymode();
#else
			Application.Quit();
#endif
		}

		private void OnDestroy()
		{
			EventSystem.RemoveListener(typeof(KillPlayerMessage), KillPlayerMessageListener);
			EventSystem.RemoveListener(typeof(LogOutResponse), LogOutResponseListener);
			EventSystem.RemoveListener(typeof(DestroyObjectMessage), DestroyObjectMessageListener);
			EventSystem.RemoveListener(typeof(PlayerSpawnResponse), PlayerSpawnResponseListener);
			EventSystem.RemoveListener(typeof(MovementMessage), MovementMessageListener);
			EventSystem.RemoveListener(typeof(PlayersOnServerResponse), PlayersOnServerResponseListener);
			EventSystem.RemoveListener(typeof(AvailableSpawnPointsResponse), AvailableSpawnPointsResponseListener);
			EventSystem.RemoveListener(typeof(ShipCollisionMessage), ShipCollisionMessageListener);
			EventSystem.RemoveListener(typeof(UpdateVesselDataMessage), UpdateVesselDataMessageListener);
			EventSystem.RemoveListener(EventSystem.InternalEventType.ShowMessageBox, ShowMessageBoxListener);
			EventSystem.RemoveListener(EventSystem.InternalEventType.OpenMainScreen, OpenMainScreenListener);
			EventSystem.RemoveListener(EventSystem.InternalEventType.ReconnectAuto, ReconnectAutoListener);
			EventSystem.RemoveListener(EventSystem.InternalEventType.RemoveLoadingCanvas, RemoveLoadingCanvasListener);
			EventSystem.RemoveListener(EventSystem.InternalEventType.ConnectionFailed, ConnectionFailedListener);
			EventSystem.RemoveListener(typeof(LogInResponse), LogInResponseListener);
			Localization.RevertToDefault();
		}

		public void OnInGameMenuClosed()
		{
			if (IsInGame)
			{
				ToggleCursor(false);
				MyPlayer.Instance.FpsController.ToggleCameraController(true);
				MyPlayer.Instance.FpsController.ToggleCameraMovement(true);
				MyPlayer.Instance.FpsController.ToggleMovement(true);
			}
		}

		/// <summary>
		/// 	Toggles the visibility and lock state of the cursor. No value inverts the current value.
		/// </summary>
		public void ToggleCursor(bool? val = null)
		{
			Cursor.visible = (!val.HasValue) ? (!Cursor.visible) : val.Value;
			Cursor.lockState = CursorLockMode.None;
			if (!Cursor.visible)
			{
				Cursor.lockState = CursorLockMode.Locked;
			}
		}

		public void OnApplicationFocus(bool focusStatus)
		{
			HasFocus = focusStatus;
			if (MyPlayer.Instance != null)
			{
				if (MyPlayer.Instance.InLockState && !MyPlayer.Instance.IsLockedToTrigger && MyPlayer.Instance.Parent is SpaceObjectVessel && (MyPlayer.Instance.Parent as SpaceObjectVessel).SpawnPoints.Values.FirstOrDefault((SceneSpawnPoint m) => m.PlayerGUID == MyPlayer.Instance.GUID) != null)
				{
					MyPlayer.Instance.FpsController.CameraController.ToggleFreeLook(isActive: true);
				}
				MyPlayer.Instance.HideHiglightedAttachPoints();
			}
		}

		public void ShowMessageBox(string title, string text)
		{
			CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.None);
			CanvasManager.ToggleCanvasMessageBox(val: true);
			CanvasManager.GetMessageBox().PopulateMessageBox(title, text, destroyOnClose: false);
		}

		public static void ShowMessageBox(string title, string text, GameObject parent, MessageBox.OnCloseDelegate onClose)
		{
			GameObject gameObject = Instantiate(Resources.Load("UI/CanvasMessageBox")) as GameObject;
			gameObject.transform.SetParent(parent.transform, false);
			gameObject.GetComponent<RectTransform>().Reset(resetScale: true);
			gameObject.SetActive(value: true);
			gameObject.GetComponent<MessageBox>().OnClose = onClose;
			gameObject.GetComponent<MessageBox>().PopulateMessageBox(title, text, destroyOnClose: true);
		}

		public void ShowConfirmMessageBox(string title, string message, string yesText, string noText, ConfirmMessageBox.OnYesDelegate onYes, ConfirmMessageBox.OnNoDelegate onNo = null)
		{
			CanvasManager.ConfirmMessageBox.SetActive(value: true);
			if (onYes != null)
			{
				CanvasManager.ConfirmMessageBox.GetComponent<ConfirmMessageBox>().OnYes = onYes;
			}
			if (onNo != null)
			{
				CanvasManager.ConfirmMessageBox.GetComponent<ConfirmMessageBox>().OnNo = onNo;
			}
			CanvasManager.ConfirmMessageBox.GetComponent<ConfirmMessageBox>().PopulateConfirmMessageBox(title, message, yesText, noText);
		}

		private void Update()
		{
			if (Time.deltaTime > 1f)
			{
			}
			if (_gameExitWanted)
			{
				_maxSecondsToWaitForExit -= Time.deltaTime;
				if (_maxSecondsToWaitForExit <= 0f)
				{
					QuitApplication();
				}
			}
			if (SignInFailed)
			{
				SignInFailed = false;
				CanvasManager.SelectScreen(CanvasManager.Screen.MainMenu);
				ShowMessageBox(Localization.ConnectionError, Localization.ServerUnreachable);
			}
			if (MyPlayer.Instance != null && MyPlayer.Instance.IsAlive && !CanvasManager.InGameMenuCanvas.activeInHierarchy && autosaveInterval > 0 && SinglePlayerMode && lastSPAutosaveTime + (float)autosaveInterval < Time.time)
			{
				AutoSave();
			}
		}

		private void KillPlayerMessageListener(NetworkData data)
		{
			KillPlayerMessage killPlayerMessage = data as KillPlayerMessage;
			if (killPlayerMessage.GUID != MyPlayer.Instance.GUID)
			{
				return;
			}
			MyPlayer.Instance.IsAlive = false;
			if (ForceRespawn)
			{
				ReconnectAutomatically = true;
				LogoutRequestSent = true;
				return;
			}
			CanvasManager.ToggleDeadMsg(val: true);
			if (killPlayerMessage.CauseOfDeath == HurtType.Shipwreck && killPlayerMessage.VesselDamageType != 0)
			{
				CanvasManager.DeadMsgText.text = killPlayerMessage.VesselDamageType.ToLocalizedString().ToUpper();
			}
			else
			{
				CanvasManager.DeadMsgText.text = killPlayerMessage.CauseOfDeath.ToLocalizedString().ToUpper();
			}
		}

		public void AddPlayer(long guid, OtherPlayer pl)
		{
			Players[guid] = pl;
			RefreshMovementIntervals();
		}

		private void RefreshMovementIntervals()
		{
			MyPlayer.SendMovementInterval = ((Players.Count <= 0) ? 1f : 0.1f);
			DynamicObject.SendMovementInterval = ((Players.Count <= 0) ? 1f : 0.1f);
			Corpse.SendMovementInterval = ((Players.Count <= 0) ? 1f : 0.1f);
		}

		public void RemovePlayer(long guid)
		{
			Players.Remove(guid);
			RefreshMovementIntervals();
		}

		public OtherPlayer GetPlayer(long guid)
		{
			if (Players.ContainsKey(guid))
			{
				return Players[guid];
			}
			if (MyPlayer.Instance != null && MyPlayer.Instance.Parent != null && MyPlayer.Instance.Parent is SpaceObjectVessel)
			{
				OtherPlayer[] componentsInChildren = (MyPlayer.Instance.Parent as SpaceObjectVessel).MainVessel.GetComponentsInChildren<OtherPlayer>();
				foreach (OtherPlayer otherPlayer in componentsInChildren)
				{
					if (otherPlayer.GUID == guid)
					{
						AddPlayer(otherPlayer.GUID, otherPlayer);
						return otherPlayer;
					}
				}
			}
			OtherPlayer[] componentsInChildren2 = Instance.ShipExteriorRoot.GetComponentsInChildren<OtherPlayer>();
			foreach (OtherPlayer otherPlayer2 in componentsInChildren2)
			{
				if (otherPlayer2.GUID == guid)
				{
					AddPlayer(otherPlayer2.GUID, otherPlayer2);
					return otherPlayer2;
				}
			}
			return null;
		}

		public void AddDynamicObject(long guid, DynamicObject obj)
		{
			DynamicObjects[guid] = obj;
		}

		public void RemoveDynamicObject(long guid)
		{
			DynamicObjects.Remove(guid);
		}

		public DynamicObject GetDynamicObject(long guid)
		{
			if (DynamicObjects.TryGetValue(guid, out var value))
			{
				return value;
			}
			if (MyPlayer.Instance != null && MyPlayer.Instance.Parent != null)
			{
				DynamicObject[] componentsInChildren = MyPlayer.Instance.Parent.GetComponentsInChildren<DynamicObject>();
				foreach (DynamicObject dynamicObject in componentsInChildren)
				{
					if (dynamicObject.GUID == guid)
					{
						AddDynamicObject(dynamicObject.GUID, dynamicObject);
						return dynamicObject;
					}
				}
			}
			DynamicObject[] componentsInChildren2 = Instance.ShipExteriorRoot.GetComponentsInChildren<DynamicObject>();
			foreach (DynamicObject dynamicObject2 in componentsInChildren2)
			{
				if (dynamicObject2.GUID == guid)
				{
					AddDynamicObject(dynamicObject2.GUID, dynamicObject2);
					return dynamicObject2;
				}
			}
			return null;
		}

		public void AddCorpse(long guid, Corpse obj)
		{
			Corpses[guid] = obj;
		}

		public void RemoveCorpse(long guid)
		{
			Corpses.Remove(guid);
		}

		public Corpse GetCorpse(long guid)
		{
			if (Corpses.ContainsKey(guid))
			{
				return Corpses[guid];
			}
			if (MyPlayer.Instance != null && MyPlayer.Instance.Parent != null)
			{
				Corpse[] componentsInChildren = MyPlayer.Instance.Parent.GetComponentsInChildren<Corpse>();
				foreach (Corpse corpse in componentsInChildren)
				{
					if (corpse.GUID == guid)
					{
						AddCorpse(corpse.GUID, corpse);
						return corpse;
					}
				}
			}
			Corpse[] componentsInChildren2 = ShipExteriorRoot.GetComponentsInChildren<Corpse>();
			foreach (Corpse corpse2 in componentsInChildren2)
			{
				if (corpse2.GUID == guid)
				{
					AddCorpse(corpse2.GUID, corpse2);
					return corpse2;
				}
			}
			return null;
		}

		public SpaceObjectVessel GetVessel(long guid)
		{
			if (ActiveVessels.TryGetValue(guid, out var value))
			{
				return value;
			}
			value = SolarSystem.ArtificialBodies.FirstOrDefault((ArtificialBody m) => m.GUID == guid) as SpaceObjectVessel;
			if (value != null)
			{
				return value;
			}
			SpaceObjectVessel[] componentsInChildren = ShipExteriorRoot.GetComponentsInChildren<SpaceObjectVessel>();
			foreach (SpaceObjectVessel spaceObjectVessel in componentsInChildren)
			{
				if (spaceObjectVessel.GUID == guid)
				{
					return spaceObjectVessel;
				}
			}
			return null;
		}

		public void SendVesselRequest(SpaceObjectVessel obj, float time, GameScenes.SceneID sceneID, string tag)
		{
			NetworkController.Instance.SendToGameServer(new VesselRequest
			{
				GUID = obj.GUID,
				Time = time,
				RescueShipSceneID = sceneID,
				RescueShipTag = tag
			});
		}

		public void SendDistressCall(ArtificialBody obj, bool isDistressActive)
		{
			NetworkController.Instance.SendToGameServer(new DistressCallRequest
			{
				GUID = obj.GUID,
				IsDistressActive = isDistressActive
			});
		}

		private void PlayersOnServerResponseListener(NetworkData data)
		{
			try
			{
				PlayersOnServerResponse playersOnServerResponse = data as PlayersOnServerResponse;
				if (playersOnServerResponse.SpawnPointID != null)
				{
					GetVessel(playersOnServerResponse.SpawnPointID.VesselGUID).GetStructureObject<SceneSpawnPoint>(playersOnServerResponse.SpawnPointID.InSceneID).ParsePlayersOnServerResponse(playersOnServerResponse);
				}
				else if (playersOnServerResponse.SecuritySystemID != null)
				{
					(GetVessel(playersOnServerResponse.SecuritySystemID.VesselGUID) as Ship).SecuritySystem.ParsePlayersOnServerResponse(playersOnServerResponse);
				}
			}
			catch (Exception ex)
			{
				Dbg.Error("PlayersOnServerResponseListener", ex.Message, ex.StackTrace);
			}
		}

		public void SendAvailableSpawnPointsRequest()
		{
			NetworkController.Instance.SendToGameServer(new AvailableSpawnPointsRequest());
		}

		private void AvailableSpawnPointsResponseListener(NetworkData data)
		{
			try
			{
				AvailableSpawnPointsResponse availableSpawnPointsResponse = data as AvailableSpawnPointsResponse;
				if (availableSpawnPointsResponse.SpawnPoints != null)
				{
					CanvasManager.ShowSpawnPoints(availableSpawnPointsResponse.SpawnPoints, SendSpawnRequest, canContinue: false);
				}
			}
			catch (Exception ex)
			{
				Dbg.Error("AvailableSpawnPointsResponseListener", ex.Message, ex.StackTrace);
			}
		}

		private void ConsoleMessageListener(NetworkData data)
		{
			try
			{
				ConsoleMessage consoleMessage = data as ConsoleMessage;
				CanvasManager.Console.CreateTextElement(consoleMessage.Text, Colors.Orange);
				if (consoleMessage.Text == "God mode: ON")
				{
					CanvasManager.Console.GodMode.isOn = true;
				}
				else if (consoleMessage.Text == "God mode: OFF")
				{
					CanvasManager.Console.GodMode.isOn = false;
				}
			}
			catch (Exception ex)
			{
				Dbg.Error("ConsoleMessageListener", ex.Message, ex.StackTrace);
			}
		}

		private void LateUpdate()
		{
			if (MyPlayer.Instance != null && MyPlayer.Instance.PlayerReady && EnvironmentReady)
			{
				SolarSystem.UpdatePositions(Time.deltaTime);
				SolarSystem.CenterPlanets();
			}
		}

		private void MovementMessageListener(NetworkData data)
		{
			if (MyPlayer.Instance == null)
			{
				return;
			}
			SpaceObjectVessel spaceObjectVessel = null;
			float num = 0f;
			MovementMessage movementMessage = data as MovementMessage;
			LastMovementMessageTime = Time.realtimeSinceStartup;
			List<ArtificialBody> list = new List<ArtificialBody>(SolarSystem.ArtificialBodies);
			if (movementMessage.Transforms != null && movementMessage.Transforms.Count > 0)
			{
				foreach (ObjectTransform transform in movementMessage.Transforms)
				{
					bool flag = false;
					ArtificialBody artificialBody = SolarSystem.GetArtificialBody(transform.GUID);
					if (artificialBody != null)
					{
						list.Remove(artificialBody);
					}
					if (artificialBody == null && transform.GUID == MyPlayer.Instance.GUID)
					{
						continue;
					}
					if (artificialBody == null && (transform.Orbit != null || transform.Realtime != null || (transform.StabilizeToTargetGUID.HasValue && transform.StabilizeToTargetGUID.Value > 0)))
					{
						artificialBody = ArtificialBody.CreateArtificialBody(transform);
						flag = true;
					}
					if (!(artificialBody == null) || transform.Type == SpaceObjectType.DynamicObject || transform.Type != SpaceObjectType.PlayerPivot)
					{
					}
					if (!(artificialBody != null))
					{
						continue;
					}
					if (transform.StabilizeToTargetGUID.HasValue && transform.StabilizeToTargetGUID.Value > 0)
					{
						artificialBody.StabilizeToTarget(transform.StabilizeToTargetGUID.Value, transform.StabilizeToTargetRelPosition.ToVector3D());
					}
					else if (transform.Orbit != null)
					{
						CelestialBody celestialBody = artificialBody.Orbit.Parent.CelestialBody;
						artificialBody.DisableStabilization();
						if (artificialBody is Ship && (artificialBody as Ship).WarpStartEffectTask != null)
						{
							(artificialBody as Ship).WarpStartEffectTask.RunSynchronously();
						}
						artificialBody.Orbit.ParseNetworkData(transform.Orbit);
						artificialBody.UpdateOrbitPosition(SolarSystem.CurrentTime, resetTime: true);
						UpdateMapObject(artificialBody, celestialBody);
					}
					else if (transform.Realtime != null)
					{
						artificialBody.DisableStabilization();
						artificialBody.Orbit.ParseNetworkData(transform.Realtime);
					}
					bool flag2 = (artificialBody.Maneuver != null && transform.Maneuver == null) || (artificialBody.Maneuver == null && transform.Maneuver != null);
					if (artificialBody.Maneuver != null && transform.Maneuver == null)
					{
						artificialBody.ManeuverExited = true;
					}
					artificialBody.Maneuver = transform.Maneuver;
					if (artificialBody.Maneuver != null)
					{
						artificialBody.Orbit.ParseNetworkData(transform.Maneuver);
					}
					if (flag2 && MyPlayer.Instance.Parent is Ship && (MyPlayer.Instance.Parent as Ship).RadarSystem != null)
					{
						(MyPlayer.Instance.Parent as Ship).RadarSystem.PassiveScanObject(artificialBody);
						Instance.SolarSystem.ArtificialBodiesVisiblityModified();
					}
					if (transform.Forward != null && transform.Up != null)
					{
						artificialBody.SetTargetPositionAndRotation(null, transform.Forward.ToVector3(), transform.Up.ToVector3(), flag || !artificialBody.IsInVisibilityRange, movementMessage.SolarSystemTime);
						artificialBody.AngularVelocity = transform.AngularVelocity.ToVector3();
						if (transform.RotationVec != null)
						{
							artificialBody.RotationVec = transform.RotationVec.ToVector3D();
						}
					}
					if (transform.CharactersMovement != null)
					{
						foreach (CharacterMovementMessage item in transform.CharactersMovement)
						{
							if (item == null)
							{
								continue;
							}
							if (item.GUID == MyPlayer.Instance.GUID)
							{
								MyPlayer.Instance.ProcessMovementMessage(item);
								continue;
							}
							OtherPlayer player = Instance.GetPlayer(item.GUID);
							if (player != null)
							{
								player.ProcessMovementMessage(item);
							}
						}
					}
					if (transform.DynamicObjectsMovement != null)
					{
						foreach (DynamicObectMovementMessage item2 in transform.DynamicObjectsMovement)
						{
							if (item2 != null)
							{
								DynamicObject dynamicObject = Instance.GetDynamicObject(item2.GUID);
								if (dynamicObject != null)
								{
									dynamicObject.ProcessDynamicObectMovementMessage(item2);
								}
							}
						}
					}
					if (transform.CorpsesMovement != null)
					{
						foreach (CorpseMovementMessage item3 in transform.CorpsesMovement)
						{
							if (item3 != null)
							{
								Corpse corpse = Instance.GetCorpse(item3.GUID);
								if (corpse != null)
								{
									corpse.ProcessMoveCorpseObectMessage(item3);
								}
							}
						}
					}
					float num2 = (artificialBody.transform.position - MyPlayer.Instance.transform.position).sqrMagnitude - (float)(artificialBody.Radius * artificialBody.Radius);
					if (artificialBody is SpaceObjectVessel && (spaceObjectVessel == null || num > num2))
					{
						spaceObjectVessel = artificialBody as SpaceObjectVessel;
						num = num2;
					}
				}
			}
			if (list.Count > 0)
			{
				foreach (ArtificialBody item4 in list)
				{
					float num3 = (item4.transform.position - MyPlayer.Instance.transform.position).sqrMagnitude - (float)(item4.Radius * item4.Radius);
					if (item4 is SpaceObjectVessel && !(item4 as SpaceObjectVessel).IsDebrisFragment && (spaceObjectVessel == null || (num > num3 && (spaceObjectVessel.FTLEngine == null || spaceObjectVessel.FTLEngine.Status != SystemStatus.OnLine || (spaceObjectVessel.Velocity - MyPlayer.Instance.Parent.Velocity).SqrMagnitude < 900.0))))
					{
						spaceObjectVessel = item4 as SpaceObjectVessel;
						num = num3;
					}
				}
			}
			MyPlayer.Instance.NearestVessel = spaceObjectVessel;
			MyPlayer.Instance.NearestVesselSqDistance = num;
		}

		private void UpdateMapObject(ArtificialBody ab, CelestialBody oldParent)
		{
			if (ab is not SpaceObjectVessel) return;

			MapObject value;
			if (!Map.AllMapObjects.TryGetValue(ab as SpaceObjectVessel, out value))
			{
				Map.InitializeMapObject(ab as SpaceObjectVessel);
				Map.AllMapObjects.TryGetValue(ab as SpaceObjectVessel, out value);
			}
			if (!(value != null))
			{
				return;
			}
			if (oldParent != ab.Orbit.Parent.CelestialBody)
			{
				if (Map.gameObject.activeInHierarchy)
				{
					Map.UpdateParent(value.MainObject);
				}
				if (MyPlayer.Instance != null && this == MyPlayer.Instance.Parent)
				{
					ProviderManager.MainProvider.UpdateStatus();
				}
			}
			if (MyPlayer.Instance.LockedToTrigger is SceneTriggerNavigationPanel || MyPlayer.Instance.ShipControlMode == ShipControlMode.Navigation)
			{
				value.SetOrbit();
			}
		}

		public SpaceObject GetObject(long guid, SpaceObjectType objectType)
		{
			switch (objectType)
			{
			case SpaceObjectType.Player:
				if (guid == MyPlayer.Instance.GUID)
				{
					return MyPlayer.Instance;
				}
				return GetPlayer(guid);
			case SpaceObjectType.DynamicObject:
				return GetDynamicObject(guid);
			case SpaceObjectType.Corpse:
				return GetCorpse(guid);
			case SpaceObjectType.PlayerPivot:
			case SpaceObjectType.DynamicObjectPivot:
			case SpaceObjectType.CorpsePivot:
			{
				ArtificialBody artificialBody3 = SolarSystem.GetArtificialBody(guid);
				if (artificialBody3 != null)
				{
					return artificialBody3 as Pivot;
				}
				break;
			}
			case SpaceObjectType.Ship:
			{
				ArtificialBody artificialBody2 = SolarSystem.GetArtificialBody(guid);
				if (artificialBody2 != null)
				{
					return artificialBody2 as Ship;
				}
				break;
			}
			case SpaceObjectType.Asteroid:
			{
				ArtificialBody artificialBody = SolarSystem.GetArtificialBody(guid);
				if (artificialBody != null)
				{
					return artificialBody as Asteroid;
				}
				break;
			}
			}
			return null;
		}

		public static void MovePanelCursor(Transform trans, float panelWidth, float panelHeight)
		{
			float x = Mathf.Clamp(trans.localPosition.x + Mouse.current.delta.x.ReadValue() * MouseSpeedOnPanels, 0f, panelWidth);
			float y = Mathf.Clamp(trans.localPosition.y + Mouse.current.delta.y.ReadValue() * MouseSpeedOnPanels * (float)((!Instance.InvertedMouse) ? 1 : (-1)), 0f, panelHeight);
			trans.localPosition = new Vector3(x, y, trans.localPosition.z);
		}

		public void ConnectToServer(ServerData server, string serverPassword = null)
		{
			_connectToServerCoroutine = ConnectToServerCoroutine(server, serverPassword);
			StartCoroutine(_connectToServerCoroutine);
		}

		/// <summary>
		/// 	Connect to a remote server.
		/// </summary>
		public IEnumerator ConnectToServerCoroutine(ServerData server, string serverPassword = null)
		{

			// Cancel if server is offline.
			if (!server.Online)
			{
				ShowMessageBox(Localization.ConnectionError, Localization.ServerOffline);
				_connectToServerCoroutine = null;
				yield break;
			}

			// Get user input for password to server if it is locked.
			if (server.Locked && serverPassword == null)
			{
				PasswordEnterPanel.SetActive(value: true);
				PasswordInputField.text = string.Empty;
				PasswordInputField.Select();
				yield return new WaitWhile(() => PasswordEnterPanel.activeInHierarchy);
				serverPassword = PasswordInputField.text;
			}

			// Create new character if you don't have it from before.
			if (server.CharacterData == null && _newCharacterData == null)
			{
				CreateCharacterPanel.SetActive(value: true);
				CurrentGenderText.text = CurrentGender.ToLocalizedString();
				InventoryCharacterPreview.instance.ResetPosition();
				CharacterInputField.text = ProviderManager.MainProvider.GetUsername();
				CharacterInputField.Select();
				yield return new WaitWhile(() => CreateCharacterPanel.activeInHierarchy);
				string newCharacterName = CharacterInputField.text.Trim();
				CharacterInputField.text = string.Empty;
				if (newCharacterName.Length > 0)
				{
					_newCharacterData = new CharacterData
					{
						Name = newCharacterName,
						Gender = CurrentGender,
						HeadType = 1,
						HairType = 1
					};
				}
			}

			// Check if server is still online.
			if (!server.Online)
			{
				ShowMessageBox(Localization.ConnectionError, Localization.ServerOffline);
				_connectToServerCoroutine = null;
				yield break;
			}

			// Prepare for connection.
			LastConnectedServer = server;
			CanvasManager.LoadingTips.text = ShuffledTexts.GetNextInLoop();
			CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.ConnectingToGame);
			LastConnectedServerPass = serverPassword;
			this.InvokeRepeating(CheckLoadingComplete, 3f, 1f);

			// Connect to server.
			NetworkController.Instance.ConnectToGame(server, _newCharacterData, serverPassword);

			// Cleanup data.
			_newCharacterData = null;
			InvitedToServerPassword = null;
			_connectToServerCoroutine = null;
		}

		/// <summary>
		/// 	Cancel connecting to server.
		/// </summary>
		public void StopConnectToServerCoroutine()
		{
			if (_connectToServerCoroutine != null)
			{
				StopCoroutine(_connectToServerCoroutine);
				_connectToServerCoroutine = null;
			}
		}

		public void DeleteCharacterRequest(ServerData gs)
		{
			ShowConfirmMessageBox(Localization.DeleteCharacter, Localization.AreYouSureDeleteCharacter, Localization.Yes, Localization.No, () =>
			{
				DeleteCharacterRequest deleteCharacterRequest = new DeleteCharacterRequest
				{
					ServerId = gs.Id,
					SteamId = NetworkController.PlayerId
				};

				NetworkController.SendTCP(deleteCharacterRequest, gs.IpAddress, gs.StatusPort, out int latency, false, true);
			});
		}

		/// <summary>
		/// 	Reconnect after we have been disconnected.
		/// </summary>
		public void Reconnect()
		{
			this.InvokeRepeating(CheckLoadingComplete, 3f, 1f);
			NetworkController.Instance.ConnectToGame(LastConnectedServer, _newCharacterData, LastConnectedServerPass);
		}

		private void LogInResponseListener(NetworkData data)
		{
			LogInResponse logInResponse = data as LogInResponse;
			LogInResponseReceived = true;

			if (ReconnectAutomatically)
			{
				ReconnectAutomatically = false;
			}

			if (logInResponse.Response == ResponseResult.Success)
			{
				Dbg.Info("Logged into game.");

				SolarSystem.Set(SolarSystemRoot.transform.Find("SunRoot"), SolarSystemRoot.transform.Find("PlanetsRoot"), logInResponse.ServerTime);
				SolarSystem.LoadDataFromResources();
				MyPlayer.SpawnMyPlayer(logInResponse);
				CanvasManager.SelectScreen(CanvasManager.Screen.Loading);

				if (logInResponse.IsAlive)
				{
					PlayerSpawnRequest playerSpawnRequest = new PlayerSpawnRequest
					{
						SpawPointParentID = 0L
					};
					NetworkController.Instance.SendToGameServer(playerSpawnRequest);
				}
				else if (InvitedToServerSpawnPointId != null)
				{
					PlayerSpawnRequest playerSpawnRequest2 = new PlayerSpawnRequest
					{
						SpawPointParentID = InvitedToServerSpawnPointId.VesselGUID
					};
					NetworkController.Instance.SendToGameServer(playerSpawnRequest2);
					InvitedToServerSpawnPointId = null;
				}
				else
				{
					ShowSpawnPointSelection(logInResponse.SpawnPointsList, logInResponse.CanContinue);
				}
				foreach (DebrisFieldDetails debrisField in logInResponse.DebrisFields)
				{
					DebrisFields.Add(new DebrisField(debrisField));
					Map.InitializeMapObject(new DebrisField(debrisField));
				}

				ItemsIngredients = logInResponse.ItemsIngredients;
				Quests = logInResponse.Quests.Select((QuestData m) => new Quest(m)).ToList();
				SpaceObjectVessel.VesselDecayRateMultiplier = logInResponse.VesselDecayRateMultiplier;
				ExposureRange = logInResponse.ExposureRange;
				VesselExposureValues = logInResponse.VesselExposureValues;
				PlayerExposureValues = logInResponse.PlayerExposureValues;
			}
			else if (logInResponse.Response == ResponseResult.WrongPassword)
			{
				CanvasManager.SelectScreen(CanvasManager.Screen.MainMenu);
				ShowMessageBox(Localization.ConnectionError, Localization.WrongPassword);
			}
			else
			{
				Dbg.Error("Server dropped connection.");
				CanvasManager.SelectScreen(CanvasManager.Screen.MainMenu);
				ShowMessageBox(Localization.ConnectionError, Localization.ServerUnreachable);
			}
		}

		public void SignInButton()
		{
			CanvasManager.SelectScreen(CanvasManager.Screen.Loading);
			SinglePlayerMode = false;
			SignIn();
		}

		/// <summary>
		/// 	Sign in to the main server.
		/// </summary>
		private void SignIn()
		{
			CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.SigningIn);
			Regex regex = new Regex("[^0-9.]");

			SignInRequest signInRequest = new SignInRequest()
			{
				PlayerId = NetworkController.PlayerId,
				Version = regex.Replace(Application.version, string.Empty),
				Hash = CombinedHash,
				JoiningId = InvitedToServerId
			};

			LastSignInRequest = signInRequest;
			try
			{
				ConnectionMain.Get<SignInResponse>(signInRequest, SignInResponseListener);
			}
			catch (Exception)
			{
				ShowMessageBox(Localization.ConnectionError, Localization.ServerUnreachable);
			}
		}

		// Set the settings for the server to connect to later.
		private void SignInResponseListener(SignInResponse signInResponse)
		{
			if (ReconnectAutomatically)
			{
				return;
			}

			if (signInResponse.Result == ResponseResult.Success)
			{
				CanvasManager.SelectScreen(CanvasManager.Screen.CharacterSelect);

				// Save connection details for later use.
				LastConnectedServer = new()
				{
					Id = signInResponse.Server.Id,
					IpAddress = signInResponse.Server.IpAddress,
					GamePort = signInResponse.Server.GamePort,
					StatusPort = signInResponse.Server.StatusPort,
					Hash = signInResponse.Server.Hash,
					Online = true
				};

				// Connect to server!
				ConnectToServer(LastConnectedServer);
			}
			else if (signInResponse.Result is ResponseResult.Error or ResponseResult.AlreadyLoggedInError)
			{
				CanvasManager.SelectScreen(CanvasManager.Screen.MainMenu);
				ShowMessageBox(Localization.ConnectionError, Localization.SignInError);
			}
			else if (signInResponse.Result == ResponseResult.ServerNotFound)
			{
				ShowMessageBox(Localization.Error, Localization.ServerNotFound);
			}
			else if (signInResponse.Result == ResponseResult.ClientVersionError)
			{
				ShowMessageBox(Localization.VersionError, Localization.VersionErrorMessage);
			}
			else if (signInResponse.Result == ResponseResult.AccountNotFound)
			{
				FindPlayerId((res) => SignIn());
			}
			else
			{
				ShowMessageBox(Localization.ConnectionError, Localization.ServerUnreachable);
			}

			LastSignInResponse = signInResponse;
			_receivedSignInResponse = true;
		}

		/// <summary>
		/// 	Attempts to get player id from the main server by using steam and discord ids. If it doesn't find the id, a new account is created.
		/// </summary>
		public void FindPlayerId(Action<PlayerIdResponse> callback)
		{
			GetPlayerIdRequest idRequest = new()
			{
				SteamId = ProviderManager.SteamId,
				DiscordId = ProviderManager.DiscordId
			};

			try
			{
				// First time booting, so we need to download id from the main server.
				ConnectionMain.Get<PlayerIdResponse>(idRequest, (data) =>
				{
					CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.FindingPlayer);

					// Get id from server and save it as a PlayerPref.
					if (data.Result == ResponseResult.Success)
					{
						callback(data);
					}
					else if (data.Result == ResponseResult.AccountNotFound)
					{
						// No account exists with that id, so we need to create a new player account.

						CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.NewPlayer);

						CreatePlayerRequest createRequest = new()
						{
							Name = ProviderManager.MainProvider.GetUsername(),
							Region = Region.Europe,
							PlayerId = NetworkController.PlayerId,
							SteamId = ProviderManager.SteamId,
							DiscordId = ProviderManager.DiscordId
						};

						// Send request to server.
						ConnectionMain.Get<PlayerIdResponse>(createRequest, (data) =>
						{
							if (data.Result == ResponseResult.Success)
							{
								Dbg.Info("Successfully created a new player account with id", data.PlayerId);
								callback(data);
							}
							else
							{
								ShowMessageBox(Localization.SignInError, Localization.ServerUnreachable);
							}
						});
					}
					else
					{
						ShowMessageBox(Localization.SignInError, Localization.ServerUnreachable);
					}
				});
			}
			catch
			{
				ShowMessageBox(Localization.ConnectionError, Localization.ServerUnreachable);
			}
		}

		public void CreateCharacterButton()
		{
			CreateCharacterPanel.SetActive(value: false);
		}

		public void CreateCharacterExit()
		{
			StopConnectToServerCoroutine();
			CreateCharacterPanel.SetActive(value: false);
		}

		/// <summary>
		/// 	Connects to server if input is successful, cancels if not.
		/// </summary>
		public void EnterPasswordMenu(bool success)
		{
			if (success)
			{
				PasswordEnterPanel.SetActive(value: false);
			}
			else
			{
				StopConnectToServerCoroutine();
				PasswordEnterPanel.SetActive(value: false);
			}
		}

		public void SwitchCurrentGender()
		{
			if (CurrentGender == Gender.Male)
			{
				CurrentGender = Gender.Female;
			}
			else
			{
				CurrentGender = Gender.Male;
			}
			CurrentGenderText.text = CurrentGender.ToLocalizedString();
			InventoryCharacterPreview.instance.ChangeGender(CurrentGender);
		}

		public void QuitGameButton()
		{
			ShowConfirmMessageBox(Localization.ExitGame, Localization.AreYouSureExitGame, Localization.Yes, Localization.No, QuitGameYes);
		}

		public void QuitGameYes()
		{
			ExitGame();
		}

		// TODO: Move this to the provider or network system.
		public void ProcessInvitation(InviteMessage inviteMessage)
		{
			if (_processInvitationCoroutine != null)
			{
				StopCoroutine(_processInvitationCoroutine);
			}
			_processInvitationCoroutine = ProcessInvitationCoroutine(inviteMessage);
			StartCoroutine(_processInvitationCoroutine);
		}

		public IEnumerator ProcessInvitationCoroutine(InviteMessage inviteMessage)
		{
			yield return new WaitUntil(() => !SceneLoader.IsPreloading);
			InvitedToServerId = inviteMessage.ServerId;
			InvitedToServerPassword = inviteMessage.Password;
			InvitedToServerSpawnPointId = inviteMessage.SpawnPointId;
			OpenMainScreen();
		}

		public void PlayNewSPGame()
		{
			StartCoroutine(PlaySPCoroutine());
		}

		/// <summary>
		/// 	Start singleplayer server, and connect to it.
		/// </summary>
		public IEnumerator PlaySPCoroutine(string filename = null)
		{
			if (!ProviderManager.AnyInitialised)
			{
				ShowMessageBox(Localization.Error, Localization.NoProvider);
				yield break;
			}

			// Enable loading screen.
			CanvasManager.SelectScreen(CanvasManager.Screen.Loading);
			yield return null;
			string filePath = _spServerPath + "\\" + _spServerFileName;
			try
			{
				KillAllSPProcesses();
				_spServerProcess = new Process();
				_spServerProcess.StartInfo.WorkingDirectory = _spServerPath;
				_spServerProcess.StartInfo.FileName = filePath;
				string text = (SinglePlayerGameMode != 0) ? "-configdir Sandbox " : string.Empty;
				if (SinglePlayerQuickLoad)
				{
					_spServerProcess.StartInfo.Arguments = text;
				}
				else if (SinglePlayerRespawn || filename == null)
				{
					_spServerProcess.StartInfo.Arguments = text + "-clean";
				}
				else
				{
					_spServerProcess.StartInfo.Arguments = text + "-load " + filename;
				}
				_spServerProcess.StartInfo.UseShellExecute = false;
				_spServerProcess.StartInfo.RedirectStandardOutput = true;
				_spServerProcess.StartInfo.CreateNoWindow = true;
				if (!_spServerProcess.Start())
				{
					CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.None);
					CanvasManager.SelectScreen(CanvasManager.Screen.MainMenu);
					_spServerProcess = null;
					throw new Exception("Process.Start function returned FALSE");
				}

				Dbg.Log("Started single player server process.");

				// Get ports to connect to.
				int gamePort = 6104;
				int statusPort = 6105;
				while (true)
				{
					string text2 = _spServerProcess.StandardOutput.ReadLine();
					if (text2 == null || text2.ToLower().EndsWith("ready"))
					{
						break;
					}
					if (text2.ToLower().StartsWith("ports:"))
					{
						try
						{
							string[] array = text2.Split(':');
							string[] array2 = array[1].Split(',');
							int.TryParse(array2[0], out gamePort);
							int.TryParse(array2[1], out statusPort);
						}
						catch
						{
						}
					}
				}

				SinglePlayerQuickLoad = false;
				SinglePlayerRespawn = false;
				SinglePlayerMode = true;
				lastSPAutosaveTime = Time.time;
				int latency = -1;

				ServerStatusRequest serverStatusRequest = new ServerStatusRequest
				{
					PlayerId = NetworkController.PlayerId,
					SendDetails = true
				};

				if (NetworkController.SendTCP(serverStatusRequest, "127.0.0.1", statusPort, out latency, true, true) is ServerStatusResponse serverStatusResponse && serverStatusResponse.Response == ResponseResult.Success)
				{
					// Create new character if it doesn't exist.
					if (serverStatusResponse.CharacterData == null)
					{
						serverStatusResponse.CharacterData = new CharacterData
						{
							Name = ProviderManager.MainProvider.GetUsername(),
							Gender = Gender.Male,
							HairType = 1,
							HeadType = 1
						};
					}

					NetworkController.Instance.ConnectToGameSP(gamePort, serverStatusResponse.CharacterData);

					this.InvokeRepeating(CheckLoadingComplete, 3f, 1f);

					Dbg.Info("Successfully connected to singleplayer server!");
					yield break;
				}

				CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.None);
				throw new Exception("Unable to connect to SP server.");
			}
			catch (Exception ex)
			{
				Dbg.Error("Unable to start single player game", ex.Message, ex.StackTrace);
				CanvasManager.SelectSpawnPointScreen.SetActive(false);
				CanvasManager.SelectScreen(CanvasManager.Screen.MainMenu);
				if (File.Exists(filePath))
				{
					ShowMessageBox(Localization.Error, Localization.UnableToStartSPGame);
				}
				else
				{
					ShowMessageBox(Localization.Error, Localization.UnableToStartSPGame + " " + Localization.Hellion_SpExeIsMissing);
				}
			}
		}

		public string GetSPPath()
		{
			return _spServerPath + ((SinglePlayerGameMode != 0) ? "\\Sandbox\\" : string.Empty);
		}

		public void QuickLoad()
		{
			if (CanvasManager.DeadScreen.activeInHierarchy)
			{
				CanvasManager.DeadScreen.SetActive(value: false);
			}
			CanvasManager.TextChat.CreateMessage(null, "LOADING GAME.", local: false);
			SinglePlayerQuickLoad = true;
			if (MyPlayer.Instance == null || !MyPlayer.Instance.IsAlive)
			{
				OpenMainScreen();
				return;
			}
			NetworkController.Instance.SendToGameServer(new ServerShutDownMessage
			{
				Restrat = false,
				CleanRestart = false
			});
		}

		public void QuickSave()
		{
			CanvasManager.TextChat.CreateMessage(null, "SAVING GAME.", local: false);
			NetworkController.Instance.SendToGameServer(new SaveGameMessage
			{
				FileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".save",
				AuxData = MyPlayer.Instance.GetSaveFileAuxData()
			});
		}

		public void AutoSave()
		{
			lastSPAutosaveTime = Time.time;
			CanvasManager.TextChat.CreateMessage(null, "SAVING GAME.", local: false);
			NetworkController.Instance.SendToGameServer(new SaveGameMessage
			{
				FileName = "autosave.save",
				AuxData = MyPlayer.Instance.GetSaveFileAuxData()
			});
		}

		private void KillAllSPProcesses()
		{
			try
			{
				if (_spServerProcess != null && !_spServerProcess.HasExited)
				{
					_spServerProcess.Kill();
					_spServerProcess = null;
				}
			}
			catch
			{
			}
			try
			{
				List<Process> list = (from m in Process.GetProcesses()
					where !m.HasExited && m.MainModule.FileName.ToLower().EndsWith(_spServerFileName.ToLower())
					select m).ToList();
				foreach (Process item in list)
				{
					try
					{
						item.Kill();
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
		}

		public void LatencyTestMessage()
		{
			_lastLatencyMessageTime = Time.realtimeSinceStartup;
			new Task(delegate
			{
				int latency;
				NetworkController.SendTCP(new LatencyTestMessage(), LastConnectedServer.IpAddress, LastConnectedServer.StatusPort, out latency);

				_latencyMs = latency;
			}).Start();
			if (MyPlayer.Instance.IsAlive)
			{
				if (LatencyMs > 120 && LatencyMs < 150)
				{
					CanvasManager.Latency.color = Colors.SlotGray;
					CanvasManager.Latency.gameObject.Activate(value: true);
				}
				else if (LatencyMs >= 150)
				{
					CanvasManager.Latency.color = Colors.PowerRed;
					CanvasManager.Latency.gameObject.Activate(value: true);
				}
				else
				{
					CanvasManager.Latency.gameObject.Activate(value: false);
				}
			}
			else
			{
				CanvasManager.Latency.gameObject.Activate(value: false);
			}
			Instance.Invoke(Instance.LatencyTestMessage, 1f);
		}

		private void CheckLoadingComplete()
		{
			if (LoadingScenesCount == 0 && _loadingFinishedTask != null && !this.IsInvoking(AfterLoadingFinishedTask) && !this.IsInvoking(ClearCanvasesAndStartGame))
			{
				this.Invoke(AfterLoadingFinishedTask, 1f);
			}
			else if (LastLoadingTipsChangeTime + 5f < Time.realtimeSinceStartup)
			{
				LastLoadingTipsChangeTime = Time.realtimeSinceStartup;
				CanvasManager.LoadingTips.text = ShuffledTexts.GetNextInLoop();
			}
		}

		public void AfterLoadingFinishedTask()
		{
			this.Invoke(ClearCanvasesAndStartGame, (MyPlayer.Instance.Parent is Pivot) ? 10 : 0);
		}

		private void ClearCanvasesAndStartGame()
		{
			this.CancelInvoke(CheckLoadingComplete);
			AkSoundEngine.SetRTPCValue(SoundManager.Instance.InGameVolume, 1f);
			MyPlayer.Instance.PlayerReady = true;
			ProviderManager.MainProvider.UpdateStatus();
			MyPlayer.Instance.InitializeCameraEffects();

			Instance.CanvasManager.SelectSpawnPointScreen.Activate(value: false);
			IsInGame = true;
			lastSPAutosaveTime = Time.time;
			ToggleCursor(false);
			CanvasManager.ToggleCanvasUI(val: true);
			CanvasManager.ToggleTextChatCanvas(val: true);
			MainMenuRoot.SetActive(value: false);
			CanvasManager.LoadingTips.text = string.Empty;
			CanvasManager.SelectScreen(CanvasManager.Screen.None);
			StartCoroutine(FixPlayerInCryo());
			_loadingFinishedTask.RunSynchronously();
			EnvironmentReady = true;
			NetworkController.Instance.SendToGameServer(new EnvironmentReadyMessage());
		}

		private IEnumerator FixPlayerInCryo()
		{
			SceneTriggerExecuter exec = MyPlayer.Instance.Parent.GetComponentsInChildren<SceneTriggerExecuter>(includeInactive: true).FirstOrDefault((SceneTriggerExecuter m) => m.IsMyPlayerInLockedState && m.CurrentState == "spawn");
			if (!(exec == null))
			{
				yield return new WaitUntil(() => MyPlayer.Instance.gameObject.activeInHierarchy);
				yield return new WaitForSecondsRealtime(0.5f);
				exec.ChangeStateImmediateForce("occupied");
			}
		}

		public void ChangeStatsByIfNotAdmin<T>(ProviderStatID id, T value)
		{
			try
			{
				if (!MyPlayer.Instance.IsAdmin)
				{
					ProviderManager.MainProvider.ChangeStatBy(id, value);
				}
			}
			catch (Exception ex)
			{
				Dbg.Error(ex.Message, ex.StackTrace);
			}
		}

		public float GetVesselExposureDamage(double distance)
		{
			if (VesselExposureValues == null)
			{
				return 1f;
			}
			return VesselExposureValues[(int)(Mathf.Clamp01((float)(distance / ExposureRange)) * 99f)];
		}

		public float GetPlayerExposureDamage(double distance)
		{
			if (PlayerExposureValues == null)
			{
				return 0f;
			}
			return PlayerExposureValues[(int)(Mathf.Clamp01((float)(distance / ExposureRange)) * 99f)];
		}

		private void UpdateVesselDataMessageListener(NetworkData data)
		{
			UpdateVesselDataMessage updateVesselDataMessage = data as UpdateVesselDataMessage;
			if (updateVesselDataMessage.VesselsDataUpdate == null)
			{
				return;
			}
			foreach (VesselDataUpdate item in updateVesselDataMessage.VesselsDataUpdate)
			{
				SpaceObjectVessel spaceObjectVessel = SolarSystem.GetArtificialBody(item.GUID) as SpaceObjectVessel;
				if (spaceObjectVessel != null && spaceObjectVessel.VesselData != null)
				{
					if (item.VesselName != null)
					{
						spaceObjectVessel.VesselData.VesselName = item.VesselName;
					}
					if (item.VesselRegistration != null)
					{
						spaceObjectVessel.VesselData.VesselRegistration = item.VesselRegistration;
					}
					if (item.RadarSignature.HasValue)
					{
						spaceObjectVessel.VesselData.RadarSignature = item.RadarSignature.Value;
					}
					if (item.IsAlwaysVisible.HasValue)
					{
						spaceObjectVessel.VesselData.IsAlwaysVisible = item.IsAlwaysVisible.Value;
					}
					if (item.IsDistressSignalActive.HasValue)
					{
						spaceObjectVessel.VesselData.IsDistressSignalActive = item.IsDistressSignalActive.Value;
					}
				}
			}
		}
	}
}
