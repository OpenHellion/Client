// DiscordProvider.cs
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
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System;
using Discord;
using OpenHellion.IO;
using OpenHellion.Social.Message;
using UnityEngine;
using static OpenHellion.Social.RichPresence.RichPresenceManager;

namespace OpenHellion.Social.RichPresence
{
	/// <seealso cref="SteamProvider"/>
	internal class DiscordProvider
	{
		private const long ClientId = 349114016968474626L;
		private const uint OptionalSteamId = 588210;

		private Discord.Discord _discord;
		private ActivityManager _activityManager;
		private UserManager _userManager;
		private OverlayManager _overlayManager;

		private Activity _activity;

		internal bool Initialise()
		{
			// Init Discord API.
			try
			{
				_discord = new(ClientId, (ulong)CreateFlags.NoRequireDiscord);
			}
			catch (ResultException)
			{
				return false;
			}

			_discord.SetLogHook(LogLevel.Debug, (level, message) => { Debug.LogFormat("Log[{0}] {1}", level, message); });

			_activityManager = _discord.GetActivityManager();

			// Required to work with Steam.
			_activityManager.RegisterSteam(OptionalSteamId);
			_activityManager.RegisterCommand();

			// Callbacks.
			_activityManager.OnActivitySpectate += OnJoining;
			_activityManager.OnActivityJoin += OnJoining;
			_activityManager.OnActivityInvite += OnInviteReceived;
			_activityManager.OnActivityJoinRequest += OnJoinRequestReceived;

			_userManager = _discord.GetUserManager();
			_overlayManager = _discord.GetOverlayManager();

			_discord.RunCallbacks();

			return true;
		}

		internal void Update()
		{
			_discord.RunCallbacks();
		}

		// When we are joining a game.
		private void OnJoining(string secret)
		{
			try
			{
				InviteMessage inviteMessage = JsonSerialiser.Deserialize<InviteMessage>(secret);
				GameStarter gameStarter = GameStarter.Create(inviteMessage);
				gameStarter.FindServerAndConnect().Forget();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		// When we get an invite to a game.
		private void OnInviteReceived(ActivityActionType type, ref User user, ref Activity activity2)
		{
			// TODO: Make this safer.
			//_activityManager.AcceptInvite(user.Id, result => { Debug.LogFormat("AcceptInvite {0}", result); });
		}

		// When we get an ask to join request from another user.
		private void OnJoinRequestReceived(ref User user)
		{
			Debug.Log($"Discord: Join request {user.Username}#{user.Discriminator}: {user.Id}");

			_activityManager.SendRequestReply(user.Id, ActivityJoinRequestReply.Yes, res =>
			{
				if (res == Result.Ok)
				{
					Console.WriteLine("Responded successfully");
				}
			});
		}

		internal void Shutdown()
		{
			_discord?.Dispose();
		}

		internal void UpdateStatus(ActivityStatus status)
		{
			_activity = new Activity
			{
				State = status.State,
				Details = status.Details,
				Assets = new()
				{
					LargeImage = status.LargeImageId,
					LargeText = status.LargeText,
					SmallImage = status.SmallImageId,
					SmallText = status.SmallText,
				},
				Secrets = new()
				{
					Join = status.JoinSecret,
				},
				Party = new()
				{
					Id = string.Empty,
					Privacy = ActivityPartyPrivacy.Public,
					Size = new()
					{
						CurrentSize = status.PlayerCount,
						MaxSize = status.MaxPlayers,
					}
				}
			};
			_activityManager?.UpdateActivity(_activity, result => { });
		}

		internal string GetUsername()
		{
			User user;
			try
			{
				user = _userManager.GetCurrentUser();
			}
			catch (ResultException ex)
			{
				Debug.LogError("Error when getting discord user." + ex.Message);
				return null;
			}

			return user.Username;
		}

		internal void InviteUser(long id, string secret)
		{
			Debug.Log("Inviting user through Discord.");

			_activity.Secrets.Join = secret;
			_activityManager.UpdateActivity(_activity, result => { });

			// Read the id without the prefix.
			_activityManager.SendInvite(id, ActivityActionType.Join,
				"You have been invited to play Hellion!", result =>
				{
					if (result == Result.Ok)
					{
						Debug.Log("Invite sent.");
					}
					else
					{
						Debug.Log("Invite failed." + result);
					}
				});
		}
	}
}
