using UnityEngine;

public class HxSimpleRotate : MonoBehaviour
{
	public Vector3 RotateSpeed;

	private void Update()
	{
		base.transform.Rotate(RotateSpeed * Time.deltaTime, Space.Self);
	}
}
