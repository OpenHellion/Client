using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class ConstraintPosition : Constraint
	{
		public Vector3 position;

		public ConstraintPosition()
		{
		}

		public ConstraintPosition(Transform transform)
		{
			base.transform = transform;
		}

		public override void UpdateConstraint()
		{
			if (!(weight <= 0f) && base.isValid)
			{
				transform.position = Vector3.Lerp(transform.position, position, weight);
			}
		}
	}
}
