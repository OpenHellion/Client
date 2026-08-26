using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ProBuilder2.Common
{
	public static class pb_MeshCompiler
	{
		[CompilerGenerated]
		private static Func<pb_Submesh, Material> _003C_003Ef__am_0024cache0;

		public static void Compile(pb_Object pb, ref Mesh target, out Material[] materials, MeshTopology preferredTopology = MeshTopology.Triangles)
		{
			target.Clear();
			target.vertices = pb.vertices;
			target.uv = GetUVs(pb);
			if (pb.hasUv3)
			{
				target.SetUVs(2, pb.uv3);
			}
			if (pb.hasUv4)
			{
				target.SetUVs(3, pb.uv4);
			}
			Vector3[] normals = pb_MeshUtility.GenerateNormals(pb);
			pb_MeshUtility.SmoothNormals(pb, ref normals);
			target.normals = normals;
			pb_MeshUtility.GenerateTangent(ref target);
			if (pb.colors != null && pb.colors.Length == target.vertexCount)
			{
				target.colors = pb.colors;
			}
			target.subMeshCount = pb_Face.GetMeshIndices(pb.faces, out var submeshes, preferredTopology);
			for (int i = 0; i < target.subMeshCount; i++)
			{
				target.SetIndices(submeshes[i].indices, submeshes[i].topology, i, calculateBounds: false);
			}
			target.name = $"pb_Mesh{pb.id}";
			pb_Submesh[] source = submeshes;
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CCompile_003Em__0;
			}
			materials = source.Select(_003C_003Ef__am_0024cache0).ToArray();
		}

		internal static Vector2[] GetUVs(pb_Object pb)
		{
			int num = -2;
			Dictionary<int, List<pb_Face>> dictionary = new Dictionary<int, List<pb_Face>>();
			bool flag = false;
			pb_Face[] faces = pb.faces;
			foreach (pb_Face pb_Face2 in faces)
			{
				if (pb_Face2.uv.useWorldSpace)
				{
					flag = true;
				}
				if (pb_Face2 != null && !pb_Face2.manualUV)
				{
					if (pb_Face2.textureGroup > 0 && dictionary.TryGetValue(pb_Face2.textureGroup, out var value))
					{
						value.Add(pb_Face2);
						continue;
					}
					dictionary.Add((pb_Face2.textureGroup <= 0) ? num-- : pb_Face2.textureGroup, new List<pb_Face> { pb_Face2 });
				}
			}
			num = 0;
			Vector3[] verts = ((!flag) ? null : pb.transform.ToWorldSpace(pb.vertices));
			Vector2[] array = ((pb.uv == null || pb.uv.Length != pb.vertexCount) ? new Vector2[pb.vertexCount] : pb.uv);
			foreach (KeyValuePair<int, List<pb_Face>> item in dictionary)
			{
				int[] indices = pb_Face.AllTrianglesDistinct(item.Value).ToArray();
				Vector3 vector = ((item.Value.Count <= 1) ? pb_Math.Normal(pb, item.Value[0]) : pb_Projection.FindBestPlane(pb.vertices, indices).normal);
				if (item.Value[0].uv.useWorldSpace)
				{
					pb_UVUtility.PlanarMap2(verts, array, indices, item.Value[0].uv, pb.transform.TransformDirection(vector));
				}
				else
				{
					pb_UVUtility.PlanarMap2(pb.vertices, array, indices, item.Value[0].uv, vector);
				}
				Vector2 localPivot = item.Value[0].uv.localPivot;
				foreach (pb_Face item2 in item.Value)
				{
					item2.uv.localPivot = localPivot;
				}
			}
			return array;
		}

		[CompilerGenerated]
		private static Material _003CCompile_003Em__0(pb_Submesh x)
		{
			return x.material;
		}
	}
}
