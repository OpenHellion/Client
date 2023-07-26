// DiscordProvider.cs
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
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Objects;
using ZeroGravity;
using Discord;
using OpenHellion.Net.Message;
using OpenHellion.IO;

namespace OpenHellion.Social.RichPresence
{
	/// <summary>
	/// 	This class handles everything related to Discord. Acts as a bridge between the game and the API.
	/// </summary>
	/// <seealso cref="SteamProvider"/>
	internal class DiscordProvider : IRichPresenceProvider
	{
		private static readonly Dictionary<long, string> Planets = new()
		{
			{ 1L, "Hellion" },
			{ 2L, "Nimath" },
			{ 3L, "Athnar" },
			{ 4L, "Ulgorat" },
			{ 5L, "Tasciana" },
			{ 6L, "Hirath" },
			{ 7L, "Calipso" },
			{ 8L, "Iblith" },
			{ 9L, "Enigma" },
			{ 10L, "Eridil" },
			{ 11L, "Arhlan" },
			{ 12L, "Teiora" },
			{ 13L, "Sinha" },
			{ 14L, "Bethyr" },
			{ 15L, "Burner" },
			{ 16L, "Broken marble" },
			{ 17L, "Everest station" },
			{ 18L, "Askatar" },
			{ 19L, "Ia" }
		};

		private static readonly List<string> Descriptions = new() { "Building a huuuuuge station", "Mining asteroids", "In a salvaging mission", "Doing a piracy job", "Repairing a hull breach" };

		private const long ClientId = 349114016968474626L;
		private const uint OptionalSteamId = 588210;

		private Discord.Discord _discord;
		private ActivityManager _activityManager;
		private UserManager _userManager;

		private User _joinUser;
		private Activity _activity;

		bool IRichPresenceProvider.Initialise()
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

			_discord.SetLogHook(LogLevel.Debug, (level, message) =>
			{
				Dbg.Log("Log[{0}] {1}", level, message);
			});

			_activityManager = _discord.GetActivityManager();

			// Required to work with Steam.
			_activityManager.RegisterSteam(OptionalSteamId);
			_activityManager.RegisterCommand();

			// Callbacks.
			_activityManager.OnActivitySpectate += JoinCallback;
			_activityManager.OnActivityJoin += JoinCallback;
			_activityManager.OnActivityInvite += InviteCallback;
			_activityManager.OnActivityJoinRequest += JoinRequestCallback;

			_userManager = _discord.GetUserManager();

			Dbg.Log("Discord API initialised.");

			_discord.RunCallbacks();

			return true;
		}

		void IRichPresenceProvider.Enable()
		{
			UpdateStatus();
		}

		void IRichPresenceProvider.Update()
		{
			_discord.RunCallbacks();
		}

		// When we are joining a game.
		private void JoinCallback(string secret)
		{
			try
			{
				InviteMessage inviteMessage = JsonSerialiser.Deserialize<InviteMessage>(secret);
				Client.Instance.ProcessInvitation(inviteMessage);
			}
			catch (Exception ex)
			{
				Dbg.Error(ex.Message, ex.StackTrace);
			}
		}

		// When we get an invite to a game.
		private void InviteCallback(ActivityActionType Type, ref User user, ref Activity activity2)
		{
			// TODO: Make this safer.
			_activityManager.AcceptInvite(user.Id, result =>
			{
				Dbg.Log("AcceptInvite {0}", result);
			});
		}

		// When we get an ask to join request from another user.
		private void JoinRequestCallback(ref User user)
		{
			Dbg.Log(string.Format("Discord: Join request {0}#{1}: {2}", user.Username, user.Discriminator, user.Id));
			_joinUser = user;

			RequestRespondYes();
		}

		private void RequestRespondYes()
		{
			Dbg.Log("Discord: Responding yes to Ask to Join request");
			_activityManager.SendRequestReply(_joinUser.Id, ActivityJoinRequestReply.Yes, res =>
			{
				if (res == Result.Ok)
				{
					Console.WriteLine("Responded successfully");
				}
			});
		}

		private void RequestRespondNo()
		{
			Dbg.Log("Discord: Responding no to Ask to Join request");
			_activityManager.SendRequestReply(_joinUser.Id, ActivityJoinRequestReply.No, res =>
			{
				if (res == Result.Ok)
				{
					Console.WriteLine("Responded successfully");
				}
			});
		}

		void IRichPresenceProvider.Destroy()
		{
			Dbg.Log("Discord: Shutdown");
			_discord?.Dispose();
		}

		/// <inheritdoc/>
		public void UpdateStatus()
		{
			try
			{
				if (Client.Instance is not null && Client.Instance.SinglePlayerMode)
				{
					_activity.State = "Playing single player game";
					_activity.Details = "Having so much fun.";
					_activity.Assets.LargeImage = "cover";
					_activity.Assets.LargeText = string.Empty;
					_activity.Assets.SmallImage = string.Empty;
					_activity.Assets.SmallText = string.Empty;
					_activity.Secrets.Join = string.Empty;
					_activity.Party.Size.CurrentSize = 0;
					_activity.Party.Size.MaxSize = 0;
					_activity.Party.Id = string.Empty;
				}
				else if (MyPlayer.Instance is not null && MyPlayer.Instance.PlayerReady)
				{
					_activity.Secrets.Join = Client.Instance.GetInviteString(null);
					_activity.Assets.LargeText = Localization.InGameDescription + ": " + Client.LastConnectedServer.Name;
					_activity.Details = Descriptions[UnityEngine.Random.Range(0, Descriptions.Count - 1)];
					ArtificialBody artificialBody = MyPlayer.Instance.Parent as ArtificialBody;
					if (artificialBody is not null && artificialBody.ParentCelesitalBody != null)
					{
						string value;
						if (Planets.TryGetValue(artificialBody.ParentCelesitalBody.GUID, out value))
						{
							_activity.Assets.LargeImage = artificialBody.ParentCelesitalBody.GUID.ToString();
						}
						else
						{
							_activity.Assets.LargeImage = "default";
							value = artificialBody.ParentCelesitalBody.Name;
						}

						if (artificialBody is Ship && (artificialBody as Ship).IsWarpOnline)
						{
							_activity.State = Localization.WarpingNear + " " + value.ToUpper();
						}
						else if (artificialBody is Pivot)
						{
							_activity.State = Localization.FloatingFreelyNear + " " + value.ToUpper();
						}
						else
						{
							_activity.State = Localization.OrbitingNear + " " + value.ToUpper();
						}
					}
					_activity.Assets.SmallImage = Client.Instance.CurrentGender.ToLocalizedString().ToLower();
					_activity.Assets.SmallText = MyPlayer.Instance.PlayerName;
					_activity.Party.Size.CurrentSize = Client.LastConnectedServer.CurrentPlayers + 1;
					_activity.Party.Size.MaxSize = Client.LastConnectedServer.MaxPlayers;
					_activity.Party.Id = Client.LastConnectedServer.Hash.ToString();
				}
				else
				{
					_activity.State = "In Menus";
					_activity.Details = "Launch Sequence Initiated";
					_activity.Assets.LargeImage = "cover";
					_activity.Assets.LargeText = string.Empty;
					_activity.Assets.SmallImage = string.Empty;
					_activity.Assets.SmallText = string.Empty;
					_activity.Secrets.Join = string.Empty;
					_activity.Party.Size.CurrentSize = 0;
					_activity.Party.Size.MaxSize = 0;
					_activity.Party.Id = string.Empty;
				}

				_activityManager.UpdateActivity(_activity, result => {});
			}
			catch (Exception ex)
			{
				Dbg.Error(ex.Message, ex.StackTrace);
			}
		}

		/// <inheritdoc/>
		public string GetUsername()
		{
			User user;
			try
			{
				user = _userManager.GetCurrentUser();
			}
			catch (ResultException ex)
			{
				Dbg.Error("Error when getting discord user.", ex);
				return null;
			}

			return user.Username;
		}

		/// <inheritdoc/>
		public void InviteUser(string id, string secret)
		{
			// Check for correct prefix.
			if (!id.StartsWith("d"))
			{
				return;
			}

			Dbg.Log("Inviting user through Discord.");

			_activity.Secrets.Join = secret;
			_activity.Secrets.Spectate = secret;
			_activityManager.UpdateActivity(_activity, result => {});

			// Read the id without the prefix.
			_activityManager.SendInvite(long.Parse(id[1..]), ActivityActionType.Join, "You have been invited to play Hellion!", result =>
			{
				if (result == Result.Ok)
				{
					Dbg.Log("Invite sent.");
				}
				else
				{
					Dbg.Log("Invite failed.", result);
				}
			});
		}
	}
}
