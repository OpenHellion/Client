using UnityEngine;
using ZeroGravity.Objects;

public class TurretTargetBehaviour : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetBool("CanTarget", true);
		animator.GetComponentInParent<PortableTurret>().ToggleTargettingSphere(true);
	}
}
