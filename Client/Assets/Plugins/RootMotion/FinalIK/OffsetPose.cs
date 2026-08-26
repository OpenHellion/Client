using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class OffsetPose : MonoBehaviour
	{
		[Serializable]
		public class EffectorLink
		{
			public FullBodyBipedEffector effector;

			public Vector3 offset;

			public Vector3 pin;

			public Vector3 pinWeight;

			public void Apply(IKSolverFullBodyBiped solver, float weight, Quaternion rotation)
			{
				solver.GetEffector(effector).positionOffset += rotation * offset * weight;
				Vector3 vector = solver.GetRoot().position + rotation * pin;
				Vector3 vector2 = vector - solver.GetEffector(effector).bone.position;
				Vector3 vector3 = pinWeight * Mathf.Abs(weight);
				solver.GetEffector(effector).positionOffset = new Vector3(
					Mathf.Lerp(solver.GetEffector(effector).positionOffset.x, vector2.x, vector3.x),
					Mathf.Lerp(solver.GetEffector(effector).positionOffset.y, vector2.y, vector3.y),
					Mathf.Lerp(solver.GetEffector(effector).positionOffset.z, vector2.z, vector3.z));
			}
		}

		public EffectorLink[] effectorLinks = new EffectorLink[0];

		public void Apply(IKSolverFullBodyBiped solver, float weight)
		{
			for (int i = 0; i < effectorLinks.Length; i++)
			{
				effectorLinks[i].Apply(solver, weight, solver.GetRoot().rotation);
			}
		}

		public void Apply(IKSolverFullBodyBiped solver, float weight, Quaternion rotation)
		{
			for (int i = 0; i < effectorLinks.Length; i++)
			{
				effectorLinks[i].Apply(solver, weight, rotation);
			}
		}
	}
}
