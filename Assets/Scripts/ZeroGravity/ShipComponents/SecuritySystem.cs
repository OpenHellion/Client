using System;
using System.Collections.Generic;
using System.Linq;
using OpenHellion.Networking;
using OpenHellion.ProviderSystem;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using TriInspector;
using OpenHellion.Networking.Message.MainServer;
using OpenHellion.Networking.Message;

namespace ZeroGravity.ShipComponents
{
	public class SecuritySystem : MonoBehaviour
	{
		public delegate void GetInvitedPlayersDelegate(List<AuthorizedPerson> availablePlayers);

		[ReadOnly]
		public List<AuthorizedPerson> AuthorizedPlayers = new List<AuthorizedPerson>();

		public SceneNameTag[] ShipNameTags;

		private GetInvitedPlayersDelegate m_onPlayersLodedDelegate;

		private Ship m_parentShip;

		public Ship ParentShip => m_parentShip;

		public void Awake()
		{
			if (m_parentShip == null)
			{
				m_parentShip = GetComponentInParent<GeometryRoot>().MainObject as Ship;
			}
		}

		public void ChangeVesselName(string newName)
		{
			if (!(m_parentShip == null) && !newName.IsNullOrEmpty())
			{
				NetworkController.Instance.SendToGameServer(new VesselSecurityRequest
				{
					VesselGUID = m_parentShip.GUID,
					VesselName = newName
				});
			}
		}

		public AuthorizedPersonRank GetPlayerRank(Player pl)
		{
			return AuthorizedPlayers.Find((AuthorizedPerson m) => m.PlayerId == pl.PlayerId)?.Rank ?? AuthorizedPersonRank.None;
		}

		public void AddPerson(AuthorizedPerson player, AuthorizedPersonRank newRank)
		{
			NetworkController.Instance.SendToGameServer(new VesselSecurityRequest
			{
				VesselGUID = m_parentShip.GUID,
				AddPlayerId = player.PlayerId,
				AddPlayerRank = newRank,
				AddPlayerName = player.Name
			});
		}

		public void RemovePerson(AuthorizedPerson player)
		{
			NetworkController.Instance.SendToGameServer(new VesselSecurityRequest
			{
				VesselGUID = m_parentShip.GUID,
				RemovePlayerId = player.PlayerId
			});
			UpdateUI();
		}

		public void Hack()
		{
			if (!(MyPlayer.Instance.CurrentActiveItem == null) && ItemTypeRange.IsHackingTool(MyPlayer.Instance.CurrentActiveItem.Type))
			{
				NetworkController.Instance.SendToGameServer(new VesselSecurityRequest
				{
					VesselGUID = m_parentShip.GUID,
					HackPanel = true
				});
			}
		}

		public void GetPlayersForAuthorization(bool getFriends, bool getPlayerFromServer, GetInvitedPlayersDelegate onPlayersLoaded)
		{
			// Cannot be run in singleplayer mode.
			if (getFriends && !Client.Instance.SinglePlayerMode)
			{
				List<AuthorizedPerson> list = new List<AuthorizedPerson>();
				GetPlayerIdRequest req = new GetPlayerIdRequest();

				List<IProvider.Friend> friends = new List<IProvider.Friend>();

				// Loop through each friend and add it to the list.
				foreach (IProvider.Friend friend in ProviderManager.MainProvider.GetFriends())
				{
					req.Ids.Add(new GetPlayerIdRequest.Entry {
						SteamId = ProviderManager.SteamId,
						DiscordId = ProviderManager.DiscordId
					});

					friends.Add(friend);
				}

				// This has the added benefit of filtering out all non-players.
				MSConnection.Get<PlayerIdResponse>(req, (res) => {
					// Loop through all entries in the same order as before.
					for (int i = 0; i < res.PlayerIds.Count; i++)
					{
						if (res.PlayerIds[i] == "-1") continue;

						// If friend is online, and not already authorised.
						if (friends[i].Status == IProvider.FriendStatus.ONLINE && AuthorizedPlayers.Find((AuthorizedPerson m) => m.PlayerId == res.PlayerIds[i]) == null)
						{
							list.Add(new AuthorizedPerson
							{
								PlayerNativeId = friends[i].NativeId,
								PlayerId = res.PlayerIds[i],
								IsFriend = true,
								Name = friends[i].Name,
								Rank = AuthorizedPersonRank.None
							});
						}
					}

					onPlayersLoaded(list);
				});
			}

			if (getPlayerFromServer)
			{
				NetworkController.Instance.SendToGameServer(new PlayersOnServerRequest
				{
					SecuritySystemID = new VesselObjectID
					{
						VesselGUID = ParentShip.GUID,
						InSceneID = 0
					}
				});
				m_onPlayersLodedDelegate = onPlayersLoaded;
			}
		}

		public void ParseSecurityData(VesselSecurityData data)
		{
			if (data == null || m_parentShip == null)
			{
				return;
			}
			AuthorizedPersonRank originalRank = GetPlayerRank(MyPlayer.Instance);
			if (m_parentShip.VesselData == null)
			{
				m_parentShip.VesselData = new VesselData();
			}
			if (!data.VesselName.IsNullOrEmpty() && data.VesselName != m_parentShip.VesselData.VesselName)
			{
				m_parentShip.VesselData.VesselName = data.VesselName;
				Client.Instance.Map.InitializeMapObject(m_parentShip);
			}
			AuthorizedPlayers.Clear();
			if (data.AuthorizedPersonel != null)
			{
				foreach (VesselSecurityAuthorizedPerson item in data.AuthorizedPersonel)
				{
					AuthorizedPlayers.Add(new AuthorizedPerson
					{
						IsFriend = false,
						Name = item.Name,
						PlayerNativeId = item.PlayerNativeId,
						PlayerId = item.PlayerId,
						Rank = item.Rank
					});
				}
			}
			if (AuthorizedPlayers.Count > 1)
			{
				AuthorizedPlayers = (from m in AuthorizedPlayers
					orderby m.Rank, m.Name
					select m).ToList();
			}
			UpdateUI();
			AuthorizedPersonRank updatedRank = GetPlayerRank(MyPlayer.Instance);
			if (originalRank != updatedRank)
			{
				if (originalRank == AuthorizedPersonRank.None && updatedRank == AuthorizedPersonRank.CommandingOfficer)
				{
					SceneQuestTrigger.OnTrigger(gameObject, SceneQuestTriggerEvent.Claim);
				}
				else if (originalRank == AuthorizedPersonRank.CommandingOfficer && updatedRank == AuthorizedPersonRank.None)
				{
					SceneQuestTrigger.OnTrigger(gameObject, SceneQuestTriggerEvent.Resign);
				}
			}
		}

		/// <summary>
		/// 	Get a list of all player on server. Used by function GetPlayersForAuthorization.
		/// </summary>
		public void ParsePlayersOnServerResponse(PlayersOnServerResponse data)
		{
			if (m_onPlayersLodedDelegate == null)
			{
				return;
			}
			List<AuthorizedPerson> list = new List<AuthorizedPerson>();
			if (data.PlayersOnServer != null && data.PlayersOnServer.Count > 0)
			{
				foreach (PlayerOnServerData item in data.PlayersOnServer)
				{
					if (item.Name != MyPlayer.Instance.PlayerName)
					{
						list.Add(new AuthorizedPerson
						{
							IsFriend = false,
							PlayerNativeId = item.PlayerNativeId,
							PlayerId = item.PlayerId,
							Name = item.Name,
							Rank = AuthorizedPersonRank.None
						});
					}
				}
			}
			if (list.Count > 0)
			{
				m_onPlayersLodedDelegate(list);
			}
		}

		public void UpdateUI()
		{
			if (MyPlayer.Instance.LockedToTrigger != null && MyPlayer.Instance.LockedToTrigger.TriggerType == SceneTriggerType.SecurityScreen)
			{
				Client.Instance.InGamePanels.Security.UpdateUI();
			}
		}

		public void UpdateSelfDestructTimer()
		{
			if (MyPlayer.Instance.LockedToTrigger != null && MyPlayer.Instance.LockedToTrigger.TriggerType == SceneTriggerType.SecurityScreen)
			{
				Client.Instance.InGamePanels.Security.RefreshSelfDestructTimer();
			}
		}
	}
}
