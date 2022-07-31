using System;
using UnityEngine;
using UnityEngine.Events;

public class AnimatorEventManager : MonoBehaviour
{
	[Serializable]
	public class AnimatorEvent : UnityEvent<AnimatorStateInfo>
	{
	}

	public AnimatorEvent OnStateEnter;

	public AnimatorEvent OnStateExit;
}
