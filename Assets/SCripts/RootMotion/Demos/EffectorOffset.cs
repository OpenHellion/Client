using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class EffectorOffset : OffsetModifier
	{
		[Range(0f, 1f)]
		public float handsMaintainRelativePositionWeight;

		public Vector3 bodyOffset;

		public Vector3 leftShoulderOffset;

		public Vector3 rightShoulderOffset;

		public Vector3 leftThighOffset;

		public Vector3 rightThighOffset;

		public Vector3 leftHandOffset;

		public Vector3 rightHandOffset;

		public Vector3 leftFootOffset;

		public Vector3 rightFootOffset;

		protected override void OnModifyOffset()
		{
			ik.solver.leftHandEffector.maintainRelativePositionWeight = handsMaintainRelativePositionWeight;
			ik.solver.rightHandEffector.maintainRelativePositionWeight = handsMaintainRelativePositionWeight;
			ik.solver.bodyEffector.positionOffset += base.transform.rotation * bodyOffset;
			ik.solver.leftShoulderEffector.positionOffset += base.transform.rotation * leftShoulderOffset;
			ik.solver.rightShoulderEffector.positionOffset += base.transform.rotation * rightShoulderOffset;
			ik.solver.leftThighEffector.positionOffset += base.transform.rotation * leftThighOffset;
			ik.solver.rightThighEffector.positionOffset += base.transform.rotation * rightThighOffset;
			ik.solver.leftHandEffector.positionOffset += base.transform.rotation * leftHandOffset;
			ik.solver.rightHandEffector.positionOffset += base.transform.rotation * rightHandOffset;
			ik.solver.leftFootEffector.positionOffset += base.transform.rotation * leftFootOffset;
			ik.solver.rightFootEffector.positionOffset += base.transform.rotation * rightFootOffset;
		}
	}
}
