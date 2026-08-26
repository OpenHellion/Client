using System.Collections.Generic;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerTextLogTerminal : SceneTriggerPanels
	{
		public LogObject LogObject;

		private TextLogTerminal _terminalUI;

		public override SceneTriggerType TriggerType => SceneTriggerType.TextLogTerminal;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.StoreItemInHands;

		public override List<ItemType> PlayerHandsItemType => null;

		public override bool IsNearTrigger => false;

		public override bool IsInteractable => true;

		public override bool CameraMovementAllowed => false;

		private TextLogTerminal TerminalUI => World.InWorldPanels.TextLogTerminal;

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!base.Interact(player, interactWithOverlappingTriggers))
			{
				return false;
			}

			if (player.CurrentActiveItem != null && (player.Inventory == null || !player.Inventory.StoreItemInHands()))
			{
				return false;
			}

			player.AttachToPanel(this);
			TerminalUI.LogObject = LogObject;
			TerminalUI.OnInteract();
			return true;
		}

		public override void CancelInteract(MyPlayer player)
		{
			player.DetachFromPanel();
			TerminalUI.OnDetach();
		}
	}
}
