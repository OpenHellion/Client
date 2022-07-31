using System.Collections.Generic;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

public class SceneDockingTrigger : BaseSceneTrigger
{
	public override bool ExclusivePlayerLocking
	{
		get
		{
			return true;
		}
	}

	public override SceneTriggerType TriggerType
	{
		get
		{
			return SceneTriggerType.DockingPortTriger;
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
			return true;
		}
	}

	public override bool CameraMovementAllowed
	{
		get
		{
			return false;
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
		return true;
	}
}
