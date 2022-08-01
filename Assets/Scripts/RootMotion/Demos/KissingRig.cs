using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class KissingRig : MonoBehaviour
	{
		[Serializable]
		public class Partner
		{
			public FullBodyBipedIK ik;

			public Transform mouth;

			public Transform mouthTarget;

			public Transform touchTargetLeftHand;

			public Transform touchTargetRightHand;

			public float bodyWeightHorizontal = 0.4f;

			public float bodyWeightVertical = 1f;

			public float neckRotationWeight = 0.3f;

			public float headTiltAngle = 10f;

			public Vector3 headTiltAxis;

			private Quaternion neckRotation;

			private Transform neck
			{
				get
				{
					return ik.solver.spineMapping.spineBones[ik.solver.spineMapping.spineBones.Length - 1];
				}
			}

			public void Initiate()
			{
				ik.enabled = false;
			}

			public void Update(float weight)
			{
				ik.solver.leftShoulderEffector.positionWeight = weight;
				ik.solver.rightShoulderEffector.positionWeight = weight;
				ik.solver.leftHandEffector.positionWeight = weight;
				ik.solver.rightHandEffector.positionWeight = weight;
				ik.solver.leftHandEffector.rotationWeight = weight;
				ik.solver.rightHandEffector.rotationWeight = weight;
				ik.solver.bodyEffector.positionWeight = weight;
				InverseTransformEffector(FullBodyBipedEffector.LeftShoulder, mouth, mouthTarget.position, weight);
				InverseTransformEffector(FullBodyBipedEffector.RightShoulder, mouth, mouthTarget.position, weight);
				InverseTransformEffector(FullBodyBipedEffector.Body, mouth, mouthTarget.position, weight);
				ik.solver.bodyEffector.position = Vector3.Lerp(new Vector3(ik.solver.bodyEffector.position.x, ik.solver.bodyEffector.bone.position.y, ik.solver.bodyEffector.position.z), ik.solver.bodyEffector.position, bodyWeightVertical * weight);
				ik.solver.bodyEffector.position = Vector3.Lerp(new Vector3(ik.solver.bodyEffector.bone.position.x, ik.solver.bodyEffector.position.y, ik.solver.bodyEffector.bone.position.z), ik.solver.bodyEffector.position, bodyWeightHorizontal * weight);
				ik.solver.leftHandEffector.position = touchTargetLeftHand.position;
				ik.solver.rightHandEffector.position = touchTargetRightHand.position;
				ik.solver.leftHandEffector.rotation = touchTargetLeftHand.rotation;
				ik.solver.rightHandEffector.rotation = touchTargetRightHand.rotation;
				neckRotation = neck.rotation;
				ik.solver.Update();
				neck.rotation = Quaternion.Slerp(neck.rotation, neckRotation, neckRotationWeight * weight);
				ik.references.head.localRotation = Quaternion.AngleAxis(headTiltAngle * weight, headTiltAxis) * ik.references.head.localRotation;
			}

			private void InverseTransformEffector(FullBodyBipedEffector effector, Transform target, Vector3 targetPosition, float weight)
			{
				Vector3 vector = ik.solver.GetEffector(effector).bone.position - target.position;
				ik.solver.GetEffector(effector).position = Vector3.Lerp(ik.solver.GetEffector(effector).bone.position, targetPosition + vector, weight);
			}
		}

		public Partner partner1;

		public Partner partner2;

		public float weight;

		public int iterations = 3;

		private void Start()
		{
			partner1.Initiate();
			partner2.Initiate();
		}

		private void LateUpdate()
		{
			for (int i = 0; i < iterations; i++)
			{
				partner1.Update(weight);
				partner2.Update(weight);
			}
		}
	}
}
