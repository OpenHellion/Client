// GlobalGUI.cs
//
// Copyright (C) 2024, OpenHellion contributors
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
using OpenHellion.Data;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UIElements;
using ZeroGravity;
using ZeroGravity.Math;

namespace OpenHellion.UI
{
	[RequireComponent(typeof(UIDocument)), RequireComponent(typeof(SettingsMenuViewModel))]
	public class GlobalGUI : MonoBehaviour
	{
		public enum LoadingScreenType
		{
			Loading,
			ConnectingToMain,
			ConnectingToGame,
			LoadWorld
		}

		private enum SettingsMenu
		{
			Game,
			Controls,
			Video,
			Audio
		}

		private VisualElement _messageBox;
		private Label _messageTitle;
		private Label _messageDescription;
		private Button _messageClose;

		private VisualElement _errorBox;
		private Label _errorTitle;
		private Label _errorDescription;
		private Button _errorBoxClose;

		private VisualElement _confirmMessageBox;
		private Label _confirmTitle;
		private Label _confirmDescription;
		private Button _confirmYes;
		private Button _confirmNo;

		private VisualElement _loadingScreen;
		private Label _loadingDescription;
		private Label _loadingTips;

		private VisualElement _settingsScreen;
		private Button _gameSettingsButton;
		private Button _controlsSettingsButton;
		private Button _videoSettingsButton;
		private Button _audioSettingsButton;
		private VisualElement _gameSettingsScreen;
		private VisualElement _controlsSettingsScreen;
		private VisualElement _videoSettingsScreen;
		private VisualElement _audioSettingsScreen;

		private static GlobalGUI _instance;

		private IEnumerator<string> _shuffledLoadingTexts;

		private float _lastTimeLoadingTipsChanged;

		private SettingsMenuViewModel _settingsViewModel;

		[SerializeField] private List<PostProcessProfile> _postEffectProfiles;

		private void Awake()
		{
			if (_instance is not null)
			{
				if (gameObject.name.Equals("GlobalGUI"))
				{
					Destroy(gameObject);
				}
				else
				{
					Debug.LogError("Two instances of GlobalGUI created.");
					Destroy(this);
				}

				return;
			}
			_instance = this;
			_settingsViewModel = gameObject.GetComponent<SettingsMenuViewModel>();

			var uiDocument = GetComponent<UIDocument>();

			_messageBox = uiDocument.rootVisualElement.Q("MessageBox");
			_messageTitle = _messageBox.Q("MessageTitle") as Label;
			_messageDescription = _messageBox.Q("MessageDescription") as Label;
			_messageClose = _messageBox.Q("MessageClose") as Button;

			_errorBox = uiDocument.rootVisualElement.Q("ErrorBox");
			_errorTitle = _errorBox.Q("ErrorTitle") as Label;
			_errorDescription = _errorBox.Q("ErrorDescription") as Label;
			_errorBoxClose = _errorBox.Q("ErrorBoxClose") as Button;

			_confirmMessageBox = uiDocument.rootVisualElement.Q("ConfirmMessageBox");
			_confirmTitle = _confirmMessageBox.Q("ConfirmTitle") as Label;
			_confirmDescription = _confirmMessageBox.Q("ConfirmDescription") as Label;
			_confirmYes = _confirmMessageBox.Q("ConfirmYes") as Button;
			_confirmNo = _confirmMessageBox.Q("ConfirmNo") as Button;

			_loadingScreen = uiDocument.rootVisualElement.Q("LoadingScreen");
			_loadingDescription = _loadingScreen.Q("Description") as Label;
			_loadingTips = _loadingScreen.Q("LoadingTips") as Label;

			_settingsScreen = uiDocument.rootVisualElement.Q("SettingsScreen");
			_gameSettingsButton = _settingsScreen.Q("Game") as Button;
			_controlsSettingsButton = _settingsScreen.Q("Controls") as Button;
			_videoSettingsButton = _settingsScreen.Q("Video") as Button;
			_audioSettingsButton = _settingsScreen.Q("Audio") as Button;
			_gameSettingsScreen = _settingsScreen.Q("GameSettings");
			_controlsSettingsScreen = _settingsScreen.Q("ControlsSettings");
			_videoSettingsScreen = _settingsScreen.Q("VideoSettings");
			_audioSettingsScreen = _settingsScreen.Q("AudioSettings");

			Debug.Assert(_messageClose != null);
			_messageClose.clicked += CloseMessageBox;
			Debug.Assert(_errorBoxClose != null);
			_errorBoxClose.clicked += CloseErrorBox;
			Debug.Assert(_confirmYes != null);
			_confirmYes.clicked += CloseConfirmMessageBox;
			Debug.Assert(_confirmNo != null);
			_confirmNo.clicked += CloseConfirmMessageBox;

			Debug.Assert(_gameSettingsButton != null);
			_gameSettingsButton.clicked += () => ChangeSettingsScreen(SettingsMenu.Game);
			Debug.Assert(_controlsSettingsButton != null);
			_controlsSettingsButton.clicked += () => ChangeSettingsScreen(SettingsMenu.Controls);
			Debug.Assert(_videoSettingsButton != null);
			_videoSettingsButton.clicked += () => ChangeSettingsScreen(SettingsMenu.Video);
			Debug.Assert(_audioSettingsButton != null);
			_audioSettingsButton.clicked += () => ChangeSettingsScreen(SettingsMenu.Audio);

			// TODO: Confirm saving settings.

			_messageBox.visible = false;
			_errorBox.visible = false;
			_confirmMessageBox.visible = false;
			_loadingScreen.visible = false;
			_settingsScreen.visible = false;

			//resolutionDropdown.options.Clear();
			//foreach (var resolution in Screen.resolutions)
			//{
			//	resolutionDropdown.options.Add(new Dropdown.OptionData(resolution.ToString()));
			//}

			//qualitySettingsDropdown.options.Clear();
			//foreach (string text in QualitySettings.names)
			//{
			//	qualitySettingsDropdown.options.Add(new Dropdown.OptionData(text.ToString()));
			//}
		}

		private void OnDisable()
		{
			CloseErrorBox();
			CloseMessageBox();
			CloseConfirmMessageBox();
			CloseLoadingScreen();
			CloseSettingsScreen();
		}

		/// <summary>
		///		Shows a message box and closes the loading screen.
		/// </summary>
		/// <param name="title">The text to put at the top.</param>
		/// <param name="text">The body text.</param>
		public static void ShowMessageBox(string title, string text)
		{
			_instance._messageBox.visible = true;
			_instance._messageTitle.text = title;
			_instance._messageDescription.text = text;

			CloseLoadingScreen();
		}

		private void CloseMessageBox()
		{
			_messageBox.visible = false;
		}

		/// <summary>
		///		Show an error message.
		/// </summary>
		/// <param name="title">The text to put at the top.</param>
		/// <param name="text">The body text.</param>
		/// <param name="onClose">An action to execute when we click the close button.</param>
		public static void ShowErrorMessage(string title, string text, Action onClose = null)
		{
			_instance._errorBox.visible = true;
			_instance._errorTitle.text = title;
			_instance._errorDescription.text = text;

			_instance._errorBoxClose.clicked += onClose;
			_instance._errorBoxClose.clicked += () => { _instance._errorBoxClose.clicked -= onClose; };
		}

		private void CloseErrorBox()
		{
			_errorBox.visible = false;
		}

		/// <summary>
		///		Shows a message box with two buttons: a confirm and deny button. The text on these as well as the click action can be changed.
		/// </summary>
		/// <param name="title">The title text of the box.</param>
		/// <param name="message">The message text of the box.</param>
		/// <param name="yesText">The text of the yes button.</param>
		/// <param name="noText">The text of the no button.</param>
		/// <param name="onYes">Action to happen when we click yes.</param>
		/// <param name="onNo">Action to happen when we click no.</param>
		public static void ShowConfirmMessageBox(string title, string message, string yesText, string noText,
			Action onYes, Action onNo = null)
		{
			_instance._confirmMessageBox.visible = true;
			_instance._confirmTitle.text = title;
			_instance._confirmDescription.text = message;

			_instance._confirmYes.text = yesText;
			_instance._confirmYes.clicked += onYes;
			_instance._confirmYes.clicked += () => { _instance._errorBoxClose.clicked -= onYes; };

			_instance._confirmNo.text = noText;
			_instance._confirmNo.clicked += onNo;
			_instance._confirmNo.clicked += () => { _instance._errorBoxClose.clicked -= onNo; };
		}

		private void CloseConfirmMessageBox()
		{
			_instance._confirmMessageBox.visible = false;
		}

		/// <summary>
		///		Shows the specified loading screen.
		/// </summary>
		/// <param name="loadingScreenType">The type of loading screen to show.</param>
		/// <remarks>This does not stop input handling.</remarks>
		public static void ShowLoadingScreen(LoadingScreenType loadingScreenType)
		{
			_instance._shuffledLoadingTexts = Localization.PreloadText.OrderBy(m => MathHelper.RandomNextDouble())
				.ToList()
				.GetEnumerator();

			if (!_instance.IsInvoking(nameof(RefreshLoadingScreen)))
			{
				_instance.InvokeRepeating(nameof(RefreshLoadingScreen), 3f, 1f);
			}

			_instance._loadingScreen.visible = true;
			_instance._loadingDescription.text = loadingScreenType.ToLocalizedString();
			_instance._loadingTips.text = _instance._shuffledLoadingTexts.GetNextInLoop();
		}

		private void RefreshLoadingScreen()
		{
			if (_lastTimeLoadingTipsChanged + 8f < Time.realtimeSinceStartup) // Change loading tips.
			{
				_lastTimeLoadingTipsChanged = Time.realtimeSinceStartup;
				_loadingTips.text = _shuffledLoadingTexts.GetNextInLoop();
			}
		}

		public static void CloseLoadingScreen()
		{
			_instance._loadingScreen.visible = false;
			_instance.CancelInvoke(nameof(RefreshLoadingScreen));
		}

		public void OpenSettingsScreen()
		{
			_instance._settingsScreen.visible = true;
		}

		private void ChangeSettingsScreen(SettingsMenu menu)
		{
			switch (menu)
			{
				case SettingsMenu.Game:
					_gameSettingsScreen.visible = true;
					_controlsSettingsScreen.visible = false;
					_videoSettingsScreen.visible = false;
					_audioSettingsScreen.visible = false;
					break;
				case SettingsMenu.Controls:
					_gameSettingsScreen.visible = false;
					_controlsSettingsScreen.visible = true;
					_videoSettingsScreen.visible = false;
					_audioSettingsScreen.visible = false;
					break;
				case SettingsMenu.Video:
					_gameSettingsScreen.visible = false;
					_controlsSettingsScreen.visible = false;
					_videoSettingsScreen.visible = true;
					_audioSettingsScreen.visible = false;
					break;
				case SettingsMenu.Audio:
					_gameSettingsScreen.visible = false;
					_controlsSettingsScreen.visible = false;
					_videoSettingsScreen.visible = false;
					_audioSettingsScreen.visible = true;
					break;
			}
		}

		/// <summary>
		///		Resets settings back to their initial values.
		/// </summary>
		public static void ResetSettings()
		{
			_instance._settingsViewModel.ResetAudioSettings();
			_instance._settingsViewModel.ResetControlsSettings();
			_instance._settingsViewModel.ResetGameSettings();
			_instance._settingsViewModel.ResetVideoSettings();
		}

		public static GameSettingsData GetGameSettings()
		{
			return _instance._settingsViewModel.GetGameSettings();
		}

		public static void SetGameSettings(GameSettingsData data)
		{
			_instance._settingsViewModel.SetGameSettings(data);
		}

		public static ControlsSettingsData GetControlsSettings()
		{
			return _instance._settingsViewModel.GetControlsSettings();
		}

		public static void SetControlsSettings(ControlsSettingsData data)
		{
			_instance._settingsViewModel.SetControlsSettings(data);
		}

		public static VideoSettingsData GetVideoSettings()
		{
			return _instance._settingsViewModel.GetVideoSettings();
		}

		public static void SetVideoSettings(VideoSettingsData data)
		{
			_instance._settingsViewModel.SetVideoSettings(data);
		}

		public static AudioSettingsData GetAudioSettings()
		{
			return _instance._settingsViewModel.GetAudioSettings();
		}

		public static void SetAudioSettings(AudioSettingsData data)
		{
			_instance._settingsViewModel.SetAudioSettings(data);
		}

		public static void CloseSettingsScreen()
		{
			_instance._settingsScreen.visible = false;
		}

		public static void UpdatePostEffects(VideoSettingsData videoData)
		{
			foreach (PostProcessProfile postEffectProfile in _instance._postEffectProfiles)
			{
				foreach (PostProcessEffectSettings setting in postEffectProfile.settings)
				{
					if (setting.name == "AmbientOcclusion")
					{
						setting.active = videoData.AmbientOcclusion;
					}
					else if (setting.name == "MotionBlur")
					{
						setting.active = videoData.MotionBlur;
					}
					else if (setting.name == "AutoExposure")
					{
						setting.active = videoData.EyeAdaptation;
					}
					else if (setting.name == "Bloom")
					{
						setting.active = videoData.Bloom;
					}
					else if (setting.name == "ChromaticAberration")
					{
						setting.active = videoData.ChromaticAberration;
					}
				}
			}
		}
	}
}
