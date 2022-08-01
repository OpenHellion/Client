using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class FBIKChain
	{
		[Serializable]
		public class ChildConstraint
		{
			public float pushElasticity;

			public float pullElasticity;

			[SerializeField]
			private Transform bone1;

			[SerializeField]
			private Transform bone2;

			private float crossFade;

			private float inverseCrossFade;

			private int chain1Index;

			private int chain2Index;

			public float nominalDistance { get; private set; }

			public bool isRigid { get; private set; }

			public ChildConstraint(Transform bone1, Transform bone2, float pushElasticity = 0f, float pullElasticity = 0f)
			{
				this.bone1 = bone1;
				this.bone2 = bone2;
				this.pushElasticity = pushElasticity;
				this.pullElasticity = pullElasticity;
			}

			public void Initiate(IKSolverFullBody solver)
			{
				chain1Index = solver.GetChainIndex(bone1);
				chain2Index = solver.GetChainIndex(bone2);
				OnPreSolve(solver);
			}

			public void OnPreSolve(IKSolverFullBody solver)
			{
				nominalDistance = Vector3.Distance(solver.chain[chain1Index].nodes[0].transform.position, solver.chain[chain2Index].nodes[0].transform.position);
				isRigid = pushElasticity <= 0f && pullElasticity <= 0f;
				if (isRigid)
				{
					float num = solver.chain[chain1Index].pull - solver.chain[chain2Index].pull;
					crossFade = 1f - (0.5f + num * 0.5f);
				}
				else
				{
					crossFade = 0.5f;
				}
				inverseCrossFade = 1f - crossFade;
			}

			public void Solve(IKSolverFullBody solver)
			{
				if (pushElasticity >= 1f && pullElasticity >= 1f)
				{
					return;
				}
				Vector3 vector = solver.chain[chain2Index].nodes[0].solverPosition - solver.chain[chain1Index].nodes[0].solverPosition;
				float magnitude = vector.magnitude;
				if (magnitude != nominalDistance && magnitude != 0f)
				{
					float num = 1f;
					if (!isRigid)
					{
						float num2 = ((!(magnitude > nominalDistance)) ? pushElasticity : pullElasticity);
						num = 1f - num2;
					}
					num *= 1f - nominalDistance / magnitude;
					Vector3 vector2 = vector * num;
					solver.chain[chain1Index].nodes[0].solverPosition += vector2 * crossFade;
					solver.chain[chain2Index].nodes[0].solverPosition -= vector2 * inverseCrossFade;
				}
			}
		}

		[Serializable]
		public enum Smoothing
		{
			None = 0,
			Exponential = 1,
			Cubic = 2
		}

		[Range(0f, 1f)]
		public float pin;

		[Range(0f, 1f)]
		public float pull = 1f;

		[Range(0f, 1f)]
		public float push;

		[Range(-1f, 1f)]
		public float pushParent;

		[Range(0f, 1f)]
		public float reach = 0.1f;

		public Smoothing reachSmoothing = Smoothing.Exponential;

		public Smoothing pushSmoothing = Smoothing.Exponential;

		public IKSolver.Node[] nodes = new IKSolver.Node[0];

		public int[] children = new int[0];

		public ChildConstraint[] childConstraints = new ChildConstraint[0];

		public IKConstraintBend bendConstraint = new IKConstraintBend();

		private float rootLength;

		private bool initiated;

		private float length;

		private float distance;

		private IKSolver.Point p;

		private float reachForce;

		private float pullParentSum;

		private float[] crossFades;

		private float sqrMag1;

		private float sqrMag2;

		private float sqrMagDif;

		private const float maxLimbLength = 0.99999f;

		public FBIKChain()
		{
		}

		public FBIKChain(float pin, float pull, params Transform[] nodeTransforms)
		{
			this.pin = pin;
			this.pull = pull;
			SetNodes(nodeTransforms);
			children = new int[0];
		}

		public void SetNodes(params Transform[] boneTransforms)
		{
			nodes = new IKSolver.Node[boneTransforms.Length];
			for (int i = 0; i < boneTransforms.Length; i++)
			{
				nodes[i] = new IKSolver.Node(boneTransforms[i]);
			}
		}

		public int GetNodeIndex(Transform boneTransform)
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				if (nodes[i].transform == boneTransform)
				{
					return i;
				}
			}
			return -1;
		}

		public bool IsValid(ref string message)
		{
			if (nodes.Length == 0)
			{
				message = "FBIK chain contains no nodes.";
				return false;
			}
			IKSolver.Node[] array = nodes;
			foreach (IKSolver.Node node in array)
			{
				if (node.transform == null)
				{
					message = "Node transform is null in FBIK chain.";
					return false;
				}
			}
			return true;
		}

		public void Initiate(IKSolverFullBody solver)
		{
			initiated = false;
			IKSolver.Node[] array = nodes;
			foreach (IKSolver.Node node in array)
			{
				node.solverPosition = node.transform.position;
			}
			CalculateBoneLengths(solver);
			ChildConstraint[] array2 = childConstraints;
			foreach (ChildConstraint childConstraint in array2)
			{
				childConstraint.Initiate(solver);
			}
			if (nodes.Length == 3)
			{
				bendConstraint.SetBones(nodes[0].transform, nodes[1].transform, nodes[2].transform);
				bendConstraint.Initiate(solver);
			}
			crossFades = new float[children.Length];
			initiated = true;
		}

		public void ReadPose(IKSolverFullBody solver, bool fullBody)
		{
			if (!initiated)
			{
				return;
			}
			for (int i = 0; i < nodes.Length; i++)
			{
				nodes[i].solverPosition = nodes[i].transform.position + nodes[i].offset;
			}
			CalculateBoneLengths(solver);
			if (!fullBody)
			{
				return;
			}
			for (int j = 0; j < childConstraints.Length; j++)
			{
				childConstraints[j].OnPreSolve(solver);
			}
			if (children.Length > 0)
			{
				float num = nodes[nodes.Length - 1].effectorPositionWeight;
				for (int k = 0; k < children.Length; k++)
				{
					num += solver.chain[children[k]].nodes[0].effectorPositionWeight * solver.chain[children[k]].pull;
				}
				num = Mathf.Clamp(num, 1f, float.PositiveInfinity);
				for (int l = 0; l < children.Length; l++)
				{
					crossFades[l] = solver.chain[children[l]].nodes[0].effectorPositionWeight * solver.chain[children[l]].pull / num;
				}
			}
			pullParentSum = 0f;
			for (int m = 0; m < children.Length; m++)
			{
				pullParentSum += solver.chain[children[m]].pull;
			}
			pullParentSum = Mathf.Clamp(pullParentSum, 1f, float.PositiveInfinity);
			if (nodes.Length == 3)
			{
				reachForce = reach * Mathf.Clamp(nodes[2].effectorPositionWeight, 0f, 1f);
			}
			else
			{
				reachForce = 0f;
			}
			if (push > 0f && nodes.Length > 1)
			{
				distance = Vector3.Distance(nodes[0].transform.position, nodes[nodes.Length - 1].transform.position);
			}
		}

		private void CalculateBoneLengths(IKSolverFullBody solver)
		{
			length = 0f;
			for (int i = 0; i < nodes.Length - 1; i++)
			{
				nodes[i].length = Vector3.Distance(nodes[i].transform.position, nodes[i + 1].transform.position);
				length += nodes[i].length;
				if (nodes[i].length == 0f)
				{
					Warning.Log("Bone " + nodes[i].transform.name + " - " + nodes[i + 1].transform.name + " length is zero, can not solve.", nodes[i].transform);
					return;
				}
			}
			for (int j = 0; j < children.Length; j++)
			{
				solver.chain[children[j]].rootLength = (solver.chain[children[j]].nodes[0].transform.position - nodes[nodes.Length - 1].transform.position).magnitude;
				if (solver.chain[children[j]].rootLength == 0f)
				{
					return;
				}
			}
			if (nodes.Length == 3)
			{
				sqrMag1 = nodes[0].length * nodes[0].length;
				sqrMag2 = nodes[1].length * nodes[1].length;
				sqrMagDif = sqrMag1 - sqrMag2;
			}
		}

		public void Reach(IKSolverFullBody solver)
		{
			if (!initiated)
			{
				return;
			}
			for (int i = 0; i < children.Length; i++)
			{
				solver.chain[children[i]].Reach(solver);
			}
			if (reachForce <= 0f)
			{
				return;
			}
			Vector3 vector = nodes[2].solverPosition - nodes[0].solverPosition;
			if (!(vector == Vector3.zero))
			{
				float magnitude = vector.magnitude;
				Vector3 vector2 = vector / magnitude * length;
				float num = Mathf.Clamp(magnitude / length, 1f - reachForce, 1f + reachForce) - 1f;
				num = Mathf.Clamp(num + reachForce, -1f, 1f);
				switch (reachSmoothing)
				{
				case Smoothing.Exponential:
					num *= num;
					break;
				case Smoothing.Cubic:
					num *= num * num;
					break;
				}
				Vector3 vector3 = vector2 * Mathf.Clamp(num, 0f, magnitude);
				nodes[0].solverPosition += vector3 * (1f - nodes[0].effectorPositionWeight);
				nodes[2].solverPosition += vector3;
			}
		}

		public Vector3 Push(IKSolverFullBody solver)
		{
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < children.Length; i++)
			{
				zero += solver.chain[children[i]].Push(solver) * solver.chain[children[i]].pushParent;
			}
			nodes[nodes.Length - 1].solverPosition += zero;
			if (nodes.Length < 2)
			{
				return Vector3.zero;
			}
			if (push <= 0f)
			{
				return Vector3.zero;
			}
			Vector3 vector = nodes[2].solverPosition - nodes[0].solverPosition;
			float magnitude = vector.magnitude;
			if (magnitude == 0f)
			{
				return Vector3.zero;
			}
			float num = 1f - magnitude / distance;
			if (num <= 0f)
			{
				return Vector3.zero;
			}
			switch (pushSmoothing)
			{
			case Smoothing.Exponential:
				num *= num;
				break;
			case Smoothing.Cubic:
				num *= num * num;
				break;
			}
			Vector3 vector2 = -vector * num * push;
			nodes[0].solverPosition += vector2;
			return vector2;
		}

		public void SolveTrigonometric(IKSolverFullBody solver, bool calculateBendDirection = false)
		{
			if (!initiated)
			{
				return;
			}
			for (int i = 0; i < children.Length; i++)
			{
				solver.chain[children[i]].SolveTrigonometric(solver, calculateBendDirection);
			}
			if (nodes.Length == 3)
			{
				Vector3 vector = nodes[2].solverPosition - nodes[0].solverPosition;
				float magnitude = vector.magnitude;
				if (magnitude != 0f)
				{
					float num = Mathf.Clamp(magnitude, 0f, length * 0.99999f);
					Vector3 direction = vector / magnitude * num;
					Vector3 bendDirection = ((!calculateBendDirection || !bendConstraint.initiated) ? (nodes[1].solverPosition - nodes[0].solverPosition) : bendConstraint.GetDir(solver));
					Vector3 dirToBendPoint = GetDirToBendPoint(direction, bendDirection, num);
					nodes[1].solverPosition = nodes[0].solverPosition + dirToBendPoint;
				}
			}
		}

		public void Stage1(IKSolverFullBody solver)
		{
			for (int i = 0; i < children.Length; i++)
			{
				solver.chain[children[i]].Stage1(solver);
			}
			if (children.Length == 0)
			{
				ForwardReach(nodes[nodes.Length - 1].solverPosition);
				return;
			}
			Vector3 solverPosition = nodes[nodes.Length - 1].solverPosition;
			SolveChildConstraints(solver);
			for (int j = 0; j < children.Length; j++)
			{
				Vector3 vector = solver.chain[children[j]].nodes[0].solverPosition;
				if (solver.chain[children[j]].rootLength > 0f)
				{
					vector = SolveFABRIKJoint(nodes[nodes.Length - 1].solverPosition, solver.chain[children[j]].nodes[0].solverPosition, solver.chain[children[j]].rootLength);
				}
				if (pullParentSum > 0f)
				{
					solverPosition += (vector - nodes[nodes.Length - 1].solverPosition) * (solver.chain[children[j]].pull / pullParentSum);
				}
			}
			ForwardReach(Vector3.Lerp(solverPosition, nodes[nodes.Length - 1].solverPosition, pin));
		}

		public void Stage2(IKSolverFullBody solver, Vector3 position)
		{
			BackwardReach(position);
			int num = Mathf.Clamp(solver.iterations, 2, 4);
			if (childConstraints.Length > 0)
			{
				for (int i = 0; i < num; i++)
				{
					SolveConstraintSystems(solver);
				}
			}
			for (int j = 0; j < children.Length; j++)
			{
				solver.chain[children[j]].Stage2(solver, nodes[nodes.Length - 1].solverPosition);
			}
		}

		public void SolveConstraintSystems(IKSolverFullBody solver)
		{
			SolveChildConstraints(solver);
			for (int i = 0; i < children.Length; i++)
			{
				SolveLinearConstraint(nodes[nodes.Length - 1], solver.chain[children[i]].nodes[0], crossFades[i], solver.chain[children[i]].rootLength);
			}
		}

		private Vector3 SolveFABRIKJoint(Vector3 pos1, Vector3 pos2, float length)
		{
			return pos2 + (pos1 - pos2).normalized * length;
		}

		protected Vector3 GetDirToBendPoint(Vector3 direction, Vector3 bendDirection, float directionMagnitude)
		{
			float num = (directionMagnitude * directionMagnitude + sqrMagDif) / 2f / directionMagnitude;
			float y = (float)Math.Sqrt(Mathf.Clamp(sqrMag1 - num * num, 0f, float.PositiveInfinity));
			if (direction == Vector3.zero)
			{
				return Vector3.zero;
			}
			return Quaternion.LookRotation(direction, bendDirection) * new Vector3(0f, y, num);
		}

		private void SolveChildConstraints(IKSolverFullBody solver)
		{
			for (int i = 0; i < childConstraints.Length; i++)
			{
				childConstraints[i].Solve(solver);
			}
		}

		private void SolveLinearConstraint(IKSolver.Node node1, IKSolver.Node node2, float crossFade, float distance)
		{
			Vector3 vector = node2.solverPosition - node1.solverPosition;
			float magnitude = vector.magnitude;
			if (distance != magnitude && magnitude != 0f)
			{
				Vector3 vector2 = vector * (1f - distance / magnitude);
				node1.solverPosition += vector2 * crossFade;
				node2.solverPosition -= vector2 * (1f - crossFade);
			}
		}

		public void ForwardReach(Vector3 position)
		{
			nodes[nodes.Length - 1].solverPosition = position;
			for (int num = nodes.Length - 2; num > -1; num--)
			{
				nodes[num].solverPosition = SolveFABRIKJoint(nodes[num].solverPosition, nodes[num + 1].solverPosition, nodes[num].length);
			}
		}

		private void BackwardReach(Vector3 position)
		{
			if (rootLength > 0f)
			{
				position = SolveFABRIKJoint(nodes[0].solverPosition, position, rootLength);
			}
			nodes[0].solverPosition = position;
			for (int i = 1; i < nodes.Length; i++)
			{
				nodes[i].solverPosition = SolveFABRIKJoint(nodes[i].solverPosition, nodes[i - 1].solverPosition, nodes[i - 1].length);
			}
		}
	}
}
