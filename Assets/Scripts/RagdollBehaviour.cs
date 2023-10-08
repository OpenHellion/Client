using UnityEngine;
using ZeroGravity.CharacterMovement;

public class RagdollBehaviour : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		RagdollHelper component = animator.gameObject.GetComponent<RagdollHelper>();
		AnimatorHelper component2 = animator.gameObject.GetComponent<AnimatorHelper>();
		if (component != null)
		{
			component.OnRagdollStateExit();
		}

		if (component2 != null)
		{
			component2.SetParameter(null, null, null, null, null, null, null, null, null, false);
			component2.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, false);
		}
	}
}
