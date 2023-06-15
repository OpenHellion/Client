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

		private Label _PreloadText;
		private ProgressBar _PreloadProgress;
		private VisualElement _PreloadBackground;


		private void OnEnable()
		{
			_NakamaClient.OnRequireAuthentification = OpenAuthScreen;

			// The UXML is already instantiated by the UIDocument component
			var uiDocument = GetComponent<UIDocument>();

			_PreloadText = uiDocument.rootVisualElement.Q("TipText") as Label;
			_PreloadBackground = uiDocument.rootVisualElement.Q("Background");
			_PreloadProgress = uiDocument.rootVisualElement.Q("ProgressBar") as ProgressBar;

			_PreloadBackground.visible = false;
		}

		private void OnDisable()
		{
			ClosePreloading();
		}

		public void ShowErrorMessage(string title, string text, Action onClose)
		{

		}

		// Opens the authentification menu and preparing it for connection.
		private void OpenAuthScreen()
		{
			// Add confirm button call to execute
		}

		private async void EnterGameCredentials()
		{
			await _NakamaClient.Authenticate("email", "password");

			// If fail: call OpenAuthScreen
			// Complete: call CloseAuthScreen
		}

		private void CloseAuthScreen()
		{
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
