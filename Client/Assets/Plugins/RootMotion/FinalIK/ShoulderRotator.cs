using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class ShoulderRotator : MonoBehaviour
	{
		[Tooltip("Weight of shoulder rotation")]
		public float weight = 1.5f;

		[Tooltip("The greater the offset, the sooner the shoulder will start rotating")]
		public float offset = 0.2f;

		private FullBodyBipedIK ik;

		private bool skip;

		private void Start()
		{
			ik = GetComponent<FullBodyBipedIK>();
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPostUpdate =
				(IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPostUpdate,
					new IKSolver.UpdateDelegate(RotateShoulders));
		}

		private void RotateShoulders()
		{
			if (!(ik == null) && !(ik.solver.IKPositionWeight <= 0f))
			{
				if (skip)
				{
					skip = false;
					return;
				}

				RotateShoulder(FullBodyBipedChain.LeftArm, weight, offset);
				RotateShoulder(FullBodyBipedChain.RightArm, weight, offset);
				skip = true;
				ik.solver.Update();
			}
		}

		private void RotateShoulder(FullBodyBipedChain chain, float weight, float offset)
		{
			Quaternion b = Quaternion.FromToRotation(GetParentBoneMap(chain).swingDirection,
				ik.solver.GetEndEffector(chain).position - GetParentBoneMap(chain).transform.position);
			Vector3 vector = ik.solver.GetEndEffector(chain).position - ik.solver.GetLimbMapping(chain).bone1.position;
			float num = ik.solver.GetChain(chain).nodes[0].length + ik.solver.GetChain(chain).nodes[1].length;
			float num2 = vector.magnitude / num - 1f + offset;
			num2 = Mathf.Clamp(num2 * weight, 0f, 1f);
			Quaternion quaternion = Quaternion.Lerp(Quaternion.identity, b,
				num2 * ik.solver.GetEndEffector(chain).positionWeight * ik.solver.IKPositionWeight);
			ik.solver.GetLimbMapping(chain).parentBone.rotation =
				quaternion * ik.solver.GetLimbMapping(chain).parentBone.rotation;
		}

		private IKMapping.BoneMap GetParentBoneMap(FullBodyBipedChain chain)
		{
			return ik.solver.GetLimbMapping(chain).GetBoneMap(IKMappingLimb.BoneMapType.Parent);
		}

		private void OnDestroy()
		{
			if (ik != null)
			{
				IKSolverFullBodyBiped solver = ik.solver;
				solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPostUpdate,
					new IKSolver.UpdateDelegate(RotateShoulders));
			}
		}
	}
}
