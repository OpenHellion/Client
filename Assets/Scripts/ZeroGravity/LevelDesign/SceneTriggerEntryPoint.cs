using System.Collections.Generic;
using ZeroGravity.Data;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerEntryPoint : BaseSceneTrigger
	{
		public override bool ExclusivePlayerLocking => false;

		public override SceneTriggerType TriggerType => SceneTriggerType.EntryPoint;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.DontCheck;

		public override List<ItemType> PlayerHandsItemType => null;

		public override bool IsNearTrigger => true;

		public override bool IsInteractable => false;

		public override bool CameraMovementAllowed => false;
	}
}
