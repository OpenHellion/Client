using System;
using UnityEngine;

namespace RootMotion.Demos
{
	[RequireComponent(typeof(Animator))]
	public class SimpleLocomotion : MonoBehaviour
	{
		[Serializable]
		public enum RotationMode
		{
			Smooth = 0,
			Linear = 1
		}

		[Tooltip("The component that updates the camera.")] [SerializeField]
		private CameraController cameraController;

		[Tooltip("Acceleration of movement.")] [SerializeField]
		private float accelerationTime = 0.2f;

		[Tooltip("Turning speed.")] [SerializeField]
		private float turnTime = 0.2f;

		[Tooltip("If true, will run on left shift, if not will walk on left shift.")] [SerializeField]
		private bool walkByDefault = true;

		[Tooltip("Smooth or linear rotation.")] [SerializeField]
		private RotationMode rotationMode;

		[Tooltip("Procedural motion speed (if not using root motion).")] [SerializeField]
		private float moveSpeed = 3f;

		private Animator animator;

		private float speed;

		private float angleVel;

		private float speedVel;

		private Vector3 linearTargetDirection;

		private CharacterController characterController;

		public bool isGrounded { get; private set; }

		private void Start()
		{
			animator = GetComponent<Animator>();
			characterController = GetComponent<CharacterController>();
			cameraController.enabled = false;
		}

		private void Update()
		{
			isGrounded = base.transform.position.y < 0.1f;
			Rotate();
			Move();
		}

		private void LateUpdate()
		{
			cameraController.UpdateInput();
			cameraController.UpdateTransform();
		}

		private void Rotate()
		{
			if (!isGrounded)
			{
				return;
			}

			Vector3 inputVector = GetInputVector();
			if (inputVector == Vector3.zero)
			{
				return;
			}

			Vector3 forward = base.transform.forward;
			switch (rotationMode)
			{
				case RotationMode.Smooth:
				{
					Vector3 vector = cameraController.transform.rotation * inputVector;
					float current = Mathf.Atan2(forward.x, forward.z) * 57.29578f;
					float target = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
					float angle = Mathf.SmoothDampAngle(current, target, ref angleVel, turnTime);
					base.transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
					break;
				}
				case RotationMode.Linear:
				{
					Vector3 inputVectorRaw = GetInputVectorRaw();
					if (inputVectorRaw != Vector3.zero)
					{
						linearTargetDirection = cameraController.transform.rotation * inputVectorRaw;
					}

					forward = Vector3.RotateTowards(forward, linearTargetDirection, Time.deltaTime * (1f / turnTime),
						1f);
					forward.y = 0f;
					base.transform.rotation = Quaternion.LookRotation(forward);
					break;
				}
			}
		}

		private void Move()
		{
			float target = (walkByDefault
				? ((!Input.GetKey(KeyCode.LeftShift)) ? 0.5f : 1f)
				: ((!Input.GetKey(KeyCode.LeftShift)) ? 1f : 0.5f));
			speed = Mathf.SmoothDamp(speed, target, ref speedVel, accelerationTime);
			float num = GetInputVector().magnitude * speed;
			animator.SetFloat("Speed", num);
			if (!animator.hasRootMotion && isGrounded)
			{
				Vector3 vector = base.transform.forward * num * moveSpeed;
				if (characterController != null)
				{
					characterController.SimpleMove(vector);
				}
				else
				{
					base.transform.position += vector * Time.deltaTime;
				}
			}
		}

		private Vector3 GetInputVector()
		{
			Vector3 result = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
			result.z += Mathf.Abs(result.x) * 0.05f;
			result.x -= Mathf.Abs(result.z) * 0.05f;
			return result;
		}

		private Vector3 GetInputVectorRaw()
		{
			return new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
		}
	}
}
