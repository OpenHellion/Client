using UnityEngine.ProBuilder;
using UnityEngine;
using System.Collections.ObjectModel;

public class HighlightNearestFace : MonoBehaviour
{
	public float travel = 50f;

	public float speed = 0.2f;

	private ProBuilderMesh target;

	private Face nearest;

	private void Start()
	{
		target = ShapeGenerator.GeneratePlane(PivotLocation.Center, travel, travel, 25, 25, Axis.Up);
		target.SetMaterial(target.faces, BuiltinMaterials.defaultMaterial);
		target.transform.position = new Vector3(travel * 0.5f, 0f, travel * 0.5f);
		target.ToMesh();
		target.Refresh();
		Camera main = Camera.main;
		main.transform.position = new Vector3(25f, 40f, 0f);
		main.transform.localRotation = Quaternion.Euler(new Vector3(65f, 0f, 0f));
	}

	private void Update()
	{
		float num = Time.time * speed;
		Vector3 position = new Vector3(Mathf.PerlinNoise(num, num) * travel, 2f, Mathf.PerlinNoise(num + 1f, num + 1f) * travel);
		base.transform.position = position;
		if (target == null)
		{
			Debug.LogWarning("Missing the ProBuilder Mesh target!");
			return;
		}
		Vector3 a = target.transform.InverseTransformPoint(base.transform.position);
		if (nearest != null)
		{
			target.SetFaceColor(nearest, Color.white);
		}
		int num2 = target.faces.Count;
		float num3 = float.PositiveInfinity;
		nearest = target.faces[0];
		for (int i = 0; i < num2; i++)
		{
			float num4 = Vector3.Distance(a, FaceCenter(target, target.faces[i]));
			if (num4 < num3)
			{
				num3 = num4;
				nearest = target.faces[i];
			}
		}
		target.SetFaceColor(nearest, Color.blue);
		target.Refresh();
	}

	private Vector3 FaceCenter(ProBuilderMesh pb, Face face)
	{
		Vector3[] vertices = pb.VerticesInWorldSpace();
		Vector3 zero = Vector3.zero;
		ReadOnlyCollection<int> distinctIndices = face.distinctIndexes;
		foreach (int num in distinctIndices)
		{
			zero.x += vertices[num].x;
			zero.y += vertices[num].y;
			zero.z += vertices[num].z;
		}
		float num2 = face.distinctIndexes.Count;
		zero.x /= num2;
		zero.y /= num2;
		zero.z /= num2;
		return zero;
	}
}
