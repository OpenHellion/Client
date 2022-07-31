using UnityEngine;

namespace RootMotion.Demos
{
	[RequireComponent(typeof(CharacterController))]
	public class VRCharacterController : MonoBehaviour
	{
		public float moveSpeed = 2f;

		public float rotationSpeed = 2f;

		[Range(0f, 180f)]
		public float rotationRatchet = 45f;

		public KeyCode ratchetRight = KeyCode.E;

		public KeyCode ratchetLeft = KeyCode.Q;

		public Transform forwardDirection;

		private CharacterController characterController;

		private void Awake()
		{
			characterController = GetComponent<CharacterController>();
			if (forwardDirection == null)
			{
				forwardDirection = base.transform;
			}
		}

		private void Update()
		{
			Vector3 vector = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
			Vector3 forward = forwardDirection.forward;
			forward.y = 0f;
			characterController.SimpleMove(Quaternion.LookRotation(forward) * Vector3.ClampMagnitude(vector, 1f) * moveSpeed);
			if (Input.GetKeyDown(ratchetRight))
			{
				base.transform.rotation = Quaternion.Euler(0f, rotationRatchet, 0f) * base.transform.rotation;
			}
			else if (Input.GetKeyDown(ratchetLeft))
			{
				base.transform.rotation = Quaternion.Euler(0f, 0f - rotationRatchet, 0f) * base.transform.rotation;
			}
			base.transform.rotation = Quaternion.Euler(0f, Input.GetAxis("Mouse X") * rotationSpeed, 0f) * base.transform.rotation;
		}
	}
}
