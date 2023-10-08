using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity.CharacterMovement
{
	public class PlayerAnimatorInteractState : StateMachineBehaviour
	{
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			MyPlayer component = animator.transform.parent.gameObject.GetComponent<MyPlayer>();
			if (component != null)
			{
				if (stateInfo.IsName("Locks"))
				{
					component.AnimInteraction_LockExit();
				}
				else if (stateInfo.IsName("Interacts"))
				{
					component.AnimInteraction_InteractExit();
				}
				else
				{
					component.AnimInteraction_NoneExit();
				}

				return;
			}

			OtherPlayer component2 = animator.transform.parent.gameObject.GetComponent<OtherPlayer>();
			if (component2 != null)
			{
				if (stateInfo.IsName("Locks"))
				{
					component2.AnimInteraction_LockExit();
				}
				else if (stateInfo.IsName("Interacts"))
				{
					component2.AnimInteraction_InteractExit();
				}
				else
				{
					component2.AnimInteraction_NoneExit();
				}
			}
		}

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			MyPlayer component = animator.transform.parent.gameObject.GetComponent<MyPlayer>();
			if (component != null)
			{
				if (stateInfo.IsName("Locks"))
				{
					component.AnimInteraction_LockEnter();
				}
				else if (stateInfo.IsName("Interacts"))
				{
					component.AnimInteraction_InteractEnter();
				}
				else
				{
					component.AnimInteraction_NoneEnter();
				}

				return;
			}

			OtherPlayer component2 = animator.transform.parent.gameObject.GetComponent<OtherPlayer>();
			if (component2 != null)
			{
				if (stateInfo.IsName("Locks"))
				{
					component2.AnimInteraction_LockEnter();
				}
				else if (stateInfo.IsName("Interacts"))
				{
					component2.AnimInteraction_InteractEnter();
				}
				else
				{
					component2.AnimInteraction_NoneEnter();
				}
			}
		}
	}
}
