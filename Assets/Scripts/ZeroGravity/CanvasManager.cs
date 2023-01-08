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
using OpenHellion.ProviderSystem;
using OpenHellion.Networking;
using TriInspector;

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
			MainMenu,
			StartingPoint,
			SpawnOptions,
			Loading
		}

		public enum LoadingScreenType
		{
			None,
			Loading,
			ConnectingToMain,
			FindingPlayer,
			NewPlayer,
			SigningIn,
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

		[Title("Start sceen")]
		public SplashScreen SplashScreen;

		public GameObject MainMenu;

		public GameObject Disclamer;

		public static volatile bool ShowDisclamer = true;

		public Text Version;

		[Title("Loading screen")]
		public GameObject LoadingCanvas;

		public Text LoadingScreenText;

		public Text LoadingTips;

		[Title("Dead screen")]
		public GameObject DeadScreen;

		public GameObject PressAnyKey;

		public Text DeadMsgText;

		public GameObject DisconectScreen;

		[Title("Character screen")]
		public GameObject CharacterGroup;

		[Title("Pop up")]
		public GameObject CanvasMessageBox;

		public ReportServerUI ReportServerBox;

		public GameObject ConfirmMessageBox;

		[Title("Spawn point selection screen")]
		[ReadOnly]
		[ListDrawerSettings(Draggable = false, HideAddButton = true, HideRemoveButton = true, AlwaysExpanded = true)]
		public List<StartingPointOptionData> StartingPointData = new List<StartingPointOptionData>();

		public GameObject StartingPointScreen;

		public Transform SpawnOptions;

		public Transform FreshStartSpawnOptions;

		public StartingPointOptionUI StartingPointUI;

		public GameObject SelectSpawnPointScreen;

		public Transform SpawnPointsHolder;

		public SpawnPointOptionUI SpawnPointOptionUI;

		public RawImage CurrentSpawnPointScreenshot;

		public Text CurrentSpawnPointDescription;

		public bool CanChooseSpawn = true;

		[Title("Single player")]
		public GameObject SinglePlayerModeScreen;

		public GameObject SinglePlayerDeadOptions;

		[Title("In game screens")]
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

		[Title("Sound")]
		public SoundEffect SoundEffect;

		[Title("My player")]
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

		[Title("Console")]
		public InGameConsole Console;

		[Title("Connection notifications")]
		public Image Latency;

		[Title("Quick tips")]
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
				SelectScreen(Screen.StartingPoint);
			}
			else if (Client.ReconnectAutomatically || Client.ForceRespawn)
			{
				ToggleLoadingScreen(LoadingScreenType.ConnectingToGame);
			}
			else
			{
				SelectScreen(Screen.MainMenu);
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
				if (MainMenu.activeInHierarchy && !InGameMenuCanvas.activeInHierarchy)
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
				else if (CharacterGroup.activeInHierarchy)
				{
					BackButton();
				}
				else if (StartingPointScreen.activeInHierarchy)
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
			SpawnPointOptionUI newWorldEntry = Instantiate(SpawnPointOptionUI, SpawnPointsHolder);
			newWorldEntry.CreateNewGameButton();

			DirectoryInfo directoryInfo = new DirectoryInfo(Client.Instance.GetSPPath());
			bool flag = true;
			foreach (FileInfo item in from m in directoryInfo.GetFiles("*.save") orderby m.LastWriteTime descending select m)
			{
				SpawnPointOptionUI spawnPointOptionUI2 = Instantiate(SpawnPointOptionUI, SpawnPointsHolder);
				spawnPointOptionUI2.SaveFile = item;
				spawnPointOptionUI2.CreateSaveGameButton();
				flag = false;
			}

			if (flag)
			{
				Client.Instance.PlayNewSPGame();
			}
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
				StartCoroutine(InvokeDeath());
				Client.IsDisconected = true;
			}
			else
			{
				Client.Instance.OpenMainScreen();
			}
		}

		private IEnumerator InvokeDeath()
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
			StartingPointOptionUI freshStartUI = GameObject.Instantiate(StartingPointUI, FreshStartSpawnOptions);
			freshStartUI.Type = StartingPointOption.FreshStart;
			freshStartUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				SplashScreen.FreshStart(CreateFreshStartTask(SpawnSetupType.Start1));
			});

			ProviderManager.MainProvider.GetAchievement(AchievementID.quest_sound_of_silence, out var achieved);
			StartingPointOptionUI strandedMinerUI = GameObject.Instantiate(StartingPointUI, FreshStartSpawnOptions);
			strandedMinerUI.Type = StartingPointOption.StrandedMiner;
			strandedMinerUI.GetComponent<Button>().interactable = achieved || Client.Instance.ExperimentalBuild;
			strandedMinerUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				SplashScreen.FreshStart(CreateFreshStartTask(SpawnSetupType.Start2));
			});

			ProviderManager.MainProvider.GetAchievement(AchievementID.quest_shattered_dreams, out achieved);
			StartingPointOptionUI evaUI = GameObject.Instantiate(StartingPointUI, FreshStartSpawnOptions);
			evaUI.Type = StartingPointOption.Eva;
			evaUI.GetComponent<Button>().interactable = achieved || Client.Instance.ExperimentalBuild;
			evaUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				SplashScreen.FreshStart(CreateFreshStartTask(SpawnSetupType.Start3));
			});

			ProviderManager.MainProvider.GetAchievement(AchievementID.quest_heart_of_stone, out achieved);
			StartingPointOptionUI soeUI = GameObject.Instantiate(StartingPointUI, FreshStartSpawnOptions);
			soeUI.Type = StartingPointOption.Soe;
			soeUI.GetComponent<Button>().interactable = achieved || Client.Instance.ExperimentalBuild;
			soeUI.GetComponent<Button>().onClick.AddListener(delegate
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
			string text = (!spawnPoint.Name.IsNullOrEmpty()) ? spawnPoint.Name : spawnPoint.SpawnSetupType.ToLocalizedString();
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

		/// <summary>
		/// 	Select one of the screens used in the start of the game, from the title menu to the loading screen.<br/>
		/// 	Contains: None, main menu, character select, spawn options, and loading screens.<br/>
		/// 	It is from before singleplayer was added.
		/// 	TODO: Remove this.
		/// </summary>
		public void SelectScreen(Screen screen)
		{
			switch (screen)
			{
			case Screen.None:
				MainMenu.SetActive(value: false);
				CharacterGroup.SetActive(value: false);
				StartingPointScreen.SetActive(value: false);
				ToggleLoadingScreen(LoadingScreenType.None);
				break;
			case Screen.MainMenu:
				MainMenu.SetActive(value: true);
				CharacterGroup.SetActive(value: false);
				StartingPointScreen.SetActive(value: false);
				ToggleLoadingScreen(Client.SinglePlayerQuickLoad ? LoadingScreenType.Loading : LoadingScreenType.None);
				break;
			case Screen.StartingPoint:
				CharacterGroup.SetActive(value: true);
				MainMenu.SetActive(value: false);
				StartingPointScreen.SetActive(value: false);
				ToggleLoadingScreen(LoadingScreenType.None);
				Client.IsLogout = false;
				Client.IsDisconected = false;
				break;
			case Screen.SpawnOptions:
				MainMenu.SetActive(value: false);
				CharacterGroup.SetActive(value: false);
				SinglePlayerModeScreen.SetActive(value: false);
				StartingPointScreen.SetActive(value: true);
				ToggleLoadingScreen(LoadingScreenType.None);
				CanChooseSpawn = true;
				break;
			case Screen.Loading:
				MainMenu.SetActive(value: false);
				CharacterGroup.SetActive(value: false);
				SinglePlayerModeScreen.SetActive(value: false);
				StartingPointScreen.SetActive(value: false);
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
			SelectScreen(Screen.MainMenu);
			NetworkController.Instance.Disconnect();
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
				SelectScreen(Screen.StartingPoint);
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
