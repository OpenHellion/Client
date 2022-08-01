using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class SecurityScreen : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003CUpdateSecurityList_003Ec__AnonStorey0
		{
			internal SecuritySystem.PlayerSecurityData crewman;

			internal SecurityScreen _0024this;

			internal void _003C_003Em__0()
			{
				_0024this.CrewMemberActions(crewman);
			}
		}

		[CompilerGenerated]
		private sealed class _003CGetPlayersDelegate_003Ec__AnonStorey1
		{
			internal SecuritySystem.PlayerSecurityData pl;

			internal SecurityScreen _0024this;
		}

		[CompilerGenerated]
		private sealed class _003CGetPlayersDelegate_003Ec__AnonStorey2
		{
			internal SecuritySystem.PlayerSecurityData plForDeleg;

			internal _003CGetPlayersDelegate_003Ec__AnonStorey1 _003C_003Ef__ref_00241;

			internal bool _003C_003Em__0(SecuritySystem.PlayerSecurityData m)
			{
				return m.SteamID == _003C_003Ef__ref_00241.pl.SteamID;
			}

			internal void _003C_003Em__1()
			{
				_003C_003Ef__ref_00241._0024this.AddToCrew(plForDeleg);
			}
		}

		public SecuritySystem SecuritySystem;

		public GameObject FreeTerminal;

		public GameObject RegistredTerminal;

		public Button ClaimButton;

		public Button InviteButton;

		public Button ResignButton;

		public Button ChangeNameButton;

		public Button ChangeEmblemButton;

		public Button ShipCrewButton;

		[Space(20f)]
		public Text PlayerName;

		public RawImage PlayerImage;

		public Texture DefaultTexture;

		public Text CustomShipName;

		public Text DefaultShipName;

		public GameObject EnterCustomNamePopUp;

		public InputField CustomShipNameInputField;

		public GameObject ResignAlertBox;

		public GameObject CrewMemberPref;

		private List<CrewMembersUI> crewMembersList = new List<CrewMembersUI>();

		public GameObject CrewMemberPanel;

		public GameObject PromotePlayerBox;

		public GameObject ShipCrewList;

		public GameObject PlayerToInvitePref;

		public GameObject InviteList;

		public Dictionary<string, InvitePlayerToPod> AvailablePlayersForInvite = new Dictionary<string, InvitePlayerToPod>();

		private SecuritySystem.PlayerSecurityData currentCrewman;

		public GameObject SelfDestructBox;

		public GameObject SelfDestructWarning;

		public Button SelfDestructButton;

		public Text SelfDestructState;

		public Text SelfDestructTimer;

		private string currentEmblemId;

		public GameObject ChooseEmblemPanel;

		public Transform EmblemParent;

		public GameObject EmblemObject;

		public Texture DefaultEmblemTexture;

		[CompilerGenerated]
		private static Predicate<SecuritySystem.PlayerSecurityData> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Predicate<SecuritySystem.PlayerSecurityData> _003C_003Ef__am_0024cache1;

		[CompilerGenerated]
		private static Predicate<SecuritySystem.PlayerSecurityData> _003C_003Ef__am_0024cache2;

		[CompilerGenerated]
		private static Predicate<SecuritySystem.PlayerSecurityData> _003C_003Ef__am_0024cache3;

		[CompilerGenerated]
		private static Predicate<SecuritySystem.PlayerSecurityData> _003C_003Ef__am_0024cache4;

		[CompilerGenerated]
		private static Predicate<SecuritySystem.PlayerSecurityData> _003C_003Ef__am_0024cache5;

		[CompilerGenerated]
		private static Predicate<SecuritySystem.PlayerSecurityData> _003C_003Ef__am_0024cache6;

		[CompilerGenerated]
		private static Predicate<SecuritySystem.PlayerSecurityData> _003C_003Ef__am_0024cache7;

		private void Start()
		{
			UpdateUI();
			ClaimButton.onClick.AddListener(_003CStart_003Em__0);
			ResignButton.onClick.AddListener(_003CStart_003Em__1);
			ChangeNameButton.onClick.AddListener(_003CStart_003Em__2);
			ChangeEmblemButton.onClick.AddListener(_003CStart_003Em__3);
			InviteButton.onClick.AddListener(_003CStart_003Em__4);
			ShipCrewButton.onClick.AddListener(_003CStart_003Em__5);
			SelfDestructButton.onClick.AddListener(_003CStart_003Em__6);
		}

		private void OnDestroy()
		{
			ClaimButton.onClick.RemoveAllListeners();
			ResignButton.onClick.RemoveAllListeners();
			ChangeNameButton.onClick.RemoveAllListeners();
			ChangeEmblemButton.onClick.RemoveAllListeners();
			InviteButton.onClick.RemoveAllListeners();
			ShipCrewButton.onClick.RemoveAllListeners();
		}

		private void Update()
		{
			if (EnterCustomNamePopUp.activeInHierarchy)
			{
				if (InputManager.GetKeyDown(KeyCode.Tab) || InputManager.GetKeyDown(KeyCode.Escape))
				{
					CancelChangeShipName();
				}
				if (InputManager.GetKeyDown(KeyCode.KeypadEnter) || InputManager.GetKeyDown(KeyCode.Return))
				{
					ChangeCustomShipName();
				}
			}
		}

		public void SetSecurityStatus()
		{
			List<SecuritySystem.PlayerSecurityData> authorizedPlayers = SecuritySystem.AuthorizedPlayers;
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CSetSecurityStatus_003Em__7;
			}
			SecuritySystem.PlayerSecurityData playerSecurityData = authorizedPlayers.Find(_003C_003Ef__am_0024cache0);
			FreeTerminal.SetActive(playerSecurityData == null);
			RegistredTerminal.SetActive(playerSecurityData != null);
			if (playerSecurityData == null)
			{
				PlayerName.text = "-";
				PlayerImage.texture = DefaultTexture;
			}
			else
			{
				PlayerName.text = playerSecurityData.Name;
				PlayerImage.texture = Player.GetAvatar(playerSecurityData.SteamID);
				float? selfDestructTimer = SecuritySystem.ParentShip.SelfDestructTimer;
				if (!selfDestructTimer.HasValue)
				{
					ResignButton.gameObject.SetActive(SecuritySystem.GetPlayerRank(MyPlayer.Instance) != AuthorizedPersonRank.None);
					InviteButton.gameObject.SetActive(SecuritySystem.GetPlayerRank(MyPlayer.Instance) == AuthorizedPersonRank.CommandingOfficer);
					ChangeNameButton.gameObject.SetActive(SecuritySystem.GetPlayerRank(MyPlayer.Instance) == AuthorizedPersonRank.CommandingOfficer);
					ChangeEmblemButton.gameObject.SetActive(SecuritySystem.GetPlayerRank(MyPlayer.Instance) == AuthorizedPersonRank.CommandingOfficer && SecuritySystem.ParentShip.Emblems != null);
					ShipCrewButton.gameObject.SetActive(SecuritySystem.GetPlayerRank(MyPlayer.Instance) != AuthorizedPersonRank.None);
				}
				else
				{
					ResignButton.gameObject.SetActive(false);
					InviteButton.gameObject.SetActive(false);
					ChangeNameButton.gameObject.SetActive(false);
					ChangeEmblemButton.gameObject.SetActive(false);
					ShipCrewButton.gameObject.SetActive(false);
				}
				Text selfDestructState = SelfDestructState;
				float? selfDestructTimer2 = SecuritySystem.ParentShip.SelfDestructTimer;
				selfDestructState.text = ((!selfDestructTimer2.HasValue) ? Localization.Activate.ToUpper() : Localization.Cancel.ToUpper()) + " " + Localization.SelfDestruct.ToUpper();
			}
			SelfDestructButton.gameObject.SetActive(SecuritySystem.GetPlayerRank(MyPlayer.Instance) == AuthorizedPersonRank.CommandingOfficer);
			RefreshSelfDestructTimer();
		}

		public void UpdateUI()
		{
			if (!(SecuritySystem == null))
			{
				SetShipName();
				SetSecurityStatus();
				UpdateSecurityList();
				UpdateEmblems();
			}
		}

		private void SetShipName()
		{
			if (SecuritySystem.ParentShip.VesselData.VesselName.IsNullOrEmpty())
			{
				CustomShipName.text = Localization.UnnamedVessel.ToUpper();
			}
			else
			{
				CustomShipName.text = SecuritySystem.ParentShip.VesselData.VesselName.ToUpper();
			}
			if (SecuritySystem.ParentShip.VesselData.VesselRegistration != null)
			{
				DefaultShipName.text = SecuritySystem.ParentShip.VesselData.VesselRegistration.ToUpper();
			}
			updateShipNameTags();
		}

		private void ChangeShipName()
		{
			EnterCustomNamePopUp.SetActive(true);
			CustomShipNameInputField.text = CustomShipName.text;
			CustomShipNameInputField.Select();
			Client.Instance.CanvasManager.IsInputFieldIsActive = true;
		}

		public void CancelChangeShipName()
		{
			Client.Instance.CanvasManager.IsInputFieldIsActive = false;
			EnterCustomNamePopUp.SetActive(false);
		}

		public void ChangeEmblem(string id)
		{
			SecuritySystem.ParentShip.ChangeStats(null, null, null, null, null, null, null, null, null, null, null, null, null, null, id);
			ShowEmblemPanel(false);
		}

		public void ChangeCustomShipName()
		{
			string text = CustomShipName.text.ToUpper();
			if (CustomShipNameInputField.text != null)
			{
				CustomShipName.text = CustomShipNameInputField.text.ToUpper();
				SecuritySystem.ChangeVesselName(CustomShipName.text);
			}
			else
			{
				CustomShipName.text = text;
			}
			SetShipName();
			Client.Instance.CanvasManager.IsInputFieldIsActive = false;
			EnterCustomNamePopUp.SetActive(false);
		}

		private void updateShipNameTags()
		{
			SceneNameTag[] shipNameTags = SecuritySystem.ShipNameTags;
			foreach (SceneNameTag sceneNameTag in shipNameTags)
			{
				sceneNameTag.SetNameTagText(CustomShipName.text);
			}
		}

		private void GetPlayerList()
		{
			PurgeList();
			SecuritySystem.GetPlayersForAuthorization(true, true, GetPlayersDelegate);
		}

		public void UpdateSecurityList()
		{
			foreach (CrewMembersUI crewMembers in crewMembersList)
			{
				UnityEngine.Object.Destroy(crewMembers.gameObject);
			}
			crewMembersList.Clear();
			List<SecuritySystem.PlayerSecurityData> authorizedPlayers = SecuritySystem.AuthorizedPlayers;
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CUpdateSecurityList_003Em__8;
			}
			SecuritySystem.PlayerSecurityData playerSecurityData = authorizedPlayers.Find(_003C_003Ef__am_0024cache1);
			List<SecuritySystem.PlayerSecurityData> authorizedPlayers2 = SecuritySystem.AuthorizedPlayers;
			if (_003C_003Ef__am_0024cache2 == null)
			{
				_003C_003Ef__am_0024cache2 = _003CUpdateSecurityList_003Em__9;
			}
			using (List<SecuritySystem.PlayerSecurityData>.Enumerator enumerator2 = authorizedPlayers2.FindAll(_003C_003Ef__am_0024cache2).GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					_003CUpdateSecurityList_003Ec__AnonStorey0 _003CUpdateSecurityList_003Ec__AnonStorey = new _003CUpdateSecurityList_003Ec__AnonStorey0();
					_003CUpdateSecurityList_003Ec__AnonStorey.crewman = enumerator2.Current;
					_003CUpdateSecurityList_003Ec__AnonStorey._0024this = this;
					GameObject gameObject = UnityEngine.Object.Instantiate(CrewMemberPref, CrewMemberPref.transform.parent);
					gameObject.SetActive(true);
					CrewMembersUI component = gameObject.GetComponent<CrewMembersUI>();
					component.Player = _003CUpdateSecurityList_003Ec__AnonStorey.crewman;
					crewMembersList.Add(component);
					component.PlayerNameText.text = _003CUpdateSecurityList_003Ec__AnonStorey.crewman.Name;
					component.Avatar.texture = Player.GetAvatar(_003CUpdateSecurityList_003Ec__AnonStorey.crewman.SteamID);
					if (playerSecurityData != null && playerSecurityData.Rank == AuthorizedPersonRank.CommandingOfficer)
					{
						component.GetComponent<Button>().interactable = true;
						component.GetComponent<Button>().onClick.AddListener(_003CUpdateSecurityList_003Ec__AnonStorey._003C_003Em__0);
					}
					else
					{
						component.GetComponent<Button>().interactable = false;
						component.GetComponent<Button>().onClick.RemoveAllListeners();
					}
				}
			}
		}

		public void CrewMemberActions(SecuritySystem.PlayerSecurityData crewman)
		{
			CrewMemberPanel.SetActive(true);
			CrewMemberPanel.GetComponentInChildren<CrewMembersUI>().Avatar.texture = Player.GetAvatar(crewman.SteamID);
			CrewMemberPanel.GetComponentInChildren<CrewMembersUI>().PlayerNameText.text = crewman.Name;
			currentCrewman = crewman;
		}

		public void CloseCrewMemberPanel()
		{
			currentCrewman = null;
			CrewMemberPanel.Activate(false);
			PromotePlayerBox.Activate(false);
		}

		public void PromotePlayer()
		{
			PromotePlayerBox.SetActive(true);
		}

		public void CancelPromote()
		{
			PromotePlayerBox.SetActive(false);
		}

		public void ConfirmPlayerPromotion()
		{
			Client.Instance.NetworkController.SendToGameServer(new VesselSecurityRequest
			{
				VesselGUID = SecuritySystem.ParentShip.GUID,
				AddPlayerSteamID = currentCrewman.SteamID,
				AddPlayerRank = AuthorizedPersonRank.CommandingOfficer,
				AddPlayerName = currentCrewman.Name
			});
			CloseCrewMemberPanel();
		}

		public void RemoveCrewMember()
		{
			RemovePlayer(currentCrewman);
			CloseCrewMemberPanel();
		}

		public void ClaimSecurityTerminal()
		{
			CSteamID steamID = SteamUser.GetSteamID();
			List<SecuritySystem.PlayerSecurityData> authorizedPlayers = SecuritySystem.AuthorizedPlayers;
			if (_003C_003Ef__am_0024cache3 == null)
			{
				_003C_003Ef__am_0024cache3 = _003CClaimSecurityTerminal_003Em__A;
			}
			SecuritySystem.PlayerSecurityData playerSecurityData = authorizedPlayers.Find(_003C_003Ef__am_0024cache3);
			if (playerSecurityData == null || playerSecurityData.SteamID == steamID.m_SteamID.ToString())
			{
				Client.Instance.NetworkController.SendToGameServer(new VesselSecurityRequest
				{
					VesselGUID = SecuritySystem.ParentShip.GUID,
					AddPlayerSteamID = steamID.m_SteamID.ToString(),
					AddPlayerRank = AuthorizedPersonRank.CommandingOfficer,
					AddPlayerName = MyPlayer.Instance.PlayerName
				});
			}
			UpdateUI();
		}

		public void Resign()
		{
			List<SecuritySystem.PlayerSecurityData> authorizedPlayers = SecuritySystem.AuthorizedPlayers;
			if (_003C_003Ef__am_0024cache4 == null)
			{
				_003C_003Ef__am_0024cache4 = _003CResign_003Em__B;
			}
			SecuritySystem.PlayerSecurityData playerSecurityData = authorizedPlayers.Find(_003C_003Ef__am_0024cache4);
			if (playerSecurityData.Rank == AuthorizedPersonRank.CommandingOfficer)
			{
				List<SecuritySystem.PlayerSecurityData> authorizedPlayers2 = SecuritySystem.AuthorizedPlayers;
				if (_003C_003Ef__am_0024cache5 == null)
				{
					_003C_003Ef__am_0024cache5 = _003CResign_003Em__C;
				}
				if (authorizedPlayers2.Find(_003C_003Ef__am_0024cache5) != null)
				{
					ResignAlertBox.SetActive(true);
					return;
				}
			}
			SecuritySystem.RemovePerson(playerSecurityData);
		}

		public void CancelResign()
		{
			ResignAlertBox.SetActive(false);
		}

		public void ConfirmCommanderResign()
		{
			List<SecuritySystem.PlayerSecurityData> authorizedPlayers = SecuritySystem.AuthorizedPlayers;
			if (_003C_003Ef__am_0024cache6 == null)
			{
				_003C_003Ef__am_0024cache6 = _003CConfirmCommanderResign_003Em__D;
			}
			SecuritySystem.PlayerSecurityData player = authorizedPlayers.Find(_003C_003Ef__am_0024cache6);
			SecuritySystem.RemovePerson(player);
			List<SecuritySystem.PlayerSecurityData> authorizedPlayers2 = SecuritySystem.AuthorizedPlayers;
			if (_003C_003Ef__am_0024cache7 == null)
			{
				_003C_003Ef__am_0024cache7 = _003CConfirmCommanderResign_003Em__E;
			}
			SecuritySystem.PlayerSecurityData playerSecurityData = authorizedPlayers2.Find(_003C_003Ef__am_0024cache7);
			Client.Instance.NetworkController.SendToGameServer(new VesselSecurityRequest
			{
				VesselGUID = SecuritySystem.ParentShip.GUID,
				AddPlayerSteamID = playerSecurityData.SteamID,
				AddPlayerRank = AuthorizedPersonRank.CommandingOfficer,
				AddPlayerName = playerSecurityData.Name
			});
			ResignAlertBox.SetActive(false);
			UpdateUI();
		}

		private void GetPlayersDelegate(List<SecuritySystem.PlayerSecurityData> availablePlayers)
		{
			InviteList.SetActive(true);
			InviteList.GetComponentInChildren<Scrollbar>(true).value = 1f;
			using (List<SecuritySystem.PlayerSecurityData>.Enumerator enumerator = availablePlayers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					_003CGetPlayersDelegate_003Ec__AnonStorey1 _003CGetPlayersDelegate_003Ec__AnonStorey = new _003CGetPlayersDelegate_003Ec__AnonStorey1();
					_003CGetPlayersDelegate_003Ec__AnonStorey.pl = enumerator.Current;
					_003CGetPlayersDelegate_003Ec__AnonStorey._0024this = this;
					if (AvailablePlayersForInvite.ContainsKey(_003CGetPlayersDelegate_003Ec__AnonStorey.pl.SteamID) || _003CGetPlayersDelegate_003Ec__AnonStorey.pl.Rank != 0)
					{
						continue;
					}
					_003CGetPlayersDelegate_003Ec__AnonStorey2 _003CGetPlayersDelegate_003Ec__AnonStorey2 = new _003CGetPlayersDelegate_003Ec__AnonStorey2();
					_003CGetPlayersDelegate_003Ec__AnonStorey2._003C_003Ef__ref_00241 = _003CGetPlayersDelegate_003Ec__AnonStorey;
					if (SecuritySystem.AuthorizedPlayers.FirstOrDefault(_003CGetPlayersDelegate_003Ec__AnonStorey2._003C_003Em__0) == null)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(PlayerToInvitePref, PlayerToInvitePref.transform.parent);
						gameObject.SetActive(true);
						InvitePlayerToPod component = gameObject.GetComponent<InvitePlayerToPod>();
						component.PlayerName.text = _003CGetPlayersDelegate_003Ec__AnonStorey.pl.Name;
						if (_003CGetPlayersDelegate_003Ec__AnonStorey.pl.IsSteamFriend)
						{
							component.IsSteamFriend.SetActive(false);
							component.Avatar.texture = Player.GetAvatar(_003CGetPlayersDelegate_003Ec__AnonStorey.pl.SteamID);
						}
						else
						{
							component.IsSteamFriend.SetActive(true);
							component.Avatar.gameObject.SetActive(false);
						}
						_003CGetPlayersDelegate_003Ec__AnonStorey2.plForDeleg = _003CGetPlayersDelegate_003Ec__AnonStorey.pl;
						component.InvitePlayerButton.onClick.AddListener(_003CGetPlayersDelegate_003Ec__AnonStorey2._003C_003Em__1);
						AvailablePlayersForInvite.Add(_003CGetPlayersDelegate_003Ec__AnonStorey.pl.SteamID, component);
					}
				}
			}
		}

		private void AddToCrew(SecuritySystem.PlayerSecurityData player)
		{
			SecuritySystem.AddPerson(player, AuthorizedPersonRank.Crewman);
			InviteList.SetActive(false);
			UpdateSecurityList();
		}

		private void RemovePlayer(SecuritySystem.PlayerSecurityData player)
		{
			SecuritySystem.RemovePerson(player);
		}

		public void SelfDestructAction()
		{
			float? selfDestructTimer = SecuritySystem.ParentShip.SelfDestructTimer;
			if (selfDestructTimer.HasValue)
			{
				ToggleSelfDestruct();
			}
			else
			{
				SelfDestructBox.SetActive(true);
			}
		}

		public void ToggleSelfDestruct()
		{
			float? selfDestructTimer = SecuritySystem.ParentShip.SelfDestructTimer;
			if (selfDestructTimer.HasValue)
			{
				SecuritySystem.ParentShip.CancelSelfDestruct();
				SelfDestructWarning.SetActive(false);
			}
			else
			{
				SecuritySystem.ParentShip.ActivateSelfDestruct(300f);
				SelfDestructWarning.SetActive(true);
				SelfDestructBox.SetActive(false);
			}
			RefreshSelfDestructTimer();
			Text selfDestructState = SelfDestructState;
			float? selfDestructTimer2 = SecuritySystem.ParentShip.SelfDestructTimer;
			selfDestructState.text = ((!selfDestructTimer2.HasValue) ? Localization.Activate.ToUpper() : Localization.Cancel.ToUpper()) + " " + Localization.SelfDestruct.ToUpper();
			SetSecurityStatus();
		}

		public void RefreshSelfDestructTimer()
		{
			float? selfDestructTimer = SecuritySystem.ParentShip.SelfDestructTimer;
			if (selfDestructTimer.HasValue)
			{
				TimeSpan timeSpan = TimeSpan.FromSeconds(SecuritySystem.ParentShip.SelfDestructTimer.Value);
				string text = string.Format("{0:n0} : {1:n0} : {2:n0}", timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
				SelfDestructTimer.text = text;
			}
			else
			{
				SelfDestructWarning.SetActive(false);
				SelfDestructTimer.text = string.Empty;
			}
		}

		private void PurgeList()
		{
			foreach (KeyValuePair<string, InvitePlayerToPod> item in AvailablePlayersForInvite)
			{
				UnityEngine.Object.Destroy(item.Value.gameObject);
			}
			AvailablePlayersForInvite.Clear();
		}

		public void ToggleCanvas(bool toggle)
		{
			if (toggle)
			{
				Client.Instance.CanvasManager.QuickTipHolder.Activate(false);
				UpdateUI();
				SelfDestructBox.Activate(false);
				InviteList.Activate(false);
				ShowShipCrew(false);
				EnterCustomNamePopUp.Activate(false);
				ResignAlertBox.Activate(false);
				ShowEmblemPanel(false);
				CloseCrewMemberPanel();
				RefreshSelfDestructTimer();
				Client.Instance.CanvasManager.IsInputFieldIsActive = false;
				UpdateEmblems();
				base.gameObject.Activate(true);
			}
			else
			{
				base.gameObject.Activate(false);
			}
		}

		public void ShowShipCrew(bool status)
		{
			ShipCrewList.SetActive(status);
		}

		public void ShowEmblemPanel(bool status)
		{
			ChooseEmblemPanel.SetActive(status);
			ChooseEmblemPanel.GetComponentInChildren<Scrollbar>(true).value = 1f;
		}

		public void UpdateEmblems()
		{
			EmblemObjectUI[] componentsInChildren = EmblemParent.GetComponentsInChildren<EmblemObjectUI>(true);
			foreach (EmblemObjectUI emblemObjectUI in componentsInChildren)
			{
				UnityEngine.Object.DestroyImmediate(emblemObjectUI.gameObject);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(EmblemObject, EmblemParent);
			gameObject.transform.localScale = Vector3.one;
			EmblemObjectUI component = gameObject.GetComponent<EmblemObjectUI>();
			component.Panel = this;
			component.EmblemId = string.Empty;
			component.Texture = null;
			component.Image.texture = DefaultEmblemTexture;
			component.IsSelected.SetActive(component.EmblemId == SecuritySystem.ParentShip.Emblems.FirstOrDefault().EmblemId);
			foreach (Texture2D value in SceneVesselEmblem.Textures.Values)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(EmblemObject, EmblemParent);
				gameObject2.transform.localScale = Vector3.one;
				EmblemObjectUI component2 = gameObject2.GetComponent<EmblemObjectUI>();
				component2.Panel = this;
				component2.EmblemId = value.name;
				component2.Texture = value;
				component2.Image.texture = component2.Texture;
				component2.IsSelected.SetActive(component2.EmblemId == SecuritySystem.ParentShip.Emblems.FirstOrDefault().EmblemId);
			}
		}

		public void OnInteract()
		{
			ToggleCanvas(true);
		}

		public void OnDetach()
		{
			ToggleCanvas(false);
		}

		public void ExitButton()
		{
			MyPlayer.Instance.LockedToTrigger.CancelInteract(MyPlayer.Instance);
		}

		[CompilerGenerated]
		private void _003CStart_003Em__0()
		{
			ClaimSecurityTerminal();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__1()
		{
			Resign();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__2()
		{
			ChangeShipName();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__3()
		{
			ShowEmblemPanel(true);
		}

		[CompilerGenerated]
		private void _003CStart_003Em__4()
		{
			GetPlayerList();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__5()
		{
			ShowShipCrew(true);
		}

		[CompilerGenerated]
		private void _003CStart_003Em__6()
		{
			SelfDestructAction();
		}

		[CompilerGenerated]
		private static bool _003CSetSecurityStatus_003Em__7(SecuritySystem.PlayerSecurityData m)
		{
			return m.Rank == AuthorizedPersonRank.CommandingOfficer;
		}

		[CompilerGenerated]
		private static bool _003CUpdateSecurityList_003Em__8(SecuritySystem.PlayerSecurityData m)
		{
			return m.GUID == MyPlayer.Instance.GUID;
		}

		[CompilerGenerated]
		private static bool _003CUpdateSecurityList_003Em__9(SecuritySystem.PlayerSecurityData m)
		{
			return m.Rank == AuthorizedPersonRank.Crewman;
		}

		[CompilerGenerated]
		private static bool _003CClaimSecurityTerminal_003Em__A(SecuritySystem.PlayerSecurityData m)
		{
			return m.Rank == AuthorizedPersonRank.CommandingOfficer;
		}

		[CompilerGenerated]
		private static bool _003CResign_003Em__B(SecuritySystem.PlayerSecurityData m)
		{
			return m.GUID == MyPlayer.Instance.GUID;
		}

		[CompilerGenerated]
		private static bool _003CResign_003Em__C(SecuritySystem.PlayerSecurityData m)
		{
			return m.Rank == AuthorizedPersonRank.Crewman;
		}

		[CompilerGenerated]
		private static bool _003CConfirmCommanderResign_003Em__D(SecuritySystem.PlayerSecurityData m)
		{
			return m.GUID == MyPlayer.Instance.GUID;
		}

		[CompilerGenerated]
		private static bool _003CConfirmCommanderResign_003Em__E(SecuritySystem.PlayerSecurityData m)
		{
			return m.Rank == AuthorizedPersonRank.Crewman;
		}
	}
}
