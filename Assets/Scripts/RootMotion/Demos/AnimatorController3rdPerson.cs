using UnityEngine;

namespace RootMotion.Demos
{
	[RequireComponent(typeof(Animator))]
	public class AnimatorController3rdPerson : MonoBehaviour
	{
		public float rotateSpeed = 7f;

		public float blendSpeed = 10f;

		public float maxAngle = 90f;

		public float moveSpeed = 1.5f;

		public float rootMotionWeight;

		protected Animator animator;

		protected Vector3 moveBlend;

		protected Vector3 moveInput;

		protected Vector3 velocity;

		protected virtual void Start()
		{
			animator = GetComponent<Animator>();
		}

		private void OnAnimatorMove()
		{
			velocity = Vector3.Lerp(velocity,
				base.transform.rotation * Vector3.ClampMagnitude(moveInput, 1f) * moveSpeed,
				Time.deltaTime * blendSpeed);
			base.transform.position +=
				Vector3.Lerp(velocity * Time.deltaTime, animator.deltaPosition, rootMotionWeight);
		}

		public virtual void Move(Vector3 moveInput, bool isMoving, Vector3 faceDirection, Vector3 aimTarget)
		{
			this.moveInput = moveInput;
			Vector3 vector = base.transform.InverseTransformDirection(faceDirection);
			float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
			float num2 = num * Time.deltaTime * rotateSpeed;
			if (num > maxAngle)
			{
				num2 = Mathf.Clamp(num2, num - maxAngle, num2);
			}

			if (num < 0f - maxAngle)
			{
				num2 = Mathf.Clamp(num2, num2, num + maxAngle);
			}

			base.transform.Rotate(Vector3.up, num2);
			moveBlend = Vector3.Lerp(moveBlend, moveInput, Time.deltaTime * blendSpeed);
			animator.SetFloat("X", moveBlend.x);
			animator.SetFloat("Z", moveBlend.z);
			animator.SetBool("IsMoving", isMoving);
		}
	}
}
