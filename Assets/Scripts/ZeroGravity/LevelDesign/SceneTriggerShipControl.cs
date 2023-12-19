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

		private static World _world;

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

		private void Awake()
		{
			_world ??= GameObject.Find("/World").GetComponent<World>();
		}

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
			MyPlayer.Instance.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
				null, null, AnimatorHelper.LockType.Chair_Sit_Idle);
			player.AttachToPanel(this, false);
			player.FpsController.CameraController.DoInertia = true;
			if (interactWithOverlappingTriggers)
			{
				SceneTriggerHelper.InteractWithOverlappingTriggers(base.gameObject, this, player);
			}

			_world.InWorldPanels.PilotingOptions.gameObject.SetActive(true);
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
			_world.InWorldPanels.PilotingOptions.gameObject.SetActive(false);
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
				if ((_world.Map.isActiveAndEnabled && ParentShip.NavPanel.InputFocused) ||
				    _world.InGameGUI.ConsoleIsUp)
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
				_world.InWorldPanels.PilotingOptions.Lights.SetActive(Headlights.Status == SystemStatus.Online);
				_world.InWorldPanels.PilotingOptions.LightsMalfunction.SetActive(Headlights.SecondaryStatus ==
					SystemSecondaryStatus.Defective);
			}
		}

		private void UpdateMode()
		{
			MyPlayer instance = MyPlayer.Instance;
			if (_world.InWorldPanels.Pilot.isActiveAndEnabled &&
			    instance.ShipControlMode != ShipControlMode.Piloting)
			{
				_world.InWorldPanels.Pilot.OnDetach();
			}

			if (_world.Map.isActiveAndEnabled && instance.ShipControlMode != ShipControlMode.Navigation)
			{
				_world.Map.OnDetach();
			}

			if (_world.InWorldPanels.Docking.isActiveAndEnabled &&
			    instance.ShipControlMode != ShipControlMode.Docking)
			{
				_world.InWorldPanels.Docking.OnDetach();
			}

			if (instance.ShipControlMode == ShipControlMode.Piloting &&
			    !_world.InWorldPanels.Pilot.isActiveAndEnabled)
			{
				_world.InWorldPanels.Pilot.OnInteract(ParentShip, TargetList, StatusScreen, Radar);
				_world.InWorldPanels.PilotingOptions.SetPilotingMode(instance.ShipControlMode);
			}
			else if (instance.ShipControlMode == ShipControlMode.Navigation && !_world.Map.isActiveAndEnabled)
			{
				_world.Map.OnInteract(ParentShip);
				_world.InWorldPanels.PilotingOptions.SetPilotingMode(instance.ShipControlMode);
			}
			else if (instance.ShipControlMode == ShipControlMode.Docking &&
			         !_world.InWorldPanels.Docking.isActiveAndEnabled)
			{
				_world.InWorldPanels.Docking.OnInteract(ParentShip);
				_world.InWorldPanels.PilotingOptions.SetPilotingMode(instance.ShipControlMode);
			}

			_world.InWorldPanels.PilotingOptions.NavigationDisabled.Activate(ParentShip.VesselBaseSystem.Status !=
				SystemStatus.Online);
		}
	}
}
