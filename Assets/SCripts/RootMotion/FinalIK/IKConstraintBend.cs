using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKConstraintBend
	{
		public Transform bone1;

		public Transform bone2;

		public Transform bone3;

		public Transform bendGoal;

		public Vector3 direction = Vector3.right;

		public Quaternion rotationOffset;

		[Range(0f, 1f)]
		public float weight;

		public Vector3 defaultLocalDirection;

		public Vector3 defaultChildDirection;

		[NonSerialized]
		public float clampF = 0.505f;

		private int chainIndex1;

		private int nodeIndex1;

		private int chainIndex2;

		private int nodeIndex2;

		private int chainIndex3;

		private int nodeIndex3;

		public bool initiated { get; private set; }

		public IKConstraintBend()
		{
		}

		public IKConstraintBend(Transform bone1, Transform bone2, Transform bone3)
		{
			SetBones(bone1, bone2, bone3);
		}

		public bool IsValid(IKSolverFullBody solver, Warning.Logger logger)
		{
			if (bone1 == null || bone2 == null || bone3 == null)
			{
				if (logger != null)
				{
					logger("Bend Constraint contains a null reference.");
				}
				return false;
			}
			if (solver.GetPoint(bone1) == null)
			{
				if (logger != null)
				{
					logger("Bend Constraint is referencing to a bone '" + bone1.name + "' that does not excist in the Node Chain.");
				}
				return false;
			}
			if (solver.GetPoint(bone2) == null)
			{
				if (logger != null)
				{
					logger("Bend Constraint is referencing to a bone '" + bone2.name + "' that does not excist in the Node Chain.");
				}
				return false;
			}
			if (solver.GetPoint(bone3) == null)
			{
				if (logger != null)
				{
					logger("Bend Constraint is referencing to a bone '" + bone3.name + "' that does not excist in the Node Chain.");
				}
				return false;
			}
			return true;
		}

		public void SetBones(Transform bone1, Transform bone2, Transform bone3)
		{
			this.bone1 = bone1;
			this.bone2 = bone2;
			this.bone3 = bone3;
		}

		public void Initiate(IKSolverFullBody solver)
		{
			solver.GetChainAndNodeIndexes(bone1, out chainIndex1, out nodeIndex1);
			solver.GetChainAndNodeIndexes(bone2, out chainIndex2, out nodeIndex2);
			solver.GetChainAndNodeIndexes(bone3, out chainIndex3, out nodeIndex3);
			direction = OrthoToBone1(solver, OrthoToLimb(solver, bone2.position - bone1.position));
			defaultLocalDirection = Quaternion.Inverse(bone1.rotation) * direction;
			Vector3 vector = Vector3.Cross((bone3.position - bone1.position).normalized, direction);
			defaultChildDirection = Quaternion.Inverse(bone3.rotation) * vector;
			initiated = true;
		}

		public void SetLimbOrientation(Vector3 upper, Vector3 lower, Vector3 last)
		{
			if (upper == Vector3.zero)
			{
				Debug.LogError("Attempting to set limb orientation to Vector3.zero axis");
			}
			if (lower == Vector3.zero)
			{
				Debug.LogError("Attempting to set limb orientation to Vector3.zero axis");
			}
			if (last == Vector3.zero)
			{
				Debug.LogError("Attempting to set limb orientation to Vector3.zero axis");
			}
			defaultLocalDirection = upper.normalized;
			defaultChildDirection = last.normalized;
		}

		public void LimitBend(float solverWeight, float positionWeight)
		{
			if (initiated)
			{
				Vector3 vector = bone1.rotation * -defaultLocalDirection;
				Vector3 fromDirection = bone3.position - bone2.position;
				bool changed = false;
				Vector3 toDirection = V3Tools.ClampDirection(fromDirection, vector, clampF * solverWeight, 0, out changed);
				Quaternion rotation = bone3.rotation;
				if (changed)
				{
					Quaternion quaternion = Quaternion.FromToRotation(fromDirection, toDirection);
					bone2.rotation = quaternion * bone2.rotation;
				}
				if (positionWeight > 0f)
				{
					Vector3 normal = bone2.position - bone1.position;
					Vector3 tangent = bone3.position - bone2.position;
					Vector3.OrthoNormalize(ref normal, ref tangent);
					Quaternion quaternion2 = Quaternion.FromToRotation(tangent, vector);
					bone2.rotation = Quaternion.Lerp(bone2.rotation, quaternion2 * bone2.rotation, positionWeight * solverWeight);
				}
				if (changed || positionWeight > 0f)
				{
					bone3.rotation = rotation;
				}
			}
		}

		public Vector3 GetDir(IKSolverFullBody solver)
		{
			if (!initiated)
			{
				return Vector3.zero;
			}
			float num = weight * solver.IKPositionWeight;
			if (bendGoal != null)
			{
				Vector3 vector = bendGoal.position - solver.GetNode(chainIndex1, nodeIndex1).solverPosition;
				if (vector != Vector3.zero)
				{
					direction = vector;
				}
			}
			if (num >= 1f)
			{
				return direction.normalized;
			}
			Vector3 vector2 = solver.GetNode(chainIndex3, nodeIndex3).solverPosition - solver.GetNode(chainIndex1, nodeIndex1).solverPosition;
			Quaternion quaternion = Quaternion.FromToRotation(bone3.position - bone1.position, vector2);
			Vector3 vector3 = quaternion * (bone2.position - bone1.position);
			if (solver.GetNode(chainIndex3, nodeIndex3).effectorRotationWeight > 0f)
			{
				Vector3 b = -Vector3.Cross(vector2, solver.GetNode(chainIndex3, nodeIndex3).solverRotation * defaultChildDirection);
				vector3 = Vector3.Lerp(vector3, b, solver.GetNode(chainIndex3, nodeIndex3).effectorRotationWeight);
			}
			if (rotationOffset != Quaternion.identity)
			{
				Quaternion quaternion2 = Quaternion.FromToRotation(rotationOffset * vector2, vector2);
				vector3 = quaternion2 * rotationOffset * vector3;
			}
			if (num <= 0f)
			{
				return vector3;
			}
			return Vector3.Lerp(vector3, direction.normalized, num);
		}

		private Vector3 OrthoToLimb(IKSolverFullBody solver, Vector3 tangent)
		{
			Vector3 normal = solver.GetNode(chainIndex3, nodeIndex3).solverPosition - solver.GetNode(chainIndex1, nodeIndex1).solverPosition;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			return tangent;
		}

		private Vector3 OrthoToBone1(IKSolverFullBody solver, Vector3 tangent)
		{
			Vector3 normal = solver.GetNode(chainIndex2, nodeIndex2).solverPosition - solver.GetNode(chainIndex1, nodeIndex1).solverPosition;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			return tangent;
		}
	}
}
