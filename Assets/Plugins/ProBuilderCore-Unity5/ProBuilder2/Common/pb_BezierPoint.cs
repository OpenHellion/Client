using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder2.Common
{
	[Serializable]
	public struct pb_BezierPoint
	{
		public Vector3 position;

		public Vector3 tangentIn;

		public Vector3 tangentOut;

		public Quaternion rotation;

		public pb_BezierPoint(Vector3 position, Vector3 tangentIn, Vector3 tangentOut, Quaternion rotation)
		{
			this.position = position;
			this.tangentIn = tangentIn;
			this.tangentOut = tangentOut;
			this.rotation = rotation;
		}

		public void EnforceTangentMode(pb_BezierTangentDirection master, pb_BezierTangentMode mode)
		{
			switch (mode)
			{
			case pb_BezierTangentMode.Aligned:
				if (master == pb_BezierTangentDirection.In)
				{
					tangentOut = position + (tangentOut - position).normalized * (tangentIn - position).magnitude;
				}
				else
				{
					tangentIn = position + (tangentIn - position).normalized * (tangentOut - position).magnitude;
				}
				break;
			case pb_BezierTangentMode.Mirrored:
				if (master == pb_BezierTangentDirection.In)
				{
					tangentOut = position - (tangentIn - position);
				}
				else
				{
					tangentIn = position - (tangentOut - position);
				}
				break;
			}
		}

		public void SetPosition(Vector3 position)
		{
			Vector3 vector = position - this.position;
			this.position = position;
			tangentIn += vector;
			tangentOut += vector;
		}

		public void SetTangentIn(Vector3 tangent, pb_BezierTangentMode mode)
		{
			tangentIn = tangent;
			EnforceTangentMode(pb_BezierTangentDirection.In, mode);
		}

		public void SetTangentOut(Vector3 tangent, pb_BezierTangentMode mode)
		{
			tangentOut = tangent;
			EnforceTangentMode(pb_BezierTangentDirection.Out, mode);
		}

		public static Vector3 QuadraticPosition(pb_BezierPoint a, pb_BezierPoint b, float t)
		{
			float x = (1f - t) * (1f - t) * a.position.x + 2f * (1f - t) * t * a.tangentOut.x + t * t * b.position.x;
			float y = (1f - t) * (1f - t) * a.position.y + 2f * (1f - t) * t * a.tangentOut.y + t * t * b.position.y;
			float z = (1f - t) * (1f - t) * a.position.z + 2f * (1f - t) * t * a.tangentOut.z + t * t * b.position.z;
			return new Vector3(x, y, z);
		}

		public static Vector3 CubicPosition(pb_BezierPoint a, pb_BezierPoint b, float t)
		{
			t = Mathf.Clamp01(t);
			float num = 1f - t;
			return num * num * num * a.position + 3f * num * num * t * a.tangentOut + 3f * num * t * t * b.tangentIn + t * t * t * b.position;
		}

		public static Vector3 GetLookDirection(IList<pb_BezierPoint> points, int index, int previous, int next)
		{
			if (previous < 0)
			{
				return (points[index].position - QuadraticPosition(points[index], points[next], 0.1f)).normalized;
			}
			if (next < 0)
			{
				return (QuadraticPosition(points[index], points[previous], 0.1f) - points[index].position).normalized;
			}
			if (next > -1 && previous > -1)
			{
				Vector3 normalized = (QuadraticPosition(points[index], points[previous], 0.1f) - points[index].position).normalized;
				Vector3 normalized2 = (QuadraticPosition(points[index], points[next], 0.1f) - points[index].position).normalized;
				return ((normalized + normalized2) * 0.5f).normalized;
			}
			return Vector3.forward;
		}
	}
}
