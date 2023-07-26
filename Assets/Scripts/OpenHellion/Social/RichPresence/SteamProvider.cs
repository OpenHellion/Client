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
using OpenHellion.Net.Message;
using ZeroGravity;
using OpenHellion.IO;

namespace OpenHellion.Social.RichPresence
{
	/// <seealso cref="DiscordProvider"/>
	internal class SteamProvider : IRichPresenceProvider
	{
		private bool m_currentStatsRequested;
		private bool m_userStatsReceived;
		private bool m_storeStats;
		private Callback<UserStatsReceived_t> m_userStatsReceivedCallback;
		private Callback<GameRichPresenceJoinRequested_t> m_GameRichPresenceJoinRequested;
		private readonly ConcurrentQueue<Task> m_pendingTasks = new ConcurrentQueue<Task>();

		private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

		[AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
		protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
		{
			Dbg.Warning(pchDebugText);
		}

		bool IRichPresenceProvider.Initialise()
		{
			if (!Packsize.Test())
			{
				Dbg.Error("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
			}

			if (!DllCheck.Test())
			{
				Dbg.Error("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
			}

			// https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
			bool success = SteamAPI.Init();

			if (success)
			{
				Dbg.Log("Steam: API initialised.");
			}

			return success;
		}

		// This should only ever get called on first load and after an Assembly reload, You should never Disable the Steamworks Manager yourself.
		void IRichPresenceProvider.Enable()
		{
			if (m_SteamAPIWarningMessageHook == null)
			{
				// Set up our callback to receive warning messages from Steam.
				// You must launch with "-debug_steamapi" in the launch args to receive warnings.
				m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
				SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
			}

			if (m_currentStatsRequested)
			{
				SteamUserStats.RequestCurrentStats();
			}

			m_GameRichPresenceJoinRequested = Callback<GameRichPresenceJoinRequested_t>.Create(OnGameRichPresenceJoinRequested);
		}

		// OnApplicationQuit gets called too early to shutdown the SteamAPI.
		// Because the SteamManager should be persistent and never disabled or destroyed we can shutdown the SteamAPI here.
		// Thus it is not recommended to perform any Steamworks work in other OnDestroy functions as the order of execution can not be garenteed upon Shutdown. Prefer OnDisable().
		void IRichPresenceProvider.Destroy()
		{
			Dbg.Log("Steam: Shutdown");
			SteamAPI.Shutdown();
		}

		void IRichPresenceProvider.Update()
		{
			// Run Steam client callbacks
			SteamAPI.RunCallbacks();

			if (!m_currentStatsRequested)
			{
				m_userStatsReceivedCallback = Callback<UserStatsReceived_t>.Create(callback => {
					m_userStatsReceived = true;
				});
				m_currentStatsRequested = SteamUserStats.RequestCurrentStats();
			}
			else if (m_userStatsReceived)
			{
				Task result;
				while (m_pendingTasks.TryDequeue(out result))
				{
					result.RunSynchronously();
				}
				if (m_storeStats)
				{
					SteamUserStats.StoreStats();
					m_storeStats = false;
				}
			}
		}

		// When we are joining a game.
		private void OnGameRichPresenceJoinRequested(GameRichPresenceJoinRequested_t param)
		{
			InviteMessage inviteMessage = JsonSerialiser.Deserialize<InviteMessage>(param.m_rgchConnect);
			Client.Instance.ProcessInvitation(inviteMessage);
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
			m_pendingTasks.Enqueue(new Task(delegate
			{
				SteamUserStats.SetAchievement(id.ToString());
				m_storeStats = true;
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

			Dbg.Log("Inviting user through Steam.");

			// Read the id without the prefix.
			SteamFriends.InviteUserToGame(new CSteamID(ulong.Parse(id[1..])), secret);
		}
	}
}
