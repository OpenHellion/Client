using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using TMPro;
using TriInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.Discord;
using ZeroGravity.Effects;
using ZeroGravity.LevelDesign;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;
using ZeroGravity.UI;

namespace ZeroGravity
{
	public class Client : MonoBehaviour
	{
		public enum SPGameMode
		{
			Standard,
			Sandbox
		}

		public enum ServerCategories
		{
			Official,
			Community,
			Favorites
		}

		public enum SceneLoadTypeValue
		{
			Simple,
			PreloadWithCopy
		}

		[ReadOnly]
		public string SpServerPath = Directory.GetCurrentDirectory() + "\\Hellion_Data\\HELLION_SP";

		private string spServerFileName = "HELLION_SP.exe";

		private CharacterData newCharacterData;

		private GameServerUI deleteChararacterFromServer;

		[NonSerialized]
		public SteamStats SteamStats;

		public bool ExperimentalBuild;

		[Multiline(2)]
		public string ExperimentalText;

		public GameObject ExperimentalGameObject;

		public bool AllowLoadingOldSaveGames;

		[NonSerialized]
		public ZeroGravity.Network.Gender CurrentGender;

		private string userName = string.Empty;

		private string ServerIP;

		private int port;

		private SignInResponse currentResponse;

		private int sortOrder = 1;

		private int prevSortMode = -1;

		[NonSerialized]
		public ConcurrentBag<GameServerUI> serverListElements = new ConcurrentBag<GameServerUI>();

		[Space(10f)]
		public ServerCategories CurrentServerFilter;

		private PlayerServersData playerServersData;

		public InputField CharacterInputField;

		public GameObject CreateCharacterPanel;

		public Text CurrentGenderText;

		public GameObject PasswordEnterPanel;

		public InputField PasswordInputField;

		public GameObject sampleServerButton;

		public Transform ServerListContentPanel;

		public InputField ServerSearchInputField;

		public Scrollbar ServerListScrollBar;

		public GameObject OfficialActive;

		public GameObject CommunityActive;

		public GameObject FavoritesActive;

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

		private ConcurrentQueue<Tuple<float, Type>> sentNetworkDataLog = new ConcurrentQueue<Tuple<float, Type>>();

		private ConcurrentQueue<Tuple<float, Type>> receivedNetworkDataLog = new ConcurrentQueue<Tuple<float, Type>>();

		private int maxNetworkDataLogsSize = 3000;

		private DateTime clientStartTime = DateTime.UtcNow.ToUniversalTime();

		public static SceneLoadTypeValue SceneLoadType = SceneLoadTypeValue.PreloadWithCopy;

		public static int ControlsVersion = 1;

		private static Client singletonInstance = null;

		public static volatile bool IsRunning = false;

		public NetworkController NetworkController;

		public SceneLoader SceneLoader;

		public SolarSystem SolarSystem;

		public CanvasManager CanvasManager;

		public CustomInputModule InputModule;

		public static SignInRequest LastSignInRequest = null;

		public static SignInResponse LastSignInResponse = null;

		public static ConcurrentBag<GameServerUI> LastGameServersData = null;

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

		private bool gameExitWanted;

		private float maxSecondsToWaitForExit = 3f;

		public bool EnvironmentReady;

		private int serverUpdateCounter;

		public bool UpdateServers;

		protected Callback<GameRichPresenceJoinRequested_t> m_GameRichPresenceJoinRequested;

		private bool receivedSignInResponse;

		private int autosaveInterval = 600;

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

		private float mouseSensitivity;

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

		private bool GetP2PPacketsThreadActive;

		[ReadOnly]
		public bool SignInFailed;

		private int collisionLayerMask;

		public static volatile bool ForceRespawn = false;

		public static volatile bool IsLogout = false;

		public static volatile bool IsDisconected = false;

		public GameObject PreloadingScreen;

		public int CurrentLanguageIndex;

		private IEnumerator processInvitationCoroutine;

		private IEnumerator inviteCoroutine;

		public GameObject InviteScreen;

		public DiscordController Discord;

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

		private float lastLatencyMessateTime = -1f;

		private int _LatencyMs;

		[Header("Cursor")]
		public Texture2D DefaultCursor;

		public Texture2D HoverCursor;

		public static bool ReconnectAutomatically = false;

		public static string LastConnectedServerPass = null;

		public static GameServerUI LastConnectedServer;

		public static long InvitedToServerId = -1L;

		public static string InvitedToServerPassword = null;

		public static VesselObjectID InvitedToServerSpawnPointId = null;

		private GameServerUI _invitedToServer;

		private IEnumerator _connectToServerCoroutine;

		private Task _loadingFinishedTask;

		private Task _restoreMapDetailsTask;

		public static Client Instance => singletonInstance;

		public static bool IsGameBuild => Instance != null;

		public float MouseSensitivity
		{
			get
			{
				return mouseSensitivity;
			}
			set
			{
				mouseSensitivity = value;
				TeamUtility.IO.InputManager.GetAxisConfiguration("KeyboardAndMouse", "LookVertical").sensitivity = mouseSensitivity / 10f;
				TeamUtility.IO.InputManager.GetAxisConfiguration("KeyboardAndMouse", "LookHorizontal").sensitivity = mouseSensitivity / 10f;
			}
		}

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

		public string SteamId => (!SteamManager.Initialized) ? null : SteamUser.GetSteamID().ToString();

		public int LatencyMs
		{
			get
			{
				if (lastLatencyMessateTime < 0f)
				{
					return 0;
				}
				float num = Time.realtimeSinceStartup - lastLatencyMessateTime;
				if (_LatencyMs < 0 || num > 5f)
				{
					return (int)(num * 1000f);
				}
				return _LatencyMs;
			}
		}


		private void Awake()
		{
			SteamStats = GetComponent<SteamStats>();
			if (SteamStats == null)
			{
				SteamStats = base.gameObject.AddComponent<SteamStats>();
			}

			Texture[] source = Resources.LoadAll<Texture>("Emblems");
			SceneVesselEmblem.Textures = source.ToDictionary((Texture x) => x.name, (Texture y) => y);

			if (!SteamManager.Initialized)
			{
				Dbg.Error("Steam isn't initialised.");
				ExitGame();
#if UNITY_EDITOR
				EditorApplication.isPlaying = false;
#endif
			}

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
			singletonInstance = this;
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
					SteamStats.SetAchievement(SteamAchievementID.other_testing_squad_member);
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
			NetworkController.EventSystem.AddListener(typeof(SignInResponse), SignInResponseListener);
			NetworkController.EventSystem.AddListener(typeof(LogInResponse), LogInResponseListener);
			playerServersData = null;
			if (File.Exists(Path.Combine(Application.persistentDataPath, "ServersData.json")))
			{
				try
				{
					playerServersData = Json.LoadPersistent<PlayerServersData>("ServersData.json");
					if (playerServersData.FavoriteServers == null)
					{
						playerServersData.FavoriteServers = new List<long>();
					}
					CurrentServerFilter = playerServersData.PreviousFilter;
				}
				catch
				{
				}
			}
			if (playerServersData == null)
			{
				playerServersData = new PlayerServersData();
				playerServersData.PreviousFilter = ServerCategories.Official;
				playerServersData.FavoriteServers = new List<long>();
			}
			OfficialActive.SetActive(CurrentServerFilter == ServerCategories.Official);
			CommunityActive.SetActive(CurrentServerFilter == ServerCategories.Community);
			FavoritesActive.SetActive(CurrentServerFilter == ServerCategories.Favorites);
			collisionLayerMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("PlayerCollision"));
			NetworkController.EventSystem.AddListener(typeof(KillPlayerMessage), KillPlayerMessageListener);
			NetworkController.EventSystem.AddListener(typeof(LogOutResponse), LogOutResponseListener);
			NetworkController.EventSystem.AddListener(typeof(DestroyObjectMessage), DestroyObjectMessageListener);
			NetworkController.EventSystem.AddListener(typeof(PlayerSpawnResponse), PlayerSpawnResponseListener);
			NetworkController.EventSystem.AddListener(typeof(SpawnObjectsResponse), SpawnObjectsReponseListener);
			NetworkController.EventSystem.AddListener(typeof(MovementMessage), MovementMessageListener);
			NetworkController.EventSystem.AddListener(typeof(DynamicObjectsInfoMessage), DynamicObjectsInfoMessageListener);
			NetworkController.EventSystem.AddListener(typeof(PlayersOnServerResponse), PlayersOnServerResponseListener);
			NetworkController.EventSystem.AddListener(typeof(AvailableSpawnPointsResponse), AvailableSpawnPointsResponseListener);
			NetworkController.EventSystem.AddListener(typeof(ConsoleMessage), ConsoleMessageListener);
			NetworkController.EventSystem.AddListener(typeof(ShipCollisionMessage), ShipCollisionMessageListener);
			NetworkController.EventSystem.AddListener(typeof(UpdateVesselDataMessage), UpdateVesselDataMessageListener);
			NetworkController.EventSystem.AddListener(EventSystem.InternalEventType.ShowMessageBox, ShowMessageBoxListener);
			NetworkController.EventSystem.AddListener(EventSystem.InternalEventType.OpenMainScreen, OpenMainScreneListener);
			NetworkController.EventSystem.AddListener(EventSystem.InternalEventType.ReconnectAuto, ReconnectAutoListener);
			NetworkController.EventSystem.AddListener(EventSystem.InternalEventType.RemoveLoadingCanvas, RemoveLoadingCanvasListener);
			NetworkController.EventSystem.AddListener(EventSystem.InternalEventType.ConnectionFailed, ConnectionFailedListener);
			NetworkController.EventSystem.AddListener(EventSystem.InternalEventType.CloseAllLoadingScreens, CloseAllLoadingScreensListener);
			m_GameRichPresenceJoinRequested = Callback<GameRichPresenceJoinRequested_t>.Create(OnGameRichPresenceJoinRequested);
			if (SteamManager.Initialized)
			{
				Analytics.SetUserId(SteamUser.GetSteamID().ToString());
			}
			Discord.UpdateStatus();
			if (InvitedToServerId > 0 && SteamManager.Initialized)
			{
				inviteCoroutine = ConnectToInvite();
				StartCoroutine(inviteCoroutine);
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
						NetworkController.EventSystem.Invoke(new EventSystem.InternalEventData(EventSystem.InternalEventType.ReconnectAuto));
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
			InviteMessage inviteMessage = new InviteMessage();
			inviteMessage.Time = Time.time;
			inviteMessage.ServerId = LastConnectedServer.Id;
			inviteMessage.Password = LastConnectedServerPass;
			inviteMessage.SpawnPointId = spawnPointId;
			InviteMessage data = inviteMessage;
			byte[] inArray = Serializer.Serialize(data);
			return Convert.ToBase64String(inArray);
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

		private void OnGameRichPresenceJoinRequested(GameRichPresenceJoinRequested_t param)
		{
			try
			{
				InviteMessage inviteMessage = Serializer.ReceiveData(new MemoryStream(Convert.FromBase64String(param.m_rgchConnect))) as InviteMessage;
				ProcessInvitation(inviteMessage);
			}
			catch (Exception)
			{
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
					CanvasManager.CanChooseSpown = true;
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
				CanvasManager.CanChooseSpown = true;
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

		public void OpenMainScreneListener(EventSystem.InternalEventData data)
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
				NetworkController.SendToGameServer(new LogOutRequest());
			}
		}

		public void ShowSpawnPointSelection(List<SpawnPointDetails> spawnPoints, bool canContinue)
		{
			CanvasManager.SelectScreen(CanvasManager.Screen.ChooseShip);
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
			if (CanvasManager.CanChooseSpown)
			{
				CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.Loading);
				CanvasManager.CanChooseSpown = false;
				PlayerSpawnRequest playerSpawnRequest = new PlayerSpawnRequest();
				playerSpawnRequest.SpawnSetupType = details.SpawnSetupType;
				playerSpawnRequest.SpawPointParentID = details.SpawnPointParentID;
				NetworkController.SendToGameServer(playerSpawnRequest);
			}
		}

		private void LogOutResponseListener(NetworkData data)
		{
			LogOutResponse logOutResponse = data as LogOutResponse;
			if (logOutResponse.Response == ResponseResult.Error)
			{
				Dbg.Error("Failed to log out properly");
			}
			LogCustomEvent("log_out", flush: true);
			NetworkController.Disconnect();
			if (SinglePlayerMode)
			{
				KillAllSPProcesses();
			}
			if (gameExitWanted)
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
			SpaceObject @object = GetObject(destroyObjectMessage.ID, destroyObjectMessage.ObjectType);
			if (@object != null && @object.Type != SpaceObjectType.PlayerPivot && @object.Type != SpaceObjectType.DynamicObjectPivot && @object.Type != SpaceObjectType.CorpsePivot)
			{
				@object.DestroyGeometry();
				if (@object is DynamicObject && (@object as DynamicObject).Item != null && (@object as DynamicObject).Item.AttachPoint != null)
				{
					(@object as DynamicObject).Item.AttachPoint.DetachItem((@object as DynamicObject).Item);
				}
				if (MyPlayer.Instance != null && MyPlayer.Instance.CurrentActiveItem != null && MyPlayer.Instance.CurrentActiveItem.GUID == @object.GUID)
				{
					MyPlayer.Instance.Inventory.RemoveItemFromHands(resetStance: true);
				}
				UnityEngine.Object.Destroy(@object.gameObject);
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

		public void OpenMainScreen()
		{
			if (!openMainSceneStarted)
			{
				openMainSceneStarted = true;
				InGamePanels.Detach();
				ToggleCursor(true);
				if (MyPlayer.Instance != null)
				{
					UnityEngine.Object.Destroy(MyPlayer.Instance.gameObject);
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
			if (CanvasManager.SpawnOptionsScreen.activeInHierarchy)
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
			InviteScreen.SetActive(value: true);
			Connect();
			CanvasManager.Disclamer.SetActive(value: false);
			yield return new WaitUntil(() => receivedSignInResponse && serverListElements.FirstOrDefault((GameServerUI m) => m.Id == InvitedToServerId) != null);
			_invitedToServer = serverListElements.FirstOrDefault((GameServerUI m) => m.Id == InvitedToServerId && m.Hash == CombinedHash);
			yield return new WaitForSeconds(1f);
			InvitedToServerId = -1L;
			if (_invitedToServer == null)
			{
				StopInviteCoroutine();
				ShowMessageBox(Localization.ConnectionError, Localization.ConnectionToGameBroken);
				yield break;
			}
			yield return new WaitUntil(() => _invitedToServer.OnLine);
			ConnectToServer(_invitedToServer, InvitedToServerPassword);
			InviteScreen.SetActive(value: false);
			inviteCoroutine = null;
		}

		public void StopInviteCoroutine()
		{
			if (inviteCoroutine != null)
			{
				StopCoroutine(inviteCoroutine);
				inviteCoroutine = null;
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
			if (!gameExitWanted)
			{
				// Prevent the app from quitting...
				Application.wantsToQuit += () => { return false; };

				// ...and exit safely instead.
				ExitGame();
			}
			else
			{
				NetworkController.DisconnectImmediate();
			}
		}

		/// <summary>
		/// 	Method to exit the game safely. Handles shutting down connections to servers/clients properly, and saves settings.
		/// </summary>
		public void ExitGame()
		{
			gameExitWanted = true;
			QuitApplication();
		}

		private void QuitApplication()
		{
			LogCustomEvent("application_exit", flush: true);
			UpdateServers = false;
			IsRunning = false;
			OnDestroy();
			NetworkController.DisconnectImmediate();
			SteamAPI.Shutdown();
			Application.Quit();
		}

		private void OnDestroy()
		{
			NetworkController.EventSystem.RemoveListener(typeof(KillPlayerMessage), KillPlayerMessageListener);
			NetworkController.EventSystem.RemoveListener(typeof(LogOutResponse), LogOutResponseListener);
			NetworkController.EventSystem.RemoveListener(typeof(DestroyObjectMessage), DestroyObjectMessageListener);
			NetworkController.EventSystem.RemoveListener(typeof(PlayerSpawnResponse), PlayerSpawnResponseListener);
			NetworkController.EventSystem.RemoveListener(typeof(MovementMessage), MovementMessageListener);
			NetworkController.EventSystem.RemoveListener(typeof(PlayersOnServerResponse), PlayersOnServerResponseListener);
			NetworkController.EventSystem.RemoveListener(typeof(AvailableSpawnPointsResponse), AvailableSpawnPointsResponseListener);
			NetworkController.EventSystem.RemoveListener(typeof(ShipCollisionMessage), ShipCollisionMessageListener);
			NetworkController.EventSystem.RemoveListener(typeof(UpdateVesselDataMessage), UpdateVesselDataMessageListener);
			NetworkController.EventSystem.RemoveListener(EventSystem.InternalEventType.ShowMessageBox, ShowMessageBoxListener);
			NetworkController.EventSystem.RemoveListener(EventSystem.InternalEventType.OpenMainScreen, OpenMainScreneListener);
			NetworkController.EventSystem.RemoveListener(EventSystem.InternalEventType.ReconnectAuto, ReconnectAutoListener);
			NetworkController.EventSystem.RemoveListener(EventSystem.InternalEventType.RemoveLoadingCanvas, RemoveLoadingCanvasListener);
			NetworkController.EventSystem.RemoveListener(EventSystem.InternalEventType.ConnectionFailed, ConnectionFailedListener);
			NetworkController.EventSystem.RemoveListener(typeof(SignInResponse), SignInResponseListener);
			NetworkController.EventSystem.RemoveListener(typeof(LogInResponse), LogInResponseListener);
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
			Cursor.visible = ((!val.HasValue) ? (!Cursor.visible) : val.Value);
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

		public void ShowMessageBox(string title, string text, MessageBox.OnCloseDelegate onClose)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("UI/CanvasMessageBox")) as GameObject;
			transform.SetParent(CanvasManager.transform, false);
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
			if (gameExitWanted)
			{
				maxSecondsToWaitForExit -= Time.deltaTime;
				if (maxSecondsToWaitForExit <= 0f)
				{
					QuitApplication();
				}
			}
			if (SignInFailed)
			{
				SignInFailed = false;
				CanvasManager.SelectScreen(CanvasManager.Screen.OnLoad);
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
			LogCustomEvent("death", new Dictionary<string, object> {
			{
				"cause_of_death",
				killPlayerMessage.CauseOfDeath.ToString()
			} });
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
			NetworkController.SendToGameServer(new VesselRequest
			{
				GUID = obj.GUID,
				Time = time,
				RescueShipSceneID = sceneID,
				RescueShipTag = tag
			});
		}

		public void SendDistressCall(ArtificialBody obj, bool isDistressActive)
		{
			NetworkController.SendToGameServer(new DistressCallRequest
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
			NetworkController.SendToGameServer(new AvailableSpawnPointsRequest());
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

		private void FixedUpdate()
		{
			if (serverUpdateCounter < 5 && UpdateServers)
			{
				List<GameServerUI> list = (from m in ServerListContentPanel.GetComponentsInChildren<GameServerUI>()
					where !m.Disabled
					select m).ToList();
				if (list.Count > 0)
				{
					new Thread(UpdateServersThread).Start(list);
				}
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

		private void UpdateServersThread(object obj)
		{
			List<GameServerUI> list = obj as List<GameServerUI>;
			if (list.Count <= 0 || !UpdateServers)
			{
				return;
			}
			serverUpdateCounter++;
			GameServerUI gameServerUI = null;
			foreach (GameServerUI item in list)
			{
				if (!UpdateServers)
				{
					break;
				}
				if (item.Hash == CombinedHash && (gameServerUI == null || gameServerUI.LastUpdateTime > item.LastUpdateTime))
				{
					gameServerUI = item;
				}
			}
			if (gameServerUI != null && (DateTime.UtcNow - gameServerUI.LastUpdateTime).TotalSeconds > 5.0)
			{
				string steamId = string.Empty;
				if (SteamManager.Initialized)
				{
					steamId = SteamUser.GetSteamID().ToString();
				}
				gameServerUI.LastUpdateTime = DateTime.UtcNow;
				int latency = -1;
				ServerStatusRequest serverStatusRequest = new ServerStatusRequest();
				serverStatusRequest.SteamId = steamId;
				serverStatusRequest.SendDetails = gameServerUI.Description == null;
				ServerStatusRequest request = serverStatusRequest;
				ServerStatusResponse serverStatusResponse = SendRequest(request, gameServerUI.IPAddress, gameServerUI.StatusPort, out latency) as ServerStatusResponse;
				if (latency < 0 && gameServerUI.AltIPAddress != null && gameServerUI.AltIPAddress != string.Empty)
				{
					serverStatusResponse = SendRequest(request, gameServerUI.AltIPAddress, gameServerUI.AltStatusPort, out latency) as ServerStatusResponse;
					gameServerUI.UseAltIPAddress = latency >= 0;
				}
				if (latency >= 0 && serverStatusResponse != null)
				{
					if (serverStatusResponse.Description != null)
					{
						gameServerUI.Description = serverStatusResponse.Description;
					}
					gameServerUI.CharacterData = serverStatusResponse.CharacterData;
					gameServerUI.CurrentPlayers = serverStatusResponse.CurrentPlayers;
					gameServerUI.AlivePlayers = serverStatusResponse.AlivePlayers;
					gameServerUI.MaxPlayers = serverStatusResponse.MaxPlayers;
					gameServerUI.OnLine = true;
					gameServerUI.PingTime = latency;
				}
				else
				{
					gameServerUI.OnLine = false;
				}
			}
			serverUpdateCounter--;
		}

		private void SendRequest(NetworkData request, string address, int port, bool logException = false)
		{
			SendRequest(request, address, port, out var _, getResponse: false, logException);
		}

		private NetworkData SendRequest(NetworkData request, string address, int port, out int latency, bool getResponse = true, bool logException = false)
		{
			latency = -1;
			try
			{
				TcpClient tcpClient = new TcpClient();
				IAsyncResult asyncResult = tcpClient.BeginConnect(address, port, null, null);
				WaitHandle asyncWaitHandle = asyncResult.AsyncWaitHandle;
				try
				{
					if (!asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(4.0), exitContext: false))
					{
						tcpClient.Close();
						throw new TimeoutException();
					}
					tcpClient.EndConnect(asyncResult);
				}
				finally
				{
					asyncWaitHandle.Close();
				}
				NetworkStream networkStream = null;
				try
				{
					networkStream = tcpClient.GetStream();
				}
				catch
				{
				}
				networkStream.ReadTimeout = 1000;
				networkStream.WriteTimeout = 1000;
				string empty = string.Empty;
				if (SteamManager.Initialized)
				{
					empty = SteamUser.GetSteamID().ToString();
				}
				byte[] array = Serializer.Serialize(request);
				DateTime dateTime = DateTime.UtcNow.ToUniversalTime();
				networkStream.Write(array, 0, array.Length);
				networkStream.Flush();
				if (getResponse)
				{
					NetworkData result = Serializer.ReceiveData(networkStream);
					latency = (int)(DateTime.UtcNow - dateTime).TotalMilliseconds;
					return result;
				}
			}
			catch (Exception ex)
			{
				if (logException)
				{
					Dbg.Error(ex.Message, ex.StackTrace);
				}
			}
			return null;
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
				bool flag = false;
				foreach (ObjectTransform transform in movementMessage.Transforms)
				{
					flag = false;
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
			if (!(ab is SpaceObjectVessel))
			{
				return;
			}
			MapObject value = null;
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
					Discord.UpdateStatus();
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
			float x = Mathf.Clamp(trans.localPosition.x + ZeroGravity.UI.InputManager.GetAxis(ZeroGravity.UI.InputManager.AxisNames.LookHorizontal) * MouseSpeedOnPanels, 0f, panelWidth);
			float y = Mathf.Clamp(trans.localPosition.y + ZeroGravity.UI.InputManager.GetAxis(ZeroGravity.UI.InputManager.AxisNames.LookVertical) * MouseSpeedOnPanels * (float)((!Instance.InvertedMouse) ? 1 : (-1)), 0f, panelHeight);
			trans.localPosition = new Vector3(x, y, trans.localPosition.z);
		}

		public void ConnectToServer(GameServerUI server, string serverPassword = null)
		{
			_connectToServerCoroutine = ConnectToServerCoroutine(server, serverPassword);
			StartCoroutine(_connectToServerCoroutine);
		}

		/// <summary>
		/// 	Connect to a remote server.
		/// </summary>
		public IEnumerator ConnectToServerCoroutine(GameServerUI server, string serverPassword = null)
		{
			// Cancel if server is offline.
			if (!server.OnLine)
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
			if (server.CharacterData == null && newCharacterData == null)
			{
				CreateCharacterPanel.SetActive(value: true);
				CurrentGenderText.text = CurrentGender.ToLocalizedString();
				InventoryCharacterPreview.instance.ResetPosition();
				if (SteamManager.Initialized)
				{
					CharacterInputField.text = SteamFriends.GetFriendPersonaName(SteamUser.GetSteamID());
				}
				else
				{
					CharacterInputField.text = string.Empty;
				}
				CharacterInputField.Select();
				yield return new WaitWhile(() => CreateCharacterPanel.activeInHierarchy);
				string newCharacterName = CharacterInputField.text.Trim();
				CharacterInputField.text = string.Empty;
				if (newCharacterName.Length > 0)
				{
					newCharacterData = new CharacterData();
					newCharacterData.Name = newCharacterName;
					newCharacterData.Gender = CurrentGender;
					newCharacterData.HeadType = 1;
					newCharacterData.HairType = 1;
				}
			}

			// Check if server is still online.
			if (!server.OnLine)
			{
				ShowMessageBox(Localization.ConnectionError, Localization.ServerOffline);
				_connectToServerCoroutine = null;
				yield break;
			}

			// Prepare for connection.
			LastGameServersData = serverListElements;
			LastConnectedServer = server;
			CanvasManager.LoadingTips.text = ShuffledTexts.GetNextInLoop();
			CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.ConnectingToGame);
			LastConnectedServerPass = serverPassword;
			if (userName == string.Empty && LastSignInRequest != null)
			{
				userName = LastSignInRequest.SteamId;
			}
			this.InvokeRepeating(CheckLoadingComplete, 3f, 1f);

			// Connect to server.
			NetworkController.ConnectToGame(server, (!SteamManager.Initialized) ? userName : SteamUser.GetSteamID().m_SteamID.ToString(), newCharacterData, serverPassword);

			// Cleanup data.
			newCharacterData = null;
			UpdateServers = false;
			_invitedToServer = null;
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

		public void DeleteCharacterRequest(GameServerUI gs)
		{
			deleteChararacterFromServer = gs;
			ShowConfirmMessageBox(Localization.DeleteCharacter, Localization.AreYouSureDeleteCharacter, Localization.Yes, Localization.No, DeleteCharacter);
		}

		public void DeleteCharacter()
		{
			if (deleteChararacterFromServer == null)
			{
				return;
			}
			TcpClient tcpClient = new TcpClient();
			IAsyncResult asyncResult = (deleteChararacterFromServer.UseAltIPAddress ? tcpClient.BeginConnect(deleteChararacterFromServer.AltIPAddress, deleteChararacterFromServer.AltStatusPort, null, null) : tcpClient.BeginConnect(deleteChararacterFromServer.IPAddress, deleteChararacterFromServer.StatusPort, null, null));
			WaitHandle asyncWaitHandle = asyncResult.AsyncWaitHandle;
			try
			{
				if (!asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(4.0), exitContext: false))
				{
					tcpClient.Close();
					throw new TimeoutException();
				}
				tcpClient.EndConnect(asyncResult);
			}
			finally
			{
				asyncWaitHandle.Close();
			}
			NetworkStream networkStream = null;
			try
			{
				networkStream = tcpClient.GetStream();
				networkStream.ReadTimeout = 1000;
				networkStream.WriteTimeout = 1000;
				DeleteCharacterRequest deleteCharacterRequest = new DeleteCharacterRequest();
				deleteCharacterRequest.ServerId = deleteChararacterFromServer.Id;
				deleteCharacterRequest.SteamId = ((!SteamManager.Initialized) ? userName : SteamUser.GetSteamID().m_SteamID.ToString());
				byte[] array = Serializer.Serialize(deleteCharacterRequest);
				DateTime dateTime = DateTime.UtcNow.ToUniversalTime();
				networkStream.Write(array, 0, array.Length);
				networkStream.Flush();
				deleteChararacterFromServer.DeleteCharacter.gameObject.SetActive(value: false);
				deleteChararacterFromServer.CharacterNameText.text = null;
				deleteChararacterFromServer.CharacterData = null;
				deleteChararacterFromServer = null;
			}
			catch
			{
			}
		}

		/// <summary>
		/// 	Reconnect after we have been disconnected.
		/// </summary>
		public void Reconnect()
		{
			if (userName == string.Empty && LastSignInRequest != null)
			{
				userName = LastSignInRequest.SteamId;
			}
			this.InvokeRepeating(CheckLoadingComplete, 3f, 1f);
			NetworkController.ConnectToGame(LastConnectedServer, (!SteamManager.Initialized) ? userName : SteamUser.GetSteamID().m_SteamID.ToString(), newCharacterData, LastConnectedServerPass);
		}

		private void CreateServerButton(ServerData serverData)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(sampleServerButton, ServerListContentPanel);
			GameServerUI server = gameObject.GetComponent<GameServerUI>();

			// Assign variables.
			server.Id = serverData.Id;
			server.Name = serverData.Name;
			server.IPAddress = serverData.IPAddress;
			server.GamePort = serverData.GamePort;
			server.StatusPort = serverData.StatusPort;
			server.AltIPAddress = serverData.AltIPAddress;
			server.AltGamePort = serverData.AltGamePort;
			server.AltStatusPort = serverData.AltStatusPort;
			server.Locked = serverData.Locked;
			server.Hash = serverData.Hash;
			server.NameText.text = serverData.Name;
			server.PingText.text = Localization.UpdatingStatus;

			// Whether to show that the server is locked or not.
			server.Private.SetActive(serverData.Locked);

			// Add button to list.
			serverListElements.Add(server);

			// Add button to correct server catergory.
			if (serverData.Tag == ServerTag.Official)
			{
				server.FilterType = ServerCategories.Official;
			}
			else
			{
				server.FilterType = ServerCategories.Community;
			}

			// Check if the server hash matches our local hash.
			if (server.Hash != CombinedHash)
			{
				server.NameText.color = Colors.Gray;
				server.PingText.color = Colors.Gray;
				server.CharacterNameText.color = Colors.Gray;
				server.PlayerCount.color = Colors.Gray;
				server.PingText.text = Localization.Disabled.ToUpper();
				server.PingText.color = Colors.Gray;
			}

			// Add a click action to the favourite button.
			server.FavouriteButton.onClick.AddListener(delegate
			{
				ToggleFavouriteServer(server);
			});

			// Make the button visibly show as a favourite.
			bool existsInFavouriteServers = playerServersData.FavoriteServers.Contains(serverData.Id);
			server.IsFavoriteServer.Activate(existsInFavouriteServers);
			server.IsFavourite = existsInFavouriteServers;

			// Whether to show the button.
			bool shouldShow = CurrentServerFilter == server.FilterType || (CurrentServerFilter == ServerCategories.Favorites && server.IsFavourite);
			gameObject.SetActive(shouldShow);
			server.IsVisible = shouldShow;
		}

		public void ClearServerList()
		{
			foreach (GameServerUI item in serverListElements)
			{
				UnityEngine.Object.Destroy(item.Panel);
			}
			serverListElements = new ConcurrentBag<GameServerUI>();
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
				deleteChararacterFromServer = null;
				SolarSystem.Set(SolarSystemRoot.transform.Find("SunRoot"), SolarSystemRoot.transform.Find("PlanetsRoot"), logInResponse.ServerTime);
				SolarSystem.LoadDataFromResources();
				MyPlayer.SpawnMyPlayer(logInResponse);
				CanvasManager.SelectScreen(CanvasManager.Screen.Loading);
				if (logInResponse.IsAlive)
				{
					PlayerSpawnRequest playerSpawnRequest = new PlayerSpawnRequest();
					playerSpawnRequest.SpawPointParentID = 0L;
					NetworkController.SendToGameServer(playerSpawnRequest);
				}
				else if (InvitedToServerSpawnPointId != null)
				{
					PlayerSpawnRequest playerSpawnRequest2 = new PlayerSpawnRequest();
					playerSpawnRequest2.SpawPointParentID = InvitedToServerSpawnPointId.VesselGUID;
					PlayerSpawnRequest data2 = playerSpawnRequest2;
					NetworkController.SendToGameServer(data2);
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
				CanvasManager.SelectScreen(CanvasManager.Screen.CharacterSelect);
				ShowMessageBox(Localization.ConnectionError, Localization.WrongPassword);
			}
			else
			{
				Dbg.Info("Server dropped connection.");
				CanvasManager.SelectScreen(CanvasManager.Screen.CharacterSelect);
				ShowMessageBox(Localization.ConnectionError, Localization.ServerUnreachable);
			}
		}

		public void SignInButton()
		{
			StartCoroutine(SignInCoroutine());
		}

		public IEnumerator SignInCoroutine()
		{
			CanvasManager.SelectScreen(CanvasManager.Screen.Loading);
			yield return null;

			// Make sure that no single player games are running.
			KillAllSPProcesses();
			if (SteamManager.Initialized)
			{
				SinglePlayerMode = false;
				prevSortMode = -1;
				Connect();
			}
		}

		/// <summary>
		/// 	Sign in to the main server.
		/// </summary>
		public void Connect()
		{
			if (SteamManager.Initialized)
			{
				userName = SteamUser.GetSteamID().ToString();
			}
			string property = Properties.GetProperty("server_address", "188.166.144.65:6000");
			if (userName.Length > 0)
			{
				CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.ConnectingToMain);
				string[] array = property.Split(':');
				ServerIP = array[0];
				port = int.Parse(array[1]);
				NetworkController.MainServerAddres = ServerIP;
				NetworkController.MainServerPort = port;
				Regex regex = new Regex("[^0-9.]");
				SignInRequest signInRequest = new SignInRequest();
				signInRequest.SteamId = userName;
				signInRequest.ClientVersion = regex.Replace(Application.version, string.Empty);
				signInRequest.ClientHash = CombinedHash;
				LastSignInRequest = signInRequest;
				UpdateServers = false;
				try
				{
					NetworkController.SendToMainServer(signInRequest);

					// Response will be handled by the SignInResponseListener method.
				}
				catch (Exception)
				{
					ShowMessageBox(Localization.ConnectionError, Localization.ServerUnreachable);
				}
			}
			else
			{
				ShowMessageBox(Localization.ConnectionError, "Could not connect to Steam.");
			}
		}

		/// <summary>
		/// 	Check for errors and get server list.
		/// </summary>
		private void SignInResponseListener(NetworkData data)
		{
			SignInResponse signInResponse = data as SignInResponse;
			if (ReconnectAutomatically)
			{
				return;
			}
			if (signInResponse.Response == ResponseResult.Error)
			{
				Dbg.Warning("Unable to sign in", signInResponse.Message);
				CanvasManager.SelectScreen(CanvasManager.Screen.OnLoad);
				ShowMessageBox(Localization.ConnectionError, Localization.LogInError);
			}
			/*else if (signInResponse.Response == ResponseResult.OwnershipIssue)
			{
				// Anti-piracy stuff.
				Application.OpenURL("http://store.steampowered.com/app/588210/");
				ExitGame();
			}*/
			else if (signInResponse.Response == ResponseResult.ClientVersionError)
			{
				ShowMessageBox(Localization.VersionError, Localization.VersionErrorMessage);
			}
			else
			{
				currentResponse = signInResponse;
				CanvasManager.SelectScreen(CanvasManager.Screen.CharacterSelect);
				ClearServerList();

				Dbg.Log("Found " + signInResponse.Servers.Count + " servers.");
				foreach (ServerData value in signInResponse.Servers.Values)
				{
					CreateServerButton(value);
				}
				// Set order buttons by name.
				OrderByButton(0);
				UpdateServers = true;
			}
			LastSignInResponse = signInResponse;
			receivedSignInResponse = true;
		}

		public void ServerCategoryButton(int filter)
		{
			switch (filter)
			{
			case 0:
				CurrentServerFilter = ServerCategories.Official;
				foreach (GameServerUI item in serverListElements)
				{
					bool flag2 = item.FilterType == CurrentServerFilter;
					item.Panel.SetActive(flag2);
					item.IsVisible = flag2;
				}
				break;
			case 1:
				CurrentServerFilter = ServerCategories.Community;
				foreach (GameServerUI item2 in serverListElements)
				{
					bool flag = item2.FilterType == CurrentServerFilter;
					item2.Panel.SetActive(flag);
					item2.IsVisible = flag;
				}
				break;
			default:
				CurrentServerFilter = ServerCategories.Favorites;
				foreach (GameServerUI item3 in serverListElements)
				{
					item3.Panel.SetActive(item3.IsFavourite);
					item3.IsVisible = item3.IsFavourite;
				}
				break;
			}
			OfficialActive.SetActive(CurrentServerFilter == ServerCategories.Official);
			CommunityActive.SetActive(CurrentServerFilter == ServerCategories.Community);
			FavoritesActive.SetActive(CurrentServerFilter == ServerCategories.Favorites);
			SaveServerData();
		}

		private void SaveServerData()
		{
			playerServersData.PreviousFilter = CurrentServerFilter;
			Json.SerializePersistent(playerServersData, "ServersData.json");
		}

		public void ToggleFavouriteServer(GameServerUI server)
		{
			server.IsFavourite = !server.IsFavourite;
			if (server.IsFavourite)
			{
				server.IsFavoriteServer.Activate(value: true);
				playerServersData.FavoriteServers.Add(server.Id);
			}
			else
			{
				server.IsFavoriteServer.Activate(value: false);
				playerServersData.FavoriteServers.Remove(server.Id);
			}
			SaveServerData();
			if (CurrentServerFilter == ServerCategories.Favorites)
			{
				server.Panel.SetActive(server.IsFavourite);
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

		/// <summary>
		/// 	Orders the server list by the specified sort mode.<br />
		/// 	1 sorts by ping.<br />
		/// 	2 sorts by whether player has joined before.<br />
		/// 	3 sorts by name.<br />
		/// 	4 sorts by player count.<br />
		/// 	5 sorts by whether it is locked.<br />
		/// 	Everything else sorts by name.
		/// </summary>
		public void OrderByButton(int sortMode)
		{
			// If clicked again, reverse the sort order.
			if (prevSortMode == sortMode)
			{
				sortOrder *= -1;
			}

			// Assemble the list.
			List<GameServerUI> list = sortMode switch
			{
				1 => (from m in ServerListContentPanel.GetComponentsInChildren<GameServerUI>()
					where !m.Disabled
					orderby !m.OnLine, m.PingTime, !m.IsFavourite
					select m).ToList(),
				2 => (from m in ServerListContentPanel.GetComponentsInChildren<GameServerUI>()
					where !m.Disabled
					orderby m.CharacterData == null, !m.IsFavourite, m.name
					select m).ToList(),
				3 => (from m in ServerListContentPanel.GetComponentsInChildren<GameServerUI>()
					where !m.Disabled
					orderby !m.IsFavourite, m.name
					select m).ToList(),
				4 => (from m in ServerListContentPanel.GetComponentsInChildren<GameServerUI>()
					where !m.Disabled
					orderby m.CurrentPlayers, !m.IsFavourite, m.name
					select m).ToList(),
				5 => (from m in ServerListContentPanel.GetComponentsInChildren<GameServerUI>()
					where !m.Disabled
					orderby m.Locked, !m.IsFavourite, m.name
					select m).ToList(),
				_ => (from m in ServerListContentPanel.GetComponentsInChildren<GameServerUI>()
					where !m.Disabled
					orderby !m.IsFavourite, m.name
					select m).ToList(),
			};

			// Reverse the order if sort order is supposed to be reversed.
			if (sortOrder == -1)
			{
				list = Enumerable.Reverse(list).ToList();
			}
			list.AddRange((from m in ServerListContentPanel.GetComponentsInChildren<GameServerUI>()
				where m.Disabled
				orderby m.name
				select m).ToList());
			foreach (GameServerUI item in list)
			{
				Transform parent = item.transform.parent;
				item.transform.SetParent(null);
				item.transform.SetParent(parent);
			}
			if (sortMode == 0)
			{
				Invoke("FirstSort", 0.5f);
			}
			prevSortMode = sortMode;
		}

		private void FirstSort()
		{
			int num = 3;
			List<GameServerUI> list = (from m in ServerListContentPanel.GetComponentsInChildren<GameServerUI>()
				where !m.Disabled
				orderby !m.IsFavourite, m.name
				select m).ToList();
			list.AddRange((from m in ServerListContentPanel.GetComponentsInChildren<GameServerUI>()
				where m.Disabled
				orderby m.name
				select m).ToList());
			foreach (GameServerUI item in list)
			{
				Transform parent = item.transform.parent;
				item.transform.SetParent(null);
				item.transform.SetParent(parent);
			}
			prevSortMode = num;
		}

		public void SearchServersInput()
		{
			foreach (GameServerUI item in serverListElements)
			{
				if (item.NameText.text.ToLower().Contains(ServerSearchInputField.text.ToLower()) && (item.FilterType == CurrentServerFilter || (item.IsFavourite && CurrentServerFilter == ServerCategories.Favorites)))
				{
					item.Panel.SetActive(value: true);
				}
				else
				{
					item.Panel.SetActive(value: false);
				}
			}
			ServerListScrollBar.value = 1f;
		}

		public void SwitchCurrentGender()
		{
			if (CurrentGender == ZeroGravity.Network.Gender.Male)
			{
				CurrentGender = ZeroGravity.Network.Gender.Female;
			}
			else
			{
				CurrentGender = ZeroGravity.Network.Gender.Male;
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

		public void ProcessInvitation(InviteMessage inviteMessage)
		{
			if (processInvitationCoroutine != null)
			{
				StopCoroutine(processInvitationCoroutine);
			}
			processInvitationCoroutine = ProcessInvitationCoroutine(inviteMessage);
			StartCoroutine(processInvitationCoroutine);
		}

		public IEnumerator ProcessInvitationCoroutine(InviteMessage inviteMessage)
		{
			yield return new WaitUntil(() => !SceneLoader.IsPreloading);
			InvitedToServerId = inviteMessage.ServerId;
			InvitedToServerPassword = inviteMessage.Password;
			InvitedToServerSpawnPointId = inviteMessage.SpawnPointId;
			OpenMainScreen();
		}

		public static AnalyticsResult LogCustomEvent(string customEventName, bool flush = false)
		{
			AnalyticsResult result = Analytics.CustomEvent(customEventName);
			if (flush)
			{
				Analytics.FlushEvents();
			}
			return result;
		}

		public static AnalyticsResult LogCustomEvent(string customEventName, Vector3 position, bool flush = false)
		{
			AnalyticsResult result = Analytics.CustomEvent(customEventName, position);
			if (flush)
			{
				Analytics.FlushEvents();
			}
			return result;
		}

		public static AnalyticsResult LogCustomEvent(string customEventName, IDictionary<string, object> eventData, bool flush = false)
		{
			try
			{
				AnalyticsResult result = Analytics.CustomEvent(customEventName, eventData);
				if (flush)
				{
					Analytics.FlushEvents();
				}
				return result;
			}
			catch (Exception ex)
			{
				Dbg.Error(ex.Message, ex.StackTrace);
			}
			return AnalyticsResult.InvalidData;
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
			// Another anti-piracy check.
			if (!SteamManager.Initialized)
			{
				yield break;
			}

			// Enable loading screen.
			CanvasManager.SelectScreen(CanvasManager.Screen.Loading);
			yield return null;
			string filePath = SpServerPath + "\\" + spServerFileName;
			try
			{
				KillAllSPProcesses();
				_spServerProcess = new Process();
				_spServerProcess.StartInfo.WorkingDirectory = SpServerPath;
				_spServerProcess.StartInfo.FileName = filePath;
				string text = ((SinglePlayerGameMode != 0) ? "-configdir Sandbox " : string.Empty);
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
					_spServerProcess = null;
					throw new Exception("Process.Start function returned FALSE");
				}

				// Scary loop.
				int result = 6104;
				int result2 = 6105;
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
							int.TryParse(array2[0], out result);
							int.TryParse(array2[1], out result2);
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
				string steamId = string.Empty;
				if (SteamManager.Initialized)
				{
					steamId = SteamUser.GetSteamID().ToString();
				}
				int latency = -1;
				ServerStatusRequest serverStatusRequest = new ServerStatusRequest();
				serverStatusRequest.SteamId = steamId;
				serverStatusRequest.SendDetails = true;
				ServerStatusRequest request = serverStatusRequest;
				if (SendRequest(request, "127.0.0.1", result2, out latency) is ServerStatusResponse serverStatusResponse && serverStatusResponse.Response == ResponseResult.Success)
				{
					if (serverStatusResponse.CharacterData == null)
					{
						serverStatusResponse.CharacterData = new CharacterData
						{
							Name = SteamFriends.GetFriendPersonaName(SteamUser.GetSteamID()),
							Gender = ZeroGravity.Network.Gender.Male,
							HairType = 1,
							HeadType = 1
						};
					}
					NetworkController.ConnectToGameSP(result, SteamId, serverStatusResponse.CharacterData);
					this.InvokeRepeating(CheckLoadingComplete, 3f, 1f);
					yield break;
				}
				CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.None);
				throw new Exception("Unable to connect to SP server.");
			}
			catch (Exception ex)
			{
				Dbg.Error("Unable to start single player game", ex.Message, ex.StackTrace);
				CanvasManager.SelectSpawnPointScreen.SetActive(false);
				CanvasManager.SelectScreen(CanvasManager.Screen.OnLoad);
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
			return SpServerPath + ((SinglePlayerGameMode != 0) ? "\\Sandbox\\" : string.Empty);
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
			NetworkController.SendToGameServer(new ServerShutDownMessage
			{
				Restrat = false,
				CleanRestart = false
			});
		}

		public void QuickSave()
		{
			CanvasManager.TextChat.CreateMessage(null, "SAVING GAME.", local: false);
			NetworkController.SendToGameServer(new SaveGameMessage
			{
				FileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".save",
				AuxData = MyPlayer.Instance.GetSaveFileAuxData()
			});
		}

		public void AutoSave()
		{
			lastSPAutosaveTime = Time.time;
			CanvasManager.TextChat.CreateMessage(null, "SAVING GAME.", local: false);
			NetworkController.SendToGameServer(new SaveGameMessage
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
					where !m.HasExited && m.MainModule.FileName.ToLower().EndsWith(spServerFileName.ToLower())
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
			lastLatencyMessateTime = Time.realtimeSinceStartup;
			new Task(delegate
			{
				int latency;
				if (LastConnectedServer.UseAltIPAddress)
				{
					SendRequest(new LatencyTestMessage(), LastConnectedServer.AltIPAddress, LastConnectedServer.AltStatusPort, out latency);
				}
				else
				{
					SendRequest(new LatencyTestMessage(), LastConnectedServer.IPAddress, LastConnectedServer.StatusPort, out latency);
				}
				_LatencyMs = latency;
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
			AkSoundEngine.SetRTPCValue(SoundManager.instance.InGameVolume, 1f);
			MyPlayer.Instance.PlayerReady = true;
			Instance.Discord.UpdateStatus();
			MyPlayer.Instance.InitializeCameraEffects();
			if (Instance.SinglePlayerMode)
			{
				LogCustomEvent("game_start_sp_" + SinglePlayerGameMode.ToString().ToLower());
				Instance.CanvasManager.TextChat.CreateMessage(null, "Use [F5] to quicksave and [F9] to quickload game.", local: false);
			}
			else
			{
				LogCustomEvent("game_start");
			}
			Instance.CanvasManager.SelectSpawnPointScreen.Activate(value: false);
			IsInGame = true;
			lastSPAutosaveTime = Time.time;
			ToggleCursor(false);
			CanvasManager.ToggleCanvasUI(val: true);
			CanvasManager.ToggleTextChatCanvas(val: true);
			MainMenuRoot.gameObject.SetActive(value: false);
			CanvasManager.LoadingTips.text = string.Empty;
			CanvasManager.SelectScreen(CanvasManager.Screen.None);
			StartCoroutine(FixPlayerInCryo());
			_loadingFinishedTask.RunSynchronously();
			EnvironmentReady = true;
			NetworkController.SendToGameServer(new EnvironmentReadyMessage());
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

		public void ChangeStatsByIfNotAdmin<T>(SteamStatID id, T value)
		{
			try
			{
				if (!MyPlayer.Instance.IsAdmin)
				{
					SteamStats.ChangeStatBy(id, value);
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

		public void LogReceivedNetworkData(Type type)
		{
			receivedNetworkDataLog.Enqueue(new Tuple<float, Type>((float)(DateTime.UtcNow.ToUniversalTime() - clientStartTime).TotalSeconds, type));
			while (receivedNetworkDataLog.Count > maxNetworkDataLogsSize)
			{
				receivedNetworkDataLog.TryDequeue(out var _);
			}
		}

		public void LogSentNetworkData(Type type)
		{
			sentNetworkDataLog.Enqueue(new Tuple<float, Type>((float)(DateTime.UtcNow.ToUniversalTime() - clientStartTime).TotalSeconds, type));
			while (sentNetworkDataLog.Count > maxNetworkDataLogsSize)
			{
				sentNetworkDataLog.TryDequeue(out var _);
			}
		}

		public string GetNetworkDataLogs()
		{
			Tuple<float, Type>[] source = receivedNetworkDataLog.ToArray();
			float lastRecvdTime = source.Last().Item1;
			IEnumerable<Tuple<float, Type>> recvd = source.Where((Tuple<float, Type> m) => lastRecvdTime - m.Item1 <= 300f);
			float item = recvd.First().Item1;
			string text = "Received packets (in last " + (lastRecvdTime - item).ToString("0") + "s):\n";
			text += string.Join("\n", from z in (from x in recvd.Select((Tuple<float, Type> m) => m.Item2).Distinct()
					select new Tuple<string, int>(x.Name, recvd.Count((Tuple<float, Type> n) => n.Item2 == x)) into y
					orderby y.Item2
					select y).Reverse()
				select z.Item1 + ": " + z.Item2);
			Tuple<float, Type>[] source2 = sentNetworkDataLog.ToArray();
			float lastSentTime = source2.Last().Item1;
			IEnumerable<Tuple<float, Type>> sent = source2.Where((Tuple<float, Type> m) => lastSentTime - m.Item1 <= 300f);
			float item2 = sent.First().Item1;
			text = text + "\n\nSent packets (in last " + (lastSentTime - item2).ToString("0") + "s):\n";
			return text + string.Join("\n", from z in (from x in sent.Select((Tuple<float, Type> m) => m.Item2).Distinct()
					select new Tuple<string, int>(x.Name, sent.Count((Tuple<float, Type> n) => n.Item2 == x)) into y
					orderby y.Item2
					select y).Reverse()
				select z.Item1 + ": " + z.Item2);
		}
	}
}
