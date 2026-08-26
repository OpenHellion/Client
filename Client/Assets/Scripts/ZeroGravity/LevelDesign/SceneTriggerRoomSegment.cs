using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.CharacterMovement;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerRoomSegment : BaseSceneTrigger
	{
		public SceneTriggerRoom BaseRoom;

		private bool isApplicationQuitting;

		public override bool ExclusivePlayerLocking => false;

		public override SceneTriggerType TriggerType => SceneTriggerType.Room;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.DontCheck;

		public override List<ItemType> PlayerHandsItemType => null;

		public override bool IsNearTrigger => true;

		public override bool IsInteractable => false;

		public override bool CameraMovementAllowed => false;

		private void OnDisable()
		{
			if (!(MyPlayer.Instance == null) && !isApplicationQuitting)
			{
				TransitionTriggerHelper componentInChildren =
					MyPlayer.Instance.GetComponentInChildren<TransitionTriggerHelper>();
				if (componentInChildren != null)
				{
					componentInChildren.ExitTriggers(GetComponentsInChildren<Collider>());
				}
			}
		}

		private void OnApplicationQuit()
		{
			isApplicationQuitting = true;
		}
	}
}
