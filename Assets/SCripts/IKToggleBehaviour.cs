using UnityEngine;
using ZeroGravity.Objects;

public class IKToggleBehaviour : StateMachineBehaviour
{
	private bool wasIK;

	private bool inTransition;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		inTransition = true;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (inTransition)
		{
			if (animator.GetComponentInParent<OtherPlayer>().Inventory.ItemInHands.useIkForTargeting)
			{
				animator.GetComponent<AnimatorHelper>().aimIKController.ToggleIK(false);
				wasIK = true;
			}
			inTransition = false;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (wasIK && animator.GetInteger("PlayerStance") != 1)
		{
			animator.GetComponent<AnimatorHelper>().aimIKController.ToggleIK(true);
		}
	}
}
