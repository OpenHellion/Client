using System.Collections.Generic;
using ZeroGravity.Data;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerPowerSupplyPanel : BaseSceneTrigger
	{
		private PowerSupply _myPowerSupply;

		public override bool ExclusivePlayerLocking => true;

		public override SceneTriggerType TriggerType => SceneTriggerType.PowerSupplyPanel;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.StoreItemInHands;

		public override List<ItemType> PlayerHandsItemType => null;

		public override bool IsNearTrigger => true;

		public override bool IsInteractable => true;

		public override bool CameraMovementAllowed => false;

		public PowerSupply MyPowerSupply
		{
			get
			{
				if (_myPowerSupply == null)
				{
					_myPowerSupply = World.InWorldPanels.PowerSupply;
				}

				return _myPowerSupply;
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
			MyPowerSupply.OnInteract();
			return true;
		}

		public override void CancelInteract(MyPlayer player)
		{
			base.CancelInteract(player);
			player.DetachFromPanel();
			MyPowerSupply.OnDetach();
		}
	}
}
