using UnityEngine;

[ExecuteInEditMode]
public class DeferredDecal : Decal
{
	public void Start()
	{
		DeferredDecalController.AddDecal(this);
	}

	public void OnEnable()
	{
		DeferredDecalController.AddDecal(this);
	}

	public void OnDisable()
	{
		DeferredDecalController.RemoveDecal(this);
	}

	private void DrawGizmo(bool selected)
	{
		Color color = new Color(0f, 0.7f, 1f, 1f);
		color.a = ((!selected) ? 0.1f : 0.3f);
		Gizmos.color = color;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
		color.a = ((!selected) ? 0.2f : 0.5f);
		Gizmos.color = color;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}

	private void OnDrawGizmos()
	{
		DrawGizmo(false);
	}

	private void OnDrawGizmosSelected()
	{
		DrawGizmo(true);
	}
}
