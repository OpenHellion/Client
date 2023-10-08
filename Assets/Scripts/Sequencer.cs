using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequencer : MonoBehaviour
{
	public List<GameObject> GameObjects;

	public float GlobalDelayTime = 0.1f;

	private float delayTime;

	private string animationParameter = string.Empty;

	private bool animationBoolValue;

	public void StartActivationSequence()
	{
		delayTime = GlobalDelayTime;
		StartCoroutine("StartActivationSequenceCoroutine");
	}

	public void StartActivationSequence(float time)
	{
		delayTime = time;
		StartCoroutine("StartActivationSequenceCoroutine");
	}

	public void StartAnimationSequence(string trigger)
	{
		delayTime = GlobalDelayTime;
		animationParameter = trigger;
		StartCoroutine("StartAnimationTriggerSequenceCoroutine");
	}

	public void StartAnimationSequence(string trigger, float time)
	{
		delayTime = time;
		animationParameter = trigger;
		StartCoroutine("StartAnimationTriggerSequenceCoroutine");
	}

	public void StartAnimationSequence(string boolName, bool boolValue)
	{
		delayTime = GlobalDelayTime;
		animationParameter = boolName;
		animationBoolValue = boolValue;
		StartCoroutine("StartAnimationSequenceCoroutine");
	}

	public void StartAnimationSequence(string boolName, bool boolValue, float time)
	{
		delayTime = time;
		animationParameter = boolName;
		animationBoolValue = boolValue;
		StartCoroutine("StartAnimationSequenceCoroutine");
	}

	private IEnumerator StartActivationSequenceCoroutine()
	{
		foreach (GameObject go in GameObjects)
		{
			go.SetActive(true);
			yield return new WaitForSeconds(delayTime);
		}
	}

	private IEnumerator StartAnimationTriggerSequenceCoroutine()
	{
		foreach (GameObject go in GameObjects)
		{
			if (go.GetComponent<Animator>() != null)
			{
				go.GetComponent<Animator>().SetTrigger(animationParameter);
			}

			yield return new WaitForSeconds(delayTime);
		}
	}

	private IEnumerator StartAnimationBoolSequenceCoroutine()
	{
		foreach (GameObject go in GameObjects)
		{
			if (go.GetComponent<Animator>() != null)
			{
				go.GetComponent<Animator>().SetBool(animationParameter, animationBoolValue);
			}

			yield return new WaitForSeconds(delayTime);
		}
	}
}
