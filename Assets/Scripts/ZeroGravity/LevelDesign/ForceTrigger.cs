using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class ForceTrigger : BaseSceneTrigger
	{
		public float ForceIntensity = 1f;

		public bool Continuous;

		public override bool ExclusivePlayerLocking
		{
			get { return false; }
		}

		public override bool CameraMovementAllowed
		{
			get { return false; }
		}

		public override bool IsInteractable
		{
			get { return false; }
		}

		public override bool IsNearTrigger
		{
			get { return true; }
		}

		public override PlayerHandsCheckType PlayerHandsCheck
		{
			get { return PlayerHandsCheckType.DontCheck; }
		}

		public override List<ItemType> PlayerHandsItemType
		{
			get { return null; }
		}

		public override SceneTriggerType TriggerType
		{
			get { return SceneTriggerType.HurtTrigger; }
		}

		private new void OnTriggerEnter(Collider coli)
		{
			if (Continuous)
			{
				return;
			}

			if (coli.gameObject == MyPlayer.Instance.gameObject)
			{
				MyPlayer.Instance.rigidBody.AddForce(base.transform.forward * ForceIntensity, ForceMode.VelocityChange);
				return;
			}

			DynamicObject componentInParent = coli.GetComponentInParent<DynamicObject>();
			if (componentInParent != null && componentInParent.Master && !componentInParent.IsAttached)
			{
				if (componentInParent.IsKinematic)
				{
					componentInParent.ToggleKinematic(false);
				}

				componentInParent.rigidBody.velocity =
					Vector3.ProjectOnPlane(componentInParent.rigidBody.velocity, base.transform.forward) +
					base.transform.forward * ForceIntensity;
			}
		}

		private void OnTriggerStay(Collider coli)
		{
			if (!Continuous)
			{
				return;
			}

			if (coli.gameObject == MyPlayer.Instance.gameObject)
			{
				MyPlayer.Instance.rigidBody.AddForce(base.transform.forward * ForceIntensity, ForceMode.VelocityChange);
				return;
			}

			DynamicObject componentInParent = coli.GetComponentInParent<DynamicObject>();
			if (componentInParent != null && componentInParent.Master && !componentInParent.IsAttached)
			{
				if (componentInParent.IsKinematic)
				{
					componentInParent.ToggleKinematic(false);
				}

				componentInParent.rigidBody.velocity =
					Vector3.ProjectOnPlane(componentInParent.rigidBody.velocity, base.transform.forward) +
					base.transform.forward * ForceIntensity;
			}
		}
	}
}
