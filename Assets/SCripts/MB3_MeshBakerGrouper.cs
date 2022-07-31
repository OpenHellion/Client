using UnityEngine;

public class MB3_MeshBakerGrouper : MonoBehaviour
{
	public MB3_MeshBakerGrouperCore grouper;

	[HideInInspector]
	public Bounds sourceObjectBounds = new Bounds(Vector3.zero, Vector3.one);

	private void OnDrawGizmosSelected()
	{
		if (grouper == null || grouper.clusterGrouper == null)
		{
			return;
		}
		if (grouper.clusterGrouper.clusterType == MB3_MeshBakerGrouperCore.ClusterType.grid)
		{
			Vector3 cellSize = grouper.clusterGrouper.cellSize;
			if (cellSize.x <= 1E-05f || cellSize.y <= 1E-05f || cellSize.z <= 1E-05f)
			{
				return;
			}
			Vector3 vector = sourceObjectBounds.center - sourceObjectBounds.extents;
			Vector3 origin = grouper.clusterGrouper.origin;
			origin.x %= cellSize.x;
			origin.y %= cellSize.y;
			origin.z %= cellSize.z;
			vector.x = Mathf.Round(vector.x / cellSize.x) * cellSize.x + origin.x;
			vector.y = Mathf.Round(vector.y / cellSize.y) * cellSize.y + origin.y;
			vector.z = Mathf.Round(vector.z / cellSize.z) * cellSize.z + origin.z;
			if (vector.x > sourceObjectBounds.center.x - sourceObjectBounds.extents.x)
			{
				vector.x -= cellSize.x;
			}
			if (vector.y > sourceObjectBounds.center.y - sourceObjectBounds.extents.y)
			{
				vector.y -= cellSize.y;
			}
			if (vector.z > sourceObjectBounds.center.z - sourceObjectBounds.extents.z)
			{
				vector.z -= cellSize.z;
			}
			Vector3 vector2 = vector;
			int num = Mathf.CeilToInt(sourceObjectBounds.size.x / cellSize.x + sourceObjectBounds.size.y / cellSize.y + sourceObjectBounds.size.z / cellSize.z);
			if (num > 200)
			{
				Gizmos.DrawWireCube(grouper.clusterGrouper.origin + cellSize / 2f, cellSize);
			}
			else
			{
				while (vector.x < sourceObjectBounds.center.x + sourceObjectBounds.extents.x)
				{
					vector.y = vector2.y;
					while (vector.y < sourceObjectBounds.center.y + sourceObjectBounds.extents.y)
					{
						vector.z = vector2.z;
						while (vector.z < sourceObjectBounds.center.z + sourceObjectBounds.extents.z)
						{
							Gizmos.DrawWireCube(vector + cellSize / 2f, cellSize);
							vector.z += cellSize.z;
						}
						vector.y += cellSize.y;
					}
					vector.x += cellSize.x;
				}
			}
		}
		if (grouper.clusterGrouper.clusterType == MB3_MeshBakerGrouperCore.ClusterType.pie && !(grouper.clusterGrouper.pieAxis.magnitude < 0.1f) && grouper.clusterGrouper.pieNumSegments >= 1)
		{
			float magnitude = sourceObjectBounds.extents.magnitude;
			DrawCircle(grouper.clusterGrouper.pieAxis, grouper.clusterGrouper.origin, magnitude, 24);
			Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, grouper.clusterGrouper.pieAxis);
			Quaternion quaternion2 = Quaternion.AngleAxis(180f / (float)grouper.clusterGrouper.pieNumSegments, Vector3.up);
			Vector3 vector3 = quaternion2 * Vector3.forward;
			for (int i = 0; i < grouper.clusterGrouper.pieNumSegments; i++)
			{
				Vector3 vector4 = quaternion * vector3;
				Gizmos.DrawLine(grouper.clusterGrouper.origin, grouper.clusterGrouper.origin + vector4 * magnitude);
				vector3 = quaternion2 * vector3;
				vector3 = quaternion2 * vector3;
			}
		}
	}

	public static void DrawCircle(Vector3 axis, Vector3 center, float radius, int subdiv)
	{
		Quaternion quaternion = Quaternion.AngleAxis(360 / subdiv, axis);
		Vector3 vector = new Vector3(axis.y, 0f - axis.x, axis.z);
		vector.Normalize();
		vector *= radius;
		for (int i = 0; i < subdiv + 1; i++)
		{
			Vector3 vector2 = quaternion * vector;
			Gizmos.DrawLine(center + vector, center + vector2);
			vector = vector2;
		}
	}
}
