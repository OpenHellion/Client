// InitialisingSceneManager.cs
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

using UnityEngine;
using System;
using System.Collections;
using OpenHellion.IO;
using OpenHellion.Social;
using OpenHellion.UI;
using UnityEngine.SceneManagement;
using ZeroGravity;
using ZeroGravity.UI;

namespace OpenHellion
{
	/// <summary>
	/// 	Checks if the program has all required specifications and dependencies it needs to run.
	/// </summary>
	public class InitialisingSceneManager : MonoBehaviour
	{
		public enum SceneLoadTypeValue
		{
			Simple,
			PreloadWithCopy
		}

		[SerializeField] private SceneLoader _sceneLoader;

		public static SceneLoadTypeValue SceneLoadType = SceneLoadTypeValue.PreloadWithCopy;

		private void Awake()
		{
			NakamaClient.OnNakamaError += HandleNakamaError;

			// Only load simple scenes if we have little available memory, regardless of settings.
			if (SystemInfo.systemMemorySize < 6000 || Application.isEditor)
			{
				SceneLoadType = SceneLoadTypeValue.Simple;
			}
			else
			{
				int property = Properties.GetProperty("load_type", (int)SceneLoadType);
				if (Enum.IsDefined(typeof(SceneLoadTypeValue), property))
				{
					SceneLoadType = (SceneLoadTypeValue)property;
				}
			}

			ControlsSubsystem.Reset();
			Settings.LoadSettings(Settings.SettingsType.All);

			string customLocalisationFile = Properties.GetProperty("custom_localization_file", string.Empty);
			if (Localization.LocalizationFiles.TryGetValue(Settings.SettingsData.GameSettings.LanguageIndex,
				    out var value))
			{
				try
				{
					Localization.ImportFromString(Resources.Load<TextAsset>(value).text);
				}
				catch
				{
					Debug.LogError("Error when loading language. Could not find language.");
				}
			}
			else if (customLocalisationFile != string.Empty)
			{
				Localization.ImportFromFile(customLocalisationFile);
			}
			else
			{
				Debug.LogError("Error when loading language. Could not find language.");
			}

			ControlsRebinder.Initialize();

			NakamaClient.Initialise();
		}

		private void OnDestroy()
		{
			NakamaClient.OnNakamaError -= HandleNakamaError;
		}

		private void Start()
		{
			HiResTime.Start();

			// Set some hard limits.
			if (SystemInfo.systemMemorySize < 4000 || SystemInfo.processorFrequency < 2000)
			{
				Debug.LogError("System has invalid specifications. Exiting...");
				GlobalGUI.ShowErrorMessage(Localization.SystemError, Localization.InvalidSystemSpesifications,
					Application.Quit);
				HiResTime.Stop();
			}
			else
			{
				_sceneLoader.InitializeScenes();
				StartCoroutine(CheckStartGame());
			}
		}

		// Start game when we are done preloading and we have authenticated with Nakama.
		private IEnumerator CheckStartGame()
		{
			yield return new WaitWhile(() => SceneLoader.IsPreloading || !NakamaClient.HasAuthenticated);
			SceneManager.LoadScene(1, LoadSceneMode.Single);
		}

		private void HandleNakamaError(string text, Action action)
		{
			GlobalGUI.ShowErrorMessage(Localization.SystemError, text, action);
		}
	}
}
