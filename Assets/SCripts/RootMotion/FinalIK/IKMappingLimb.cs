using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKMappingLimb : IKMapping
	{
		[Serializable]
		public enum BoneMapType
		{
			Parent = 0,
			Bone1 = 1,
			Bone2 = 2,
			Bone3 = 3
		}

		public Transform parentBone;

		public Transform bone1;

		public Transform bone2;

		public Transform bone3;

		[Range(0f, 1f)]
		public float maintainRotationWeight;

		[Range(0f, 1f)]
		public float weight = 1f;

		private BoneMap boneMapParent = new BoneMap();

		private BoneMap boneMap1 = new BoneMap();

		private BoneMap boneMap2 = new BoneMap();

		private BoneMap boneMap3 = new BoneMap();

		public IKMappingLimb()
		{
		}

		public IKMappingLimb(Transform bone1, Transform bone2, Transform bone3, Transform parentBone = null)
		{
			SetBones(bone1, bone2, bone3, parentBone);
		}

		public override bool IsValid(IKSolver solver, ref string message)
		{
			if (!base.IsValid(solver, ref message))
			{
				return false;
			}
			if (!BoneIsValid(bone1, solver, ref message))
			{
				return false;
			}
			if (!BoneIsValid(bone2, solver, ref message))
			{
				return false;
			}
			if (!BoneIsValid(bone3, solver, ref message))
			{
				return false;
			}
			return true;
		}

		public BoneMap GetBoneMap(BoneMapType boneMap)
		{
			switch (boneMap)
			{
			case BoneMapType.Parent:
				if (parentBone == null)
				{
					Warning.Log("This limb does not have a parent (shoulder) bone", bone1);
				}
				return boneMapParent;
			case BoneMapType.Bone1:
				return boneMap1;
			case BoneMapType.Bone2:
				return boneMap2;
			default:
				return boneMap3;
			}
		}

		public void SetLimbOrientation(Vector3 upper, Vector3 lower)
		{
			boneMap1.defaultLocalTargetRotation = Quaternion.Inverse(Quaternion.Inverse(bone1.rotation) * Quaternion.LookRotation(bone2.position - bone1.position, bone1.rotation * -upper));
			boneMap2.defaultLocalTargetRotation = Quaternion.Inverse(Quaternion.Inverse(bone2.rotation) * Quaternion.LookRotation(bone3.position - bone2.position, bone2.rotation * -lower));
		}

		public void SetBones(Transform bone1, Transform bone2, Transform bone3, Transform parentBone = null)
		{
			this.bone1 = bone1;
			this.bone2 = bone2;
			this.bone3 = bone3;
			this.parentBone = parentBone;
		}

		public void StoreDefaultLocalState()
		{
			if (parentBone != null)
			{
				boneMapParent.StoreDefaultLocalState();
			}
			boneMap1.StoreDefaultLocalState();
			boneMap2.StoreDefaultLocalState();
			boneMap3.StoreDefaultLocalState();
		}

		public void FixTransforms()
		{
			if (parentBone != null)
			{
				boneMapParent.FixTransform(false);
			}
			boneMap1.FixTransform(true);
			boneMap2.FixTransform(false);
			boneMap3.FixTransform(false);
		}

		public override void Initiate(IKSolverFullBody solver)
		{
			if (boneMapParent == null)
			{
				boneMapParent = new BoneMap();
			}
			if (boneMap1 == null)
			{
				boneMap1 = new BoneMap();
			}
			if (boneMap2 == null)
			{
				boneMap2 = new BoneMap();
			}
			if (boneMap3 == null)
			{
				boneMap3 = new BoneMap();
			}
			if (parentBone != null)
			{
				boneMapParent.Initiate(parentBone, solver);
			}
			boneMap1.Initiate(bone1, solver);
			boneMap2.Initiate(bone2, solver);
			boneMap3.Initiate(bone3, solver);
			boneMap1.SetPlane(solver, boneMap1.transform, boneMap2.transform, boneMap3.transform);
			boneMap2.SetPlane(solver, boneMap2.transform, boneMap3.transform, boneMap1.transform);
			if (parentBone != null)
			{
				boneMapParent.SetLocalSwingAxis(boneMap1);
			}
		}

		public void ReadPose()
		{
			boneMap1.UpdatePlane(true, true);
			boneMap2.UpdatePlane(true, false);
			weight = Mathf.Clamp(weight, 0f, 1f);
			boneMap3.MaintainRotation();
		}

		public void WritePose(IKSolverFullBody solver, bool fullBody)
		{
			if (!(weight <= 0f))
			{
				if (fullBody && parentBone != null)
				{
					boneMapParent.Swing(solver.GetNode(boneMap1.chainIndex, boneMap1.nodeIndex).solverPosition, weight);
				}
				boneMap1.RotateToPlane(solver, weight);
				boneMap2.RotateToPlane(solver, weight);
				boneMap3.RotateToMaintain(maintainRotationWeight * weight * solver.IKPositionWeight);
				boneMap3.RotateToEffector(solver, weight);
			}
		}
	}
}
