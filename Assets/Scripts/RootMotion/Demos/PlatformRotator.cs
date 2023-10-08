using System.Collections;
using UnityEngine;

namespace RootMotion.Demos
{
	public class PlatformRotator : MonoBehaviour
	{
		public float maxAngle = 70f;

		public float switchRotationTime = 0.5f;

		public float random = 0.5f;

		public float rotationSpeed = 50f;

		public Vector3 movePosition;

		public float moveSpeed = 5f;

		public int characterLayer;

		private Quaternion defaultRotation;

		private Quaternion targetRotation;

		private Vector3 targetPosition;

		private Vector3 velocity;

		private Rigidbody r;

		private void Start()
		{
			defaultRotation = base.transform.rotation;
			targetPosition = base.transform.position + movePosition;
			r = GetComponent<Rigidbody>();
			StartCoroutine(SwitchRotation());
		}

		private void FixedUpdate()
		{
			r.MovePosition(Vector3.SmoothDamp(r.position, targetPosition, ref velocity, 1f, moveSpeed));
			if (Vector3.Distance(GetComponent<Rigidbody>().position, targetPosition) < 0.1f)
			{
				movePosition = -movePosition;
				targetPosition += movePosition;
			}

			r.MoveRotation(Quaternion.RotateTowards(r.rotation, targetRotation, rotationSpeed * Time.deltaTime));
		}

		private IEnumerator SwitchRotation()
		{
			while (true)
			{
				float angle = Random.Range(0f - maxAngle, maxAngle);
				Vector3 axis = Random.onUnitSphere;
				targetRotation = Quaternion.AngleAxis(angle, axis) * defaultRotation;
				yield return new WaitForSeconds(switchRotationTime + Random.value * random);
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (collision.gameObject.layer == characterLayer)
			{
				CharacterThirdPerson component = collision.gameObject.GetComponent<CharacterThirdPerson>();
				if (!(component == null) && component.smoothPhysics)
				{
					component.smoothPhysics = false;
				}
			}
		}

		private void OnCollisionExit(Collision collision)
		{
			if (collision.gameObject.layer == characterLayer)
			{
				CharacterThirdPerson component = collision.gameObject.GetComponent<CharacterThirdPerson>();
				if (!(component == null))
				{
					component.smoothPhysics = true;
				}
			}
		}
	}
}
