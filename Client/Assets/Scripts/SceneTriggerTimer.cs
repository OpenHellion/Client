using System.Collections;
using UnityEngine;

public class SceneTriggerTimer : MonoBehaviour
{
	public Collider Collider;

	private float duration;

	private void Awake()
	{
		Collider = GetComponent<Collider>();
	}

	public void DisableTriggerForDuration(float time)
	{
		if (Collider != null)
		{
			duration = time;
			Collider.enabled = false;
			StopCoroutine(EnableTriggerAfterTime());
			StartCoroutine(EnableTriggerAfterTime());
		}
	}

	private IEnumerator EnableTriggerAfterTime()
	{
		while (duration > 0f)
		{
			duration -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		if (Collider != null)
		{
			Collider.enabled = true;
		}
	}
}
