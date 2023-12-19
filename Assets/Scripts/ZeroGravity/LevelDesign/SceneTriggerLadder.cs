using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerLadder : BaseSceneTrigger
	{
		public Transform StartPosition;

		public float MaxAttachVelocity = 10f;

		public List<Transform> DetachPoints = new List<Transform>();

		public override bool ExclusivePlayerLocking => false;

		public override SceneTriggerType TriggerType => SceneTriggerType.Ladder;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.DontCheck;

		public override List<ItemType> PlayerHandsItemType => null;

		public override bool IsNearTrigger => true;

		public override bool IsInteractable => true;

		public override bool CameraMovementAllowed => false;

		public void LadderAttach(MyPlayer pl, bool checkGravity = true)
		{
			if (!checkGravity || !pl.Gravity.IsEpsilonEqual(Vector3.zero))
			{
				pl.FpsController.ToggleOnLadder(this, true);
				pl.FpsController.ToggleAttached(true);
				pl.transform.position = StartPosition.position +
				                        Vector3.Project(pl.transform.position - StartPosition.position,
					                        StartPosition.up);
				pl.transform.rotation = StartPosition.rotation;
				pl.FpsController.ResetVelocity();
				pl.FpsController.ToggleAutoFreeLook(true);
				pl.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null,
					null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
					null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
					true);
			}
		}

		public void LadderDetach(MyPlayer pl)
		{
			MyPlayer.Instance.LockedToTrigger = null;
			if (DetachPoints != null && DetachPoints.Count > 0)
			{
				Vector3 vector = pl.transform.up * 1.34f;
				Vector3 vector2 = DetachPoints[0].position + vector;
				float num = (vector2 - pl.transform.position).sqrMagnitude;
				for (int i = 1; i < DetachPoints.Count; i++)
				{
					float sqrMagnitude = (DetachPoints[i].position + vector - pl.transform.position).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						vector2 = DetachPoints[i].position + vector;
					}
				}

				pl.rigidBody.AddForce((vector2 - pl.transform.position).normalized * 200f, ForceMode.Impulse);
			}
			else
			{
				pl.rigidBody.AddForce(pl.transform.right * 200f, ForceMode.Impulse);
			}

			pl.rigidBody.AddForce(pl.transform.up * 100f, ForceMode.Impulse);
			pl.FpsController.ToggleOnLadder(null, false);
			pl.FpsController.ToggleAutoFreeLook(false);
			pl.FpsController.ToggleAttached(false);
			pl.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false);
		}

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!base.Interact(player, interactWithOverlappingTriggers))
			{
				return false;
			}

			LadderAttach(player);
			if (interactWithOverlappingTriggers)
			{
				SceneTriggerHelper.InteractWithOverlappingTriggers(base.gameObject, this, player);
			}

			return true;
		}

		public override void CancelInteract(MyPlayer player)
		{
			base.CancelInteract(player);
			LadderDetach(player);
		}
	}
}
