using System;
using System.Collections;
using UnityEngine;
using ZeroGravity.LevelDesign;
using ZeroGravity.Math;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.CharacterMovement
{
	public class CameraController : MonoBehaviour
	{
		public AnimatorHelper animatorHelper;

		[SerializeField]
		private Transform centerOfMassTransform;

		[SerializeField]
		private Transform mouseLookYTransform;

		[SerializeField]
		private Transform mouseLookXTransform;

		[SerializeField]
		private Transform freeLookYTransform;

		[SerializeField]
		private Transform freeLookXTransform;

		public Transform spineTransform;

		[SerializeField]
		private Transform spineFollowerTransform;

		[SerializeField]
		private Transform InertiaCameraHolder;

		[SerializeField]
		private BoxCollider itemInHandsCollider;

		[SerializeField]
		private MyCharacterController characterController;

		[SerializeField]
		private Camera mainCamera;

		public CameraShake cameraShakeController;

		[SerializeField]
		private float mouseSensitivity = 2.5f;

		private float mouseRightAxis;

		private float mouseUpAxis;

		private float leanRightAxis;

		private bool isFreeLook;

		private bool autoFreeLook;

		private bool isAttached;

		private bool isZeroG;

		private bool canMoveCamera = true;

		private bool isMovementEnabled = true;

		private float angleX;

		private float freeLookAngleY;

		private float freeLookAngleX;

		private float freeLookAngleYZero;

		private float freeLookAngleXZero;

		[SerializeField]
		private float resetLerpDuration = 0.15f;

		private float resetLerpTime;

		private float resetLerpStep;

		[SerializeField]
		private float maxUpAngle = 85f;

		[SerializeField]
		private float maxDownAngle = 85f;

		[SerializeField]
		private float maxRightAngle = 80f;

		[SerializeField]
		private float maxFreeLookRightAngle = 90f;

		[SerializeField]
		private float freeLookMaxUpAngle = 85f;

		[SerializeField]
		private float freeLookMaxDownAngle = 40f;

		private Vector3 freeLookYTransformPos = new Vector3(0f, 0.36f, 0f);

		private bool animatorIsTurning = true;

		private float xRotationLerped;

		private float yRotationLerped;

		private float zRotationLerped;

		private float xSwayHelper;

		private float ySwayHelper;

		public float swayMultiplier;

		public float xSwayMultiplier;

		private bool isCameraAttachedToHeadBone;

		private bool lerpControllerBack;

		private float lerpControllerBackStartTime;

		private float lerpControllerBackStrength;

		private Vector3 lerpControllerBackZeroPos;

		private Quaternion lerpControllerBackZeroRot;

		private Vector3 lerpControllerBackStartPos;

		private Quaternion lerpControllerBackStartRot;

		private bool doLateUpdate = true;

		private bool doInertia;

		public Vector3 translationInertion = Vector3.zero;

		public Vector3 rotationInertion = Vector3.zero;

		public float lerpCoef = 1f;

		public float rotInertionMultiplier = 10f;

		public float tranInertionMultiplier = 0.5f;

		public Transform FreelookTransform
		{
			get
			{
				return freeLookYTransform;
			}
		}

		public float MouseSensitivity
		{
			get
			{
				return mouseSensitivity;
			}
			set
			{
				mouseSensitivity = value;
			}
		}

		public bool IsZeroG
		{
			set
			{
				isZeroG = value;
			}
		}

		public bool IsFreeLook
		{
			get
			{
				return isFreeLook;
			}
		}

		public bool AutoFreeLook
		{
			get
			{
				return autoFreeLook;
			}
		}

		public bool IsAttached
		{
			get
			{
				return isAttached;
			}
			set
			{
				isAttached = value;
			}
		}

		public float MouseUpAxis
		{
			get
			{
				return mouseUpAxis;
			}
		}

		public float MouseRightAxis
		{
			get
			{
				return mouseRightAxis;
			}
		}

		public float MaxUpAngle
		{
			get
			{
				return maxUpAngle;
			}
		}

		public float MaxDownAngle
		{
			get
			{
				return maxDownAngle;
			}
		}

		public float MaxRightAngle
		{
			get
			{
				return maxRightAngle;
			}
		}

		public Transform MouseLookYTransform
		{
			get
			{
				return mouseLookYTransform;
			}
		}

		public Transform MouseLookXTransform
		{
			get
			{
				return mouseLookXTransform;
			}
		}

		public Transform FreeLookXTransform
		{
			get
			{
				return freeLookXTransform;
			}
		}

		public Transform SpineTransform
		{
			get
			{
				return spineTransform;
			}
		}

		public Camera MainCamera
		{
			get
			{
				return mainCamera;
			}
		}

		public bool DoInertia
		{
			set
			{
				doInertia = value;
				if (!value)
				{
					InertiaCameraHolder.transform.localPosition = Vector3.zero;
				}
			}
		}

		private void Awake()
		{
			resetLerpStep = 1f / resetLerpDuration;
		}

		private void GetMouseAxis()
		{
			mouseRightAxis = InputController.GetAxis(InputController.AxisNames.LookHorizontal) * mouseSensitivity;
			mouseUpAxis = (0f - InputController.GetAxis(InputController.AxisNames.LookVertical)) * mouseSensitivity;
			if (Client.IsGameBuild)
			{
				mouseUpAxis = -((!Client.Instance.InvertedMouse) ? 1 : (-1)) * (InputController.GetAxis(InputController.AxisNames.LookVertical) * mouseSensitivity);
			}
			else
			{
				mouseUpAxis = -1f * (InputController.GetAxis(InputController.AxisNames.LookVertical) * mouseSensitivity);
			}
		}

		private void SetAnimatorTurningVelocity()
		{
			if (!characterController.IsMovementEnabled || isAttached)
			{
				if (animatorIsTurning)
				{
					animatorHelper.SetParameter(null, null, null, null, null, null, false);
					animatorIsTurning = false;
				}
			}
			else if (!animatorHelper.GetParameterBool(AnimatorHelper.Parameter.isMoving) && !isFreeLook && Mathf.Abs(mouseRightAxis) > 0f)
			{
				animatorHelper.SetParameter(null, null, null, null, null, null, true);
				animatorIsTurning = true;
				if (mouseRightAxis > 0.4f)
				{
					animatorHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 1);
				}
				if (mouseRightAxis < -0.4f)
				{
					animatorHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, -1);
				}
			}
			else
			{
				animatorHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 0);
				animatorHelper.SetParameter(null, null, null, null, null, null, false);
			}
		}

		public void RotateCharacter()
		{
			if (angleX.IsNotEpsilonZero())
			{
				characterController.transform.rotation = characterController.MainCamera.transform.rotation;
				angleX = 0f;
				mouseLookXTransform.localRotation = Quaternion.Euler(angleX, 0f, 0f);
			}
		}

		private void RotateCharacter(float xRotation, float yRotation, float zRotation)
		{
			if (!isZeroG)
			{
				centerOfMassTransform.RotateAround(centerOfMassTransform.position, centerOfMassTransform.up, yRotation);
				characterController.RotateVelocity(centerOfMassTransform.up, yRotation);
				angleX = Mathf.Clamp(angleX + xRotation, 0f - maxUpAngle, maxDownAngle);
				mouseLookXTransform.localRotation = Quaternion.Euler(angleX, 0f, 0f);
			}
			else
			{
				characterController.rigidBody.maxAngularVelocity = characterController.MaxAngularVelocity;
				yRotationLerped = Mathf.Lerp(0f, yRotation, Time.deltaTime);
				xRotationLerped = Mathf.Lerp(0f, xRotation, Time.deltaTime);
				zRotationLerped = Mathf.Lerp(0f, zRotation, Time.deltaTime);
				characterController.rigidBody.AddRelativeTorque(new Vector3(xRotationLerped, yRotationLerped, (leanRightAxis == 0f) ? 0f : zRotationLerped) * characterController.CurrentSpeeds.RotateVelocity, ForceMode.Impulse);
			}
			SwayWeapon(xRotation, yRotation);
		}

		private void SwayWeapon(float xRotation, float yRotation)
		{
			ySwayHelper = Mathf.Lerp(ySwayHelper, Mathf.Clamp(MathHelper.ProportionalValue(yRotation, -25f, 25f, -1f, 1f), -1f, 1f), Time.deltaTime * swayMultiplier);
			xSwayHelper = Mathf.Lerp(xSwayHelper, Mathf.Clamp(MathHelper.ProportionalValue(xRotation * xSwayMultiplier, -25f, 25f, -1f, 1f), -1f, 1f), Time.deltaTime * swayMultiplier);
			AnimatorHelper obj = animatorHelper;
			float? rotationDirectionRight = Mathf.Clamp(ySwayHelper, -1f, 1f);
			obj.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, rotationDirectionRight);
			AnimatorHelper obj2 = animatorHelper;
			rotationDirectionRight = Mathf.Clamp(xSwayHelper, -1f, 1f);
			obj2.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, rotationDirectionRight);
		}

		private void RotateCamera(float xRotation, float yRotation)
		{
			freeLookAngleY = Mathf.Clamp(freeLookAngleY + yRotation, 0f - maxFreeLookRightAngle, maxFreeLookRightAngle);
			freeLookYTransform.rotation = Quaternion.AngleAxis(freeLookAngleY, base.transform.up) * mouseLookXTransform.transform.rotation;
			freeLookAngleX = Mathf.Clamp(freeLookAngleX + xRotation, 0f - Mathf.Max(freeLookMaxUpAngle + ((!(angleX > 0f)) ? angleX : 0f), 0f), Mathf.Max(freeLookMaxDownAngle - ((!(angleX > 0f)) ? 0f : angleX), 0f));
			freeLookXTransform.localRotation = Quaternion.Euler(freeLookAngleX, 0f, 0f);
		}

		public void ToggleAutoFreeLook(bool isActive)
		{
			autoFreeLook = isActive;
			ToggleFreeLook(autoFreeLook);
		}

		public void ToggleFreeLook(bool isActive)
		{
			if (MyPlayer.Instance.ShipControlMode == ShipControlMode.Navigation || MyPlayer.Instance.ShipControlMode == ShipControlMode.Docking)
			{
				return;
			}
			isFreeLook = isActive;
			if (isFreeLook)
			{
				if (Client.IsGameBuild && MyPlayer.Instance.IsLockedToTrigger)
				{
					Client.Instance.InputModule.UseCustomCursorPosition = false;
				}
				return;
			}
			canMoveCamera = false;
			if (Client.IsGameBuild && MyPlayer.Instance.IsLockedToTrigger)
			{
				Client.Instance.InputModule.UseCustomCursorPosition = true;
			}
		}

		private void Update()
		{
			if (!isMovementEnabled && !MyPlayer.Instance.ShouldMoveCamera)
			{
				return;
			}
			if (lerpControllerBack)
			{
				LerpCameraControllerBackWorker();
			}
			GetMouseAxis();
			SetAnimatorTurningVelocity();
			if (doInertia)
			{
				AddCameraInertia();
			}
			if (canMoveCamera)
			{
				if (!autoFreeLook || !isFreeLook)
				{
					if (InputController.GetButtonDown(InputController.AxisNames.LeftAlt))
					{
						ToggleFreeLook(true);
					}
					if (InputController.GetButtonUp(InputController.AxisNames.LeftAlt) && isFreeLook)
					{
						ToggleFreeLook(false);
					}
				}
				if (isAttached && !isFreeLook)
				{
					return;
				}
				if (isFreeLook)
				{
					if (MyPlayer.Instance.LockedToTrigger == null || MyPlayer.Instance.ShipControlMode == ShipControlMode.Piloting)
					{
						RotateCamera(mouseUpAxis, mouseRightAxis);
						if (!isAttached && !autoFreeLook)
						{
							RotateCharacter(0f, 0f, leanRightAxis);
						}
					}
				}
				else
				{
					RotateCharacter(mouseUpAxis, mouseRightAxis, leanRightAxis);
				}
				Client.Instance.CanvasManager.HelmetOverlayModel.SetAxis(mouseRightAxis, mouseUpAxis, leanRightAxis);
			}
			else
			{
				resetLerpTime += resetLerpStep * Time.deltaTime;
				freeLookXTransform.localRotation = Quaternion.Lerp(freeLookXTransform.localRotation, Quaternion.Euler(freeLookAngleXZero, 0f, 0f), resetLerpTime);
				freeLookYTransform.localRotation = Quaternion.Lerp(freeLookYTransform.localRotation, Quaternion.Euler(0f, freeLookAngleYZero, 0f), resetLerpTime);
				canMoveCamera = resetLerpTime > 0.9999f;
				if (canMoveCamera)
				{
					resetLerpTime = 0f;
					freeLookAngleX = freeLookAngleXZero;
					freeLookAngleY = freeLookAngleYZero;
					freeLookXTransform.localRotation = Quaternion.Euler(freeLookAngleXZero, 0f, 0f);
					freeLookYTransform.localRotation = Quaternion.Euler(0f, freeLookAngleYZero, 0f);
					resetLerpStep = 1f / resetLerpDuration;
				}
			}
		}

		private float CalculateAnimationValByAngle(float camAngle, float minAngle, float maxAngle)
		{
			return camAngle / ((!(camAngle < 0f)) ? maxAngle : minAngle);
		}

		public void ResetCameraPositionAndRotation()
		{
			resetLerpTime = 0f;
			freeLookAngleX = freeLookAngleY = 0f;
			freeLookAngleXZero = freeLookAngleYZero = 0f;
			angleX = 0f;
			mouseLookYTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			mouseLookXTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			freeLookXTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			freeLookYTransform.SetLocalPositionAndRotation(freeLookYTransformPos, Quaternion.identity);
		}

		public void ResetLookAt(float? duration = null)
		{
			resetLerpTime = 0f;
			freeLookAngleX = 0f;
			freeLookAngleY = 0f;
			freeLookAngleXZero = 0f;
			freeLookAngleYZero = 0f;
			yRotationLerped = 0f;
			xRotationLerped = 0f;
			zRotationLerped = 0f;
			if (duration.HasValue)
			{
				resetLerpStep = 1f / duration.Value;
			}
			ToggleFreeLook(autoFreeLook);
		}

		public void LookAtPoint(Vector3 point)
		{
			mouseLookXTransform.localRotation = Quaternion.identity;
			freeLookYTransform.LookAt(point);
			freeLookAngleY = ((!(freeLookYTransform.localEulerAngles.y > 180f)) ? freeLookYTransform.localEulerAngles.y : (freeLookYTransform.localEulerAngles.y - 360f));
			freeLookYTransform.localRotation = Quaternion.Euler(0f, freeLookAngleY, 0f);
			freeLookXTransform.LookAt(point);
			freeLookAngleX = ((!(freeLookXTransform.localEulerAngles.x > 180f)) ? freeLookXTransform.localEulerAngles.x : (freeLookXTransform.localEulerAngles.x - 360f));
			freeLookXTransform.localRotation = Quaternion.Euler(freeLookAngleX, 0f, 0f);
			freeLookAngleXZero = freeLookAngleX;
			freeLookAngleYZero = freeLookAngleY;
		}

		public void RefreshOutfitData(Transform outfitTrans)
		{
			spineTransform = animatorHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
			if (isCameraAttachedToHeadBone)
			{
				base.transform.parent = characterController.HeadCameraParent;
			}
		}

		public void ToggleCameraMovement(bool? isEnabled)
		{
			if (isEnabled.HasValue)
			{
				isMovementEnabled = isEnabled.Value;
			}
			else
			{
				isMovementEnabled = !isMovementEnabled;
			}
		}

		public void SetLeanRightAxis(float value)
		{
			leanRightAxis = value;
		}

		public void AddCharacterRotation(Vector3 eulerAngle)
		{
			RotateCharacter(eulerAngle.x, eulerAngle.y, eulerAngle.z);
		}

		private void OnApplicationFocus(bool focus)
		{
			if (!focus)
			{
				canMoveCamera = !isFreeLook;
				isFreeLook = false;
			}
		}

		public void ToggleCameraAttachToHeadBone(bool? attachVal = null, Vector3? lockedAngle = null)
		{
			bool flag = (isAttached = ((!attachVal.HasValue) ? (!isCameraAttachedToHeadBone) : attachVal.Value));
			if (isCameraAttachedToHeadBone == flag)
			{
				LerpCameraControllerBack(1f, Vector3.zero, lockedAngle);
				return;
			}
			mouseLookXTransform.parent = characterController.transform;
			IEnumerator enumerator = mouseLookXTransform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Transform)enumerator.Current).localPosition += ((!flag) ? freeLookYTransformPos : (-freeLookYTransformPos));
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
			if (flag)
			{
				base.transform.parent = characterController.HeadCameraParent;
			}
			else
			{
				base.transform.parent = characterController.transform;
			}
			mouseLookXTransform.parent = mouseLookYTransform;
			LerpCameraControllerBack(1f, Vector3.zero, lockedAngle);
			characterController.LerpCameraBack(1f);
			isCameraAttachedToHeadBone = flag;
		}

		public void LerpCameraControllerBack(float time, Vector3? zeroPos = null, Vector3? zeroRot = null)
		{
			angleX = ((!zeroRot.HasValue) ? 0f : zeroRot.Value.x);
			lerpControllerBack = true;
			lerpControllerBackStartTime = Time.time;
			lerpControllerBackStrength = 1f / time;
			lerpControllerBackZeroPos = ((!zeroPos.HasValue) ? Vector3.zero : zeroPos.Value);
			lerpControllerBackZeroRot = ((!zeroRot.HasValue) ? Quaternion.identity : Quaternion.Euler(zeroRot.Value));
			lerpControllerBackStartPos = MouseLookXTransform.localPosition;
			lerpControllerBackStartRot = MouseLookXTransform.localRotation;
		}

		private void LerpCameraControllerBackWorker()
		{
			float num = (Time.time - lerpControllerBackStartTime) * lerpControllerBackStrength;
			mouseLookXTransform.SetLocalPositionAndRotation(Vector3.Lerp(lerpControllerBackStartPos, lerpControllerBackZeroPos, num), Quaternion.Lerp(lerpControllerBackStartRot, lerpControllerBackZeroRot, num));
			if (num >= 1f)
			{
				lerpControllerBack = false;
			}
		}

		public void SetHandsBoxCollider(BoxCollider collider)
		{
			itemInHandsCollider.center = collider.center;
			itemInHandsCollider.size = collider.size;
		}

		public void UpdateSpineTransform()
		{
			if (doLateUpdate && !isCameraAttachedToHeadBone)
			{
				spineTransform.SetPositionAndRotation(spineFollowerTransform.position, spineFollowerTransform.rotation);
			}
		}

		public void AddCameraInertia()
		{
			InertiaCameraHolder.SetLocalPositionAndRotation(Vector3.Lerp(InertiaCameraHolder.localPosition, translationInertion, Time.deltaTime * lerpCoef), Quaternion.Slerp(InertiaCameraHolder.localRotation, Quaternion.Euler(new Vector3(rotationInertion.x, rotationInertion.y, 0f - rotationInertion.z)), Time.deltaTime * lerpCoef));
		}

		public void SetCameraInertia(Vector3? tranInertia = null, Vector3? rotInertia = null)
		{
			doInertia = true;
			if (tranInertia.HasValue)
			{
				translationInertion = Vector3.ClampMagnitude(tranInertia.Value * tranInertionMultiplier, 0.1f);
			}
			if (rotInertia.HasValue)
			{
				rotationInertion = Vector3.ClampMagnitude(rotInertia.Value * rotInertionMultiplier, 1.6f);
			}
		}
	}
}
