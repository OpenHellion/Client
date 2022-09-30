using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ProBuilder2.Common
{
	public static class pb_Smoothing
	{
		public const int SMOOTHING_GROUP_NONE = 0;

		public const int SMOOTH_RANGE_MIN = 1;

		public const int SMOOTH_RANGE_MAX = 24;

		public const int HARD_RANGE_MIN = 25;

		public const int HARD_RANGE_MAX = 42;

		[CompilerGenerated]
		private static Func<pb_Face, int> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<pb_Face, int> _003C_003Ef__am_0024cache1;

		public static int GetUnusedSmoothingGroup(pb_Object pb)
		{
			pb_Face[] faces = pb.faces;
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CGetUnusedSmoothingGroup_003Em__0;
			}
			return GetNextUnusedSmoothingGroup(1, new HashSet<int>(faces.Select(_003C_003Ef__am_0024cache0)));
		}

		private static int GetNextUnusedSmoothingGroup(int start, HashSet<int> used)
		{
			while (used.Contains(start) && start < 2147483646)
			{
				start++;
				if (start > 24 && start < 42)
				{
					start = 43;
				}
			}
			return start;
		}

		public static bool IsSmooth(int index)
		{
			return index > 0 && (index < 25 || index > 42);
		}

		public static void ApplySmoothingGroups(pb_Object pb, IEnumerable<pb_Face> faces, float angleThreshold, Vector3[] normals = null)
		{
			bool flag = false;
			foreach (pb_Face face in faces)
			{
				if (face.smoothingGroup != 0)
				{
					flag = true;
				}
				face.smoothingGroup = 0;
			}
			if (normals == null)
			{
				if (flag)
				{
					pb.msh.normals = null;
				}
				normals = pb.GetNormals();
			}
			float angleThreshold2 = Mathf.Abs(Mathf.Cos(Mathf.Clamp(angleThreshold, 0f, 89.999f) * ((float)Math.PI / 180f)));
			pb_Face[] faces2 = pb.faces;
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CApplySmoothingGroups_003Em__1;
			}
			HashSet<int> hashSet = new HashSet<int>(faces2.Select(_003C_003Ef__am_0024cache1));
			int nextUnusedSmoothingGroup = GetNextUnusedSmoothingGroup(1, hashSet);
			HashSet<pb_Face> hashSet2 = new HashSet<pb_Face>();
			List<pb_WingedEdge> wingedEdges = pb_WingedEdge.GetWingedEdges(pb, faces, oneWingPerFace: true);
			foreach (pb_WingedEdge item in wingedEdges)
			{
				if (hashSet2.Add(item.face))
				{
					item.face.smoothingGroup = nextUnusedSmoothingGroup;
					if (FindSoftEdgesRecursive(normals, item, angleThreshold2, hashSet2))
					{
						hashSet.Add(nextUnusedSmoothingGroup);
						nextUnusedSmoothingGroup = GetNextUnusedSmoothingGroup(nextUnusedSmoothingGroup, hashSet);
					}
					else
					{
						item.face.smoothingGroup = 0;
					}
				}
			}
		}

		private static bool FindSoftEdgesRecursive(Vector3[] normals, pb_WingedEdge wing, float angleThreshold, HashSet<pb_Face> processed)
		{
			bool result = false;
			foreach (pb_WingedEdge item in wing)
			{
				if (item.opposite != null && item.opposite.face.smoothingGroup == 0 && IsSoftEdge(normals, item.edge, item.opposite.edge, angleThreshold) && processed.Add(item.opposite.face))
				{
					result = true;
					item.opposite.face.smoothingGroup = wing.face.smoothingGroup;
					FindSoftEdgesRecursive(normals, item.opposite, angleThreshold, processed);
				}
			}
			return result;
		}

		private static bool IsSoftEdge(Vector3[] normals, pb_EdgeLookup left, pb_EdgeLookup right, float threshold)
		{
			Vector3 lhs = normals[left.local.x];
			Vector3 lhs2 = normals[left.local.y];
			Vector3 rhs = normals[(right.common.x != left.common.x) ? right.local.y : right.local.x];
			Vector3 rhs2 = normals[(right.common.y != left.common.y) ? right.local.x : right.local.y];
			lhs.Normalize();
			lhs2.Normalize();
			rhs.Normalize();
			rhs2.Normalize();
			return Mathf.Abs(Vector3.Dot(lhs, rhs)) > threshold && Mathf.Abs(Vector3.Dot(lhs2, rhs2)) > threshold;
		}

		[CompilerGenerated]
		private static int _003CGetUnusedSmoothingGroup_003Em__0(pb_Face x)
		{
			return x.smoothingGroup;
		}

		[CompilerGenerated]
		private static int _003CApplySmoothingGroups_003Em__1(pb_Face x)
		{
			return x.smoothingGroup;
		}
	}
}
