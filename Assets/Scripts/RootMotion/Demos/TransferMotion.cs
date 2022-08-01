using UnityEngine;

namespace RootMotion.Demos
{
	public class TransferMotion : MonoBehaviour
	{
		[Tooltip("The Transform to transfer motion to.")]
		public Transform to;

		[Tooltip("The amount of motion to transfer.")]
		[Range(0f, 1f)]
		public float transferMotion = 0.9f;

		private Vector3 lastPosition;

		private void OnEnable()
		{
			lastPosition = base.transform.position;
		}

		private void Update()
		{
			Vector3 vector = base.transform.position - lastPosition;
			to.position += vector * transferMotion;
			lastPosition = base.transform.position;
		}
	}
}
