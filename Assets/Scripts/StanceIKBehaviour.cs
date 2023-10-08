using UnityEngine;

public class StanceIKBehaviour : StateMachineBehaviour
{
	private bool inTransition;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		inTransition = true;
		AimIKController component = animator.GetComponent<AimIKController>();
		if (stateInfo.IsTag("None") && component.IKEnabled())
		{
			component.ToggleIK(false, true);
			inTransition = false;
		}

		if (stateInfo.IsTag("None") && !animator.GetBool("InStance"))
		{
			animator.SetInteger("BaseDisabled", 0);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (inTransition)
		{
			AimIKController component = animator.GetComponent<AimIKController>();
			if (!stateInfo.IsTag("None"))
			{
				component.ToggleIK((!stateInfo.IsTag("Passive")) ? true : false);
			}

			inTransition = false;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		AimIKController component = animator.GetComponent<AimIKController>();
		if (animator.GetNextAnimatorStateInfo(layerIndex).IsTag("Passive") ||
		    (animator.GetNextAnimatorStateInfo(layerIndex).IsTag("Active") &&
		     animator.GetNextAnimatorStateInfo(layerIndex).IsTag("Special")))
		{
			component.ToggleIK(false);
		}
	}
}
