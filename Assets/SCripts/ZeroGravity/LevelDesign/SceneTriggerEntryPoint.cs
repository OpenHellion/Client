using System.Collections.Generic;
using ZeroGravity.Data;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerEntryPoint : BaseSceneTrigger
	{
		public override bool ExclusivePlayerLocking
		{
			get
			{
				return false;
			}
		}

		public override SceneTriggerType TriggerType
		{
			get
			{
				return SceneTriggerType.EntryPoint;
			}
		}

		public override PlayerHandsCheckType PlayerHandsCheck
		{
			get
			{
				return PlayerHandsCheckType.DontCheck;
			}
		}

		public override List<ItemType> PlayerHandsItemType
		{
			get
			{
				return null;
			}
		}

		public override bool IsNearTrigger
		{
			get
			{
				return true;
			}
		}

		public override bool IsInteractable
		{
			get
			{
				return false;
			}
		}

		public override bool CameraMovementAllowed
		{
			get
			{
				return false;
			}
		}
	}
}
