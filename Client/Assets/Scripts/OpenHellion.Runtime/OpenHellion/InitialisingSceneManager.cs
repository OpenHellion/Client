// InitialisingSceneManager.cs
//
// Copyright (C) 2026, OpenHellion contributors
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
using OpenHellion.IO;
using OpenHellion.Social;
using OpenHellion.UI;
using UnityEngine.SceneManagement;
using ZeroGravity;
using ZeroGravity.UI;
using Cysharp.Threading.Tasks;
using OpenHellion.Social.RichPresence;

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

		public static SceneLoadTypeValue SceneLoadType = SceneLoadTypeValue.PreloadWithCopy;

		private const int MainServerAttempts = 3;

		private const int MainServerRetryDelayMs = 2000;

		private async UniTaskVoid Awake()
		{
			Profile.Load();

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

			Localization.LoadLanguage(Settings.SettingsData.GameSettings.LanguageIndex);

			ControlsRebinder.Initialize();

			if (!Profile.OfflineMode)
			{
				NakamaClient.OnNakamaError += HandleNakamaError;
				for (int attempt = 1; !await NakamaClient.Initialise(); attempt++)
				{
					if (attempt >= MainServerAttempts)
					{
						Debug.LogWarning("Could not reach the main server.");

						// UniTask wizardry to close game if the message box ok is clicked.
						UniTaskCompletionSource<bool> retry = new();
						GlobalGUI.ShowConfirmMessageBox(Localization.ConnectionError, Localization.NoNakamaConnection,
							Localization.Retry, Localization.Quit, () => retry.TrySetResult(true),
							() => retry.TrySetResult(false));
						if (!await retry.Task)
						{
							Application.Quit();
							return;
						}

						attempt = 0;
					}

					await UniTask.Delay(MainServerRetryDelayMs * attempt);
				}
			}

			RichPresenceManager.Initialise();
			RichPresenceManager.UpdateStatus();
		}

		private void OnDestroy()
		{
			NakamaClient.OnNakamaError -= HandleNakamaError;
		}

		private async UniTaskVoid Start()
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
				await Globals.SceneLoader.InitializeScenes();
				await ResolvePlayerIdentity();
				await UniTask.WaitWhile(() => Globals.SceneLoader.IsPreloading || !Profile.HasIdentity);
				await SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
			}
		}

		// Fetch our identity from nakama if connected, derive from a local username if not.
		private async UniTask ResolvePlayerIdentity()
		{
			if (!Profile.OfflineMode)
			{
				await UniTask.WaitWhile(() => !NakamaClient.HasAuthenticated);
				Profile.Username = await NakamaClient.GetDisplayName();
				Profile.PlayerId = await NakamaClient.GetUserId();
				return;
			}

			if (!Profile.HasUsername)
			{
				Profile.OnRequireUsername?.Invoke();
				await UniTask.WaitWhile(() => !Profile.HasUsername);
			}

			Profile.PlayerId = Profile.DeriveId(Profile.Username);
			Profile.Save();
		}

		private void HandleNakamaError(string text, Action action)
		{
			GlobalGUI.ShowErrorMessage(Localization.SystemError, text, action);
		}
	}
}
