using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ProBuilder2.Common
{
	public static class EdgeExtensions
	{
		[CompilerGenerated]
		private sealed class _003CDistinctCommon_003Ec__AnonStorey0
		{
			internal Dictionary<int, int> lookup;

			internal pb_EdgeLookup _003C_003Em__0(pb_Edge x)
			{
				return new pb_EdgeLookup(new pb_Edge(lookup[x.x], lookup[x.y]), x);
			}
		}

		[CompilerGenerated]
		private static Func<pb_EdgeLookup, pb_Edge> _003C_003Ef__am_0024cache0;

		public static bool ContainsDuplicate(this List<pb_Edge> edges, pb_Edge edge, Dictionary<int, int> lookup)
		{
			int num = 0;
			for (int i = 0; i < edges.Count; i++)
			{
				if (edges[i].Equals(edge, lookup) && ++num > 1)
				{
					return true;
				}
			}
			return false;
		}

		public static bool Contains(this pb_Edge[] edges, pb_Edge edge)
		{
			for (int i = 0; i < edges.Length; i++)
			{
				if (edges[i].Equals(edge))
				{
					return true;
				}
			}
			return false;
		}

		public static bool Contains(this pb_Edge[] edges, int x, int y)
		{
			for (int i = 0; i < edges.Length; i++)
			{
				if ((x == edges[i].x && y == edges[i].y) || (x == edges[i].y && y == edges[i].x))
				{
					return true;
				}
			}
			return false;
		}

		public static IEnumerable<pb_Edge> DistinctCommon(this IEnumerable<pb_Edge> edges, Dictionary<int, int> lookup)
		{
			_003CDistinctCommon_003Ec__AnonStorey0 _003CDistinctCommon_003Ec__AnonStorey = new _003CDistinctCommon_003Ec__AnonStorey0();
			_003CDistinctCommon_003Ec__AnonStorey.lookup = lookup;
			IEnumerable<pb_EdgeLookup> source = edges.Select(_003CDistinctCommon_003Ec__AnonStorey._003C_003Em__0);
			source = source.Distinct();
			IEnumerable<pb_EdgeLookup> source2 = source;
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CDistinctCommon_003Em__0;
			}
			return source2.Select(_003C_003Ef__am_0024cache0);
		}

		public static int IndexOf(this IList<pb_Edge> edges, pb_Edge edge, Dictionary<int, int> lookup)
		{
			for (int i = 0; i < edges.Count; i++)
			{
				if (edges[i].Equals(edge, lookup))
				{
					return i;
				}
			}
			return -1;
		}

		public static List<int> ToIntList(this List<pb_Edge> edges)
		{
			List<int> list = new List<int>();
			foreach (pb_Edge edge in edges)
			{
				list.Add(edge.x);
				list.Add(edge.y);
			}
			return list;
		}

		public static int[] AllTriangles(this pb_Edge[] edges)
		{
			int[] array = new int[edges.Length * 2];
			int num = 0;
			for (int i = 0; i < edges.Length; i++)
			{
				array[num++] = edges[i].x;
				array[num++] = edges[i].y;
			}
			return array;
		}

		public static List<int> AllTriangles(this List<pb_Edge> edges)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < edges.Count; i++)
			{
				list.Add(edges[i].x);
				list.Add(edges[i].y);
			}
			return list;
		}

		[CompilerGenerated]
		private static pb_Edge _003CDistinctCommon_003Em__0(pb_EdgeLookup x)
		{
			return x.local;
		}
	}
}
