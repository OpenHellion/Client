using UnityEngine;

public class AttachPointGizmo : MonoBehaviour
{
	public Vector3 GizmoScale = Vector3.one;

	public Vector3 Offset = Vector3.zero;

	public Color GizmoColor = new Color(1f, 0f, 0f, 0.5f);

	private void OnDrawGizmos()
	{
		Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
		Gizmos.color = GizmoColor;
		Gizmos.DrawCube(new Vector3(0f, GizmoScale.y / 2f, 0f) + Offset, GizmoScale);
	}
}
