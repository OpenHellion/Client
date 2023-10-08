using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
	public class TerrainOffset : MonoBehaviour
	{
		public AimIK aimIK;

		public Vector3 raycastOffset = new Vector3(0f, 2f, 1.5f);

		public LayerMask raycastLayers;

		public float min = -2f;

		public float max = 2f;

		public float lerpSpeed = 10f;

		private RaycastHit hit;

		private Vector3 offset;

		private void LateUpdate()
		{
			Vector3 vector = base.transform.rotation * raycastOffset;
			Vector3 groundHeightOffset = GetGroundHeightOffset(base.transform.position + vector);
			offset = Vector3.Lerp(offset, groundHeightOffset, Time.deltaTime * lerpSpeed);
			Vector3 vector2 = base.transform.position + new Vector3(vector.x, 0f, vector.z);
			aimIK.solver.transform.LookAt(vector2);
			aimIK.solver.IKPosition = vector2 + offset;
		}

		private Vector3 GetGroundHeightOffset(Vector3 worldPosition)
		{
			Debug.DrawRay(worldPosition, Vector3.down * raycastOffset.y * 2f, Color.green);
			if (Physics.Raycast(worldPosition, Vector3.down, out hit, raycastOffset.y * 2f))
			{
				return Mathf.Clamp(hit.point.y - base.transform.position.y, min, max) * Vector3.up;
			}

			return Vector3.zero;
		}
	}
}
