using System;
using UnityEngine;

namespace RootMotion
{
	public static class V3Tools
	{
		public static Vector3 ExtractVertical(Vector3 v, Vector3 verticalAxis, float weight)
		{
			if (weight == 0f)
			{
				return Vector3.zero;
			}

			return Vector3.Project(v, verticalAxis) * weight;
		}

		public static Vector3 ExtractHorizontal(Vector3 v, Vector3 normal, float weight)
		{
			if (weight == 0f)
			{
				return Vector3.zero;
			}

			Vector3 tangent = v;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			return Vector3.Project(v, tangent) * weight;
		}

		public static Vector3 ClampDirection(Vector3 direction, Vector3 normalDirection, float clampWeight,
			int clampSmoothing, out bool changed)
		{
			changed = false;
			if (clampWeight <= 0f)
			{
				return direction;
			}

			if (clampWeight >= 1f)
			{
				changed = true;
				return normalDirection;
			}

			float num = Vector3.Angle(normalDirection, direction);
			float num2 = 1f - num / 180f;
			if (num2 > clampWeight)
			{
				return direction;
			}

			changed = true;
			float num3 = ((!(clampWeight > 0f)) ? 1f : Mathf.Clamp(1f - (clampWeight - num2) / (1f - num2), 0f, 1f));
			float num4 = ((!(clampWeight > 0f)) ? 1f : Mathf.Clamp(num2 / clampWeight, 0f, 1f));
			for (int i = 0; i < clampSmoothing; i++)
			{
				float f = num4 * (float)Math.PI * 0.5f;
				num4 = Mathf.Sin(f);
			}

			return Vector3.Slerp(normalDirection, direction, num4 * num3);
		}

		public static Vector3 LineToPlane(Vector3 origin, Vector3 direction, Vector3 planeNormal, Vector3 planePoint)
		{
			float num = Vector3.Dot(planePoint - origin, planeNormal);
			float num2 = Vector3.Dot(direction, planeNormal);
			if (num2 == 0f)
			{
				return Vector3.zero;
			}

			float num3 = num / num2;
			return origin + direction.normalized * num3;
		}
	}
}
