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
				return SceneTriggerType.Room;
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

		private void OnDisable()
		{
			if (!(MyPlayer.Instance == null) && !isApplicationQuitting)
			{
				TransitionTriggerHelper componentInChildren = MyPlayer.Instance.GetComponentInChildren<TransitionTriggerHelper>();
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
