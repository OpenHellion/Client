// DiscordProvider.cs
//
// Copyright (C) 2024, OpenHellion contributors
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
using Discord;
using OpenHellion.IO;
using OpenHellion.Net.Message;
using UnityEngine;
using ZeroGravity;
using ZeroGravity.Network;
using ZeroGravity.Objects;

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

		private static readonly List<string> Descriptions = new()
		{
			"Building station", "Mining asteroids", "In a salvaging mission", "Doing a piracy job",
			"Repairing a hull breach"
		};

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

			Debug.Log("Discord API initialised.");

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

		void IRichPresenceProvider.Destroy()
		{
			Debug.Log("Discord: Shutdown");
			_discord?.Dispose();
		}

		/// <inheritdoc/>
		public void UpdateStatus()
		{
			try
			{
				if (MyPlayer.Instance is not null && MyPlayer.Instance.PlayerReady)
				{
					_activity.Secrets.Join = Globals.GetInviteString(null);
					_activity.Assets.LargeText = Localization.InGameDescription;
					_activity.Details = Descriptions[UnityEngine.Random.Range(0, Descriptions.Count - 1)];
					if (MyPlayer.Instance.Parent is ArtificialBody { ParentCelestialBody: not null } artificialBody)
					{
						if (Planets.TryGetValue(artificialBody.ParentCelestialBody.Guid, out var value))
						{
							_activity.Assets.LargeImage = artificialBody.ParentCelestialBody.Guid.ToString();
						}
						else
						{
							_activity.Assets.LargeImage = "default";
							value = artificialBody.ParentCelestialBody.Name;
						}

						if (artificialBody is Ship { IsWarpOnline: true })
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

					_activity.Assets.SmallImage = Gender.Male.ToLocalizedString().ToLower();
					_activity.Assets.SmallText = MyPlayer.Instance.PlayerName;
					_activity.Party.Id = string.Empty;
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

				_activityManager.UpdateActivity(_activity, result => { });
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
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
				Debug.LogError("Error when getting discord user." + ex.Message);
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

			Debug.Log("Inviting user through Discord.");

			_activity.Secrets.Join = secret;
			_activity.Secrets.Spectate = secret;
			_activityManager.UpdateActivity(_activity, result => { });

			// Read the id without the prefix.
			_activityManager.SendInvite(long.Parse(id[1..]), ActivityActionType.Join,
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
