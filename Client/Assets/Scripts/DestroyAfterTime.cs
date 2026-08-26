using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
	public float timeToDestroy;

	private void Update()
	{
		if (timeToDestroy <= 0f)
		{
			Destroy(gameObject);
		}

		timeToDestroy -= Time.deltaTime;
	}
}
