using UnityEngine;

public class LookAtTest : MonoBehaviour
{
	public Transform LookAt;

	private void Start()
	{
	}

	private void Update()
	{
		if (LookAt != null)
		{
			base.transform.LookAt(LookAt);
		}
	}
}
