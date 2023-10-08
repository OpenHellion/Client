using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class ConstraintRotationOffset : Constraint
	{
		public Quaternion offset;

		private Quaternion defaultRotation;

		private Quaternion defaultLocalRotation;

		private Quaternion lastLocalRotation;

		private Quaternion defaultTargetLocalRotation;

		private bool initiated;

		private bool rotationChanged
		{
			get { return transform.localRotation != lastLocalRotation; }
		}

		public ConstraintRotationOffset()
		{
		}

		public ConstraintRotationOffset(Transform transform)
		{
			base.transform = transform;
		}

		public override void UpdateConstraint()
		{
			if (!(weight <= 0f) && base.isValid)
			{
				if (!initiated)
				{
					defaultLocalRotation = transform.localRotation;
					lastLocalRotation = transform.localRotation;
					initiated = true;
				}

				if (rotationChanged)
				{
					defaultLocalRotation = transform.localRotation;
				}

				transform.localRotation = defaultLocalRotation;
				transform.rotation = Quaternion.Slerp(transform.rotation, offset, weight);
				lastLocalRotation = transform.localRotation;
			}
		}
	}
}
