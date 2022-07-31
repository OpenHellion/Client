using System;
using UnityEngine;

namespace RootMotion
{
	public class CameraController : MonoBehaviour
	{
		[Serializable]
		public enum UpdateMode
		{
			Update = 0,
			FixedUpdate = 1,
			LateUpdate = 2
		}

		public Transform target;

		public UpdateMode updateMode = UpdateMode.LateUpdate;

		public bool lockCursor = true;

		public bool smoothFollow;

		public float followSpeed = 10f;

		public float distance = 10f;

		public float minDistance = 4f;

		public float maxDistance = 10f;

		public float zoomSpeed = 10f;

		public float zoomSensitivity = 1f;

		public float rotationSensitivity = 3.5f;

		public float yMinLimit = -20f;

		public float yMaxLimit = 80f;

		public Vector3 offset = new Vector3(0f, 1.5f, 0.5f);

		public bool rotateAlways = true;

		public bool rotateOnLeftButton;

		public bool rotateOnRightButton;

		public bool rotateOnMiddleButton;

		private Vector3 targetDistance;

		private Vector3 position;

		private Quaternion rotation = Quaternion.identity;

		private Vector3 smoothPosition;

		private Camera cam;

		public float x { get; private set; }

		public float y { get; private set; }

		public float distanceTarget { get; private set; }

		private float zoomAdd
		{
			get
			{
				float axis = Input.GetAxis("Mouse ScrollWheel");
				if (axis > 0f)
				{
					return 0f - zoomSensitivity;
				}
				if (axis < 0f)
				{
					return zoomSensitivity;
				}
				return 0f;
			}
		}

		protected virtual void Awake()
		{
			Vector3 eulerAngles = base.transform.eulerAngles;
			x = eulerAngles.y;
			y = eulerAngles.x;
			distanceTarget = distance;
			smoothPosition = base.transform.position;
			cam = GetComponent<Camera>();
		}

		protected virtual void Update()
		{
			if (updateMode == UpdateMode.Update)
			{
				UpdateTransform();
			}
		}

		protected virtual void FixedUpdate()
		{
			if (updateMode == UpdateMode.FixedUpdate)
			{
				UpdateTransform();
			}
		}

		protected virtual void LateUpdate()
		{
			UpdateInput();
			if (updateMode == UpdateMode.LateUpdate)
			{
				UpdateTransform();
			}
		}

		public void UpdateInput()
		{
			if (!(target == null) && cam.enabled)
			{
				Cursor.lockState = (lockCursor ? CursorLockMode.Locked : CursorLockMode.None);
				Cursor.visible = ((!lockCursor) ? true : false);
				if (rotateAlways || (rotateOnLeftButton && Input.GetMouseButton(0)) || (rotateOnRightButton && Input.GetMouseButton(1)) || (rotateOnMiddleButton && Input.GetMouseButton(2)))
				{
					x += Input.GetAxis("Mouse X") * rotationSensitivity;
					y = ClampAngle(y - Input.GetAxis("Mouse Y") * rotationSensitivity, yMinLimit, yMaxLimit);
				}
				distanceTarget = Mathf.Clamp(distanceTarget + zoomAdd, minDistance, maxDistance);
			}
		}

		public void UpdateTransform()
		{
			UpdateTransform(Time.deltaTime);
		}

		public void UpdateTransform(float deltaTime)
		{
			if (!(target == null) && cam.enabled)
			{
				distance += (distanceTarget - distance) * zoomSpeed * deltaTime;
				rotation = Quaternion.AngleAxis(x, Vector3.up) * Quaternion.AngleAxis(y, Vector3.right);
				if (!smoothFollow)
				{
					smoothPosition = target.position;
				}
				else
				{
					smoothPosition = Vector3.Lerp(smoothPosition, target.position, deltaTime * followSpeed);
				}
				position = smoothPosition + rotation * (offset - Vector3.forward * distance);
				base.transform.position = position;
				base.transform.rotation = rotation;
			}
		}

		private float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360f)
			{
				angle += 360f;
			}
			if (angle > 360f)
			{
				angle -= 360f;
			}
			return Mathf.Clamp(angle, min, max);
		}
	}
}
