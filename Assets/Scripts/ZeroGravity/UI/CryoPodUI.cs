using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class CryoPodUI : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003COnInvitePlayersLoaded_003Ec__AnonStorey0
		{
			internal SceneSpawnPoint.PlayerInviteData plForDeleg;

			internal CryoPodUI _0024this;

			internal void _003C_003Em__0()
			{
				_0024this.InvitePlayer(plForDeleg);
			}
		}

		public SceneSpawnPoint SpawnPoint;

		public GameObject PlayerToInvitePref;

		public GameObject InviteList;

		public Dictionary<string, InvitePlayerToPod> AvailablePlayersForInvite = new Dictionary<string, InvitePlayerToPod>();

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
			RegisterPodButton.onClick.AddListener(_003CStart_003Em__0);
			UnregisterPodButton.onClick.AddListener(_003CStart_003Em__1);
			CancelInviteButton.onClick.AddListener(_003CStart_003Em__2);
			AuthorizePodButton.onClick.AddListener(_003CStart_003Em__3);
			InviteToPodButton.onClick.AddListener(_003CStart_003Em__4);
		}

		private void OnDestroy()
		{
			RegisterPodButton.onClick.RemoveAllListeners();
			UnregisterPodButton.onClick.RemoveAllListeners();
			CancelInviteButton.onClick.RemoveAllListeners();
			AuthorizePodButton.onClick.RemoveAllListeners();
			InviteToPodButton.onClick.RemoveAllListeners();
		}

		private void Update()
		{
		}

		public void ToggleCanvas(bool toggle)
		{
			if (toggle)
			{
				Client.Instance.CanvasManager.QuickTipHolder.Activate(false);
				UpdateUI();
				InviteList.Activate(false);
				AlertBox.Activate(false);
				ActionHolder.Activate(true);
				base.gameObject.Activate(true);
			}
			else
			{
				base.gameObject.Activate(false);
			}
		}

		private void OnInvitePlayersLoaded(List<SceneSpawnPoint.PlayerInviteData> availablePlayers)
		{
			InviteList.SetActive(true);
			foreach (SceneSpawnPoint.PlayerInviteData availablePlayer in availablePlayers)
			{
				if (AvailablePlayersForInvite.ContainsKey(availablePlayer.PlayerNativeId))
				{
					AvailablePlayersForInvite[availablePlayer.PlayerNativeId].InGameName.text = availablePlayer.Name;
					AvailablePlayersForInvite[availablePlayer.PlayerNativeId].InvitePlayerButton.interactable = !availablePlayer.AlreadyHasInvite;
					continue;
				}
				_003COnInvitePlayersLoaded_003Ec__AnonStorey0 _003COnInvitePlayersLoaded_003Ec__AnonStorey = new _003COnInvitePlayersLoaded_003Ec__AnonStorey0();
				_003COnInvitePlayersLoaded_003Ec__AnonStorey._0024this = this;
				GameObject gameObject = Object.Instantiate(PlayerToInvitePref, PlayerToInvitePref.transform.parent);
				gameObject.SetActive(true);
				InvitePlayerToPod component = gameObject.GetComponent<InvitePlayerToPod>();
				component.PlayerName.text = availablePlayer.Name;
				component.InvitePlayerButton.interactable = !availablePlayer.AlreadyHasInvite;
				if (availablePlayer.IsFriend)
				{
					component.IsFriend.SetActive(false);
					component.Avatar.texture = Player.GetAvatar(availablePlayer.PlayerNativeId);
				}
				else
				{
					component.IsFriend.SetActive(true);
					component.Avatar.gameObject.SetActive(false);
				}
				_003COnInvitePlayersLoaded_003Ec__AnonStorey.plForDeleg = availablePlayer;
				component.InvitePlayerButton.onClick.AddListener(_003COnInvitePlayersLoaded_003Ec__AnonStorey._003C_003Em__0);
				AvailablePlayersForInvite.Add(availablePlayer.PlayerNativeId, component);
			}
		}

		public void CloseInviteList()
		{
			InviteList.SetActive(false);
			UnregisterPodButton.gameObject.SetActive(true);
			InviteToPodButton.gameObject.SetActive(true);
			AuthorizePodButton.gameObject.SetActive(true);
		}

		public void UpdateUI()
		{
			SetPodStateUI();
			if (SpawnPoint.State == SpawnPointState.Unlocked)
			{
				PlayerImage.texture = null;
				DefaultPlayerImage.gameObject.SetActive(true);
				LockedPlayerName.text = Localization.Free.ToUpper();
				LockedStateText.text = "-";
				Description.text = Localization.RegisterToAccess.ToUpper();
				Description.color = Colors.GrayText;
			}
			else if (!SpawnPoint.InvitedPlayerName.IsNullOrEmpty())
			{
				PlayerImage.texture = Player.GetAvatar(SpawnPoint.InvitedPlayerId);
				DefaultPlayerImage.gameObject.SetActive(false);
				LockedStateText.text = Localization.InvitePending.ToUpper() + "...";
				LockedPlayerName.text = SpawnPoint.InvitedPlayerName;
				Description.text = Localization.InviteSent.ToUpper() + "!";
				Description.color = Colors.GrayText;
			}
			else if (SpawnPoint.State == SpawnPointState.Locked)
			{
				PlayerImage.texture = Player.GetAvatar(SpawnPoint.PlayerNativeId);
				DefaultPlayerImage.gameObject.SetActive(false);
				LockedPlayerName.text = SpawnPoint.PlayerName.ToString();
				LockedStateText.text = Localization.Registered.ToUpper();
				Description.text = Localization.SpawnPointNotSet.ToUpper();
				Description.color = Colors.RedText;
			}
			else if (SpawnPoint.State == SpawnPointState.Authorized)
			{
				PlayerImage.texture = Player.GetAvatar(SpawnPoint.PlayerNativeId);
				DefaultPlayerImage.gameObject.SetActive(false);
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
				RegisterPodButton.gameObject.SetActive(true);
				CancelInviteButton.gameObject.SetActive(false);
				AuthorizePodButton.gameObject.SetActive(false);
				InviteToPodButton.gameObject.SetActive(false);
				UnregisterPodButton.gameObject.SetActive(false);
			}
			else if (SpawnPoint.State == SpawnPointState.Locked && SpawnPoint.PlayerGUID == MyPlayer.Instance.GUID)
			{
				if (!SpawnPoint.InvitedPlayerName.IsNullOrEmpty())
				{
					RegisterPodButton.gameObject.SetActive(false);
					CancelInviteButton.gameObject.SetActive(true);
					AuthorizePodButton.gameObject.SetActive(false);
					InviteToPodButton.gameObject.SetActive(false);
					UnregisterPodButton.gameObject.SetActive(false);
				}
				else
				{
					RegisterPodButton.gameObject.SetActive(false);
					CancelInviteButton.gameObject.SetActive(false);
					AuthorizePodButton.gameObject.SetActive(true);
					InviteToPodButton.gameObject.SetActive(true);
					UnregisterPodButton.gameObject.SetActive(true);
				}
			}
			else if (SpawnPoint.State == SpawnPointState.Locked || SpawnPoint.State == SpawnPointState.Authorized)
			{
				RegisterPodButton.gameObject.SetActive(false);
				CancelInviteButton.gameObject.SetActive(false);
				AuthorizePodButton.gameObject.SetActive(false);
				InviteToPodButton.gameObject.SetActive(false);
				UnregisterPodButton.gameObject.SetActive(false);
			}
		}

		public void ToggleLock(bool val)
		{
			SpawnPoint.ToggleLock(val);
		}

		public void CancelInvite()
		{
			SpawnPoint.ToggleLock(false);
			SpawnPoint.InvitePlayer(null);
		}

		public void AssignOnCryo()
		{
			AlertBox.SetActive(true);
			ActionHolder.SetActive(false);
		}

		public void ConfirmAssignOnCryo()
		{
			AlertBox.SetActive(false);
			ActionHolder.SetActive(true);
			SpawnPoint.AuthorizeToSpawnPoint();
		}

		public void CancelAssignOnCryo()
		{
			AlertBox.SetActive(false);
			ActionHolder.SetActive(true);
		}

		public void OpenPlayerList()
		{
			UnregisterPodButton.gameObject.SetActive(false);
			InviteToPodButton.gameObject.SetActive(false);
			AuthorizePodButton.gameObject.SetActive(false);
			foreach (InvitePlayerToPod value in AvailablePlayersForInvite.Values)
			{
				Object.Destroy(value.gameObject);
			}
			AvailablePlayersForInvite.Clear();
			SpawnPoint.GetPlayersForInvite(true, true, OnInvitePlayersLoaded);
		}

		public void InvitePlayer(SceneSpawnPoint.PlayerInviteData pl)
		{
			SpawnPoint.InvitePlayer(pl);
			InviteList.SetActive(false);
			UpdateUI();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__0()
		{
			ToggleLock(true);
		}

		[CompilerGenerated]
		private void _003CStart_003Em__1()
		{
			ToggleLock(false);
		}

		[CompilerGenerated]
		private void _003CStart_003Em__2()
		{
			CancelInvite();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__3()
		{
			AssignOnCryo();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__4()
		{
			OpenPlayerList();
		}
	}
}
