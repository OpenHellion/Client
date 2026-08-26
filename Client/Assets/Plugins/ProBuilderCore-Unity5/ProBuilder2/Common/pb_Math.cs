using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder2.Common
{
	public static class pb_Math
	{
		public const float PHI = 1.618034f;

		public const float FLT_EPSILON = float.Epsilon;

		public const float FLT_COMPARE_EPSILON = 0.0001f;

		public const float HANDLE_EPSILON = 0.0001f;

		private static Vector3 tv1;

		private static Vector3 tv2;

		private static Vector3 tv3;

		private static Vector3 tv4;

		public static Vector2 PointInCircumference(float radius, float angleInDegrees, Vector2 origin)
		{
			float x = radius * Mathf.Cos((float)Math.PI / 180f * angleInDegrees) + origin.x;
			float y = radius * Mathf.Sin((float)Math.PI / 180f * angleInDegrees) + origin.y;
			return new Vector2(x, y);
		}

		public static Vector3 PointInSphere(float radius, float latitudeAngle, float longitudeAngle)
		{
			float x = radius * Mathf.Cos((float)Math.PI / 180f * latitudeAngle) * Mathf.Sin((float)Math.PI / 180f * longitudeAngle);
			float y = radius * Mathf.Sin((float)Math.PI / 180f * latitudeAngle) * Mathf.Sin((float)Math.PI / 180f * longitudeAngle);
			float z = radius * Mathf.Cos((float)Math.PI / 180f * longitudeAngle);
			return new Vector3(x, y, z);
		}

		public static float SignedAngle(Vector2 a, Vector2 b)
		{
			float num = Vector2.Angle(a, b);
			if (b.x - a.x < 0f)
			{
				num = 360f - num;
			}
			return num;
		}

		public static float SqrDistance(Vector3 a, Vector3 b)
		{
			float num = b.x - a.x;
			float num2 = b.y - a.y;
			float num3 = b.z - a.z;
			return num * num + num2 * num2 + num3 * num3;
		}

		public static float TriangleArea(Vector3 x, Vector3 y, Vector3 z)
		{
			float num = SqrDistance(x, y);
			float num2 = SqrDistance(y, z);
			float num3 = SqrDistance(z, x);
			return Mathf.Sqrt((2f * num * num2 + 2f * num2 * num3 + 2f * num3 * num - num * num - num2 * num2 - num3 * num3) / 16f);
		}

		public static float PolygonArea(Vector3[] vertices, int[] indices)
		{
			float num = 0f;
			for (int i = 0; i < indices.Length; i += 3)
			{
				num += TriangleArea(vertices[indices[i]], vertices[indices[i + 1]], vertices[indices[i + 2]]);
			}
			return num;
		}

		public static Vector2 RotateAroundPoint(this Vector2 v, Vector2 origin, float theta)
		{
			float x = origin.x;
			float y = origin.y;
			float x2 = v.x;
			float y2 = v.y;
			float num = Mathf.Sin(theta * ((float)Math.PI / 180f));
			float num2 = Mathf.Cos(theta * ((float)Math.PI / 180f));
			x2 -= x;
			y2 -= y;
			float num3 = x2 * num2 + y2 * num;
			float num4 = (0f - x2) * num + y2 * num2;
			x2 = num3 + x;
			y2 = num4 + y;
			return new Vector2(x2, y2);
		}

		public static Vector2 ScaleAroundPoint(this Vector2 v, Vector2 origin, Vector2 scale)
		{
			Vector2 a = v - origin;
			a = Vector2.Scale(a, scale);
			return a + origin;
		}

		public static Vector2 Perpendicular(Vector2 a, Vector2 b)
		{
			float x = a.x;
			float y = a.y;
			float x2 = b.x;
			float y2 = b.y;
			return new Vector2(0f - (y2 - y), x2 - x).normalized;
		}

		public static Vector2 Perpendicular(Vector2 a)
		{
			return new Vector2(0f - a.y, a.x).normalized;
		}

		public static Vector2 ReflectPoint(Vector2 point, Vector2 a, Vector2 b)
		{
			Vector2 from = b - a;
			Vector2 vector = new Vector2(0f - from.y, from.x);
			float num = Mathf.Sin(Vector2.Angle(from, point - a) * ((float)Math.PI / 180f)) * Vector2.Distance(point, a);
			return point + vector * (num * 2f) * ((!(Vector2.Dot(point - a, vector) > 0f)) ? 1f : (-1f));
		}

		public static float DistancePointLineSegment(Vector2 p, Vector2 v, Vector2 w)
		{
			float num = (v.x - w.x) * (v.x - w.x) + (v.y - w.y) * (v.y - w.y);
			if (num == 0f)
			{
				return Vector2.Distance(p, v);
			}
			float num2 = Vector2.Dot(p - v, w - v) / num;
			if ((double)num2 < 0.0)
			{
				return Vector2.Distance(p, v);
			}
			if ((double)num2 > 1.0)
			{
				return Vector2.Distance(p, w);
			}
			Vector2 b = v + num2 * (w - v);
			return Vector2.Distance(p, b);
		}

		public static float DistancePointLineSegment(Vector3 p, Vector3 v, Vector3 w)
		{
			float num = (v.x - w.x) * (v.x - w.x) + (v.y - w.y) * (v.y - w.y) + (v.z - w.z) * (v.z - w.z);
			if (num == 0f)
			{
				return Vector3.Distance(p, v);
			}
			float num2 = Vector3.Dot(p - v, w - v) / num;
			if ((double)num2 < 0.0)
			{
				return Vector3.Distance(p, v);
			}
			if ((double)num2 > 1.0)
			{
				return Vector3.Distance(p, w);
			}
			Vector3 b = v + num2 * (w - v);
			return Vector3.Distance(p, b);
		}

		public static Vector3 GetNearestPointRayRay(Vector3 ao, Vector3 ad, Vector3 bo, Vector3 bd)
		{
			if (ad == bd)
			{
				return ao;
			}
			Vector3 rhs = bo - ao;
			float num = (0f - Vector3.Dot(ad, bd)) * Vector3.Dot(bd, rhs) + Vector3.Dot(ad, rhs) * Vector3.Dot(bd, bd);
			float num2 = Vector3.Dot(ad, ad) * Vector3.Dot(bd, bd) - Vector3.Dot(ad, bd) * Vector3.Dot(ad, bd);
			return ao + ad * (num / num2);
		}

		public static bool GetLineSegmentIntersect(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, ref Vector2 intersect)
		{
			intersect = Vector2.zero;
			Vector2 vector = default(Vector2);
			vector.x = p1.x - p0.x;
			vector.y = p1.y - p0.y;
			Vector2 vector2 = default(Vector2);
			vector2.x = p3.x - p2.x;
			vector2.y = p3.y - p2.y;
			float num = ((0f - vector.y) * (p0.x - p2.x) + vector.x * (p0.y - p2.y)) / ((0f - vector2.x) * vector.y + vector.x * vector2.y);
			float num2 = (vector2.x * (p0.y - p2.y) - vector2.y * (p0.x - p2.x)) / ((0f - vector2.x) * vector.y + vector.x * vector2.y);
			if (num >= 0f && num <= 1f && num2 >= 0f && num2 <= 1f)
			{
				intersect.x = p0.x + num2 * vector.x;
				intersect.y = p0.y + num2 * vector.y;
				return true;
			}
			return false;
		}

		public static bool GetLineSegmentIntersect(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
		{
			Vector2 vector = default(Vector2);
			vector.x = p1.x - p0.x;
			vector.y = p1.y - p0.y;
			Vector2 vector2 = default(Vector2);
			vector2.x = p3.x - p2.x;
			vector2.y = p3.y - p2.y;
			float num = ((0f - vector.y) * (p0.x - p2.x) + vector.x * (p0.y - p2.y)) / ((0f - vector2.x) * vector.y + vector.x * vector2.y);
			float num2 = (vector2.x * (p0.y - p2.y) - vector2.y * (p0.x - p2.x)) / ((0f - vector2.x) * vector.y + vector.x * vector2.y);
			return num >= 0f && num <= 1f && num2 >= 0f && num2 <= 1f;
		}

		public static bool PointInPolygon(Vector2[] polygon, Vector2 point, int[] indices = null)
		{
			int num = ((indices == null) ? polygon.Length : indices.Length);
			if (num % 2 != 0)
			{
				Debug.LogError("PointInPolygon requires polygon indices be divisible by 2!");
				return false;
			}
			pb_Bounds2D pb_Bounds2D2 = new pb_Bounds2D(polygon, indices);
			if (pb_Bounds2D2.ContainsPoint(point))
			{
				Vector2 p = pb_Bounds2D2.center + Vector2.up * (pb_Bounds2D2.size.y + 2f);
				int num2 = 0;
				for (int i = 0; i < num; i += 2)
				{
					int num3 = ((indices == null) ? i : indices[i]);
					int num4 = ((indices == null) ? (i + 1) : indices[i + 1]);
					if (GetLineSegmentIntersect(p, point, polygon[num3], polygon[num4]))
					{
						num2++;
					}
				}
				return num2 % 2 != 0;
			}
			return false;
		}

		public static bool PointInPolygon(Vector2[] polygon, pb_Bounds2D polyBounds, pb_Edge[] edges, Vector2 point)
		{
			int num = edges.Length * 2;
			Vector2 p = polyBounds.center + Vector2.up * (polyBounds.size.y + 2f);
			int num2 = 0;
			for (int i = 0; i < num; i += 2)
			{
				if (GetLineSegmentIntersect(p, point, polygon[i], polygon[i + 1]))
				{
					num2++;
				}
			}
			return num2 % 2 != 0;
		}

		public static bool RectIntersectsLineSegment(Rect rect, Vector2 a, Vector2 b)
		{
			Vector2 vector = new Vector2(rect.xMin, rect.yMax);
			Vector2 p = new Vector2(rect.xMax, rect.yMax);
			Vector2 vector2 = new Vector2(rect.xMin, rect.yMin);
			Vector2 vector3 = new Vector2(rect.xMax, rect.yMin);
			return GetLineSegmentIntersect(p, vector, a, b) || GetLineSegmentIntersect(vector, vector2, a, b) || GetLineSegmentIntersect(vector2, vector3, a, b) || GetLineSegmentIntersect(vector3, vector, a, b);
		}

		public static bool RayIntersectsTriangle(Ray InRay, Vector3 InTriangleA, Vector3 InTriangleB, Vector3 InTriangleC, out float OutDistance, out Vector3 OutPoint)
		{
			OutDistance = 0f;
			OutPoint = Vector3.zero;
			Vector3 vector = InTriangleB - InTriangleA;
			Vector3 vector2 = InTriangleC - InTriangleA;
			Vector3 rhs = Vector3.Cross(InRay.direction, vector2);
			float num = Vector3.Dot(vector, rhs);
			if (num > 0f - Mathf.Epsilon && num < Mathf.Epsilon)
			{
				return false;
			}
			float num2 = 1f / num;
			Vector3 lhs = InRay.origin - InTriangleA;
			float num3 = Vector3.Dot(lhs, rhs) * num2;
			if (num3 < 0f || num3 > 1f)
			{
				return false;
			}
			Vector3 rhs2 = Vector3.Cross(lhs, vector);
			float num4 = Vector3.Dot(InRay.direction, rhs2) * num2;
			if (num4 < 0f || num3 + num4 > 1f)
			{
				return false;
			}
			float num5 = Vector3.Dot(vector2, rhs2) * num2;
			if (num5 > Mathf.Epsilon)
			{
				OutDistance = num5;
				OutPoint.x = num3 * InTriangleB.x + num4 * InTriangleC.x + (1f - (num3 + num4)) * InTriangleA.x;
				OutPoint.y = num3 * InTriangleB.y + num4 * InTriangleC.y + (1f - (num3 + num4)) * InTriangleA.y;
				OutPoint.z = num3 * InTriangleB.z + num4 * InTriangleC.z + (1f - (num3 + num4)) * InTriangleA.z;
				return true;
			}
			return false;
		}

		public static bool RayIntersectsTriangle2(Vector3 origin, Vector3 dir, Vector3 vert0, Vector3 vert1, Vector3 vert2, ref float distance, ref Vector3 normal)
		{
			Subtract(vert0, vert1, ref tv1);
			Subtract(vert0, vert2, ref tv2);
			Cross(dir, tv2, ref tv4);
			float num = Vector3.Dot(tv1, tv4);
			if (num < Mathf.Epsilon)
			{
				return false;
			}
			Subtract(vert0, origin, ref tv3);
			float num2 = Vector3.Dot(tv3, tv4);
			if (num2 < 0f || num2 > num)
			{
				return false;
			}
			Cross(tv3, tv1, ref tv4);
			float num3 = Vector3.Dot(dir, tv4);
			if (num3 < 0f || num2 + num3 > num)
			{
				return false;
			}
			distance = Vector3.Dot(tv2, tv4) * (1f / num);
			Cross(tv1, tv2, ref normal);
			return true;
		}

		public static float Secant(float x)
		{
			return 1f / Mathf.Cos(x);
		}

		public static Vector3 Normal(Vector3 p0, Vector3 p1, Vector3 p2)
		{
			float ax = p1.x - p0.x;
			float ay = p1.y - p0.y;
			float az = p1.z - p0.z;
			float bx = p2.x - p0.x;
			float by = p2.y - p0.y;
			float bz = p2.z - p0.z;
			Vector3 zero = Vector3.zero;
			Cross(ax, ay, az, bx, by, bz, ref zero.x, ref zero.y, ref zero.z);
			if (zero.magnitude < Mathf.Epsilon)
			{
				return new Vector3(0f, 0f, 0f);
			}
			zero.Normalize();
			return zero;
		}

		public static Vector3 Normal(IList<pb_Vertex> vertices, IList<int> indices = null)
		{
			if (indices == null || indices.Count % 3 != 0)
			{
				Vector3 result = Vector3.Cross(vertices[1].position - vertices[0].position, vertices[2].position - vertices[0].position);
				result.Normalize();
				return result;
			}
			int count = indices.Count;
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < count; i += 3)
			{
				zero += Normal(vertices[indices[i]].position, vertices[indices[i + 1]].position, vertices[indices[i + 2]].position);
			}
			zero /= (float)count / 3f;
			zero.Normalize();
			return zero;
		}

		public static Vector3 Normal(pb_Object pb, pb_Face face)
		{
			Vector3[] vertices = pb.vertices;
			Vector3 vector = Normal(vertices[face.indices[0]], vertices[face.indices[1]], vertices[face.indices[2]]);
			if (face.indices.Length > 7)
			{
				Vector3 normal = pb_Projection.FindBestPlane(vertices, face.distinctIndices).normal;
				if (Vector3.Dot(vector, normal) < 0f)
				{
					vector.x = 0f - normal.x;
					vector.y = 0f - normal.y;
					vector.z = 0f - normal.z;
				}
				else
				{
					vector.x = normal.x;
					vector.y = normal.y;
					vector.z = normal.z;
				}
			}
			return vector;
		}

		public static Vector3 Normal(IList<Vector3> p)
		{
			if (p == null || p.Count < 3)
			{
				return Vector3.zero;
			}
			int count = p.Count;
			if (count % 3 == 0)
			{
				Vector3 zero = Vector3.zero;
				for (int i = 0; i < count; i += 3)
				{
					zero += Normal(p[i], p[i + 1], p[i + 2]);
				}
				zero /= (float)count / 3f;
				zero.Normalize();
				return zero;
			}
			Vector3 vector = Vector3.Cross(p[1] - p[0], p[2] - p[0]);
			if (vector.magnitude < Mathf.Epsilon)
			{
				return new Vector3(0f, 0f, 0f);
			}
			return vector.normalized;
		}

		public static void NormalTangentBitangent(pb_Object pb, pb_Face face, out Vector3 normal, out Vector3 tangent, out Vector3 bitangent)
		{
			if (face.indices.Length < 3)
			{
				Debug.LogWarning("Cannot find normal / tangent / bitangent for face with < 3 indices.");
				normal = Vector3.zero;
				tangent = Vector3.zero;
				bitangent = Vector3.zero;
				return;
			}
			normal = Normal(pb, face);
			Vector3 tangent2 = Vector3.zero;
			Vector3 zero = Vector3.zero;
			Vector4 vector = new Vector4(0f, 0f, 0f, 1f);
			long num = face.indices[0];
			long num2 = face.indices[1];
			long num3 = face.indices[2];
			Vector3 vector2 = pb.vertices[num];
			Vector3 vector3 = pb.vertices[num2];
			Vector3 vector4 = pb.vertices[num3];
			Vector2 vector5 = pb.uv[num];
			Vector2 vector6 = pb.uv[num2];
			Vector2 vector7 = pb.uv[num3];
			float num4 = vector3.x - vector2.x;
			float num5 = vector4.x - vector2.x;
			float num6 = vector3.y - vector2.y;
			float num7 = vector4.y - vector2.y;
			float num8 = vector3.z - vector2.z;
			float num9 = vector4.z - vector2.z;
			float num10 = vector6.x - vector5.x;
			float num11 = vector7.x - vector5.x;
			float num12 = vector6.y - vector5.y;
			float num13 = vector7.y - vector5.y;
			float num14 = 1f / (num10 * num13 - num11 * num12);
			Vector3 vector8 = new Vector3((num13 * num4 - num12 * num5) * num14, (num13 * num6 - num12 * num7) * num14, (num13 * num8 - num12 * num9) * num14);
			Vector3 vector9 = new Vector3((num10 * num5 - num11 * num4) * num14, (num10 * num7 - num11 * num6) * num14, (num10 * num9 - num11 * num8) * num14);
			tangent2 += vector8;
			zero += vector9;
			Vector3 normal2 = normal;
			Vector3.OrthoNormalize(ref normal2, ref tangent2);
			vector.x = tangent2.x;
			vector.y = tangent2.y;
			vector.z = tangent2.z;
			vector.w = ((!(Vector3.Dot(Vector3.Cross(normal2, tangent2), zero) < 0f)) ? 1f : (-1f));
			tangent = (Vector3)vector * vector.w;
			bitangent = Vector3.Cross(normal, tangent);
		}

		public static bool IsCardinalAxis(Vector3 v, float epsilon = 1E-05f)
		{
			v.Normalize();
			return 1f - Mathf.Abs(Vector3.Dot(Vector3.up, v)) < epsilon || 1f - Mathf.Abs(Vector3.Dot(Vector3.forward, v)) < epsilon || 1f - Mathf.Abs(Vector3.Dot(Vector3.right, v)) < epsilon;
		}

		public static T Max<T>(T[] array) where T : IComparable<T>
		{
			if (array == null || array.Length < 1)
			{
				return default(T);
			}
			T val = array[0];
			for (int i = 1; i < array.Length; i++)
			{
				if (array[i].CompareTo(val) >= 0)
				{
					val = array[i];
				}
			}
			return val;
		}

		public static T Min<T>(T[] array) where T : IComparable<T>
		{
			if (array == null || array.Length < 1)
			{
				return default(T);
			}
			T val = array[0];
			for (int i = 1; i < array.Length; i++)
			{
				if (array[i].CompareTo(val) < 0)
				{
					val = array[i];
				}
			}
			return val;
		}

		public static float LargestValue(Vector3 v)
		{
			if (v.x > v.y && v.x > v.z)
			{
				return v.x;
			}
			if (v.y > v.x && v.y > v.z)
			{
				return v.y;
			}
			return v.z;
		}

		public static float LargestValue(Vector2 v)
		{
			return (!(v.x > v.y)) ? v.y : v.x;
		}

		public static Vector2 SmallestVector2(Vector2[] v)
		{
			int num = v.Length;
			Vector2 result = v[0];
			for (int i = 0; i < num; i++)
			{
				if (v[i].x < result.x)
				{
					result.x = v[i].x;
				}
				if (v[i].y < result.y)
				{
					result.y = v[i].y;
				}
			}
			return result;
		}

		public static Vector2 SmallestVector2(Vector2[] v, int[] indices)
		{
			int num = indices.Length;
			Vector2 result = v[indices[0]];
			for (int i = 0; i < num; i++)
			{
				if (v[indices[i]].x < result.x)
				{
					result.x = v[indices[i]].x;
				}
				if (v[indices[i]].y < result.y)
				{
					result.y = v[indices[i]].y;
				}
			}
			return result;
		}

		public static Vector2 LargestVector2(Vector2[] v)
		{
			int num = v.Length;
			Vector2 result = v[0];
			for (int i = 0; i < num; i++)
			{
				if (v[i].x > result.x)
				{
					result.x = v[i].x;
				}
				if (v[i].y > result.y)
				{
					result.y = v[i].y;
				}
			}
			return result;
		}

		public static Vector2 LargestVector2(Vector2[] v, int[] indices)
		{
			int num = indices.Length;
			Vector2 result = v[indices[0]];
			for (int i = 0; i < num; i++)
			{
				if (v[indices[i]].x > result.x)
				{
					result.x = v[indices[i]].x;
				}
				if (v[indices[i]].y > result.y)
				{
					result.y = v[indices[i]].y;
				}
			}
			return result;
		}

		public static Vector3 BoundsCenter(Vector3[] verts)
		{
			if (verts.Length < 1)
			{
				return Vector3.zero;
			}
			Vector3 vector = verts[0];
			Vector3 vector2 = vector;
			for (int i = 1; i < verts.Length; i++)
			{
				vector.x = Mathf.Min(verts[i].x, vector.x);
				vector2.x = Mathf.Max(verts[i].x, vector2.x);
				vector.y = Mathf.Min(verts[i].y, vector.y);
				vector2.y = Mathf.Max(verts[i].y, vector2.y);
				vector.z = Mathf.Min(verts[i].z, vector.z);
				vector2.z = Mathf.Max(verts[i].z, vector2.z);
			}
			return (vector + vector2) * 0.5f;
		}

		public static Vector2 Average(IList<Vector2> v, IList<int> indices = null)
		{
			Vector2 zero = Vector2.zero;
			float num = indices?.Count ?? v.Count;
			if (indices == null)
			{
				for (int i = 0; (float)i < num; i++)
				{
					zero += v[i];
				}
			}
			else
			{
				for (int j = 0; (float)j < num; j++)
				{
					zero += v[indices[j]];
				}
			}
			return zero / num;
		}

		public static Vector3 Average(IList<Vector3> v, IList<int> indices = null)
		{
			Vector3 zero = Vector3.zero;
			float num = indices?.Count ?? v.Count;
			if (indices == null)
			{
				for (int i = 0; (float)i < num; i++)
				{
					zero.x += v[i].x;
					zero.y += v[i].y;
					zero.z += v[i].z;
				}
			}
			else
			{
				for (int j = 0; (float)j < num; j++)
				{
					zero.x += v[indices[j]].x;
					zero.y += v[indices[j]].y;
					zero.z += v[indices[j]].z;
				}
			}
			return zero / num;
		}

		public static Vector3 Average<T>(this IList<T> v, Func<T, Vector3> selector, IList<int> indices = null)
		{
			Vector3 zero = Vector3.zero;
			float num = indices?.Count ?? v.Count;
			if (indices == null)
			{
				for (int i = 0; (float)i < num; i++)
				{
					zero += selector(v[i]);
				}
			}
			else
			{
				for (int j = 0; (float)j < num; j++)
				{
					zero += selector(v[indices[j]]);
				}
			}
			return zero / num;
		}

		public static Vector4 Average(IList<Vector4> v, IList<int> indices = null)
		{
			Vector4 zero = Vector4.zero;
			float num = indices?.Count ?? v.Count;
			if (indices == null)
			{
				for (int i = 0; (float)i < num; i++)
				{
					zero += v[i];
				}
			}
			else
			{
				for (int j = 0; (float)j < num; j++)
				{
					zero += v[indices[j]];
				}
			}
			return zero / num;
		}

		public static Color Average(IList<Color> c, IList<int> indices = null)
		{
			Color color = c[0];
			float num = indices?.Count ?? c.Count;
			if (indices == null)
			{
				for (int i = 1; (float)i < num; i++)
				{
					color += c[i];
				}
			}
			else
			{
				for (int j = 1; (float)j < num; j++)
				{
					color += c[indices[j]];
				}
			}
			return color / num;
		}

		public static bool Approx2(this Vector2 v, Vector2 b, float delta = 0.0001f)
		{
			return Mathf.Abs(v.x - b.x) < delta && Mathf.Abs(v.y - b.y) < delta;
		}

		public static bool Approx3(this Vector3 v, Vector3 b, float delta = 0.0001f)
		{
			return Mathf.Abs(v.x - b.x) < delta && Mathf.Abs(v.y - b.y) < delta && Mathf.Abs(v.z - b.z) < delta;
		}

		public static bool Approx4(this Vector4 v, Vector4 b, float delta = 0.0001f)
		{
			return Mathf.Abs(v.x - b.x) < delta && Mathf.Abs(v.y - b.y) < delta && Mathf.Abs(v.z - b.z) < delta && Mathf.Abs(v.w - b.w) < delta;
		}

		public static bool ApproxC(this Color a, Color b, float delta = 0.0001f)
		{
			return Mathf.Abs(a.r - b.r) < delta && Mathf.Abs(a.g - b.g) < delta && Mathf.Abs(a.b - b.b) < delta && Mathf.Abs(a.a - b.a) < delta;
		}

		public static bool Approx(this float a, float b, float delta = 0.0001f)
		{
			return Mathf.Abs(b - a) < Mathf.Abs(delta);
		}

		public static bool ContainsApprox(Vector3[] v, Vector3 p, float eps)
		{
			for (int i = 0; i < v.Length; i++)
			{
				if (v[i].Approx3(p, eps))
				{
					return true;
				}
			}
			return false;
		}

		public static int Wrap(int value, int lowerBound, int upperBound)
		{
			int num = upperBound - lowerBound + 1;
			if (value < lowerBound)
			{
				value += num * ((lowerBound - value) / num + 1);
			}
			return lowerBound + (value - lowerBound) % num;
		}

		public static int Clamp(int value, int lowerBound, int upperBound)
		{
			return (value < lowerBound) ? lowerBound : ((value <= upperBound) ? value : upperBound);
		}

		public static Vector2 ToMask(this Vector2 vec, float delta = float.Epsilon)
		{
			return new Vector2((!(Mathf.Abs(vec.x) > delta)) ? 0f : 1f, (!(Mathf.Abs(vec.y) > delta)) ? 0f : 1f);
		}

		public static Vector3 ToMask(this Vector3 vec, float delta = float.Epsilon)
		{
			return new Vector3((!(Mathf.Abs(vec.x) > delta)) ? 0f : 1f, (!(Mathf.Abs(vec.y) > delta)) ? 0f : 1f, (!(Mathf.Abs(vec.z) > delta)) ? 0f : 1f);
		}

		public static Vector3 ToSignedMask(this Vector3 vec, float delta = float.Epsilon)
		{
			return new Vector3((!(Mathf.Abs(vec.x) > delta)) ? 0f : (vec.x / Mathf.Abs(vec.x)), (!(Mathf.Abs(vec.y) > delta)) ? 0f : (vec.y / Mathf.Abs(vec.y)), (!(Mathf.Abs(vec.z) > delta)) ? 0f : (vec.z / Mathf.Abs(vec.z)));
		}

		public static Vector3 Abs(this Vector3 v)
		{
			return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
		}

		public static int IntSum(this Vector3 mask)
		{
			return (int)Mathf.Abs(mask.x) + (int)Mathf.Abs(mask.y) + (int)Mathf.Abs(mask.z);
		}

		public static void Cross(Vector3 a, Vector3 b, ref float x, ref float y, ref float z)
		{
			x = a.y * b.z - a.z * b.y;
			y = a.z * b.x - a.x * b.z;
			z = a.x * b.y - a.y * b.x;
		}

		public static void Cross(Vector3 a, Vector3 b, ref Vector3 res)
		{
			res.x = a.y * b.z - a.z * b.y;
			res.y = a.z * b.x - a.x * b.z;
			res.z = a.x * b.y - a.y * b.x;
		}

		public static void Cross(float ax, float ay, float az, float bx, float by, float bz, ref float x, ref float y, ref float z)
		{
			x = ay * bz - az * by;
			y = az * bx - ax * bz;
			z = ax * by - ay * bx;
		}

		public static void Subtract(Vector3 a, Vector3 b, ref Vector3 res)
		{
			res.x = b.x - a.x;
			res.y = b.y - a.y;
			res.z = b.z - a.z;
		}
	}
}
