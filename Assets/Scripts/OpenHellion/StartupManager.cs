// StartupManager.cs
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
using ZeroGravity;
using OpenHellion.RichPresence;
using System;
using OpenHellion.UI;
using System.Collections;
using OpenHellion.Nakama;
using UnityEngine.SceneManagement;

namespace OpenHellion
{
	/// <summary>
	/// 	Checks if the program has all required specifications and dependencies it needs to run.
	/// </summary>
	public class StartupManager : MonoBehaviour
	{
		public enum SceneLoadTypeValue
		{
			Simple,
			PreloadWithCopy
		}

		[SerializeField] private StartupGUI _StartupGUI;
		[SerializeField] private SceneLoader _SceneLoader;
		[SerializeField] private NakamaClient _NakamaClient;

		public static SceneLoadTypeValue SceneLoadType = SceneLoadTypeValue.PreloadWithCopy;

		private void Awake()
		{
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
		}
		
		private void Start()
		{
			HiResTime.Start();

			if (!PresenceManager.AnyInitialised)
			{
				Dbg.Error("No external provider could be found. Exiting...");
				_StartupGUI.ShowErrorMessage(Localization.SystemError, Localization.NoProvider, Application.Quit);
				HiResTime.Stop();
			}
			else if (SystemInfo.systemMemorySize < 4000 || SystemInfo.processorFrequency < 2000)
			{
				Dbg.Error("System has invalid spesifications. Exiting...");
				_StartupGUI.ShowErrorMessage(Localization.SystemError, Localization.InvalidSystemSpesifications, Application.Quit);
				HiResTime.Stop();
			}
			else
			{
				_SceneLoader.InitializeScenes();
				StartCoroutine(CheckStartGame());
			}
		}

		// Start game when we are done preloading and we have authenticated with Nakama.
		private IEnumerator CheckStartGame()
		{
			yield return new WaitWhile(() => _SceneLoader.IsPreloading || !_NakamaClient.HasAuthenticated);
			SceneManager.LoadScene("Client", LoadSceneMode.Single);
		}
	}
}
