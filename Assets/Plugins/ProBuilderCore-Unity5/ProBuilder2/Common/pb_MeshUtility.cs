using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace ProBuilder2.Common
{
	public class pb_MeshUtility
	{
		[CompilerGenerated]
		private static Func<Dictionary<pb_Vertex, int>, IEnumerable<pb_Vertex>> _003C_003Ef__am_0024cache0;

		public static pb_Vertex[] GeneratePerTriangleMesh(Mesh m)
		{
			pb_Vertex[] vertices = pb_Vertex.GetVertices(m);
			int subMeshCount = m.subMeshCount;
			pb_Vertex[] array = new pb_Vertex[m.triangles.Length];
			int[][] array2 = new int[subMeshCount][];
			int num = 0;
			for (int i = 0; i < subMeshCount; i++)
			{
				array2[i] = m.GetTriangles(i);
				int num2 = array2[i].Length;
				for (int j = 0; j < num2; j++)
				{
					array[num++] = new pb_Vertex(vertices[array2[i][j]]);
					array2[i][j] = num - 1;
				}
			}
			pb_Vertex.SetMesh(m, array);
			m.subMeshCount = subMeshCount;
			for (int k = 0; k < subMeshCount; k++)
			{
				m.SetTriangles(array2[k], k);
			}
			return array;
		}

		public static void CollapseSharedVertices(Mesh m, pb_Vertex[] vertices = null)
		{
			if (vertices == null)
			{
				vertices = pb_Vertex.GetVertices(m);
			}
			int subMeshCount = m.subMeshCount;
			List<Dictionary<pb_Vertex, int>> list = new List<Dictionary<pb_Vertex, int>>();
			int[][] array = new int[subMeshCount][];
			int num = 0;
			for (int i = 0; i < subMeshCount; i++)
			{
				array[i] = m.GetTriangles(i);
				Dictionary<pb_Vertex, int> dictionary = new Dictionary<pb_Vertex, int>();
				for (int j = 0; j < array[i].Length; j++)
				{
					pb_Vertex key = vertices[array[i][j]];
					if (dictionary.TryGetValue(key, out var value))
					{
						array[i][j] = value;
						continue;
					}
					array[i][j] = num;
					dictionary.Add(key, num);
					num++;
				}
				list.Add(dictionary);
			}
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CCollapseSharedVertices_003Em__0;
			}
			pb_Vertex[] vertices2 = list.SelectMany(_003C_003Ef__am_0024cache0).ToArray();
			pb_Vertex.SetMesh(m, vertices2);
			m.subMeshCount = subMeshCount;
			for (int k = 0; k < subMeshCount; k++)
			{
				m.SetTriangles(array[k], k);
			}
		}

		public static void GenerateTangent(ref Mesh InMesh)
		{
			int[] triangles = InMesh.triangles;
			Vector3[] vertices = InMesh.vertices;
			Vector2[] uv = InMesh.uv;
			Vector3[] normals = InMesh.normals;
			int num = triangles.Length;
			int num2 = vertices.Length;
			Vector3[] array = new Vector3[num2];
			Vector3[] array2 = new Vector3[num2];
			Vector4[] array3 = new Vector4[num2];
			for (long num3 = 0L; num3 < num; num3 += 3)
			{
				long num4 = triangles[num3];
				long num5 = triangles[num3 + 1];
				long num6 = triangles[num3 + 2];
				Vector3 vector = vertices[num4];
				Vector3 vector2 = vertices[num5];
				Vector3 vector3 = vertices[num6];
				Vector2 vector4 = uv[num4];
				Vector2 vector5 = uv[num5];
				Vector2 vector6 = uv[num6];
				float num7 = vector2.x - vector.x;
				float num8 = vector3.x - vector.x;
				float num9 = vector2.y - vector.y;
				float num10 = vector3.y - vector.y;
				float num11 = vector2.z - vector.z;
				float num12 = vector3.z - vector.z;
				float num13 = vector5.x - vector4.x;
				float num14 = vector6.x - vector4.x;
				float num15 = vector5.y - vector4.y;
				float num16 = vector6.y - vector4.y;
				float num17 = 1f / (num13 * num16 - num14 * num15);
				Vector3 vector7 = new Vector3((num16 * num7 - num15 * num8) * num17, (num16 * num9 - num15 * num10) * num17, (num16 * num11 - num15 * num12) * num17);
				Vector3 vector8 = new Vector3((num13 * num8 - num14 * num7) * num17, (num13 * num10 - num14 * num9) * num17, (num13 * num12 - num14 * num11) * num17);
				array[num4] += vector7;
				array[num5] += vector7;
				array[num6] += vector7;
				array2[num4] += vector8;
				array2[num5] += vector8;
				array2[num6] += vector8;
			}
			for (long num18 = 0L; num18 < num2; num18++)
			{
				Vector3 normal = normals[num18];
				Vector3 tangent = array[num18];
				Vector3.OrthoNormalize(ref normal, ref tangent);
				array3[num18].x = tangent.x;
				array3[num18].y = tangent.y;
				array3[num18].z = tangent.z;
				array3[num18].w = ((!(Vector3.Dot(Vector3.Cross(normal, tangent), array2[num18]) < 0f)) ? 1f : (-1f));
			}
			InMesh.tangents = array3;
		}

		public static Mesh DeepCopy(Mesh mesh)
		{
			Mesh mesh2 = new Mesh();
			CopyTo(mesh, mesh2);
			return mesh2;
		}

		public static void CopyTo(Mesh source, Mesh destination)
		{
			Vector3[] array = new Vector3[source.vertices.Length];
			int[][] array2 = new int[source.subMeshCount][];
			Vector2[] array3 = new Vector2[source.uv.Length];
			Vector2[] array4 = new Vector2[source.uv2.Length];
			Vector4[] array5 = new Vector4[source.tangents.Length];
			Vector3[] array6 = new Vector3[source.normals.Length];
			Color32[] array7 = new Color32[source.colors32.Length];
			Array.Copy(source.vertices, array, array.Length);
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = source.GetTriangles(i);
			}
			Array.Copy(source.uv, array3, array3.Length);
			Array.Copy(source.uv2, array4, array4.Length);
			Array.Copy(source.normals, array6, array6.Length);
			Array.Copy(source.tangents, array5, array5.Length);
			Array.Copy(source.colors32, array7, array7.Length);
			destination.Clear();
			destination.name = source.name;
			destination.vertices = array;
			destination.subMeshCount = array2.Length;
			for (int j = 0; j < array2.Length; j++)
			{
				destination.SetTriangles(array2[j], j);
			}
			destination.uv = array3;
			destination.uv2 = array4;
			destination.tangents = array5;
			destination.normals = array6;
			destination.colors32 = array7;
		}

		public static Vector3[] GenerateNormals(pb_Object pb)
		{
			int vertexCount = pb.vertexCount;
			Vector3[] array = new Vector3[vertexCount];
			Vector3[] vertices = pb.vertices;
			Vector3[] array2 = new Vector3[vertexCount];
			int[] array3 = new int[vertexCount];
			pb_Face[] faces = pb.faces;
			for (int i = 0; i < faces.Length; i++)
			{
				int[] indices = faces[i].indices;
				for (int j = 0; j < indices.Length; j += 3)
				{
					int num = indices[j];
					int num2 = indices[j + 1];
					int num3 = indices[j + 2];
					Vector3 vector = pb_Math.Normal(vertices[num], vertices[num2], vertices[num3]);
					array[num].x += vector.x;
					array[num2].x += vector.x;
					array[num3].x += vector.x;
					array[num].y += vector.y;
					array[num2].y += vector.y;
					array[num3].y += vector.y;
					array[num].z += vector.z;
					array[num2].z += vector.z;
					array[num3].z += vector.z;
					array3[num]++;
					array3[num2]++;
					array3[num3]++;
				}
			}
			for (int k = 0; k < vertexCount; k++)
			{
				array2[k].x = array[k].x * (float)array3[k];
				array2[k].y = array[k].y * (float)array3[k];
				array2[k].z = array[k].z * (float)array3[k];
			}
			return array2;
		}

		public static void SmoothNormals(pb_Object pb, ref Vector3[] normals)
		{
			int vertexCount = pb.vertexCount;
			int[] array = new int[vertexCount];
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			pb_Face[] faces = pb.faces;
			int num = 24;
			pb_Face[] array2 = faces;
			foreach (pb_Face pb_Face2 in array2)
			{
				int[] distinctIndices = pb_Face2.distinctIndices;
				foreach (int num2 in distinctIndices)
				{
					array[num2] = pb_Face2.smoothingGroup;
					if (pb_Face2.smoothingGroup >= num)
					{
						num = pb_Face2.smoothingGroup + 1;
					}
				}
			}
			Vector3[] array3 = new Vector3[num];
			float[] array4 = new float[num];
			for (int k = 0; k < sharedIndices.Length; k++)
			{
				for (int l = 0; l < num; l++)
				{
					array3[l].x = 0f;
					array3[l].y = 0f;
					array3[l].z = 0f;
					array4[l] = 0f;
				}
				for (int m = 0; m < sharedIndices[k].array.Length; m++)
				{
					int num3 = sharedIndices[k].array[m];
					int num4 = array[num3];
					if (num4 > 0 && (num4 <= 24 || num4 >= 42))
					{
						array3[num4].x += normals[num3].x;
						array3[num4].y += normals[num3].y;
						array3[num4].z += normals[num3].z;
						array4[num4] += 1f;
					}
				}
				for (int n = 0; n < sharedIndices[k].array.Length; n++)
				{
					int num5 = sharedIndices[k].array[n];
					int num6 = array[num5];
					if (num6 > 0 && (num6 <= 24 || num6 >= 42))
					{
						normals[num5].x = array3[num6].x / array4[num6];
						normals[num5].y = array3[num6].y / array4[num6];
						normals[num5].z = array3[num6].z / array4[num6];
					}
				}
			}
		}

		public static T GetMeshAttribute<T>(GameObject go, Func<Mesh, T> attributeGetter) where T : IList
		{
			MeshFilter component = go.GetComponent<MeshFilter>();
			Mesh mesh = ((!(component != null)) ? null : component.sharedMesh);
			T result = default(T);
			if (mesh == null)
			{
				return result;
			}
			int vertexCount = mesh.vertexCount;
			MeshRenderer component2 = go.GetComponent<MeshRenderer>();
			Mesh mesh2 = ((!(component2 != null)) ? null : component2.additionalVertexStreams);
			if (mesh2 != null)
			{
				result = attributeGetter(mesh2);
				if (result != null && result.Count == vertexCount)
				{
					return result;
				}
			}
			result = attributeGetter(mesh);
			return (result == null || result.Count != vertexCount) ? default(T) : result;
		}

		public static string Print(Mesh m)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"vertices: {m.vertexCount}\ntriangles: {m.triangles.Length}\nsubmeshes: {m.subMeshCount}");
			stringBuilder.AppendLine(string.Format("     {0,-28}{7,-16}{1,-28}{2,-28}{3,-28}{4,-28}{5,-28}{6,-28}", "Positions", "Colors", "Tangents", "UV0", "UV2", "UV3", "UV4", "Position Hash"));
			Vector3[] array = m.vertices;
			Color[] array2 = m.colors;
			Vector4[] array3 = m.tangents;
			List<Vector4> list = new List<Vector4>();
			Vector2[] array4 = m.uv2;
			List<Vector4> list2 = new List<Vector4>();
			List<Vector4> list3 = new List<Vector4>();
			m.GetUVs(0, list);
			m.GetUVs(2, list2);
			m.GetUVs(3, list3);
			if (array != null && array.Count() != m.vertexCount)
			{
				array = null;
			}
			if (array2 != null && array2.Count() != m.vertexCount)
			{
				array2 = null;
			}
			if (array3 != null && array3.Count() != m.vertexCount)
			{
				array3 = null;
			}
			if (list != null && list.Count() != m.vertexCount)
			{
				list = null;
			}
			if (array4 != null && array4.Count() != m.vertexCount)
			{
				array4 = null;
			}
			if (list2 != null && list2.Count() != m.vertexCount)
			{
				list2 = null;
			}
			if (list3 != null && list3.Count() != m.vertexCount)
			{
				list3 = null;
			}
			for (int i = 0; i < m.vertexCount; i++)
			{
				stringBuilder.AppendLine(string.Format("{7,-5}{0,-28}{8,-16}{1,-28}{2,-28}{3,-28}{4,-28}{5,-28}{6,-28}", (array != null) ? $"{array[i].x:F3}, {array[i].y:F3}, {array[i].z:F3}" : "null", (array2 != null) ? $"{array2[i].r:F2}, {array2[i].g:F2}, {array2[i].b:F2}, {array2[i].a:F2}" : "null", (array3 != null) ? $"{array3[i].x:F2}, {array3[i].y:F2}, {array3[i].z:F2}, {array3[i].w:F2}" : "null", (list != null) ? $"{list[i].x:F2}, {list[i].y:F2}, {list[i].z:F2}, {list[i].w:F2}" : "null", (array4 != null) ? $"{array4[i].x:F2}, {array4[i].y:F2}" : "null", (list2 != null) ? $"{list2[i].x:F2}, {list2[i].y:F2}, {list2[i].z:F2}, {list2[i].w:F2}" : "null", (list3 != null) ? $"{list3[i].x:F2}, {list3[i].y:F2}, {list3[i].z:F2}, {list3[i].w:F2}" : "null", i, pb_Vector.GetHashCode(array[i])));
			}
			for (int j = 0; j < m.triangles.Length; j += 3)
			{
				stringBuilder.AppendLine($"{m.triangles[j]}, {m.triangles[j + 1]}, {m.triangles[j + 2]}");
			}
			return stringBuilder.ToString();
		}

		[CompilerGenerated]
		private static IEnumerable<pb_Vertex> _003CCollapseSharedVertices_003Em__0(Dictionary<pb_Vertex, int> x)
		{
			return x.Keys;
		}
	}
}
