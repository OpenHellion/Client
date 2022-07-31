using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class AnimationWarping : OffsetModifier
	{
		[Serializable]
		public struct Warp
		{
			[Tooltip("Layer of the 'Animation State' in the Animator.")]
			public int animationLayer;

			[Tooltip("Name of the state in the Animator to warp.")]
			public string animationState;

			[Tooltip("Warping weight by normalized time of the animation state.")]
			public AnimationCurve weightCurve;

			[Tooltip("Animated point to warp from. This should be in character space so keep this Transform parented to the root of the character.")]
			public Transform warpFrom;

			[Tooltip("World space point to warp to.")]
			public Transform warpTo;

			[Tooltip("Which FBBIK effector to use?")]
			public FullBodyBipedEffector effector;
		}

		[Serializable]
		public enum EffectorMode
		{
			PositionOffset = 0,
			Position = 1
		}

		[Tooltip("Reference to the Animator component to use")]
		public Animator animator;

		[Tooltip("Using effector.positionOffset or effector.position with effector.positionWeight? The former will enable you to use effector.position for other things, the latter will weigh in the effectors, hence using Reach and Pull in the process.")]
		public EffectorMode effectorMode;

		[Space(10f)]
		[Tooltip("The array of warps, can have multiple simultaneous warps.")]
		public Warp[] warps;

		private EffectorMode lastMode;

		protected override void Start()
		{
			base.Start();
			lastMode = effectorMode;
		}

		public float GetWarpWeight(int warpIndex)
		{
			if (warpIndex < 0)
			{
				Debug.LogError("Warp index out of range.");
				return 0f;
			}
			if (warpIndex >= warps.Length)
			{
				Debug.LogError("Warp index out of range.");
				return 0f;
			}
			if (animator == null)
			{
				Debug.LogError("Animator unassigned in AnimationWarping");
				return 0f;
			}
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(warps[warpIndex].animationLayer);
			if (!currentAnimatorStateInfo.IsName(warps[warpIndex].animationState))
			{
				return 0f;
			}
			return warps[warpIndex].weightCurve.Evaluate(currentAnimatorStateInfo.normalizedTime - (float)(int)currentAnimatorStateInfo.normalizedTime);
		}

		protected override void OnModifyOffset()
		{
			for (int i = 0; i < warps.Length; i++)
			{
				float warpWeight = GetWarpWeight(i);
				Vector3 vector = warps[i].warpTo.position - warps[i].warpFrom.position;
				switch (effectorMode)
				{
				case EffectorMode.PositionOffset:
					ik.solver.GetEffector(warps[i].effector).positionOffset += vector * warpWeight * weight;
					break;
				case EffectorMode.Position:
					ik.solver.GetEffector(warps[i].effector).position = ik.solver.GetEffector(warps[i].effector).bone.position + vector;
					ik.solver.GetEffector(warps[i].effector).positionWeight = weight * warpWeight;
					break;
				}
			}
			if (lastMode == EffectorMode.Position && effectorMode == EffectorMode.PositionOffset)
			{
				Warp[] array = warps;
				for (int j = 0; j < array.Length; j++)
				{
					Warp warp = array[j];
					ik.solver.GetEffector(warp.effector).positionWeight = 0f;
				}
			}
			lastMode = effectorMode;
		}

		private void OnDisable()
		{
			if (effectorMode == EffectorMode.Position)
			{
				Warp[] array = warps;
				for (int i = 0; i < array.Length; i++)
				{
					Warp warp = array[i];
					ik.solver.GetEffector(warp.effector).positionWeight = 0f;
				}
			}
		}
	}
}
