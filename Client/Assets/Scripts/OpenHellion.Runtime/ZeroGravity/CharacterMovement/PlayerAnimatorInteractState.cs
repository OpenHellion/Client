using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity.CharacterMovement
{
	public class PlayerAnimatorInteractState : StateMachineBehaviour
	{
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (animator.transform.parent.gameObject.TryGetComponent<MyPlayer>(out var myPlayer))
			{
				if (stateInfo.IsName("Locks"))
				{
					myPlayer.AnimInteraction_LockExit();
				}
				else if (stateInfo.IsName("Interacts"))
				{
					myPlayer.AnimInteraction_InteractExit();
				}
				else
				{
					myPlayer.AnimInteraction_NoneExit();
				}

				return;
			}

			if (animator.transform.parent.gameObject.TryGetComponent<OtherPlayer>(out var otherPlayer))
			{
				if (stateInfo.IsName("Locks"))
				{
					otherPlayer.AnimInteraction_LockExit();
				}
				else if (stateInfo.IsName("Interacts"))
				{
					otherPlayer.AnimInteraction_InteractExit();
				}
				else
				{
					otherPlayer.AnimInteraction_NoneExit();
				}
			}
		}

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (animator.transform.parent.gameObject.TryGetComponent<MyPlayer>(out var myPlayer))
			{
				if (stateInfo.IsName("Locks"))
				{
					myPlayer.AnimInteraction_LockEnter();
				}
				else if (stateInfo.IsName("Interacts"))
				{
					myPlayer.AnimInteraction_InteractEnter();
				}
				else
				{
					myPlayer.AnimInteraction_NoneEnter();
				}

				return;
			}

			if (animator.transform.parent.gameObject.TryGetComponent<OtherPlayer>(out var otherPlayer))
			{
				if (stateInfo.IsName("Locks"))
				{
					otherPlayer.AnimInteraction_LockEnter();
				}
				else if (stateInfo.IsName("Interacts"))
				{
					otherPlayer.AnimInteraction_InteractEnter();
				}
				else
				{
					otherPlayer.AnimInteraction_NoneEnter();
				}
			}
		}
	}
}
