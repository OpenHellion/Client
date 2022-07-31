using UnityEngine;

namespace RootMotion.Demos
{
	public class UserControlThirdPerson : MonoBehaviour
	{
		public struct State
		{
			public Vector3 move;

			public Vector3 lookPos;

			public bool crouch;

			public bool jump;

			public int actionIndex;
		}

		public bool walkByDefault;

		public bool canCrouch = true;

		public bool canJump = true;

		public State state = default(State);

		protected Transform cam;

		private void Start()
		{
			cam = Camera.main.transform;
		}

		protected virtual void Update()
		{
			state.crouch = canCrouch && Input.GetKey(KeyCode.C);
			state.jump = canJump && Input.GetButton("Jump");
			float axisRaw = Input.GetAxisRaw("Horizontal");
			float axisRaw2 = Input.GetAxisRaw("Vertical");
			ref State reference = ref state;
			Vector3 vector = new Vector3(cam.forward.x, 0f, cam.forward.z);
			reference.move = Quaternion.LookRotation(vector.normalized) * new Vector3(axisRaw, 0f, axisRaw2).normalized;
			bool key = Input.GetKey(KeyCode.LeftShift);
			float num = (walkByDefault ? ((!key) ? 0.5f : 1f) : ((!key) ? 1f : 0.5f));
			state.move *= num;
			state.lookPos = base.transform.position + cam.forward * 100f;
		}
	}
}
