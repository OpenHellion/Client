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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Objects;
using ZeroGravity;
using Discord;
using OpenHellion.Networking.Message;
using OpenHellion.IO;

namespace OpenHellion.ProviderSystem
{
	/// <summary>
	/// 	This class handles everything related to Discord. Acts as a bridge between the game and the API.
	/// </summary>
	/// <seealso cref="SteamProvider"/>
	internal class DiscordProvider : IProvider
	{
		private static readonly Dictionary<long, string> s_planets = new Dictionary<long, string>
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

		private static readonly List<string> s_descriptions = new List<string> { "Building a huuuuuge station", "Mining asteroids", "In a salvaging mission", "Doing a piracy job", "Repairing a hull breach" };

		private const long m_clientId = 349114016968474626L;
		private const uint m_optionalSteamId = 588210;

		private Discord.Discord m_discord;
		private ActivityManager m_activityManager;
		private UserManager m_userManager;
		private RelationshipManager m_relationshipManager;

		private User m_joinUser;
		private Activity m_activity = new();

		bool IProvider.Initialise()
		{
			// Init Discord API.
			try
			{
				m_discord = new(m_clientId, (UInt64)CreateFlags.NoRequireDiscord);
			}
			catch (ResultException)
			{
				return false;
			}

			m_discord.SetLogHook(LogLevel.Debug, (level, message) =>
			{
				Dbg.Log("Log[{0}] {1}", level, message);
			});

			m_activityManager = m_discord.GetActivityManager();

			// Required to work with Steam.
			m_activityManager.RegisterSteam(m_optionalSteamId);
			m_activityManager.RegisterCommand();

			// Callbacks.
			m_activityManager.OnActivitySpectate += JoinCallback;
			m_activityManager.OnActivityJoin += JoinCallback;
			m_activityManager.OnActivityInvite += InviteCallback;
			m_activityManager.OnActivityJoinRequest += JoinRequestCallback;

			m_userManager = m_discord.GetUserManager();
			m_relationshipManager = m_discord.GetRelationshipManager();

			Dbg.Log("Discord API initialised.");

			return true;
		}

		void IProvider.Enable()
		{
			UpdateStatus();
		}

		void IProvider.Update()
		{
			m_discord.RunCallbacks();
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
			m_activityManager.AcceptInvite(user.Id, result =>
			{
				Dbg.Log("AcceptInvite {0}", result);
			});
		}

		// When we get an ask to join request from another user.
		private void JoinRequestCallback(ref User user)
		{
			Dbg.Log(string.Format("Discord: Join request {0}#{1}: {2}", user.Username, user.Discriminator, user.Id));
			m_joinUser = user;

			RequestRespondYes();
		}

		public void RequestRespondYes()
		{
			Dbg.Log("Discord: Responding yes to Ask to Join request");
			m_activityManager.SendRequestReply(m_joinUser.Id, ActivityJoinRequestReply.Yes, res =>
			{
				if (res == Result.Ok)
				{
					Console.WriteLine("Responded successfully");
				}
			});
		}

		public void RequestRespondNo()
		{
			Dbg.Log("Discord: Responding no to Ask to Join request");
			m_activityManager.SendRequestReply(m_joinUser.Id, ActivityJoinRequestReply.No, res =>
			{
				if (res == Result.Ok)
				{
					Console.WriteLine("Responded successfully");
				}
			});
		}

		void IProvider.Destroy()
		{
			Dbg.Log("Discord: Shutdown");
			m_discord?.Dispose();
		}

		/// <inheritdoc/>
		public void UpdateStatus()
		{
			try
			{
				if (Client.Instance.SinglePlayerMode)
				{
					m_activity.State = "Playing single player game";
					m_activity.Details = "Having so much fun.";
					m_activity.Assets.LargeImage = "cover";
					m_activity.Assets.LargeText = string.Empty;
					m_activity.Assets.SmallImage = string.Empty;
					m_activity.Assets.SmallText = string.Empty;
					m_activity.Secrets.Join = string.Empty;
					m_activity.Party.Size.CurrentSize = 0;
					m_activity.Party.Size.MaxSize = 0;
					m_activity.Party.Id = string.Empty;
				}
				else if (MyPlayer.Instance != null && MyPlayer.Instance.PlayerReady)
				{
					m_activity.Secrets.Join = Client.Instance.GetInviteString(null);
					m_activity.Assets.LargeText = Localization.InGameDescription + ": " + Client.LastConnectedServer.Name;
					m_activity.Details = s_descriptions[UnityEngine.Random.Range(0, s_descriptions.Count - 1)];
					ArtificialBody artificialBody = MyPlayer.Instance.Parent as ArtificialBody;
					if (artificialBody != null && artificialBody.ParentCelesitalBody != null)
					{
						string value;
						if (s_planets.TryGetValue(artificialBody.ParentCelesitalBody.GUID, out value))
						{
							m_activity.Assets.LargeImage = artificialBody.ParentCelesitalBody.GUID.ToString();
						}
						else
						{
							m_activity.Assets.LargeImage = "default";
							value = artificialBody.ParentCelesitalBody.Name;
						}

						if (artificialBody is Ship && (artificialBody as Ship).IsWarpOnline)
						{
							m_activity.State = Localization.WarpingNear + " " + value.ToUpper();
						}
						else if (artificialBody is Pivot)
						{
							m_activity.State = Localization.FloatingFreelyNear + " " + value.ToUpper();
						}
						else
						{
							m_activity.State = Localization.OrbitingNear + " " + value.ToUpper();
						}
					}
					m_activity.Assets.SmallImage = Client.Instance.CurrentGender.ToLocalizedString().ToLower();
					m_activity.Assets.SmallText = MyPlayer.Instance.PlayerName;
					m_activity.Party.Size.CurrentSize = Client.LastConnectedServer.CurrentPlayers + 1;
					m_activity.Party.Size.MaxSize = Client.LastConnectedServer.MaxPlayers;
					m_activity.Party.Id = Client.LastConnectedServer.Hash.ToString();
				}
				else
				{
					m_activity.State = "In Menus";
					m_activity.Details = "Launch Sequence Initiated";
					m_activity.Assets.LargeImage = "cover";
					m_activity.Assets.LargeText = string.Empty;
					m_activity.Assets.SmallImage = string.Empty;
					m_activity.Assets.SmallText = string.Empty;
					m_activity.Secrets.Join = string.Empty;
					m_activity.Party.Size.CurrentSize = 0;
					m_activity.Party.Size.MaxSize = 0;
					m_activity.Party.Id = string.Empty;
				}

				m_activityManager.UpdateActivity(m_activity, result => {});
			}
			catch (Exception ex)
			{
				Dbg.Error(ex.Message, ex.StackTrace);
			}
		}

		/// <inheritdoc/>
		public bool GetAchievement(AchievementID id, out bool achieved)
		{
			achieved = false;
			return false;
		}

		/// <inheritdoc/>
		public void SetAchievement(AchievementID id)
		{

		}

		/// <inheritdoc/>
		public string GetUsername()
		{
			User user;
			try
			{
				user = m_userManager.GetCurrentUser();
			}
			catch (ResultException ex)
			{
				Dbg.Error("Error when getting discord user.", ex);
				return null;
			}

			return user.Username;
		}

		/// <inheritdoc/>
		public string GetPrefixedNativeId()
		{
			User user;
			try
			{
				user = m_userManager.GetCurrentUser();
			}
			catch (ResultException ex)
			{
				Dbg.Error("Error when getting discord user.", ex);
				return null;
			}

			return "d" + user.Id.ToString();
		}

		/// <inheritdoc/>
		public string GetNativeId()
		{
			User user;
			try
			{
				user = m_userManager.GetCurrentUser();
			}
			catch (ResultException ex)
			{
				Dbg.Error("Error when getting discord user.", ex);
				return null;
			}

			return user.Id.ToString();
		}

		/// <inheritdoc/>
		public IProvider.Friend[] GetFriends()
		{
			// Filter our list of relationships to users online and friends of us.
			m_relationshipManager.Filter((ref Relationship relationship) =>
			{
				return relationship.Type == RelationshipType.Friend;
			});

			List<IProvider.Friend> friends = new();

			// Get all relationships.
			for (uint i = 0; i < m_relationshipManager.Count(); i++)
			{
				// Get an individual relationship from the list.
				Relationship r = m_relationshipManager.GetAt(i);

				// Add the relationship to our friends list.
				friends.Add(new IProvider.Friend
				{
					NativeId = "d" + r.User.Id.ToString(),
					Name = r.User.Username,
					Status = r.Presence.Status == Status.Online ? IProvider.FriendStatus.ONLINE : IProvider.FriendStatus.OFFLINE
				});
			}

			return friends.ToArray();
		}

		/// <inheritdoc/>
		public Texture2D GetAvatar(string id)
		{
			Texture2D texture = Resources.Load<Texture2D>("UI/default_avatar");

			// Check for correct prefix.
			if (!id.StartsWith("d"))
			{
				return texture;
			}

			// Read the id without the prefix.
			m_discord.GetImageManager().Fetch(ImageHandle.User(long.Parse(id[1..])), false, (result, handle) =>
			{
				if (result == Result.Ok)
				{
					texture = m_discord.GetImageManager().GetTexture(handle);
				}
			});

			return texture;
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

			m_activity.Secrets.Join = secret;
			m_activity.Secrets.Spectate = secret;
			m_activityManager.UpdateActivity(m_activity, result => {});

			// Read the id without the prefix.
			m_activityManager.SendInvite(long.Parse(id[1..]), ActivityActionType.Join, "You have been invited to play Hellion!", result =>
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
