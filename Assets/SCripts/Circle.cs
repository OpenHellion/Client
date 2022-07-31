using System;
using UnityEngine;

[ExecuteInEditMode]
public class Circle : MonoBehaviour
{
	public int segments;

	public float xradius;

	public float yradius;

	private LineRenderer line;

	private void Update()
	{
		line = base.gameObject.GetComponent<LineRenderer>();
		line.positionCount = segments + 1;
		line.useWorldSpace = false;
		CreatePoints();
	}

	private void CreatePoints()
	{
		float z = 0f;
		float num = 20f;
		for (int i = 0; i < segments + 1; i++)
		{
			float x = Mathf.Sin((float)Math.PI / 180f * num) * xradius;
			float y = Mathf.Cos((float)Math.PI / 180f * num) * yradius;
			line.SetPosition(i, new Vector3(x, y, z));
			num += 360f / (float)segments;
		}
	}
}
