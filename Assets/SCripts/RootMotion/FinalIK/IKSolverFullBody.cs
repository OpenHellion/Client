using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKSolverFullBody : IKSolver
	{
		[Range(0f, 10f)]
		public int iterations = 4;

		public FBIKChain[] chain = new FBIKChain[0];

		public IKEffector[] effectors = new IKEffector[0];

		public IKMappingSpine spineMapping = new IKMappingSpine();

		public IKMappingBone[] boneMappings = new IKMappingBone[0];

		public IKMappingLimb[] limbMappings = new IKMappingLimb[0];

		public UpdateDelegate OnPreRead;

		public UpdateDelegate OnPreSolve;

		public IterationDelegate OnPreIteration;

		public IterationDelegate OnPostIteration;

		public UpdateDelegate OnPreBend;

		public UpdateDelegate OnPostSolve;

		public UpdateDelegate OnStoreDefaultLocalState;

		public UpdateDelegate OnFixTransforms;

		public IKEffector GetEffector(Transform t)
		{
			for (int i = 0; i < effectors.Length; i++)
			{
				if (effectors[i].bone == t)
				{
					return effectors[i];
				}
			}
			return null;
		}

		public FBIKChain GetChain(Transform transform)
		{
			int chainIndex = GetChainIndex(transform);
			if (chainIndex == -1)
			{
				return null;
			}
			return chain[chainIndex];
		}

		public int GetChainIndex(Transform transform)
		{
			for (int i = 0; i < chain.Length; i++)
			{
				for (int j = 0; j < chain[i].nodes.Length; j++)
				{
					if (chain[i].nodes[j].transform == transform)
					{
						return i;
					}
				}
			}
			return -1;
		}

		public Node GetNode(int chainIndex, int nodeIndex)
		{
			return chain[chainIndex].nodes[nodeIndex];
		}

		public void GetChainAndNodeIndexes(Transform transform, out int chainIndex, out int nodeIndex)
		{
			chainIndex = GetChainIndex(transform);
			if (chainIndex == -1)
			{
				nodeIndex = -1;
			}
			else
			{
				nodeIndex = chain[chainIndex].GetNodeIndex(transform);
			}
		}

		public override Point[] GetPoints()
		{
			int num = 0;
			for (int i = 0; i < chain.Length; i++)
			{
				num += chain[i].nodes.Length;
			}
			Point[] array = new Point[num];
			int num2 = 0;
			for (int j = 0; j < chain.Length; j++)
			{
				for (int k = 0; k < chain[j].nodes.Length; k++)
				{
					array[num2] = chain[j].nodes[k];
				}
			}
			return array;
		}

		public override Point GetPoint(Transform transform)
		{
			for (int i = 0; i < chain.Length; i++)
			{
				for (int j = 0; j < chain[i].nodes.Length; j++)
				{
					if (chain[i].nodes[j].transform == transform)
					{
						return chain[i].nodes[j];
					}
				}
			}
			return null;
		}

		public override bool IsValid(ref string message)
		{
			if (chain == null)
			{
				message = "FBIK chain is null, can't initiate solver.";
				return false;
			}
			if (chain.Length == 0)
			{
				message = "FBIK chain length is 0, can't initiate solver.";
				return false;
			}
			for (int i = 0; i < chain.Length; i++)
			{
				if (!chain[i].IsValid(ref message))
				{
					return false;
				}
			}
			IKEffector[] array = effectors;
			foreach (IKEffector iKEffector in array)
			{
				if (!iKEffector.IsValid(this, ref message))
				{
					return false;
				}
			}
			if (!spineMapping.IsValid(this, ref message))
			{
				return false;
			}
			IKMappingLimb[] array2 = limbMappings;
			foreach (IKMappingLimb iKMappingLimb in array2)
			{
				if (!iKMappingLimb.IsValid(this, ref message))
				{
					return false;
				}
			}
			IKMappingBone[] array3 = boneMappings;
			foreach (IKMappingBone iKMappingBone in array3)
			{
				if (!iKMappingBone.IsValid(this, ref message))
				{
					return false;
				}
			}
			return true;
		}

		public override void StoreDefaultLocalState()
		{
			spineMapping.StoreDefaultLocalState();
			for (int i = 0; i < limbMappings.Length; i++)
			{
				limbMappings[i].StoreDefaultLocalState();
			}
			for (int j = 0; j < boneMappings.Length; j++)
			{
				boneMappings[j].StoreDefaultLocalState();
			}
			if (OnStoreDefaultLocalState != null)
			{
				OnStoreDefaultLocalState();
			}
		}

		public override void FixTransforms()
		{
			if (!(IKPositionWeight <= 0f))
			{
				spineMapping.FixTransforms();
				for (int i = 0; i < limbMappings.Length; i++)
				{
					limbMappings[i].FixTransforms();
				}
				for (int j = 0; j < boneMappings.Length; j++)
				{
					boneMappings[j].FixTransforms();
				}
				if (OnFixTransforms != null)
				{
					OnFixTransforms();
				}
			}
		}

		protected override void OnInitiate()
		{
			for (int i = 0; i < chain.Length; i++)
			{
				chain[i].Initiate(this);
			}
			IKEffector[] array = effectors;
			foreach (IKEffector iKEffector in array)
			{
				iKEffector.Initiate(this);
			}
			spineMapping.Initiate(this);
			IKMappingBone[] array2 = boneMappings;
			foreach (IKMappingBone iKMappingBone in array2)
			{
				iKMappingBone.Initiate(this);
			}
			IKMappingLimb[] array3 = limbMappings;
			foreach (IKMappingLimb iKMappingLimb in array3)
			{
				iKMappingLimb.Initiate(this);
			}
		}

		protected override void OnUpdate()
		{
			if (IKPositionWeight <= 0f)
			{
				for (int i = 0; i < effectors.Length; i++)
				{
					effectors[i].positionOffset = Vector3.zero;
				}
			}
			else if (chain.Length != 0)
			{
				IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
				if (OnPreRead != null)
				{
					OnPreRead();
				}
				ReadPose();
				if (OnPreSolve != null)
				{
					OnPreSolve();
				}
				Solve();
				if (OnPostSolve != null)
				{
					OnPostSolve();
				}
				WritePose();
				for (int j = 0; j < effectors.Length; j++)
				{
					effectors[j].OnPostWrite();
				}
			}
		}

		protected virtual void ReadPose()
		{
			for (int i = 0; i < chain.Length; i++)
			{
				if (chain[i].bendConstraint.initiated)
				{
					chain[i].bendConstraint.LimitBend(IKPositionWeight, GetEffector(chain[i].nodes[2].transform).positionWeight);
				}
			}
			for (int j = 0; j < effectors.Length; j++)
			{
				effectors[j].ResetOffset(this);
			}
			for (int k = 0; k < effectors.Length; k++)
			{
				effectors[k].OnPreSolve(this);
			}
			for (int l = 0; l < chain.Length; l++)
			{
				chain[l].ReadPose(this, iterations > 0);
			}
			if (iterations > 0)
			{
				spineMapping.ReadPose();
				for (int m = 0; m < boneMappings.Length; m++)
				{
					boneMappings[m].ReadPose();
				}
			}
			for (int n = 0; n < limbMappings.Length; n++)
			{
				limbMappings[n].ReadPose();
			}
		}

		protected virtual void Solve()
		{
			if (iterations > 0)
			{
				for (int i = 0; i < iterations; i++)
				{
					if (OnPreIteration != null)
					{
						OnPreIteration(i);
					}
					for (int j = 0; j < effectors.Length; j++)
					{
						if (effectors[j].isEndEffector)
						{
							effectors[j].Update(this);
						}
					}
					chain[0].Push(this);
					chain[0].Reach(this);
					for (int k = 0; k < effectors.Length; k++)
					{
						if (!effectors[k].isEndEffector)
						{
							effectors[k].Update(this);
						}
					}
					chain[0].SolveTrigonometric(this);
					chain[0].Stage1(this);
					for (int l = 0; l < effectors.Length; l++)
					{
						if (!effectors[l].isEndEffector)
						{
							effectors[l].Update(this);
						}
					}
					chain[0].Stage2(this, chain[0].nodes[0].solverPosition);
					if (OnPostIteration != null)
					{
						OnPostIteration(i);
					}
				}
			}
			if (OnPreBend != null)
			{
				OnPreBend();
			}
			for (int m = 0; m < effectors.Length; m++)
			{
				if (effectors[m].isEndEffector)
				{
					effectors[m].Update(this);
				}
			}
			ApplyBendConstraints();
		}

		protected virtual void ApplyBendConstraints()
		{
			chain[0].SolveTrigonometric(this, true);
		}

		protected virtual void WritePose()
		{
			if (IKPositionWeight <= 0f)
			{
				return;
			}
			if (iterations > 0)
			{
				spineMapping.WritePose(this);
				for (int i = 0; i < boneMappings.Length; i++)
				{
					boneMappings[i].WritePose(IKPositionWeight);
				}
			}
			for (int j = 0; j < limbMappings.Length; j++)
			{
				limbMappings[j].WritePose(this, iterations > 0);
			}
		}
	}
}
