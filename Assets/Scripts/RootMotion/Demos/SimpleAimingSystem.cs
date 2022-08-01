using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class SimpleAimingSystem : MonoBehaviour
	{
		[Tooltip("AimPoser is a tool that returns an animation name based on direction.")]
		public AimPoser aimPoser;

		[Tooltip("Reference to the AimIK component.")]
		public AimIK aim;

		[Tooltip("Reference to the LookAt component (only used for the head in this instance).")]
		public LookAtIK lookAt;

		[Tooltip("Reference to the Animator component.")]
		public Animator animator;

		[Tooltip("Time of cross-fading from pose to pose.")]
		public float crossfadeTime = 0.2f;

		[Tooltip("Will keep the aim target at a distance.")]
		public float minAimDistance = 0.5f;

		private AimPoser.Pose aimPose;

		private AimPoser.Pose lastPose;

		private void Start()
		{
			aim.enabled = false;
			lookAt.enabled = false;
		}

		private void LateUpdate()
		{
			if (aim.solver.target == null)
			{
				Debug.LogWarning("AimIK and LookAtIK need to have their 'Target' value assigned.", base.transform);
			}
			Pose();
			aim.solver.Update();
			if (lookAt != null)
			{
				lookAt.solver.Update();
			}
		}

		private void Pose()
		{
			LimitAimTarget();
			Vector3 direction = aim.solver.target.position - aim.solver.bones[0].transform.position;
			Vector3 localDirection = base.transform.InverseTransformDirection(direction);
			aimPose = aimPoser.GetPose(localDirection);
			if (aimPose != lastPose)
			{
				aimPoser.SetPoseActive(aimPose);
				lastPose = aimPose;
			}
			AimPoser.Pose[] poses = aimPoser.poses;
			foreach (AimPoser.Pose pose in poses)
			{
				if (pose == aimPose)
				{
					DirectCrossFade(pose.name, 1f);
				}
				else
				{
					DirectCrossFade(pose.name, 0f);
				}
			}
		}

		private void LimitAimTarget()
		{
			Vector3 position = aim.solver.bones[0].transform.position;
			Vector3 vector = aim.solver.target.position - position;
			vector = vector.normalized * Mathf.Max(vector.magnitude, minAimDistance);
			aim.solver.target.position = position + vector;
		}

		private void DirectCrossFade(string state, float target)
		{
			float value = Mathf.MoveTowards(animator.GetFloat(state), target, Time.deltaTime * (1f / crossfadeTime));
			animator.SetFloat(state, value);
		}
	}
}
