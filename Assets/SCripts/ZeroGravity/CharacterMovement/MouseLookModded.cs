using UnityEngine;
using ZeroGravity.Math;
using ZeroGravity.UI;

namespace ZeroGravity.CharacterMovement
{
	public class MouseLookModded : MonoBehaviour
	{
		public float XSensitivity = 40f;

		public float YSensitivity = 40f;

		public bool clampVerticalRotation = true;

		public float MaxUpAngle = -90f;

		public float MaxDownAngle = 90f;

		public float MaxLeftAngle = -45f;

		public float MaxRightAngle = 45f;

		public bool smooth;

		public float smoothTime = 5f;

		private Quaternion m_CharacterTargetRot;

		public Transform character;

		private float previousRotationX;

		private Quaternion m_CameraYrot;

		private Quaternion m_CameraTargetRot;

		private bool cameraFreed = true;

		public bool freeLook;

		public void Start()
		{
			m_CharacterTargetRot = character.transform.rotation;
			m_CameraTargetRot = Quaternion.identity;
			m_CameraYrot = Quaternion.identity;
		}

		private void Update()
		{
			if (InputManager.GetButtonDown(InputManager.AxisNames.LeftAlt))
			{
				previousRotationX = base.transform.localRotation.eulerAngles.x;
				freeLook = true;
			}
			if (InputManager.GetButtonUp(InputManager.AxisNames.LeftAlt))
			{
				freeLook = false;
				cameraFreed = false;
			}
			float y = InputManager.GetAxis(InputManager.AxisNames.LookHorizontal) * XSensitivity;
			float num = InputManager.GetAxis(InputManager.AxisNames.LookHorizontal) * YSensitivity;
			if (cameraFreed)
			{
				if (freeLook)
				{
					m_CameraYrot *= Quaternion.Euler(0f, y, 0f);
					m_CameraYrot = Quaternion.Euler(0f, ClampAngle(m_CameraYrot.eulerAngles.y, MaxLeftAngle, MaxRightAngle), 0f);
					base.transform.parent.localRotation = Quaternion.Slerp(base.transform.parent.localRotation, m_CameraYrot, smoothTime * Time.deltaTime);
				}
				else
				{
					m_CharacterTargetRot *= Quaternion.Euler(0f, y, 0f);
					character.localRotation = Quaternion.Lerp(character.localRotation, m_CharacterTargetRot, smoothTime * Time.deltaTime);
				}
				m_CameraTargetRot *= Quaternion.Euler(0f - num, 0f, 0f);
				m_CameraTargetRot = Quaternion.Euler(ClampAngle(m_CameraTargetRot.eulerAngles.x, MaxDownAngle, MaxUpAngle), 0f, 0f);
				base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, m_CameraTargetRot, smoothTime * Time.deltaTime);
			}
			else
			{
				bool done = false;
				bool done2 = false;
				base.transform.parent.localRotation = MathHelper.QuaternionSlerp(base.transform.parent.localRotation, Quaternion.identity, smoothTime * Time.deltaTime, ref done);
				base.transform.localRotation = MathHelper.QuaternionSlerp(base.transform.localRotation, Quaternion.Euler(previousRotationX, 0f, 0f), smoothTime * Time.deltaTime, ref done2);
				m_CameraYrot = Quaternion.identity;
				m_CameraTargetRot = Quaternion.Euler(previousRotationX, 0f, 0f);
				cameraFreed = done && done2;
			}
		}

		private void OnApplicationFocus(bool focus)
		{
			if (!focus)
			{
				freeLook = false;
				cameraFreed = false;
			}
		}

		private void FixedUpdate()
		{
		}

		private float ClampAngle(float value, float max, float min)
		{
			float num = value;
			if (num < 360f - max && num >= 180f)
			{
				num = Mathf.Clamp(num, 360f - max, 360f);
			}
			else if (num > min && num < 180f)
			{
				num = Mathf.Clamp(num, 0f, min);
			}
			return num;
		}

		public void ResetCamera()
		{
			base.transform.parent.transform.localRotation = Quaternion.identity;
			m_CameraYrot = Quaternion.identity;
			m_CameraTargetRot = Quaternion.identity;
		}

		private Quaternion ClampRotationAroundYAxis(Quaternion q)
		{
			return Quaternion.identity;
		}
	}
}
