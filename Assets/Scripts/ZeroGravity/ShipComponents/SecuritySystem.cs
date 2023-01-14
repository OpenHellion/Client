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

namespace ZeroGravity.ShipComponents
{
	public class SecuritySystem : MonoBehaviour
	{
		[Serializable]
		public class PlayerSecurityData
		{
			public long GUID;

			public string PlayerNativeId;

			public string PlayerId;

			public string Name;

			public bool IsFriend;

			public AuthorizedPersonRank Rank;
		}

		public delegate void GetInvitedPlayersDelegate(List<PlayerSecurityData> availablePlayers);

		[ReadOnly]
		public List<PlayerSecurityData> AuthorizedPlayers = new List<PlayerSecurityData>();

		private Ship parentShip;

		public SceneNameTag[] ShipNameTags;

		private GetInvitedPlayersDelegate onPlayersLodedDelegate;

		public Ship ParentShip => parentShip;

		public void Awake()
		{
			if (parentShip == null)
			{
				parentShip = GetComponentInParent<GeometryRoot>().MainObject as Ship;
			}
		}

		public void ChangeVesselName(string newName)
		{
			if (!(parentShip == null) && !newName.IsNullOrEmpty())
			{
				NetworkController.Instance.SendToGameServer(new VesselSecurityRequest
				{
					VesselGUID = parentShip.GUID,
					VesselName = newName
				});
			}
		}

		public AuthorizedPersonRank GetPlayerRank(Player pl)
		{
			return AuthorizedPlayers.Find((PlayerSecurityData m) => m.PlayerId == pl.PlayerId)?.Rank ?? AuthorizedPersonRank.None;
		}

		public void AddPerson(PlayerSecurityData player, AuthorizedPersonRank newRank)
		{
			NetworkController.Instance.SendToGameServer(new VesselSecurityRequest
			{
				VesselGUID = parentShip.GUID,
				AddPlayerId = player.PlayerId,
				AddPlayerRank = newRank,
				AddPlayerName = player.Name
			});
		}

		public void RemovePerson(PlayerSecurityData player)
		{
			NetworkController.Instance.SendToGameServer(new VesselSecurityRequest
			{
				VesselGUID = parentShip.GUID,
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
					VesselGUID = parentShip.GUID,
					HackPanel = true
				});
			}
		}

		public void GetPlayersForAuthorization(bool getFriends, bool getPlayerFromServer, GetInvitedPlayersDelegate onPlayersLoaded)
		{
			if (getFriends)
			{
				List<PlayerSecurityData> list = new List<PlayerSecurityData>();

				// Loop through each friend and add it to the list.
				foreach (IProvider.Friend friend in ProviderManager.MainProvider.GetFriends())
				{
					// If friend is online, and not already authorised.
					if (friend.Status == IProvider.FriendStatus.ONLINE && AuthorizedPlayers.Find((PlayerSecurityData m) => m.PlayerNativeId == friend.NativeId) == null)
					{
						list.Add(new PlayerSecurityData
						{
							PlayerNativeId = friend.NativeId,
							IsFriend = true,
							Name = friend.Name,
							Rank = AuthorizedPersonRank.None
						});
					}
				}

				onPlayersLoaded(list);
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
				onPlayersLodedDelegate = onPlayersLoaded;
			}
		}

		public void ParseSecurityData(VesselSecurityData data)
		{
			if (data == null || parentShip == null)
			{
				return;
			}
			AuthorizedPersonRank playerRank = GetPlayerRank(MyPlayer.Instance);
			if (parentShip.VesselData == null)
			{
				parentShip.VesselData = new VesselData();
			}
			if (!data.VesselName.IsNullOrEmpty() && data.VesselName != parentShip.VesselData.VesselName)
			{
				parentShip.VesselData.VesselName = data.VesselName;
				Client.Instance.Map.InitializeMapObject(parentShip);
			}
			AuthorizedPlayers.Clear();
			if (data.AuthorizedPersonel != null)
			{
				foreach (VesselSecurityAuthorizedPerson item in data.AuthorizedPersonel)
				{
					AuthorizedPlayers.Add(new PlayerSecurityData
					{
						IsFriend = false,
						GUID = item.GUID,
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
			AuthorizedPersonRank playerRank2 = GetPlayerRank(MyPlayer.Instance);
			if (playerRank != playerRank2)
			{
				if (playerRank == AuthorizedPersonRank.None && playerRank2 == AuthorizedPersonRank.CommandingOfficer)
				{
					SceneQuestTrigger.Check(base.gameObject, SceneQuestTriggerEvent.Claim);
				}
				else if (playerRank == AuthorizedPersonRank.CommandingOfficer && playerRank2 == AuthorizedPersonRank.None)
				{
					SceneQuestTrigger.Check(base.gameObject, SceneQuestTriggerEvent.Resign);
				}
			}
		}

		public void ParsePlayersOnServerResponse(PlayersOnServerResponse data)
		{
			if (onPlayersLodedDelegate == null)
			{
				return;
			}
			List<PlayerSecurityData> list = new List<PlayerSecurityData>();
			if (data.PlayersOnServer != null && data.PlayersOnServer.Count > 0)
			{
				foreach (PlayerOnServerData item in data.PlayersOnServer)
				{
					if (item.Name != MyPlayer.Instance.PlayerName)
					{
						list.Add(new PlayerSecurityData
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
				onPlayersLodedDelegate(list);
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
