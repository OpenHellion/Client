using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class FABRIKChain
	{
		public FABRIK ik;

		[Range(0f, 1f)] public float pull = 1f;

		[Range(0f, 1f)] public float pin = 1f;

		public int[] children = new int[0];

		public bool IsValid(ref string message)
		{
			if (ik == null)
			{
				message = "IK unassigned in FABRIKChain.";
				return false;
			}

			if (!ik.solver.IsValid(ref message))
			{
				return false;
			}

			return true;
		}

		public void Initiate()
		{
			ik.enabled = false;
		}

		public void Stage1(FABRIKChain[] chain)
		{
			for (int i = 0; i < children.Length; i++)
			{
				chain[children[i]].Stage1(chain);
			}

			if (children.Length == 0)
			{
				ik.solver.SolveForward(ik.solver.GetIKPosition());
			}
			else
			{
				ik.solver.SolveForward(GetCentroid(chain));
			}
		}

		public void Stage2(Vector3 rootPosition, FABRIKChain[] chain)
		{
			ik.solver.SolveBackward(rootPosition);
			for (int i = 0; i < children.Length; i++)
			{
				chain[children[i]].Stage2(ik.solver.bones[ik.solver.bones.Length - 1].transform.position, chain);
			}
		}

		private Vector3 GetCentroid(FABRIKChain[] chain)
		{
			Vector3 iKPosition = ik.solver.GetIKPosition();
			if (pin >= 1f)
			{
				return iKPosition;
			}

			float num = 0f;
			for (int i = 0; i < children.Length; i++)
			{
				num += chain[children[i]].pull;
			}

			if (num <= 0f)
			{
				return iKPosition;
			}

			if (num < 1f)
			{
				num = 1f;
			}

			Vector3 vector = iKPosition;
			for (int j = 0; j < children.Length; j++)
			{
				Vector3 vector2 = chain[children[j]].ik.solver.bones[0].solverPosition - iKPosition;
				float num2 = chain[children[j]].pull / num;
				vector += vector2 * num2;
			}

			if (pin <= 0f)
			{
				return vector;
			}

			return vector + (iKPosition - vector) * pin;
		}
	}
}
