using System;
using System.Collections.Generic;
using OpenHellion;
using OpenHellion.IO;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerShipControl : BaseSceneTrigger
	{
		[SerializeField] private Transform _CharacterPosition;

		[SerializeField] private Transform _CameraLookAt;

		[SerializeField] private Transform _CameraPosition;

		public SubSystem Headlights;

		private Transform MainCameraDefaultParent;

		public Transform CharacterSitPosition;

		public Transform CharacterStandPosition;

		public PilotTargetList TargetList;

		public PilotStatusScreen StatusScreen;

		public PilotRadar Radar;

		public override bool ExclusivePlayerLocking => true;

		public override SceneTriggerType TriggerType => SceneTriggerType.ShipControl;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.StoreItemInHands;

		public override List<ItemType> PlayerHandsItemType => null;

		public override bool IsNearTrigger => true;

		public override bool IsInteractable => true;

		public Transform CharacterPosition => _CharacterPosition;

		public Transform CameraLookAt => _CameraLookAt;

		public Transform CameraPosition => _CameraPosition;

		public float CameraFov => -1f;

		public override bool CameraMovementAllowed => true;

		public override bool CheckAuthorization()
		{
			return IsAuthorized;
		}

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!base.Interact(player, interactWithOverlappingTriggers))
			{
				return false;
			}

			MyPlayer.Instance.transform.SetPositionAndRotation(CharacterSitPosition.position, CharacterPosition.rotation);
			MyPlayer.Instance.FpsController.CameraController.ResetCameraPositionAndRotation();
			MainCameraDefaultParent = MyPlayer.Instance.FpsController.CameraController.FreelookTransform.parent;
			MyPlayer.Instance.FpsController.CameraController.FreelookTransform.parent = CameraPosition;
			MyPlayer.Instance.FpsController.CameraController.FreelookTransform.Reset();
			MyPlayer.Instance.ShipControlMode = ShipControlMode.Piloting;
			UpdateMode();
			MyPlayer.Instance.FpsController.CameraController.ToggleCameraAttachToHeadBone(true);
			MyPlayer.Instance.animHelper.SetParameterTrigger(AnimatorHelper.Triggers.LockImmediate);
			MyPlayer.Instance.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
				null, null, AnimatorHelper.LockType.Chair_Sit_Idle);
			player.AttachToPanel(this, false);
			player.FpsController.CameraController.DoInertia = true;
			if (interactWithOverlappingTriggers)
			{
				SceneTriggerHelper.InteractWithOverlappingTriggers(base.gameObject, this, player);
			}

			World.InWorldPanels.PilotingOptions.gameObject.SetActive(true);
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
			MyPlayer.Instance.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
				null, null, AnimatorHelper.LockType.Chair_StandUp_Idle);
			player.FpsController.CameraController.DoInertia = false;
			player.DetachFromPanel();
			World.InWorldPanels.PilotingOptions.gameObject.SetActive(false);
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
			if (instance.LockedToTrigger is SceneTriggerShipControl)
			{
				if ((World.Map.isActiveAndEnabled && ParentShip.NavPanel.InputFocused) ||
				    World.InGameGUI.ConsoleIsUp)
				{
					return;
				}

				if (ControlsSubsystem.GetButtonDown(ControlsSubsystem.ConfigAction.Quick1))
				{
					instance.ShipControlMode = ShipControlMode.Piloting;
				}
				else if (ControlsSubsystem.GetButtonDown(ControlsSubsystem.ConfigAction.Quick2) &&
				         base.ParentShip.VesselBaseSystem.Status == SystemStatus.Online)
				{
					instance.ShipControlMode = ShipControlMode.Navigation;
				}
				else if (ControlsSubsystem.GetButtonDown(ControlsSubsystem.ConfigAction.Quick3))
				{
					instance.ShipControlMode = ShipControlMode.Docking;
				}
				else if (ControlsSubsystem.GetButtonDown(ControlsSubsystem.ConfigAction.Quick4) && Headlights != null)
				{
					Headlights.Toggle();
				}
			}

			UpdateMode();
			if (Headlights != null)
			{
				World.InWorldPanels.PilotingOptions.Lights.SetActive(Headlights.Status == SystemStatus.Online);
				World.InWorldPanels.PilotingOptions.LightsMalfunction.SetActive(Headlights.SecondaryStatus ==
					SystemSecondaryStatus.Defective);
			}
		}

		private void UpdateMode()
		{
			MyPlayer instance = MyPlayer.Instance;
			if (World.InWorldPanels.Pilot.isActiveAndEnabled &&
			    instance.ShipControlMode != ShipControlMode.Piloting)
			{
				World.InWorldPanels.Pilot.OnDetach();
			}

			if (World.Map.isActiveAndEnabled && instance.ShipControlMode != ShipControlMode.Navigation)
			{
				World.Map.OnDetach();
			}

			if (World.InWorldPanels.Docking.isActiveAndEnabled &&
			    instance.ShipControlMode != ShipControlMode.Docking)
			{
				World.InWorldPanels.Docking.OnDetach();
			}

			if (instance.ShipControlMode == ShipControlMode.Piloting &&
			    !World.InWorldPanels.Pilot.isActiveAndEnabled)
			{
				World.InWorldPanels.Pilot.OnInteract(ParentShip, TargetList, StatusScreen, Radar);
				World.InWorldPanels.PilotingOptions.SetPilotingMode(instance.ShipControlMode);
			}
			else if (instance.ShipControlMode == ShipControlMode.Navigation && !World.Map.isActiveAndEnabled)
			{
				World.Map.OnInteract(ParentShip);
				World.InWorldPanels.PilotingOptions.SetPilotingMode(instance.ShipControlMode);
			}
			else if (instance.ShipControlMode == ShipControlMode.Docking &&
			         !World.InWorldPanels.Docking.isActiveAndEnabled)
			{
				World.InWorldPanels.Docking.OnInteract(ParentShip);
				World.InWorldPanels.PilotingOptions.SetPilotingMode(instance.ShipControlMode);
			}

			World.InWorldPanels.PilotingOptions.NavigationDisabled.Activate(ParentShip.VesselBaseSystem.Status !=
				SystemStatus.Online);
		}
	}
}
