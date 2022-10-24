using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Steamworks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity
{

	// TODO: Clean up this file. I want to rewrite this to add a new central system controlling
	//       the shown menu, using an enum with a gameobject to define the menu.
	public class CanvasManager : MonoBehaviour
	{
		public delegate void OnSpawnPointClicked(SpawnPointDetails details);

		public enum Screen
		{
			None,
			OnLoad,
			CharacterSelect,
			ChooseShip,
			Loading
		}

		public enum LoadingScreenType
		{
			None,
			Loading,
			ConnectingToMain,
			ConnectingToGame
		}

		public enum StartingPointOption
		{
			StandardGameMod,
			SandboxGameMod,
			NewGame,
			FreshStart,
			Continue,
			Invite,
			CustomStartingPoint,
			Eva,
			StrandedMiner,
			Soe
		}

		[HideInInspector]
		public Canvas Canvas;

		[Header("Start sceen")]
		public SplashScreen SplashScreen;

		public GameObject OnLoadPanel;

		public GameObject Disclamer;

		public static volatile bool ShowDisclamer = true;

		public Text Version;

		[Header("Loading screen")]
		public GameObject LoadingCanvas;

		public Text LoadingScreenText;

		public Text LoadingTips;

		[Header("Dead screen")]
		public GameObject DeadScreen;

		public GameObject PressAnyKey;

		public Text DeadMsgText;

		public GameObject DisconectScreen;

		[Header("Character and server screen")]
		public GameObject CharacterAndServerGroup;

		public GameServerUI currentlySelectedServer;

		[Header("Pop up")]
		public GameObject CanvasMessageBox;

		public ReportServerUI ReportServerBox;

		public GameObject ConfirmMessageBox;

		[Header("Spawn point selection screen")]
		public List<StartingPointOptionData> StartingPointData = new List<StartingPointOptionData>();

		public GameObject SpawnOptionsScreen;

		public Transform SpawnOptions;

		public Transform FreshStartSpawnOptions;

		public StartingPointOptionUI StartingPointUI;

		public GameObject SelectSpawnPointScreen;

		public Transform SpawnPointsHolder;

		public SpawnPointOptionUI SpawnPointOptionUI;

		public RawImage CurrentSpawnPointScreenshot;

		public Text CurrentSpawnPointDescription;

		public bool CanChooseSpown = true;

		[Header("Single player")]
		public GameObject SinglePlayerModeScreen;

		public GameObject SinglePlayerDeadOptions;

		[Header("In game screens")]
		public GameMenu GameMenuScript;

		public CanvasUI CanvasUI;

		public GameObject InGameMenuCanvas;

		public GameObject ReportServerInGame;

		public InteractionCanvas InteractionCanvas;

		public Chat TextChat;

		public GameObject TextChatCanvas;

		public GameObject HitCanvas;

		public GameObject BusyLoadingInfo;

		public GameObject ScreenShootMod;

		[Header("Sound")]
		public SoundEffect SoundEffect;

		[Header("My player")]
		public PlayerOverview PlayerOverview;

		private float hitCanvasTime;

		private bool hitCanvasVisible;

		public InventoryUI InventoryUI;

		public bool OverlayCanvasIsOn;

		public bool IsInputFieldIsActive;

		public bool ShowTutorial;

		public bool DisableChat;

		public bool ShowTips;

		public bool ShowCrosshair;

		public bool AutoStabilization;

		public Material HighlightSlotMaterial;

		public Texture HighlightSlotNormal;

		public Texture HighlightSlotItemHere;

		private float showDeadMsgTime;

		public bool IsBusyLoading;

		public bool IsConfirmBoxActive;

		public HelmetOverlayModel HelmetOverlayModel;

		[Header("Console")]
		public InGameConsole Console;

		[Header("Connection notifications")]
		public Image Latency;

		[Header("Quick tips")]
		public bool DefaultInteractionTipSeen;

		public GameObject DefaultInteractionTip;

		public GameObject QuickTipHolder;

		public Text QuickTipContent;

		private int busyLoadingActiveCount;

		public bool IsPlayerOverviewOpen => PlayerOverview.gameObject.activeInHierarchy;

		public bool IsGameMenuOpen => InGameMenuCanvas.activeInHierarchy;

		public bool ConsoleIsUp => Console.gameObject.activeInHierarchy;

		private void Start()
		{
			Canvas = GetComponent<Canvas>();

			// Localize text in child objects.
			foreach (Text text in GetComponentsInChildren<Text>(true))
			{
				string value = null;
				if (Localization.CanvasManagerLocalization.TryGetValue(text.name, out value))
				{
					text.text = value;
				}
			}

			if (Client.IsLogout || Client.IsDisconected)
			{
				SelectScreen(Screen.CharacterSelect);
			}
			else if (Client.ReconnectAutomatically || Client.ForceRespawn)
			{
				ToggleLoadingScreen(LoadingScreenType.ConnectingToGame);
			}
			else
			{
				SelectScreen(Screen.OnLoad);
			}

			Cursor.visible = true;
			Disclamer.SetActive(ShowDisclamer);
			if (ShowDisclamer)
			{
				SplashScreen.StartVideo(0);
			}
			else
			{
				SplashScreen.gameObject.Activate(value: false);
				Client.Instance.SceneLoader.InitializeScenes();
				Client.Instance.AmbientSounds.SwitchAmbience("MainMenu");
				Client.Instance.AmbientSounds.Play(0);
			}
			CanvasUI.gameObject.Activate(value: false);
			ScreenShootMod.Activate(value: false);
			Version.text = string.Format(Localization.ClientVersion, Application.version);
			StartingPointData = Resources.LoadAll<StartingPointOptionData>("StartingPoints").ToList();
		}

		private void Update()
		{
			// Exiting the splash screen is handled by its own script.
			if (SplashScreen.VideoPlayer.isPlaying)
			{
				return;
			}

			if (hitCanvasVisible)
			{
				hitCanvasTime += Time.deltaTime;
				if (hitCanvasTime >= 0.1f)
				{
					hitCanvasTime = 0f;
					HitCanvas.SetActive(value: false);
					hitCanvasVisible = false;
				}
			}

			// Close the disclaimer.
			if (Disclamer.activeInHierarchy && Input.anyKeyDown)
			{
				if (!InputManager.GetKeyDown(KeyCode.Mouse0) && !InputManager.GetKeyDown(KeyCode.Mouse1))
				{
					DisclaimerAgree();
				}
				return;
			}
			if (InputManager.GetKeyDown(KeyCode.KeypadEnter) || InputManager.GetKeyDown(KeyCode.Return))
			{
				if (IsConfirmBoxActive || ReportServerBox.gameObject.activeInHierarchy || CanvasMessageBox.activeInHierarchy)
				{
					return;
				}
				if (OnLoadPanel.activeInHierarchy && !InGameMenuCanvas.activeInHierarchy)
				{
					Client.Instance.SignInButton();
				}
				else if (Client.Instance.PasswordEnterPanel.activeInHierarchy)
				{
					Client.Instance.EnterPasswordMenu(true);
				}
				else if (Client.Instance.CreateCharacterPanel.activeInHierarchy)
				{
					Client.Instance.CreateCharacterButton();
				}
				else if (CharacterAndServerGroup.activeInHierarchy && currentlySelectedServer != null)
				{
					Client.Instance.ConnectToServer(currentlySelectedServer);
				}
			}

			// If esc is clicked.
			if (InputManager.GetKeyDown(KeyCode.Escape))
			{
				if (IsConfirmBoxActive || ReportServerBox.gameObject.activeInHierarchy || InGameMenuCanvas.activeInHierarchy || CanvasMessageBox.activeInHierarchy)
				{
					return;
				}
				if (ScreenShootMod.activeInHierarchy)
				{
					ToggleScreenShootMod();
					return;
				}
				if (Client.Instance.IsInGame && !OverlayCanvasIsOn && InputManager.GetKeyDown(KeyCode.Escape))
				{
					if (IsPlayerOverviewOpen)
					{
						PlayerOverview.Toggle(val: false);
					}
				}
				else if (Client.Instance.PasswordEnterPanel.activeInHierarchy)
				{
					Client.Instance.EnterPasswordMenu(false);
				}
				else if (Client.Instance.CreateCharacterPanel.activeInHierarchy)
				{
					Client.Instance.CreateCharacterExit();
				}
				else if (CharacterAndServerGroup.activeInHierarchy)
				{
					BackButton();
				}
				else if (SpawnOptionsScreen.activeInHierarchy)
				{
					ExitSpawnOptionsScreen();
				}
			}
			if (ScreenShootMod.activeInHierarchy && Input.GetKeyDown(KeyCode.F11))
			{
				StartCoroutine(TakeScreenShoot());
			}
			if (Client.Instance.IsInGame && MyPlayer.Instance != null && Input.GetKeyDown(KeyCode.F10))
			{
				ToggleScreenShootMod();
			}
			if (Client.Instance.IsInGame && InputManager.GetKeyDown(KeyCode.F2) && !Client.Instance.IsChatOpened && !IsInputFieldIsActive && MyPlayer.Instance.IsAdmin)
			{
				if (Console.gameObject.activeInHierarchy)
				{
					Console.Close();
				}
				else
				{
					Console.Open();
				}
			}
			if (DeadScreen.activeInHierarchy && Input.anyKey && Time.time - showDeadMsgTime > 3f && !Client.Instance.SinglePlayerMode)
			{
				ToggleDeadMsg(val: false);
			}
			else if (DeadScreen.activeInHierarchy && Client.Instance.SinglePlayerMode && Input.GetKeyDown(KeyCode.Escape))
			{
				ToggleDeadMsg(val: false);
			}
		}

		/// <summary>
		/// 	Open the ingame menu.
		/// </summary>
		public void OpenInGameMenu()
		{
			ToggleInGameMenuCanvas(val: true);
			Client.Instance.ToggleCursor(true);
			MyPlayer.Instance.FpsController.ToggleCameraController(false);
			MyPlayer.Instance.FpsController.ToggleCameraMovement(false);
			MyPlayer.Instance.FpsController.ToggleMovement(false);
			if (Client.Instance.IsChatOpened)
			{
				TextChat.CloseChat();
			}
		}

		public void DisclaimerAgree()
		{
			ShowDisclamer = false;
			Disclamer.SetActive(ShowDisclamer);
			Client.Instance.AmbientSounds.SwitchAmbience("MainMenu");
			Client.Instance.AmbientSounds.Play(0);
		}

		public void SelectGameMode(int mode)
		{
			CurrentSpawnPointDescription.text = Localization.FreshStart;
			Client.SinglePlayerGameMode = (Client.SPGameMode) mode;
			ShowSingleplayerSaves();
		}

		public void ShowSingleplayerSaves()
		{
			// Enable menu.
			SelectSpawnPointScreen.SetActive(value: true);

			// Clear all entries.
			SpawnPointsHolder.DestroyAll<SpawnPointOptionUI>();

			// Create default entry for creating new game.
			// TODO: Shouldn't this be a static gui element?
			SpawnPointOptionUI newWorldEntry = GameObject.Instantiate(SpawnPointOptionUI, SpawnPointsHolder);
			newWorldEntry.CreateNewGameButton();

			DirectoryInfo directoryInfo = new DirectoryInfo(Client.Instance.GetSPPath());
			bool flag = true;
			foreach (FileInfo item in from m in directoryInfo.GetFiles("*.save") orderby m.LastWriteTime descending select m)
			{
				SpawnPointOptionUI spawnPointOptionUI2 = GameObject.Instantiate(SpawnPointOptionUI, SpawnPointsHolder);
				spawnPointOptionUI2.SaveFile = item;
				spawnPointOptionUI2.CreateSaveGameButton();
				flag = false;
			}

			if (flag)
			{
				Client.Instance.PlayNewSPGame();
			}
		}

		public void VisitForumLink()
		{
			Application.OpenURL("https://www.playhellion.com/forum/");
		}

		public void RentYourServerLink()
		{
			Application.OpenURL("https://nitra.do/hellion");
			Client.LogCustomEvent("mb_rent_server");
		}

		public void DiscordButton()
		{
			Application.OpenURL("https://discord.gg/qZKJS5R");
		}

		public void GamepediaButton()
		{
			Application.OpenURL("https://hellion.gamepedia.com/Hellion_Wiki");
		}

		public void ToggleInGameMenuCanvas(bool val)
		{
			InGameMenuCanvas.SetActive(val);
			if (Client.Instance.IsInGame)
			{
				Client.Instance.ToggleCursor(true);
				GameMenuScript.MainMenu(val);
				if (!MyPlayer.Instance.FpsController.IsZeroG)
				{
					MyPlayer.Instance.FpsController.ResetVelocity();
				}
				if (MyPlayer.Instance != null && MyPlayer.Instance.CurrentActiveItem != null)
				{
					MyPlayer.Instance.CurrentActiveItem.PrimaryReleased();
				}
			}
		}

		public void TogglePlayerDisconect(bool val)
		{
			if (val)
			{
				Client.Instance.CanvasManager.CanvasUI.QuestCutScene.OnCutSceneFinished();
				Client.Instance.InGamePanels.Detach();
				DisconectScreen.SetActive(value: true);
				StartCoroutine(invokeDeath());
				Client.IsDisconected = true;
			}
			else
			{
				Client.Instance.OpenMainScreen();
			}
		}

		private IEnumerator invokeDeath()
		{
			yield return new WaitForSeconds(4f);
			TogglePlayerDisconect(val: false);
		}

		public void ToggleDeadMsg(bool val)
		{
			if (val)
			{
				Client.Instance.CanvasManager.CanvasUI.QuestCutScene.OnCutSceneFinished();
				Client.Instance.InGamePanels.Detach();
				showDeadMsgTime = Time.time;
				DeadScreen.SetActive(value: true);
				if (MyPlayer.Instance != null)
				{
					MyPlayer.Instance.FpsController.ToggleMovement(false);
					MyPlayer.Instance.FpsController.ToggleCameraMovement(false);
				}
				SinglePlayerDeadOptions.SetActive(Client.Instance.SinglePlayerMode);
				if (Client.Instance.IsChatOpened)
				{
					TextChat.CloseChat();
				}
				if (IsPlayerOverviewOpen)
				{
					PlayerOverview.Toggle(val: false);
				}
				if (IsGameMenuOpen)
				{
					ToggleInGameMenuCanvas(val: false);
				}
				if (Console.gameObject.activeInHierarchy)
				{
					Console.gameObject.SetActive(value: false);
				}
				Client.Instance.InGamePanels.Detach();
				if (Client.Instance.SinglePlayerMode)
				{
					Client.Instance.ToggleCursor(true);
					Client.Instance.InputModule.ToggleCustomCursorPosition(val: false);
				}
				PressAnyKey.Activate(!Client.Instance.SinglePlayerMode);
			}
			else
			{
				showDeadMsgTime = 0f;
				if (!Client.Instance.SinglePlayerMode)
				{
					Client.ReconnectAutomatically = true;
				}
				DeadScreen.SetActive(value: false);
				ToggleLoadingScreen(LoadingScreenType.ConnectingToMain);
				Client.Instance.OpenMainScreen();
			}
		}

		public void ToggleCanvasMessageBox(bool val)
		{
			CanvasMessageBox.SetActive(val);
		}

		public void ToggleCanvasUI(bool val)
		{
			if (ScreenShootMod.activeInHierarchy)
			{
				ScreenShootMod.Activate(value: false);
			}
			CanvasUI.gameObject.SetActive(val);
		}

		public void ToggleTextChatCanvas(bool val)
		{
			TextChatCanvas.SetActive(val);
		}

		public void ToggleLoadingScreen(LoadingScreenType type)
		{
			LoadingCanvas.Activate(type != LoadingScreenType.None);
			LoadingScreenText.text = type.ToLocalizedString();
			Client.Instance.LastLoadingTipsChangeTime = Time.realtimeSinceStartup;
		}

		public MessageBox GetMessageBox()
		{
			return CanvasMessageBox.GetComponent<MessageBox>();
		}

		public void ShowFreshStartOptions()
		{
			SpawnOptions.gameObject.Activate(value: false);
			FreshStartSpawnOptions.gameObject.Activate(value: true);
		}

		private void InstantiateFreshStartOptions()
		{
			FreshStartSpawnOptions.DestroyAll<StartingPointOptionUI>();
			StartingPointOptionUI startingPointOptionUI = GameObject.Instantiate(StartingPointUI, FreshStartSpawnOptions);
			startingPointOptionUI.Type = StartingPointOption.FreshStart;
			startingPointOptionUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				SplashScreen.FreshStart(CreateFreshStartTask(SpawnSetupType.Start1));
			});
			SteamStats.GetAchievement(SteamAchievementID.quest_sound_of_silence, out var achieved);
			StartingPointOptionUI startingPointOptionUI2 = GameObject.Instantiate(StartingPointUI, FreshStartSpawnOptions);
			startingPointOptionUI2.Type = StartingPointOption.StrandedMiner;
			startingPointOptionUI2.GetComponent<Button>().interactable = achieved || Client.Instance.ExperimentalBuild;
			startingPointOptionUI2.GetComponent<Button>().onClick.AddListener(delegate
			{
				SplashScreen.FreshStart(CreateFreshStartTask(SpawnSetupType.Start2));
			});
			SteamStats.GetAchievement(SteamAchievementID.quest_shattered_dreams, out achieved);
			StartingPointOptionUI startingPointOptionUI3 = GameObject.Instantiate(StartingPointUI, FreshStartSpawnOptions);
			startingPointOptionUI3.Type = StartingPointOption.Eva;
			startingPointOptionUI3.GetComponent<Button>().interactable = achieved || Client.Instance.ExperimentalBuild;
			startingPointOptionUI3.GetComponent<Button>().onClick.AddListener(delegate
			{
				SplashScreen.FreshStart(CreateFreshStartTask(SpawnSetupType.Start3));
			});
			SteamStats.GetAchievement(SteamAchievementID.quest_heart_of_stone, out achieved);
			StartingPointOptionUI startingPointOptionUI4 = GameObject.Instantiate(StartingPointUI, FreshStartSpawnOptions);
			startingPointOptionUI4.Type = StartingPointOption.Soe;
			startingPointOptionUI4.GetComponent<Button>().interactable = achieved || Client.Instance.ExperimentalBuild;
			startingPointOptionUI4.GetComponent<Button>().onClick.AddListener(delegate
			{
				SplashScreen.FreshStart(CreateFreshStartTask(SpawnSetupType.Start4));
			});
		}

		private static Task CreateFreshStartTask(SpawnSetupType tip)
		{
			return new Task(delegate
			{
				Client.Instance.SendSpawnRequest(new SpawnPointDetails
				{
					SpawnSetupType = tip,
					IsPartOfCrew = false,
					PlayersOnShip = new List<string>()
				});
			});
		}

		public void ShowSpawnPoints(List<SpawnPointDetails> spawnPoints, OnSpawnPointClicked onSpawnClicked, bool canContinue)
		{
			Client.ForceRespawn = false;
			FreshStartSpawnOptions.gameObject.Activate(value: false);
			SpawnOptions.DestroyAll<StartingPointOptionUI>();
			SpawnOptions.gameObject.Activate(value: true);
			SelectSpawnPointScreen.SetActive(value: false);
			SpawnPointsHolder.DestroyAll<SpawnPointOptionUI>();
			for (int i = 0; i < spawnPoints.Count; i++)
			{
				InstantiateSpawnButton(spawnPoints[i], i, onSpawnClicked);
			}
			StartingPointOptionUI startingPointOptionUI = GameObject.Instantiate(StartingPointUI, SpawnOptions);
			startingPointOptionUI.Type = StartingPointOption.NewGame;
			if (canContinue)
			{
				startingPointOptionUI.GetComponent<Button>().onClick.AddListener(delegate
				{
					OnFreshStartConfirm();
				});
			}
			else
			{
				startingPointOptionUI.GetComponent<Button>().onClick.AddListener(delegate
				{
					ShowFreshStartOptions();
				});
			}
			if (!Client.Instance.SinglePlayerMode)
			{
				StartingPointOptionUI startingPointOptionUI2 = GameObject.Instantiate(StartingPointUI, SpawnOptions);
				startingPointOptionUI2.Type = StartingPointOption.Continue;
				SpawnPointDetails continueSP = new SpawnPointDetails
				{
					SpawnSetupType = SpawnSetupType.Continue,
					IsPartOfCrew = false,
					PlayersOnShip = new List<string>()
				};
				startingPointOptionUI2.GetComponent<Button>().onClick.AddListener(delegate
				{
					onSpawnClicked(continueSP);
				});
				startingPointOptionUI2.GetComponent<Button>().interactable = canContinue;
			}
			StartingPointOptionUI startingPointOptionUI3 = GameObject.Instantiate(StartingPointUI, SpawnOptions);
			startingPointOptionUI3.Type = ((!Client.Instance.SinglePlayerMode) ? StartingPointOption.Invite : StartingPointOption.CustomStartingPoint);
			startingPointOptionUI3.GetComponent<Button>().interactable = spawnPoints.Count > 0;
			startingPointOptionUI3.GetComponent<Button>().onClick.AddListener(delegate
			{
				SelectSpawnPointScreen.SetActive(value: true);
			});
			InstantiateFreshStartOptions();
		}

		private void InstantiateSpawnButton(SpawnPointDetails spawnPoint, int posIndex, OnSpawnPointClicked onSpawnClicked)
		{
			SpawnPointOptionUI spawnPointOptionUI = GameObject.Instantiate(SpawnPointOptionUI, SpawnPointsHolder);
			string text = ((!spawnPoint.Name.IsNullOrEmpty()) ? spawnPoint.Name : spawnPoint.SpawnSetupType.ToLocalizedString());
			spawnPointOptionUI.Name.text = text + ((spawnPoint.PlayersOnShip == null || spawnPoint.PlayersOnShip.Count <= 0) ? string.Empty : ("\n" + string.Join(", ", spawnPoint.PlayersOnShip.ToArray())));
			spawnPointOptionUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				onSpawnClicked(spawnPoint);
			});
			spawnPointOptionUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				SelectSpawnPointScreen.SetActive(value: false);
			});
			if (Client.Instance.SinglePlayerMode)
			{
				spawnPointOptionUI.Description = Localization.GetLocalizedField(spawnPoint.SpawnSetupType.ToString() + "Description");
				spawnPointOptionUI.Screenshot = Client.Instance.SpriteManager.GetTexture(spawnPoint.SpawnSetupType);
			}
			else
			{
				spawnPointOptionUI.Description = Localization.InvitePending;
				spawnPointOptionUI.Screenshot = Client.Instance.SpriteManager.InviteTexture;
			}
		}

		private void OnFreshStartConfirm()
		{
			Client.Instance.ShowConfirmMessageBox(Localization.FreshStartConfrimTitle, Localization.FreshStartConfrimText, Localization.Yes, Localization.No, delegate
			{
				ShowFreshStartOptions();
			});
		}

		public void ShowInteractionCanvasMessage(string text, float hideTime = 1f)
		{
			InteractionCanvas.gameObject.SetActive(value: true);
			InteractionCanvas.ShowCanvas(text, hideTime);
		}

		public void SelectScreen(Screen screen)
		{
			switch (screen)
			{
			case Screen.None:
				OnLoadPanel.SetActive(value: false);
				CharacterAndServerGroup.SetActive(value: false);
				SpawnOptionsScreen.SetActive(value: false);
				ToggleLoadingScreen(LoadingScreenType.None);
				break;
			case Screen.OnLoad:
				OnLoadPanel.SetActive(value: true);
				CharacterAndServerGroup.SetActive(value: false);
				SpawnOptionsScreen.SetActive(value: false);
				ToggleLoadingScreen(Client.SinglePlayerQuickLoad ? LoadingScreenType.Loading : LoadingScreenType.None);
				break;
			case Screen.CharacterSelect:
				CharacterAndServerGroup.SetActive(value: true);
				OnLoadPanel.SetActive(value: false);
				SpawnOptionsScreen.SetActive(value: false);
				ToggleLoadingScreen(LoadingScreenType.None);
				Client.IsLogout = false;
				Client.IsDisconected = false;
				currentlySelectedServer = null;
				break;
			case Screen.ChooseShip:
				OnLoadPanel.SetActive(value: false);
				CharacterAndServerGroup.SetActive(value: false);
				SinglePlayerModeScreen.SetActive(value: false);
				SpawnOptionsScreen.SetActive(value: true);
				ToggleLoadingScreen(LoadingScreenType.None);
				CanChooseSpown = true;
				break;
			case Screen.Loading:
				OnLoadPanel.SetActive(value: false);
				CharacterAndServerGroup.SetActive(value: false);
				SinglePlayerModeScreen.SetActive(value: false);
				SpawnOptionsScreen.SetActive(value: false);
				ToggleLoadingScreen(LoadingScreenType.Loading);
				break;
			}
		}

		/// <summary>
		/// 	The singleplayer button in the main menu.
		/// </summary>
		public void PlaySP()
		{
			SelectScreen(Screen.None);
			SinglePlayerModeScreen.SetActive(value: true);
		}

		/// <summary>
		/// 	The multiplayer button in the main menu.
		/// </summary>
		public void PlayMP()
		{
			Client.Instance.SignInButton();
		}

		/// <summary>
		/// 	The settings button in the main menu.
		/// </summary>
		public void SettingsButton()
		{
			ToggleInGameMenuCanvas(val: true);
			GameMenuScript.SettingsMenu(isUp: true);
		}

		/// <summary>
		/// 	The quit button in the main menu.
		/// </summary>
		public void QuitButton()
		{
			Client.Instance.ExitGame();

		#if UNITY_EDITOR
			// Exit play mode if we are in the editor.
			Dbg.Info("Exiting play mode...");
			EditorApplication.isPlaying = false;
		#endif
		}

		/// <summary>
		/// 	Used to exit the server list and character select menu.
		/// </summary>
		public void BackButton()
		{
			Client.Instance.ClearServerList();
			Client.Instance.UpdateServers = false;
			SelectScreen(Screen.OnLoad);
			Client.Instance.NetworkController.Disconnect();
		}

		/// <summary>
		/// 	Handle exiting the spawn options screen.
		/// </summary>
		public void ExitSpawnOptionsScreen()
		{
			// Go back to general spawn options (new game, continue, invite).
			if (FreshStartSpawnOptions.gameObject.activeSelf)
			{
				SpawnOptions.gameObject.SetActive(true);
				FreshStartSpawnOptions.gameObject.SetActive(false);
			}
			else
			{
				// Exit screen completely, and go back to server list.
				SelectScreen(Screen.CharacterSelect);
			}
		}

		public void BeingHit()
		{
			HitCanvas.SetActive(value: true);
			hitCanvasTime = 0f;
			hitCanvasVisible = true;
		}

		public void ToggleBusyLoading(bool isActive)
		{
			if (Client.SceneLoadType == Client.SceneLoadTypeValue.PreloadWithCopy)
			{
				isActive = false;
			}
			if (isActive)
			{
				busyLoadingActiveCount++;
			}
			else
			{
				busyLoadingActiveCount--;
			}
			if (busyLoadingActiveCount > 0)
			{
				BusyLoadingInfo.SetActive(value: true);
				IsBusyLoading = true;
			}
			else
			{
				BusyLoadingInfo.SetActive(value: false);
				busyLoadingActiveCount = 0;
				IsBusyLoading = false;
			}
		}

		public void ReportServerFromMenu()
		{
			ReportServerBox.ActivateBox(null);
		}

		public void SaveGameOption()
		{
			InGameMenuCanvas.GetComponent<GameMenu>().ResumeButton();
			StartCoroutine(QuickSave());
		}

		private IEnumerator QuickSave()
		{
			yield return new WaitForSeconds(0.5f);
			Client.Instance.QuickSave();
		}

		public void LoadGameOption()
		{
			Client.Instance.ShowConfirmMessageBox(Localization.LoadGame, Localization.AreYouSureLoad, Localization.Yes, Localization.No, ConfirmLoadGame);
		}

		private void ConfirmLoadGame()
		{
			Client.Instance.QuickLoad();
		}

		public void ToggleScreenShootMod()
		{
			ScreenShootMod.Activate(!ScreenShootMod.activeInHierarchy);
			CanvasUI.gameObject.Activate(!CanvasUI.gameObject.activeInHierarchy);
		}

		public IEnumerator TakeScreenShoot()
		{
			yield return null;
			ScreenShootMod.Activate(value: false);
			yield return new WaitForEndOfFrame();
			Texture2D tex = new Texture2D(UnityEngine.Screen.width, UnityEngine.Screen.height, TextureFormat.RGB24, mipChain: false);
			tex.ReadPixels(new Rect(0f, 0f, UnityEngine.Screen.width, UnityEngine.Screen.height), 0, 0, recalculateMipMaps: false);
			tex.Apply(updateMipmaps: false);
			byte[] bytes = tex.GetRawTextureData();
			GameObject.Destroy(tex);
			byte[] flipped = new byte[bytes.Length];
			for (int i = 0; i < tex.height; i++)
			{
				for (int j = 0; j < tex.width * 3; j++)
				{
					flipped[i * tex.width * 3 + j] = bytes[(tex.height - i - 1) * tex.width * 3 + j];
				}
			}
			SteamScreenshots.WriteScreenshot(flipped, (uint)flipped.Length, tex.width, tex.height);
			ScreenShootMod.Activate(value: true);
		}

		public void QuickTip(string msg)
		{
			QuickTipHolder.Activate(value: false);
			QuickTipContent.text = msg;
			QuickTipHolder.SetActive(value: true);
		}

		public void ToggleDefaultInteractionTip()
		{
			if (!DefaultInteractionTipSeen && Client.Instance.CanvasManager.ShowTips)
			{
				DefaultInteractionTip.GetComponentInChildren<Text>(includeInactive: true).text = string.Format(Localization.PressToInteract, InputManager.GetAxisKeyName(InputManager.AxisNames.F)).ToUpper();
				DefaultInteractionTip.Activate(value: true);
				DefaultInteractionTipSeen = true;
			}
		}
	}
}
