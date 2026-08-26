using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ProBuilder2.Common
{
	public static class pb_Projection
	{
		private static Vector3 t_uaxis = Vector3.zero;

		private static Vector3 t_vaxis = Vector3.zero;

		[CompilerGenerated]
		private static Comparison<pb_Tuple<float, Vector2>> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<pb_Tuple<float, Vector2>, Vector2> _003C_003Ef__am_0024cache1;

		public static Vector2[] PlanarProject(IList<Vector3> verts, Vector3 planeNormal)
		{
			return PlanarProject(verts.ToArray(), planeNormal, VectorToProjectionAxis(planeNormal));
		}

		public static Vector2[] PlanarProject(pb_Object pb, pb_Face face)
		{
			Vector3 vector = pb_Math.Normal(pb, face);
			return PlanarProject(pb.vertices, vector, VectorToProjectionAxis(vector), face.indices);
		}

		public static Vector2[] PlanarProject(IList<pb_Vertex> vertices, IList<int> indices)
		{
			int count = indices.Count;
			Vector3[] array = new Vector3[count];
			for (int i = 0; i < count; i++)
			{
				ref Vector3 reference = ref array[i];
				reference = vertices[indices[i]].position;
			}
			Vector3 vector = pb_Math.Normal(vertices, indices);
			ProjectionAxis projectionAxis = VectorToProjectionAxis(vector);
			return PlanarProject(array, vector, projectionAxis);
		}

		public static Vector2[] PlanarProject(Vector3[] verts, Vector3 planeNormal, ProjectionAxis projectionAxis, int[] indices = null)
		{
			int num = ((indices != null && indices.Length >= 1) ? indices.Length : verts.Length);
			Vector2[] array = new Vector2[num];
			Vector3 rhs = Vector3.zero;
			switch (projectionAxis)
			{
			case ProjectionAxis.X:
			case ProjectionAxis.X_Negative:
				rhs = Vector3.up;
				break;
			case ProjectionAxis.Y:
			case ProjectionAxis.Y_Negative:
				rhs = Vector3.forward;
				break;
			case ProjectionAxis.Z:
			case ProjectionAxis.Z_Negative:
				rhs = Vector3.up;
				break;
			}
			Vector3 lhs = Vector3.Cross(planeNormal, rhs);
			lhs.Normalize();
			Vector3 lhs2 = Vector3.Cross(lhs, planeNormal);
			lhs2.Normalize();
			for (int i = 0; i < num; i++)
			{
				int num2 = ((indices == null) ? i : indices[i]);
				float x = Vector3.Dot(lhs, verts[num2]);
				float y = Vector3.Dot(lhs2, verts[num2]);
				ref Vector2 reference = ref array[i];
				reference = new Vector2(x, y);
			}
			return array;
		}

		public static void PlanarProject(Vector3[] verts, Vector2[] uvs, int[] indices, Vector3 planeNormal, ProjectionAxis projectionAxis)
		{
			Vector3 b;
			switch (projectionAxis)
			{
			case ProjectionAxis.X:
			case ProjectionAxis.X_Negative:
				b = Vector3.up;
				break;
			case ProjectionAxis.Y:
			case ProjectionAxis.Y_Negative:
				b = Vector3.forward;
				break;
			case ProjectionAxis.Z:
			case ProjectionAxis.Z_Negative:
				b = Vector3.up;
				break;
			default:
				b = Vector3.up;
				break;
			}
			pb_Math.Cross(planeNormal, b, ref t_uaxis.x, ref t_uaxis.y, ref t_uaxis.z);
			t_uaxis.Normalize();
			pb_Math.Cross(t_uaxis, planeNormal, ref t_vaxis.x, ref t_vaxis.y, ref t_vaxis.z);
			t_vaxis.Normalize();
			int num = indices.Length;
			for (int i = 0; i < num; i++)
			{
				int num2 = indices[i];
				uvs[num2].x = Vector3.Dot(t_uaxis, verts[num2]);
				uvs[num2].y = Vector3.Dot(t_vaxis, verts[num2]);
			}
		}

		public static Vector2[] SphericalProject(IList<Vector3> vertices, IList<int> indices = null)
		{
			int num = indices?.Count ?? vertices.Count;
			Vector2[] array = new Vector2[num];
			Vector3 vector = pb_Math.Average(vertices, indices);
			for (int i = 0; i < num; i++)
			{
				int index = indices?[i] ?? i;
				Vector3 vector2 = vertices[index] - vector;
				vector2.Normalize();
				array[i].x = 0.5f + Mathf.Atan2(vector2.z, vector2.x) / ((float)Math.PI * 2f);
				array[i].y = 0.5f - Mathf.Asin(vector2.y) / (float)Math.PI;
			}
			return array;
		}

		public static IList<Vector2> Sort(IList<Vector2> verts, SortMethod method = SortMethod.CounterClockwise)
		{
			Vector2 vector = pb_Math.Average(verts);
			Vector2 up = Vector2.up;
			int count = verts.Count;
			List<pb_Tuple<float, Vector2>> list = new List<pb_Tuple<float, Vector2>>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(new pb_Tuple<float, Vector2>(pb_Math.SignedAngle(up, verts[i] - vector), verts[i]));
			}
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CSort_003Em__0;
			}
			list.Sort(_003C_003Ef__am_0024cache0);
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CSort_003Em__1;
			}
			IList<Vector2> list2 = list.Select(_003C_003Ef__am_0024cache1).ToList();
			if (method == SortMethod.Clockwise)
			{
				list2.Reverse();
			}
			return list2;
		}

		public static Vector3 ProjectionAxisToVector(ProjectionAxis axis)
		{
			return axis switch
			{
				ProjectionAxis.X => Vector3.right, 
				ProjectionAxis.Y => Vector3.up, 
				ProjectionAxis.Z => Vector3.forward, 
				ProjectionAxis.X_Negative => -Vector3.right, 
				ProjectionAxis.Y_Negative => -Vector3.up, 
				ProjectionAxis.Z_Negative => -Vector3.forward, 
				_ => Vector3.zero, 
			};
		}

		public static ProjectionAxis VectorToProjectionAxis(Vector3 plane)
		{
			if (Mathf.Abs(plane.x) > Mathf.Abs(plane.y) && Mathf.Abs(plane.x) > Mathf.Abs(plane.z))
			{
				return (!(plane.x > 0f)) ? ProjectionAxis.X_Negative : ProjectionAxis.X;
			}
			if (Mathf.Abs(plane.y) > Mathf.Abs(plane.z))
			{
				return (plane.y > 0f) ? ProjectionAxis.Y : ProjectionAxis.Y_Negative;
			}
			return (!(plane.z > 0f)) ? ProjectionAxis.Z_Negative : ProjectionAxis.Z;
		}

		public static Plane FindBestPlane<T>(IList<T> points, Func<T, Vector3> selector, IList<int> indices = null)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			float num6 = 0f;
			bool flag = indices != null && indices.Count > 0;
			int num7 = ((!flag) ? points.Count : indices.Count);
			Vector3 vector = points.Average(selector, indices);
			for (int i = 0; i < num7; i++)
			{
				Vector3 vector2 = selector(points[(!flag) ? i : indices[i]]) - vector;
				num += vector2.x * vector2.x;
				num2 += vector2.x * vector2.y;
				num3 += vector2.x * vector2.z;
				num4 += vector2.y * vector2.y;
				num5 += vector2.y * vector2.z;
				num6 += vector2.z * vector2.z;
			}
			float num8 = num4 * num6 - num5 * num5;
			float num9 = num * num6 - num3 * num3;
			float num10 = num * num4 - num2 * num2;
			Vector3 inNormal = ((num8 > num9 && num8 > num10) ? new Vector3(1f, (num3 * num5 - num2 * num6) / num8, (num2 * num5 - num3 * num4) / num8) : ((!(num9 > num10)) ? new Vector3((num5 * num2 - num3 * num4) / num10, (num3 * num2 - num5 * num) / num10, 1f) : new Vector3((num5 * num3 - num2 * num6) / num9, 1f, (num2 * num3 - num5 * num) / num9)));
			inNormal.Normalize();
			return new Plane(inNormal, vector);
		}

		public static Plane FindBestPlane(Vector3[] points, int[] indices = null)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			float num6 = 0f;
			bool flag = indices != null && indices.Length > 0;
			int num7 = ((!flag) ? points.Length : indices.Length);
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			for (int i = 0; i < num7; i++)
			{
				zero.x += points[(!flag) ? i : indices[i]].x;
				zero.y += points[(!flag) ? i : indices[i]].y;
				zero.z += points[(!flag) ? i : indices[i]].z;
			}
			zero.x /= num7;
			zero.y /= num7;
			zero.z /= num7;
			for (int j = 0; j < num7; j++)
			{
				Vector3 vector = points[(!flag) ? j : indices[j]] - zero;
				num += vector.x * vector.x;
				num2 += vector.x * vector.y;
				num3 += vector.x * vector.z;
				num4 += vector.y * vector.y;
				num5 += vector.y * vector.z;
				num6 += vector.z * vector.z;
			}
			float num8 = num4 * num6 - num5 * num5;
			float num9 = num * num6 - num3 * num3;
			float num10 = num * num4 - num2 * num2;
			if (num8 > num9 && num8 > num10)
			{
				zero2.x = 1f;
				zero2.y = (num3 * num5 - num2 * num6) / num8;
				zero2.z = (num2 * num5 - num3 * num4) / num8;
			}
			else if (num9 > num10)
			{
				zero2.x = (num5 * num3 - num2 * num6) / num9;
				zero2.y = 1f;
				zero2.z = (num2 * num3 - num5 * num) / num9;
			}
			else
			{
				zero2.x = (num5 * num2 - num3 * num4) / num10;
				zero2.y = (num3 * num2 - num5 * num) / num10;
				zero2.z = 1f;
			}
			zero2.Normalize();
			return new Plane(zero2, zero);
		}

		[CompilerGenerated]
		private static int _003CSort_003Em__0(pb_Tuple<float, Vector2> a, pb_Tuple<float, Vector2> b)
		{
			return (!(a.Item1 < b.Item1)) ? 1 : (-1);
		}

		[CompilerGenerated]
		private static Vector2 _003CSort_003Em__1(pb_Tuple<float, Vector2> x)
		{
			return x.Item2;
		}
	}
}
