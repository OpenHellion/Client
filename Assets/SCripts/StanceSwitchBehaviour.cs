using UnityEngine;
using ZeroGravity.Objects;

public class StanceSwitchBehaviour : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		MyPlayer.Instance.animHelper.ToggleWeaponStanceSwitch(false);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		MyPlayer.Instance.animHelper.ToggleWeaponStanceSwitch(true);
	}
}
