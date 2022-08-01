using UnityEngine;

public class UnlockBehaviour : StateMachineBehaviour
{
	public AnimatorHelper.UnlockAnimator itemToUnlock;

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		AnimatorHelper component = animator.gameObject.GetComponent<AnimatorHelper>();
		if (component != null)
		{
			if (itemToUnlock == AnimatorHelper.UnlockAnimator.Reload)
			{
				component.SetParameter(null, null, null, null, null, null, null, null, null, false);
			}
			else if (itemToUnlock == AnimatorHelper.UnlockAnimator.Equip)
			{
				component.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, false);
			}
		}
	}
}
