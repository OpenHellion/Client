using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKMapping
	{
		[Serializable]
		public class BoneMap
		{
			public Transform transform;

			public int chainIndex = -1;

			public int nodeIndex = -1;

			public Vector3 defaultLocalPosition;

			public Quaternion defaultLocalRotation;

			public Vector3 localSwingAxis;

			public Vector3 localTwistAxis;

			public Vector3 planePosition;

			public Vector3 ikPosition;

			public Quaternion defaultLocalTargetRotation;

			private Quaternion maintainRotation;

			public float length;

			public Quaternion animatedRotation;

			private Transform planeBone1;

			private Transform planeBone2;

			private Transform planeBone3;

			private int plane1ChainIndex = -1;

			private int plane1NodeIndex = -1;

			private int plane2ChainIndex = -1;

			private int plane2NodeIndex = -1;

			private int plane3ChainIndex = -1;

			private int plane3NodeIndex = -1;

			public Vector3 swingDirection
			{
				get { return transform.rotation * localSwingAxis; }
			}

			public bool isNodeBone
			{
				get { return nodeIndex != -1; }
			}

			private Quaternion lastAnimatedTargetRotation
			{
				get
				{
					if (planeBone1.position == planeBone3.position)
					{
						return Quaternion.identity;
					}

					return Quaternion.LookRotation(planeBone2.position - planeBone1.position,
						planeBone3.position - planeBone1.position);
				}
			}

			public void Initiate(Transform transform, IKSolverFullBody solver)
			{
				this.transform = transform;
				solver.GetChainAndNodeIndexes(transform, out chainIndex, out nodeIndex);
			}

			public void StoreDefaultLocalState()
			{
				defaultLocalPosition = transform.localPosition;
				defaultLocalRotation = transform.localRotation;
			}

			public void FixTransform(bool position)
			{
				if (position)
				{
					transform.localPosition = defaultLocalPosition;
				}

				transform.localRotation = defaultLocalRotation;
			}

			public void SetLength(BoneMap nextBone)
			{
				length = Vector3.Distance(transform.position, nextBone.transform.position);
			}

			public void SetLocalSwingAxis(BoneMap swingTarget)
			{
				SetLocalSwingAxis(swingTarget, this);
			}

			public void SetLocalSwingAxis(BoneMap bone1, BoneMap bone2)
			{
				localSwingAxis = Quaternion.Inverse(transform.rotation) *
				                 (bone1.transform.position - bone2.transform.position);
			}

			public void SetLocalTwistAxis(Vector3 twistDirection, Vector3 normalDirection)
			{
				Vector3.OrthoNormalize(ref normalDirection, ref twistDirection);
				localTwistAxis = Quaternion.Inverse(transform.rotation) * twistDirection;
			}

			public void SetPlane(IKSolverFullBody solver, Transform planeBone1, Transform planeBone2,
				Transform planeBone3)
			{
				this.planeBone1 = planeBone1;
				this.planeBone2 = planeBone2;
				this.planeBone3 = planeBone3;
				solver.GetChainAndNodeIndexes(planeBone1, out plane1ChainIndex, out plane1NodeIndex);
				solver.GetChainAndNodeIndexes(planeBone2, out plane2ChainIndex, out plane2NodeIndex);
				solver.GetChainAndNodeIndexes(planeBone3, out plane3ChainIndex, out plane3NodeIndex);
				UpdatePlane(true, true);
			}

			public void UpdatePlane(bool rotation, bool position)
			{
				Quaternion rotation2 = lastAnimatedTargetRotation;
				if (rotation)
				{
					defaultLocalTargetRotation = QuaTools.RotationToLocalSpace(transform.rotation, rotation2);
				}

				if (position)
				{
					planePosition = Quaternion.Inverse(rotation2) * (transform.position - planeBone1.position);
				}
			}

			public void SetIKPosition()
			{
				ikPosition = transform.position;
			}

			public void MaintainRotation()
			{
				maintainRotation = transform.rotation;
			}

			public void SetToIKPosition()
			{
				transform.position = ikPosition;
			}

			public void FixToNode(IKSolverFullBody solver, float weight, IKSolver.Node fixNode = null)
			{
				if (fixNode == null)
				{
					fixNode = solver.GetNode(chainIndex, nodeIndex);
				}

				if (weight >= 1f)
				{
					transform.position = fixNode.solverPosition;
				}
				else
				{
					transform.position = Vector3.Lerp(transform.position, fixNode.solverPosition, weight);
				}
			}

			public Vector3 GetPlanePosition(IKSolverFullBody solver)
			{
				return solver.GetNode(plane1ChainIndex, plane1NodeIndex).solverPosition +
				       GetTargetRotation(solver) * planePosition;
			}

			public void PositionToPlane(IKSolverFullBody solver)
			{
				transform.position = GetPlanePosition(solver);
			}

			public void RotateToPlane(IKSolverFullBody solver, float weight)
			{
				Quaternion quaternion = GetTargetRotation(solver) * defaultLocalTargetRotation;
				if (weight >= 1f)
				{
					transform.rotation = quaternion;
				}
				else
				{
					transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, weight);
				}
			}

			public void Swing(Vector3 swingTarget, float weight)
			{
				Swing(swingTarget, transform.position, weight);
			}

			public void Swing(Vector3 pos1, Vector3 pos2, float weight)
			{
				Quaternion quaternion = Quaternion.FromToRotation(transform.rotation * localSwingAxis, pos1 - pos2) *
				                        transform.rotation;
				if (weight >= 1f)
				{
					transform.rotation = quaternion;
				}
				else
				{
					transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, weight);
				}
			}

			public void Twist(Vector3 twistDirection, Vector3 normalDirection, float weight)
			{
				Vector3.OrthoNormalize(ref normalDirection, ref twistDirection);
				Quaternion quaternion = Quaternion.FromToRotation(transform.rotation * localTwistAxis, twistDirection) *
				                        transform.rotation;
				if (weight >= 1f)
				{
					transform.rotation = quaternion;
				}
				else
				{
					transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, weight);
				}
			}

			public void RotateToMaintain(float weight)
			{
				if (!(weight <= 0f))
				{
					transform.rotation = Quaternion.Lerp(transform.rotation, maintainRotation, weight);
				}
			}

			public void RotateToEffector(IKSolverFullBody solver, float weight)
			{
				if (!isNodeBone)
				{
					return;
				}

				float num = weight * solver.GetNode(chainIndex, nodeIndex).effectorRotationWeight;
				if (!(num <= 0f))
				{
					if (num >= 1f)
					{
						transform.rotation = solver.GetNode(chainIndex, nodeIndex).solverRotation;
					}
					else
					{
						transform.rotation = Quaternion.Lerp(transform.rotation,
							solver.GetNode(chainIndex, nodeIndex).solverRotation, num);
					}
				}
			}

			private Quaternion GetTargetRotation(IKSolverFullBody solver)
			{
				Vector3 solverPosition = solver.GetNode(plane1ChainIndex, plane1NodeIndex).solverPosition;
				Vector3 solverPosition2 = solver.GetNode(plane2ChainIndex, plane2NodeIndex).solverPosition;
				Vector3 solverPosition3 = solver.GetNode(plane3ChainIndex, plane3NodeIndex).solverPosition;
				if (solverPosition == solverPosition3)
				{
					return Quaternion.identity;
				}

				return Quaternion.LookRotation(solverPosition2 - solverPosition, solverPosition3 - solverPosition);
			}
		}

		public virtual bool IsValid(IKSolver solver, ref string message)
		{
			return true;
		}

		public virtual void Initiate(IKSolverFullBody solver)
		{
		}

		protected bool BoneIsValid(Transform bone, IKSolver solver, ref string message, Warning.Logger logger = null)
		{
			if (bone == null)
			{
				message = "IKMappingLimb contains a null reference.";
				if (logger != null)
				{
					logger(message);
				}

				return false;
			}

			if (solver.GetPoint(bone) == null)
			{
				message = "IKMappingLimb is referencing to a bone '" + bone.name +
				          "' that does not excist in the Node Chain.";
				if (logger != null)
				{
					logger(message);
				}

				return false;
			}

			return true;
		}

		protected Vector3 SolveFABRIKJoint(Vector3 pos1, Vector3 pos2, float length)
		{
			return pos2 + (pos1 - pos2).normalized * length;
		}
	}
}
