using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ProBuilder2.Common
{
	public class pb_WingedEdge : IEquatable<pb_WingedEdge>, IEnumerable
	{
		[CompilerGenerated]
		private sealed class _003CSortCommonIndicesByAdjacency_003Ec__AnonStorey0
		{
			internal HashSet<int> common;

			internal bool _003C_003Em__0(pb_WingedEdge x)
			{
				return common.Contains(x.edge.common.x) && common.Contains(x.edge.common.y);
			}
		}

		public pb_EdgeLookup edge;

		public pb_Face face;

		public pb_WingedEdge next;

		public pb_WingedEdge previous;

		public pb_WingedEdge opposite;

		[CompilerGenerated]
		private static Func<pb_WingedEdge, pb_Edge> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<pb_Edge, int> _003C_003Ef__am_0024cache1;

		public bool Equals(pb_WingedEdge b)
		{
			return b != null && edge.local.Equals(b.edge.local);
		}

		public override bool Equals(object b)
		{
			if (b is pb_WingedEdge b2 && Equals(b2))
			{
				return true;
			}
			if (b is pb_Edge && edge.local.Equals((pb_Edge)b))
			{
				return true;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return edge.local.GetHashCode();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public pb_WingedEdgeEnumerator GetEnumerator()
		{
			return new pb_WingedEdgeEnumerator(this);
		}

		public int Count()
		{
			pb_WingedEdge pb_WingedEdge2 = this;
			int num = 0;
			do
			{
				num++;
				pb_WingedEdge2 = pb_WingedEdge2.next;
			}
			while (pb_WingedEdge2 != null && pb_WingedEdge2 != this);
			return num;
		}

		public override string ToString()
		{
			return string.Format("Common: {0}\nLocal: {1}\nOpposite: {2}\nFace: {3}", edge.common.ToString(), edge.local.ToString(), (opposite != null) ? opposite.edge.ToString() : "null", face.ToString());
		}

		public static int[] MakeQuad(pb_WingedEdge left, pb_WingedEdge right)
		{
			if (left.Count() != 3 || right.Count() != 3)
			{
				return null;
			}
			pb_EdgeLookup[] array = new pb_EdgeLookup[6]
			{
				left.edge,
				left.next.edge,
				left.next.next.edge,
				right.edge,
				right.next.edge,
				right.next.next.edge
			};
			int[] array2 = new int[6];
			int num = 0;
			for (int i = 0; i < 3; i++)
			{
				for (int j = 3; j < 6; j++)
				{
					if (array[i].Equals(array[j]))
					{
						num++;
						array2[i] = 1;
						array2[j] = 1;
						break;
					}
				}
			}
			if (num != 1)
			{
				return null;
			}
			int num2 = 0;
			pb_EdgeLookup[] array3 = new pb_EdgeLookup[4];
			for (int k = 0; k < 6; k++)
			{
				if (array2[k] < 1)
				{
					array3[num2++] = array[k];
				}
			}
			int[] array4 = new int[4]
			{
				array3[0].local.x,
				array3[0].local.y,
				-1,
				-1
			};
			int y = array3[0].common.y;
			int num3 = -1;
			if (array3[1].common.x == y)
			{
				array4[2] = array3[1].local.y;
				num3 = array3[1].common.y;
			}
			else if (array3[2].common.x == y)
			{
				array4[2] = array3[2].local.y;
				num3 = array3[2].common.y;
			}
			else if (array3[3].common.x == y)
			{
				array4[2] = array3[3].local.y;
				num3 = array3[3].common.y;
			}
			if (array3[1].common.x == num3)
			{
				array4[3] = array3[1].local.y;
			}
			else if (array3[2].common.x == num3)
			{
				array4[3] = array3[2].local.y;
			}
			else if (array3[3].common.x == num3)
			{
				array4[3] = array3[3].local.y;
			}
			if (array4[2] == -1 || array4[3] == -1)
			{
				return null;
			}
			return array4;
		}

		public pb_WingedEdge GetAdjacentEdgeWithCommonIndex(int common)
		{
			if (next.edge.common.Contains(common))
			{
				return next;
			}
			if (previous.edge.common.Contains(common))
			{
				return previous;
			}
			return null;
		}

		public static List<pb_Edge> SortEdgesByAdjacency(pb_Face face)
		{
			List<pb_Edge> edges = new List<pb_Edge>(face.edges);
			return SortEdgesByAdjacency(edges);
		}

		public static List<pb_Edge> SortEdgesByAdjacency(List<pb_Edge> edges)
		{
			for (int i = 1; i < edges.Count; i++)
			{
				int y = edges[i - 1].y;
				for (int j = i + 1; j < edges.Count; j++)
				{
					if (edges[j].x == y || edges[j].y == y)
					{
						pb_Edge value = edges[j];
						edges[j] = edges[i];
						edges[i] = value;
					}
				}
			}
			return edges;
		}

		public static Dictionary<int, List<pb_WingedEdge>> GetSpokes(List<pb_WingedEdge> wings)
		{
			Dictionary<int, List<pb_WingedEdge>> dictionary = new Dictionary<int, List<pb_WingedEdge>>();
			List<pb_WingedEdge> value = null;
			for (int i = 0; i < wings.Count; i++)
			{
				if (dictionary.TryGetValue(wings[i].edge.common.x, out value))
				{
					value.Add(wings[i]);
				}
				else
				{
					dictionary.Add(wings[i].edge.common.x, new List<pb_WingedEdge> { wings[i] });
				}
				if (dictionary.TryGetValue(wings[i].edge.common.y, out value))
				{
					value.Add(wings[i]);
					continue;
				}
				dictionary.Add(wings[i].edge.common.y, new List<pb_WingedEdge> { wings[i] });
			}
			return dictionary;
		}

		public static List<int> SortCommonIndicesByAdjacency(List<pb_WingedEdge> wings, HashSet<int> common)
		{
			_003CSortCommonIndicesByAdjacency_003Ec__AnonStorey0 _003CSortCommonIndicesByAdjacency_003Ec__AnonStorey = new _003CSortCommonIndicesByAdjacency_003Ec__AnonStorey0();
			_003CSortCommonIndicesByAdjacency_003Ec__AnonStorey.common = common;
			IEnumerable<pb_WingedEdge> source = wings.Where(_003CSortCommonIndicesByAdjacency_003Ec__AnonStorey._003C_003Em__0);
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CSortCommonIndicesByAdjacency_003Em__0;
			}
			List<pb_Edge> list = source.Select(_003C_003Ef__am_0024cache0).ToList();
			if (list.Count != _003CSortCommonIndicesByAdjacency_003Ec__AnonStorey.common.Count)
			{
				return null;
			}
			List<pb_Edge> source2 = SortEdgesByAdjacency(list);
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CSortCommonIndicesByAdjacency_003Em__1;
			}
			return source2.Select(_003C_003Ef__am_0024cache1).ToList();
		}

		public static List<pb_WingedEdge> GetWingedEdges(pb_Object pb, bool oneWingPerFace = false)
		{
			return GetWingedEdges(pb, pb.faces, oneWingPerFace);
		}

		public static List<pb_WingedEdge> GetWingedEdges(pb_Object pb, IEnumerable<pb_Face> faces, bool oneWingPerFace = false, Dictionary<int, int> sharedIndexLookup = null)
		{
			Dictionary<int, int> dictionary = ((sharedIndexLookup != null) ? sharedIndexLookup : pb.sharedIndices.ToDictionary());
			IEnumerable<pb_Face> enumerable = faces.Distinct();
			List<pb_WingedEdge> list = new List<pb_WingedEdge>();
			Dictionary<pb_Edge, pb_WingedEdge> dictionary2 = new Dictionary<pb_Edge, pb_WingedEdge>();
			int num = 0;
			foreach (pb_Face item in enumerable)
			{
				List<pb_Edge> list2 = SortEdgesByAdjacency(item);
				int count = list2.Count;
				pb_WingedEdge pb_WingedEdge2 = null;
				pb_WingedEdge pb_WingedEdge3 = null;
				for (int i = 0; i < count; i++)
				{
					pb_Edge pb_Edge2 = list2[i];
					pb_WingedEdge pb_WingedEdge4 = new pb_WingedEdge();
					pb_WingedEdge4.edge = new pb_EdgeLookup(dictionary[pb_Edge2.x], dictionary[pb_Edge2.y], pb_Edge2.x, pb_Edge2.y);
					pb_WingedEdge4.face = item;
					if (i < 1)
					{
						pb_WingedEdge2 = pb_WingedEdge4;
					}
					if (i > 0)
					{
						pb_WingedEdge4.previous = pb_WingedEdge3;
						pb_WingedEdge3.next = pb_WingedEdge4;
					}
					if (i == count - 1)
					{
						pb_WingedEdge4.next = pb_WingedEdge2;
						pb_WingedEdge2.previous = pb_WingedEdge4;
					}
					pb_WingedEdge3 = pb_WingedEdge4;
					if (dictionary2.TryGetValue(pb_WingedEdge4.edge.common, out var value))
					{
						value.opposite = pb_WingedEdge4;
						pb_WingedEdge4.opposite = value;
					}
					else
					{
						pb_WingedEdge4.opposite = null;
						dictionary2.Add(pb_WingedEdge4.edge.common, pb_WingedEdge4);
					}
					if (!oneWingPerFace || i < 1)
					{
						list.Add(pb_WingedEdge4);
					}
				}
				num += count;
			}
			return list;
		}

		[CompilerGenerated]
		private static pb_Edge _003CSortCommonIndicesByAdjacency_003Em__0(pb_WingedEdge y)
		{
			return y.edge.common;
		}

		[CompilerGenerated]
		private static int _003CSortCommonIndicesByAdjacency_003Em__1(pb_Edge x)
		{
			return x.x;
		}
	}
}
