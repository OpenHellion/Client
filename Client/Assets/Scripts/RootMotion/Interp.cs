using System;
using UnityEngine;

namespace RootMotion
{
	public class Interp
	{
		public static float Float(float t, InterpolationMode mode)
		{
			switch (mode)
			{
				case InterpolationMode.None:
					return None(t, 0f, 1f);
				case InterpolationMode.InOutCubic:
					return InOutCubic(t, 0f, 1f);
				case InterpolationMode.InOutQuintic:
					return InOutQuintic(t, 0f, 1f);
				case InterpolationMode.InQuintic:
					return InQuintic(t, 0f, 1f);
				case InterpolationMode.InQuartic:
					return InQuartic(t, 0f, 1f);
				case InterpolationMode.InCubic:
					return InCubic(t, 0f, 1f);
				case InterpolationMode.InQuadratic:
					return InQuadratic(t, 0f, 1f);
				case InterpolationMode.OutQuintic:
					return OutQuintic(t, 0f, 1f);
				case InterpolationMode.OutQuartic:
					return OutQuartic(t, 0f, 1f);
				case InterpolationMode.OutCubic:
					return OutCubic(t, 0f, 1f);
				case InterpolationMode.OutInCubic:
					return OutInCubic(t, 0f, 1f);
				case InterpolationMode.OutInQuartic:
					return OutInCubic(t, 0f, 1f);
				case InterpolationMode.BackInCubic:
					return BackInCubic(t, 0f, 1f);
				case InterpolationMode.BackInQuartic:
					return BackInQuartic(t, 0f, 1f);
				case InterpolationMode.OutBackCubic:
					return OutBackCubic(t, 0f, 1f);
				case InterpolationMode.OutBackQuartic:
					return OutBackQuartic(t, 0f, 1f);
				case InterpolationMode.OutElasticSmall:
					return OutElasticSmall(t, 0f, 1f);
				case InterpolationMode.OutElasticBig:
					return OutElasticBig(t, 0f, 1f);
				case InterpolationMode.InElasticSmall:
					return InElasticSmall(t, 0f, 1f);
				case InterpolationMode.InElasticBig:
					return InElasticBig(t, 0f, 1f);
				case InterpolationMode.InSine:
					return InSine(t, 0f, 1f);
				case InterpolationMode.OutSine:
					return OutSine(t, 0f, 1f);
				case InterpolationMode.InOutSine:
					return InOutSine(t, 0f, 1f);
				case InterpolationMode.InElastic:
					return OutElastic(t, 0f, 1f);
				case InterpolationMode.OutElastic:
					return OutElastic(t, 0f, 1f);
				case InterpolationMode.InBack:
					return InBack(t, 0f, 1f);
				case InterpolationMode.OutBack:
					return OutBack(t, 0f, 1f);
				default:
					return 0f;
			}
		}

		public static Vector3 V3(Vector3 v1, Vector3 v2, float t, InterpolationMode mode)
		{
			float num = Float(t, mode);
			return (1f - num) * v1 + num * v2;
		}

		public static float LerpValue(float value, float target, float increaseSpeed, float decreaseSpeed)
		{
			if (value == target)
			{
				return target;
			}

			if (value < target)
			{
				return Mathf.Clamp(value + Time.deltaTime * increaseSpeed, float.NegativeInfinity, target);
			}

			return Mathf.Clamp(value - Time.deltaTime * decreaseSpeed, target, float.PositiveInfinity);
		}

		private static float None(float t, float b, float c)
		{
			return b + c * t;
		}

		private static float InOutCubic(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * (-2f * num2 + 3f * num);
		}

		private static float InOutQuintic(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * (6f * num2 * num + -15f * num * num + 10f * num2);
		}

		private static float InQuintic(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * (num2 * num);
		}

		private static float InQuartic(float t, float b, float c)
		{
			float num = t * t;
			return b + c * (num * num);
		}

		private static float InCubic(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * num2;
		}

		private static float InQuadratic(float t, float b, float c)
		{
			float num = t * t;
			return b + c * num;
		}

		private static float OutQuintic(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * (num2 * num + -5f * num * num + 10f * num2 + -10f * num + 5f * t);
		}

		private static float OutQuartic(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * (-1f * num * num + 4f * num2 + -6f * num + 4f * t);
		}

		private static float OutCubic(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * (num2 + -3f * num + 3f * t);
		}

		private static float OutInCubic(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * (4f * num2 + -6f * num + 3f * t);
		}

		private static float OutInQuartic(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * (6f * num2 + -9f * num + 4f * t);
		}

		private static float BackInCubic(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * (4f * num2 + -3f * num);
		}

		private static float BackInQuartic(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * (2f * num * num + 2f * num2 + -3f * num);
		}

		private static float OutBackCubic(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * (4f * num2 + -9f * num + 6f * t);
		}

		private static float OutBackQuartic(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * (-2f * num * num + 10f * num2 + -15f * num + 8f * t);
		}

		private static float OutElasticSmall(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * (33f * num2 * num + -106f * num * num + 126f * num2 + -67f * num + 15f * t);
		}

		private static float OutElasticBig(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * (56f * num2 * num + -175f * num * num + 200f * num2 + -100f * num + 20f * t);
		}

		private static float InElasticSmall(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * (33f * num2 * num + -59f * num * num + 32f * num2 + -5f * num);
		}

		private static float InElasticBig(float t, float b, float c)
		{
			float num = t * t;
			float num2 = num * t;
			return b + c * (56f * num2 * num + -105f * num * num + 60f * num2 + -10f * num);
		}

		private static float InSine(float t, float b, float c)
		{
			c -= b;
			return (0f - c) * Mathf.Cos(t / 1f * ((float)Math.PI / 2f)) + c + b;
		}

		private static float OutSine(float t, float b, float c)
		{
			c -= b;
			return c * Mathf.Sin(t / 1f * ((float)Math.PI / 2f)) + b;
		}

		private static float InOutSine(float t, float b, float c)
		{
			c -= b;
			return (0f - c) / 2f * (Mathf.Cos((float)Math.PI * t / 1f) - 1f) + b;
		}

		private static float InElastic(float t, float b, float c)
		{
			c -= b;
			float num = 1f;
			float num2 = num * 0.3f;
			float num3 = 0f;
			float num4 = 0f;
			if (t == 0f)
			{
				return b;
			}

			if ((t /= num) == 1f)
			{
				return b + c;
			}

			if (num4 == 0f || num4 < Mathf.Abs(c))
			{
				num4 = c;
				num3 = num2 / 4f;
			}
			else
			{
				num3 = num2 / ((float)Math.PI * 2f) * Mathf.Asin(c / num4);
			}

			return 0f - num4 * Mathf.Pow(2f, 10f * (t -= 1f)) *
				Mathf.Sin((t * num - num3) * ((float)Math.PI * 2f) / num2) + b;
		}

		private static float OutElastic(float t, float b, float c)
		{
			c -= b;
			float num = 1f;
			float num2 = num * 0.3f;
			float num3 = 0f;
			float num4 = 0f;
			if (t == 0f)
			{
				return b;
			}

			if ((t /= num) == 1f)
			{
				return b + c;
			}

			if (num4 == 0f || num4 < Mathf.Abs(c))
			{
				num4 = c;
				num3 = num2 / 4f;
			}
			else
			{
				num3 = num2 / ((float)Math.PI * 2f) * Mathf.Asin(c / num4);
			}

			return num4 * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * num - num3) * ((float)Math.PI * 2f) / num2) + c + b;
		}

		private static float InBack(float t, float b, float c)
		{
			c -= b;
			t /= 1f;
			float num = 1.70158f;
			return c * t * t * ((num + 1f) * t - num) + b;
		}

		private static float OutBack(float t, float b, float c)
		{
			float num = 1.70158f;
			c -= b;
			t = t / 1f - 1f;
			return c * (t * t * ((num + 1f) * t + num) + 1f) + b;
		}
	}
}
