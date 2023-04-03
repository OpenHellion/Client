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
using UnityEngine.InputSystem;
using OpenHellion.IO;

namespace ZeroGravity
{

	// TODO: Use new UI system.
	public class CanvasManager : MonoBehaviour
	{
		public enum Screen
		{
			None,
			MainMenu,
			CharacterSelect,
			StartingPoint,
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
		[ListDrawerSettings(Draggable = false, HideAddButton = true, HideRemoveButton = true, AlwaysExpanded = true)]
		public List<StartingPointOptionData> StartingPointData = new List<StartingPointOptionData>();

		public GameObject StartingPointScreen;

		public Transform SpawnOptions;

		public Transform FreshStartSpawnOptions;

		public StartingPointOptionUI StartingPointUI;

		[Title("Save selection screen")]
		public GameObject SaveAndSpawnPointScreen;

		public Transform SaveGamesHolder;

		[AssetsOnly, Required]
		public SaveGameOptionUI SaveGameOptionUI;

		public RawImage CurrentSaveGameScreenshot;

		public Text CurrentSaveGameDescription;

		public bool CanChooseSpawn = true;

		[Title("Single player")]
		public GameObject SinglePlayerModeScreen;

		public GameObject SinglePlayerDeadOptions;

		[Title("In game screens")]
		public GameMenu GameMenuScript;

		public CanvasUI CanvasUI;

		public GameObject InGameMenuCanvas;

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

		private float m_HitCanvasTime;

		private bool m_HitCanvasVisible;

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

		private float m_ShowDeadMsgTime;

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

		private int m_BusyLoadingActiveCount;

		public bool IsPlayerOverviewOpen => PlayerOverview.gameObject.activeInHierarchy;

		public bool IsGameMenuOpen => InGameMenuCanvas.activeInHierarchy;

		public bool ConsoleIsUp => Console.gameObject.activeInHierarchy;

		private void Start()
		{
			Canvas = GetComponent<Canvas>();

			// Localize text in child objects.
			foreach (Text text in GetComponentsInChildren<Text>(true))
			{
				if (Localization.CanvasManagerLocalization.TryGetValue(text.name, out string value))
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

			if (m_HitCanvasVisible)
			{
				m_HitCanvasTime += Time.deltaTime;
				if (m_HitCanvasTime >= 0.1f)
				{
					m_HitCanvasTime = 0f;
					HitCanvas.SetActive(value: false);
					m_HitCanvasVisible = false;
				}
			}

			// Close the disclaimer.
			if (Disclamer.activeInHierarchy && Keyboard.current.anyKey.wasPressedThisFrame)
			{
				DisclaimerAgree();
				return;
			}

			if (Keyboard.current.enterKey.wasPressedThisFrame)
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
			if (Keyboard.current.escapeKey.wasPressedThisFrame)
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
				if (Client.Instance.IsInGame && !OverlayCanvasIsOn && Keyboard.current.escapeKey.wasPressedThisFrame)
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
					ExitStartingPointScreen();
				}
			}
			if (ScreenShootMod.activeInHierarchy && Keyboard.current.f11Key.wasPressedThisFrame)
			{
				StartCoroutine(TakeScreenShoot());
			}
			if (Client.Instance.IsInGame && MyPlayer.Instance != null && Keyboard.current.f10Key.wasPressedThisFrame)
			{
				ToggleScreenShootMod();
			}
			if (Client.Instance.IsInGame && Keyboard.current.f2Key.wasPressedThisFrame && !Client.Instance.IsChatOpened && !IsInputFieldIsActive && MyPlayer.Instance.IsAdmin)
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
			if (DeadScreen.activeInHierarchy && Input.anyKey && Time.time - m_ShowDeadMsgTime > 3f && !Client.Instance.SinglePlayerMode)
			{
				ToggleDeadMsg(val: false);
			}
			else if (DeadScreen.activeInHierarchy && Client.Instance.SinglePlayerMode && Keyboard.current.escapeKey.isPressed)
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
			CurrentSaveGameDescription.text = Localization.FreshStart;
			Client.SinglePlayerGameMode = (Client.SPGameMode) mode;
			ShowSingleplayerSaves();
		}

		public void ShowSingleplayerSaves()
		{
			// Enable menu.
			SaveAndSpawnPointScreen.SetActive(value: true);

			// Clear all entries.
			SaveGamesHolder.DestroyAll<SaveGameOptionUI>();

			// Create default entry for creating new game.
			SaveGameOptionUI newWorldEntry = Instantiate(SaveGameOptionUI, SaveGamesHolder);
			newWorldEntry.CreateNewGameButton();

			DirectoryInfo directoryInfo = new DirectoryInfo(Client.Instance.GetSPPath());
			bool hasNoSaves = true;
			foreach (FileInfo item in from m in directoryInfo.GetFiles("*.save") orderby m.LastWriteTime descending select m)
			{
				SaveGameOptionUI saveOptionUI = Instantiate(SaveGameOptionUI, SaveGamesHolder);
				saveOptionUI.SaveFile = item;
				saveOptionUI.CreateSaveGameButton();
				hasNoSaves = false;
			}

			if (hasNoSaves)
			{
				Client.Instance.PlayNewSPGame();
			}
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
				m_ShowDeadMsgTime = Time.time;
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
					//Client.Instance.InputModule.ToggleCustomCursorPosition(val: false);
				}
				PressAnyKey.Activate(!Client.Instance.SinglePlayerMode);
			}
			else
			{
				m_ShowDeadMsgTime = 0f;
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
			StartingPointOptionUI freshStartUI = Instantiate(StartingPointUI, FreshStartSpawnOptions);
			freshStartUI.Type = StartingPointOption.FreshStart;
			freshStartUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				SplashScreen.FreshStart(CreateFreshStartTask(SpawnSetupType.Start1));
			});

			ProviderManager.GetAchievement(AchievementID.quest_sound_of_silence, out var achieved);
			StartingPointOptionUI strandedMinerUI = Instantiate(StartingPointUI, FreshStartSpawnOptions);
			strandedMinerUI.Type = StartingPointOption.StrandedMiner;
			strandedMinerUI.GetComponent<Button>().interactable = achieved || Client.Instance.ExperimentalBuild;
			strandedMinerUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				SplashScreen.FreshStart(CreateFreshStartTask(SpawnSetupType.Start2));
			});

			ProviderManager.GetAchievement(AchievementID.quest_shattered_dreams, out achieved);
			StartingPointOptionUI evaUI = Instantiate(StartingPointUI, FreshStartSpawnOptions);
			evaUI.Type = StartingPointOption.Eva;
			evaUI.GetComponent<Button>().interactable = achieved || Client.Instance.ExperimentalBuild;
			evaUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				SplashScreen.FreshStart(CreateFreshStartTask(SpawnSetupType.Start3));
			});

			ProviderManager.GetAchievement(AchievementID.quest_heart_of_stone, out achieved);
			StartingPointOptionUI soeUI = Instantiate(StartingPointUI, FreshStartSpawnOptions);
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

		public void ShowSpawnPoints(List<SpawnPointDetails> spawnPoints, Action<SpawnPointDetails> onSpawnClicked, bool canContinue)
		{
			Client.ForceRespawn = false;
			FreshStartSpawnOptions.gameObject.Activate(value: false);
			SpawnOptions.DestroyAll<StartingPointOptionUI>();
			SpawnOptions.gameObject.Activate(value: true);
			SaveAndSpawnPointScreen.SetActive(value: false);
			SaveGamesHolder.DestroyAll<SaveGameOptionUI>();
			foreach (SpawnPointDetails spawnPoint in spawnPoints)
			{
				InstantiateSpawnButton(spawnPoint, onSpawnClicked);
			}

			// Show fresh start. canContinue determines if we're able to open the next menu with spawn points.
			StartingPointOptionUI freshStartOptionUI = Instantiate(StartingPointUI, SpawnOptions);
			freshStartOptionUI.Type = StartingPointOption.NewGame;
			if (canContinue)
			{
				freshStartOptionUI.GetComponent<Button>().onClick.AddListener(delegate
				{
					OnFreshStartConfirm();
				});
			}
			else
			{
				freshStartOptionUI.GetComponent<Button>().onClick.AddListener(delegate
				{
					ShowFreshStartOptions();
				});
			}

			// Add continue button if this isn't singleplayer.
			if (!Client.Instance.SinglePlayerMode)
			{
				StartingPointOptionUI continueOptionUI = Instantiate(StartingPointUI, SpawnOptions);
				continueOptionUI.Type = StartingPointOption.Continue;
				SpawnPointDetails continueSpawnPoint = new SpawnPointDetails
				{
					SpawnSetupType = SpawnSetupType.Continue,
					IsPartOfCrew = false,
					PlayersOnShip = new List<string>()
				};
				continueOptionUI.GetComponent<Button>().onClick.AddListener(delegate
				{
					onSpawnClicked(continueSpawnPoint);
				});
				continueOptionUI.GetComponent<Button>().interactable = canContinue;
			}

			// In single player mode add a custom starting point option, while in multiplayer add an invite starting point option.
			StartingPointOptionUI inviteCustomOptionUI = Instantiate(StartingPointUI, SpawnOptions);
			inviteCustomOptionUI.Type = !Client.Instance.SinglePlayerMode ? StartingPointOption.Invite : StartingPointOption.CustomStartingPoint;
			inviteCustomOptionUI.GetComponent<Button>().interactable = spawnPoints.Count > 0;
			inviteCustomOptionUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				SaveAndSpawnPointScreen.SetActive(value: true);
			});
			InstantiateFreshStartOptions();
		}

		private void InstantiateSpawnButton(SpawnPointDetails spawnPoint, Action<SpawnPointDetails> onSpawnClicked)
		{
			SaveGameOptionUI spawnPointOptionUI = Instantiate(SaveGameOptionUI, SaveGamesHolder);
			string text = (!spawnPoint.Name.IsNullOrEmpty()) ? spawnPoint.Name : spawnPoint.SpawnSetupType.ToLocalizedString();
			spawnPointOptionUI.Name.text = text + ((spawnPoint.PlayersOnShip == null || spawnPoint.PlayersOnShip.Count <= 0) ? string.Empty : ("\n" + string.Join(", ", spawnPoint.PlayersOnShip.ToArray())));
			spawnPointOptionUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				onSpawnClicked(spawnPoint);
			});
			spawnPointOptionUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				SaveAndSpawnPointScreen.SetActive(value: false);
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
			case Screen.CharacterSelect:
				CharacterGroup.SetActive(value: true);
				MainMenu.SetActive(value: false);
				StartingPointScreen.SetActive(value: false);
				ToggleLoadingScreen(LoadingScreenType.None);
				Client.IsLogout = false;
				Client.IsDisconected = false;
				break;
			case Screen.StartingPoint:
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
		/// 	Handle exiting the statring point screen, the menu where you select if you want to continue or start a new game, as well as what type of new game.
		/// </summary>
		public void ExitStartingPointScreen()
		{
			// Go back to general spawn options (new game, continue, invite).
			if (FreshStartSpawnOptions.gameObject.activeSelf)
			{
				SpawnOptions.gameObject.SetActive(true);
				FreshStartSpawnOptions.gameObject.SetActive(false);
			}
			else
			{
				if (Client.Instance.SinglePlayerMode) // Go back to selecting game mode.
				{
					SelectScreen(Screen.None);
					SinglePlayerModeScreen.SetActive(value: true);
				}
				else // Exit screen completely, and go back to creating a character.
				{
					SelectScreen(Screen.CharacterSelect);
				}
			}
		}

		public void BeingHit()
		{
			HitCanvas.SetActive(value: true);
			m_HitCanvasTime = 0f;
			m_HitCanvasVisible = true;
		}

		public void ToggleBusyLoading(bool isActive)
		{
			if (Client.SceneLoadType == Client.SceneLoadTypeValue.PreloadWithCopy)
			{
				isActive = false;
			}
			if (isActive)
			{
				m_BusyLoadingActiveCount++;
			}
			else
			{
				m_BusyLoadingActiveCount--;
			}
			if (m_BusyLoadingActiveCount > 0)
			{
				BusyLoadingInfo.SetActive(value: true);
				IsBusyLoading = true;
			}
			else
			{
				BusyLoadingInfo.SetActive(value: false);
				m_BusyLoadingActiveCount = 0;
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
				DefaultInteractionTip.GetComponentInChildren<Text>(includeInactive: true).text = string.Format(Localization.PressToInteract, InputManager.GetAxisKeyName(InputManager.ConfigAction.Interact)).ToUpper();
				DefaultInteractionTip.Activate(value: true);
				DefaultInteractionTipSeen = true;
			}
		}
	}
}
