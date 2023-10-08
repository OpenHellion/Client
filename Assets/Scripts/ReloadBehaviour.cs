using UnityEngine;

public class ReloadBehaviour : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (animator.GetCurrentAnimatorStateInfo(layerIndex).IsName("DrawHolster.Reload") &&
		    animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime % 1f > 0.97f)
		{
			animator.SetBool("Reloading", false);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}
}
