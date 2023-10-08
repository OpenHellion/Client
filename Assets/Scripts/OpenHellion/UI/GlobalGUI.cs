// GlobalGUI.cs
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
using UnityEngine;
using UnityEngine.UIElements;
using ZeroGravity;
using ZeroGravity.Math;

namespace OpenHellion.UI
{
	public class GlobalGUI : MonoBehaviour
	{
		public enum LoadingScreenType
		{
			None,
			Loading,
			ConnectingToMain,
			SigningIn,
			ConnectingToGame
		}

		private VisualElement _errorBox;
		private Label _errorTitle;
		private Label _errorDescription;
		private Button _errorBoxClose;

		private VisualElement _loadingScreen;
		private Label _loadingDescription;
		private Label _loadingTips;

		private static GlobalGUI _instance;

		private readonly IEnumerator<string> _shuffledLoadingTexts =
			Localization.PreloadText.OrderBy(m => MathHelper.RandomNextDouble()).ToList().GetEnumerator();

		private float _lastTimeLoadingTipsChanged;

		private void Awake()
		{
			if (_instance is not null)
			{
				Dbg.Error("Two instances of GlobalGUI created.");
				Destroy(this);
				return;
			}

			_instance = this;

			var uiDocument = GetComponent<UIDocument>();

			_errorBox = uiDocument.rootVisualElement.Q("ErrorBox");
			_errorTitle = _errorBox.Q("ErrorTitle") as Label;
			_errorDescription = _errorBox.Q("ErrorDescription") as Label;
			_errorBoxClose = _errorBox.Q("ErrorBoxClose") as Button;

			_loadingScreen = uiDocument.rootVisualElement.Q("LoadingScreen");
			_loadingDescription = _loadingScreen.Q("Description") as Label;
			_loadingTips = _loadingScreen.Q("LoadingTips") as Label;

			_errorBox.visible = false;
			_loadingScreen.visible = false;
		}

		private void OnDisable()
		{
			CloseErrorBox();
			CloseMessageBox();
			CloseConfirmMessageBox();
			CloseLoadingScreen();
		}

		/// <summary>
		///		Shows a message box and closes the loading screen.
		/// </summary>
		/// <param name="title">The text to put at the top.</param>
		/// <param name="text">The body text.</param>
		public static void ShowMessageBox(string title, string text)
		{
			CloseLoadingScreen();
		}

		private void CloseMessageBox()
		{
		}

		/// <summary>
		///		Show an error message.
		/// </summary>
		/// <param name="title">The text to put at the top.</param>
		/// <param name="text">The body text.</param>
		/// <param name="onClose">An action to execute when we click the close button.</param>
		public static void ShowErrorMessage(string title, string text, Action onClose)
		{
			_instance._errorBox.visible = true;
			_instance._errorTitle.text = title;
			_instance._errorDescription.text = text;

			_instance._errorBoxClose.clicked += _instance.CloseErrorBox;
			_instance._errorBoxClose.clicked += onClose;
			_instance._errorBoxClose.clicked += () => { _instance._errorBoxClose.clicked -= onClose; };
		}

		private void CloseErrorBox()
		{
			_instance._errorBoxClose.clicked -= _instance.CloseErrorBox;
			_errorBox.visible = false;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="title"></param>
		/// <param name="message"></param>
		/// <param name="yesText"></param>
		/// <param name="noText"></param>
		/// <param name="onYes"></param>
		/// <param name="onNo"></param>
		public static void ShowConfirmMessageBox(string title, string message, string yesText, string noText,
			Action onYes, Action onNo = null)
		{
		}

		private void CloseConfirmMessageBox()
		{
		}

		/// <summary>
		///		Shows the specified loading screen.
		/// </summary>
		/// <param name="loadingScreenType">The type of loading screen to show.</param>
		/// <remarks>This does not stop input handling.</remarks>
		public static void ShowLoadingScreen(LoadingScreenType loadingScreenType)
		{
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
			if (_lastTimeLoadingTipsChanged + 5f < Time.realtimeSinceStartup) // Change loading tips.
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
	}
}
