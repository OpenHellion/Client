using System;
using System.Collections;
using System.Linq;
using OpenHellion.IO;
using UnityEngine;
using ZeroGravity;
using ZeroGravity.LevelDesign;
using ZeroGravity.Math;
using ZeroGravity.Objects;

namespace ZeroGravity.CharacterMovement
{
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(SphereCollider))]
	[RequireComponent(typeof(Rigidbody))]
	public class MyCharacterController : MonoBehaviour
	{
		[Serializable]
		public struct MovementSpeed
		{
			public float ForwardVelocity;

			public float RightVelocity;

			public float BackwardVelocity;

			public float LeanVelocity;

			public float RotateVelocity;

			public float AccelerationStep;

			public float DeaccelerationStep;

			public float AccelerationTime
			{
				get
				{
					return 1f / AccelerationStep;
				}
				set
				{
					AccelerationStep = 1f / value;
				}
			}

			public float DeaccelerationTime
			{
				get
				{
					return 1f / DeaccelerationStep;
				}
				set
				{
					DeaccelerationStep = 1f / value;
				}
			}
		}

		private enum MovementState
		{
			Normal = 1,
			Run,
			Crouch,
			Jump
		}

		public struct MovementAxis
		{
			public float Forward;

			public float Right;

			public float LeanRight;

			public float Up;
		}

		public delegate void TranslateDelegate();

		[SerializeField]
		private CapsuleCollider collider1G;

		[SerializeField]
		private SphereCollider collider0G;

		[SerializeField]
		private Transform characterRoot;

		[SerializeField]
		public Rigidbody rigidBody;

		[SerializeField]
		private MyPlayer myPlayer;

		[SerializeField]
		public AnimatorHelper animatorHelper;

		[SerializeField]
		private CameraController cameraController;

		[SerializeField]
		private Camera mainCamera;

		[SerializeField]
		private Camera farCamera;

		[SerializeField]
		private Camera nearCamera;

		public Transform HeadCameraParent;

		public ParticleSystem StarDustParticle;

		public Transform StarDustCustomSpace;

		[SerializeField]
		private MovementSpeed normalSpeeds;

		[SerializeField]
		private MovementSpeed runSpeeds;

		[SerializeField]
		private MovementSpeed crouchSpeeds;

		[SerializeField]
		private MovementSpeed airSpeeds;

		[SerializeField]
		private MovementSpeed zeroGNormalSpeeds;

		[SerializeField]
		private MovementSpeed zeroGFloatingSpeeds;

		[SerializeField]
		private MovementSpeed zeroGJetpackSpeeds;

		[SerializeField]
		private MovementSpeed zeroGGlideSpeeds;

		private MovementSpeed currSpeeds;

		[SerializeField]
		private float animatorForwardMaxVelocity;

		[SerializeField]
		private float animatorBackwardMaxVelocity;

		[SerializeField]
		private float animatorRightMaxVelocity;

		[SerializeField]
		public Rigidbody ragdollChestRigidbody;

		[SerializeField]
		private Transform centerOfMass;

		public Rigidbody CenterOfMassRigidbody;

		[SerializeField]
		private float maxSlopeAngle;

		private float currSlopeAngle;

		private Vector3 currSlopeNormal;

		private bool isZeroG;

		private bool isMovementEnabled = true;

		[SerializeField]
		private bool isGrounded;

		private float groundCheckDistance = 0.01f;

		private bool isOnLadder;

		[SerializeField]
		private float ladderVelocity = 1.5f;

		public Jetpack CurrentJetpack;

		private bool gravityChanged;

		private bool gravityChangedRagdoll;

		private float gravitChangeStartTime;

		private Quaternion gravityChangeStartRotation = Quaternion.identity;

		private Quaternion gravityChangeEndingRotation = Quaternion.identity;

		private float gravityChangeLerpHelper;

		private float gravityChangeRagdollTimer;

		private float gravityChangeRagdollTimeMax = 5f;

		private float normalColliderHeight;

		private float crouchColliderHeight = 0.8f;

		private float normalColliderCenter = -0.44f;

		private float crouchColliderCenter = -0.935f;

		private float crouchLerpHelper = 1f;

		[SerializeField]
		private float crouchLerpSpeed = 1.5f;

		[SerializeField]
		private float zgAtmosphereAngularDrag = 1.5f;

		[SerializeField]
		private float zgAtmosphereDrag = 0.1f;

		[SerializeField]
		private float zgJetpackAngularDrag = 3f;

		[SerializeField]
		private float zgGrabDrag = 2.2f;

		[SerializeField]
		private float zgGrabAngularDrag = 1.5f;

		[SerializeField]
		private float jumpHeightMax = 0.8f;

		private Vector3 currMovementDirection;

		private MovementAxis movementAxis;

		private MovementState lastMovementState;

		private float currForwardVelocity;

		private float currRightVelocity;

		private float currUpVelocity;

		private float stanceSpeedMultiplier = 1f;

		private float currForwardAnimationVal;

		private float currRightAnimationVal;

		private float currForwardFloatingAnimationVal;

		private float currRightFloatingAnimationVal;

		private float headBobStrength = 1f;

		private bool canLockToPoint;

		private bool isLockedToPoint;

		private bool canGrabWall;

		private bool lerpCameraBack;

		private float lerpCameraBackStartTime;

		private float lerpCameraBackStep = 0.3333f;

		private Quaternion lerpCameraBackZeroRotation = Quaternion.identity;

		private Vector3 lerpCameraBackStartPos = Vector3.zero;

		private Quaternion lerpCameraBackStartRot = Quaternion.identity;

		public bool grabSlowEnabled;

		private float translateLerpHelper;

		public bool HasTumbled;

		public SpaceObjectVessel NearbyVessel;

		private int collisionLayerMask;

		[SerializeField]
		private float playerImpactVelocityTreshold;

		public float DefaultMaxAngularVelocity = 20f;

		public SceneTriggerLadder LockedToLadder;

		private Vector3 TriggerHelperCenter;

		private float lastImpactTime;

		public GlassPostEffect HelmetGlassEffect;

		public SpaceObjectVessel StickToVessel;

		public Quaternion StickToVesselRotation;

		public Vector3 StickToVesselTangentialVelocity;

		public Vector3 startLockedPos;

		public float legDistance = 1.15f;

		private float sprintTime;

		private RaycastHit groundRayHit = default(RaycastHit);

		private bool doJump;

		private float airTime;

		private float slopeJumpTimer;

		private bool playingStabilizeSound;

		private float lockLerp;

		private float lockSpeed = 0.5f;

		public Transform attachPointSaved;

		public float MaxAngularVelocity { get; private set; }

		public bool IsGrounded => isGrounded;

		public float HeadBobStrength
		{
			get
			{
				return headBobStrength;
			}
			set
			{
				headBobStrength = value;
				AnimatorHelper obj = animatorHelper;
				float? num = headBobStrength;
				obj.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, num);
			}
		}

		public Camera MainCamera => mainCamera;

		public Camera FarCamera => farCamera;

		public Camera NearCamera => nearCamera;

		public Vector3 CameraPosition => mainCamera.transform.position;

		public Vector3 CameraForward => mainCamera.transform.forward;

		public Vector2 FreeLookAngle
		{
			get
			{
				float num = (Quaternion.FromToRotation(cameraController.MouseLookXTransform.forward, cameraController.FreeLookXTransform.forward).Inverse() * cameraController.transform.rotation).eulerAngles.x;
				float num2 = MathHelper.AngleSigned(Vector3.ProjectOnPlane(cameraController.FreeLookXTransform.forward, cameraController.MouseLookXTransform.up).normalized, cameraController.MouseLookXTransform.forward, cameraController.MouseLookXTransform.up);
				if (num > 180f)
				{
					num -= 360f;
				}
				if (num2 > 180f)
				{
					num2 -= 360f;
				}
				return new Vector2((0f - num) / 90f, num2 / 90f);
			}
		}

		public float MouseLookXAngle
		{
			get
			{
				float num = cameraController.MouseLookXTransform.localRotation.eulerAngles.x;
				if (num > 180f)
				{
					num -= 360f;
				}
				return 0f - num;
			}
		}

		public bool IsAttached => cameraController.IsAttached;

		public bool IsJump => lastMovementState == MovementState.Jump;

		public bool IsCrouch => lastMovementState == MovementState.Crouch;

		public bool IsZeroG => isZeroG;

		public bool IsFreeLook => cameraController.IsFreeLook;

		public float AnimationForward => (!isZeroG) ? currForwardAnimationVal : 0f;

		public float AnimationRight => (!isZeroG) ? currRightAnimationVal : 0f;

		public float AnimationZeroGForward => (!isZeroG) ? 0f : currForwardAnimationVal;

		public float AnimationZeroGRight => (!isZeroG) ? 0f : currRightAnimationVal;

		public bool MeleeTriggered { get; set; }

		public bool UseConsumableTriggered { get; set; }

		public Vector3 Velocity => rigidBody.velocity;

		public bool IsEquippingAnimationTriggered { get; set; }

		public float MouseUpAxis => cameraController.MouseUpAxis;

		public float MouseRightAxis => cameraController.MouseRightAxis;

		public float CameraMaxUpAngle => cameraController.MaxUpAngle;

		public float CameraMaxDownAngle => cameraController.MaxDownAngle;

		public float CameraMaxRightAngle => cameraController.MaxRightAngle;

		public bool IsOnLadder => isOnLadder;

		public bool IsJetpackOn
		{
			get
			{
				return CurrentJetpack != null && CurrentJetpack.IsActive;
			}
			set
			{
				if (CurrentJetpack != null)
				{
					CurrentJetpack.IsActive = value;
				}
			}
		}

		public float JetpackMaxFuel => (!(CurrentJetpack != null)) ? 0f : CurrentJetpack.MaxFuel;

		public float JetpackFuel => (!(CurrentJetpack != null)) ? 0f : CurrentJetpack.CurrentFuel;

		public bool CanGrabWall => canGrabWall;

		public bool CanLockToPoint
		{
			get
			{
				return canLockToPoint;
			}
			set
			{
				canLockToPoint = value;
			}
		}

		public bool IsLockedToPoint
		{
			get
			{
				return isLockedToPoint;
			}
			set
			{
				isLockedToPoint = value;
			}
		}

		public MovementSpeed CurrentSpeeds => currSpeeds;

		public Transform MouseLookXTransform => cameraController.MouseLookXTransform;

		public CameraController CameraController => cameraController;

		public bool IsMovementEnabled => isMovementEnabled;

		public float AirTime => airTime;

		public float StanceSpeedMultiplier => stanceSpeedMultiplier;

		private void Awake()
		{
			collisionLayerMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("PlayerCollision"));
			currSpeeds = normalSpeeds;
			lastMovementState = MovementState.Normal;
			normalColliderHeight = collider1G.height;
			collider0G.enabled = false;
			RefreshMaxAngularVelocity();
		}

		private void Update()
		{
			if (isLockedToPoint)
			{
				LockPlayerToPoint(locked: true);
			}
			if (grabSlowEnabled)
			{
				CheckVelocityForLock();
			}
			if (!isZeroG)
			{
				update1GInput();
			}
			if (isZeroG)
			{
				CheckLegDistanceFromFloor();
			}
			if (lerpCameraBack)
			{
				LerpCameraToLocalZero();
			}
			if (myPlayer.Parent is Pivot && rigidBody.drag.IsNotEpsilonZero() && NearbyVessel != null)
			{
				Pivot pivot = myPlayer.Parent as Pivot;
				rigidBody.velocity -= ((pivot.Velocity - NearbyVessel.Velocity) * Time.deltaTime).ToVector3();
			}
		}

		private void FixedUpdate()
		{
			if (Client.IsGameBuild && !myPlayer.PlayerReady)
			{
				return;
			}
			if (isZeroG)
			{
				update0GMovement();
			}
			else
			{
				if (!isGrounded && !HasTumbled)
				{
					float num = Vector3.Dot(Vector3.Project(rigidBody.velocity, myPlayer.GravityDirection), myPlayer.GravityDirection);
					if (num > 7f)
					{
						Tumble(rigidBody.velocity);
					}
				}
				update1GMovement();
			}
			if (InputManager.GetButton(InputManager.ConfigAction.Sprint))
			{
				canGrabWall = Physics.OverlapSphere(centerOfMass.position, 0.8f, collisionLayerMask, QueryTriggerInteraction.Ignore).Length > 0;
			}
		}

		private void CheckLegDistanceFromFloor()
		{
			bool? touchingFloor = Physics.Raycast(base.transform.position, -base.transform.up, legDistance, collisionLayerMask);
			animatorHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, touchingFloor);
		}

		private void CalculateCrouch()
		{
			if (!IsCrouch && collider1G.height < normalColliderHeight && Physics.SphereCast(characterRoot.position, collider1G.radius - 0.02f, characterRoot.up, out var _, normalColliderHeight - collider1G.radius, collisionLayerMask))
			{
				lastMovementState = MovementState.Crouch;
			}
			float num = crouchLerpHelper;
			if (IsCrouch && num > 0f)
			{
				num = Mathf.Max(num - Time.deltaTime * crouchLerpSpeed, 0f);
			}
			else if (!IsCrouch && num < 1f)
			{
				Physics.SphereCast(characterRoot.position, collider1G.radius - 0.02f, characterRoot.up, out var hitInfo2, normalColliderHeight - collider1G.radius, collisionLayerMask);
				if (hitInfo2.transform == null)
				{
					num = Mathf.Min(num + Time.deltaTime * crouchLerpSpeed, 1f);
				}
				else
				{
					lastMovementState = MovementState.Crouch;
					if (num > 0f)
					{
						num = Mathf.Max(num - Time.deltaTime * crouchLerpSpeed, 0f);
					}
				}
			}
			if (crouchLerpHelper != num)
			{
				crouchLerpHelper = num;
				collider1G.height = Mathf.Lerp(crouchColliderHeight, normalColliderHeight, crouchLerpHelper);
				collider1G.center = new Vector3(0f, Mathf.Lerp(crouchColliderCenter, normalColliderCenter, crouchLerpHelper), 0f);
				currSpeeds.AccelerationStep = Mathf.Lerp(crouchSpeeds.AccelerationStep, normalSpeeds.AccelerationStep, crouchLerpHelper);
				currSpeeds.DeaccelerationStep = Mathf.Lerp(crouchSpeeds.DeaccelerationStep, normalSpeeds.DeaccelerationStep, crouchLerpHelper);
				currSpeeds.ForwardVelocity = Mathf.Lerp(crouchSpeeds.ForwardVelocity, normalSpeeds.ForwardVelocity, crouchLerpHelper);
				currSpeeds.BackwardVelocity = Mathf.Lerp(crouchSpeeds.BackwardVelocity, normalSpeeds.BackwardVelocity, crouchLerpHelper);
				currSpeeds.RightVelocity = Mathf.Lerp(crouchSpeeds.RightVelocity, normalSpeeds.RightVelocity, crouchLerpHelper);
				if (crouchLerpHelper < 0.7f && myPlayer.MeshRenderersEnabled)
				{
					myPlayer.ToggleMeshRendereres(enableMesh: false);
				}
				else if (crouchLerpHelper >= 0.7f && !myPlayer.MeshRenderersEnabled)
				{
					myPlayer.ToggleMeshRendereres(enableMesh: true);
				}
			}
			animatorHelper.SetParameter(IsCrouch);
		}

		private static void RecalculateAxisValue(ref float currAxisVal, ref float currVelocity, ref float positiveVelocityMax, ref float negativeVelocityMax)
		{
			if (currAxisVal > 0.0001f)
			{
				currAxisVal = currVelocity / positiveVelocityMax;
			}
			else if (currAxisVal < -0.0001f)
			{
				currAxisVal = currVelocity / negativeVelocityMax;
			}
		}

		private static void SetAxisValue(ref float currAxisVal, float inputAxisVal, ref float accelerationStep, ref float deaccelerationStep)
		{
			if (currAxisVal > 1f)
			{
				currAxisVal -= deaccelerationStep * Time.fixedDeltaTime;
				return;
			}
			if (currAxisVal < -1f)
			{
				currAxisVal += deaccelerationStep * Time.fixedDeltaTime;
				return;
			}
			if (inputAxisVal > 0.0001f)
			{
				if (currAxisVal > -0.0001f)
				{
					currAxisVal += accelerationStep * Time.fixedDeltaTime;
				}
				else
				{
					currAxisVal += deaccelerationStep * Time.fixedDeltaTime;
				}
			}
			else if (inputAxisVal < -0.0001f)
			{
				if (currAxisVal < 0.0001f)
				{
					currAxisVal -= accelerationStep * Time.fixedDeltaTime;
				}
				else
				{
					currAxisVal -= deaccelerationStep * Time.fixedDeltaTime;
				}
			}
			else if (currAxisVal > 0.0001f)
			{
				currAxisVal = Mathf.Max(currAxisVal - deaccelerationStep * Time.fixedDeltaTime, 0f);
			}
			else if (currAxisVal < -0.0001f)
			{
				currAxisVal = Mathf.Min(currAxisVal + deaccelerationStep * Time.fixedDeltaTime, 0f);
			}
			else
			{
				currAxisVal = 0f;
			}
			currAxisVal = Mathf.Clamp(currAxisVal, -1f, 1f);
		}

		private void Calculate1GMovementData()
		{
			if (InputManager.GetButton(InputManager.ConfigAction.Sprint) && !IsCrouch && animatorHelper.CanRun)
			{
				if (lastMovementState != MovementState.Run)
				{
					RecalculateAxisValue(ref movementAxis.Forward, ref currForwardVelocity, ref runSpeeds.ForwardVelocity, ref runSpeeds.BackwardVelocity);
					RecalculateAxisValue(ref movementAxis.Right, ref currRightVelocity, ref runSpeeds.RightVelocity, ref runSpeeds.RightVelocity);
				}
				currSpeeds = runSpeeds;
				lastMovementState = MovementState.Run;
				sprintTime += Time.deltaTime;
				myPlayer.HealthSounds.Switch("WalkRun", "Run");
			}
			else if (InputManager.GetButton(InputManager.ConfigAction.Crouch))
			{
				if (lastMovementState != MovementState.Crouch)
				{
					RecalculateAxisValue(ref movementAxis.Forward, ref currForwardVelocity, ref crouchSpeeds.ForwardVelocity, ref crouchSpeeds.BackwardVelocity);
					RecalculateAxisValue(ref movementAxis.Right, ref currRightVelocity, ref crouchSpeeds.RightVelocity, ref crouchSpeeds.RightVelocity);
				}
				currSpeeds = crouchSpeeds;
				lastMovementState = MovementState.Crouch;
				myPlayer.HealthSounds.Switch("WalkRun", "Walk");
			}
			else
			{
				if (lastMovementState != MovementState.Normal)
				{
					RecalculateAxisValue(ref movementAxis.Forward, ref currForwardVelocity, ref normalSpeeds.ForwardVelocity, ref normalSpeeds.BackwardVelocity);
					RecalculateAxisValue(ref movementAxis.Right, ref currRightVelocity, ref normalSpeeds.RightVelocity, ref normalSpeeds.RightVelocity);
				}
				currSpeeds = normalSpeeds;
				lastMovementState = MovementState.Normal;
				sprintTime = 0f;
				myPlayer.HealthSounds.Switch("WalkRun", "Walk");
			}
			movementAxis.Forward = InputManager.GetAxisRaw(InputManager.ConfigAction.Forward);
			movementAxis.Right = InputManager.GetAxisRaw(InputManager.ConfigAction.Right);
			movementAxis.Forward *= stanceSpeedMultiplier;
			movementAxis.Right *= stanceSpeedMultiplier;
			if (movementAxis.Forward != 0f && movementAxis.Right != 0f)
			{
				currSpeeds.ForwardVelocity -= currSpeeds.ForwardVelocity * Mathf.Abs(movementAxis.Right) * 0.25f;
				currSpeeds.RightVelocity -= currSpeeds.RightVelocity * Mathf.Abs(movementAxis.Forward) * 0.25f;
				currSpeeds.BackwardVelocity -= currSpeeds.BackwardVelocity * Mathf.Abs(movementAxis.Right) * 0.25f;
			}
			currMovementDirection = movementAxis.Forward * base.transform.forward;
			currMovementDirection += movementAxis.Right * base.transform.right;
			currMovementDirection.Normalize();
		}

		private void update1GInput()
		{
			if (isOnLadder)
			{
				return;
			}
			if (gravityChanged)
			{
				if (gravityChangedRagdoll)
				{
					isGrounded = Physics.Raycast(ragdollChestRigidbody.position, myPlayer.GravityDirection, out groundRayHit, 0.4f, collisionLayerMask, QueryTriggerInteraction.Ignore);
				}
				else
				{
					isGrounded = Physics.Raycast(base.transform.position, myPlayer.GravityDirection, out groundRayHit, 1.8f, collisionLayerMask, QueryTriggerInteraction.Ignore);
				}
				if (CurrentJetpack != null)
				{
					CurrentJetpack.StartNozzles(Vector4.zero, myJetpack: true);
				}
			}
			else
			{
				isGrounded = Physics.SphereCast(characterRoot.position + characterRoot.up * (collider1G.height / 2f), collider1G.radius - 0.01f, myPlayer.GravityDirection, out groundRayHit, collider1G.height / 2f - (collider1G.radius - 0.01f) + groundCheckDistance + 0.02f, collisionLayerMask);
			}
			if (!isGrounded)
			{
				airTime += Time.deltaTime;
			}
			else if (isGrounded)
			{
				airTime = 0f;
			}
			AnimatorHelper obj = animatorHelper;
			bool? flag = isGrounded;
			float? num = airTime;
			obj.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flag, null, null, null, null, null, null, null, null, num);
			if (isMovementEnabled && crouchLerpHelper.IsEpsilonEqual(1f, 0.001f) && isGrounded && InputManager.GetButtonDown(InputManager.ConfigAction.Jump))
			{
				doJump = true;
			}
		}

		private void update1GMovement()
		{
			cameraController.SetLeanRightAxis(0f);
			if (isOnLadder)
			{
				float axisRaw = InputManager.GetAxisRaw(InputManager.ConfigAction.Forward);
				base.transform.Translate(base.transform.up * (axisRaw * ladderVelocity * Time.fixedDeltaTime), Space.World);
				AnimatorHelper animHelper = myPlayer.animHelper;
				float? ladderDirection = axisRaw;
				animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, ladderDirection);
				return;
			}
			if (gravityChanged)
			{
				if (!gravityChangedRagdoll)
				{
					gravityChangeLerpHelper += Time.fixedDeltaTime * 2f;
					if (gravityChangeLerpHelper > 1f)
					{
						gravityChangeLerpHelper = 1f;
					}
					base.transform.rotation = gravityChangeEndingRotation;
					if (collider1G.height < 1.8f)
					{
						collider1G.height = Mathf.Lerp(1f, 1.8f, gravityChangeLerpHelper);
						collider1G.center = new Vector3(0f, Mathf.Lerp(0f, -0.44f, gravityChangeLerpHelper), 0f);
					}
				}
				else
				{
					if (gravityChangeLerpHelper < 0.01f)
					{
						gravityChangeRagdollTimer += Time.fixedDeltaTime;
						if (gravityChangeRagdollTimer >= gravityChangeRagdollTimeMax)
						{
							gravityChangeLerpHelper += Time.fixedDeltaTime * 2f;
						}
					}
					if (!ragdollChestRigidbody.velocity.sqrMagnitude.IsNotEpsilonZero(0.001f) || gravityChangeLerpHelper.IsNotEpsilonZero(0.001f))
					{
						gravityChangeLerpHelper += Time.fixedDeltaTime * 2f;
					}
				}
			}
			if (gravityChanged)
			{
				if (!gravityChangedRagdoll && isGrounded && gravityChangeLerpHelper >= 1f)
				{
					gravityChanged = false;
					cameraController.ToggleCameraMovement(true);
					gravityChangeLerpHelper = 0f;
					base.transform.rotation = gravityChangeEndingRotation;
					cameraController.ResetLookAt();
					return;
				}
				if (gravityChangedRagdoll && gravityChangeLerpHelper >= 1f)
				{
					myPlayer.ToggleRagdoll(false);
					gravityChangeLerpHelper = 0f;
					gravityChanged = false;
					HasTumbled = false;
					return;
				}
			}
			if (isGrounded)
			{
				if (gravityChanged || !isMovementEnabled)
				{
					return;
				}
				currSlopeNormal = groundRayHit.normal;
				currSlopeAngle = Vector3.Angle(currSlopeNormal, base.transform.up);
				if (currSlopeAngle > 5f)
				{
					bool flag = false;
					RaycastHit[] array = Physics.SphereCastAll(characterRoot.position + characterRoot.up * (collider1G.height / 2f), collider1G.radius - 0.01f, myPlayer.GravityDirection, collider1G.height / 2f - (collider1G.radius - 0.01f) + groundCheckDistance + 0.02f, collisionLayerMask);
					if (array != null && array.Length > 0)
					{
						for (int i = 0; i < array.Length; i++)
						{
							if (array[i].collider.GetComponent<DynamicObject>() == null && array[i].collider.GetComponent<MyPlayer>() == null && array[i].collider.GetComponent<RagdollCollider>() == null)
							{
								flag = false;
								groundRayHit = array[i];
								break;
							}
							if (array[i].collider.GetComponent<RagdollCollider>() != null)
							{
								flag = true;
							}
						}
					}
					if (flag)
					{
						currSlopeNormal = base.transform.up;
						currSlopeAngle = 0f;
					}
					else
					{
						currSlopeNormal = groundRayHit.normal;
						currSlopeAngle = Vector3.Angle(currSlopeNormal, base.transform.up);
					}
				}
				bool? isMoving = rigidBody.velocity.magnitude > float.Epsilon;
				animatorHelper.SetParameter(null, isMoving);
				if (InputManager.GetButton(InputManager.ConfigAction.Crouch))
				{
					lastMovementState = MovementState.Crouch;
				}
				CalculateCrouch();
				if (currSlopeAngle < maxSlopeAngle)
				{
					if (slopeJumpTimer != 0f)
					{
						slopeJumpTimer = 0f;
					}
					if (doJump && !IsCrouch)
					{
						float num = Mathf.Sqrt(2f * jumpHeightMax * myPlayer.Gravity.magnitude);
						rigidBody.AddForce(base.transform.up * num, ForceMode.VelocityChange);
						lastMovementState = MovementState.Jump;
						animatorHelper.SetParameterTrigger(AnimatorHelper.Triggers.Jump);
						doJump = false;
						isGrounded = false;
					}
					else
					{
						Calculate1GMovementData();
					}
					if (currSlopeAngle > 1f)
					{
						currMovementDirection = Vector3.ProjectOnPlane(currMovementDirection, currSlopeNormal);
					}
					currForwardVelocity = movementAxis.Forward * ((!(movementAxis.Forward > 0f)) ? currSpeeds.BackwardVelocity : currSpeeds.ForwardVelocity);
					currRightVelocity = movementAxis.Right * currSpeeds.RightVelocity;
					if (isGrounded)
					{
						if (Mathf.Abs(currForwardVelocity) > float.Epsilon || Mathf.Abs(currRightVelocity) > float.Epsilon)
						{
							rigidBody.AddForce(currMovementDirection * Mathf.Sqrt(currForwardVelocity * currForwardVelocity + currRightVelocity * currRightVelocity) - rigidBody.velocity, ForceMode.VelocityChange);
						}
						else
						{
							rigidBody.AddForce(currMovementDirection * (currForwardVelocity + currRightVelocity) - rigidBody.velocity, ForceMode.VelocityChange);
						}
					}
					currForwardAnimationVal = ((!(Mathf.Abs(movementAxis.Forward) <= float.Epsilon)) ? Mathf.Clamp(Vector3.Project(myPlayer.MyVelocity, base.transform.forward).magnitude / runSpeeds.ForwardVelocity * (float)MathHelper.Sign(movementAxis.Forward), -1f, 1f) : 0f);
					currRightAnimationVal = ((!(Mathf.Abs(movementAxis.Right) <= float.Epsilon)) ? Mathf.Clamp(Vector3.Project(myPlayer.MyVelocity, base.transform.right).magnitude / runSpeeds.RightVelocity * (float)MathHelper.Sign(movementAxis.Right), -1f, 1f) : 0f);
					float? ladderDirection = currForwardAnimationVal;
					float? velocityRight = currRightAnimationVal;
					animatorHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, velocityRight, ladderDirection);
				}
				else
				{
					slopeJumpTimer += Time.deltaTime;
					movementAxis.Forward = 0f;
					movementAxis.Right = 0f;
					currForwardAnimationVal = 0f;
					currRightAnimationVal = 0f;
					float? ladderDirection = currForwardAnimationVal;
					float? velocityRight = currRightAnimationVal;
					animatorHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, velocityRight, ladderDirection);
					if (slopeJumpTimer > 1f && isGrounded && InputManager.GetButtonDown(InputManager.ConfigAction.Jump))
					{
						float num2 = Mathf.Sqrt(2f * jumpHeightMax * myPlayer.Gravity.magnitude);
						rigidBody.AddForce(base.transform.up * num2, ForceMode.VelocityChange);
						lastMovementState = MovementState.Jump;
						animatorHelper.SetParameterTrigger(AnimatorHelper.Triggers.Jump);
						doJump = false;
						isGrounded = false;
						slopeJumpTimer = 0f;
					}
				}
			}
			else if (isMovementEnabled)
			{
				float axisRaw2 = InputManager.GetAxisRaw(InputManager.ConfigAction.Forward);
				float axisRaw3 = InputManager.GetAxisRaw(InputManager.ConfigAction.Right);
				if (axisRaw2 != 0f && Vector3.Project(rigidBody.velocity, base.transform.forward).sqrMagnitude < ((!(movementAxis.Forward > 0f)) ? (airSpeeds.BackwardVelocity * airSpeeds.BackwardVelocity) : (airSpeeds.ForwardVelocity * airSpeeds.ForwardVelocity)))
				{
					Vector3 force = base.transform.forward * ((!(axisRaw2 > 0f)) ? (0f - airSpeeds.BackwardVelocity) : airSpeeds.ForwardVelocity);
					rigidBody.AddForce(force, ForceMode.VelocityChange);
				}
				if (axisRaw3 != 0f && Vector3.Project(rigidBody.velocity, base.transform.right).sqrMagnitude < airSpeeds.RightVelocity * airSpeeds.RightVelocity)
				{
					Vector3 force2 = base.transform.right * airSpeeds.RightVelocity * 0.005f;
					rigidBody.AddForce(force2, ForceMode.VelocityChange);
				}
				movementAxis.Forward = Mathf.Lerp(movementAxis.Forward, 0f, 0.3f * Time.fixedDeltaTime);
				movementAxis.Right = Mathf.Lerp(movementAxis.Right, 0f, 0.3f * Time.fixedDeltaTime);
			}
			if (!isGrounded || currSlopeAngle >= maxSlopeAngle)
			{
				if (!isMovementEnabled && myPlayer.IsLockedToTrigger)
				{
					rigidBody.velocity = Vector3.zero;
					rigidBody.angularVelocity = Vector3.zero;
				}
				else
				{
					rigidBody.AddForce(myPlayer.Gravity, ForceMode.Acceleration);
				}
			}
		}

		private void Check0GCurrentSpeed()
		{
			if (IsJetpackOn && JetpackFuel > float.Epsilon)
			{
				currSpeeds = zeroGJetpackSpeeds;
			}
			else if (canGrabWall)
			{
				currSpeeds = zeroGNormalSpeeds;
			}
			else
			{
				currSpeeds = zeroGNormalSpeeds;
			}
		}

		/// <summary>
		/// 	Calculates movement velocities.
		/// </summary>
		private void Calculate0GMovementData()
		{
			// Get normal input.
			movementAxis.Forward = InputManager.GetAxis(InputManager.ConfigAction.Forward);
			movementAxis.Right = InputManager.GetAxis(InputManager.ConfigAction.Right);
			movementAxis.LeanRight = InputManager.GetAxis(InputManager.ConfigAction.Lean);

			// Get vertical input.
			if (InputManager.GetButton(InputManager.ConfigAction.Jump))
			{
				movementAxis.Up = Mathf.Min(movementAxis.Up + Time.fixedDeltaTime * 4f, 1f);
			}
			else if (InputManager.GetButton(InputManager.ConfigAction.Crouch))
			{
				movementAxis.Up = Mathf.Max(movementAxis.Up - Time.fixedDeltaTime * 4f, -1f);
			}
			else if (movementAxis.Up < 0f)
			{
				movementAxis.Up = Mathf.Min(movementAxis.Up + Time.fixedDeltaTime * 4f, 0f);
			}
			else
			{
				movementAxis.Up = Mathf.Max(movementAxis.Up - Time.fixedDeltaTime * 4f, 0f);
			}

			// Sound and VFX.
			if (CurrentJetpack != null && CurrentJetpack.IsActive && JetpackFuel > float.Epsilon)
			{
				CurrentJetpack.StartNozzles(new Vector4(movementAxis.Right, movementAxis.Up, movementAxis.Forward, movementAxis.LeanRight), myJetpack: true);
			}

			Check0GCurrentSpeed();

			// Calculate final velocity if numbers are not epsilon zero.
			if (!movementAxis.Forward.IsNotEpsilonZero() || !movementAxis.Right.IsNotEpsilonZero() || !movementAxis.Up.IsNotEpsilonZero())
			{
				float a = Mathf.Abs(movementAxis.Forward) * currSpeeds.ForwardVelocity;
				float b = Mathf.Abs(movementAxis.Right) * currSpeeds.RightVelocity;
				float c = Mathf.Abs(movementAxis.Up) * currSpeeds.RightVelocity;
				float num2 = MathHelper.AverageMaxValue(a, b, c, currSpeeds.ForwardVelocity, currSpeeds.RightVelocity, currSpeeds.RightVelocity);
				currSpeeds.ForwardVelocity = num2;
				currSpeeds.RightVelocity = num2;
				currSpeeds.BackwardVelocity = num2;
			}
			myPlayer.HealthSounds.Switch("WalkRun", "Walk");
		}

		private void Calculate0GAnimatorParams()
		{
			Vector3 vector = Vector3.Project(myPlayer.MyVelocity, base.transform.forward);
			Vector3 vector2 = Vector3.Project(myPlayer.MyVelocity, base.transform.right);
			float f = Vector3.Dot(vector2.normalized, base.transform.right);
			float num3 = Vector3.Dot(vector.normalized, base.transform.forward);
			float num = Mathf.Min(MathHelper.ProportionalValue(vector.magnitude, 0f, (!(num3 > 1f)) ? animatorBackwardMaxVelocity : animatorForwardMaxVelocity, 0f, 1f), 1f) * Mathf.Sign(num3);
			float num2 = Mathf.Min(MathHelper.ProportionalValue(vector2.magnitude, 0f, animatorRightMaxVelocity, 0f, 1f), 1f) * Mathf.Sign(f);
			if (!Mathf.Approximately(currForwardAnimationVal, num))
			{
				currForwardAnimationVal = Mathf.Lerp(currForwardAnimationVal, num, Time.fixedDeltaTime * animatorForwardMaxVelocity);
			}
			if (!Mathf.Approximately(currRightAnimationVal, num2))
			{
				currRightAnimationVal = Mathf.Lerp(currRightAnimationVal, num2, Time.fixedDeltaTime * animatorRightMaxVelocity);
			}
			float? velocityForward = currForwardAnimationVal;
			float? velocityRight = currRightAnimationVal;
			animatorHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, velocityRight, velocityForward);
			float forward = movementAxis.Forward;
			float right = movementAxis.Right;
			AnimatorHelper animHelper = myPlayer.animHelper;
			bool? isMovingZeroG = forward.IsNotEpsilonZero() || right.IsNotEpsilonZero();
			animHelper.SetParameter(null, null, null, isMovingZeroG);
			if (!Mathf.Approximately(currForwardFloatingAnimationVal, forward))
			{
				currForwardFloatingAnimationVal = Mathf.Lerp(currForwardFloatingAnimationVal, forward, Time.fixedDeltaTime * animatorForwardMaxVelocity);
			}
			if (!Mathf.Approximately(currRightFloatingAnimationVal, right))
			{
				currRightFloatingAnimationVal = Mathf.Lerp(currRightFloatingAnimationVal, right, Time.fixedDeltaTime * animatorRightMaxVelocity);
			}
			AnimatorHelper animHelper2 = myPlayer.animHelper;
			velocityForward = currForwardFloatingAnimationVal;
			velocityRight = currRightFloatingAnimationVal;
			animHelper2.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, velocityForward, velocityRight);
		}

		private void update0GMovement()
		{
			if (isOnLadder)
			{
				float axisRaw = InputManager.GetAxisRaw(InputManager.ConfigAction.Forward);
				base.transform.Translate(base.transform.up * (axisRaw * ladderVelocity * Time.fixedDeltaTime), Space.World);
				AnimatorHelper animHelper = myPlayer.animHelper;
				float? ladderDirection = axisRaw;
				animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, ladderDirection);
			}
			else if (gravityChanged && gravityChangedRagdoll)
			{
				gravityChangeLerpHelper += Time.fixedDeltaTime * 2f;
				if (gravityChangedRagdoll && gravityChangeLerpHelper >= 1f)
				{
					myPlayer.ToggleRagdoll(false);
					gravityChangeLerpHelper = 0f;
					gravityChanged = false;
				}
			}
			else
			{
				if (!isMovementEnabled)
				{
					return;
				}
				Calculate0GMovementData();
				cameraController.SetLeanRightAxis(0f - movementAxis.LeanRight);
				bool hasToStabilise = InputManager.GetButton(InputManager.ConfigAction.Sprint) || (myPlayer.IsUsingItemInHands && myPlayer.Inventory.CheckIfItemInHandsIsType<HandDrill>());
				if (hasToStabilise && canGrabWall && NearbyVessel != null && (NearbyVessel.Velocity - (myPlayer.Parent.Velocity + rigidBody.velocity.ToVector3D())).SqrMagnitude < 100.0)
				{
					if (myPlayer.CurrentRoomTrigger == null)
					{
						if (StickToVessel == null)
						{
							StickToVessel = NearbyVessel;
							StickToVesselRotation = NearbyVessel.transform.rotation;
						}
						if (StickToVessel != null)
						{
							rigidBody.velocity = Vector3.Lerp(rigidBody.velocity, (StickToVessel.Velocity - myPlayer.Parent.Velocity).ToVector3(), 0.1f);
						}
					}
					else
					{
						StickToVessel = null;
						rigidBody.drag = zgGrabDrag;
						rigidBody.velocity *= 0.9f;
					}
				}
				else if (myPlayer.IsInsideSpaceObject && myPlayer.CurrentRoomTrigger != null && myPlayer.CurrentRoomTrigger.AirPressure > 0.5f)
				{
					StickToVessel = null;
					StickToVesselTangentialVelocity = Vector3.zero;
					StickToVesselRotation = Quaternion.identity;
					rigidBody.drag = zgAtmosphereDrag;
				}
				else
				{
					if (StickToVessel != null)
					{
						rigidBody.velocity = StickToVesselTangentialVelocity;
					}
					StickToVessel = null;
					StickToVesselTangentialVelocity = Vector3.zero;
					StickToVesselRotation = Quaternion.identity;
					rigidBody.drag = 0f;
				}

				// Handle rotation.
				if (IsJetpackOn)
				{
					rigidBody.angularDrag = zgJetpackAngularDrag;
				}
				else if (hasToStabilise && canGrabWall)
				{
					rigidBody.angularDrag = zgGrabAngularDrag;
				}
				else if (myPlayer.IsInsideSpaceObject && myPlayer.CurrentRoomTrigger != null && myPlayer.CurrentRoomTrigger.AirPressure > 0.5f)
				{
					rigidBody.angularDrag = zgAtmosphereAngularDrag;
				}
				else
				{
					rigidBody.angularDrag = 0f;
				}

				if (movementAxis.Right.IsNotEpsilonZero() || movementAxis.Forward.IsNotEpsilonZero() || movementAxis.Up.IsNotEpsilonZero())
				{
					Vector3 vector = cameraController.transform.forward * movementAxis.Forward * currSpeeds.ForwardVelocity
					+ cameraController.transform.right * movementAxis.Right * currSpeeds.RightVelocity
					+ cameraController.transform.up * movementAxis.Up * currSpeeds.RightVelocity;

					if (!IsJetpackOn && rigidBody.velocity.sqrMagnitude >= currSpeeds.ForwardVelocity * currSpeeds.ForwardVelocity)
					{
						Vector3 vector2 = Vector3.Project(vector, rigidBody.velocity);
						if (MathHelper.SameSign(rigidBody.velocity.normalized, vector2.normalized))
						{
							vector -= vector2;
						}
					}
					if (IsJetpackOn || myPlayer.IsInsideSpaceObject || canGrabWall)
					{
						rigidBody.AddForce(vector, ForceMode.Impulse);
					}
				}

				// Set animation paramaters.
				Calculate0GAnimatorParams();
			}
		}

		private void LerpCameraToLocalZero()
		{
			float num = (Time.time - lerpCameraBackStartTime) * lerpCameraBackStep;
			mainCamera.transform.localPosition = Vector3.Lerp(lerpCameraBackStartPos, Vector3.zero, num);
			mainCamera.transform.localRotation = Quaternion.Lerp(lerpCameraBackStartRot, lerpCameraBackZeroRotation, num);
			if (num >= 1f)
			{
				lerpCameraBack = false;
			}
		}

		public void LockPlayerToPoint(bool locked, Transform attachPoint = null)
		{
			isLockedToPoint = locked;
			if (locked)
			{
				if (attachPoint != null)
				{
					attachPointSaved = attachPoint;
				}
				myPlayer.transform.parent = attachPointSaved;
				if (lockLerp < 1f)
				{
					lockLerp += lockSpeed * Time.deltaTime;
				}
				base.transform.position = Vector3.Lerp(base.transform.position, attachPointSaved.position, Mathf.Clamp01(lockLerp));
			}
			else
			{
				lockLerp = 0f;
			}
		}

		public void LockRigidbodyZRotation(bool locked)
		{
			if (locked)
			{
				rigidBody.constraints = RigidbodyConstraints.FreezeRotationZ;
			}
			else
			{
				rigidBody.constraints = RigidbodyConstraints.None;
			}
		}

		public void SetGravity(Vector3 newGravity)
		{
			if (!base.isActiveAndEnabled && !gravityChangedRagdoll)
			{
				return;
			}
			if (IsOnLadder && LockedToLadder != null)
			{
				if (!isZeroG && myPlayer.Gravity.IsEpsilonEqual(Vector3.zero))
				{
					isZeroG = true;
					isGrounded = false;
					collider1G.enabled = false;
					collider0G.enabled = true;
					rigidBody.freezeRotation = false;
					rigidBody.angularDrag = 1.5f;
					cameraController.IsZeroG = true;
					animatorHelper.SetParameter(null, false, true, false);
					cameraController.LerpCameraControllerBack(1f);
				}
				else if (isZeroG && !myPlayer.Gravity.IsEpsilonEqual(Vector3.zero))
				{
					isZeroG = false;
					collider0G.enabled = false;
					collider1G.enabled = true;
					cameraController.IsZeroG = false;
					rigidBody.drag = 0f;
					rigidBody.angularDrag = 0f;
					rigidBody.freezeRotation = true;
					if (CurrentJetpack != null && CurrentJetpack.IsActive)
					{
						CurrentJetpack.OldJetpackActiveState = CurrentJetpack.IsActive;
						CurrentJetpack.IsActive = false;
						CurrentJetpack.StartNozzles(Vector4.zero, myJetpack: true);
						RefreshMaxAngularVelocity();
					}
					animatorHelper.SetParameter(null, false, false, false);
				}
				LockedToLadder.LadderAttach(myPlayer, checkGravity: false);
				myPlayer.OldGravity = myPlayer.Gravity;
				return;
			}
			if (!isZeroG && myPlayer.Gravity.IsEpsilonEqual(Vector3.zero))
			{
				isZeroG = true;
				isGrounded = false;
				collider1G.enabled = false;
				collider0G.enabled = true;
				rigidBody.freezeRotation = false;
				rigidBody.angularDrag = 1.5f;
				cameraController.IsZeroG = true;
				if (cameraController.transform.localPosition.y.IsNotEpsilonZero())
				{
					ReparentCenterOfMass(isInChest: false);
					base.transform.position += base.transform.up * cameraController.transform.localPosition.y;
					lastMovementState = MovementState.Normal;
					crouchLerpHelper = 1f;
					collider1G.height = normalColliderHeight;
					collider1G.center = new Vector3(0f, normalColliderCenter, 0f);
					animatorHelper.SetParameter(false);
					animatorHelper.SetParameterTrigger(AnimatorHelper.Triggers.InstantStandUp);
					ReparentCenterOfMass(isInChest: true);
				}
				if (!IsAttached)
				{
					cameraController.LerpCameraControllerBack(1f);
				}
				animatorHelper.SetParameter(null, false, true, false);
				if (CurrentJetpack != null && CurrentJetpack.OldJetpackActiveState)
				{
					CurrentJetpack.IsActive = true;
					CurrentJetpack.OldJetpackActiveState = false;
					RefreshMaxAngularVelocity();
				}
				ToggleJetPack(true);
			}
			else if (isZeroG && !myPlayer.Gravity.IsEpsilonEqual(Vector3.zero))
			{
				isZeroG = false;
				collider0G.enabled = false;
				collider1G.enabled = true;
				cameraController.IsZeroG = false;
				rigidBody.drag = 0f;
				rigidBody.angularDrag = 0f;
				rigidBody.freezeRotation = true;
				if (CurrentJetpack != null && CurrentJetpack.IsActive)
				{
					CurrentJetpack.OldJetpackActiveState = CurrentJetpack.IsActive;
					CurrentJetpack.IsActive = false;
					CurrentJetpack.StartNozzles(Vector4.zero, myJetpack: true);
					RefreshMaxAngularVelocity();
				}
				animatorHelper.SetParameter(null, false, false, false);
			}
			if (!isZeroG && !myPlayer.OldGravity.IsEpsilonEqual(newGravity) && !myPlayer.InIteractLayer)
			{
				gravityChanged = true;
				gravityChangedRagdoll = false;
				gravityChangeLerpHelper = 0f;
				gravityChangeRagdollTimer = 0f;
				cameraController.ToggleCameraMovement(false);
				gravityChangeStartRotation = base.transform.rotation;
				Vector3 vector = Vector3.ProjectOnPlane(base.transform.forward, -myPlayer.GravityDirection).normalized;
				if (!vector.IsNotEpsilonZero())
				{
					vector = Vector3.forward;
				}
				gravityChangeEndingRotation = Quaternion.LookRotation(vector, -myPlayer.GravityDirection);
				if (Vector3.Angle(base.transform.up, -myPlayer.GravityDirection) > 40f || CheckGravityRagdollDistance())
				{
					gravityChangedRagdoll = true;
					ragdollChestRigidbody.velocity = rigidBody.velocity;
					myPlayer.ToggleRagdoll(true);
				}
				else if (myPlayer.OldGravity.IsEpsilonEqual(Vector3.zero))
				{
					if (Physics.SphereCast(base.transform.position, collider1G.radius - 0.05f, myPlayer.GravityDirection, out var _, 1.34f - collider1G.radius, collisionLayerMask))
					{
						gravityChanged = false;
						base.transform.rotation = gravityChangeEndingRotation;
						cameraController.ToggleCameraMovement(true);
						base.transform.position += base.transform.up * (0f - crouchColliderCenter + 0.05f);
						crouchLerpHelper = 0f;
						collider1G.height = crouchColliderHeight;
						collider1G.center = new Vector3(0f, crouchColliderCenter, 0f);
						lastMovementState = MovementState.Crouch;
						animatorHelper.SetParameter(true);
						myPlayer.ToggleMeshRendereres(enableMesh: false);
					}
					else
					{
						collider1G.height = 1f;
						collider1G.center = Vector3.zero;
					}
				}
			}
			myPlayer.OldGravity = myPlayer.Gravity;
		}

		private bool CheckGravityRagdollDistance()
		{
			RaycastHit[] array = (from h in Physics.RaycastAll(base.transform.position, myPlayer.GravityDirection, 4f, collisionLayerMask)
				orderby h.distance
				select h).ToArray();
			if (array != null && array.Length > 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].collider.GetComponent<DynamicObject>() == null && array[i].collider.GetComponent<MyPlayer>() == null && array[i].collider.GetComponent<RagdollCollider>() == null)
					{
						if (array[i].distance < 3.34f)
						{
							return false;
						}
						break;
					}
				}
			}
			return true;
		}

		private void OnCollisionEnter(Collision coli)
		{
			if (coli.relativeVelocity.magnitude >= playerImpactVelocityTreshold)
			{
				CameraController.cameraShakeController.CamShake(0.2f, 0.05f, 15f, 15f);
			}
		}

		public void ResetVelocity()
		{
			movementAxis.Forward = 0f;
			movementAxis.Right = 0f;
			if (isZeroG)
			{
				currSpeeds = normalSpeeds;
			}
			else
			{
				currSpeeds = zeroGNormalSpeeds;
			}
			lastMovementState = MovementState.Normal;
			rigidBody.velocity = Vector3.zero;
			rigidBody.angularVelocity = Vector3.zero;
			animatorHelper.SetParameter(false, null, null, null, null, null, null, null, null, null, 0f, 0f);
			myPlayer.ToggleMeshRendereres(enableMesh: true);
		}

		public void ResetPlayerLock()
		{
			IsLockedToPoint = false;
			myPlayer.SetParentTransferableObjectsRoot();
			LockRigidbodyZRotation(locked: false);
		}

		public void CheckVelocityForLock()
		{
			if (rigidBody.velocity.magnitude < 0.1f)
			{
				canLockToPoint = true;
				grabSlowEnabled = false;
			}
		}

		public void SetStateSpeedMultiplier(float speedMultiplier)
		{
			stanceSpeedMultiplier = Mathf.Clamp01(speedMultiplier);
		}

		public void ToggleCameraController(bool? isEnabled = null)
		{
			if (isEnabled.HasValue)
			{
				cameraController.enabled = isEnabled.Value;
			}
			else
			{
				cameraController.enabled = !cameraController.enabled;
			}
		}

		public void ToggleCameraMovement(bool? isEnabled = null)
		{
			cameraController.ToggleCameraMovement(isEnabled);
		}

		public void ToggleMovement(bool? isEnabled = null)
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

		public void ToggleAttached(bool? isAttached = null)
		{
			if (isAttached.HasValue)
			{
				cameraController.IsAttached = isAttached.Value;
			}
			else
			{
				cameraController.IsAttached = !cameraController.IsAttached;
			}
			if (cameraController.IsAttached)
			{
				cameraController.ToggleFreeLook(isActive: false);
			}
			else if (cameraController.IsAttached && cameraController.AutoFreeLook)
			{
				cameraController.ToggleFreeLook(isActive: true);
			}
		}

		public void ToggleOnLadder(SceneTriggerLadder ladder, bool? isOn = null)
		{
			if (isOn.HasValue)
			{
				isOnLadder = isOn.Value;
			}
			else
			{
				isOnLadder = !isOnLadder;
			}
			LockedToLadder = ((!isOnLadder) ? null : ladder);
		}

		public void ToggleJetPack(bool? isOn = null)
		{
			if (IsZeroG)
			{
				if (JetpackFuel <= float.Epsilon)
				{
					isOn = false;
				}
				if (isOn.HasValue)
				{
					IsJetpackOn = isOn.Value;
				}
				else
				{
					IsJetpackOn = !IsJetpackOn;
				}
				if (CurrentJetpack != null && !IsJetpackOn && CurrentJetpack != null)
				{
					RefreshMaxAngularVelocity();
				}
			}
		}

		public void ToggleInPlayerCollider(bool? isInTrigger = null)
		{
			if (!(myPlayer.CurrentRoomTrigger == null))
			{
			}
		}

		public void ToggleColliders(bool isEnabled)
		{
			collider1G.enabled = !isZeroG && isEnabled;
			collider0G.enabled = isZeroG && isEnabled;
		}

		public void ResetLookAt(float? duration = null)
		{
			cameraController.ResetLookAt(duration);
		}

		public void ToggleAutoFreeLook(bool isActive)
		{
			cameraController.ToggleAutoFreeLook(isActive);
		}

		public void LookAtPoint(Vector3 point)
		{
			cameraController.LookAtPoint(point);
		}

		public void AddForce(Vector3 force, ForceMode foceMode)
		{
			rigidBody.AddForce(force, foceMode);
		}

		public void AddTorque(Vector3 torque, ForceMode forceMode)
		{
			rigidBody.AddTorque(torque, forceMode);
		}

		public void RotateVelocity(Vector3 axis, float angle)
		{
			if (isGrounded && !IsJump)
			{
				rigidBody.velocity = Quaternion.AngleAxis(angle, axis) * rigidBody.velocity;
			}
		}

		public void RefreshOutfitData(Transform outfitTrans)
		{
			HeadCameraParent = animatorHelper.GetBone(AnimatorHelper.HumanBones.Head).Find("HeadCameraParent");
			ragdollChestRigidbody = animatorHelper.GetBone(AnimatorHelper.HumanBones.Spine2).GetComponent<Rigidbody>();
			centerOfMass.transform.parent = ragdollChestRigidbody.transform;
			centerOfMass.localScale = Vector3.one;
			centerOfMass.transform.localPosition = new Vector3(-0.133f, 0.014f, 0.001f);
			centerOfMass.transform.localRotation = Quaternion.Euler(97.33099f, -90f, 0.2839966f);
			cameraController.RefreshOutfitData(outfitTrans);
		}

		public void AddCharacterRotation(Vector3 euler)
		{
			cameraController.AddCharacterRotation(euler);
		}

		public IEnumerator AddCharacterRotationByTime(Vector3 euler, float time)
		{
			float x = euler.x / time;
			float y = euler.y / time;
			float z = euler.z / time;
			while (time > 0f)
			{
				cameraController.AddCharacterRotation(new Vector3(x, y, z));
				yield return new WaitForEndOfFrame();
				time -= Time.deltaTime;
			}
			yield return null;
		}

		public void ToggleCameraAttachToHeadBone(bool? isAttached = null, Vector3? lockedAngle = null)
		{
			cameraController.ToggleCameraAttachToHeadBone(isAttached, lockedAngle);
		}

		public void ToggleKinematic(bool? isKinematic = null)
		{
			bool flag = ((!isKinematic.HasValue) ? (!rigidBody.isKinematic) : isKinematic.Value);
			collider0G.enabled = !flag && isZeroG;
			collider1G.enabled = !flag && !isZeroG;
			rigidBody.isKinematic = flag;
		}

		public void ReparentCenterOfMass(bool isInChest)
		{
			if (!isInChest)
			{
				centerOfMass.transform.parent = null;
				return;
			}
			centerOfMass.transform.parent = ragdollChestRigidbody.transform;
			centerOfMass.localScale = Vector3.one;
			centerOfMass.transform.localPosition = new Vector3(-0.133f, 0.014f, 0.001f);
			centerOfMass.transform.localRotation = Quaternion.Euler(97.33099f, -90f, 0.2839966f);
		}

		public void LerpCameraBack(float time, Vector3? localZeroRotation = null)
		{
			lerpCameraBack = true;
			if (localZeroRotation.HasValue)
			{
				lerpCameraBackZeroRotation = Quaternion.Euler(localZeroRotation.Value);
			}
			else
			{
				lerpCameraBackZeroRotation = Quaternion.identity;
			}
			lerpCameraBackStartPos = mainCamera.transform.localPosition;
			lerpCameraBackStartRot = mainCamera.transform.localRotation;
			lerpCameraBackStartTime = Time.time;
			lerpCameraBackStep = 1f / time;
		}

		public IEnumerator TranslateAndLookAt(Transform position, Transform lookAt, TranslateDelegate actionToCall)
		{
			ResetVelocity();
			ToggleMovement(false);
			ToggleCameraMovement(false);
			Vector3 startingPosition = base.transform.position;
			Quaternion startingRotation = base.transform.rotation;
			Vector3 startingLookAt = mainCamera.transform.position + mainCamera.transform.forward;
			translateLerpHelper = 0f;
			myPlayer.InLerpingState = true;
			if (Client.Instance.CanvasManager.IsPlayerOverviewOpen)
			{
				Client.Instance.CanvasManager.PlayerOverview.Toggle(val: false);
			}
			if (myPlayer.CurrentActiveItem != null)
			{
				myPlayer.Inventory.AddToInventoryOrDrop(myPlayer.CurrentActiveItem, myPlayer.Inventory.HandsSlot);
			}
			while (translateLerpHelper < 1f)
			{
				if (position != null)
				{
					base.transform.position = Vector3.Lerp(startingPosition, position.position, Mathf.SmoothStep(0f, 1f, translateLerpHelper));
					base.transform.rotation = Quaternion.Lerp(startingRotation, position.rotation, Mathf.SmoothStep(0f, 1f, translateLerpHelper));
				}
				if (lookAt != null)
				{
					cameraController.LookAtPoint(Vector3.Lerp(startingLookAt, lookAt.position, Mathf.SmoothStep(0f, 1f, translateLerpHelper)));
				}
				translateLerpHelper += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
			myPlayer.InLerpingState = false;
			base.transform.position = position.position;
			base.transform.rotation = position.rotation;
			ToggleCameraMovement(true);
			if (lookAt != null)
			{
				cameraController.LookAtPoint(lookAt.position);
			}
			actionToCall();
		}

		public void Tumble(Vector3? tumbleVelocity = null)
		{
			HasTumbled = true;
			gravityChanged = true;
			gravityChangedRagdoll = true;
			gravityChangeLerpHelper = 0f;
			gravityChangeRagdollTimer = 0f;
			cameraController.ToggleCameraMovement(false);
			if (tumbleVelocity.HasValue)
			{
				float num = tumbleVelocity.Value.magnitude;
				if (num < playerImpactVelocityTreshold)
				{
					num = playerImpactVelocityTreshold + 1f;
				}
				myPlayer.ImpactVelocity = num;
			}
			myPlayer.ToggleRagdoll(true);
			ragdollChestRigidbody.velocity = myPlayer.GravityDirection * 0.1f;
		}

		public void SetHandsBoxCollider(BoxCollider collider)
		{
			CameraController.SetHandsBoxCollider(collider);
		}

		public int CheckGetUpRoom()
		{
			if (Physics.SphereCast(characterRoot.position, collider1G.radius - 0.02f, -myPlayer.GravityDirection * 1.34f, out var _, normalColliderHeight - collider1G.radius, collisionLayerMask))
			{
				crouchLerpHelper = 0f;
				collider1G.height = crouchColliderHeight;
				collider1G.center = new Vector3(0f, crouchColliderCenter, 0f);
				lastMovementState = MovementState.Crouch;
				animatorHelper.SetParameter(true);
				myPlayer.ToggleMeshRendereres(enableMesh: false);
				return 0;
			}
			return 1;
		}

		public void RefreshMaxAngularVelocity()
		{
			MaxAngularVelocity = (float)((double)DefaultMaxAngularVelocity * (System.Math.PI / 180.0));
		}

		private void OnParticleCollision(GameObject other)
		{
			if ((bool)other.GetComponentInParent<DebrisFieldEffect>())
			{
				myPlayer.ImpactVelocity = 9f;
				CameraController.cameraShakeController.CamShake(0.2f, 0.05f, 15f, 15f);
			}
		}

		public Collider GetCollider()
		{
			return (!collider1G.enabled) ? ((Collider)collider0G) : ((Collider)collider1G);
		}
	}
}
