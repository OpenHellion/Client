using UnityEngine;

public class RotateImage : MonoBehaviour
{
	public float RotateSpeed = 1f;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Rotate(new Vector3(0f, 0f, RotateSpeed));
	}
}
