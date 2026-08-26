using UnityEngine;

namespace RootMotion.Demos
{
	[RequireComponent(typeof(Animator))]
	public class CharacterAnimationThirdPerson : CharacterAnimationBase
	{
		public CharacterThirdPerson characterController;

		[SerializeField] private float turnSensitivity = 0.2f;

		[SerializeField] private float turnSpeed = 5f;

		[SerializeField] private float runCycleLegOffset = 0.2f;

		[Range(0.1f, 3f)] [SerializeField] private float animSpeedMultiplier = 1f;

		protected Animator animator;

		private Vector3 lastForward;

		private const string groundedDirectional = "Grounded Directional";

		private const string groundedStrafe = "Grounded Strafe";

		public override bool animationGrounded
		{
			get
			{
				return animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded Directional") ||
				       animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded Strafe");
			}
		}

		protected override void Start()
		{
			base.Start();
			animator = GetComponent<Animator>();
			lastForward = base.transform.forward;
		}

		public override Vector3 GetPivotPoint()
		{
			return animator.pivotPosition;
		}

		protected virtual void Update()
		{
			if (Time.deltaTime != 0f)
			{
				if (characterController.animState.jump)
				{
					float num = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + runCycleLegOffset,
						1f);
					float value = (float)((num < 0f) ? 1 : (-1)) * characterController.animState.moveDirection.z;
					animator.SetFloat("JumpLeg", value);
				}

				float num2 = 0f - GetAngleFromForward(lastForward);
				lastForward = base.transform.forward;
				num2 *= turnSensitivity * 0.01f;
				num2 = Mathf.Clamp(num2 / Time.deltaTime, -1f, 1f);
				animator.SetFloat("Turn", Mathf.Lerp(animator.GetFloat("Turn"), num2, Time.deltaTime * turnSpeed));
				animator.SetFloat("Forward", characterController.animState.moveDirection.z);
				animator.SetFloat("Right", characterController.animState.moveDirection.x);
				animator.SetBool("Crouch", characterController.animState.crouch);
				animator.SetBool("OnGround", characterController.animState.onGround);
				animator.SetBool("IsStrafing", characterController.animState.isStrafing);
				if (!characterController.animState.onGround)
				{
					animator.SetFloat("Jump", characterController.animState.yVelocity);
				}

				if (characterController.animState.onGround && characterController.animState.moveDirection.z > 0f)
				{
					animator.speed = animSpeedMultiplier;
				}
				else
				{
					animator.speed = 1f;
				}
			}
		}

		private void OnAnimatorMove()
		{
			characterController.Move(animator.deltaPosition, animator.deltaRotation);
		}
	}
}
