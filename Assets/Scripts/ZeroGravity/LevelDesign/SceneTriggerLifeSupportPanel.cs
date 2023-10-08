using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerLifeSupportPanel : BaseSceneTrigger
	{
		[SerializeField] private bool _cancelExecuterAtSameTime;

		[SerializeField] private bool _isExteriorTrigger;

		private LifeSupportPanel myLifeSupport;

		public override bool ExclusivePlayerLocking => true;

		public override SceneTriggerType TriggerType => SceneTriggerType.LifeSupportPanel;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.StoreItemInHands;

		public override List<ItemType> PlayerHandsItemType => null;

		public override bool IsNearTrigger => true;

		public override bool IsInteractable => true;

		public bool CancelExecuterAtSameTime => _cancelExecuterAtSameTime;

		public bool IsExteriorTrigger => _isExteriorTrigger;

		public override bool CameraMovementAllowed => false;

		public LifeSupportPanel MyLifeSupport
		{
			get
			{
				myLifeSupport = World.InWorldPanels.LifeSupport;
				return myLifeSupport;
			}
		}

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
			MyLifeSupport.OnInteract();
			return true;
		}

		public override void CancelInteract(MyPlayer player)
		{
			base.CancelInteract(player);
			player.DetachFromPanel();
			MyLifeSupport.OnDetach();
		}
	}
}
