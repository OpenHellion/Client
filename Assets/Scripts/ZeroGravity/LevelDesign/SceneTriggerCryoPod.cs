using System.Collections.Generic;
using ZeroGravity.Data;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerCryoPod : BaseSceneTrigger
	{
		public SceneSpawnPoint SpawnPoint;

		public override bool ExclusivePlayerLocking => true;

		public override SceneTriggerType TriggerType => SceneTriggerType.CryoPodPanel;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.DontCheck;

		public override List<ItemType> PlayerHandsItemType => null;

		public override bool IsNearTrigger => true;

		public override bool IsInteractable => true;

		public override bool CameraMovementAllowed => false;

		private CryoPodUI MyCryoPod => World.InWorldPanels.Cryo;

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!base.Interact(player, interactWithOverlappingTriggers))
			{
				return false;
			}

			MyCryoPod.SpawnPoint = SpawnPoint;
			if (player.CurrentActiveItem != null)
			{
				if (player.CurrentActiveItem is DisposableHackingTool)
				{
					MyCryoPod.SpawnPoint.HackSpawnPoint();
				}
				else if (player.Inventory == null || !player.Inventory.StoreItemInHands())
				{
					return false;
				}
			}

			player.AttachToPanel(this);
			MyCryoPod.ToggleCanvas(true);
			return true;
		}

		public override void CancelInteract(MyPlayer player)
		{
			base.CancelInteract(player);
			player.DetachFromPanel();
			MyCryoPod.ToggleCanvas(false);
		}
	}
}
