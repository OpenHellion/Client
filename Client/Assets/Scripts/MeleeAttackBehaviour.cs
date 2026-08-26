using UnityEngine;

public class MeleeAttackBehaviour : StateMachineBehaviour
{
	private int previous = -1;

	private int repeatCounter;

	private int DoRandom(int previous)
	{
		int num = previous;
		do
		{
			num = Random.Range(0, 2);
		} while (num == previous);

		return num;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		int num = DoRandom(previous);
		if (previous != num)
		{
			previous = num;
			repeatCounter = 0;
		}
		else
		{
			repeatCounter++;
		}

		if (repeatCounter == 2)
		{
			num = DoRandom(previous);
		}

		animator.SetFloat("MeleeAttackType", num);
	}
}
