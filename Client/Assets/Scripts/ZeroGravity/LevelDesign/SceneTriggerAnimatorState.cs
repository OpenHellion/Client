using UnityEngine;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerAnimatorState : StateMachineBehaviour
	{
		private SceneTriggerAnimation.AnimationState GetAnimationState(AnimatorStateInfo stateInfo)
		{
			if (stateInfo.IsName(SceneTriggerAnimation.AnimationState.Active.ToString()))
			{
				return SceneTriggerAnimation.AnimationState.Active;
			}

			if (stateInfo.IsName(SceneTriggerAnimation.AnimationState.Inactive.ToString()))
			{
				return SceneTriggerAnimation.AnimationState.Inactive;
			}

			if (stateInfo.IsName(SceneTriggerAnimation.AnimationState.Fail.ToString()))
			{
				return SceneTriggerAnimation.AnimationState.Fail;
			}

			if (stateInfo.IsName(SceneTriggerAnimation.AnimationState.ActiveIdle.ToString()))
			{
				return SceneTriggerAnimation.AnimationState.ActiveIdle;
			}

			if (stateInfo.IsName(SceneTriggerAnimation.AnimationState.InactiveIdle.ToString()))
			{
				return SceneTriggerAnimation.AnimationState.InactiveIdle;
			}

			return SceneTriggerAnimation.AnimationState.None;
		}

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			SceneTriggerAnimation component = animator.gameObject.GetComponent<SceneTriggerAnimation>();
			if (component != null)
			{
				component.OnAnimatorStateEnter(GetAnimationState(stateInfo));
			}
		}

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			SceneTriggerAnimation component = animator.gameObject.GetComponent<SceneTriggerAnimation>();
			if (component != null)
			{
				component.OnAnimatorStateExit(GetAnimationState(stateInfo));
			}
		}
	}
}
