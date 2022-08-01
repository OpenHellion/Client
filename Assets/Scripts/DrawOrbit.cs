using System;
using System.Collections.Generic;
using UnityEngine;

public class DrawOrbit : MonoBehaviour
{
	public Vector3 orbitCenter;

	private LineRenderer lineRender;

	public float radius = 5f;

	private void Start()
	{
		lineRender = GetComponent<LineRenderer>();
		orbitCenter = base.transform.position;
		List<Vector3> orbitPositions = GetOrbitPositions();
		lineRender.positionCount = orbitPositions.Count + 1;
		for (int i = 0; i < orbitPositions.Count; i++)
		{
			lineRender.SetPosition(i, orbitPositions[i]);
		}
		lineRender.SetPosition(orbitPositions.Count, orbitPositions[0]);
	}

	public List<Vector3> GetOrbitPositions()
	{
		List<Vector3> list = new List<Vector3>();
		int num = 100;
		float num2 = (float)Math.PI * 2f / (float)num;
		for (int i = 0; i < num; i++)
		{
			list.Add(new Vector3(orbitCenter.x + Mathf.Cos(num2 * (float)i) * radius, orbitCenter.y, orbitCenter.z + Mathf.Sin(num2 * (float)i) * radius));
		}
		return list;
	}
}
