using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ProBuilder2.Common
{
	[Serializable]
	public struct pb_Edge : IEquatable<pb_Edge>
	{
		[CompilerGenerated]
		private sealed class _003CGetPerimeterEdges_003Ec__AnonStorey0
		{
			internal int[] count;

			internal bool _003C_003Em__0(pb_Edge val, int index)
			{
				return count[index] < 1;
			}
		}

		public int x;

		public int y;

		public static readonly pb_Edge Empty = new pb_Edge(-1, -1);

		public pb_Edge(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public pb_Edge(pb_Edge edge)
		{
			x = edge.x;
			y = edge.y;
		}

		public bool IsValid()
		{
			return x > -1 && y > -1 && x != y;
		}

		public override string ToString()
		{
			return "[" + x + ", " + y + "]";
		}

		public bool Equals(pb_Edge edge)
		{
			return (x == edge.x && y == edge.y) || (x == edge.y && y == edge.x);
		}

		public override bool Equals(object b)
		{
			return b is pb_Edge && Equals((pb_Edge)b);
		}

		public override int GetHashCode()
		{
			int num = 27;
			num = num * 29 + ((x >= y) ? y : x);
			return num * 29 + ((x >= y) ? x : y);
		}

		public static pb_Edge operator +(pb_Edge a, pb_Edge b)
		{
			return new pb_Edge(a.x + b.x, a.y + b.y);
		}

		public static pb_Edge operator -(pb_Edge a, pb_Edge b)
		{
			return new pb_Edge(a.x - b.x, a.y - b.y);
		}

		public static pb_Edge operator +(pb_Edge a, int b)
		{
			return new pb_Edge(a.x + b, a.y + b);
		}

		public static pb_Edge operator -(pb_Edge a, int b)
		{
			return new pb_Edge(a.x - b, a.y - b);
		}

		public static bool operator ==(pb_Edge a, pb_Edge b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(pb_Edge a, pb_Edge b)
		{
			return !(a == b);
		}

		public int[] ToArray()
		{
			return new int[2] { x, y };
		}

		public bool Equals(pb_Edge b, Dictionary<int, int> lookup)
		{
			int num = lookup[x];
			int num2 = lookup[y];
			int num3 = lookup[b.x];
			int num4 = lookup[b.y];
			return (num == num3 && num2 == num4) || (num == num4 && num2 == num3);
		}

		public bool Contains(int a)
		{
			return x == a || y == a;
		}

		public bool Contains(pb_Edge b)
		{
			return x == b.x || y == b.x || x == b.y || y == b.x;
		}

		public bool Contains(int a, pb_IntArray[] sharedIndices)
		{
			int num = sharedIndices.IndexOf(a);
			return Array.IndexOf(sharedIndices[num], x) > -1 || Array.IndexOf(sharedIndices[num], y) > -1;
		}

		public static pb_Edge[] GetUniversalEdges(pb_Edge[] edges, Dictionary<int, int> sharedIndicesLookup)
		{
			pb_Edge[] array = new pb_Edge[edges.Length];
			for (int i = 0; i < edges.Length; i++)
			{
				ref pb_Edge reference = ref array[i];
				reference = new pb_Edge(sharedIndicesLookup[edges[i].x], sharedIndicesLookup[edges[i].y]);
			}
			return array;
		}

		public static pb_Edge[] GetUniversalEdges(pb_Edge[] edges, pb_IntArray[] sharedIndices)
		{
			return GetUniversalEdges(edges, sharedIndices.ToDictionary());
		}

		public static pb_Edge GetLocalEdgeFast(pb_Edge edge, pb_IntArray[] sharedIndices)
		{
			return new pb_Edge(sharedIndices[edge.x][0], sharedIndices[edge.y][0]);
		}

		public static bool ValidateEdge(pb_Object pb, pb_Edge edge, out pb_Tuple<pb_Face, pb_Edge> validEdge)
		{
			pb_Face[] faces = pb.faces;
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			pb_Edge pb_Edge2 = new pb_Edge(sharedIndices.IndexOf(edge.x), sharedIndices.IndexOf(edge.y));
			int index_a = -1;
			int index_a2 = -1;
			int index_b = -1;
			int index_b2 = -1;
			for (int i = 0; i < faces.Length; i++)
			{
				if (faces[i].distinctIndices.ContainsMatch(sharedIndices[pb_Edge2.x].array, out index_a, out index_b) && faces[i].distinctIndices.ContainsMatch(sharedIndices[pb_Edge2.y].array, out index_a2, out index_b2))
				{
					int num = faces[i].distinctIndices[index_a];
					int num2 = faces[i].distinctIndices[index_a2];
					validEdge = new pb_Tuple<pb_Face, pb_Edge>(faces[i], new pb_Edge(num, num2));
					return true;
				}
			}
			validEdge = null;
			return false;
		}

		public static List<pb_Edge> ValidateEdges(pb_Object pb, pb_Edge[] edges)
		{
			pb_Face[] faces = pb.faces;
			Dictionary<int, int> dictionary = pb.sharedIndices.ToDictionary();
			HashSet<pb_EdgeLookup> hashSet = new HashSet<pb_EdgeLookup>(pb_EdgeLookup.GetEdgeLookup(edges, dictionary));
			List<pb_Edge> list = new List<pb_Edge>();
			bool flag = false;
			for (int i = 0; i < faces.Length; i++)
			{
				if (flag)
				{
					break;
				}
				pb_Edge[] edges2 = faces[i].edges;
				for (int j = 0; j < edges2.Length; j++)
				{
					if (flag)
					{
						break;
					}
					pb_EdgeLookup pb_EdgeLookup2 = new pb_EdgeLookup(dictionary[edges2[j].x], dictionary[edges2[j].y], edges2[j].x, edges2[j].y);
					if (hashSet.Contains(pb_EdgeLookup2))
					{
						hashSet.Remove(pb_EdgeLookup2);
						flag = hashSet.Count < 1;
						list.Add(pb_EdgeLookup2.local);
					}
				}
			}
			return list;
		}

		public static pb_Edge[] GetLocalEdges_Fast(pb_Edge[] edges, pb_IntArray[] sharedIndices)
		{
			pb_Edge[] array = new pb_Edge[edges.Length];
			for (int i = 0; i < array.Length; i++)
			{
				ref pb_Edge reference = ref array[i];
				reference = new pb_Edge(sharedIndices[edges[i].x][0], sharedIndices[edges[i].y][0]);
			}
			return array;
		}

		public static pb_Edge[] AllEdges(pb_Face[] faces)
		{
			List<pb_Edge> list = new List<pb_Edge>();
			foreach (pb_Face pb_Face2 in faces)
			{
				list.AddRange(pb_Face2.edges);
			}
			return list.ToArray();
		}

		public static bool ContainsDuplicateFast(pb_Edge[] edges, pb_Edge edge)
		{
			int num = 0;
			for (int i = 0; i < edges.Length; i++)
			{
				if (edges[i].Equals(edge))
				{
					num++;
				}
			}
			return num > 1;
		}

		public static Vector3[] VerticesWithEdges(pb_Edge[] edges, Vector3[] vertices)
		{
			Vector3[] array = new Vector3[edges.Length * 2];
			int num = 0;
			for (int i = 0; i < edges.Length; i++)
			{
				ref Vector3 reference = ref array[num++];
				reference = vertices[edges[i].x];
				ref Vector3 reference2 = ref array[num++];
				reference2 = vertices[edges[i].y];
			}
			return array;
		}

		public static pb_Edge[] GetPerimeterEdges(pb_Edge[] edges)
		{
			_003CGetPerimeterEdges_003Ec__AnonStorey0 _003CGetPerimeterEdges_003Ec__AnonStorey = new _003CGetPerimeterEdges_003Ec__AnonStorey0();
			_003CGetPerimeterEdges_003Ec__AnonStorey.count = pbUtil.FilledArray(0, edges.Length);
			for (int i = 0; i < edges.Length - 1; i++)
			{
				for (int j = i + 1; j < edges.Length; j++)
				{
					if (edges[i].Equals(edges[j]))
					{
						_003CGetPerimeterEdges_003Ec__AnonStorey.count[i]++;
						_003CGetPerimeterEdges_003Ec__AnonStorey.count[j]++;
					}
				}
			}
			return edges.Where(_003CGetPerimeterEdges_003Ec__AnonStorey._003C_003Em__0).ToArray();
		}
	}
}
