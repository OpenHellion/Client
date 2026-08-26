using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder2.Common
{
	public static class pb_Spline
	{
		public static pb_Object Extrude(IList<pb_BezierPoint> points, float radius = 0.5f, int columns = 32, int rows = 16, bool closeLoop = false, bool smooth = true)
		{
			pb_Object target = null;
			Extrude(points, radius, columns, rows, closeLoop, smooth, ref target);
			return target;
		}

		public static void Extrude(IList<pb_BezierPoint> bezierPoints, float radius, int columns, int rows, bool closeLoop, bool smooth, ref pb_Object target)
		{
			List<Quaternion> list = new List<Quaternion>();
			List<Vector3> controlPoints = GetControlPoints(bezierPoints, columns, closeLoop, list);
			Extrude(controlPoints, radius, rows, closeLoop, smooth, ref target, list);
		}

		public static List<Vector3> GetControlPoints(IList<pb_BezierPoint> bezierPoints, int subdivisionsPerSegment, bool closeLoop, List<Quaternion> rotations)
		{
			int count = bezierPoints.Count;
			List<Vector3> list = new List<Vector3>(subdivisionsPerSegment * count);
			if (rotations != null)
			{
				rotations.Clear();
				rotations.Capacity = subdivisionsPerSegment * count;
			}
			int num = ((!closeLoop) ? (count - 1) : count);
			for (int i = 0; i < num; i++)
			{
				int num2 = ((closeLoop || i < count - 2) ? subdivisionsPerSegment : (subdivisionsPerSegment + 1));
				for (int j = 0; j < num2; j++)
				{
					float num3 = subdivisionsPerSegment;
					list.Add(pb_BezierPoint.CubicPosition(bezierPoints[i], bezierPoints[(i + 1) % count], (float)j / num3));
					rotations?.Add(Quaternion.Slerp(bezierPoints[i].rotation, bezierPoints[(i + 1) % count].rotation, (float)j / (float)(num2 - 1)));
				}
			}
			return list;
		}

		public static void Extrude(IList<Vector3> points, float radius, int radiusRows, bool closeLoop, bool smooth, ref pb_Object target, IList<Quaternion> pointRotations = null)
		{
			if (points == null || points.Count < 2)
			{
				return;
			}
			int count = points.Count;
			int num = Math.Max(3, radiusRows);
			int segments = num + 1;
			int num2 = num * 2;
			int num3 = ((!closeLoop) ? (count - 1) : count) * 2 * num2;
			bool flag = false;
			bool flag2 = pointRotations != null && pointRotations.Count == points.Count;
			Vector3[] array = new Vector3[num3];
			pb_Face[] array2 = ((!flag) ? new pb_Face[((!closeLoop) ? (count - 1) : count) * num] : null);
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = ((!closeLoop) ? (count - 1) : count);
			for (int i = 0; i < num7; i++)
			{
				float secant;
				Quaternion ringRotation = GetRingRotation(points, i, closeLoop, out secant);
				float secant2;
				Quaternion ringRotation2 = GetRingRotation(points, (i + 1) % count, closeLoop, out secant2);
				if (flag2)
				{
					ringRotation *= pointRotations[i];
					ringRotation2 *= pointRotations[(i + 1) % count];
				}
				Vector3[] sourceArray = VertexRing(ringRotation, points[i], radius, segments);
				Vector3[] sourceArray2 = VertexRing(ringRotation2, points[(i + 1) % count], radius, segments);
				Array.Copy(sourceArray, 0, array, num6, num2);
				num6 += num2;
				Array.Copy(sourceArray2, 0, array, num6, num2);
				num6 += num2;
				if (flag)
				{
					continue;
				}
				for (int j = 0; j < num2; j += 2)
				{
					array2[num5] = new pb_Face(new int[6]
					{
						num4,
						num4 + 1,
						num4 + num2,
						num4 + num2,
						num4 + 1,
						num4 + num2 + 1
					});
					if (smooth)
					{
						array2[num5].smoothingGroup = 2;
					}
					num5++;
					num4 += 2;
				}
				num4 += num2;
			}
			if (target != null)
			{
				if (array2 != null)
				{
					target.GeometryWithVerticesFaces(array, array2);
					return;
				}
				target.SetVertices(array);
				target.ToMesh();
				target.Refresh(RefreshMask.UV | RefreshMask.Colors | RefreshMask.Normals | RefreshMask.Tangents);
			}
			else
			{
				target = pb_Object.CreateInstanceWithVerticesFaces(array, array2);
			}
		}

		private static Quaternion GetRingRotation(IList<Vector3> points, int i, bool closeLoop, out float secant)
		{
			int count = points.Count;
			Vector3 vector;
			if (closeLoop || (i > 0 && i < count - 1))
			{
				int index = ((i >= 1) ? (i - 1) : (count - 1));
				int index2 = (i + 1) % count;
				Vector3 normalized = (points[i] - points[index]).normalized;
				Vector3 normalized2 = (points[index2] - points[i]).normalized;
				vector = (normalized + normalized2) * 0.5f;
				secant = pb_Math.Secant(Vector3.Angle(normalized, vector) * ((float)Math.PI / 180f));
			}
			else
			{
				vector = ((i >= 1) ? (points[i] - points[i - 1]) : (points[i + 1] - points[i]));
				secant = 1f;
			}
			vector.Normalize();
			if (vector.Approx3(Vector3.up) || vector.Approx3(Vector3.zero))
			{
				return Quaternion.identity;
			}
			return Quaternion.LookRotation(vector);
		}

		private static Vector3[] VertexRing(Quaternion orientation, Vector3 offset, float radius, int segments)
		{
			Vector3[] array = new Vector3[segments * 2];
			for (int i = 0; i < segments; i++)
			{
				float f = (float)i / (float)(segments - 1) * 360f * ((float)Math.PI / 180f);
				int num = (i + 1) % segments;
				float f2 = (float)num / (float)(segments - 1) * 360f * ((float)Math.PI / 180f);
				ref Vector3 reference = ref array[i * 2];
				reference = offset + orientation * new Vector3(Mathf.Cos(f) * radius, Mathf.Sin(f) * radius, 0f);
				ref Vector3 reference2 = ref array[i * 2 + 1];
				reference2 = offset + orientation * new Vector3(Mathf.Cos(f2) * radius, Mathf.Sin(f2) * radius, 0f);
			}
			return array;
		}
	}
}
