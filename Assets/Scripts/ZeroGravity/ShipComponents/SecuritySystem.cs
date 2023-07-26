using System;
using System.Collections.Generic;
using System.Linq;
using Nakama;
using OpenHellion.Net;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.ShipComponents
{
	public class SecuritySystem : MonoBehaviour
	{
		public delegate void GetInvitedPlayersDelegate(List<AuthorizedPerson> availablePlayers);

		public List<AuthorizedPerson> AuthorizedPlayers = new List<AuthorizedPerson>();

		public SceneNameTag[] ShipNameTags;

		private GetInvitedPlayersDelegate m_onPlayersLodedDelegate;

		private Ship m_parentShip;

		public Ship ParentShip => m_parentShip;

		public void Awake()
		{
			if (m_parentShip is null)
			{
				m_parentShip = GetComponentInParent<GeometryRoot>().MainObject as Ship;
			}
		}

		public void ChangeVesselName(string newName)
		{
			if (m_parentShip is not null && !newName.IsNullOrEmpty())
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
			if (MyPlayer.Instance.CurrentActiveItem is not null && ItemTypeRange.IsHackingTool(MyPlayer.Instance.CurrentActiveItem.Type))
			{
				NetworkController.Instance.SendToGameServer(new VesselSecurityRequest
				{
					VesselGUID = m_parentShip.GUID,
					HackPanel = true
				});
			}
		}

		public async void GetPlayersForAuthorization(bool getFriends, bool getPlayerFromServer, GetInvitedPlayersDelegate updatePlayerListUI)
		{
			// Cannot be run in single player mode.
			if (getFriends && !Client.Instance.SinglePlayerMode)
			{
				List<AuthorizedPerson> list = new List<AuthorizedPerson>();

				IApiFriend[] nakamaFriends = await Client.Instance.Nakama.GetFriends();
				foreach (IApiFriend friend in nakamaFriends)
				{
					// If friend is online, and not already authorised.
					if (friend.User.Online && AuthorizedPlayers.Find((AuthorizedPerson m) => m.PlayerId == friend.User.Id) is null)
					{
						list.Add(new AuthorizedPerson
						{
							PlayerId = friend.User.Id,
							IsFriend = true,
							Name = friend.User.DisplayName,
							Rank = AuthorizedPersonRank.None
						});
					}
				}

				updatePlayerListUI(list);
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
				m_onPlayersLodedDelegate = updatePlayerListUI;
			}
		}

		public void ParseSecurityData(VesselSecurityData data)
		{
			if (data is null || m_parentShip is null)
			{
				return;
			}
			AuthorizedPersonRank originalRank = GetPlayerRank(MyPlayer.Instance);
			if (m_parentShip.VesselData is null)
			{
				m_parentShip.VesselData = new VesselData();
			}
			if (!data.VesselName.IsNullOrEmpty() && data.VesselName != m_parentShip.VesselData.VesselName)
			{
				m_parentShip.VesselData.VesselName = data.VesselName;
				Client.Instance.Map.InitializeMapObject(m_parentShip);
			}
			AuthorizedPlayers.Clear();
			if (data.AuthorizedPersonel is not null)
			{
				foreach (VesselSecurityAuthorizedPerson item in data.AuthorizedPersonel)
				{
					AuthorizedPlayers.Add(new AuthorizedPerson
					{
						IsFriend = false,
						Name = item.Name,
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
			if (data.PlayersOnServer is not null && data.PlayersOnServer.Count > 0)
			{
				foreach (PlayerOnServerData item in data.PlayersOnServer)
				{
					if (item.Name != MyPlayer.Instance.PlayerName)
					{
						list.Add(new AuthorizedPerson
						{
							IsFriend = false,
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
			if (MyPlayer.Instance.LockedToTrigger is not null && MyPlayer.Instance.LockedToTrigger.TriggerType == SceneTriggerType.SecurityScreen)
			{
				Client.Instance.InGamePanels.Security.UpdateUI();
			}
		}

		public void UpdateSelfDestructTimer()
		{
			if (MyPlayer.Instance.LockedToTrigger is not null && MyPlayer.Instance.LockedToTrigger.TriggerType == SceneTriggerType.SecurityScreen)
			{
				Client.Instance.InGamePanels.Security.RefreshSelfDestructTimer();
			}
		}
	}
}
