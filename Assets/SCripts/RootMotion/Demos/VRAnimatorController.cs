using UnityEngine;

namespace RootMotion.Demos
{
	[RequireComponent(typeof(Animator))]
	public class VRAnimatorController : MonoBehaviour
	{
		[Header("Component References")]
		public VRSetup oculusSetup;

		public Transform characterController;

		public Transform cam;

		[Header("Main Properties")]
		[Tooltip("Offset of the VR camera")]
		public Vector3 cameraOffset;

		[Tooltip("How long to accelerate to target velocity using SmoothDamp?")]
		public float smoothAccelerationTime = 0.2f;

		[Tooltip("How fast to accelerate liearily? If this is zero, will only use smooth acceleration.")]
		public float linearAcceleration = 2f;

		[Tooltip("Rotate the character along if camera is looking too far left/right.")]
		public float maxViewAngle = 60f;

		[Tooltip("The master speed of locomotion animations.")]
		public float locomotionSpeed = 1f;

		private Animator animator;

		private Vector3 velocityC;

		private bool rootCorrection;

		private Vector3 playerVelocity;

		private Vector3 playerLastPosition;

		private FixFeet fixFeet;

		private Transform cameraPivot;

		public Vector3 velocity { get; private set; }

		private void Start()
		{
			animator = GetComponent<Animator>();
			fixFeet = base.gameObject.AddComponent<FixFeet>();
			animator.SetBool("IsStrafing", true);
			playerLastPosition = characterController.position;
			cameraPivot = new GameObject().transform;
			cameraPivot.name = "Camera Pivot";
			cameraPivot.position = characterController.position + characterController.rotation * cameraOffset;
			cameraPivot.rotation = characterController.rotation;
			cameraPivot.parent = characterController;
			cam.parent = cameraPivot;
		}

		private void Update()
		{
			if (!oculusSetup.isFinished)
			{
				if (fixFeet != null)
				{
					fixFeet.weight = 1f;
				}
				velocity = Vector3.zero;
				animator.SetFloat("Right", 0f);
				animator.SetFloat("Forward", 0f);
				return;
			}
			RotateCharacter(cam.forward, maxViewAngle, cameraPivot);
			Vector3 velocityTarget = GetVelocityTarget();
			velocity = Vector3.MoveTowards(velocity, velocityTarget, Time.deltaTime * linearAcceleration);
			velocity = Vector3.SmoothDamp(velocity, velocityTarget, ref velocityC, smoothAccelerationTime);
			base.transform.position = new Vector3(characterController.position.x, base.transform.position.y, characterController.position.z);
			if (fixFeet != null)
			{
				float target = ((!(velocity == Vector3.zero)) ? 0f : 1f);
				fixFeet.weight = Mathf.MoveTowards(fixFeet.weight, target, Time.deltaTime * 3f);
			}
			animator.SetFloat("Right", velocity.x);
			animator.SetFloat("Forward", velocity.z);
		}

		private Vector3 GetVelocityTarget()
		{
			Vector3 zero = Vector3.zero;
			playerVelocity = (characterController.position - playerLastPosition) / Time.deltaTime;
			playerLastPosition = characterController.position;
			return zero + Quaternion.Inverse(base.transform.rotation) * playerVelocity * locomotionSpeed;
		}

		public void RotateCharacter(Vector3 forward, float maxAngle, Transform fix = null)
		{
			if (maxAngle >= 180f)
			{
				return;
			}
			Quaternion rotation = ((!(fix != null)) ? Quaternion.identity : fix.rotation);
			if (maxAngle <= 0f)
			{
				characterController.rotation = Quaternion.LookRotation(new Vector3(forward.x, 0f, forward.z));
				if (fix != null)
				{
					fix.rotation = rotation;
				}
				return;
			}
			Vector3 vector = characterController.InverseTransformDirection(forward);
			float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
			if (Mathf.Abs(num) > Mathf.Abs(maxAngle))
			{
				float angle = num - maxAngle;
				if (num < 0f)
				{
					angle = num + maxAngle;
				}
				characterController.rotation = Quaternion.AngleAxis(angle, characterController.up) * characterController.rotation;
			}
			if (fix != null)
			{
				fix.rotation = rotation;
			}
		}
	}
}
