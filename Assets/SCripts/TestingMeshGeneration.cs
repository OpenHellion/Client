using System.Collections.Generic;
using UnityEngine;

public class TestingMeshGeneration : MonoBehaviour
{
	private float tUnit = 0.25f;

	private Vector2 tStone = new Vector2(0f, 0f);

	private Vector2 tGrass = new Vector2(0f, 1f);

	public Material GGMat;

	public List<Vector3> newVertices = new List<Vector3>();

	public List<int> newTriangles = new List<int>();

	public List<Vector2> newUV = new List<Vector2>();

	private Mesh mesh;

	private void Start()
	{
		mesh = GetComponent<MeshFilter>().mesh;
		for (int i = 0; i < 4; i++)
		{
		}
		float x = base.transform.position.x;
		float y = base.transform.position.y;
		float z = base.transform.position.z;
		newVertices.Add(new Vector3(x, y, z));
		newVertices.Add(new Vector3(x + 1f, y, z));
		newVertices.Add(new Vector3(x + 1f, y - 1f, z));
		newVertices.Add(new Vector3(x, y - 1f, z));
		newTriangles.Add(0);
		newTriangles.Add(1);
		newTriangles.Add(2);
		newTriangles.Add(0);
		newTriangles.Add(2);
		newTriangles.Add(3);
		newUV.Add(new Vector2(tUnit * tStone.x, tUnit * tStone.y + tUnit));
		newUV.Add(new Vector2(tUnit * tStone.x + tUnit, tUnit * tStone.y + tUnit));
		newUV.Add(new Vector2(tUnit * tStone.x + tUnit, tUnit * tStone.y));
		newUV.Add(new Vector2(tUnit * tStone.x, tUnit * tStone.y));
		mesh.Clear();
		mesh.vertices = newVertices.ToArray();
		mesh.triangles = newTriangles.ToArray();
		mesh.uv = newUV.ToArray();
		mesh.RecalculateNormals();
		GetComponent<MeshRenderer>().material = GGMat;
	}

	private void Update()
	{
	}
}
