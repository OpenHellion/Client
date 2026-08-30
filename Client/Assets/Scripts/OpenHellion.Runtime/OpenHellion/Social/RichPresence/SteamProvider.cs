// SteamProvider.cs
//
// Copyright (C) 2026, OpenHellion contributors
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
using System.Collections.Concurrent;
using OpenHellion.IO;
using OpenHellion.Social.Message;
using static OpenHellion.Social.RichPresence.RichPresenceManager;
using System;

namespace OpenHellion.Social.RichPresence
{
	/// <seealso cref="DiscordProvider"/>
	internal class SteamProvider
	{
		private bool _currentStatsRequested;
		private bool _userStatsReceived;
		private bool _storeStats;
		private readonly ConcurrentQueue<Action> _pendingTasks = new ConcurrentQueue<Action>();

		internal bool Initialise()
		{
			if (!Packsize.Test())
			{
				Debug.LogError(
					"Steam: Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
			}

			if (!DllCheck.Test())
			{
				Debug.LogError(
					"Steam: DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
			}

			// https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
			bool success = SteamAPI.Init();

			return success;
		}

		// This should only ever get called on first load and after an Assembly reload, You should never Disable the Steamworks Manager yourself.
		internal void Enable()
		{
			[AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
			static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
			{
				Debug.LogWarning(pchDebugText);
			}

			// Set up our callback to receive warning messages from Steam.
			// You must launch with "-debug_steamapi" in the launch args to receive warnings.
			SteamClient.SetWarningMessageHook(new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook));

			if (_currentStatsRequested)
			{
				SteamUserStats.RequestCurrentStats();
			}

			Callback<GameRichPresenceJoinRequested_t>.Create(OnGameRichPresenceJoinRequested);
		}

		// OnApplicationQuit gets called too early to shutdown the SteamAPI.
		// Because the SteamManager should be persistent and never disabled or destroyed we can shutdown the SteamAPI here.
		// Thus it is not recommended to perform any Steamworks work in other OnDestroy functions as the order of execution can not be garenteed upon Shutdown. Prefer OnDisable().
		internal void Shutdown()
		{
			SteamAPI.Shutdown();
		}

		internal void Update()
		{
			// Run Steam client callbacks
			SteamAPI.RunCallbacks();

			if (!_currentStatsRequested)
			{
				Callback<UserStatsReceived_t>.Create(callback =>
				{
					_userStatsReceived = true;
				});
				_currentStatsRequested = SteamUserStats.RequestCurrentStats();
			}
			else if (_userStatsReceived)
			{
				while (_pendingTasks.TryDequeue(out var result) && result != null)
				{
					result();
				}

				if (_storeStats)
				{
					SteamUserStats.StoreStats();
					_storeStats = false;
				}
			}
		}

		// When we are joining a game.
		private void OnGameRichPresenceJoinRequested(GameRichPresenceJoinRequested_t param)
		{
			InviteMessage inviteMessage = JsonSerialiser.Deserialize<InviteMessage>(param.m_rgchConnect);
			GameStarter gameStarter = GameStarter.Create(inviteMessage);
			gameStarter.FindServerAndConnect().Forget();
		}

		internal void UpdateStatus(ActivityStatus status)
		{
			string displayStatus = string.IsNullOrEmpty(status.Details)
				? status.State
				: $"{status.State} - {status.Details}";

			SteamFriends.SetRichPresence("status", displayStatus);
			SteamFriends.SetRichPresence("connect", status.JoinSecret ?? string.Empty);
		}

		internal bool GetAchievement(AchievementID id)
		{
			SteamUserStats.GetAchievement(id.ToString(), out bool achieved);
			return achieved;
		}

		internal void SetAchievement(AchievementID id)
		{
			_pendingTasks.Enqueue(new Action(delegate
			{
				SteamUserStats.SetAchievement(id.ToString());
				_storeStats = true;
			}));
		}

		internal string GetUsername()
		{
			return SteamFriends.GetFriendPersonaName(SteamUser.GetSteamID());
		}

		internal void InviteUser(ulong id, string secret)
		{
			Debug.Log("Inviting user through Steam.");

			// Read the id without the prefix.
			SteamFriends.InviteUserToGame(new CSteamID(id), secret);
		}
	}
}
