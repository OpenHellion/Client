using System;
using UnityEngine;

[Serializable]
public class MouseOrbit1 : MonoBehaviour
{
	public Transform target;

	public Transform parent;

	public float distance;

	public float xSpeed;

	public float ySpeed;

	public int xMinLimit;

	public int xMaxLimit;

	public int yMinLimit;

	public int yMaxLimit;

	public bool cameraLock;

	public bool permaLock;

	public float x;

	public float y;

	public MouseOrbit1()
	{
		distance = 10f;
		xSpeed = 250f;
		ySpeed = 120f;
		xMinLimit = -20;
		xMaxLimit = 80;
		yMinLimit = -20;
		yMaxLimit = 80;
		permaLock = true;
	}

	public void Start()
	{
		Vector3 localEulerAngles = transform.localEulerAngles;
		x = localEulerAngles.y;
		y = localEulerAngles.x;
		if ((bool)GetComponent<Rigidbody>())
		{
			GetComponent<Rigidbody>().freezeRotation = true;
		}
		if (!target)
		{
			target = GameObject.Find("Camera Target").transform;
		}
	}

	public void LateUpdate()
	{
		LockCamera();
		if ((bool)target)
		{
			x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
			y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
			y = ClampAngle(y, yMinLimit, yMaxLimit);
			Quaternion quaternion = default(Quaternion);
			if (!cameraLock && !permaLock)
			{
				quaternion = Quaternion.Euler(y, x, 0f);
				transform.localEulerAngles = new Vector3(y, x, 0f);
			}
			Vector3 position = quaternion * new Vector3(0f, 0f, 0f - distance) + target.position;
			float z = target.eulerAngles.z;
			Vector3 localEulerAngles = parent.localEulerAngles;
			float num = (localEulerAngles.z = z);
			Vector3 vector2 = (parent.localEulerAngles = localEulerAngles);
			transform.position = position;
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

	public void LockCamera()
	{
		if (!Input.GetKeyDown("l"))
		{
			return;
		}
		cameraLock = !cameraLock;
		if ((bool)gameObject.GetComponent<AudioSource>())
		{
			if (gameObject.GetComponent<AudioSource>().isPlaying)
			{
				gameObject.GetComponent<AudioSource>().Stop();
			}
			gameObject.GetComponent<AudioSource>().Play();
		}
	}
}
