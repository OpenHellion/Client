using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
	public float timeToDestroy;

	private void Update()
	{
		if (timeToDestroy <= 0f)
		{
			Object.Destroy(base.gameObject);
		}

		timeToDestroy -= Time.deltaTime;
	}
}
