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
	/// 	This class is a mess and needs to be rewritten completely. TODO
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

		public string clientID = "349114016968474626";
		public uint optionalSteamId = 588210;
		public int callbackCalls;

		Discord.Discord discord;
		ActivityManager activityManager;
		Activity activity = new();
		User joinUser;

		[Tooltip("When player responds to an 'ask to join' request. ")]
		public UnityEngine.Events.UnityEvent hasResponded;

		private void OnEnable()
		{
			Dbg.Log("Discord: Init");
			callbackCalls = 0;

			discord = new(Int64.Parse(clientID), (UInt64)CreateFlags.NoRequireDiscord);

			discord.SetLogHook(Discord.LogLevel.Debug, (level, message) =>
			{
		   		Dbg.Log("Log[{0}] {1}", level, message);
			});

			activityManager = discord.GetActivityManager();

			activityManager.RegisterSteam(optionalSteamId);

			activityManager.OnActivitySpectate += SpectateCallback;
			activityManager.OnActivityJoin += JoinCallback;
			activityManager.OnActivityInvite += InviteCallback;
			activityManager.OnActivityJoinRequest += JoinRequestCallback;

			activityManager.RegisterCommand();

			UpdateStatus();
		}

		private void Update()
		{
			discord.RunCallbacks();
		}

		private void SpectateCallback(string secret)
		{
			callbackCalls++;
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

		private void JoinCallback(string secret)
		{
			callbackCalls++;
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

		private void InviteCallback(ActivityActionType Type, ref User user, ref Activity activity2)
		{
			callbackCalls++;
		}

		private void JoinRequestCallback(ref User user)
		{
			Dbg.Log(string.Format("Discord: Join request {0}#{1}: {2}", user.Username, user.Discriminator, user.Id));
			callbackCalls++;
			/*activityManager.AcceptInvite(user.Id, result => {
				Dbg.Log("AcceptInvite {0}", result);
			});*/

			joinUser = user;
		}

		public void RequestRespondYes()
		{
			Dbg.Log("Discord: Responding yes to Ask to Join request");
			activityManager.SendRequestReply(joinUser.Id, Discord.ActivityJoinRequestReply.Yes, res => {
				if (res == Discord.Result.Ok)
	  			{
	  			  Console.WriteLine("Responded successfully");
	  			}
			});
			hasResponded.Invoke();
		}

		public void RequestRespondNo()
		{
			Dbg.Log("Discord: Responding no to Ask to Join request");
			activityManager.SendRequestReply(joinUser.Id, Discord.ActivityJoinRequestReply.No, res => {
				if (res == Discord.Result.Ok)
	  			{
	  			  Console.WriteLine("Responded successfully");
	  			}
			});
			hasResponded.Invoke();
		}

		private void OnDisable()
		{
			Dbg.Log("Discord: Shutdown");
			discord.Dispose();
		}

		public void UpdateStatus()
		{
			try
			{
				if (Client.Instance.SinglePlayerMode)
				{
					activity.State = "Playing single player game";
					activity.Details = "Having so much fun.";
					activity.Assets.LargeImage = "cover";
					activity.Assets.LargeText = string.Empty;
					activity.Assets.SmallImage = string.Empty;
					activity.Assets.SmallText = string.Empty;
					activity.Secrets.Join = string.Empty;
					activity.Party.Size.CurrentSize = 0;
					activity.Party.Size.MaxSize = 0;
					activity.Party.Id = string.Empty;
				}
				else if (MyPlayer.Instance != null && MyPlayer.Instance.PlayerReady)
				{
					activity.Secrets.Join = Client.Instance.GetInviteString(null);
					activity.Assets.LargeText = Localization.InGameDescription + ": " + Client.LastConnectedServer.Name;
					activity.Details = descriptions[UnityEngine.Random.Range(0, descriptions.Count - 1)];
					ArtificialBody artificialBody = MyPlayer.Instance.Parent as ArtificialBody;
					if (artificialBody != null && artificialBody.ParentCelesitalBody != null)
					{
						string value;
						if (planets.TryGetValue(artificialBody.ParentCelesitalBody.GUID, out value))
						{
							activity.Assets.LargeImage = artificialBody.ParentCelesitalBody.GUID.ToString();
						}
						else
						{
							activity.Assets.LargeImage = "default";
							value = artificialBody.ParentCelesitalBody.Name;
						}
						if (artificialBody is Ship && (artificialBody as Ship).IsWarpOnline)
						{
							activity.State = Localization.WarpingNear + " " + value.ToUpper();
						}
						else if (artificialBody is Pivot)
						{
							activity.State = Localization.FloatingFreelyNear + " " + value.ToUpper();
						}
						else
						{
							activity.State = Localization.OrbitingNear + " " + value.ToUpper();
						}
					}
					activity.Assets.SmallImage = Client.Instance.CurrentGender.ToLocalizedString().ToLower();
					activity.Assets.SmallText = MyPlayer.Instance.PlayerName;
					activity.Party.Size.CurrentSize = Client.LastConnectedServer.CurrentPlayers + 1;
					activity.Party.Size.MaxSize = Client.LastConnectedServer.MaxPlayers;
					activity.Party.Id = Client.LastConnectedServer.Hash.ToString();
				}
				else
				{
					activity.State = "In Menus";
					activity.Details = "Launch Sequence Initiated";
					activity.Assets.LargeImage = "cover";
					activity.Assets.LargeText = string.Empty;
					activity.Assets.SmallImage = string.Empty;
					activity.Assets.SmallText = string.Empty;
					activity.Secrets.Join = string.Empty;
					activity.Party.Size.CurrentSize = 0;
					activity.Party.Size.MaxSize = 0;
					activity.Party.Id = string.Empty;
				}
				activityManager.UpdateActivity(activity, result => {});
			}
			catch (Exception ex)
			{
				Dbg.Error(ex.Message, ex.StackTrace);
			}
		}
	}
}
