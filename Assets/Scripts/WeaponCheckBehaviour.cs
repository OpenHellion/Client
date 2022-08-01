using UnityEngine;

public class WeaponCheckBehaviour : StateMachineBehaviour
{
	public bool unlocking;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!unlocking)
		{
			animator.SetBool("WeaponCheckLock", true);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (unlocking)
		{
			animator.SetBool("WeaponCheckLock", false);
		}
	}
}
