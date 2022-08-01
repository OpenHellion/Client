using UnityEngine;

public class bulletDestroy : MonoBehaviour
{
	private float timer = 0.04f;

	private float nesto;

	private void Start()
	{
	}

	private void Update()
	{
		nesto += Time.deltaTime;
		if (nesto > timer)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
