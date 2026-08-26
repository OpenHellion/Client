using System.Collections.Generic;

namespace ProBuilder2.Common
{
	public class pb_FaceRebuildData
	{
		public pb_Face face;

		public List<pb_Vertex> vertices;

		public List<int> sharedIndices;

		public List<int> sharedIndicesUV;

		private int _appliedOffset;

		public int Offset()
		{
			return _appliedOffset;
		}

		public override string ToString()
		{
			return $"{pbUtil.ToString(vertices)}\n{pbUtil.ToString(sharedIndices)}";
		}

		public static void Apply(IEnumerable<pb_FaceRebuildData> newFaces, pb_Object pb, List<pb_Vertex> vertices = null, List<pb_Face> faces = null, Dictionary<int, int> lookup = null, Dictionary<int, int> lookupUV = null)
		{
			List<pb_Face> list = ((faces != null) ? faces : new List<pb_Face>(pb.faces));
			if (vertices == null)
			{
				vertices = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			}
			if (lookup == null)
			{
				lookup = pb.sharedIndices.ToDictionary();
			}
			if (lookupUV == null)
			{
				lookupUV = ((pb.sharedIndicesUV == null) ? null : pb.sharedIndicesUV.ToDictionary());
			}
			Apply(newFaces, vertices, list, lookup, lookupUV);
			pb.SetVertices(vertices);
			pb.SetFaces(list.ToArray());
			pb.SetSharedIndices(lookup);
			pb.SetSharedIndicesUV(lookupUV);
		}

		public static void Apply(IEnumerable<pb_FaceRebuildData> newFaces, List<pb_Vertex> vertices, List<pb_Face> faces, Dictionary<int, int> sharedIndices, Dictionary<int, int> sharedIndicesUV = null)
		{
			int num = vertices.Count;
			foreach (pb_FaceRebuildData newFace in newFaces)
			{
				pb_Face pb_Face2 = newFace.face;
				int count = newFace.vertices.Count;
				bool flag = sharedIndices != null && newFace.sharedIndices != null && newFace.sharedIndices.Count == count;
				bool flag2 = sharedIndicesUV != null && newFace.sharedIndicesUV != null && newFace.sharedIndicesUV.Count == count;
				for (int i = 0; i < count; i++)
				{
					int num2 = i;
					sharedIndices?.Add(num2 + num, (!flag) ? (-1) : newFace.sharedIndices[num2]);
					sharedIndicesUV?.Add(num2 + num, (!flag2) ? (-1) : newFace.sharedIndicesUV[num2]);
				}
				newFace._appliedOffset = num;
				for (int j = 0; j < pb_Face2.indices.Length; j++)
				{
					pb_Face2.indices[j] += num;
				}
				pb_Face2.RebuildCaches();
				num += newFace.vertices.Count;
				faces.Add(pb_Face2);
				vertices.AddRange(newFace.vertices);
			}
		}
	}
}
