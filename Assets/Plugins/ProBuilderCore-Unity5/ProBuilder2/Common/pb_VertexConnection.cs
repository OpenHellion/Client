using System;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.Common
{
	public class pb_VertexConnection : IEquatable<pb_VertexConnection>
	{
		public pb_Face face;

		public List<int> indices;

		public bool isValid => indices != null && indices.Count > 1;

		public pb_VertexConnection(pb_Face face, List<int> indices)
		{
			this.face = face;
			this.indices = indices;
		}

		public pb_VertexConnection Distinct(pb_IntArray[] sharedIndices)
		{
			return new pb_VertexConnection(face, sharedIndices.UniqueIndicesWithValues(indices).ToList());
		}

		public override bool Equals(object b)
		{
			return b is pb_VertexConnection && face == ((pb_VertexConnection)b).face;
		}

		public bool Equals(pb_VertexConnection vc)
		{
			return face == vc.face;
		}

		public static implicit operator pb_Face(pb_VertexConnection vc)
		{
			return vc.face;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return face.ToString() + " : " + pbUtil.ToString(indices);
		}

		public static List<int> AllTriangles(List<pb_VertexConnection> vcs)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < vcs.Count; i++)
			{
				list.AddRange(vcs[i].indices);
			}
			return list;
		}
	}
}
