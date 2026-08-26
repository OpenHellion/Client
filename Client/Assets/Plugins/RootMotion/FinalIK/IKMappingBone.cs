using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKMappingBone : IKMapping
	{
		public Transform bone;

		[Range(0f, 1f)] public float maintainRotationWeight = 1f;

		private BoneMap boneMap = new BoneMap();

		public IKMappingBone()
		{
		}

		public IKMappingBone(Transform bone)
		{
			this.bone = bone;
		}

		public override bool IsValid(IKSolver solver, ref string message)
		{
			if (!base.IsValid(solver, ref message))
			{
				return false;
			}

			if (bone == null)
			{
				message = "IKMappingBone's bone is null.";
				return false;
			}

			return true;
		}

		public void StoreDefaultLocalState()
		{
			boneMap.StoreDefaultLocalState();
		}

		public void FixTransforms()
		{
			boneMap.FixTransform(false);
		}

		public override void Initiate(IKSolverFullBody solver)
		{
			if (boneMap == null)
			{
				boneMap = new BoneMap();
			}

			boneMap.Initiate(bone, solver);
		}

		public void ReadPose()
		{
			boneMap.MaintainRotation();
		}

		public void WritePose(float solverWeight)
		{
			boneMap.RotateToMaintain(solverWeight * maintainRotationWeight);
		}
	}
}
