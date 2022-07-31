using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKSolverFABRIK : IKSolverHeuristic
	{
		public IterationDelegate OnPreIteration;

		private bool[] limitedBones = new bool[0];

		private Vector3[] solverLocalPositions = new Vector3[0];

		protected override bool boneLengthCanBeZero
		{
			get
			{
				return false;
			}
		}

		public void SolveForward(Vector3 position)
		{
			if (!base.initiated)
			{
				if (!Warning.logged)
				{
					LogWarning("Trying to solve uninitiated FABRIK chain.");
				}
			}
			else
			{
				OnPreSolve();
				ForwardReach(position);
			}
		}

		public void SolveBackward(Vector3 position)
		{
			if (!base.initiated)
			{
				if (!Warning.logged)
				{
					LogWarning("Trying to solve uninitiated FABRIK chain.");
				}
			}
			else
			{
				BackwardReach(position);
				OnPostSolve();
			}
		}

		public override Vector3 GetIKPosition()
		{
			if (target != null)
			{
				return target.position;
			}
			return IKPosition;
		}

		protected override void OnInitiate()
		{
			if (firstInitiation || !Application.isPlaying)
			{
				IKPosition = bones[bones.Length - 1].transform.position;
			}
			for (int i = 0; i < bones.Length; i++)
			{
				bones[i].solverPosition = bones[i].transform.position;
				bones[i].solverRotation = bones[i].transform.rotation;
			}
			limitedBones = new bool[bones.Length];
			solverLocalPositions = new Vector3[bones.Length];
			InitiateBones();
			for (int j = 0; j < bones.Length; j++)
			{
				solverLocalPositions[j] = Quaternion.Inverse(GetParentSolverRotation(j)) * (bones[j].transform.position - GetParentSolverPosition(j));
			}
		}

		protected override void OnUpdate()
		{
			if (IKPositionWeight <= 0f)
			{
				return;
			}
			IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
			OnPreSolve();
			if (target != null)
			{
				IKPosition = target.position;
			}
			if (XY)
			{
				IKPosition.z = bones[0].transform.position.z;
			}
			Vector3 vector = ((maxIterations <= 1) ? Vector3.zero : GetSingularityOffset());
			for (int i = 0; i < maxIterations && (!(vector == Vector3.zero) || i < 1 || !(tolerance > 0f) || !(base.positionOffset < tolerance * tolerance)); i++)
			{
				lastLocalDirection = localDirection;
				if (OnPreIteration != null)
				{
					OnPreIteration(i);
				}
				Solve(IKPosition + ((i != 0) ? Vector3.zero : vector));
			}
			OnPostSolve();
		}

		private Vector3 SolveJoint(Vector3 pos1, Vector3 pos2, float length)
		{
			if (XY)
			{
				pos1.z = pos2.z;
			}
			return pos2 + (pos1 - pos2).normalized * length;
		}

		private void OnPreSolve()
		{
			for (int i = 0; i < bones.Length; i++)
			{
				bones[i].solverPosition = bones[i].transform.position;
				bones[i].solverRotation = bones[i].transform.rotation;
				chainLength = 0f;
				if (i < bones.Length - 1)
				{
					bones[i].length = (bones[i].transform.position - bones[i + 1].transform.position).magnitude;
					bones[i].axis = Quaternion.Inverse(bones[i].transform.rotation) * (bones[i + 1].transform.position - bones[i].transform.position);
					chainLength += bones[i].length;
				}
				if (useRotationLimits)
				{
					solverLocalPositions[i] = Quaternion.Inverse(GetParentSolverRotation(i)) * (bones[i].transform.position - GetParentSolverPosition(i));
				}
			}
		}

		private void OnPostSolve()
		{
			if (!useRotationLimits)
			{
				MapToSolverPositions();
			}
			else
			{
				MapToSolverPositionsLimited();
			}
			lastLocalDirection = localDirection;
		}

		private void Solve(Vector3 targetPosition)
		{
			ForwardReach(targetPosition);
			BackwardReach(bones[0].transform.position);
		}

		private void ForwardReach(Vector3 position)
		{
			bones[bones.Length - 1].solverPosition = Vector3.Lerp(bones[bones.Length - 1].solverPosition, position, IKPositionWeight);
			for (int i = 0; i < limitedBones.Length; i++)
			{
				limitedBones[i] = false;
			}
			for (int num = bones.Length - 2; num > -1; num--)
			{
				bones[num].solverPosition = SolveJoint(bones[num].solverPosition, bones[num + 1].solverPosition, bones[num].length);
				LimitForward(num, num + 1);
			}
			LimitForward(0, 0);
		}

		private void SolverMove(int index, Vector3 offset)
		{
			for (int i = index; i < bones.Length; i++)
			{
				bones[i].solverPosition += offset;
			}
		}

		private void SolverRotate(int index, Quaternion rotation, bool recursive)
		{
			for (int i = index; i < bones.Length; i++)
			{
				bones[i].solverRotation = rotation * bones[i].solverRotation;
				if (!recursive)
				{
					break;
				}
			}
		}

		private void SolverRotateChildren(int index, Quaternion rotation)
		{
			for (int i = index + 1; i < bones.Length; i++)
			{
				bones[i].solverRotation = rotation * bones[i].solverRotation;
			}
		}

		private void SolverMoveChildrenAroundPoint(int index, Quaternion rotation)
		{
			for (int i = index + 1; i < bones.Length; i++)
			{
				Vector3 vector = bones[i].solverPosition - bones[index].solverPosition;
				bones[i].solverPosition = bones[index].solverPosition + rotation * vector;
			}
		}

		private Quaternion GetParentSolverRotation(int index)
		{
			if (index > 0)
			{
				return bones[index - 1].solverRotation;
			}
			if (bones[0].transform.parent == null)
			{
				return Quaternion.identity;
			}
			return bones[0].transform.parent.rotation;
		}

		private Vector3 GetParentSolverPosition(int index)
		{
			if (index > 0)
			{
				return bones[index - 1].solverPosition;
			}
			if (bones[0].transform.parent == null)
			{
				return Vector3.zero;
			}
			return bones[0].transform.parent.position;
		}

		private Quaternion GetLimitedRotation(int index, Quaternion q, out bool changed)
		{
			changed = false;
			Quaternion parentSolverRotation = GetParentSolverRotation(index);
			Quaternion localRotation = Quaternion.Inverse(parentSolverRotation) * q;
			Quaternion limitedLocalRotation = bones[index].rotationLimit.GetLimitedLocalRotation(localRotation, out changed);
			if (!changed)
			{
				return q;
			}
			return parentSolverRotation * limitedLocalRotation;
		}

		private void LimitForward(int rotateBone, int limitBone)
		{
			if (!useRotationLimits || bones[limitBone].rotationLimit == null)
			{
				return;
			}
			Vector3 solverPosition = bones[bones.Length - 1].solverPosition;
			for (int i = rotateBone; i < bones.Length - 1 && !limitedBones[i]; i++)
			{
				Quaternion rotation = Quaternion.FromToRotation(bones[i].solverRotation * bones[i].axis, bones[i + 1].solverPosition - bones[i].solverPosition);
				SolverRotate(i, rotation, false);
			}
			bool changed = false;
			Quaternion limitedRotation = GetLimitedRotation(limitBone, bones[limitBone].solverRotation, out changed);
			if (changed)
			{
				if (limitBone < bones.Length - 1)
				{
					Quaternion rotation2 = QuaTools.FromToRotation(bones[limitBone].solverRotation, limitedRotation);
					bones[limitBone].solverRotation = limitedRotation;
					SolverRotateChildren(limitBone, rotation2);
					SolverMoveChildrenAroundPoint(limitBone, rotation2);
					Quaternion rotation3 = Quaternion.FromToRotation(bones[bones.Length - 1].solverPosition - bones[rotateBone].solverPosition, solverPosition - bones[rotateBone].solverPosition);
					SolverRotate(rotateBone, rotation3, true);
					SolverMoveChildrenAroundPoint(rotateBone, rotation3);
					SolverMove(rotateBone, solverPosition - bones[bones.Length - 1].solverPosition);
				}
				else
				{
					bones[limitBone].solverRotation = limitedRotation;
				}
			}
			limitedBones[limitBone] = true;
		}

		private void BackwardReach(Vector3 position)
		{
			if (useRotationLimits)
			{
				BackwardReachLimited(position);
			}
			else
			{
				BackwardReachUnlimited(position);
			}
		}

		private void BackwardReachUnlimited(Vector3 position)
		{
			bones[0].solverPosition = position;
			for (int i = 1; i < bones.Length; i++)
			{
				bones[i].solverPosition = SolveJoint(bones[i].solverPosition, bones[i - 1].solverPosition, bones[i - 1].length);
			}
		}

		private void BackwardReachLimited(Vector3 position)
		{
			bones[0].solverPosition = position;
			for (int i = 0; i < bones.Length - 1; i++)
			{
				Vector3 vector = SolveJoint(bones[i + 1].solverPosition, bones[i].solverPosition, bones[i].length);
				Quaternion quaternion = Quaternion.FromToRotation(bones[i].solverRotation * bones[i].axis, vector - bones[i].solverPosition);
				Quaternion quaternion2 = quaternion * bones[i].solverRotation;
				if (bones[i].rotationLimit != null)
				{
					bool changed = false;
					quaternion2 = GetLimitedRotation(i, quaternion2, out changed);
				}
				Quaternion rotation = QuaTools.FromToRotation(bones[i].solverRotation, quaternion2);
				bones[i].solverRotation = quaternion2;
				SolverRotateChildren(i, rotation);
				bones[i + 1].solverPosition = bones[i].solverPosition + bones[i].solverRotation * solverLocalPositions[i + 1];
			}
			for (int j = 0; j < bones.Length; j++)
			{
				bones[j].solverRotation = Quaternion.LookRotation(bones[j].solverRotation * Vector3.forward, bones[j].solverRotation * Vector3.up);
			}
		}

		private void MapToSolverPositions()
		{
			bones[0].transform.position = bones[0].solverPosition;
			for (int i = 0; i < bones.Length - 1; i++)
			{
				if (XY)
				{
					bones[i].Swing2D(bones[i + 1].solverPosition);
				}
				else
				{
					bones[i].Swing(bones[i + 1].solverPosition);
				}
			}
		}

		private void MapToSolverPositionsLimited()
		{
			for (int i = 0; i < bones.Length; i++)
			{
				bones[i].transform.position = bones[i].solverPosition;
				if (i < bones.Length - 1)
				{
					bones[i].transform.rotation = bones[i].solverRotation;
				}
			}
		}
	}
}
