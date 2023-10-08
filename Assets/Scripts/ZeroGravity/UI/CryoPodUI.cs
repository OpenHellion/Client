using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class CryoPodUI : MonoBehaviour
	{
		public SceneSpawnPoint SpawnPoint;

		public GameObject PlayerToInvitePref;

		public GameObject InviteList;

		public Dictionary<string, InvitePlayerToPod> AvailablePlayersForInvite =
			new Dictionary<string, InvitePlayerToPod>();

		public GameObject ActionHolder;

		public GameObject AlertBox;

		public Button RegisterPodButton;

		public Button UnregisterPodButton;

		public Button CancelInviteButton;

		public Button AuthorizePodButton;

		public Button InviteToPodButton;

		public RawImage PlayerImage;

		public Image DefaultPlayerImage;

		public Text LockedPlayerName;

		public Text LockedStateText;

		public Text Description;

		private void Start()
		{
			RegisterPodButton.onClick.AddListener(delegate { ToggleLock(val: true); });
			UnregisterPodButton.onClick.AddListener(delegate { ToggleLock(val: false); });
			CancelInviteButton.onClick.AddListener(CancelInvite);
			AuthorizePodButton.onClick.AddListener(AssignOnCryo);
			InviteToPodButton.onClick.AddListener(OpenPlayerList);
		}

		private void OnDestroy()
		{
			RegisterPodButton.onClick.RemoveAllListeners();
			UnregisterPodButton.onClick.RemoveAllListeners();
			CancelInviteButton.onClick.RemoveAllListeners();
			AuthorizePodButton.onClick.RemoveAllListeners();
			InviteToPodButton.onClick.RemoveAllListeners();
		}

		public void ToggleCanvas(bool toggle)
		{
			if (toggle)
			{
				UpdateUI();
				InviteList.Activate(value: false);
				AlertBox.Activate(value: false);
				ActionHolder.Activate(value: true);
				gameObject.Activate(value: true);
			}
			else
			{
				gameObject.Activate(value: false);
			}
		}

		private void OnInvitePlayersLoaded(List<SceneSpawnPoint.PlayerInviteData> availablePlayers)
		{
			InviteList.SetActive(value: true);
			foreach (SceneSpawnPoint.PlayerInviteData availablePlayer in availablePlayers)
			{
				if (AvailablePlayersForInvite.ContainsKey(availablePlayer.PlayerId))
				{
					AvailablePlayersForInvite[availablePlayer.PlayerId].InGameName.text = availablePlayer.Name;
					AvailablePlayersForInvite[availablePlayer.PlayerId].InvitePlayerButton.interactable =
						!availablePlayer.AlreadyHasInvite;
					continue;
				}

				GameObject gameObject = Instantiate(PlayerToInvitePref, PlayerToInvitePref.transform.parent);
				gameObject.SetActive(value: true);
				InvitePlayerToPod component = gameObject.GetComponent<InvitePlayerToPod>();
				component.PlayerName.text = availablePlayer.Name;
				component.InvitePlayerButton.interactable = !availablePlayer.AlreadyHasInvite;
				if (availablePlayer.IsFriend)
				{
					component.IsFriend.SetActive(value: false);
					component.Avatar.texture = Player.GetAvatar(availablePlayer.PlayerId);
				}
				else
				{
					component.IsFriend.SetActive(value: true);
					component.Avatar.gameObject.SetActive(value: false);
				}

				SceneSpawnPoint.PlayerInviteData plForDeleg = availablePlayer;
				component.InvitePlayerButton.onClick.AddListener(delegate { InvitePlayer(plForDeleg); });
				AvailablePlayersForInvite.Add(availablePlayer.PlayerId, component);
			}
		}

		public void CloseInviteList()
		{
			InviteList.SetActive(value: false);
			UnregisterPodButton.gameObject.SetActive(value: true);
			InviteToPodButton.gameObject.SetActive(value: true);
			AuthorizePodButton.gameObject.SetActive(value: true);
		}

		public void UpdateUI()
		{
			SetPodStateUI();
			if (SpawnPoint.State == SpawnPointState.Unlocked)
			{
				PlayerImage.texture = null;
				DefaultPlayerImage.gameObject.SetActive(value: true);
				LockedPlayerName.text = Localization.Free.ToUpper();
				LockedStateText.text = "-";
				Description.text = Localization.RegisterToAccess.ToUpper();
				Description.color = Colors.GrayText;
			}
			else if (!SpawnPoint.InvitedPlayerName.IsNullOrEmpty())
			{
				PlayerImage.texture = Player.GetAvatar(SpawnPoint.InvitedPlayerId);
				DefaultPlayerImage.gameObject.SetActive(value: false);
				LockedStateText.text = Localization.InvitePending.ToUpper() + "...";
				LockedPlayerName.text = SpawnPoint.InvitedPlayerName;
				Description.text = Localization.InviteSent.ToUpper() + "!";
				Description.color = Colors.GrayText;
			}
			else if (SpawnPoint.State == SpawnPointState.Locked)
			{
				PlayerImage.texture = Player.GetAvatar(SpawnPoint.PlayerId);
				DefaultPlayerImage.gameObject.SetActive(value: false);
				LockedPlayerName.text = SpawnPoint.PlayerName.ToString();
				LockedStateText.text = Localization.Registered.ToUpper();
				Description.text = Localization.SpawnPointNotSet.ToUpper();
				Description.color = Colors.RedText;
			}
			else if (SpawnPoint.State == SpawnPointState.Authorized)
			{
				PlayerImage.texture = Player.GetAvatar(SpawnPoint.PlayerId);
				DefaultPlayerImage.gameObject.SetActive(value: false);
				LockedPlayerName.text = SpawnPoint.PlayerName.ToString();
				LockedStateText.text = Localization.Registered.ToUpper();
				Description.text = Localization.SpawnPointSet.ToUpper();
				Description.color = Colors.GreenText;
			}
		}

		public void SetPodStateUI()
		{
			if (SpawnPoint.State == SpawnPointState.Unlocked)
			{
				RegisterPodButton.gameObject.SetActive(value: true);
				CancelInviteButton.gameObject.SetActive(value: false);
				AuthorizePodButton.gameObject.SetActive(value: false);
				InviteToPodButton.gameObject.SetActive(value: false);
				UnregisterPodButton.gameObject.SetActive(value: false);
			}
			else if (SpawnPoint.State is SpawnPointState.Locked && SpawnPoint.PlayerGUID == MyPlayer.Instance.GUID)
			{
				if (!SpawnPoint.InvitedPlayerName.IsNullOrEmpty())
				{
					RegisterPodButton.gameObject.SetActive(value: false);
					CancelInviteButton.gameObject.SetActive(value: true);
					AuthorizePodButton.gameObject.SetActive(value: false);
					InviteToPodButton.gameObject.SetActive(value: false);
					UnregisterPodButton.gameObject.SetActive(value: false);
				}
				else
				{
					RegisterPodButton.gameObject.SetActive(value: false);
					CancelInviteButton.gameObject.SetActive(value: false);
					AuthorizePodButton.gameObject.SetActive(value: true);
					InviteToPodButton.gameObject.SetActive(value: true);
					UnregisterPodButton.gameObject.SetActive(value: true);
				}
			}
			else if (SpawnPoint.State is SpawnPointState.Locked or SpawnPointState.Authorized)
			{
				RegisterPodButton.gameObject.SetActive(value: false);
				CancelInviteButton.gameObject.SetActive(value: false);
				AuthorizePodButton.gameObject.SetActive(value: false);
				InviteToPodButton.gameObject.SetActive(value: false);
				UnregisterPodButton.gameObject.SetActive(value: false);
			}
		}

		public void ToggleLock(bool val)
		{
			SpawnPoint.ToggleLock(val);
		}

		public void CancelInvite()
		{
			SpawnPoint.ToggleLock(isLocked: false);
			SpawnPoint.InvitePlayer(null);
		}

		public void AssignOnCryo()
		{
			AlertBox.SetActive(value: true);
			ActionHolder.SetActive(value: false);
		}

		public void ConfirmAssignOnCryo()
		{
			AlertBox.SetActive(value: false);
			ActionHolder.SetActive(value: true);
			SpawnPoint.AuthorizeToSpawnPoint();
		}

		public void CancelAssignOnCryo()
		{
			AlertBox.SetActive(value: false);
			ActionHolder.SetActive(value: true);
		}

		public void OpenPlayerList()
		{
			UnregisterPodButton.gameObject.SetActive(value: false);
			InviteToPodButton.gameObject.SetActive(value: false);
			AuthorizePodButton.gameObject.SetActive(value: false);
			foreach (InvitePlayerToPod value in AvailablePlayersForInvite.Values)
			{
				Destroy(value.gameObject);
			}

			AvailablePlayersForInvite.Clear();
			SpawnPoint.GetPlayersForInvite(getNakamaFriends: true, getPlayerFromServer: true, OnInvitePlayersLoaded);
		}

		public void InvitePlayer(SceneSpawnPoint.PlayerInviteData pl)
		{
			SpawnPoint.InvitePlayer(pl);
			InviteList.SetActive(value: false);
			UpdateUI();
		}
	}
}
