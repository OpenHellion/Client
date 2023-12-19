// MainMenuGUI.cs
//
// Copyright (C) 2023, OpenHellion contributors
//
// SPDX-License-Identifier: GPL-3.0-or-later
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenHellion.Data;
using OpenHellion.Net;
using OpenHellion.Social;
using OpenHellion.Social.RichPresence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using ZeroGravity;
using ZeroGravity.Network;
using ZeroGravity.UI;

namespace OpenHellion.UI
{
	public class MainMenuGUI : MonoBehaviour
	{
		public enum Screen
		{
			None,
			CreateCharacter,
			StartingPoint
		}

		public enum StartingPointOption
		{
			NewGame,
			FreshStart,
			Continue,
			Invite,
			Eva,
			StrandedMiner,
			Soe
		}

		[Title("Start screen")] public SplashScreen SplashScreen;

		public GameObject MainMenu;

		public GameObject Disclamer;

		private static bool ShowDisclaimer = true;

		public Text Version;

		[Title("Disconnect screen")] public GameObject DisconnectScreen;

		public static bool HasDisconnected { private get; set; }

		[Title("Character screen")]
		public InputField CharacterInputField;

		public GameObject CreateCharacterPanel;

		public Text CurrentGenderText;

		[Title("Spawn point selection screen")]
		public List<StartingPointOptionData> StartingPointData = new List<StartingPointOptionData>();

		public GameObject StartingPointScreen;

		public Transform SpawnOptions;

		public Transform FreshStartSpawnOptions;

		public StartingPointOptionUI StartingPointUI;

		[Title("Spawn point selection screen")]
		public GameObject SpawnPointScreen;

		[FormerlySerializedAs("SpawnPointHolder")]
		public Transform SpawnPointsHolder;

		public GameObject SaveGameOptionUI;

		public static bool CanChooseSpawn = true;

		private Gender _currentGenderGUI;

		public static ServerData LastConnectedServer;

		private void Awake()
		{
			if (HasDisconnected)
			{
				DisconnectScreen.SetActive(value: true);
				HasDisconnected = false;
			}

			#if DEBUG
			RichPresenceManager.SetAchievement(AchievementID.other_testing_squad_member);
			#endif
		}

		private async void Start()
		{
			// Localize text in child objects.
			foreach (Text text in GetComponentsInChildren<Text>(true))
			{
				if (Localization.MainMenuLocalisation.TryGetValue(text.name, out string value))
				{
					text.text = value;
				}
			}

			Cursor.visible = true;
			Disclamer.SetActive(ShowDisclaimer);
			if (ShowDisclaimer)
			{
				SplashScreen.StartVideo(0);
			}
			else
			{
				DisclaimerAgree();
			}

			Version.text = string.Format(Localization.ClientVersion, Application.version);
			StartingPointData = Resources.LoadAll<StartingPointOptionData>("StartingPoints").ToList();

			var data = await NakamaClient.GetCharacterData();
			if (data == null)
			{
				SelectScreen(Screen.CreateCharacter);
				CharacterInputField.text = await NakamaClient.GetDisplayName();
				SwitchCurrentGender();
			}

			EventSystem.AddListener(typeof(AvailableSpawnPointsResponse), AvailableSpawnPointsResponseListener);
		}

		private void Update()
		{
			// Exiting the splash screen is handled by its own script.
			if (SplashScreen.VideoPlayer.isPlaying)
			{
				return;
			}

			// Close the disclaimer.
			if (Disclamer.activeInHierarchy && Keyboard.current.anyKey.wasPressedThisFrame)
			{
				DisclaimerAgree();
				return;
			}

			if (Keyboard.current.enterKey.wasPressedThisFrame)
			{
				if (MainMenu.activeInHierarchy)
				{
					PlayButton();
				}
				else if (CreateCharacterPanel.activeInHierarchy)
				{
					CreateCharacterButton();
				}
			}

			// If esc is clicked.
			if (Keyboard.current.escapeKey.wasPressedThisFrame)
			{
				if (StartingPointScreen.activeInHierarchy)
				{
					ExitStartingPointScreen();
				}
			}
		}

		private void OnDestroy()
		{
			EventSystem.RemoveListener(typeof(AvailableSpawnPointsResponse), AvailableSpawnPointsResponseListener);
		}

		public void DisclaimerAgree()
		{
			ShowDisclaimer = false;
			Disclamer.SetActive(ShowDisclaimer);
			//_world.AmbientSounds.SwitchAmbience("MainMenu"); TODO
			//_world.AmbientSounds.Play(0);
		}

		public void DiscordButton()
		{
			Application.OpenURL("https://discord.gg/qZKJS5R");
		}

		public void GamepediaButton()
		{
			Application.OpenURL("https://hellion.gamepedia.com/Hellion_Wiki");
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
			freshStartUI.Data = StartingPointData.FirstOrDefault(m => m.Type == StartingPointOption.FreshStart);
			freshStartUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				SplashScreen.FreshStart(CreateFreshStartTask(SpawnSetupType.Start1));
			});

			RichPresenceManager.GetAchievement(AchievementID.quest_sound_of_silence, out var achieved);
			StartingPointOptionUI strandedMinerUI = Instantiate(StartingPointUI, FreshStartSpawnOptions);
			strandedMinerUI.Type = StartingPointOption.StrandedMiner;
			strandedMinerUI.Data = StartingPointData.FirstOrDefault(m => m.Type == StartingPointOption.StrandedMiner);
			strandedMinerUI.GetComponent<Button>().interactable = achieved;
			strandedMinerUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				SplashScreen.FreshStart(CreateFreshStartTask(SpawnSetupType.Start2));
			});

			RichPresenceManager.GetAchievement(AchievementID.quest_shattered_dreams, out achieved);
			StartingPointOptionUI evaUI = Instantiate(StartingPointUI, FreshStartSpawnOptions);
			evaUI.Type = StartingPointOption.Eva;
			evaUI.Data = StartingPointData.FirstOrDefault(m => m.Type == StartingPointOption.Eva);
			evaUI.GetComponent<Button>().interactable = achieved;
			evaUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				SplashScreen.FreshStart(CreateFreshStartTask(SpawnSetupType.Start3));
			});

			RichPresenceManager.GetAchievement(AchievementID.quest_heart_of_stone, out achieved);
			StartingPointOptionUI soeUI = Instantiate(StartingPointUI, FreshStartSpawnOptions);
			soeUI.Type = StartingPointOption.Soe;
			soeUI.Data = StartingPointData.FirstOrDefault(m => m.Type == StartingPointOption.Soe);
			soeUI.GetComponent<Button>().interactable = achieved;
			soeUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				SplashScreen.FreshStart(CreateFreshStartTask(SpawnSetupType.Start4));
			});
		}

		private Task CreateFreshStartTask(SpawnSetupType tip)
		{
			return new Task(delegate
			{
				SendSpawnRequest(new SpawnPointDetails
				{
					SpawnSetupType = tip,
					IsPartOfCrew = false,
					PlayersOnShip = new List<string>()
				});
			});
		}

		private void OnFreshStartConfirm()
		{
			GlobalGUI.ShowConfirmMessageBox(Localization.FreshStartConfrimTitle, Localization.FreshStartConfrimText,
				Localization.Yes, Localization.No, ShowFreshStartOptions);
		}

		public static void SendSpawnRequest(SpawnPointDetails details)
		{
			if (CanChooseSpawn)
			{
				CanChooseSpawn = false;
				PlayerSpawnRequest playerSpawnRequest = new PlayerSpawnRequest
				{
					SpawnSetupType = details.SpawnSetupType,
					SpawnPointParentId = details.SpawnPointParentID
				};
				NetworkController.SendToGameServer(playerSpawnRequest);
			}
		}

		public void ShowSpawnPointSelection(List<SpawnPointDetails> spawnPoints, bool canContinue)
		{
			SelectScreen(Screen.StartingPoint);
			if (spawnPoints == null)
			{
				spawnPoints = new List<SpawnPointDetails>();
			}

			ShowStartingPoints(spawnPoints, SendSpawnRequest, canContinue);
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
					ShowStartingPoints(availableSpawnPointsResponse.SpawnPoints, SendSpawnRequest, canContinue: false);
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public void ShowStartingPoints(List<SpawnPointDetails> spawnPoints, Action<SpawnPointDetails> onSpawnClicked,
			bool canContinue)
		{
			FreshStartSpawnOptions.gameObject.Activate(value: false);
			SpawnOptions.DestroyAll<StartingPointOptionUI>();
			SpawnOptions.gameObject.Activate(value: true);
			SpawnPointScreen.SetActive(value: false);
			SpawnPointsHolder.DestroyAll();
			foreach (SpawnPointDetails spawnPoint in spawnPoints)
			{
				CreateInviteSpawnPoints(spawnPoint, onSpawnClicked);
			}

			// Show fresh start. canContinue determines if we're able to open the next menu with spawn points.
			StartingPointOptionUI freshStartOptionUI = Instantiate(StartingPointUI, SpawnOptions);
			freshStartOptionUI.Type = StartingPointOption.NewGame;
			freshStartOptionUI.Data = StartingPointData.FirstOrDefault(m => m.Type == StartingPointOption.NewGame);
			if (canContinue)
			{
				freshStartOptionUI.GetComponent<Button>().onClick.AddListener(OnFreshStartConfirm);
			}
			else
			{
				freshStartOptionUI.GetComponent<Button>().onClick.AddListener(ShowFreshStartOptions);
			}

			StartingPointOptionUI continueOptionUI = Instantiate(StartingPointUI, SpawnOptions);
			continueOptionUI.Type = StartingPointOption.Continue;
			continueOptionUI.Data = StartingPointData.FirstOrDefault(m => m.Type == StartingPointOption.Continue);
			SpawnPointDetails continueSpawnPoint = new SpawnPointDetails
			{
				SpawnSetupType = SpawnSetupType.Continue,
				IsPartOfCrew = false,
				PlayersOnShip = new List<string>()
			};
			continueOptionUI.GetComponent<Button>().onClick
				.AddListener(delegate { onSpawnClicked(continueSpawnPoint); });
			continueOptionUI.GetComponent<Button>().interactable = canContinue;

			// In single player mode add a custom starting point option, while in multiplayer add an invite starting point option.
			StartingPointOptionUI inviteCustomOptionUI = Instantiate(StartingPointUI, SpawnOptions);
			inviteCustomOptionUI.Type = StartingPointOption.Invite;
			inviteCustomOptionUI.Data = StartingPointData.FirstOrDefault(m => m.Type == StartingPointOption.Invite);
			inviteCustomOptionUI.GetComponent<Button>().interactable = spawnPoints.Count > 0;
			inviteCustomOptionUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				SpawnPointScreen.SetActive(value: true);
			});
			InstantiateFreshStartOptions();
		}

		private void CreateInviteSpawnPoints(SpawnPointDetails spawnPoint, Action<SpawnPointDetails> onSpawnClicked)
		{
			GameObject spawnPointOptionUI = Instantiate(SaveGameOptionUI, SpawnPointsHolder);
			string text = !spawnPoint.Name.IsNullOrEmpty()
				? spawnPoint.Name
				: spawnPoint.SpawnSetupType.ToLocalizedString();
			spawnPointOptionUI.GetComponentInChildren<TextMeshProUGUI>().text = text +
				(spawnPoint.PlayersOnShip == null || spawnPoint.PlayersOnShip.Count <= 0
					? string.Empty
					: ("\n" + string.Join(", ", spawnPoint.PlayersOnShip.ToArray())));
			spawnPointOptionUI.GetComponent<Button>().onClick.AddListener(delegate
			{
				onSpawnClicked(spawnPoint);
				SpawnPointScreen.SetActive(value: false);
			});
		}

		/// <summary>
		/// 	Open one of the most important screens on the main menu.
		/// </summary>
		public void SelectScreen(Screen screen)
		{
			switch (screen)
			{
				case Screen.None:
					MainMenu.SetActive(value: false);
					CreateCharacterPanel.SetActive(value: false);
					StartingPointScreen.SetActive(value: false);
					GlobalGUI.ShowLoadingScreen(GlobalGUI.LoadingScreenType.None);
					break;
				case Screen.CreateCharacter:
					CreateCharacterPanel.SetActive(value: true);
					MainMenu.SetActive(value: false);
					StartingPointScreen.SetActive(value: false);
					GlobalGUI.ShowLoadingScreen(GlobalGUI.LoadingScreenType.None);
					break;
				case Screen.StartingPoint:
					MainMenu.SetActive(value: false);
					CreateCharacterPanel.SetActive(value: false);
					StartingPointScreen.SetActive(value: true);
					GlobalGUI.ShowLoadingScreen(GlobalGUI.LoadingScreenType.None);
					CanChooseSpawn = true;
					break;
			}
		}

		/// <summary>
		///		Starts connecting to multiplayer assuming you are on the main menu screen.
		/// </summary>
		public void PlayButton()
		{
			SelectScreen(Screen.None);
			GameStarter gameStarter = GameStarter.Create(ref LastConnectedServer);
			gameStarter.FindServerAndConnect();
		}

		/// <summary>
		/// 	The settings button in the main menu.
		/// </summary>
		public void SettingsButton()
		{
		}

		/// <summary>
		/// 	The quit button in the main menu.
		/// </summary>
		public void QuitButton()
		{
			Globals.ExitGame();
		}

		public void CreateCharacterButton()
		{
			var character = new CharacterData
			{
				Name = CharacterInputField.text,
				Gender = _currentGenderGUI,
				HeadType = 1,
				HairType = 1
			};

			NakamaClient.UpdateCharacterData(character);

			SelectScreen(Screen.None);
		}

		public void SwitchCurrentGender()
		{
			_currentGenderGUI = _currentGenderGUI == Gender.Male ? Gender.Female : Gender.Male;
			CurrentGenderText.text = _currentGenderGUI.ToLocalizedString();
			InventoryCharacterPreview.Instance.ChangeGender(_currentGenderGUI);
		}

		public void QuitGameButton()
		{
			GlobalGUI.ShowConfirmMessageBox(Localization.ExitGame, Localization.AreYouSureExitGame, Localization.Yes,
				Localization.No, Globals.ExitGame);
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
				// Exit screen completely, and go back to creating a character.
				SelectScreen(Screen.CreateCharacter);
			}
		}
	}
}
