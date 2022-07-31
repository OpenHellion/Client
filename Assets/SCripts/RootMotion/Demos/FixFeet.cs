using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	[RequireComponent(typeof(FullBodyBipedIK))]
	public class FixFeet : MonoBehaviour
	{
		[Range(0f, 1f)]
		public float weight = 1f;

		private FullBodyBipedIK ik;

		private Vector3 relativePosL;

		private Vector3 relativePosR;

		private Quaternion relativeRotL;

		private Quaternion relativeRotR;

		private void Start()
		{
			ik = GetComponent<FullBodyBipedIK>();
			Sample();
		}

		public void Sample()
		{
			relativePosL = base.transform.InverseTransformPoint(ik.solver.leftFootEffector.bone.position);
			relativePosR = base.transform.InverseTransformPoint(ik.solver.rightFootEffector.bone.position);
			relativeRotL = Quaternion.Inverse(base.transform.rotation) * ik.solver.leftFootEffector.bone.rotation;
			relativeRotR = Quaternion.Inverse(base.transform.rotation) * ik.solver.rightFootEffector.bone.rotation;
		}

		private void LateUpdate()
		{
			if (!(weight <= 0f))
			{
				ik.solver.leftFootEffector.positionOffset = (base.transform.TransformPoint(relativePosL) - ik.solver.leftFootEffector.bone.position) * weight;
				ik.solver.rightFootEffector.positionOffset = (base.transform.TransformPoint(relativePosR) - ik.solver.rightFootEffector.bone.position) * weight;
				ik.solver.leftFootEffector.bone.rotation = Quaternion.Lerp(ik.solver.leftFootEffector.bone.rotation, base.transform.rotation * relativeRotL, weight);
				ik.solver.rightFootEffector.bone.rotation = Quaternion.Lerp(ik.solver.rightFootEffector.bone.rotation, base.transform.rotation * relativeRotR, weight);
			}
		}
	}
}
