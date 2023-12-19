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

		public override bool IsNearTrigger => false;

		public override SceneTriggerType TriggerType => SceneTriggerType.Turret;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.StoreItemInHands;

		public override bool ExclusivePlayerLocking => false;

		public override List<ItemType> PlayerHandsItemType => null;

		public bool IsExteriorTrigger => _isExteriorTrigger;

		public override bool IsInteractable => true;

		public bool IsLockable => true;

		public bool CancelExecuterAtSameTime => _cancelExecuterAtSameTime;

		public override bool CameraMovementAllowed => true;

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
