using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKSolverFABRIKRoot : IKSolver
	{
		public int iterations = 4;

		[Range(0f, 1f)] public float rootPin;

		public FABRIKChain[] chains = new FABRIKChain[0];

		private bool zeroWeightApplied;

		private bool[] isRoot;

		private Vector3 rootDefaultPosition;

		public override bool IsValid(ref string message)
		{
			if (chains.Length == 0)
			{
				message = "IKSolverFABRIKRoot contains no chains.";
				return false;
			}

			FABRIKChain[] array = chains;
			foreach (FABRIKChain fABRIKChain in array)
			{
				if (!fABRIKChain.IsValid(ref message))
				{
					return false;
				}
			}

			for (int j = 0; j < chains.Length; j++)
			{
				for (int k = 0; k < chains.Length; k++)
				{
					if (j != k && chains[j].ik == chains[k].ik)
					{
						message = chains[j].ik.name + " is represented more than once in IKSolverFABRIKRoot chain.";
						return false;
					}
				}
			}

			for (int l = 0; l < chains.Length; l++)
			{
				for (int m = 0; m < chains[l].children.Length; m++)
				{
					int num = chains[l].children[m];
					if (num < 0)
					{
						message = chains[l].ik.name + "IKSolverFABRIKRoot chain at index " + l +
						          " has invalid children array. Child index is < 0.";
						return false;
					}

					if (num == l)
					{
						message = chains[l].ik.name + "IKSolverFABRIKRoot chain at index " + l +
						          " has invalid children array. Child index is referencing to itself.";
						return false;
					}

					if (num >= chains.Length)
					{
						message = chains[l].ik.name + "IKSolverFABRIKRoot chain at index " + l +
						          " has invalid children array. Child index > number of chains";
						return false;
					}

					for (int n = 0; n < chains.Length; n++)
					{
						if (num != n)
						{
							continue;
						}

						for (int num2 = 0; num2 < chains[n].children.Length; num2++)
						{
							if (chains[n].children[num2] == l)
							{
								message = "Circular parenting. " + chains[n].ik.name + " already has " +
								          chains[l].ik.name + " listed as it's child.";
								return false;
							}
						}
					}

					for (int num3 = 0; num3 < chains[l].children.Length; num3++)
					{
						if (m != num3 && chains[l].children[num3] == num)
						{
							message = "Chain number " + num + " is represented more than once in the children of " +
							          chains[l].ik.name;
							return false;
						}
					}
				}
			}

			return true;
		}

		public override void StoreDefaultLocalState()
		{
			rootDefaultPosition = root.localPosition;
			for (int i = 0; i < chains.Length; i++)
			{
				chains[i].ik.solver.StoreDefaultLocalState();
			}
		}

		public override void FixTransforms()
		{
			root.localPosition = rootDefaultPosition;
			for (int i = 0; i < chains.Length; i++)
			{
				chains[i].ik.solver.FixTransforms();
			}
		}

		protected override void OnInitiate()
		{
			for (int i = 0; i < chains.Length; i++)
			{
				chains[i].Initiate();
			}

			isRoot = new bool[chains.Length];
			for (int j = 0; j < chains.Length; j++)
			{
				isRoot[j] = IsRoot(j);
			}
		}

		private bool IsRoot(int index)
		{
			for (int i = 0; i < chains.Length; i++)
			{
				for (int j = 0; j < chains[i].children.Length; j++)
				{
					if (chains[i].children[j] == index)
					{
						return false;
					}
				}
			}

			return true;
		}

		protected override void OnUpdate()
		{
			if (IKPositionWeight <= 0f && zeroWeightApplied)
			{
				return;
			}

			IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
			for (int i = 0; i < chains.Length; i++)
			{
				chains[i].ik.solver.IKPositionWeight = IKPositionWeight;
			}

			if (IKPositionWeight <= 0f)
			{
				zeroWeightApplied = true;
				return;
			}

			zeroWeightApplied = false;
			for (int j = 0; j < iterations; j++)
			{
				for (int k = 0; k < chains.Length; k++)
				{
					if (isRoot[k])
					{
						chains[k].Stage1(chains);
					}
				}

				Vector3 centroid = GetCentroid();
				root.position = centroid;
				for (int l = 0; l < chains.Length; l++)
				{
					if (isRoot[l])
					{
						chains[l].Stage2(centroid, chains);
					}
				}
			}
		}

		public override Point[] GetPoints()
		{
			Point[] array = new Point[0];
			for (int i = 0; i < chains.Length; i++)
			{
				AddPointsToArray(ref array, chains[i]);
			}

			return array;
		}

		public override Point GetPoint(Transform transform)
		{
			Point point = null;
			for (int i = 0; i < chains.Length; i++)
			{
				point = chains[i].ik.solver.GetPoint(transform);
				if (point != null)
				{
					return point;
				}
			}

			return null;
		}

		private void AddPointsToArray(ref Point[] array, FABRIKChain chain)
		{
			Point[] points = chain.ik.solver.GetPoints();
			Array.Resize(ref array, array.Length + points.Length);
			int num = 0;
			for (int i = array.Length - points.Length; i < array.Length; i++)
			{
				array[i] = points[num];
				num++;
			}
		}

		private Vector3 GetCentroid()
		{
			Vector3 position = root.position;
			if (rootPin >= 1f)
			{
				return position;
			}

			float num = 0f;
			for (int i = 0; i < chains.Length; i++)
			{
				if (isRoot[i])
				{
					num += chains[i].pull;
				}
			}

			for (int j = 0; j < chains.Length; j++)
			{
				if (isRoot[j] && num > 0f)
				{
					position += (chains[j].ik.solver.bones[0].solverPosition - root.position) *
					            (chains[j].pull / Mathf.Clamp(num, 1f, num));
				}
			}

			return Vector3.Lerp(position, root.position, rootPin);
		}
	}
}
