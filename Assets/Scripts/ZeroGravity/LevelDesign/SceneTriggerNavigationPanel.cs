using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerNavigationPanel : BaseSceneTrigger
	{
		[SerializeField] private bool _cancelExecuterAtSameTime;

		[SerializeField] private bool _isExteriorTrigger;

		private Ship myParentShip;

		public override bool ExclusivePlayerLocking => false;

		public override SceneTriggerType TriggerType => SceneTriggerType.NavigationPanel;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.StoreItemInHands;

		public override List<ItemType> PlayerHandsItemType => null;

		public override bool IsNearTrigger => true;

		public override bool IsInteractable => true;

		public bool CancelExecuterAtSameTime => _cancelExecuterAtSameTime;

		public bool IsExteriorTrigger => _isExteriorTrigger;

		public override bool CameraMovementAllowed => false;

		public Ship MyParentShip => GetComponentInParent<GeometryRoot>().MainObject as Ship;

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!base.Interact(player, interactWithOverlappingTriggers))
			{
				return false;
			}

			if (interactWithOverlappingTriggers)
			{
				SceneTriggerHelper.InteractWithOverlappingTriggers(base.gameObject, this, player);
			}

			player.AttachToPanel(this);
			World.Map.OnInteract(MyParentShip);
			return true;
		}

		public void InteractWithPanel()
		{
			Interact(MyPlayer.Instance);
		}

		public override void CancelInteract(MyPlayer player)
		{
			base.CancelInteract(player);
			player.DetachFromPanel();
			World.Map.OnDetach();
		}
	}
}
