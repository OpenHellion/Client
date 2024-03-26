using System;
using System.Collections;
using OpenHellion;
using OpenHellion.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using ZeroGravity.LevelDesign;
using ZeroGravity.Math;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.CharacterMovement
{
	public class CameraController : MonoBehaviour
	{
		public AnimatorHelper animatorHelper;

		[SerializeField] private Transform centerOfMassTransform;

		[SerializeField] private Transform mouseLookYTransform;

		[SerializeField] private Transform mouseLookXTransform;

		[SerializeField] private Transform freeLookYTransform;

		[SerializeField] private Transform freeLookXTransform;

		public Transform spineTransform;

		[SerializeField] private Transform spineFollowerTransform;

		[SerializeField] private Transform InertiaCameraHolder;

		[SerializeField] private BoxCollider itemInHandsCollider;

		[SerializeField] private MyCharacterController characterController;

		public CameraShake cameraShakeController;

		private float _mouseRightAxis;

		private float _mouseUpAxis;

		private float _leanRightAxis;

		private bool _isFreeLook;

		private bool _autoFreeLook;

		private bool _isZeroG;

		private bool _canMoveCamera = true;

		private bool _isMovementEnabled = true;

		private float _angleX;

		private float _freeLookAngleY;

		private float _freeLookAngleX;

		private float _freeLookAngleYZero;

		private float _freeLookAngleXZero;

		[SerializeField] private float resetLerpDuration = 0.15f;

		private float _resetLerpTime;

		private float _resetLerpStep;

		[SerializeField] private float maxUpAngle = 85f;

		[SerializeField] private float maxDownAngle = 85f;

		[SerializeField] private float maxRightAngle = 80f;

		[SerializeField] private float maxFreeLookRightAngle = 90f;

		[SerializeField] private float freeLookMaxUpAngle = 85f;

		[SerializeField] private float freeLookMaxDownAngle = 40f;

		private readonly Vector3 _freeLookYTransformPos = new Vector3(0f, 0.36f, 0f);

		private bool _animatorIsTurning = true;

		private float _xRotationLerped;

		private float _yRotationLerped;

		private float _zRotationLerped;

		private float _xSwayHelper;

		private float _ySwayHelper;

		public float swayMultiplier;

		public float xSwayMultiplier;

		private bool _isCameraAttachedToHeadBone;

		private bool _lerpControllerBack;

		private float _lerpControllerBackStartTime;

		private float _lerpControllerBackStrength;

		private Vector3 _lerpControllerBackZeroPos;

		private Quaternion _lerpControllerBackZeroRot;

		private Vector3 _lerpControllerBackStartPos;

		private Quaternion _lerpControllerBackStartRot;

		private bool _doInertia;

		public Vector3 translationInertion = Vector3.zero;

		public Vector3 rotationInertion = Vector3.zero;

		public float lerpCoef = 1f;

		public float rotInertionMultiplier = 10f;

		public float tranInertionMultiplier = 0.5f;

		public Transform FreelookTransform => freeLookYTransform;

		public bool IsZeroG
		{
			set => _isZeroG = value;
		}

		public bool IsFreeLook => _isFreeLook;

		public bool AutoFreeLook => _autoFreeLook;

		public bool IsAttached { get; set; }

		public float MouseUpAxis => _mouseUpAxis;

		public float MouseRightAxis => _mouseRightAxis;

		public float MaxUpAngle => maxUpAngle;

		public float MaxDownAngle => maxDownAngle;

		public float MaxRightAngle => maxRightAngle;

		public Transform MouseLookYTransform => mouseLookYTransform;

		public Transform MouseLookXTransform => mouseLookXTransform;

		public Transform FreeLookXTransform => freeLookXTransform;

		private World _world;

		public bool DoInertia
		{
			set
			{
				_doInertia = value;
				if (!value)
				{
					InertiaCameraHolder.transform.localPosition = Vector3.zero;
				}
			}
		}

		private void Awake()
		{
			_world = GameObject.Find("/World").GetComponent<World>();
			_resetLerpStep = 1f / resetLerpDuration;
		}

		private void Update()
		{
			if (!_isMovementEnabled && !MyPlayer.Instance.ShouldMoveCamera)
			{
				return;
			}

			if (_lerpControllerBack)
			{
				LerpCameraControllerBackWorker();
			}

			GetMouseAxis();
			SetAnimatorTurningVelocity();
			if (_doInertia)
			{
				AddCameraInertia();
			}

			if (_canMoveCamera)
			{
				if (!_autoFreeLook || !_isFreeLook)
				{
					if (ControlsSubsystem.GetButtonDown(ControlsSubsystem.ConfigAction.FreeLook))
					{
						ToggleFreeLook(true);
					}

					if (ControlsSubsystem.GetButtonUp(ControlsSubsystem.ConfigAction.FreeLook) && _isFreeLook)
					{
						ToggleFreeLook(false);
					}
				}

				if (IsAttached && !_isFreeLook)
				{
					return;
				}

				if (_isFreeLook)
				{
					if (MyPlayer.Instance.LockedToTrigger == null ||
					    MyPlayer.Instance.ShipControlMode == ShipControlMode.Piloting)
					{
						RotateCamera(_mouseUpAxis, _mouseRightAxis);
						if (!IsAttached && !_autoFreeLook)
						{
							RotateCharacter(0f, 0f, _leanRightAxis);
						}
					}
				}
				else
				{
					RotateCharacter(_mouseUpAxis, _mouseRightAxis, _leanRightAxis);
				}

				_world.InGameGUI.HelmetOverlayModel.SetAxis(_mouseRightAxis, _mouseUpAxis, _leanRightAxis);
			}
			else
			{
				_resetLerpTime += _resetLerpStep * Time.deltaTime;
				freeLookXTransform.localRotation = Quaternion.Lerp(freeLookXTransform.localRotation,
					Quaternion.Euler(_freeLookAngleXZero, 0f, 0f), _resetLerpTime);
				freeLookYTransform.localRotation = Quaternion.Lerp(freeLookYTransform.localRotation,
					Quaternion.Euler(0f, _freeLookAngleYZero, 0f), _resetLerpTime);
				_canMoveCamera = _resetLerpTime > 0.9999f;
				if (_canMoveCamera)
				{
					_resetLerpTime = 0f;
					_freeLookAngleX = _freeLookAngleXZero;
					_freeLookAngleY = _freeLookAngleYZero;
					freeLookXTransform.localRotation = Quaternion.Euler(_freeLookAngleXZero, 0f, 0f);
					freeLookYTransform.localRotation = Quaternion.Euler(0f, _freeLookAngleYZero, 0f);
					_resetLerpStep = 1f / resetLerpDuration;
				}
			}
		}

		private void GetMouseAxis()
		{
			_mouseRightAxis = Mouse.current.delta.x.ReadValue() * 0.1f * ControlsSubsystem.RealSensitivity;
			_mouseUpAxis = Mouse.current.delta.y.ReadValue() * 0.1f * ControlsSubsystem.RealSensitivity;
			_mouseUpAxis = Settings.SettingsData.ControlsSettings.InvertMouse
				? 0.1f
				: -0.1f * Mouse.current.delta.y.ReadValue() * ControlsSubsystem.RealSensitivity;
		}

		private void SetAnimatorTurningVelocity()
		{
			if (!characterController.IsMovementEnabled || IsAttached)
			{
				if (_animatorIsTurning)
				{
					animatorHelper.SetParameter(null, null, null, null, null, null, false);
					_animatorIsTurning = false;
				}
			}
			else if (!animatorHelper.GetParameterBool(AnimatorHelper.Parameter.isMoving) && !_isFreeLook &&
			         Mathf.Abs(_mouseRightAxis) > 0f)
			{
				animatorHelper.SetParameter(null, null, null, null, null, null, true);
				_animatorIsTurning = true;
				if (_mouseRightAxis > 0.4f)
				{
					animatorHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null,
						null, null, null, null, null, null, 1);
				}

				if (_mouseRightAxis < -0.4f)
				{
					animatorHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null,
						null, null, null, null, null, null, -1);
				}
			}
			else
			{
				animatorHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null,
					null, null, null, null, null, null, 0);
				animatorHelper.SetParameter(null, null, null, null, null, null, false);
			}
		}

		public void RotateCharacter()
		{
			if (_angleX.IsNotEpsilonZero())
			{
				characterController.transform.rotation = characterController.MainCamera.transform.rotation;
				_angleX = 0f;
				mouseLookXTransform.localRotation = Quaternion.Euler(_angleX, 0f, 0f);
			}
		}

		private void RotateCharacter(float xRotation, float yRotation, float zRotation)
		{
			if (!_isZeroG)
			{
				centerOfMassTransform.RotateAround(centerOfMassTransform.position, centerOfMassTransform.up, yRotation);
				characterController.RotateVelocity(centerOfMassTransform.up, yRotation);
				_angleX = Mathf.Clamp(_angleX + xRotation, 0f - maxUpAngle, maxDownAngle);
				mouseLookXTransform.localRotation = Quaternion.Euler(_angleX, 0f, 0f);
			}
			else
			{
				characterController.RigidBody.maxAngularVelocity = characterController.MaxAngularVelocity;
				_yRotationLerped = Mathf.Lerp(0f, yRotation, Time.deltaTime);
				_xRotationLerped = Mathf.Lerp(0f, xRotation, Time.deltaTime);
				_zRotationLerped = Mathf.Lerp(0f, zRotation, Time.deltaTime);
				characterController.RigidBody.AddRelativeTorque(
					new Vector3(_xRotationLerped, _yRotationLerped, (_leanRightAxis == 0f) ? 0f : _zRotationLerped) *
					characterController.CurrentSpeeds.RotateVelocity, ForceMode.Impulse);
			}

			SwayWeapon(xRotation, yRotation);
		}

		private void SwayWeapon(float xRotation, float yRotation)
		{
			_ySwayHelper = Mathf.Lerp(_ySwayHelper,
				Mathf.Clamp(MathHelper.ProportionalValue(yRotation, -25f, 25f, -1f, 1f), -1f, 1f),
				Time.deltaTime * swayMultiplier);
			_xSwayHelper = Mathf.Lerp(_xSwayHelper,
				Mathf.Clamp(MathHelper.ProportionalValue(xRotation * xSwayMultiplier, -25f, 25f, -1f, 1f), -1f, 1f),
				Time.deltaTime * swayMultiplier);
			AnimatorHelper obj = animatorHelper;
			float? rotationDirectionRight = Mathf.Clamp(_ySwayHelper, -1f, 1f);
			obj.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null, null, null, rotationDirectionRight);
			AnimatorHelper obj2 = animatorHelper;
			rotationDirectionRight = Mathf.Clamp(_xSwayHelper, -1f, 1f);
			obj2.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null, null, null, null, null, null, rotationDirectionRight);
		}

		private void RotateCamera(float xRotation, float yRotation)
		{
			_freeLookAngleY = Mathf.Clamp(_freeLookAngleY + yRotation, 0f - maxFreeLookRightAngle,
				maxFreeLookRightAngle);
			freeLookYTransform.rotation = Quaternion.AngleAxis(_freeLookAngleY, base.transform.up) *
			                              mouseLookXTransform.transform.rotation;
			_freeLookAngleX = Mathf.Clamp(_freeLookAngleX + xRotation,
				0f - Mathf.Max(freeLookMaxUpAngle + ((!(_angleX > 0f)) ? _angleX : 0f), 0f),
				Mathf.Max(freeLookMaxDownAngle - ((!(_angleX > 0f)) ? 0f : _angleX), 0f));
			freeLookXTransform.localRotation = Quaternion.Euler(_freeLookAngleX, 0f, 0f);
		}

		public void ToggleAutoFreeLook(bool isActive)
		{
			_autoFreeLook = isActive;
			ToggleFreeLook(_autoFreeLook);
		}

		public void ToggleFreeLook(bool isActive)
		{
			if (MyPlayer.Instance.ShipControlMode == ShipControlMode.Navigation ||
			    MyPlayer.Instance.ShipControlMode == ShipControlMode.Docking)
			{
				return;
			}

			_isFreeLook = isActive;
			if (_isFreeLook)
			{
				return;
			}

			_canMoveCamera = false;
		}

		private float CalculateAnimationValByAngle(float camAngle, float minAngle, float maxAngle)
		{
			return camAngle / ((!(camAngle < 0f)) ? maxAngle : minAngle);
		}

		public void ResetCameraPositionAndRotation()
		{
			_resetLerpTime = 0f;
			_freeLookAngleX = _freeLookAngleY = 0f;
			_freeLookAngleXZero = _freeLookAngleYZero = 0f;
			_angleX = 0f;
			mouseLookYTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			mouseLookXTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			freeLookXTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			freeLookYTransform.SetLocalPositionAndRotation(_freeLookYTransformPos, Quaternion.identity);
		}

		public void ResetLookAt(float? duration = null)
		{
			_resetLerpTime = 0f;
			_freeLookAngleX = 0f;
			_freeLookAngleY = 0f;
			_freeLookAngleXZero = 0f;
			_freeLookAngleYZero = 0f;
			_yRotationLerped = 0f;
			_xRotationLerped = 0f;
			_zRotationLerped = 0f;
			if (duration.HasValue)
			{
				_resetLerpStep = 1f / duration.Value;
			}

			ToggleFreeLook(_autoFreeLook);
		}

		public void LookAtPoint(Vector3 point)
		{
			mouseLookXTransform.localRotation = Quaternion.identity;
			freeLookYTransform.LookAt(point);
			_freeLookAngleY = ((!(freeLookYTransform.localEulerAngles.y > 180f))
				? freeLookYTransform.localEulerAngles.y
				: (freeLookYTransform.localEulerAngles.y - 360f));
			freeLookYTransform.localRotation = Quaternion.Euler(0f, _freeLookAngleY, 0f);
			freeLookXTransform.LookAt(point);
			_freeLookAngleX = ((!(freeLookXTransform.localEulerAngles.x > 180f))
				? freeLookXTransform.localEulerAngles.x
				: (freeLookXTransform.localEulerAngles.x - 360f));
			freeLookXTransform.localRotation = Quaternion.Euler(_freeLookAngleX, 0f, 0f);
			_freeLookAngleXZero = _freeLookAngleX;
			_freeLookAngleYZero = _freeLookAngleY;
		}

		public void RefreshOutfitData(Transform outfitTrans)
		{
			spineTransform = animatorHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
			if (_isCameraAttachedToHeadBone)
			{
				base.transform.parent = characterController.HeadCameraParent;
			}
		}

		public void ToggleCameraMovement(bool? isEnabled)
		{
			if (isEnabled.HasValue)
			{
				_isMovementEnabled = isEnabled.Value;
			}
			else
			{
				_isMovementEnabled = !_isMovementEnabled;
			}
		}

		public void SetLeanRightAxis(float value)
		{
			_leanRightAxis = value;
		}

		public void AddCharacterRotation(Vector3 eulerAngle)
		{
			RotateCharacter(eulerAngle.x, eulerAngle.y, eulerAngle.z);
		}

		private void OnApplicationFocus(bool focus)
		{
			if (!focus)
			{
				_canMoveCamera = !_isFreeLook;
				_isFreeLook = false;
			}
		}

		public void ToggleCameraAttachToHeadBone(bool? attachVal = null, Vector3? lockedAngle = null)
		{
			bool flag = (IsAttached = ((!attachVal.HasValue) ? (!_isCameraAttachedToHeadBone) : attachVal.Value));
			if (_isCameraAttachedToHeadBone == flag)
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
					((Transform)enumerator.Current).localPosition +=
						((!flag) ? _freeLookYTransformPos : (-_freeLookYTransformPos));
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
			_isCameraAttachedToHeadBone = flag;
		}

		public void LerpCameraControllerBack(float time, Vector3? zeroPos = null, Vector3? zeroRot = null)
		{
			_angleX = ((!zeroRot.HasValue) ? 0f : zeroRot.Value.x);
			_lerpControllerBack = true;
			_lerpControllerBackStartTime = Time.time;
			_lerpControllerBackStrength = 1f / time;
			_lerpControllerBackZeroPos = ((!zeroPos.HasValue) ? Vector3.zero : zeroPos.Value);
			_lerpControllerBackZeroRot = ((!zeroRot.HasValue) ? Quaternion.identity : Quaternion.Euler(zeroRot.Value));
			_lerpControllerBackStartPos = MouseLookXTransform.localPosition;
			_lerpControllerBackStartRot = MouseLookXTransform.localRotation;
		}

		private void LerpCameraControllerBackWorker()
		{
			float num = (Time.time - _lerpControllerBackStartTime) * _lerpControllerBackStrength;
			mouseLookXTransform.SetLocalPositionAndRotation(
				Vector3.Lerp(_lerpControllerBackStartPos, _lerpControllerBackZeroPos, num),
				Quaternion.Lerp(_lerpControllerBackStartRot, _lerpControllerBackZeroRot, num));
			if (num >= 1f)
			{
				_lerpControllerBack = false;
			}
		}

		public void SetHandsBoxCollider(BoxCollider collider)
		{
			itemInHandsCollider.center = collider.center;
			itemInHandsCollider.size = collider.size;
		}

		public void UpdateSpineTransform()
		{
			if (!_isCameraAttachedToHeadBone)
			{
				spineTransform.SetPositionAndRotation(spineFollowerTransform.position, spineFollowerTransform.rotation);
			}
		}

		public void AddCameraInertia()
		{
			InertiaCameraHolder.SetLocalPositionAndRotation(
				Vector3.Lerp(InertiaCameraHolder.localPosition, translationInertion, Time.deltaTime * lerpCoef),
				Quaternion.Slerp(InertiaCameraHolder.localRotation,
					Quaternion.Euler(new Vector3(rotationInertion.x, rotationInertion.y, 0f - rotationInertion.z)),
					Time.deltaTime * lerpCoef));
		}

		public void SetCameraInertia(Vector3? tranInertia = null, Vector3? rotInertia = null)
		{
			_doInertia = true;
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
