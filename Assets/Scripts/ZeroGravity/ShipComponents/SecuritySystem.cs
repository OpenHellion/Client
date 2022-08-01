using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Steamworks;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.ShipComponents
{
	public class SecuritySystem : MonoBehaviour
	{
		public class PlayerSecurityData
		{
			public long GUID;

			public string SteamID;

			public string Name;

			public bool IsSteamFriend;

			public AuthorizedPersonRank Rank;
		}

		public delegate void GetInvitedPlayersDelegate(List<PlayerSecurityData> availablePlayers);

		[CompilerGenerated]
		private sealed class _003CGetPlayerRank_003Ec__AnonStorey0
		{
			internal Player pl;

			internal bool _003C_003Em__0(PlayerSecurityData m)
			{
				return m.GUID == pl.GUID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CGetPlayersForAuthorization_003Ec__AnonStorey1
		{
			internal CSteamID id;

			internal bool _003C_003Em__0(PlayerSecurityData m)
			{
				return m.SteamID == id.m_SteamID.ToString();
			}
		}

		public List<PlayerSecurityData> AuthorizedPlayers = new List<PlayerSecurityData>();

		private Ship parentShip;

		public SceneNameTag[] ShipNameTags;

		private GetInvitedPlayersDelegate onPlayersLodedDelegate;

		[CompilerGenerated]
		private static Func<PlayerSecurityData, AuthorizedPersonRank> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<PlayerSecurityData, string> _003C_003Ef__am_0024cache1;

		public Ship ParentShip
		{
			get
			{
				return parentShip;
			}
		}

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
				Client.Instance.NetworkController.SendToGameServer(new VesselSecurityRequest
				{
					VesselGUID = parentShip.GUID,
					VesselName = newName
				});
			}
		}

		public AuthorizedPersonRank GetPlayerRank(Player pl)
		{
			_003CGetPlayerRank_003Ec__AnonStorey0 _003CGetPlayerRank_003Ec__AnonStorey = new _003CGetPlayerRank_003Ec__AnonStorey0();
			_003CGetPlayerRank_003Ec__AnonStorey.pl = pl;
			PlayerSecurityData playerSecurityData = AuthorizedPlayers.Find(_003CGetPlayerRank_003Ec__AnonStorey._003C_003Em__0);
			if (playerSecurityData != null)
			{
				return playerSecurityData.Rank;
			}
			return AuthorizedPersonRank.None;
		}

		public void AddPerson(PlayerSecurityData player, AuthorizedPersonRank newRank)
		{
			Client.Instance.NetworkController.SendToGameServer(new VesselSecurityRequest
			{
				VesselGUID = parentShip.GUID,
				AddPlayerSteamID = player.SteamID,
				AddPlayerRank = newRank,
				AddPlayerName = player.Name
			});
		}

		public void RemovePerson(PlayerSecurityData player)
		{
			Client.Instance.NetworkController.SendToGameServer(new VesselSecurityRequest
			{
				VesselGUID = parentShip.GUID,
				RemovePlayerSteamID = player.SteamID
			});
			UpdateUI();
		}

		public void Hack()
		{
			if (!(MyPlayer.Instance.CurrentActiveItem == null) && ItemTypeRange.IsHackingTool(MyPlayer.Instance.CurrentActiveItem.Type))
			{
				Client.Instance.NetworkController.SendToGameServer(new VesselSecurityRequest
				{
					VesselGUID = parentShip.GUID,
					HackPanel = true
				});
			}
		}

		public void GetPlayersForAuthorization(bool getSteamFriends, bool getPlayerFromServer, GetInvitedPlayersDelegate onPlayersLoaded)
		{
			if (getSteamFriends && SteamManager.Initialized)
			{
				List<PlayerSecurityData> list = new List<PlayerSecurityData>();
				for (int i = 0; i < SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate); i++)
				{
					_003CGetPlayersForAuthorization_003Ec__AnonStorey1 _003CGetPlayersForAuthorization_003Ec__AnonStorey = new _003CGetPlayersForAuthorization_003Ec__AnonStorey1();
					_003CGetPlayersForAuthorization_003Ec__AnonStorey.id = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
					EPersonaState friendPersonaState = SteamFriends.GetFriendPersonaState(_003CGetPlayersForAuthorization_003Ec__AnonStorey.id);
					if ((friendPersonaState == EPersonaState.k_EPersonaStateOnline || friendPersonaState == EPersonaState.k_EPersonaStateLookingToPlay) && AuthorizedPlayers.Find(_003CGetPlayersForAuthorization_003Ec__AnonStorey._003C_003Em__0) == null)
					{
						list.Add(new PlayerSecurityData
						{
							SteamID = _003CGetPlayersForAuthorization_003Ec__AnonStorey.id.m_SteamID.ToString(),
							IsSteamFriend = true,
							Name = SteamFriends.GetFriendPersonaName(_003CGetPlayersForAuthorization_003Ec__AnonStorey.id),
							Rank = AuthorizedPersonRank.None
						});
					}
				}
				onPlayersLoaded(list);
			}
			if (getPlayerFromServer)
			{
				Client.Instance.NetworkController.SendToGameServer(new PlayersOnServerRequest
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
						IsSteamFriend = false,
						GUID = item.GUID,
						Name = item.Name,
						SteamID = item.SteamID,
						Rank = item.Rank
					});
				}
			}
			if (AuthorizedPlayers.Count > 1)
			{
				List<PlayerSecurityData> authorizedPlayers = AuthorizedPlayers;
				if (_003C_003Ef__am_0024cache0 == null)
				{
					_003C_003Ef__am_0024cache0 = _003CParseSecurityData_003Em__0;
				}
				IOrderedEnumerable<PlayerSecurityData> source = authorizedPlayers.OrderBy(_003C_003Ef__am_0024cache0);
				if (_003C_003Ef__am_0024cache1 == null)
				{
					_003C_003Ef__am_0024cache1 = _003CParseSecurityData_003Em__1;
				}
				AuthorizedPlayers = source.ThenBy(_003C_003Ef__am_0024cache1).ToList();
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
							IsSteamFriend = false,
							SteamID = item.SteamID,
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

		[CompilerGenerated]
		private static AuthorizedPersonRank _003CParseSecurityData_003Em__0(PlayerSecurityData m)
		{
			return m.Rank;
		}

		[CompilerGenerated]
		private static string _003CParseSecurityData_003Em__1(PlayerSecurityData m)
		{
			return m.Name;
		}
	}
}
