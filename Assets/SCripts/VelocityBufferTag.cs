using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Playdead/VelocityBufferTag")]
public class VelocityBufferTag : MonoBehaviour
{
	public static List<VelocityBufferTag> activeObjects = new List<VelocityBufferTag>(128);

	[NonSerialized]
	[HideInInspector]
	public Mesh mesh;

	[NonSerialized]
	[HideInInspector]
	public Matrix4x4 localToWorldPrev;

	[NonSerialized]
	[HideInInspector]
	public Matrix4x4 localToWorldCurr;

	private SkinnedMeshRenderer skinnedMesh;

	public bool useSkinnedMesh;

	public const int framesNotRenderedThreshold = 60;

	private int framesNotRendered = 60;

	[NonSerialized]
	public bool sleeping;

	private void Start()
	{
		if (useSkinnedMesh)
		{
			SkinnedMeshRenderer component = GetComponent<SkinnedMeshRenderer>();
			if (component != null)
			{
				mesh = new Mesh();
				skinnedMesh = component;
				skinnedMesh.BakeMesh(mesh);
			}
		}
		else
		{
			MeshFilter component2 = GetComponent<MeshFilter>();
			if (component2 != null)
			{
				mesh = component2.sharedMesh;
			}
		}
		localToWorldCurr = base.transform.localToWorldMatrix;
		localToWorldPrev = localToWorldCurr;
	}

	private void VelocityUpdate()
	{
		if (useSkinnedMesh)
		{
			if (skinnedMesh == null)
			{
				Debug.LogWarning("vbuf skinnedMesh not set", this);
				return;
			}
			if (sleeping)
			{
				skinnedMesh.BakeMesh(mesh);
				mesh.normals = mesh.vertices;
			}
			else
			{
				Vector3[] vertices = mesh.vertices;
				skinnedMesh.BakeMesh(mesh);
				mesh.normals = vertices;
			}
		}
		if (sleeping)
		{
			localToWorldCurr = base.transform.localToWorldMatrix;
			localToWorldPrev = localToWorldCurr;
		}
		else
		{
			localToWorldPrev = localToWorldCurr;
			localToWorldCurr = base.transform.localToWorldMatrix;
		}
		sleeping = false;
	}

	private void LateUpdate()
	{
		if (framesNotRendered < 60)
		{
			framesNotRendered++;
			VelocityUpdate();
		}
		else
		{
			sleeping = true;
		}
	}

	private void OnWillRenderObject()
	{
		if (!(Camera.current != Camera.main))
		{
			if (sleeping)
			{
				VelocityUpdate();
			}
			framesNotRendered = 0;
		}
	}

	private void OnEnable()
	{
		activeObjects.Add(this);
	}

	private void OnDisable()
	{
		activeObjects.Remove(this);
	}
}
