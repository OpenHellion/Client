using System;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	[Obsolete("Use pb_ConnectEdges class directly.")]
	public class pb_EdgeConnection : IEquatable<pb_EdgeConnection>
	{
		public pb_Face face;

		public List<pb_Edge> edges;

		public bool isValid => edges != null && edges.Count > 1;

		public pb_EdgeConnection(pb_Face face, List<pb_Edge> edges)
		{
			this.face = face;
			this.edges = edges;
		}

		public override bool Equals(object b)
		{
			return b is pb_EdgeConnection && face == ((pb_EdgeConnection)b).face;
		}

		public bool Equals(pb_EdgeConnection fc)
		{
			return face == fc.face;
		}

		public static explicit operator pb_Face(pb_EdgeConnection fc)
		{
			return fc.face;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return face.ToString() + " : " + pbUtil.ToString(edges);
		}

		public static List<int> AllTriangles(List<pb_EdgeConnection> ec)
		{
			List<pb_Edge> list = new List<pb_Edge>();
			foreach (pb_EdgeConnection item in ec)
			{
				list.AddRange(item.edges);
			}
			return list.AllTriangles();
		}
	}
}
