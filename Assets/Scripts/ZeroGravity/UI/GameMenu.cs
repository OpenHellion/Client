using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class GameMenu : MonoBehaviour
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

		[Header("Game menu options")]
		public GameObject InGameMenuScreen;

		public GameObject SaveButton;

		public GameObject LoadButton;

		[Header("Server info")]
		public GameObject ServerInfoScreen;

		public GameObject ReportServerButton;

		public Text ServerNameText;

		public Text ServerDescriptionText;

		public GameObject ServerRestart;

		public Text ServerRestartTime;

		private string oldSettings;

		private string newSettings;

		private void Start()
		{
			if (!Client.Instance.IsInGame)
			{
				gameMenuUp = false;
				SettingsMenuUp = true;
			}
			else
			{
				gameMenuUp = true;
				SettingsMenuUp = false;
			}
		}

		private void Update()
		{
			if (gameMenuUp && ServerInfoScreen.activeInHierarchy)
			{
				if (Client.Instance.ServerRestartTime.HasValue)
				{
					ServerRestart.SetActive(true);
					TimeSpan timeSpan = Client.Instance.ServerRestartTime.Value - DateTime.UtcNow;
					string empty = string.Empty;
					empty = string.Format("{0:00}:{1:00}:{2:00}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
					ServerRestartTime.text = empty;
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
			if (InputManager.GetKeyDown(KeyCode.Escape) && !DisableGameMenu)
			{
				if (Client.Instance.CanvasManager.ReportServerBox.gameObject.activeInHierarchy)
				{
					Client.Instance.CanvasManager.ReportServerBox.DeactivateBox();
				}
				else if (GlossaryMenuUp)
				{
					GlosseryMenuButton();
				}
				else if (gameMenuUp)
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
			if (!Client.Instance.IsInGame)
			{
				return;
			}
			gameMenuUp = isUp;
			MainMenuPanel.SetActive(isUp);
			if (isUp)
			{
				ServerInfoScreen.SetActive(false);
				InGameMenuScreen.SetActive(true);
				Client.Instance.InputModule.UseCustomCursorPosition = false;
				ServerNameText.text = ((!Client.Instance.SinglePlayerMode) ? Client.LastConnectedServer.Name : Localization.SinglePlayer);
				ServerDescriptionText.text = ((!Client.Instance.SinglePlayerMode) ? Client.LastConnectedServer.Description : Localization.SinglePlayerModeDescription);
				SaveButton.SetActive(Client.Instance.SinglePlayerMode);
				LoadButton.SetActive(Client.Instance.SinglePlayerMode);
				ReportServerButton.SetActive(!Client.Instance.SinglePlayerMode);
				LogoutMainMenu.text = ((!Client.Instance.SinglePlayerMode) ? Localization.Logout.ToUpper() : Localization.MainMenu.ToUpper());
			}
			else
			{
				BaseSceneTrigger lockedToTrigger = MyPlayer.Instance.LockedToTrigger;
				if (MyPlayer.Instance.IsLockedToTrigger)
				{
					Client.Instance.InputModule.UseCustomCursorPosition = true;
				}
				else
				{
					Client.Instance.InputModule.UseCustomCursorPosition = false;
				}
			}
		}

		public void SettingsMenu(bool isUp)
		{
			if (!isUp && Settings.Instance.controlsComponent.ControlsRebind.CheckIfEmpty())
			{
				return;
			}
			SettingsMenuPanel.SetActive(isUp);
			SettingsMenuUp = isUp;
			if (!isUp && !Client.Instance.IsInGame)
			{
				base.gameObject.SetActive(false);
			}
			if (isUp)
			{
				ActionOnButton(0);
				oldSettings = Json.Serialize(GetComponent<Settings>().SettingsData);
				if (Client.Instance.IsInGame)
				{
					gameMenuUp = false;
					MainMenu(false);
					if (MyPlayer.Instance.IsLockedToTrigger)
					{
						Client.Instance.InputModule.UseCustomCursorPosition = false;
					}
				}
				return;
			}
			newSettings = Json.Serialize(GetComponent<Settings>().SettingsData);
			if (oldSettings != newSettings)
			{
				Client.Instance.ShowConfirmMessageBox(Localization.Settings, Localization.AreYouSureYouWantToSave, Localization.Yes, Localization.No, ConfirmSave, DiscardChanges);
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
			if (Client.Instance.IsInGame)
			{
				gameMenuUp = true;
				MainMenu(true);
				if (MyPlayer.Instance.IsLockedToTrigger)
				{
					Client.Instance.InputModule.UseCustomCursorPosition = true;
				}
			}
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
			Client.Instance.CanvasManager.ToggleInGameMenuCanvas(false);
			Client.Instance.OnInGameMenuClosed();
		}

		public void LogoutButton()
		{
			if (Client.Instance.SinglePlayerMode)
			{
				Client.Instance.ShowConfirmMessageBox(Localization.MainMenu, Localization.AreYouSureExitGame, Localization.Yes, Localization.No, LogoutYes);
			}
			else
			{
				Client.Instance.ShowConfirmMessageBox(Localization.Logout, Localization.AreYouSureLogout, Localization.Yes, Localization.No, LogoutYes);
			}
		}

		public void LogoutYes()
		{
			Client.Instance.LogOut();
		}

		public void ExitGameButton()
		{
			Client.Instance.ShowConfirmMessageBox(Localization.ExitGame, Localization.AreYouSureExitGame, Localization.Yes, Localization.No, ExitGameYes);
		}

		public void ExitGameYes()
		{
			Client.Instance.ExitGame();
		}

		public void RespawnButton()
		{
			Client.Instance.ShowConfirmMessageBox(Localization.Respawn, Localization.AreYouSureRespawn, Localization.Yes, Localization.No, PlayerRespawnYes);
		}

		public void PlayerRespawnYes()
		{
			Client.Instance.CanvasManager.CanvasUI.QuestCutScene.OnCutSceneFinished();
			gameMenuUp = false;
			MainMenu(false);
			Client.Instance.CanvasManager.InGameMenuCanvas.SetActive(false);
			Client.ForceRespawn = true;
			MyPlayer.Instance.Suicide();
			Client.Instance.SolarSystemRoot.SetActive(false);
			if (Client.Instance.SinglePlayerMode)
			{
				Client.SinglePlayerRespawn = true;
				Client.Instance.CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.Loading);
			}
			else
			{
				Client.SinglePlayerRespawn = false;
				Client.Instance.CanvasManager.ToggleLoadingScreen(CanvasManager.LoadingScreenType.ConnectingToGame);
			}
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
			InputManager.LoadDefaultJSON();
			Settings.Instance.SaveSettings(Settings.SettingsType.Controls);
		}

		public void ControlsResetToDefault()
		{
			Client.Instance.ShowConfirmMessageBox(Localization.ResetControls, Localization.ResetControlsMessage, Localization.Yes, Localization.No, ControlsResetToDefaultYes);
		}

		public void SaveSettingsButton()
		{
			if (!Settings.Instance.controlsComponent.ControlsRebind.CheckIfEmpty())
			{
				string text = Json.Serialize(GetComponent<Settings>().SettingsData);
				if (!(text == oldSettings))
				{
					Client.Instance.ShowConfirmMessageBox(Localization.SaveGameSettings, Localization.AreYouSureYouWantToSave, Localization.Yes, Localization.No, ConfirmSave, DiscardChanges);
				}
			}
		}

		public void ConfirmSave()
		{
			Settings.Instance.SaveSettings(Settings.SettingsType.All);
			oldSettings = Json.Serialize(GetComponent<Settings>().SettingsData);
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
