using UnityEngine;

public class CombatLayerBehaviour : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (animator.GetNextAnimatorStateInfo(layerIndex).IsName("Stance1->Stance2") ||
		    animator.GetNextAnimatorStateInfo(layerIndex).IsName("Stance1->Stance3") ||
		    animator.GetNextAnimatorStateInfo(layerIndex).IsName("Stance2->Stance1") ||
		    animator.GetNextAnimatorStateInfo(layerIndex).IsName("Stance2->Stance3") ||
		    animator.GetNextAnimatorStateInfo(layerIndex).IsName("Stance3->Stance1") ||
		    animator.GetNextAnimatorStateInfo(layerIndex).IsName("Stance3->Stance2"))
		{
			animator.SetBool("CanSwitchState", false);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (animator.GetCurrentAnimatorStateInfo(layerIndex).IsName("Stance1->Stance2") ||
		    animator.GetCurrentAnimatorStateInfo(layerIndex).IsName("Stance1->Stance3") ||
		    animator.GetCurrentAnimatorStateInfo(layerIndex).IsName("Stance2->Stance1") ||
		    animator.GetCurrentAnimatorStateInfo(layerIndex).IsName("Stance2->Stance3") ||
		    animator.GetCurrentAnimatorStateInfo(layerIndex).IsName("Stance3->Stance1") ||
		    animator.GetCurrentAnimatorStateInfo(layerIndex).IsName("Stance3->Stance2"))
		{
			animator.SetBool("CanSwitchState", true);
		}
	}
}
