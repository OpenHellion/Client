using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.Discord
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

		public DiscordRpc.RichPresence presence = new();
		public string applicationId = "349114016968474626";
		public string optionalSteamId = "588210";
		public int callbackCalls;
		public int clickCounter;
		public DiscordRpc.DiscordUser joinRequest;
		public UnityEngine.Events.UnityEvent onConnect;
		public UnityEngine.Events.UnityEvent onDisconnect;
		public UnityEngine.Events.UnityEvent hasResponded;

		private DiscordRpc.EventHandlers handlers;

		public void UpdateStatus()
		{
			try
			{
				if (Client.Instance.SinglePlayerMode)
				{
					presence.state = "Playing single player game";
					presence.details = "Having so much fun.";
					presence.largeImageKey = "cover";
					presence.largeImageText = string.Empty;
					presence.smallImageKey = string.Empty;
					presence.smallImageText = string.Empty;
					presence.joinSecret = string.Empty;
					presence.partySize = 0;
					presence.partyId = string.Empty;
					presence.partyMax = 0;
				}
				else if (MyPlayer.Instance != null && MyPlayer.Instance.PlayerReady)
				{
					presence.joinSecret = Client.Instance.GetInviteString(null);
					presence.largeImageText = Localization.InGameDescription + ": " + Client.LastConnectedServer.Name;
					presence.details = descriptions[UnityEngine.Random.Range(0, descriptions.Count - 1)];
					ArtificialBody artificialBody = MyPlayer.Instance.Parent as ArtificialBody;
					if (artificialBody != null && artificialBody.ParentCelesitalBody != null)
					{
						string value;
						if (planets.TryGetValue(artificialBody.ParentCelesitalBody.GUID, out value))
						{
							presence.largeImageKey = artificialBody.ParentCelesitalBody.GUID.ToString();
						}
						else
						{
							presence.largeImageKey = "default";
							value = artificialBody.ParentCelesitalBody.Name;
						}
						if (artificialBody is Ship && (artificialBody as Ship).IsWarpOnline)
						{
							presence.state = Localization.WarpingNear + " " + value.ToUpper();
						}
						else if (artificialBody is Pivot)
						{
							presence.state = Localization.FloatingFreelyNear + " " + value.ToUpper();
						}
						else
						{
							presence.state = Localization.OrbitingNear + " " + value.ToUpper();
						}
					}
					presence.smallImageKey = Client.Instance.CurrentGender.ToLocalizedString().ToLower();
					presence.smallImageText = MyPlayer.Instance.PlayerName;
					presence.partySize = Client.LastConnectedServer.CurrentPlayers + 1;
					presence.partyId = Client.LastConnectedServer.Hash.ToString();
					presence.partyMax = Client.LastConnectedServer.MaxPlayers;
				}
				else
				{
					presence.state = "In Menus";
					presence.details = "Launch Sequence Initiated";
					presence.largeImageKey = "cover";
					presence.largeImageText = string.Empty;
					presence.smallImageKey = string.Empty;
					presence.smallImageText = string.Empty;
					presence.joinSecret = string.Empty;
					presence.partySize = 0;
					presence.partyId = string.Empty;
					presence.partyMax = 0;
				}
				DiscordRpc.UpdatePresence(presence);
			}
			catch (Exception ex)
			{
				Dbg.Error(ex.Message, ex.StackTrace);
			}
		}

		public void OnClick()
		{
			Dbg.Log("Discord: On click!");
			clickCounter++;

			presence.details = string.Format("Button clicked {0} times", clickCounter);
			presence.joinSecret = "aSecret";
			presence.partyId = "aPartyId";
			presence.partySize = 1;
			presence.partyMax = 3;
			presence.partyPrivacy = DiscordRpc.PartyPrivacy.Public;

			DiscordRpc.UpdatePresence(presence);
		}

		public void RequestRespondYes()
		{
			Dbg.Log("Discord: Responding yes to Ask to Join request");
			DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.Yes);
			hasResponded.Invoke();
		}
	
		public void RequestRespondNo()
		{
			Dbg.Log("Discord: Responding no to Ask to Join request");
			DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.No);
			hasResponded.Invoke();
		}

		public void ReadyCallback(ref DiscordRpc.DiscordUser connectedUser)
		{
			callbackCalls++;
			onConnect.Invoke();
		}

		public void DisconnectedCallback(int errorCode, string message)
		{
			callbackCalls++;
			onDisconnect.Invoke();
		}

		public void ErrorCallback(int errorCode, string message)
		{
			callbackCalls++;
		}

		public void JoinCallback(string secret)
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

		public void SpectateCallback(string secret)
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

		public void RequestCallback(ref DiscordRpc.DiscordUser request)
		{
			Dbg.Log(string.Format("Discord: Join request {0}#{1}: {2}", request.username, request.discriminator, request.userId));
			callbackCalls++;
			DiscordRpc.Respond(request.userId, DiscordRpc.Reply.Yes);

			joinRequest = request;
		}

		private void Update()
		{
			DiscordRpc.RunCallbacks();
		}

		private void OnEnable()
		{
			Dbg.Log("Discord: Init");
			callbackCalls = 0;
			handlers = new DiscordRpc.EventHandlers();
			handlers.readyCallback += ReadyCallback;
			handlers.disconnectedCallback += DisconnectedCallback;
			handlers.errorCallback += ErrorCallback;
			handlers.joinCallback += JoinCallback;
			handlers.spectateCallback += SpectateCallback;
			handlers.requestCallback += RequestCallback;
			DiscordRpc.Initialize(applicationId, ref handlers, true, optionalSteamId);
			UpdateStatus();
		}

		private void OnDisable()
		{
			Dbg.Log("Discord: Shutdown");
			DiscordRpc.Shutdown();
		}

		private void OnDestroy()
		{
		}
	}
}
