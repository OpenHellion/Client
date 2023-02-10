using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;
using ZeroGravity.UI;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerShipControl2 : BaseSceneTrigger
	{
		[SerializeField]
		private Transform _CharacterPosition;

		[SerializeField]
		private Transform _CameraLookAt;

		[SerializeField]
		private Transform _CameraPosition;

		public SubSystem Headlights;

		private Transform MainCameraDefaultParent;

		public Transform CharacterSitPosition;

		public Transform CharacterStandPosition;

		public PilotTargetList TargetList;

		public PilotStatusScreen StatusScreen;

		public PilotRadar Radar;

		public override bool ExclusivePlayerLocking
		{
			get
			{
				return true;
			}
		}

		public override SceneTriggerType TriggerType
		{
			get
			{
				return SceneTriggerType.ShipControl;
			}
		}

		public override PlayerHandsCheckType PlayerHandsCheck
		{
			get
			{
				return PlayerHandsCheckType.StoreItemInHands;
			}
		}

		public override List<ItemType> PlayerHandsItemType
		{
			get
			{
				return null;
			}
		}

		public override bool IsNearTrigger
		{
			get
			{
				return true;
			}
		}

		public override bool IsInteractable
		{
			get
			{
				return true;
			}
		}

		public Transform CharacterPosition
		{
			get
			{
				return _CharacterPosition;
			}
		}

		public Transform CameraLookAt
		{
			get
			{
				return _CameraLookAt;
			}
		}

		public Transform CameraPosition
		{
			get
			{
				return _CameraPosition;
			}
		}

		public float CameraFov
		{
			get
			{
				return -1f;
			}
		}

		public override bool CameraMovementAllowed
		{
			get
			{
				return true;
			}
		}

		public override bool CheckAuthorization()
		{
			return base.IsAuthorized;
		}

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!base.Interact(player, interactWithOverlappingTriggers))
			{
				return false;
			}
			MyPlayer.Instance.transform.position = CharacterSitPosition.position;
			MyPlayer.Instance.transform.rotation = CharacterPosition.rotation;
			MyPlayer.Instance.FpsController.CameraController.ResetCameraPositionAndRotation();
			MainCameraDefaultParent = MyPlayer.Instance.FpsController.CameraController.FreelookTransform.parent;
			MyPlayer.Instance.FpsController.CameraController.FreelookTransform.parent = CameraPosition;
			MyPlayer.Instance.FpsController.CameraController.FreelookTransform.Reset();
			MyPlayer.Instance.ShipControlMode = ShipControlMode.Piloting;
			UpdateMode();
			MyPlayer.Instance.FpsController.CameraController.ToggleCameraAttachToHeadBone(true);
			MyPlayer.Instance.animHelper.SetParameterTrigger(AnimatorHelper.Triggers.LockImmediate);
			MyPlayer.Instance.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, AnimatorHelper.LockType.Chair_Sit_Idle);
			player.AttachToPanel(this, false);
			player.FpsController.CameraController.DoInertia = true;
			if (interactWithOverlappingTriggers)
			{
				SceneTriggerHelper.InteractWithOverlappingTriggers(base.gameObject, this, player);
			}
			Client.Instance.InGamePanels.PilotingOptions.gameObject.SetActive(true);
			if (TargetList != null)
			{
				TargetList.ToggleTargetList(false);
			}
			if (StatusScreen != null)
			{
				StatusScreen.ToggleStatusScreen(false);
			}
			if (Radar != null)
			{
				Radar.ToggleRadarScreen(false);
			}
			return true;
		}

		public override void CancelInteract(MyPlayer player)
		{
			base.CancelInteract(player);
			if (MyPlayer.Instance.ShipControlMode != ShipControlMode.Piloting)
			{
				MyPlayer.Instance.ShipControlMode = ShipControlMode.Piloting;
				UpdateMode();
				return;
			}
			MyPlayer.Instance.transform.position = CharacterStandPosition.position;
			MyPlayer.Instance.transform.rotation = CharacterPosition.rotation;
			MyPlayer.Instance.ShipControlMode = ShipControlMode.Piloting;
			UpdateMode();
			MyPlayer.Instance.FpsController.CameraController.FreelookTransform.parent = MainCameraDefaultParent;
			MyPlayer.Instance.FpsController.CameraController.ToggleCameraAttachToHeadBone(false);
			MyPlayer.Instance.FpsController.CameraController.ResetCameraPositionAndRotation();
			MyPlayer.Instance.ShipControlMode = ShipControlMode.None;
			UpdateMode();
			MyPlayer.Instance.animHelper.SetParameterTrigger(AnimatorHelper.Triggers.UnlockImmediate);
			MyPlayer.Instance.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, AnimatorHelper.LockType.Chair_StandUp_Idle);
			player.FpsController.CameraController.DoInertia = false;
			player.DetachFromPanel();
			Client.Instance.InGamePanels.PilotingOptions.gameObject.SetActive(false);
			if (TargetList != null)
			{
				TargetList.ToggleTargetList(true);
			}
			if (StatusScreen != null)
			{
				StatusScreen.ToggleStatusScreen(true);
			}
			if (Radar != null)
			{
				Radar.ToggleRadarScreen(true);
			}
		}

		private void Update()
		{
			if (MyPlayer.Instance.LockedToTrigger != this)
			{
				return;
			}
			MyPlayer instance = MyPlayer.Instance;
			if (instance.LockedToTrigger is SceneTriggerShipControl2)
			{
				if ((Client.Instance.Map.isActiveAndEnabled && Client.Instance.InGamePanels.Navigation.InputFocused) || Client.Instance.CanvasManager.ConsoleIsUp)
				{
					return;
				}
				if (InputController.GetButtonDown(InputController.Actions.Quick1))
				{
					instance.ShipControlMode = ShipControlMode.Piloting;
				}
				else if (InputController.GetButtonDown(InputController.Actions.Quick2) && base.ParentShip.VesselBaseSystem.Status == SystemStatus.OnLine)
				{
					instance.ShipControlMode = ShipControlMode.Navigation;
				}
				else if (InputController.GetButtonDown(InputController.Actions.Quick3))
				{
					instance.ShipControlMode = ShipControlMode.Docking;
				}
				else if (InputController.GetButtonDown(InputController.Actions.Quick4) && Headlights != null)
				{
					Headlights.Toggle();
				}
			}
			UpdateMode();
			if (Headlights != null)
			{
				Client.Instance.InGamePanels.PilotingOptions.Lights.SetActive(Headlights.Status == SystemStatus.OnLine);
				Client.Instance.InGamePanels.PilotingOptions.LightsMalfunction.SetActive(Headlights.SecondaryStatus == SystemSecondaryStatus.Defective);
			}
		}

		private void UpdateMode()
		{
			MyPlayer instance = MyPlayer.Instance;
			if (Client.Instance.InGamePanels.Pilot.isActiveAndEnabled && instance.ShipControlMode != ShipControlMode.Piloting)
			{
				Client.Instance.InGamePanels.Pilot.OnDetach();
			}
			if (Client.Instance.Map.isActiveAndEnabled && instance.ShipControlMode != ShipControlMode.Navigation)
			{
				Client.Instance.Map.OnDetach();
			}
			if (Client.Instance.InGamePanels.Docking.isActiveAndEnabled && instance.ShipControlMode != ShipControlMode.Docking)
			{
				Client.Instance.InGamePanels.Docking.OnDetach();
			}
			if (instance.ShipControlMode == ShipControlMode.Piloting && !Client.Instance.InGamePanels.Pilot.isActiveAndEnabled)
			{
				Client.Instance.InGamePanels.Pilot.OnInteract(base.ParentShip, TargetList, StatusScreen, Radar);
				Client.Instance.InGamePanels.PilotingOptions.SetPilotingMode(instance.ShipControlMode);
			}
			else if (instance.ShipControlMode == ShipControlMode.Navigation && !Client.Instance.Map.isActiveAndEnabled)
			{
				Client.Instance.Map.OnInteract(base.ParentShip);
				Client.Instance.InGamePanels.PilotingOptions.SetPilotingMode(instance.ShipControlMode);
			}
			else if (instance.ShipControlMode == ShipControlMode.Docking && !Client.Instance.InGamePanels.Docking.isActiveAndEnabled)
			{
				Client.Instance.InGamePanels.Docking.OnInteract(base.ParentShip);
				Client.Instance.InGamePanels.PilotingOptions.SetPilotingMode(instance.ShipControlMode);
			}
			Client.Instance.InGamePanels.PilotingOptions.NavigationDisabled.Activate(base.ParentShip.VesselBaseSystem.Status != SystemStatus.OnLine);
		}
	}
}
