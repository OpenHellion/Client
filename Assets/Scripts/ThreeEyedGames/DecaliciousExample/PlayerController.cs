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
			Vector3 forward = base.transform.forward;
			forward.y = 0f;
			forward.Normalize();
			base.transform.Rotate(new Vector3(0f, Input.GetAxis("Mouse X") * MouseSensitivity, 0f), Space.World);
			base.transform.Rotate(new Vector3((0f - Input.GetAxis("Mouse Y")) * MouseSensitivity, 0f, 0f), Space.Self);
			float moveSpeed = MoveSpeed;
			if (Input.GetKey(KeyCode.W))
			{
				base.transform.Translate(forward * Time.deltaTime * moveSpeed, Space.World);
			}

			if (Input.GetKey(KeyCode.S))
			{
				base.transform.Translate(-forward * Time.deltaTime * moveSpeed, Space.World);
			}

			if (Input.GetKey(KeyCode.A))
			{
				base.transform.Translate(-base.transform.right * Time.deltaTime * moveSpeed, Space.World);
			}

			if (Input.GetKey(KeyCode.D))
			{
				base.transform.Translate(base.transform.right * Time.deltaTime * moveSpeed, Space.World);
			}

			RaycastHit hitInfo;
			if (Input.GetKeyDown(KeyCode.E) &&
			    Physics.Raycast(Camera.main.ViewportPointToRay(Vector3.one * 0.5f), out hitInfo))
			{
				bool flag = false;
				if (Vector3.SqrMagnitude(base.transform.position - hitInfo.point) < 2.25f &&
				    hitInfo.collider.GetComponent<IInteract>() != null)
				{
					hitInfo.collider.GetComponent<IInteract>().Interact();
					flag = true;
				}

				AudioSource.PlayClipAtPoint((!flag) ? InteractError : InteractSuccess,
					base.transform.position + base.transform.forward * 0.5f);
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				DecaliciousRenderer component = Camera.main.GetComponent<DecaliciousRenderer>();
				component.enabled = !component.enabled;
			}
		}
	}
}
