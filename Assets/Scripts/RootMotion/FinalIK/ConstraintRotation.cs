using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class ConstraintRotation : Constraint
	{
		public Quaternion rotation;

		public ConstraintRotation()
		{
		}

		public ConstraintRotation(Transform transform)
		{
			base.transform = transform;
		}

		public override void UpdateConstraint()
		{
			if (!(weight <= 0f) && base.isValid)
			{
				transform.rotation = Quaternion.Slerp(transform.rotation, rotation, weight);
			}
		}
	}
}
