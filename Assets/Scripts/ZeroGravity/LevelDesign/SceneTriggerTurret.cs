using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerTurret : BaseSceneTrigger
	{
		[SerializeField] private bool _isExteriorTrigger;

		[SerializeField] private bool _cancelExecuterAtSameTime;

		public override bool IsNearTrigger
		{
			get { return false; }
		}

		public override SceneTriggerType TriggerType
		{
			get { return SceneTriggerType.Turret; }
		}

		public override PlayerHandsCheckType PlayerHandsCheck
		{
			get { return PlayerHandsCheckType.StoreItemInHands; }
		}

		public override bool ExclusivePlayerLocking
		{
			get { return false; }
		}

		public override List<ItemType> PlayerHandsItemType
		{
			get { return null; }
		}

		public bool IsExteriorTrigger
		{
			get { return _isExteriorTrigger; }
		}

		public override bool IsInteractable
		{
			get { return true; }
		}

		public bool IsLockable
		{
			get { return true; }
		}

		public bool CancelExecuterAtSameTime
		{
			get { return _cancelExecuterAtSameTime; }
		}

		public override bool CameraMovementAllowed
		{
			get { return true; }
		}

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!base.Interact(player, interactWithOverlappingTriggers))
			{
				return false;
			}

			player.FpsController.ResetVelocity();
			player.FpsController.ToggleAttached(true);
			player.FpsController.ToggleMovement(false);
			player.LockedToTrigger = player.LookingAtTrigger;
			if (interactWithOverlappingTriggers)
			{
				SceneTriggerHelper.InteractWithOverlappingTriggers(base.gameObject, this, player);
			}

			return true;
		}
	}
}
