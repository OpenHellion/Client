using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using Discord;

namespace ZeroGravity
{
	/// <summary>
	/// 	This class handles everything related to Discord. Acts as a bridge between the game and the API.
	/// </summary>
	public class DiscordController : MonoBehaviour
	{
		private static Dictionary<long, string> planets = new Dictionary<long, string>
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

		private static List<string> descriptions = new List<string> { "Building a huuuuuge station", "Mining asteroids", "In a salvaging mission", "Doing a piracy job", "Repairing a hull breach" };

		public long ClientID = 349114016968474626L;
		public uint OptionalSteamId = 588210;

		private int _callbackCalls;
		private Discord.Discord _discord;
		private ActivityManager _activityManager;
		private Activity _activity = new();
		public User _joinUser;

		[Tooltip("When player responds to an 'ask to join' request. ")]
		public UnityEvent HasResponded;

		private void OnEnable()
		{
			Dbg.Log("Discord: Init");
			_callbackCalls = 0;

			// Init Discord API.
			try
			{
				_discord = new(ClientID, (UInt64)CreateFlags.NoRequireDiscord);
			}
			catch (ResultException)
			{
				Dbg.Log("Discord API could not start.");
				this.enabled = false;
				return;
			}

			_discord.SetLogHook(Discord.LogLevel.Debug, (level, message) =>
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

			UpdateStatus();
		}

		private void Update()
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
				if (res == Discord.Result.Ok)
	  			{
	  			  Console.WriteLine("Responded successfully");
	  			}
			});
			HasResponded.Invoke();
		}

		public void RequestRespondNo()
		{
			Dbg.Log("Discord: Responding no to Ask to Join request");
			_activityManager.SendRequestReply(_joinUser.Id, ActivityJoinRequestReply.No, res => {
				if (res == Discord.Result.Ok)
	  			{
	  			  Console.WriteLine("Responded successfully");
	  			}
			});
			HasResponded.Invoke();
		}

		private void OnDisable()
		{
			Dbg.Log("Discord: Shutdown");

			try
			{
				_discord.Dispose();
			}
			catch (NullReferenceException) {}
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
					_activity.Details = descriptions[UnityEngine.Random.Range(0, descriptions.Count - 1)];
					ArtificialBody artificialBody = MyPlayer.Instance.Parent as ArtificialBody;
					if (artificialBody != null && artificialBody.ParentCelesitalBody != null)
					{
						string value;
						if (planets.TryGetValue(artificialBody.ParentCelesitalBody.GUID, out value))
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

				if (_activityManager != null)
				{
					_activityManager.UpdateActivity(_activity, result => {});
				}
			}
			catch (Exception ex)
			{
				Dbg.Error(ex.Message, ex.StackTrace);
			}
		}
	}
}
