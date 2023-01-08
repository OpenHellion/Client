using System;
using System.Collections.Generic;
using System.Linq;
using OpenHellion;
using OpenHellion.Networking;
using OpenHellion.ProviderSystem;
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

		private void Start()
		{
			UpdateUI();
			ClaimButton.onClick.AddListener(delegate
			{
				ClaimSecurityTerminal();
			});
			ResignButton.onClick.AddListener(delegate
			{
				Resign();
			});
			ChangeNameButton.onClick.AddListener(delegate
			{
				ChangeShipName();
			});
			ChangeEmblemButton.onClick.AddListener(delegate
			{
				ShowEmblemPanel(status: true);
			});
			InviteButton.onClick.AddListener(delegate
			{
				GetPlayerList();
			});
			ShipCrewButton.onClick.AddListener(delegate
			{
				ShowShipCrew(status: true);
			});
			SelfDestructButton.onClick.AddListener(delegate
			{
				SelfDestructAction();
			});
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
			SecuritySystem.PlayerSecurityData playerSecurityData = SecuritySystem.AuthorizedPlayers.Find((SecuritySystem.PlayerSecurityData m) => m.Rank == AuthorizedPersonRank.CommandingOfficer);
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
				PlayerImage.texture = Player.GetAvatar(playerSecurityData.PlayerID);
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
					ResignButton.gameObject.SetActive(value: false);
					InviteButton.gameObject.SetActive(value: false);
					ChangeNameButton.gameObject.SetActive(value: false);
					ChangeEmblemButton.gameObject.SetActive(value: false);
					ShipCrewButton.gameObject.SetActive(value: false);
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
			EnterCustomNamePopUp.SetActive(value: true);
			CustomShipNameInputField.text = CustomShipName.text;
			CustomShipNameInputField.Select();
			Client.Instance.CanvasManager.IsInputFieldIsActive = true;
		}

		public void CancelChangeShipName()
		{
			Client.Instance.CanvasManager.IsInputFieldIsActive = false;
			EnterCustomNamePopUp.SetActive(value: false);
		}

		public void ChangeEmblem(string id)
		{
			SecuritySystem.ParentShip.ChangeStats(null, null, null, null, null, null, null, null, null, null, null, null, null, null, id);
			ShowEmblemPanel(status: false);
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
			EnterCustomNamePopUp.SetActive(value: false);
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
			SecuritySystem.GetPlayersForAuthorization(getFriends: true, getPlayerFromServer: true, GetPlayersDelegate);
		}

		public void UpdateSecurityList()
		{
			foreach (CrewMembersUI crewMembers in crewMembersList)
			{
				UnityEngine.Object.Destroy(crewMembers.gameObject);
			}
			crewMembersList.Clear();
			SecuritySystem.PlayerSecurityData playerSecurityData = SecuritySystem.AuthorizedPlayers.Find((SecuritySystem.PlayerSecurityData m) => m.GUID == MyPlayer.Instance.GUID);
			foreach (SecuritySystem.PlayerSecurityData crewman in SecuritySystem.AuthorizedPlayers.FindAll((SecuritySystem.PlayerSecurityData m) => m.Rank == AuthorizedPersonRank.Crewman))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(CrewMemberPref, CrewMemberPref.transform.parent);
				gameObject.SetActive(value: true);
				CrewMembersUI component = gameObject.GetComponent<CrewMembersUI>();
				component.Player = crewman;
				crewMembersList.Add(component);
				component.PlayerNameText.text = crewman.Name;
				component.Avatar.texture = Player.GetAvatar(crewman.PlayerID);
				if (playerSecurityData != null && playerSecurityData.Rank == AuthorizedPersonRank.CommandingOfficer)
				{
					component.GetComponent<Button>().interactable = true;
					component.GetComponent<Button>().onClick.AddListener(delegate
					{
						CrewMemberActions(crewman);
					});
				}
				else
				{
					component.GetComponent<Button>().interactable = false;
					component.GetComponent<Button>().onClick.RemoveAllListeners();
				}
			}
		}

		public void CrewMemberActions(SecuritySystem.PlayerSecurityData crewman)
		{
			CrewMemberPanel.SetActive(value: true);
			CrewMemberPanel.GetComponentInChildren<CrewMembersUI>().Avatar.texture = Player.GetAvatar(crewman.PlayerID);
			CrewMemberPanel.GetComponentInChildren<CrewMembersUI>().PlayerNameText.text = crewman.Name;
			currentCrewman = crewman;
		}

		public void CloseCrewMemberPanel()
		{
			currentCrewman = null;
			CrewMemberPanel.Activate(value: false);
			PromotePlayerBox.Activate(value: false);
		}

		public void PromotePlayer()
		{
			PromotePlayerBox.SetActive(value: true);
		}

		public void CancelPromote()
		{
			PromotePlayerBox.SetActive(value: false);
		}

		public void ConfirmPlayerPromotion()
		{
			NetworkController.Instance.SendToGameServer(new VesselSecurityRequest
			{
				VesselGUID = SecuritySystem.ParentShip.GUID,
				AddPlayerSteamID = currentCrewman.PlayerID,
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
			SecuritySystem.PlayerSecurityData playerSecurityData = SecuritySystem.AuthorizedPlayers.Find((SecuritySystem.PlayerSecurityData m) => m.Rank == AuthorizedPersonRank.CommandingOfficer);
			if (playerSecurityData == null || playerSecurityData.PlayerID == IdManager.PlayerId)
			{
				NetworkController.Instance.SendToGameServer(new VesselSecurityRequest
				{
					VesselGUID = SecuritySystem.ParentShip.GUID,
					AddPlayerSteamID = IdManager.PlayerId,
					AddPlayerRank = AuthorizedPersonRank.CommandingOfficer,
					AddPlayerName = MyPlayer.Instance.PlayerName
				});
			}
			UpdateUI();
		}

		public void Resign()
		{
			SecuritySystem.PlayerSecurityData playerSecurityData = SecuritySystem.AuthorizedPlayers.Find((SecuritySystem.PlayerSecurityData m) => m.GUID == MyPlayer.Instance.GUID);
			if (playerSecurityData.Rank == AuthorizedPersonRank.CommandingOfficer && SecuritySystem.AuthorizedPlayers.Find((SecuritySystem.PlayerSecurityData m) => m.Rank == AuthorizedPersonRank.Crewman) != null)
			{
				ResignAlertBox.SetActive(value: true);
			}
			else
			{
				SecuritySystem.RemovePerson(playerSecurityData);
			}
		}

		public void CancelResign()
		{
			ResignAlertBox.SetActive(value: false);
		}

		public void ConfirmCommanderResign()
		{
			SecuritySystem.PlayerSecurityData player = SecuritySystem.AuthorizedPlayers.Find((SecuritySystem.PlayerSecurityData m) => m.GUID == MyPlayer.Instance.GUID);
			SecuritySystem.RemovePerson(player);
			SecuritySystem.PlayerSecurityData playerSecurityData = SecuritySystem.AuthorizedPlayers.Find((SecuritySystem.PlayerSecurityData m) => m.Rank == AuthorizedPersonRank.Crewman);
			NetworkController.Instance.SendToGameServer(new VesselSecurityRequest
			{
				VesselGUID = SecuritySystem.ParentShip.GUID,
				AddPlayerSteamID = playerSecurityData.PlayerID,
				AddPlayerRank = AuthorizedPersonRank.CommandingOfficer,
				AddPlayerName = playerSecurityData.Name
			});
			ResignAlertBox.SetActive(value: false);
			UpdateUI();
		}

		private void GetPlayersDelegate(List<SecuritySystem.PlayerSecurityData> availablePlayers)
		{
			InviteList.SetActive(value: true);
			InviteList.GetComponentInChildren<Scrollbar>(includeInactive: true).value = 1f;
			foreach (SecuritySystem.PlayerSecurityData pl in availablePlayers)
			{
				if (AvailablePlayersForInvite.ContainsKey(pl.PlayerID) || pl.Rank != 0)
				{
					continue;
				}
				if (SecuritySystem.AuthorizedPlayers.FirstOrDefault((SecuritySystem.PlayerSecurityData m) => m.PlayerID == pl.PlayerID) == null)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(PlayerToInvitePref, PlayerToInvitePref.transform.parent);
					gameObject.SetActive(value: true);
					InvitePlayerToPod component = gameObject.GetComponent<InvitePlayerToPod>();
					component.PlayerName.text = pl.Name;
					if (pl.IsFriend)
					{
						component.IsFriend.SetActive(value: false);
						component.Avatar.texture = Player.GetAvatar(pl.PlayerID);
					}
					else
					{
						component.IsFriend.SetActive(value: true);
						component.Avatar.gameObject.SetActive(value: false);
					}
					component.InvitePlayerButton.onClick.AddListener(delegate
					{
						AddToCrew(pl);
					});
					AvailablePlayersForInvite.Add(pl.PlayerID, component);
				}
			}
		}

		private void AddToCrew(SecuritySystem.PlayerSecurityData player)
		{
			SecuritySystem.AddPerson(player, AuthorizedPersonRank.Crewman);
			InviteList.SetActive(value: false);
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
				SelfDestructBox.SetActive(value: true);
			}
		}

		public void ToggleSelfDestruct()
		{
			float? selfDestructTimer = SecuritySystem.ParentShip.SelfDestructTimer;
			if (selfDestructTimer.HasValue)
			{
				SecuritySystem.ParentShip.CancelSelfDestruct();
				SelfDestructWarning.SetActive(value: false);
			}
			else
			{
				SecuritySystem.ParentShip.ActivateSelfDestruct(300f);
				SelfDestructWarning.SetActive(value: true);
				SelfDestructBox.SetActive(value: false);
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
				string text = $"{timeSpan.TotalHours:n0} : {timeSpan.Minutes:n0} : {timeSpan.Seconds:n0}";
				SelfDestructTimer.text = text;
			}
			else
			{
				SelfDestructWarning.SetActive(value: false);
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
				Client.Instance.CanvasManager.QuickTipHolder.Activate(value: false);
				UpdateUI();
				SelfDestructBox.Activate(value: false);
				InviteList.Activate(value: false);
				ShowShipCrew(status: false);
				EnterCustomNamePopUp.Activate(value: false);
				ResignAlertBox.Activate(value: false);
				ShowEmblemPanel(status: false);
				CloseCrewMemberPanel();
				RefreshSelfDestructTimer();
				Client.Instance.CanvasManager.IsInputFieldIsActive = false;
				UpdateEmblems();
				base.gameObject.Activate(value: true);
			}
			else
			{
				base.gameObject.Activate(value: false);
			}
		}

		public void ShowShipCrew(bool status)
		{
			ShipCrewList.SetActive(status);
		}

		public void ShowEmblemPanel(bool status)
		{
			ChooseEmblemPanel.SetActive(status);
			ChooseEmblemPanel.GetComponentInChildren<Scrollbar>(includeInactive: true).value = 1f;
		}

		public void UpdateEmblems()
		{
			EmblemObjectUI[] componentsInChildren = EmblemParent.GetComponentsInChildren<EmblemObjectUI>(includeInactive: true);
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
			ToggleCanvas(toggle: true);
		}

		public void OnDetach()
		{
			ToggleCanvas(toggle: false);
		}

		public void ExitButton()
		{
			MyPlayer.Instance.LockedToTrigger.CancelInteract(MyPlayer.Instance);
		}
	}
}
