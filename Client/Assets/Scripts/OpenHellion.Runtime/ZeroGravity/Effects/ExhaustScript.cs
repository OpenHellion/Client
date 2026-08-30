using UnityEngine;

namespace ZeroGravity.Effects
{
	public class ExhaustScript : MonoBehaviour
	{
		public Camera cameraToLookAt;

		private void Update()
		{
			if (cameraToLookAt != null && transform != null)
			{
				transform.rotation = Quaternion.LookRotation(
					Vector3.ProjectOnPlane(transform.position - cameraToLookAt.transform.position,
						transform.up), transform.up);
			}
		}
	}
}
