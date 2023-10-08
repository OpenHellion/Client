using UnityEngine;

public class TubeLightShadowPlane : MonoBehaviour
{
	public struct Params
	{
		public Vector4 plane;

		public float feather;
	}

	[MinValue(0f)] public float m_Feather = 1f;

	public float feather
	{
		get { return m_Feather * 0.1f; }
	}

	public Vector4 GetShadowPlaneVector()
	{
		Transform transform = base.transform;
		Vector3 forward = transform.forward;
		float w = Vector3.Dot(transform.position, forward);
		return new Vector4(forward.x, forward.y, forward.z, w);
	}

	private void OnDrawGizmosSelected()
	{
		Matrix4x4 zero = Matrix4x4.zero;
		Transform transform = base.transform;
		zero.SetTRS(transform.position, transform.rotation, new Vector3(1f, 1f, 0f));
		Gizmos.matrix = zero;
		Gizmos.DrawWireSphere(Vector3.zero, 1f);
	}
}
