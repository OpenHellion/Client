using UnityEngine;

namespace RootMotion.Demos
{
	public class MotionAbsorbCharacter : MonoBehaviour
	{
		public Animator animator;

		public MotionAbsorb motionAbsorb;

		public Transform cube;

		public float cubeRandomPosition = 0.1f;

		public AnimationCurve motionAbsorbWeight;

		private Vector3 cubeDefaultPosition;

		private AnimatorStateInfo info;

		private Rigidbody cubeRigidbody;

		private void Start()
		{
			cubeDefaultPosition = cube.position;
			cubeRigidbody = cube.GetComponent<Rigidbody>();
		}

		private void Update()
		{
			info = animator.GetCurrentAnimatorStateInfo(0);
			motionAbsorb.weight = motionAbsorbWeight.Evaluate(info.normalizedTime - (float)(int)info.normalizedTime);
		}

		private void SwingStart()
		{
			cubeRigidbody.MovePosition(cubeDefaultPosition + Random.insideUnitSphere * cubeRandomPosition);
			cubeRigidbody.MoveRotation(Quaternion.identity);
			cubeRigidbody.velocity = Vector3.zero;
			cubeRigidbody.angularVelocity = Vector3.zero;
		}
	}
}
