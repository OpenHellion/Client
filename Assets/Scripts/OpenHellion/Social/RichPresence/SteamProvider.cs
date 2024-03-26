// SteamProvider.cs
//
// Copyright (C) 2023, OpenHellion contributors
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
using Steamworks;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OpenHellion.IO;
using OpenHellion.Net.Message;
using OpenHellion;

namespace OpenHellion.Social.RichPresence
{
	/// <seealso cref="DiscordProvider"/>
	internal class SteamProvider : IRichPresenceProvider
	{
		private bool _currentStatsRequested;
		private bool _userStatsReceived;
		private bool _storeStats;
		private Callback<UserStatsReceived_t> _userStatsReceivedCallback;
		private Callback<GameRichPresenceJoinRequested_t> _gameRichPresenceJoinRequested;
		private readonly ConcurrentQueue<Task> _pendingTasks = new ConcurrentQueue<Task>();

		private SteamAPIWarningMessageHook_t _steamAPIWarningMessageHook;

		[AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
		protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
		{
			Debug.LogWarning(pchDebugText);
		}

		bool IRichPresenceProvider.Initialise()
		{
			if (!Packsize.Test())
			{
				Debug.LogError(
					"[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
			}

			if (!DllCheck.Test())
			{
				Debug.LogError(
					"[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
			}

			// https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
			bool success = SteamAPI.Init();

			if (success)
			{
				Debug.Log("Steam: API initialised.");
			}

			return success;
		}

		// This should only ever get called on first load and after an Assembly reload, You should never Disable the Steamworks Manager yourself.
		void IRichPresenceProvider.Enable()
		{
			if (_steamAPIWarningMessageHook == null)
			{
				// Set up our callback to receive warning messages from Steam.
				// You must launch with "-debug_steamapi" in the launch args to receive warnings.
				_steamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
				SteamClient.SetWarningMessageHook(_steamAPIWarningMessageHook);
			}

			if (_currentStatsRequested)
			{
				SteamUserStats.RequestCurrentStats();
			}

			_gameRichPresenceJoinRequested =
				Callback<GameRichPresenceJoinRequested_t>.Create(OnGameRichPresenceJoinRequested);
		}

		// OnApplicationQuit gets called too early to shutdown the SteamAPI.
		// Because the SteamManager should be persistent and never disabled or destroyed we can shutdown the SteamAPI here.
		// Thus it is not recommended to perform any Steamworks work in other OnDestroy functions as the order of execution can not be garenteed upon Shutdown. Prefer OnDisable().
		void IRichPresenceProvider.Destroy()
		{
			Debug.Log("Steam: Shutdown");
			SteamAPI.Shutdown();
		}

		void IRichPresenceProvider.Update()
		{
			// Run Steam client callbacks
			SteamAPI.RunCallbacks();

			if (!_currentStatsRequested)
			{
				_userStatsReceivedCallback = Callback<UserStatsReceived_t>.Create(callback =>
				{
					_userStatsReceived = true;
				});
				_currentStatsRequested = SteamUserStats.RequestCurrentStats();
			}
			else if (_userStatsReceived)
			{
				while (_pendingTasks.TryDequeue(out var result))
				{
					result.RunSynchronously();
				}

				if (_storeStats)
				{
					SteamUserStats.StoreStats();
					_storeStats = false;
				}
			}
		}

		// When we are joining a game.
		// TODO: Add invites.
		private void OnGameRichPresenceJoinRequested(GameRichPresenceJoinRequested_t param)
		{
			InviteMessage inviteMessage = JsonSerialiser.Deserialize<InviteMessage>(param.m_rgchConnect);
		}

		/// <inheritdoc/>
		public void UpdateStatus()
		{
		}

		/// <inheritdoc/>
		public bool GetAchievement(AchievementID id, out bool achieved)
		{
			return SteamUserStats.GetAchievement(id.ToString(), out achieved);
		}

		/// <inheritdoc/>
		public void SetAchievement(AchievementID id)
		{
			_pendingTasks.Enqueue(new Task(delegate
			{
				SteamUserStats.SetAchievement(id.ToString());
				_storeStats = true;
			}));
		}

		/// <inheritdoc/>
		public string GetUsername()
		{
			return SteamFriends.GetFriendPersonaName(SteamUser.GetSteamID());
		}

		/// <inheritdoc/>
		public void InviteUser(string id, string secret)
		{
			// Check for correct prefix.
			if (!id.StartsWith("s"))
			{
				return;
			}

			Debug.Log("Inviting user through Steam.");

			// Read the id without the prefix.
			SteamFriends.InviteUserToGame(new CSteamID(ulong.Parse(id[1..])), secret);
		}
	}
}
