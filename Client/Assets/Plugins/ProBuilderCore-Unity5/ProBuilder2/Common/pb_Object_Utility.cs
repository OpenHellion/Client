using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProBuilder2.Common
{
	public static class pb_Object_Utility
	{
		public static Vector3[] VerticesInWorldSpace(this pb_Object pb)
		{
			Vector3[] array = new Vector3[pb.vertices.Length];
			Array.Copy(pb.vertices, array, array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				ref Vector3 reference = ref array[i];
				reference = pb.transform.TransformPoint(array[i]);
			}
			return array;
		}

		public static Vector3[] VerticesInWorldSpace(this pb_Object pb, int[] indices)
		{
			if (indices == null)
			{
				Debug.LogWarning("indices == null -> VerticesInWorldSpace");
			}
			Vector3[] array = pb.vertices.ValuesWithIndices(indices);
			for (int i = 0; i < array.Length; i++)
			{
				ref Vector3 reference = ref array[i];
				reference = pb.transform.TransformPoint(array[i]);
			}
			return array;
		}

		public static void TranslateVertices_World(this pb_Object pb, int[] selectedTriangles, Vector3 offset)
		{
			pb.TranslateVertices_World(selectedTriangles, offset, 0f, snapAxisOnly: false, null);
		}

		public static void TranslateVertices_World(this pb_Object pb, int[] selectedTriangles, Vector3 offset, float snapValue, bool snapAxisOnly, Dictionary<int, int> lookup)
		{
			int num = 0;
			int[] array = ((lookup == null) ? pb.sharedIndices.AllIndicesWithValues(selectedTriangles).ToArray() : pb.sharedIndices.AllIndicesWithValues(lookup, selectedTriangles).ToArray());
			Matrix4x4 worldToLocalMatrix = pb.transform.worldToLocalMatrix;
			Vector3 vector = worldToLocalMatrix * offset;
			Vector3[] vertices = pb.vertices;
			if (Mathf.Abs(snapValue) > Mathf.Epsilon)
			{
				Matrix4x4 localToWorldMatrix = pb.transform.localToWorldMatrix;
				Vector3 zero = Vector3.zero;
				Vector3 vector2 = ((!snapAxisOnly) ? Vector3.one : offset.ToMask(0.0001f));
				for (num = 0; num < array.Length; num++)
				{
					zero = localToWorldMatrix.MultiplyPoint3x4(vertices[array[num]] + vector);
					ref Vector3 reference = ref vertices[array[num]];
					reference = worldToLocalMatrix.MultiplyPoint3x4(pb_Snap.SnapValue(zero, snapValue * vector2));
				}
			}
			else
			{
				for (num = 0; num < array.Length; num++)
				{
					vertices[array[num]] += vector;
				}
			}
			pb.SetVertices(vertices);
			pb.msh.vertices = vertices;
		}

		public static void TranslateVertices(this pb_Object pb, int[] selectedTriangles, Vector3 offset)
		{
			int num = 0;
			int[] array = pb.sharedIndices.AllIndicesWithValues(selectedTriangles).ToArray();
			Vector3[] vertices = pb.vertices;
			for (num = 0; num < array.Length; num++)
			{
				vertices[array[num]] += offset;
			}
			pb.SetVertices(vertices);
			pb.msh.vertices = vertices;
		}

		public static void SetSharedVertexPosition(this pb_Object pb, int sharedIndex, Vector3 position)
		{
			Vector3[] vertices = pb.vertices;
			int[] array = pb.sharedIndices[sharedIndex].array;
			for (int i = 0; i < array.Length; i++)
			{
				vertices[array[i]] = position;
			}
			pb.SetVertices(vertices);
			pb.msh.vertices = vertices;
		}

		public static void SetSharedVertexValues(this pb_Object pb, int sharedIndex, pb_Vertex vertex)
		{
			pb_Vertex[] vertices = pb_Vertex.GetVertices(pb);
			int[] array = pb.sharedIndices[sharedIndex].array;
			for (int i = 0; i < array.Length; i++)
			{
				vertices[array[i]] = vertex;
			}
			pb.SetVertices(vertices);
		}

		public static bool FaceWithTriangle(this pb_Object pb, int[] tri, out pb_Face face)
		{
			for (int i = 0; i < pb.faces.Length; i++)
			{
				if (pb.faces[i].Contains(tri))
				{
					face = pb.faces[i];
					return true;
				}
			}
			face = null;
			return false;
		}

		public static bool FaceWithTriangle(this pb_Object pb, int[] tri, out int face)
		{
			for (int i = 0; i < pb.faces.Length; i++)
			{
				if (pb.faces[i].Contains(tri))
				{
					face = i;
					return true;
				}
			}
			face = -1;
			return false;
		}
	}
}
