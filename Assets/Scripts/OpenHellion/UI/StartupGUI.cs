// StartupGUI.cs
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

using OpenHellion.Social;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using ZeroGravity;
using ZeroGravity.Math;

namespace OpenHellion.UI
{
	/// <summary>
	///		Controls the graphical user interface of the startup menu. This is the preloading menu, account login and creation menu, and the starting error box.
	/// </summary>
	public class StartupGUI : MonoBehaviour
	{
		[SerializeField] private List<Sprite> _PreloadImages = new List<Sprite>();

		[SerializeField] private NakamaClient _NakamaClient;

		private VisualElement _preloadBackground;
		private VisualElement _preloadBottom;
		private Label _preloadText;
		private ProgressBar _preloadProgress;

		private VisualElement _authenticationScreen;
		private TextField _authenticationEmail;
		private TextField _authenticationPassword;
		private Button _createAccountScreenButton;
		private Button _authenticateButton;

		private VisualElement _createAccountScreen;
		private TextField _createScreenUsername;
		private TextField _createScreenDisplayName;
		private TextField _createScreenEmail;
		private TextField _createScreenPassword;
		private Toggle _createScreenConsent;
		private Button _createScreenBackButton;
		private Button _createScreenButton;

		private VisualElement _errorBox;
		private Label _errorTitle;
		private Label _errorDescription;
		private Button _errorBoxClose;

		private IEnumerator<string> _shuffledTexts;
		private IEnumerator<Sprite> _shuffledImages;

		private void OnEnable()
		{
			_NakamaClient.OnRequireAuthentication.AddListener(OpenAuthenticationScreen);

			// The UXML is already instantiated by the UIDocument component
			var uiDocument = GetComponent<UIDocument>();

			_preloadBackground = uiDocument.rootVisualElement.Q("Background");
			_preloadBottom = _preloadBackground.Q("Bottom");
			_preloadText = _preloadBackground.Q("TipText") as Label;
			_preloadProgress = _preloadBackground.Q("ProgressBar") as ProgressBar;

			_authenticationScreen = uiDocument.rootVisualElement.Q("SignInScreen");
			_authenticationEmail = _authenticationScreen.Q("Email") as TextField;
			_authenticationPassword = _authenticationScreen.Q("Password") as TextField;
			_createAccountScreenButton = _authenticationScreen.Q("CreateAccountScreenButton") as Button;
			_authenticateButton = _authenticationScreen.Q("AuthenticateButton") as Button;

			_createAccountScreen = uiDocument.rootVisualElement.Q("CreateAccountScreen");
			_createScreenUsername = _createAccountScreen.Q("Username") as TextField;
			_createScreenDisplayName = _createAccountScreen.Q("DisplayName") as TextField;
			_createScreenEmail = _createAccountScreen.Q("Email") as TextField;
			_createScreenPassword = _createAccountScreen.Q("Password") as TextField;
			_createScreenConsent = _createAccountScreen.Q("Consent") as Toggle;
			_createScreenBackButton = _createAccountScreen.Q("BackButton") as Button;
			_createScreenButton = _createAccountScreen.Q("CreateAccount") as Button;

			_errorBox = uiDocument.rootVisualElement.Q("ErrorBox");
			_errorTitle = _errorBox.Q("ErrorTitle") as Label;
			_errorDescription = _errorBox.Q("ErrorDescription") as Label;
			_errorBoxClose = _errorBox.Q("ErrorBoxClose") as Button;

			_preloadBottom.visible = false;
			_authenticationScreen.visible = false;
			_createAccountScreen.visible = false;
			_errorBox.visible = false;

			_preloadBackground.style.backgroundImage = new StyleBackground(_PreloadImages[0]);

			Debug.Assert(_authenticateButton is not null);
			_authenticateButton.clicked += EnterGameCredentials;

			Debug.Assert(_createAccountScreenButton is not null);
			_createAccountScreenButton.clicked += OpenCreateAccountScreen;

			Debug.Assert(_createScreenBackButton is not null);
			_createScreenBackButton.clicked += OpenAuthenticationScreen;

			Debug.Assert(_createScreenButton is not null);
			_createScreenButton.clicked += CreateNewAccount;
		}

		private void OnDisable()
		{
			CloseAuthenticationScreen();
			CloseCreateAccountScreen();
			ClosePreloading();
			CloseErrorBox();
		}

		private void OpenAuthenticationScreen()
		{
			_createAccountScreen.visible = false;
			_authenticationScreen.visible = true;
		}

		private async void EnterGameCredentials()
		{
			string email = _authenticationEmail.value;
			string password = _authenticationPassword.value;

			if (!Regex.IsMatch(email, "\\S+@\\S+\\.\\w+"))
			{
				ShowErrorMessage(Localization.Error, Localization.InvalidEmail, null);
				return;
			}

			bool success = await _NakamaClient.Authenticate(email, password);

			// Clear the contents for next time.
			_authenticationEmail.value = string.Empty;
			_authenticationPassword.value = string.Empty;

			if (success)
			{
				Dbg.Log("Successfully authenticated with Nakama.");
				CloseAuthenticationScreen();
			}
		}

		private void CloseAuthenticationScreen()
		{
			_authenticationScreen.visible = false;
		}

		private void OpenCreateAccountScreen()
		{
			_authenticationScreen.visible = false;
			_createAccountScreen.visible = true;
		}

		private async void CreateNewAccount()
		{
			if (!_createScreenConsent.value)
			{
				ShowErrorMessage(Localization.Error, Localization.ConsentToDataStorage, null);
				return;
			}

			string username = _createScreenUsername.value;
			string displayName = _createScreenDisplayName.value;
			string email = _createScreenEmail.value;
			string password = _createScreenPassword.value;

			if (Regex.IsMatch(username, "[^a-z0-9_.]"))
			{
				ShowErrorMessage(Localization.Error, Localization.InvalidUsername, null);
				return;
			}

			if (!Regex.IsMatch(email, "\\S+@\\S+\\.\\w+"))
			{
				ShowErrorMessage(Localization.Error, Localization.InvalidEmail, null);
				return;
			}

			bool success = await _NakamaClient.CreateAccount(email, password, username, displayName);


			// Clear the contents for next time.
			_createScreenUsername.value = string.Empty;
			_createScreenEmail.value = string.Empty;
			_createScreenPassword.value = string.Empty;

			if (success)
			{
				Dbg.Log("Successfully created a new Nakama account.");
				CloseCreateAccountScreen();
			}

		}

		private void CloseCreateAccountScreen()
		{
			_createAccountScreen.visible = false;
		}

		public void OpenPreloading()
		{
			_preloadBottom.visible = true;

			_shuffledTexts = Localization.PreloadText.OrderBy(m => MathHelper.RandomNextDouble()).GetEnumerator();
			_shuffledImages = _PreloadImages.OrderBy(m => MathHelper.RandomNextDouble()).ToList().GetEnumerator();

			StartCoroutine(Preload());
		}

		/// <summary>
		///		Sets the bars progress. A value between 0 and 100.
		/// </summary>
		/// <param name="progress">Progress from 0 to 100.</param>
		public void UpdateProgressBar(float progress)
		{
			_preloadProgress.value = progress;
		}

		public void ClosePreloading()
		{
			_preloadBottom.visible = false;
			StopCoroutine(Preload());
			_preloadBackground.style.backgroundImage = new StyleBackground(_PreloadImages[0]);

			_shuffledTexts?.Dispose();
			_shuffledImages?.Dispose();
		}

		private IEnumerator Preload()
		{
			_preloadText.text = _shuffledTexts.GetNextInLoop();
			_preloadBackground.style.backgroundImage = new StyleBackground(_shuffledImages.GetNextInLoop());
			yield return new WaitForSeconds(10f);
		}

		public void ShowErrorMessage(string title, string text, Action onClose)
		{
			_errorBox.visible = true;
			_errorTitle.text = title;
			_errorDescription.text = text;

			_errorBoxClose.clicked += CloseErrorBox;
			_errorBoxClose.clicked += onClose;
		}

		private void CloseErrorBox()
		{
			_errorBox.visible = false;
		}
	}
}
