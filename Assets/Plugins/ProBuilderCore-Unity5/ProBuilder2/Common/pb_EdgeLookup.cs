using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ProBuilder2.Common
{
	public class pb_EdgeLookup : IEquatable<pb_EdgeLookup>
	{
		[CompilerGenerated]
		private sealed class _003CGetEdgeLookup_003Ec__AnonStorey0
		{
			internal Dictionary<int, int> lookup;

			internal pb_EdgeLookup _003C_003Em__0(pb_Edge x)
			{
				return new pb_EdgeLookup(new pb_Edge(lookup[x.x], lookup[x.y]), x);
			}
		}

		public pb_Edge local;

		public pb_Edge common;

		public pb_EdgeLookup(pb_Edge common, pb_Edge local)
		{
			this.common = common;
			this.local = local;
		}

		public pb_EdgeLookup(int cx, int cy, int x, int y)
		{
			common = new pb_Edge(cx, cy);
			local = new pb_Edge(x, y);
		}

		public bool Equals(pb_EdgeLookup b)
		{
			return common.Equals((!object.ReferenceEquals(b, null)) ? b.common : pb_Edge.Empty);
		}

		public override bool Equals(object b)
		{
			return b is pb_EdgeLookup pb_EdgeLookup2 && common.Equals(pb_EdgeLookup2.common);
		}

		public override int GetHashCode()
		{
			return common.GetHashCode();
		}

		public override string ToString()
		{
			return $"({common.x}, {common.y})";
		}

		public static IEnumerable<pb_EdgeLookup> GetEdgeLookup(IEnumerable<pb_Edge> edges, Dictionary<int, int> lookup)
		{
			_003CGetEdgeLookup_003Ec__AnonStorey0 _003CGetEdgeLookup_003Ec__AnonStorey = new _003CGetEdgeLookup_003Ec__AnonStorey0();
			_003CGetEdgeLookup_003Ec__AnonStorey.lookup = lookup;
			return edges.Select(_003CGetEdgeLookup_003Ec__AnonStorey._003C_003Em__0);
		}
	}
}
