using UnityEngine;

public class JumpTypeSwitchBehaviour : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetFloat("JumpType", animator.GetBool("InStance") ? 1 : 0);
	}
}
