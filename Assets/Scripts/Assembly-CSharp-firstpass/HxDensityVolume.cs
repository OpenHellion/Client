using UnityEngine;

[ExecuteInEditMode]
public class HxDensityVolume : MonoBehaviour
{
	public enum DensityShape
	{
		Square = 0,
		Sphere = 1,
		Cylinder = 2
	}

	public enum DensityBlendMode
	{
		Max = 0,
		Add = 1,
		Min = 2,
		Sub = 3
	}

	public static HxOctree<HxDensityVolume> DensityOctree;

	private HxOctreeNode<HxDensityVolume>.NodeObject octreeNode;

	public DensityShape Shape;

	public DensityBlendMode BlendMode = DensityBlendMode.Add;

	[HideInInspector]
	public Vector3 minBounds;

	[HideInInspector]
	public Vector3 maxBounds;

	[HideInInspector]
	public Matrix4x4 ToLocalSpace;

	public float Density = 0.1f;

	private static Color gizmoColor = new Color(0.992f, 0.749f, 0.592f);

	private static Vector3 c1 = new Vector3(0.5f, 0.5f, 0.5f);

	private static Vector3 c2 = new Vector3(-0.5f, 0.5f, 0.5f);

	private static Vector3 c3 = new Vector3(0.5f, 0.5f, -0.5f);

	private static Vector3 c4 = new Vector3(-0.5f, 0.5f, -0.5f);

	private static Vector3 c5 = new Vector3(0.5f, -0.5f, 0.5f);

	private static Vector3 c6 = new Vector3(-0.5f, -0.5f, 0.5f);

	private static Vector3 c7 = new Vector3(0.5f, -0.5f, -0.5f);

	private static Vector3 c8 = new Vector3(-0.5f, -0.5f, -0.5f);

	private void OnEnable()
	{
		CalculateBounds();
		if (DensityOctree == null)
		{
			DensityOctree = new HxOctree<HxDensityVolume>();
		}
		HxVolumetricCamera.AllDensityVolumes.Add(this);
		octreeNode = DensityOctree.Add(this, minBounds, maxBounds);
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "AreaLight Gizmo", true);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = gizmoColor;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}

	private void OnDisable()
	{
		HxVolumetricCamera.AllDensityVolumes.Remove(this);
		if (DensityOctree != null)
		{
			DensityOctree.Remove(this);
			DensityOctree = null;
		}
	}

	private void OnDestroy()
	{
		HxVolumetricCamera.AllDensityVolumes.Remove(this);
		if (DensityOctree != null)
		{
			DensityOctree.Remove(this);
			DensityOctree = null;
		}
	}

	public void UpdateVolume()
	{
		if (base.transform.hasChanged)
		{
			CalculateBounds();
			DensityOctree.Move(octreeNode, minBounds, maxBounds);
			base.transform.hasChanged = false;
		}
	}

	private void CalculateBounds()
	{
		Vector3 lhs = base.transform.TransformPoint(c1);
		Vector3 lhs2 = base.transform.TransformPoint(c2);
		Vector3 lhs3 = base.transform.TransformPoint(c3);
		Vector3 lhs4 = base.transform.TransformPoint(c4);
		Vector3 lhs5 = base.transform.TransformPoint(c5);
		Vector3 lhs6 = base.transform.TransformPoint(c6);
		Vector3 lhs7 = base.transform.TransformPoint(c7);
		Vector3 rhs = base.transform.TransformPoint(c8);
		minBounds = Vector3.Min(lhs, Vector3.Min(lhs2, Vector3.Min(lhs3, Vector3.Min(lhs4, Vector3.Min(lhs5, Vector3.Min(lhs6, Vector3.Min(lhs7, rhs)))))));
		maxBounds = Vector3.Max(lhs, Vector3.Max(lhs2, Vector3.Max(lhs3, Vector3.Max(lhs4, Vector3.Max(lhs5, Vector3.Max(lhs6, Vector3.Max(lhs7, rhs)))))));
		ToLocalSpace = base.transform.worldToLocalMatrix;
	}
}
