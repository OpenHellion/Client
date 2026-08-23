using UnityEngine;

namespace ThreeEyedGames.DecaliciousExample
{
	public class PlayerController : MonoBehaviour
	{
		public float MouseSensitivity = 1f;

		public float MoveSpeed = 1f;

		public AudioClip InteractSuccess;

		public AudioClip InteractError;

		private void Start()
		{
			Cursor.lockState = CursorLockMode.Locked;
		}

		private void Update()
		{
			Vector3 forward = transform.forward;
			forward.y = 0f;
			forward.Normalize();
			transform.Rotate(new Vector3(0f, Input.GetAxis("Mouse X") * MouseSensitivity, 0f), Space.World);
			transform.Rotate(new Vector3((0f - Input.GetAxis("Mouse Y")) * MouseSensitivity, 0f, 0f), Space.Self);
			float moveSpeed = MoveSpeed;
			if (Input.GetKey(KeyCode.W))
			{
				transform.Translate(forward * Time.deltaTime * moveSpeed, Space.World);
			}

			if (Input.GetKey(KeyCode.S))
			{
				transform.Translate(-forward * Time.deltaTime * moveSpeed, Space.World);
			}

			if (Input.GetKey(KeyCode.A))
			{
				transform.Translate(-transform.right * Time.deltaTime * moveSpeed, Space.World);
			}

			if (Input.GetKey(KeyCode.D))
			{
				transform.Translate(transform.right * Time.deltaTime * moveSpeed, Space.World);
			}

			RaycastHit hitInfo;
			if (Input.GetKeyDown(KeyCode.E) &&
			    Physics.Raycast(Camera.main.ViewportPointToRay(Vector3.one * 0.5f), out hitInfo))
			{
				bool flag = false;
				if (Vector3.SqrMagnitude(transform.position - hitInfo.point) < 2.25f &&
				    hitInfo.collider.GetComponent<IInteract>() != null)
				{
					hitInfo.collider.GetComponent<IInteract>().Interact();
					flag = true;
				}

				AudioSource.PlayClipAtPoint((!flag) ? InteractError : InteractSuccess,
					transform.position + transform.forward * 0.5f);
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				DecaliciousRenderer component = Camera.main.GetComponent<DecaliciousRenderer>();
				component.enabled = !component.enabled;
			}
		}
	}
}
