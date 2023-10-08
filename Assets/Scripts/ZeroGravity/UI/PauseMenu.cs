using System;
using System.Collections.Generic;
using OpenHellion;
using OpenHellion.IO;
using OpenHellion.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class PauseMenu : MonoBehaviour
	{
		public bool gameMenuUp;

		public bool SettingsMenuUp;

		public bool GlossaryMenuUp;

		private int currentPanel;

		public GameObject MainMenuPanel;

		public GameObject SettingsMenuPanel;

		public GameObject GlosseryMenuPanel;

		public static bool DisableGameMenu;

		public List<GameObject> ListaPanela;

		public List<GameObject> ActiveButtonList;

		public Text LogoutMainMenu;

		[Title("Game menu options")] public GameObject InGameMenuScreen;

		[Title("Server info")] public GameObject ServerInfoScreen;

		public GameObject ReportServerButton;

		public Text ServerNameText;

		public Text ServerDescriptionText;

		public GameObject ServerRestart;

		public Text ServerRestartTime;

		private string _oldSettings;

		private string _newSettings;

		[FormerlySerializedAs("_worldState")] [SerializeField] private World _world;

		private void Update()
		{
			if (gameMenuUp && ServerInfoScreen.activeInHierarchy)
			{
				if (_world.ServerRestartTime.HasValue)
				{
					ServerRestart.SetActive(true);
					TimeSpan timeSpan = _world.ServerRestartTime.Value - DateTime.UtcNow;
					ServerRestartTime.text =
						$"{(int)timeSpan.TotalHours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
					if (timeSpan.TotalMinutes < 5.0)
					{
						ServerRestartTime.color = Colors.Red;
					}
					else
					{
						ServerRestartTime.color = Colors.White;
					}
				}
				else
				{
					ServerRestart.SetActive(false);
				}
			}

			if (Keyboard.current.escapeKey.wasPressedThisFrame && !DisableGameMenu)
			{
				if (_world.InGameGUI.ReportServerBox.gameObject.activeInHierarchy)
				{
					_world.InGameGUI.ReportServerBox.DeactivateBox();
				}
				else if (GlossaryMenuUp)
				{
					GlosseryMenuButton();
				}
				else if (gameMenuUp) // If no other menus are up.
				{
					ResumeButton();
				}
				else
				{
					SettingsMenu(false);
				}
			}
		}

		public void MainMenu(bool isUp)
		{
			gameMenuUp = isUp;
			MainMenuPanel.SetActive(isUp);
			if (isUp)
			{
				ServerInfoScreen.SetActive(false);
				InGameMenuScreen.SetActive(true);
				ServerNameText.text = Localization.Multiplayer;
				ServerDescriptionText.text = MainMenuGUI.LastConnectedServer.Description;
				ReportServerButton.SetActive(true);
				LogoutMainMenu.text = (true) ? Localization.Logout.ToUpper() : Localization.MainMenu.ToUpper();
			}
		}

		public void SettingsMenu(bool isUp)
		{
			if (!isUp && Settings.Instance.ControlsComponent.ControlsRebind.CheckIfEmpty())
			{
				return;
			}

			SettingsMenuPanel.SetActive(isUp);
			SettingsMenuUp = isUp;
			if (isUp)
			{
				ActionOnButton(0);
				_oldSettings = JsonSerialiser.Serialize(GetComponent<Settings>().SettingsData);

				gameMenuUp = false;
				MainMenu(false);
				return;
			}

			_newSettings = JsonSerialiser.Serialize(GetComponent<Settings>().SettingsData);
			if (_oldSettings != _newSettings)
			{
				GlobalGUI.ShowConfirmMessageBox(Localization.Settings, Localization.AreYouSureYouWantToSave,
					Localization.Yes, Localization.No, ConfirmSave, DiscardChanges);
			}

			foreach (GameObject item in ListaPanela)
			{
				item.SetActive(false);
			}

			foreach (GameObject activeButton in ActiveButtonList)
			{
				activeButton.SetActive(false);
			}

			currentPanel = 0;
			gameMenuUp = true;
			MainMenu(true);
		}

		public void ResumeButton()
		{
			if (ServerInfoScreen.activeInHierarchy)
			{
				ServerInfoScreen.SetActive(false);
				InGameMenuScreen.SetActive(true);
				return;
			}

			gameMenuUp = false;
			MainMenu(false);
			_world.InGameGUI.ToggleInGameMenuCanvas(false);
			Globals.ToggleCursor(false);
		}

		public void LogoutButton()
		{
			GlobalGUI.ShowConfirmMessageBox(Localization.Logout, Localization.AreYouSureLogout, Localization.Yes,
				Localization.No, LogoutYes);
		}

		public void LogoutYes()
		{
			_world.LogOut();
		}

		public void ExitGameButton()
		{
			GlobalGUI.ShowConfirmMessageBox(Localization.ExitGame, Localization.AreYouSureExitGame, Localization.Yes,
				Localization.No, ExitGameYes);
		}

		public void ExitGameYes()
		{
			Globals.ExitGame();
		}

		public void RespawnButton()
		{
			GlobalGUI.ShowConfirmMessageBox(Localization.Respawn, Localization.AreYouSureRespawn, Localization.Yes,
				Localization.No, PlayerRespawnYes);
		}

		public void PlayerRespawnYes()
		{
			_world.InGameGUI.QuestCutScene.OnCutSceneFinished();
			gameMenuUp = false;
			MainMenu(false);
			_world.InWorldPanels.Detach();
			MyPlayer.Instance.Suicide();
			_world.SolarSystemRoot.SetActive(false);
			GlobalGUI.ShowLoadingScreen(GlobalGUI.LoadingScreenType.ConnectingToGame);
		}

		public void ActionOnButton(int current)
		{
			currentPanel = current;
			foreach (GameObject item in ListaPanela)
			{
				item.SetActive(false);
			}

			foreach (GameObject activeButton in ActiveButtonList)
			{
				activeButton.SetActive(false);
			}

			ListaPanela[currentPanel].SetActive(true);
			ActiveButtonList[currentPanel].SetActive(true);
			if (current == 1)
			{
				ListaPanela[1].GetComponentInChildren<ControlsRebinder>().EnableAllButtons(true);
			}
		}

		public void GlosseryMenuButton()
		{
			GlosseryMenuPanel.SetActive(!GlosseryMenuPanel.activeInHierarchy);
			GlossaryMenuUp = !GlossaryMenuUp;
			MainMenu(!gameMenuUp);
		}

		public void ControlsResetToDefaultYes()
		{
			InputManager.LoadDefaultConfig();
			Settings.Instance.SaveSettings(Settings.SettingsType.Controls);
		}

		public void ControlsResetToDefault()
		{
			GlobalGUI.ShowConfirmMessageBox(Localization.ResetControls, Localization.ResetControlsMessage,
				Localization.Yes, Localization.No, ControlsResetToDefaultYes);
		}

		public void SaveSettingsButton()
		{
			if (!Settings.Instance.ControlsComponent.ControlsRebind.CheckIfEmpty())
			{
				string text = JsonSerialiser.Serialize(GetComponent<Settings>().SettingsData);
				if (!(text == _oldSettings))
				{
					GlobalGUI.ShowConfirmMessageBox(Localization.SaveGameSettings, Localization.AreYouSureYouWantToSave,
						Localization.Yes, Localization.No, ConfirmSave, DiscardChanges);
				}
			}
		}

		public void ConfirmSave()
		{
			Settings.Instance.SaveSettings(Settings.SettingsType.All);
			_oldSettings = JsonSerialiser.Serialize(GetComponent<Settings>().SettingsData);
		}

		public void DiscardChanges()
		{
			Settings.Instance.LoadSettings(Settings.SettingsType.All);
		}

		public void ServerInfoButton()
		{
			InGameMenuScreen.SetActive(false);
			ServerInfoScreen.SetActive(true);
		}
	}
}
