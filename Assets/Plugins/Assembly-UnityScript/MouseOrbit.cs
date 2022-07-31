using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("Camera-Control/Mouse Orbit")]
public class MouseOrbit : MonoBehaviour
{
	public Transform target;

	public float distance;

	public float xSpeed;

	public float ySpeed;

	public int yMinLimit;

	public int yMaxLimit;

	private float x;

	private float y;

	public MouseOrbit()
	{
		distance = 10f;
		xSpeed = 250f;
		ySpeed = 120f;
		yMinLimit = -20;
		yMaxLimit = 80;
	}

	public void Start()
	{
		Vector3 eulerAngles = transform.eulerAngles;
		x = eulerAngles.y;
		y = eulerAngles.x;
		if ((bool)GetComponent<Rigidbody>())
		{
			GetComponent<Rigidbody>().freezeRotation = true;
		}
	}

	public void LateUpdate()
	{
		if ((bool)target && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
		{
			Vector3 mousePosition = Input.mousePosition;
			if (mousePosition.x >= 250f || (float)Screen.height - mousePosition.y >= 250f)
			{
				Cursor.visible = false;
				x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
				y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
				y = ClampAngle(y, yMinLimit, yMaxLimit);
				Quaternion quaternion = Quaternion.Euler(y, x, 0f);
				Vector3 position = quaternion * new Vector3(0f, 0f, 0f - distance) + target.position;
				transform.rotation = quaternion;
				transform.position = position;
			}
		}
		else
		{
			Cursor.visible = true;
		}
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (!(angle >= -360f))
		{
			angle += 360f;
		}
		if (!(angle <= 360f))
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}
}
