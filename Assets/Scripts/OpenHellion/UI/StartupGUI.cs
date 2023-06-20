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

using OpenHellion.Nakama;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using ZeroGravity;
using ZeroGravity.Math;

namespace OpenHellion.UI
{
	public class StartupGUI : MonoBehaviour
	{
		[SerializeField] private List<Sprite> _PreloadImages = new List<Sprite>();

		[SerializeField] private NakamaClient _NakamaClient;

		private VisualElement _PreloadBackground;
		private Label _PreloadText;
		private ProgressBar _PreloadProgress;

		private VisualElement _AuthentificationScreen;
		private TextField _AuthentificationEmail;
		private TextField _AuthentificationPassword;
		private Button _AuthentificateButton;

		private VisualElement _ErrorBox;
		private TextField _ErrorTitle;
		private TextField _ErrorDescription;
		private Button _ErrorBoxClose;

		private void OnEnable()
		{
			_NakamaClient.OnRequireAuthentification.AddListener(OpenAuthentificationScreen);

			// The UXML is already instantiated by the UIDocument component
			var uiDocument = GetComponent<UIDocument>();

			_PreloadBackground = uiDocument.rootVisualElement.Q("Background");
			_PreloadText = uiDocument.rootVisualElement.Q("TipText") as Label;
			_PreloadProgress = uiDocument.rootVisualElement.Q("ProgressBar") as ProgressBar;

			_AuthentificationScreen = uiDocument.rootVisualElement.Q("AuthentificationScreen");
			_AuthentificationEmail = uiDocument.rootVisualElement.Q("Email") as TextField;
			_AuthentificationPassword = uiDocument.rootVisualElement.Q("Password") as TextField;
			_AuthentificateButton = uiDocument.rootVisualElement.Q("AuthenticateButton") as Button;

			_PreloadBackground.visible = false;
			_AuthentificationScreen.visible = false;

			_AuthentificateButton.clicked += EnterGameCredentials;
		}

		private void OnDisable()
		{
			CloseAuthentificationScreen();
			ClosePreloading();
		}

		public void ShowErrorMessage(string title, string text, Action onClose)
		{
			// TODO: Add error box.
#if false
			_ErrorBox.visible = true;
			_ErrorTitle.label = title;
			_ErrorDescription.label = text;

			_ErrorBoxClose.clicked += CloseErrorBox;
			_ErrorBoxClose.clicked += onClose;
#endif
		}

		private void CloseErrorBox()
		{
			_ErrorBox.visible = false;
		}

		private void OpenAuthentificationScreen()
		{
			_AuthentificationScreen.visible = true;
		}

		private async void EnterGameCredentials()
		{
			string email = _AuthentificationEmail.value;
			string password = _AuthentificationPassword.value;
			bool success = await _NakamaClient.Authenticate(email, password);

			// Clear the contents for next time.
			_AuthentificationEmail.value = string.Empty;
			_AuthentificationPassword.value = string.Empty;

			if (success)
			{
				CloseAuthentificationScreen();
			}
			else
			{
				OpenAuthentificationScreen();
			}
		}

		private void CloseAuthentificationScreen()
		{
			_AuthentificationScreen.visible = false;

			// Close menu and continue loading/open game.
		}

		public void OpenPreloading()
		{
			_PreloadBackground.visible = true;
			StartCoroutine(Preload());
		}

		public void UpdateProgressBar(float progress)
		{
			_PreloadProgress.value = progress;
		}

		public void ClosePreloading()
		{
			_PreloadBackground.visible = false;
			StopCoroutine(Preload());
		}

		private IEnumerator Preload()
		{
			IEnumerator<string> shuffledTexts = Localization.PreloadText.OrderBy((string m) => MathHelper.RandomNextDouble()).GetEnumerator();
			IEnumerator<Sprite> shuffledImages = _PreloadImages.OrderBy((Sprite m) => MathHelper.RandomNextDouble()).ToList().GetEnumerator();

			yield return new WaitForSeconds(10f);
			_PreloadText.text = shuffledTexts.GetNextInLoop();
			_PreloadBackground.style.backgroundImage = new StyleBackground(shuffledImages.GetNextInLoop());
		}
	}
}
