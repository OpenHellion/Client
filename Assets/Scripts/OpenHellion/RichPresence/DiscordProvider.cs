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
using OpenHellion.Networking.Message;
using OpenHellion.IO;
using static Discord.UserManager;

namespace OpenHellion.RichPresence
{
	/// <summary>
	/// 	This class handles everything related to Discord. Acts as a bridge between the game and the API.
	/// </summary>
	/// <seealso cref="SteamProvider"/>
	internal class DiscordProvider : IPresenceProvider
	{
		private static readonly Dictionary<long, string> s_Planets = new Dictionary<long, string>
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

		private static readonly List<string> s_Descriptions = new List<string> { "Building a huuuuuge station", "Mining _Asteroids", "In a salvaging mission", "Doing a piracy job", "Repairing a hull breach" };

		private const long m_clientId = 349114016968474626L;
		private const uint m_optionalSteamId = 588210;

		private Discord.Discord m_Discord;
		private ActivityManager m_ActivityManager;
		private RelationshipManager m_RelationshipManager;
		private UserManager m_UserManager;

		private User m_JoinUser;
		private Activity m_Activity = new();

		public CurrentUserUpdateHandler OnUserUpdate
		{
			set
			{
				m_UserManager.OnCurrentUserUpdate += value;
			}
		}

		bool IPresenceProvider.Initialise()
		{
			// Init Discord API.
			try
			{
				m_Discord = new(m_clientId, (ulong)CreateFlags.NoRequireDiscord);
			}
			catch (ResultException)
			{
				return false;
			}

			m_Discord.SetLogHook(LogLevel.Debug, (level, message) =>
			{
				Dbg.Log("Log[{0}] {1}", level, message);
			});

			m_ActivityManager = m_Discord.GetActivityManager();

			// Required to work with Steam.
			m_ActivityManager.RegisterSteam(m_optionalSteamId);
			m_ActivityManager.RegisterCommand();

			// Callbacks.
			m_ActivityManager.OnActivitySpectate += JoinCallback;
			m_ActivityManager.OnActivityJoin += JoinCallback;
			m_ActivityManager.OnActivityInvite += InviteCallback;
			m_ActivityManager.OnActivityJoinRequest += JoinRequestCallback;

			m_RelationshipManager = m_Discord.GetRelationshipManager();
			m_UserManager = m_Discord.GetUserManager();

			Dbg.Log("Discord API initialised.");

			m_Discord.RunCallbacks();

			return true;
		}

		void IPresenceProvider.Enable()
		{
			UpdateStatus();
		}

		void IPresenceProvider.Update()
		{
			m_Discord.RunCallbacks();
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
			m_ActivityManager.AcceptInvite(user.Id, result =>
			{
				Dbg.Log("AcceptInvite {0}", result);
			});
		}

		// When we get an ask to join request from another user.
		private void JoinRequestCallback(ref User user)
		{
			Dbg.Log(string.Format("Discord: Join request {0}#{1}: {2}", user.Username, user.Discriminator, user.Id));
			m_JoinUser = user;

			RequestRespondYes();
		}

		private void RequestRespondYes()
		{
			Dbg.Log("Discord: Responding yes to Ask to Join request");
			m_ActivityManager.SendRequestReply(m_JoinUser.Id, ActivityJoinRequestReply.Yes, res =>
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
			m_ActivityManager.SendRequestReply(m_JoinUser.Id, ActivityJoinRequestReply.No, res =>
			{
				if (res == Result.Ok)
				{
					Console.WriteLine("Responded successfully");
				}
			});
		}

		void IPresenceProvider.Destroy()
		{
			Dbg.Log("Discord: Shutdown");
			m_Discord?.Dispose();
		}

		/// <inheritdoc/>
		public void UpdateStatus()
		{
			try
			{
				if (Client.Instance != null && Client.Instance.SinglePlayerMode)
				{
					m_Activity.State = "Playing single player game";
					m_Activity.Details = "Having so much fun.";
					m_Activity.Assets.LargeImage = "cover";
					m_Activity.Assets.LargeText = string.Empty;
					m_Activity.Assets.SmallImage = string.Empty;
					m_Activity.Assets.SmallText = string.Empty;
					m_Activity.Secrets.Join = string.Empty;
					m_Activity.Party.Size.CurrentSize = 0;
					m_Activity.Party.Size.MaxSize = 0;
					m_Activity.Party.Id = string.Empty;
				}
				else if (MyPlayer.Instance != null && MyPlayer.Instance.PlayerReady)
				{
					m_Activity.Secrets.Join = Client.Instance.GetInviteString(null);
					m_Activity.Assets.LargeText = Localization.InGameDescription + ": " + Client.LastConnectedServer.Name;
					m_Activity.Details = s_Descriptions[UnityEngine.Random.Range(0, s_Descriptions.Count - 1)];
					ArtificialBody artificialBody = MyPlayer.Instance.Parent as ArtificialBody;
					if (artificialBody != null && artificialBody.ParentCelesitalBody != null)
					{
						string value;
						if (s_Planets.TryGetValue(artificialBody.ParentCelesitalBody.GUID, out value))
						{
							m_Activity.Assets.LargeImage = artificialBody.ParentCelesitalBody.GUID.ToString();
						}
						else
						{
							m_Activity.Assets.LargeImage = "default";
							value = artificialBody.ParentCelesitalBody.Name;
						}

						if (artificialBody is Ship && (artificialBody as Ship).IsWarpOnline)
						{
							m_Activity.State = Localization.WarpingNear + " " + value.ToUpper();
						}
						else if (artificialBody is Pivot)
						{
							m_Activity.State = Localization.FloatingFreelyNear + " " + value.ToUpper();
						}
						else
						{
							m_Activity.State = Localization.OrbitingNear + " " + value.ToUpper();
						}
					}
					m_Activity.Assets.SmallImage = Client.Instance.CurrentGender.ToLocalizedString().ToLower();
					m_Activity.Assets.SmallText = MyPlayer.Instance.PlayerName;
					m_Activity.Party.Size.CurrentSize = Client.LastConnectedServer.CurrentPlayers + 1;
					m_Activity.Party.Size.MaxSize = Client.LastConnectedServer.MaxPlayers;
					m_Activity.Party.Id = Client.LastConnectedServer.Hash.ToString();
				}
				else
				{
					m_Activity.State = "In Menus";
					m_Activity.Details = "Launch Sequence Initiated";
					m_Activity.Assets.LargeImage = "cover";
					m_Activity.Assets.LargeText = string.Empty;
					m_Activity.Assets.SmallImage = string.Empty;
					m_Activity.Assets.SmallText = string.Empty;
					m_Activity.Secrets.Join = string.Empty;
					m_Activity.Party.Size.CurrentSize = 0;
					m_Activity.Party.Size.MaxSize = 0;
					m_Activity.Party.Id = string.Empty;
				}

				m_ActivityManager.UpdateActivity(m_Activity, result => {});
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
				user = m_UserManager.GetCurrentUser();
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
				user = m_UserManager.GetCurrentUser();
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
				user = m_UserManager.GetCurrentUser();
			}
			catch (ResultException ex)
			{
				Dbg.Error("Error when getting discord user.", ex);
				return null;
			}

			return user.Id.ToString();
		}

		/// <inheritdoc/>
		public IPresenceProvider.Friend[] GetFriends()
		{
			// Filter our list of relationships to users online and friends of us.
			m_RelationshipManager.Filter((ref Relationship relationship) =>
			{
				return relationship.Type == RelationshipType.Friend;
			});

			List<IPresenceProvider.Friend> friends = new();

			// Get all relationships.
			for (uint i = 0; i < m_RelationshipManager.Count(); i++)
			{
				// Get an individual relationship from the list.
				Relationship r = m_RelationshipManager.GetAt(i);

				// Add the relationship to our friends list.
				friends.Add(new IPresenceProvider.Friend
				{
					NativeId = "d" + r.User.Id.ToString(),
					Name = r.User.Username,
					Status = r.Presence.Status == Status.Online ? IPresenceProvider.FriendStatus.ONLINE : IPresenceProvider.FriendStatus.OFFLINE
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
			m_Discord.GetImageManager().Fetch(ImageHandle.User(long.Parse(id[1..])), false, (result, handle) =>
			{
				if (result == Result.Ok)
				{
					texture = m_Discord.GetImageManager().GetTexture(handle);
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

			m_Activity.Secrets.Join = secret;
			m_Activity.Secrets.Spectate = secret;
			m_ActivityManager.UpdateActivity(m_Activity, result => {});

			// Read the id without the prefix.
			m_ActivityManager.SendInvite(long.Parse(id[1..]), ActivityActionType.Join, "You have been invited to play Hellion!", result =>
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
