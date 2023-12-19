// GameState.cs
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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OpenHellion.IO;
using OpenHellion.Net;
using OpenHellion.Net.Message;
using OpenHellion.UI;
using UnityEditor;
using UnityEngine;
using ZeroGravity;
using ZeroGravity.Data;
using ZeroGravity.Network;
using Debug = UnityEngine.Debug;

namespace OpenHellion
{
	/// <summary>
	///		A class that has data and functions that we can call anywhere in the game. This means that this class always exists, and can be called from anywhere.
	/// </summary>
	public class Globals : MonoBehaviour
	{
		private static readonly uint NetworkDataHash = ClassHasher.GetClassHashCode(typeof(NetworkData));

		private static readonly uint SceneDataHash = ClassHasher.GetClassHashCode(typeof(ISceneData));

		public static readonly uint CombinedHash = NetworkDataHash * SceneDataHash;

		private float _secondsToWaitForExit = 3f;

		private bool _gameExitWanted;

		[NonSerialized] public Action OnHellionQuit;

		public float DefaultCameraFov = 75f;

		public float SpecialCameraFov = 40f;

		public float MouseSpeedOnPanels = 30f;

		public const int SettingsVersion = 1;

		public static Globals Instance { get; private set; }

		private void Awake()
		{
			if (Instance is not null)
			{
				Debug.LogError("Two instances of GlobalFunctions found!");
				Destroy(this);
				return;
			}

			Debug.LogFormat("Current combined hash: {0}", CombinedHash);

			Instance = this;
			DontDestroyOnLoad(gameObject);
		}

		private void Update()
		{
			if (_gameExitWanted)
			{
				_secondsToWaitForExit -= Time.deltaTime;
				if (_secondsToWaitForExit <= 0f)
				{
					QuitApplication();
				}
			}
		}

		private void OnApplicationQuit()
		{
			// If we didn't tell the game explicitly that we want to exit.
			if (!_gameExitWanted)
			{
				// Prevent the app from quitting...
				Application.wantsToQuit += () => false;

				// ...and exit safely instead.
				ExitGame();
			}
			else if (NetworkController.Instance is not null)
			{
				NetworkController.Instance.Disconnect();
			}
		}

		/// <summary>
		/// 	Method to exit the game safely. Handles shutting down connections to servers/clients properly, and saves settings.
		/// </summary>
		public static void ExitGame()
		{
			Instance._gameExitWanted = true;
			Instance.QuitApplication();
		}

		private void QuitApplication()
		{
			OnHellionQuit?.Invoke();
			NetworkController.Instance.Disconnect();
			HiResTime.Stop();
#if UNITY_EDITOR
			EditorApplication.ExitPlaymode();
#else
			Application.Quit();
#endif
		}

		/// <summary>
		/// 	Toggles the visibility and lock state of the cursor. No value inverts the current value.
		/// </summary>
		public static void ToggleCursor(bool? val = null)
		{
			Cursor.visible = !val.HasValue ? !Cursor.visible : val.Value;
			if (!Cursor.visible)
			{
				Cursor.lockState = CursorLockMode.Locked;
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
			}
		}

		/// <summary>
		///		Get an invite string that other players can use to connect to our game.
		/// </summary>
		/// <param name="spawnPointId">The spawn point to join. Leave as null to just invite to session.</param>
		/// <returns></returns>
		public static string GetInviteString(VesselObjectID spawnPointId)
		{
			InviteMessage inviteMessage = new InviteMessage
			{
				Time = Time.time,
				ServerId = MainMenuGUI.LastConnectedServer.Id,
				SpawnPointId = spawnPointId
			};
			return JsonSerialiser.Serialize(inviteMessage);
		}
	}
}
