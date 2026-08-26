using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class FBIKHandsOnProp : MonoBehaviour
	{
		public FullBodyBipedIK ik;

		public bool leftHanded;

		private void Awake()
		{
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPreRead =
				(IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPreRead, new IKSolver.UpdateDelegate(OnPreRead));
		}

		private void OnPreRead()
		{
			if (leftHanded)
			{
				HandsOnProp(ik.solver.leftHandEffector, ik.solver.rightHandEffector);
			}
			else
			{
				HandsOnProp(ik.solver.rightHandEffector, ik.solver.leftHandEffector);
			}
		}

		private void HandsOnProp(IKEffector mainHand, IKEffector otherHand)
		{
			Vector3 vector = otherHand.bone.position - mainHand.bone.position;
			Vector3 vector2 = Quaternion.Inverse(mainHand.bone.rotation) * vector;
			Vector3 vector3 = mainHand.bone.position + vector * 0.5f;
			Quaternion quaternion = Quaternion.Inverse(mainHand.bone.rotation) * otherHand.bone.rotation;
			Vector3 toDirection = otherHand.bone.position + otherHand.positionOffset -
			                      (mainHand.bone.position + mainHand.positionOffset);
			Vector3 vector4 = mainHand.bone.position + mainHand.positionOffset + vector * 0.5f;
			mainHand.position = mainHand.bone.position + mainHand.positionOffset + (vector4 - vector3);
			mainHand.positionWeight = 1f;
			Quaternion quaternion2 = Quaternion.FromToRotation(vector, toDirection);
			mainHand.rotation = quaternion2 * mainHand.bone.rotation;
			mainHand.rotationWeight = 1f;
			otherHand.position = mainHand.position + mainHand.rotation * vector2;
			otherHand.positionWeight = 1f;
			otherHand.rotation = mainHand.rotation * quaternion;
			otherHand.rotationWeight = 1f;
		}

		private void OnDestroy()
		{
			if (ik != null)
			{
				IKSolverFullBodyBiped solver = ik.solver;
				solver.OnPreRead =
					(IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPreRead, new IKSolver.UpdateDelegate(OnPreRead));
			}
		}
	}
}
