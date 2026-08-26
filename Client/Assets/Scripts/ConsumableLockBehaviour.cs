using UnityEngine;

public class ConsumableLockBehaviour : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<AnimatorHelper>().ToggleConsumableLock(true);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<AnimatorHelper>().ToggleConsumableLock(false);
	}
}
