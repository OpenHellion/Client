using UnityEngine;

namespace RootMotion.FinalIK
{
	public class BodyTilt : OffsetModifier
	{
		[Tooltip("Speed of tilting")] public float tiltSpeed = 6f;

		[Tooltip("Sensitivity of tilting")] public float tiltSensitivity = 0.07f;

		[Tooltip("The OffsetPose components")] public OffsetPose poseLeft;

		[Tooltip("The OffsetPose components")] public OffsetPose poseRight;

		private float tiltAngle;

		private Vector3 lastForward;

		protected override void Start()
		{
			base.Start();
			lastForward = base.transform.forward;
		}

		protected override void OnModifyOffset()
		{
			Quaternion quaternion = Quaternion.FromToRotation(lastForward, base.transform.forward);
			float angle = 0f;
			Vector3 axis = Vector3.zero;
			quaternion.ToAngleAxis(out angle, out axis);
			if (axis.y > 0f)
			{
				angle = 0f - angle;
			}

			angle *= tiltSensitivity * 0.01f;
			angle /= base.deltaTime;
			angle = Mathf.Clamp(angle, -1f, 1f);
			tiltAngle = Mathf.Lerp(tiltAngle, angle, base.deltaTime * tiltSpeed);
			float num = Mathf.Abs(tiltAngle) / 1f;
			if (tiltAngle < 0f)
			{
				poseRight.Apply(ik.solver, num);
			}
			else
			{
				poseLeft.Apply(ik.solver, num);
			}

			lastForward = base.transform.forward;
		}
	}
}
