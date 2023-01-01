using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity;
using Discord;

namespace OpenHellion.ProviderSystem
{
	/// <summary>
	/// 	This class handles everything related to Discord. Acts as a bridge between the game and the API.
	/// </summary>
	internal class DiscordProvider : IProvider
	{
		private static Dictionary<long, string> s_planets = new Dictionary<long, string>
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

		private static List<string> s_descriptions = new List<string> { "Building a huuuuuge station", "Mining asteroids", "In a salvaging mission", "Doing a piracy job", "Repairing a hull breach" };

		private long _clientId = 349114016968474626L;
		private uint _optionalSteamId = 588210;

		private int _callbackCalls;
		private Discord.Discord _discord;
		private ActivityManager _activityManager;
		private UserManager _userManager;
		private RelationshipManager _relationshipManager;

		private User _joinUser;
		private Activity _activity = new();

		private string _generatedId;

		bool IProvider.Initialise()
		{
			_callbackCalls = 0;

			// Init Discord API.
			try
			{
				_discord = new(_clientId, (UInt64)CreateFlags.NoRequireDiscord);
			}
			catch (ResultException)
			{
				return false;
			}

			_discord.SetLogHook(Discord.LogLevel.Debug, (level, message) =>
			{
				Dbg.Log("Log[{0}] {1}", level, message);
			});

			_activityManager = _discord.GetActivityManager();

			// Required to work with Steam.
			_activityManager.RegisterSteam(_optionalSteamId);
			_activityManager.RegisterCommand();

			// Callbacks.
			_activityManager.OnActivitySpectate += JoinCallback;
			_activityManager.OnActivityJoin += JoinCallback;
			_activityManager.OnActivityInvite += InviteCallback;
			_activityManager.OnActivityJoinRequest += JoinRequestCallback;

			_userManager = _discord.GetUserManager();
			_relationshipManager = _discord.GetRelationshipManager();

			Dbg.Log("Discord API initialised.");

			return true;
		}


		void IProvider.Enable()
		{
			// Check if a player id is stored in the cloud, then locally, then generate.

			UpdateStatus();
		}

		void IProvider.Update()
		{
			_discord.RunCallbacks();
		}

		// When we are joining a game.
		private void JoinCallback(string secret)
		{
			_callbackCalls++;
			try
			{
				InviteMessage inviteMessage = Serializer.ReceiveData(new MemoryStream(Convert.FromBase64String(secret))) as InviteMessage;
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
			_callbackCalls++;

			// TODO: Make this safer.
			_activityManager.AcceptInvite(user.Id, result => {
				Dbg.Log("AcceptInvite {0}", result);
			});
		}

		// When we get an ask to join request from another user.
		private void JoinRequestCallback(ref User user)
		{
			Dbg.Log(string.Format("Discord: Join request {0}#{1}: {2}", user.Username, user.Discriminator, user.Id));
			_callbackCalls++;
			_joinUser = user;

			RequestRespondYes();
		}

		public void RequestRespondYes()
		{
			Dbg.Log("Discord: Responding yes to Ask to Join request");
			_activityManager.SendRequestReply(_joinUser.Id, ActivityJoinRequestReply.Yes, res => {
				if (res == Result.Ok)
				{
					Console.WriteLine("Responded successfully");
				}
			});
		}

		public void RequestRespondNo()
		{
			Dbg.Log("Discord: Responding no to Ask to Join request");
			_activityManager.SendRequestReply(_joinUser.Id, ActivityJoinRequestReply.No, res => {
				if (res == Result.Ok)
				{
					Console.WriteLine("Responded successfully");
				}
			});
		}

		void IProvider.Destroy()
		{
			Dbg.Log("Discord: Shutdown");

			try
			{
				_discord.Dispose();
			}
			catch (NullReferenceException) {}
		}

		public bool IsInitialised()
		{
			return true;
		}

		public void UpdateStatus()
		{
			try
			{
				if (Client.Instance.SinglePlayerMode)
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
				else if (MyPlayer.Instance != null && MyPlayer.Instance.PlayerReady)
				{
					_activity.Secrets.Join = Client.Instance.GetInviteString(null);
					_activity.Assets.LargeText = Localization.InGameDescription + ": " + Client.LastConnectedServer.Name;
					_activity.Details = s_descriptions[UnityEngine.Random.Range(0, s_descriptions.Count - 1)];
					ArtificialBody artificialBody = MyPlayer.Instance.Parent as ArtificialBody;
					if (artificialBody != null && artificialBody.ParentCelesitalBody != null)
					{
						string value;
						if (s_planets.TryGetValue(artificialBody.ParentCelesitalBody.GUID, out value))
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

		public bool GetAchievement(AchievementID id, out bool achieved)
		{
			achieved = false;
			return false;
		}

		public void SetAchievement(AchievementID id)
		{

		}

		public bool GetStat(ProviderStatID id, out int value)
		{
			value = 0;
			return false;
		}

		public void SetStat(ProviderStatID id, int value)
		{

		}

		public void ResetStat(ProviderStatID id)
		{

		}

		public void ChangeStatBy<T>(ProviderStatID id, T value)
		{
			throw new NotImplementedException();
		}

		public string GetUsername()
		{
			return _userManager.GetCurrentUser().Username;
		}

		public string GetId()
		{
			return _generatedId;
		}

		// TODO: Custom ID generation.
		public IProvider.Friend[] GetFriends()
		{
			// Filter our list of relationships to users online and friends of us.
			_relationshipManager.Filter((ref Relationship relationship) =>
			{
				return relationship.Type == RelationshipType.Friend;
			});

			List<IProvider.Friend> friends = new();

			// Get all relationships.
			for (uint i = 0; i < _relationshipManager.Count(); i++)
			{
				// Get an individual relationship from the list.
				Relationship r = _relationshipManager.GetAt(i);

				// Add the relationship to our friends list.
				friends.Add(new IProvider.Friend
				{
					// Get id from main server.
					Id = r.User.Id.ToString(),
					NativeId = r.User.Id.ToString(),
					Name = r.User.Username,
					Status = r.Presence.Status == Status.Online ? IProvider.FriendStatus.ONLINE : IProvider.FriendStatus.OFFLINE
				});
			}

			return friends.ToArray();
		}

		public Texture2D GetAvatar(string id)
		{
			Texture2D texture = Resources.Load<Texture2D>("UI/default_avatar");

			_discord.GetImageManager().Fetch(ImageHandle.User(Int64.Parse(id)), false, (result, handle) =>
			{
				if (result == Result.Ok)
				{
					texture = _discord.GetImageManager().GetTexture(handle);
				}
			});

			return texture;
		}

		public void InviteUser(string id, string secret)
		{
			Dbg.Log("Inviting user through Discord.");

			_activity.Secrets.Join = secret;
			_activity.Secrets.Spectate = secret;
			_activityManager.UpdateActivity(_activity, result => {});

			_activityManager.SendInvite(Int64.Parse(id), ActivityActionType.Join, "You have been invited to play Hellion!", result =>
			{
				if (result == Result.Ok)
				{
					Dbg.Log("Invite sent.");
				}
				else
				{
					Dbg.Log("Invite failed.");
				}
			});
		}
	}
}
