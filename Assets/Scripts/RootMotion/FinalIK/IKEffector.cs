using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKEffector
	{
		public Transform bone;

		public Transform target;

		[Range(0f, 1f)] public float positionWeight;

		[Range(0f, 1f)] public float rotationWeight;

		public Vector3 position = Vector3.zero;

		public Quaternion rotation = Quaternion.identity;

		public Vector3 positionOffset;

		public bool effectChildNodes = true;

		[Range(0f, 1f)] public float maintainRelativePositionWeight;

		public Transform[] childBones = new Transform[0];

		public Transform planeBone1;

		public Transform planeBone2;

		public Transform planeBone3;

		public Quaternion planeRotationOffset = Quaternion.identity;

		private float posW;

		private float rotW;

		private Vector3[] localPositions = new Vector3[0];

		private bool usePlaneNodes;

		private Quaternion animatedPlaneRotation = Quaternion.identity;

		private Vector3 animatedPosition;

		private bool firstUpdate;

		private int chainIndex = -1;

		private int nodeIndex = -1;

		private int plane1ChainIndex;

		private int plane1NodeIndex = -1;

		private int plane2ChainIndex = -1;

		private int plane2NodeIndex = -1;

		private int plane3ChainIndex = -1;

		private int plane3NodeIndex = -1;

		private int[] childChainIndexes = new int[0];

		private int[] childNodeIndexes = new int[0];

		public bool isEndEffector { get; private set; }

		public IKEffector()
		{
		}

		public IKEffector(Transform bone, Transform[] childBones)
		{
			this.bone = bone;
			this.childBones = childBones;
		}

		public IKSolver.Node GetNode(IKSolverFullBody solver)
		{
			return solver.chain[chainIndex].nodes[nodeIndex];
		}

		public void PinToBone(float positionWeight, float rotationWeight)
		{
			position = bone.position;
			this.positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);
			rotation = bone.rotation;
			this.rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
		}

		public bool IsValid(IKSolver solver, ref string message)
		{
			if (bone == null)
			{
				message = "IK Effector bone is null.";
				return false;
			}

			if (solver.GetPoint(bone) == null)
			{
				message = "IK Effector is referencing to a bone '" + bone.name +
				          "' that does not excist in the Node Chain.";
				return false;
			}

			Transform[] array = childBones;
			foreach (Transform transform in array)
			{
				if (transform == null)
				{
					message = "IK Effector contains a null reference.";
					return false;
				}
			}

			Transform[] array2 = childBones;
			foreach (Transform transform2 in array2)
			{
				if (solver.GetPoint(transform2) == null)
				{
					message = "IK Effector is referencing to a bone '" + transform2.name +
					          "' that does not excist in the Node Chain.";
					return false;
				}
			}

			if (planeBone1 != null && solver.GetPoint(planeBone1) == null)
			{
				message = "IK Effector is referencing to a bone '" + planeBone1.name +
				          "' that does not excist in the Node Chain.";
				return false;
			}

			if (planeBone2 != null && solver.GetPoint(planeBone2) == null)
			{
				message = "IK Effector is referencing to a bone '" + planeBone2.name +
				          "' that does not excist in the Node Chain.";
				return false;
			}

			if (planeBone3 != null && solver.GetPoint(planeBone3) == null)
			{
				message = "IK Effector is referencing to a bone '" + planeBone3.name +
				          "' that does not excist in the Node Chain.";
				return false;
			}

			return true;
		}

		public void Initiate(IKSolverFullBody solver)
		{
			position = bone.position;
			rotation = bone.rotation;
			animatedPlaneRotation = Quaternion.identity;
			solver.GetChainAndNodeIndexes(bone, out chainIndex, out nodeIndex);
			childChainIndexes = new int[childBones.Length];
			childNodeIndexes = new int[childBones.Length];
			for (int i = 0; i < childBones.Length; i++)
			{
				solver.GetChainAndNodeIndexes(childBones[i], out childChainIndexes[i], out childNodeIndexes[i]);
			}

			localPositions = new Vector3[childBones.Length];
			usePlaneNodes = false;
			if (planeBone1 != null)
			{
				solver.GetChainAndNodeIndexes(planeBone1, out plane1ChainIndex, out plane1NodeIndex);
				if (planeBone2 != null)
				{
					solver.GetChainAndNodeIndexes(planeBone2, out plane2ChainIndex, out plane2NodeIndex);
					if (planeBone3 != null)
					{
						solver.GetChainAndNodeIndexes(planeBone3, out plane3ChainIndex, out plane3NodeIndex);
						usePlaneNodes = true;
					}
				}

				isEndEffector = true;
			}
			else
			{
				isEndEffector = false;
			}
		}

		public void ResetOffset(IKSolverFullBody solver)
		{
			solver.GetNode(chainIndex, nodeIndex).offset = Vector3.zero;
			for (int i = 0; i < childChainIndexes.Length; i++)
			{
				solver.GetNode(childChainIndexes[i], childNodeIndexes[i]).offset = Vector3.zero;
			}
		}

		public void SetToTarget()
		{
			if (!(target == null))
			{
				position = target.position;
				rotation = target.rotation;
			}
		}

		public void OnPreSolve(IKSolverFullBody solver)
		{
			positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);
			rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
			maintainRelativePositionWeight = Mathf.Clamp(maintainRelativePositionWeight, 0f, 1f);
			posW = positionWeight * solver.IKPositionWeight;
			rotW = rotationWeight * solver.IKPositionWeight;
			solver.GetNode(chainIndex, nodeIndex).effectorPositionWeight = posW;
			solver.GetNode(chainIndex, nodeIndex).effectorRotationWeight = rotW;
			solver.GetNode(chainIndex, nodeIndex).solverRotation = rotation;
			if (float.IsInfinity(positionOffset.x) || float.IsInfinity(positionOffset.y) ||
			    float.IsInfinity(positionOffset.z))
			{
				Debug.LogError(
					"Invalid IKEffector.positionOffset (contains Infinity)! Please make sure not to set IKEffector.positionOffset to infinite values.",
					bone);
			}

			if (float.IsNaN(positionOffset.x) || float.IsNaN(positionOffset.y) || float.IsNaN(positionOffset.z))
			{
				Debug.LogError(
					"Invalid IKEffector.positionOffset (contains NaN)! Please make sure not to set IKEffector.positionOffset to NaN values.",
					bone);
			}

			if (positionOffset.sqrMagnitude > 1E+10f)
			{
				Debug.LogError(
					"Additive effector positionOffset detected in Full Body IK (extremely large value). Make sure you are not circularily adding to effector positionOffset each frame.",
					bone);
			}

			if (float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z))
			{
				Debug.LogError("Invalid IKEffector.position (contains Infinity)!");
			}

			solver.GetNode(chainIndex, nodeIndex).offset += positionOffset * solver.IKPositionWeight;
			if (effectChildNodes && solver.iterations > 0)
			{
				for (int i = 0; i < childBones.Length; i++)
				{
					localPositions[i] = childBones[i].transform.position - bone.transform.position;
					solver.GetNode(childChainIndexes[i], childNodeIndexes[i]).offset +=
						positionOffset * solver.IKPositionWeight;
				}
			}

			if (usePlaneNodes && maintainRelativePositionWeight > 0f)
			{
				animatedPlaneRotation = Quaternion.LookRotation(planeBone2.position - planeBone1.position,
					planeBone3.position - planeBone1.position);
			}

			firstUpdate = true;
		}

		public void OnPostWrite()
		{
			positionOffset = Vector3.zero;
		}

		private Quaternion GetPlaneRotation(IKSolverFullBody solver)
		{
			Vector3 solverPosition = solver.GetNode(plane1ChainIndex, plane1NodeIndex).solverPosition;
			Vector3 solverPosition2 = solver.GetNode(plane2ChainIndex, plane2NodeIndex).solverPosition;
			Vector3 solverPosition3 = solver.GetNode(plane3ChainIndex, plane3NodeIndex).solverPosition;
			Vector3 vector = solverPosition2 - solverPosition;
			Vector3 upwards = solverPosition3 - solverPosition;
			if (vector == Vector3.zero)
			{
				Warning.Log(
					"Make sure you are not placing 2 or more FBBIK effectors of the same chain to exactly the same position.",
					bone);
				return Quaternion.identity;
			}

			return Quaternion.LookRotation(vector, upwards);
		}

		public void Update(IKSolverFullBody solver)
		{
			if (firstUpdate)
			{
				animatedPosition = bone.position + solver.GetNode(chainIndex, nodeIndex).offset;
				firstUpdate = false;
			}

			solver.GetNode(chainIndex, nodeIndex).solverPosition =
				Vector3.Lerp(GetPosition(solver, out planeRotationOffset), position, posW);
			if (effectChildNodes)
			{
				for (int i = 0; i < childBones.Length; i++)
				{
					solver.GetNode(childChainIndexes[i], childNodeIndexes[i]).solverPosition = Vector3.Lerp(
						solver.GetNode(childChainIndexes[i], childNodeIndexes[i]).solverPosition,
						solver.GetNode(chainIndex, nodeIndex).solverPosition + localPositions[i], posW);
				}
			}
		}

		private Vector3 GetPosition(IKSolverFullBody solver, out Quaternion planeRotationOffset)
		{
			planeRotationOffset = Quaternion.identity;
			if (!isEndEffector)
			{
				return solver.GetNode(chainIndex, nodeIndex).solverPosition;
			}

			if (maintainRelativePositionWeight <= 0f)
			{
				return animatedPosition;
			}

			Vector3 vector = bone.position;
			Vector3 vector2 = vector - planeBone1.position;
			planeRotationOffset = GetPlaneRotation(solver) * Quaternion.Inverse(animatedPlaneRotation);
			vector = solver.GetNode(plane1ChainIndex, plane1NodeIndex).solverPosition + planeRotationOffset * vector2;
			planeRotationOffset =
				Quaternion.Lerp(Quaternion.identity, planeRotationOffset, maintainRelativePositionWeight);
			return Vector3.Lerp(animatedPosition, vector + solver.GetNode(chainIndex, nodeIndex).offset,
				maintainRelativePositionWeight);
		}
	}
}
