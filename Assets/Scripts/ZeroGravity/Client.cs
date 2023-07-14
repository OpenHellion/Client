using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
using OpenHellion.RichPresence;
using OpenHellion.Networking;
using OpenHellion.Networking.Message.MainServer;
using OpenHellion.Networking.Message;
using OpenHellion.IO;
using OpenHellion;
using UnityEngine.InputSystem;
using OpenHellion.Social;
using Debug = UnityEngine.Debug;

namespace ZeroGravity
{
	/// TODO: This class needs to be decoupled. It doesn't follow the SOLID principles at all.
	/// Perhaps a lot of this can be decoupled into a GameState class?
	/// <summary>
	/// 	This is the main component of the game. It handles everything from saving to managing parts of the GUI.<br/>
	/// 	Avoid referencing this class if you're able. Make classes as self-contained as possible.
	/// </summary>
	/// <remarks>
	/// 	This class is just a mess of different functions. It has no explicit function, other than to be a class to reference all other important classes.<br/>
	/// </remarks>
	/// <seealso cref="MyPlayer"/>
	public class Client : MonoBehaviour
	{
		public enum SPGameMode
		{
			Standard,
			Sandbox
		}

		private readonly string _spServerPath = Directory.GetCurrentDirectory() + "\\Hellion_Data\\HELLION_SP";

		private const string SpServerFileName = "HELLION_SP.exe";

		private const int AutosaveInterval = 600;

		public static uint NetworkDataHash = ClassHasher.GetClassHashCode(typeof(NetworkData));

		public static uint SceneDataHash = ClassHasher.GetClassHashCode(typeof(ISceneData));

		public static uint CombinedHash = NetworkDataHash * SceneDataHash;

		[Title("Config")]
		public bool ExperimentalBuild;

		[Multiline(2)]
		public string ExperimentalText;

		public bool AllowLoadingOldSaveGames;

		public float MouseSpeedOnPanels = 30f;

		public int ControlsVersion = 2;

		public bool InvertMouseWhileDriving;

		public float RCS_THRUST_SENSITIVITY = 0.5f;

		public float RCS_ROTATION_SENSITIVITY = 5f;

		public static double CELESTIAL_BODY_RADIUS_MULTIPLIER = 1.0;

		public static float DROP_THRESHOLD = 0.2f;

		public static float DROP_MIN_FORCE = 0f;

		public static float DROP_MAX_FORCE = 8f;

		public static float DROP_MAX_TIME = 3f;

		public static float VESSEL_ROTATION_LERP_VALUE = 0.9f;

		public static bool VESSEL_ROTATION_LERP_UNCLAMPED = false;

		public static float VESSEL_TRANSLATION_LERP_VALUE = 0.8f;

		public static bool VESSEL_TRANSLATION_LERP_UNCLAMPED = false;

		public float DefaultCameraFov = 75f;

		public float SpecialCameraFov = 40f;

		public IEnumerator<string> ShuffledTexts;

		public List<DebrisField> DebrisFields = new List<DebrisField>();

		public Texture2D DefaultCursor;

		[NonSerialized]
		public Gender CurrentGender;

		[Title("Object references")]
		public GameObject ExperimentalGameObject;

		public InputField CharacterInputField;

		public GameObject CreateCharacterPanel;

		public Text CurrentGenderText;

		public MainMenuSceneController MainMenuSceneController;

		public SolarSystem SolarSystem;

		public CanvasManager CanvasManager;

		public GameObject SolarSystemRoot;

		public GameObject ShipExteriorRoot;

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

		public GameObject MainMenuRoot;

		public GameObject InviteScreen;

		public DebrisFieldEffect DebrisEffect;

		public EffectPrefabs EffectPrefabs;

		public SpriteManager SpriteManager;

		public InGameCanvasManager InGamePanels;

		public QuestCollectionObject QuestCollection;

		[NonSerialized]
		public volatile bool LogoutRequestSent;

		[NonSerialized]
		public volatile bool LogInResponseReceived;

		private bool _gameExitWanted;

		private float _maxSecondsToWaitForExit = 3f;

		private bool _environmentReady;

		private bool _receivedSignInResponse;

		[NonSerialized]
		public Dictionary<long, OtherPlayer> Players = new Dictionary<long, OtherPlayer>();

		[NonSerialized]
		public Dictionary<long, DynamicObject> DynamicObjects = new Dictionary<long, DynamicObject>();

		[NonSerialized]
		public Dictionary<long, Corpse> Corpses = new Dictionary<long, Corpse>();

		[NonSerialized]
		public Dictionary<long, SpaceObjectVessel> ActiveVessels = new Dictionary<long, SpaceObjectVessel>();

		[NonSerialized]
		public List<ItemIngredientsData> ItemsIngredients;

		[NonSerialized]
		public List<Quest> Quests = new List<Quest>();

		[NonSerialized]
		public float LastMovementMessageTime;

		[NonSerialized]
		public int LoadingScenesCount;

		[NonSerialized]
		public float LastLoadingTipsChangeTime;

		// TODO: Move to correct class.
		[NonSerialized]
		public bool OffSpeedHelper = true;

		private bool _openMainSceneStarted;

		[NonSerialized]
		public readonly Dictionary<long, CharacterInteractionState> CharacterInteractionStatesQueue = new Dictionary<long, CharacterInteractionState>();

		public static volatile bool ForceRespawn = false;

		public static volatile bool IsLogout = false;

		public static volatile bool IsDisconnected = false;

		[NonSerialized]
		public DateTime? ServerRestartTime;

		private static Process _spServerProcess;

		private float _lastSpAutosaveTime;

		private CharacterData _newCharacterData;

		// Can't these two be done with the underlying network transport?
		private float _lastLatencyMessageTime = -1f;
		private volatile int _latencyMs;

		public static volatile int MainThreadID;

		private IEnumerator _processInvitationCoroutine;

		private IEnumerator _inviteCoroutine;

		public static bool ReconnectAutomatically;

		public static ServerData LastConnectedServer;

		public static string InvitedToServerId;

		public static VesselObjectID InvitedToServerSpawnPointId;

		private IEnumerator _connectToServerCoroutine;

		private Task _loadingFinishedTask;

		private Task _restoreMapDetailsTask;

		[NonSerialized]
		public bool SinglePlayerMode;

		public static bool SinglePlayerQuickLoad;

		public static bool SinglePlayerRespawn;

		public static SPGameMode SinglePlayerGameMode;

		[NonSerialized]
		public int CurrentLanguageIndex;

		[NonSerialized]
		public double ExposureRange;

		[NonSerialized]
		public float[] VesselExposureValues;

		[NonSerialized]
		public float[] PlayerExposureValues;

		/// <summary>
		/// 	If we have loaded up a save or joined a server, and is in game.<br/>
		/// 	True when game is started.
		/// </summary>
		[NonSerialized]
		public bool IsInGame;

		[NonSerialized]
		public bool InvertedMouse;

		[NonSerialized]
		public bool IsChatOpened;

		public string UserId { get; private set; }

		public string Username { get; private set; }

		public NakamaClient Nakama { get; private set; }

		public SceneLoader SceneLoader { get; private set; }

		private static Client _instance;
		public static Client Instance => _instance;

		public static bool IsGameBuild => Instance != null;

		// TODO: Move these three to settings perhaps?
		private float _headbobStrength;
		public float HeadbobStrength
		{
			get
			{
				return _headbobStrength;
			}
			set
			{
				_headbobStrength = value;
				if (MyPlayer.Instance != null)
				{
					MyPlayer.Instance.FpsController.HeadBobStrength = _headbobStrength;
				}
			}
		}

		private int _antialiasingOption;
		public int AntialiasingOption
		{
			get
			{
				return _antialiasingOption;
			}
			set
			{
				_antialiasingOption = value;
				if (MyPlayer.Instance != null)
				{
					MyPlayer.Instance.InitializeCameraEffects();
				}
			}
		}

		private bool _volumetricOption;
		public bool VolumetricOption
		{
			get
			{
				return _volumetricOption;
			}
			set
			{
				_volumetricOption = value;
				if (MyPlayer.Instance != null)
				{
					MyPlayer.Instance.InitializeCameraEffects();
				}
			}
		}

		public static int DefaultLayerMask => 1 << LayerMask.NameToLayer("Default");

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


		private async void Awake()
		{
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
			_openMainSceneStarted = false;

			Nakama = FindObjectOfType<NakamaClient>();
			SceneLoader = FindObjectOfType<SceneLoader>();

			Texture[] emblems = Resources.LoadAll<Texture>("Emblems");
			SceneVesselEmblem.Textures = emblems.ToDictionary(x => x.name, y => y);

			UserId = await Nakama.GetUserId();
			Username = await Nakama.GetUsername();
		}

		private void Start()
		{
			ShuffledTexts = Localization.PreloadText.OrderBy(m => MathHelper.RandomNextDouble()).ToList().GetEnumerator();
			if (ExperimentalGameObject != null)
			{
				if (ExperimentalBuild)
				{
					ExperimentalGameObject.SetActive(value: true);
					ExperimentalGameObject.GetComponentInChildren<Text>().text = ExperimentalText.Trim() + " " + Application.version;
					PresenceManager.SetAchievement(AchievementID.other_testing_squad_member);
				}
				else
				{
					ExperimentalGameObject.SetActive(value: false);
				}
			}
			if (Properties.GetProperty("save_default_localization_file", defaultValue: false))
			{
				Localization.SaveToFile("localization_default.txt");
			}

			string customLocalisationFile = Properties.GetProperty("custom_localization_file", string.Empty);
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
			else if (customLocalisationFile != string.Empty)
			{
				Localization.ImportFromFile(customLocalisationFile);
			}
			else
			{
				Localization.RevertToDefault();
			}
			InGamePanels.LocalizePanels();
			EventSystem.AddListener(typeof(LogInResponse), LogInResponseListener);
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
				InvitedToServerSpawnPointId = null;
				if (IsLogout || IsDisconnected)
				{
					CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.ConnectingToMain);
					CanvasManager.SelectScreen(CanvasManager.Screen.CharacterSelect);
				}
				if (ReconnectAutomatically)
				{
					if (SinglePlayerRespawn)
					{
						SinglePlayerQuickLoad = false;
						PlaySingleplayer();
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
				PlaySingleplayer();
			}
		}

		public string GetInviteString(VesselObjectID spawnPointId)
		{
			InviteMessage inviteMessage = new InviteMessage
			{
				Time = Time.time,
				ServerId = LastConnectedServer.Id,
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
					Dbg.Log("Starting main scene load, Ship");
					StartCoroutine(LoadMainScenesCoroutine(s, ship, s.VesselObjects));
				}
				else if (s.ParentType == SpaceObjectType.Asteroid)
				{
					Asteroid asteroid = Asteroid.Create(s.ParentTransform, s.VesselData, isMainObject: true);
					asteroid.gameObject.SetActive(value: true);
					MyPlayer.Instance.Parent = asteroid;
					Dbg.Log("Starting main scene load, Asteroid");
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
				KillAllSpProcesses();
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
			if (!_openMainSceneStarted)
			{
				_openMainSceneStarted = true;
				InGamePanels.Detach();
				ToggleCursor(true);
				if (MyPlayer.Instance is not null)
				{
					Destroy(MyPlayer.Instance.gameObject);
				}
				SceneManager.LoadScene("Client", LoadSceneMode.Single);
				if (MainMenuSceneController is not null)
				{
					MainMenuSceneController.gameObject.SetActive(value: true);
				}
			}
		}

		public void ConnectionFailedListener(EventSystem.InternalEventData data)
		{
			CanvasManager.SelectScreen(CanvasManager.Screen.CharacterSelect);
			ReconnectAutomatically = false;
			StartMultiplayer(true);
		}

		public void CloseAllLoadingScreensListener(EventSystem.InternalEventData data)
		{
			if (CanvasManager.StartingPointScreen.activeInHierarchy)
			{
				CanvasManager.ExitStartingPointScreen();
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

			StartMultiplayer();
			CanvasManager.Disclamer.SetActive(value: false);

			yield return new WaitUntil(() => _receivedSignInResponse);

			InvitedToServerId = null;
			if (LastConnectedServer == null)
			{
				StopInviteCoroutine();
				ShowMessageBox(Localization.ConnectionError, Localization.ConnectionToGameBroken);
				yield break;
			}

			ConnectToServer(LastConnectedServer);
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
				KillAllSpProcesses();
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
			OnDestroy();
			NetworkController.Instance.Disconnect();
			HiResTime.Stop();
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

		public void OnInGameMenu(bool val)
		{
			if (IsInGame)
			{
				ToggleCursor(val);
				MyPlayer.Instance.FpsController.ToggleCameraController(!val);
				MyPlayer.Instance.FpsController.ToggleCameraMovement(!val);
				MyPlayer.Instance.FpsController.ToggleMovement(!val);
			}
		}

		/// <summary>
		/// 	Toggles the visibility and lock state of the cursor. No value inverts the current value.
		/// </summary>
		public void ToggleCursor(bool? val = null)
		{
			Cursor.visible = !val.HasValue ? !Cursor.visible : val.Value;
			if (!Cursor.visible)
			{
				Cursor.lockState = CursorLockMode.Locked;
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
			}
		}

		public void OnApplicationFocus(bool focusStatus)
		{
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
			Dbg.Log("Showing message box with text: " + text);
			GameObject gameObject = Instantiate(Resources.Load("UI/CanvasMessageBox")) as GameObject;
			gameObject.transform.SetParent(parent.transform, false);
			gameObject.GetComponent<RectTransform>().Reset(resetScale: true);
			gameObject.SetActive(true);
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
			if (MyPlayer.Instance != null && MyPlayer.Instance.IsAlive && !CanvasManager.InGameMenuCanvas.activeInHierarchy && AutosaveInterval > 0 && SinglePlayerMode && _lastSpAutosaveTime + (float)AutosaveInterval < Time.time)
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
			if (MyPlayer.Instance is not null && MyPlayer.Instance.PlayerReady && _environmentReady)
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
					if (item4 is SpaceObjectVessel && !(item4 as SpaceObjectVessel).IsDebrisFragment && (spaceObjectVessel == null || (num > num3 && (spaceObjectVessel.FTLEngine == null || spaceObjectVessel.FTLEngine.Status != SystemStatus.Online || (spaceObjectVessel.Velocity - MyPlayer.Instance.Parent.Velocity).SqrMagnitude < 900.0))))
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
					PresenceManager.UpdateStatus();
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
			float x = Mathf.Clamp(trans.localPosition.x + Mouse.current.delta.x.ReadValue() * Instance.MouseSpeedOnPanels, 0f, panelWidth);
			float y = Mathf.Clamp(trans.localPosition.y + Mouse.current.delta.y.ReadValue() * Instance.MouseSpeedOnPanels * (float)((!Instance.InvertedMouse) ? 1 : (-1)), 0f, panelHeight);
			trans.localPosition = new Vector3(x, y, trans.localPosition.z);
		}

		public void ConnectToServer(ServerData server)
		{
			_connectToServerCoroutine = ConnectToServerCoroutine(server);
			StartCoroutine(_connectToServerCoroutine);
		}

		/// <summary>
		/// 	Connect to a remote server.
		/// </summary>
		public IEnumerator ConnectToServerCoroutine(ServerData server)
		{

			// Cancel if server is offline.
			if (!server.Online)
			{
				ShowMessageBox(Localization.ConnectionError, Localization.ServerOffline);
				_connectToServerCoroutine = null;
				yield break;
			}

			// TODO: Make this a function call to a class for the CreateCharacterPanel and get the result from there.
			// Create new character if you don't have it from before.
			if (server.CharacterData is null && _newCharacterData is null)
			{
				CreateCharacterPanel.SetActive(value: true);
				CurrentGenderText.text = CurrentGender.ToLocalizedString();
				InventoryCharacterPreview.instance.ResetPosition();
				CharacterInputField.text = Username;
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
			this.InvokeRepeating(CheckLoadingComplete, 3f, 1f);

			// Connect to server.
			NetworkController.Instance.ConnectToGame(server, _newCharacterData);

			// Cleanup data.
			_newCharacterData = null;
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
			ShowConfirmMessageBox(Localization.DeleteCharacter, Localization.AreYouSureDeleteCharacter, Localization.Yes, Localization.No, async delegate
			{
				DeleteCharacterRequest deleteCharacterRequest = new DeleteCharacterRequest
				{
					ServerId = gs.Id,
					PlayerId = NetworkController.PlayerId
				};

				await NetworkController.SendTcp(deleteCharacterRequest, gs.IpAddress, gs.StatusPort, false, true);
			});
		}

		/// <summary>
		/// 	Reconnect after we have been disconnected.
		/// </summary>
		public void Reconnect()
		{
			this.InvokeRepeating(CheckLoadingComplete, 3f, 1f);
			NetworkController.Instance.ConnectToGame(LastConnectedServer, _newCharacterData);
		}

		private void LogInResponseListener(NetworkData data)
		{
			LogInResponse logInResponse = data as LogInResponse;
			LogInResponseReceived = true;

			// If we're reconnecting we don't need to initalise game state again.
			if (ReconnectAutomatically)
			{
				ReconnectAutomatically = false;
			}

			if (logInResponse.Response == ResponseResult.Success)
			{
				Dbg.Log("Logged into game.");

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
			else
			{
				Dbg.Error("Server dropped connection.");
				CanvasManager.SelectScreen(CanvasManager.Screen.MainMenu);
				ShowMessageBox(Localization.ConnectionError, Localization.NoServerConnection);
			}
		}

		public void SignInButton()
		{
			CanvasManager.SelectScreen(CanvasManager.Screen.Loading);
			SinglePlayerMode = false;
			StartMultiplayer();
		}

		/// <summary>
		/// 	Get server to connect to from the main server, then start multiplayer game.
		/// </summary>
		private void StartMultiplayer(bool reconnecting = false)
		{
			CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.SigningIn);

			// Reconnecting is handled elsewhere, so we just cancel the rest of this code.
			if (ReconnectAutomatically)
			{
				return;
			}

			// If signing in for the first time this session, get server we should connect to.
			ServerData connectingServerData = LastConnectedServer;
			if (!reconnecting)
			{
				Regex regex = new Regex("[^0-9.]");
				SignInRequest signInRequest = new SignInRequest()
				{
					Version = regex.Replace(Application.version, string.Empty),
					Hash = CombinedHash,
					JoiningId = InvitedToServerId
				};

				SignInResponse signInResponse;
				try
				{
					//  TODO: Send connection message to Nakama.
					// signInResponse = await Nakama.DoSomething();
					signInResponse = new SignInResponse();
				}
				catch (Exception)
				{
					ShowMessageBox(Localization.ConnectionError, Localization.NoNakamaConnection);
					OpenMainScreen();
					return;
				}

				// TODO: Move this to InitialisingScene.
				//if (signInResponse.Result is ResponseResult.ServerNotFound)
				//{
				//	CanvasManager.SelectScreen(CanvasManager.Screen.MainMenu);
				//	ShowMessageBox(Localization.ConnectionError, Localization.NoServerConnection);
				//}
				//else if (signInResponse.Result is ResponseResult.ClientVersionError)
				//{
				//	CanvasManager.SelectScreen(CanvasManager.Screen.MainMenu);
				//	ShowMessageBox(Localization.VersionError, Localization.VersionErrorMessage);
				//}
				//else
				{
					connectingServerData = new()
					{
						Id = signInResponse.Server.Id,
						IpAddress = signInResponse.Server.IpAddress,
						GamePort = signInResponse.Server.GamePort,
						StatusPort = signInResponse.Server.StatusPort,
						Hash = signInResponse.Server.Hash,
						Online = true
					};
				}
			}

			CanvasManager.SelectScreen(CanvasManager.Screen.CharacterSelect);

			// Connect to server!
			ConnectToServer(connectingServerData);
			LastConnectedServer = connectingServerData;
			_receivedSignInResponse = true;
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
			InvitedToServerSpawnPointId = inviteMessage.SpawnPointId;
			OpenMainScreen();
		}

		/// <summary>
		/// 	Start singleplayer server, and connect to it.
		/// </summary>
		public async void PlaySingleplayer(string saveFilePath = null)
		{
			// Enable loading screen.
			CanvasManager.SelectScreen(CanvasManager.Screen.Loading);

			// Wait a little so the loading screen shows. TODO: Unity will have it's own way of doing this with Awaitable.NextFrameAsync.
			await Task.Delay(5);

			string executablePath = _spServerPath + "\\" + SpServerFileName;

			try
			{
				KillAllSpProcesses();
				_spServerProcess = new Process();
				_spServerProcess.StartInfo.WorkingDirectory = _spServerPath;
				_spServerProcess.StartInfo.FileName = executablePath;
				string singlePlayerModeLaunchArgument = (SinglePlayerGameMode != SPGameMode.Standard)
					? "-configdir Sandbox "
					: string.Empty;
				if (SinglePlayerQuickLoad)
				{
					_spServerProcess.StartInfo.Arguments = singlePlayerModeLaunchArgument;
				}
				else if (SinglePlayerRespawn || saveFilePath == null)
				{
					_spServerProcess.StartInfo.Arguments = singlePlayerModeLaunchArgument + "-clean";
				}
				else
				{
					_spServerProcess.StartInfo.Arguments = singlePlayerModeLaunchArgument + "-load " + saveFilePath;
				}

				_spServerProcess.StartInfo.UseShellExecute = false;
				_spServerProcess.StartInfo.RedirectStandardOutput = true;
				_spServerProcess.StartInfo.CreateNoWindow = true;
				if (!_spServerProcess.Start())
				{
					_spServerProcess = null;
					throw new Exception("Process.Start function returned FALSE");
				}

				Dbg.Log("Started single player server process.");

				int gamePort = 6104;
				int statusPort = 6105;

				// Wait until server is ready to give us connection ports.
				// Run this async as not to block the current thread.
				// TODO: Need to replace this at some point.
				await Task.Run(() =>
				{
					while (true)
					{
						string lineText = _spServerProcess.StandardOutput.ReadLine();
						if (lineText == null || lineText.ToLower().EndsWith("ready"))
						{
							break;
						}

						if (lineText.ToLower().StartsWith("ports:"))
						{
							try
							{
								string[] array = lineText.Split(':');
								string[] array2 = array[1].Split(',');
								int.TryParse(array2[0], out gamePort);
								int.TryParse(array2[1], out statusPort);
							}
							catch
							{
								// Ignored.
							}
						}
						Thread.Sleep(50);
					}
				});

				SinglePlayerQuickLoad = false;
				SinglePlayerRespawn = false;
				SinglePlayerMode = true;
				_lastSpAutosaveTime = Time.time;

				ServerStatusRequest serverStatusRequest = new ServerStatusRequest
				{
					PlayerId = NetworkController.PlayerId,
					SendDetails = true
				};

				ServerStatusResponse response = await NetworkController.SendTcp(serverStatusRequest, "127.0.0.1", statusPort, true, true) as ServerStatusResponse;

				Debug.Assert(response != null);
				if (response.Response == ResponseResult.Success)
				{
					// Create new character if it doesn't exist.
					if (response.CharacterData is null)
					{
						response.CharacterData = new CharacterData
						{
							Name = Username,
							Gender = Gender.Male,
							HairType = 1,
							HeadType = 1
						};
					}

					NetworkController.Instance.ConnectToGameSP(gamePort, response.CharacterData);

					this.InvokeRepeating(CheckLoadingComplete, 3f, 1f);

					Dbg.Log("Successfully connected to singleplayer server!");
					return;
				}

				throw new Exception("Unable to connect to SP server.");
			}
			catch (Exception ex)
			{
				Dbg.Error("Unable to start single player game", ex.Message, ex.StackTrace);
				CanvasManager.SaveAndSpawnPointScreen.SetActive(false);
				CanvasManager.SelectScreen(CanvasManager.Screen.MainMenu);
				if (File.Exists(executablePath))
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
			return _spServerPath + ((SinglePlayerGameMode != SPGameMode.Standard) ? "\\Sandbox\\" : string.Empty);
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
			_lastSpAutosaveTime = Time.time;
			CanvasManager.TextChat.CreateMessage(null, "SAVING GAME.", local: false);
			NetworkController.Instance.SendToGameServer(new SaveGameMessage
			{
				FileName = "autosave.save",
				AuxData = MyPlayer.Instance.GetSaveFileAuxData()
			});
		}

		private async void KillAllSpProcesses()
		{
			try
			{
				if (_spServerProcess is not null && !_spServerProcess.HasExited)
				{
					_spServerProcess.Kill();
					_spServerProcess = null;
				}


				await Task.Run(() =>
				{
					List<Process> list = (from m in Process.GetProcesses()
						where !m.HasExited && m.MainModule.FileName.EndsWith(SpServerFileName, true, null)
						select m).ToList();

					foreach (Process item in list)
					{
						item.Kill();
					}
				});
			}
			catch (Exception ex)
			{
				Dbg.Warning("Exception when killing all single player processes.", ex.Message);
			}
		}

		public async void LatencyTestMessage()
		{
			_lastLatencyMessageTime = Time.realtimeSinceStartup;

			int latency = await NetworkController.LatencyTest(LastConnectedServer.IpAddress, LastConnectedServer.StatusPort);
			_latencyMs = latency;

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
			else if (LastLoadingTipsChangeTime + 5f < Time.realtimeSinceStartup) // Change loading tips.
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
			PresenceManager.UpdateStatus();
			MyPlayer.Instance.InitializeCameraEffects();

			Instance.CanvasManager.SaveAndSpawnPointScreen.Activate(value: false);
			IsInGame = true;
			_lastSpAutosaveTime = Time.time;
			ToggleCursor(false);
			CanvasManager.ToggleCanvasUI(val: true);
			CanvasManager.ToggleTextChatCanvas(val: true);
			MainMenuRoot.SetActive(value: false);
			CanvasManager.LoadingTips.text = string.Empty;
			CanvasManager.SelectScreen(CanvasManager.Screen.None);
			StartCoroutine(FixPlayerInCryo());
			_loadingFinishedTask.RunSynchronously();
			_environmentReady = true;
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
			if (updateVesselDataMessage is null || updateVesselDataMessage.VesselsDataUpdate is null)
			{
				return;
			}
			foreach (VesselDataUpdate item in updateVesselDataMessage.VesselsDataUpdate)
			{
				SpaceObjectVessel spaceObjectVessel = SolarSystem.GetArtificialBody(item.GUID) as SpaceObjectVessel;
				if (spaceObjectVessel is not null && spaceObjectVessel.VesselData is not null)
				{
					if (item.VesselName is not null)
					{
						spaceObjectVessel.VesselData.VesselName = item.VesselName;
					}
					if (item.VesselRegistration is not null)
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
