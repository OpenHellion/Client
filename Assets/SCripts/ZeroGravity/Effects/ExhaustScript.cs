using UnityEngine;

namespace ZeroGravity.Effects
{
	public class ExhaustScript : MonoBehaviour
	{
		public Camera cameraToLookAt;

		private void Update()
		{
			if (Client.IsGameBuild && cameraToLookAt != null && base.transform != null)
			{
				base.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(base.transform.position - cameraToLookAt.transform.position, base.transform.up), base.transform.up);
			}
		}
	}
}
