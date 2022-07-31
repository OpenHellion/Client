using System;
using UnityEngine;

namespace ZeroGravity.LevelDesign
{
	[RequireComponent(typeof(Animator))]
	public class SceneTriggerAnimation : MonoBehaviour
	{
		public delegate void OnStateEnterExitDelegate(SceneTriggerAnimation anim, AnimationState state);

		[Serializable]
		public class ActionEffects
		{
			public AnimationClip ExecutingAnimation;

			public AnimationClip IdleAnimation;

			public AnimationClip FailAnimation;
		}

		public enum AnimationState
		{
			None = 0,
			Active = 1,
			ActiveIdle = 2,
			Inactive = 3,
			InactiveIdle = 4,
			Fail = 5
		}

		public OnStateEnterExitDelegate OnStateEnter;

		public OnStateEnterExitDelegate OnStateBeforeEnter;

		public OnStateEnterExitDelegate OnStateExit;

		public OnStateEnterExitDelegate OnStateAfterExit;

		[SerializeField]
		private ActionEffects activeEffects;

		[SerializeField]
		private ActionEffects inactiveEffects;

		[SerializeField]
		private Animator anim;

		private AnimationState currentState = AnimationState.InactiveIdle;

		public bool IsEventRunning
		{
			get
			{
				return currentState == AnimationState.Active || currentState == AnimationState.Inactive || currentState == AnimationState.Fail;
			}
		}

		public bool IsEventFinished
		{
			get
			{
				return currentState == AnimationState.ActiveIdle || currentState == AnimationState.InactiveIdle || currentState == AnimationState.None;
			}
		}

		public bool IsActive
		{
			get
			{
				return currentState == AnimationState.Active || currentState == AnimationState.ActiveIdle;
			}
		}

		public bool IsInactive
		{
			get
			{
				return currentState == AnimationState.Inactive || currentState == AnimationState.InactiveIdle;
			}
		}

		private void Awake()
		{
			if (anim == null)
			{
				anim = GetComponent<Animator>();
			}
			AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController();
			animatorOverrideController.runtimeAnimatorController = anim.runtimeAnimatorController;
			if (animatorOverrideController.runtimeAnimatorController == null)
			{
				Dbg.Error("Cannot override controller", base.name);
			}
			if (activeEffects.ExecutingAnimation != null)
			{
				animatorOverrideController["Active"] = activeEffects.ExecutingAnimation;
			}
			if (activeEffects.IdleAnimation != null)
			{
				animatorOverrideController["ActiveIdle"] = activeEffects.IdleAnimation;
			}
			if (activeEffects.FailAnimation != null)
			{
				animatorOverrideController["ActiveFail"] = activeEffects.FailAnimation;
			}
			if (inactiveEffects.ExecutingAnimation != null)
			{
				animatorOverrideController["Inactive"] = inactiveEffects.ExecutingAnimation;
			}
			if (inactiveEffects.IdleAnimation != null)
			{
				animatorOverrideController["InactiveIdle"] = inactiveEffects.IdleAnimation;
			}
			if (inactiveEffects.FailAnimation != null)
			{
				animatorOverrideController["InactiveFail"] = inactiveEffects.FailAnimation;
			}
			anim.runtimeAnimatorController = animatorOverrideController;
		}

		public void ChangeState(bool newState, bool instant)
		{
			try
			{
				if (instant)
				{
					anim.Play((!newState) ? "InactiveIdle" : "ActiveIdle");
				}
				anim.SetFloat("isActive", Convert.ToSingle(newState));
			}
			catch (Exception ex)
			{
				Dbg.Error(ex, ex.StackTrace, base.gameObject.name);
			}
		}

		public void ChangeState(bool newState)
		{
			ChangeState(newState, false);
		}

		public void ChangeStateInstant(bool newState)
		{
			ChangeState(newState, true);
		}

		public void CancelState(bool oldState)
		{
			anim.SetTrigger("CancelInteraction");
			anim.SetFloat("isActive", Convert.ToSingle(oldState));
		}

		public void PlayFail()
		{
			anim.Play("Fail");
		}

		public bool HasFailAnimation(bool isActive)
		{
			if (isActive)
			{
				return activeEffects.FailAnimation != null;
			}
			return inactiveEffects.FailAnimation != null;
		}

		public void OnAnimatorStateEnter(AnimationState state)
		{
			currentState = state;
			if (OnStateBeforeEnter != null)
			{
				OnStateBeforeEnter(this, state);
			}
			if (OnStateEnter != null)
			{
				OnStateEnter(this, state);
			}
		}

		public void OnAnimatorStateExit(AnimationState state)
		{
			if (OnStateExit != null)
			{
				OnStateExit(this, state);
			}
			if (OnStateAfterExit != null)
			{
				OnStateAfterExit(this, state);
			}
		}

		public float GetActiveExecutingDuration()
		{
			if (activeEffects.ExecutingAnimation != null)
			{
				return activeEffects.ExecutingAnimation.length;
			}
			return 0f;
		}

		public float GetInactiveExecutingDuration()
		{
			if (inactiveEffects.ExecutingAnimation != null)
			{
				return inactiveEffects.ExecutingAnimation.length;
			}
			return 0f;
		}
	}
}
