using System;
using UnityEngine;

namespace ZeroGravity.Math
{
	public static class MathHelper
	{
		public const double RadToDeg = 180.0 / System.Math.PI;

		public const double DegToRad = System.Math.PI / 180.0;

		private static System.Random _randGenerator = new System.Random();

		public static float DimbeProgression(float val)
		{
			val *= 7.8f;
			return (Mathf.Log(val + 0.2f, 2f) + 2.3f) / 5.3f;
		}

		public static int Clamp(int value, int min, int max)
		{
			return (value < min) ? min : ((value <= max) ? value : max);
		}

		public static float Clamp(float value, float min, float max)
		{
			return (value < min) ? min : ((!(value > max)) ? value : max);
		}

		public static double Clamp(double value, double min, double max)
		{
			return (value < min) ? min : ((!(value > max)) ? value : max);
		}

		public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
		{
			return new Vector3(Clamp(value.x, min.x, max.x), Clamp(value.y, min.y, max.y),
				Clamp(value.z, min.z, max.z));
		}

		public static float Lerp(float value1, float value2, float amount)
		{
			return value1 + (value2 - value1) * Clamp(amount, 0f, 1f);
		}

		public static double Lerp(double value1, double value2, double amount)
		{
			return value1 + (value2 - value1) * Clamp(amount, 0.0, 1.0);
		}

		public static float LerpValue(float fromVelocity, float toVelocity, float lerpAmount, float epsilon = 0.01f)
		{
			if (fromVelocity != toVelocity)
			{
				fromVelocity = ((!(fromVelocity < toVelocity))
					? System.Math.Max(fromVelocity + (toVelocity - fromVelocity) * Clamp(lerpAmount, 0f, 1f),
						toVelocity)
					: System.Math.Min(fromVelocity + (toVelocity - fromVelocity) * Clamp(lerpAmount, 0f, 1f),
						toVelocity));
				if (System.Math.Abs(toVelocity - fromVelocity) < epsilon)
				{
					fromVelocity = toVelocity;
				}
			}

			return fromVelocity;
		}

		public static double SmoothStep(double value1, double value2, double amount)
		{
			return Hermite(value1, 0.0, value2, 0.0, Clamp(amount, 0.0, 1.0));
		}

		public static int Sign(double value)
		{
			if (value < 0.0)
			{
				return -1;
			}

			return 1;
		}

		public static double Hermite(double value1, double tangent1, double value2, double tangent2, double amount)
		{
			double num = amount * amount * amount;
			double num2 = amount * amount;
			if (amount == 0.0)
			{
				return value1;
			}

			if (amount == 1.0)
			{
				return value2;
			}

			return (2.0 * value1 - 2.0 * value2 + tangent2 + tangent1) * num +
			       (3.0 * value2 - 3.0 * value1 - 2.0 * tangent1 - tangent2) * num2 + tangent1 * amount + value1;
		}

		public static float ProportionalValue(float basedOnCurrent, float basedOnMin, float basedOnMax, float resultMin,
			float resoultMax)
		{
			return resultMin + (resoultMax - resultMin) * ((basedOnCurrent - basedOnMin) / (basedOnMax - basedOnMin));
		}

		public static Vector3D ProportionalValue(Vector3D basedOnCurrent, Vector3D basedOnMin, Vector3D basedOnMax,
			Vector3D resultMin, Vector3D resoultMax)
		{
			return resultMin + (resoultMax - resultMin) *
				((basedOnCurrent - basedOnMin).Magnitude / (basedOnMax - basedOnMin).Magnitude);
		}

		public static bool SameSign(Vector3 first, Vector3 second)
		{
			float num = Sign(first.x) * Sign(second.x) + Sign(first.y) * Sign(second.y) +
			            Sign(first.z) * Sign(second.z);
			return num == 3f;
		}

		public static float SetEpsilonZero(float value, float epsilon = float.Epsilon)
		{
			return (!(System.Math.Abs(value) > epsilon)) ? 0f : value;
		}

		public static long LongRandom(long min, long max, System.Random rand)
		{
			byte[] array = new byte[8];
			rand.NextBytes(array);
			long num = BitConverter.ToInt64(array, 0);
			return System.Math.Abs(num % (max - min)) + min;
		}

		public static Quaternion QuaternionSlerp(Quaternion from, Quaternion to, float amount, ref bool done)
		{
			float num = from.x * to.x + from.y * to.y + from.z * to.z + from.w * to.w;
			bool flag = false;
			if (num < 0f)
			{
				flag = true;
				num = 0f - num;
			}

			float num2;
			float num3;
			if (num > 0.999999f)
			{
				num2 = 1f - amount;
				num3 = ((!flag) ? amount : (0f - amount));
				done = true;
			}
			else
			{
				float num4 = Mathf.Acos(num);
				float num5 = 1f / Mathf.Sin(num4);
				num2 = Mathf.Sin((1f - amount) * num4) * num5;
				num3 = ((!flag) ? (Mathf.Sin(amount * num4) * num5) : ((0f - Mathf.Sin(amount * num4)) * num5));
			}

			return new Quaternion(num2 * from.x + num3 * to.x, num2 * from.y + num3 * to.y, num2 * from.z + num3 * to.z,
				num2 * from.w + num3 * to.w);
		}

		public static float AngleSigned(Vector3 vec1, Vector3 vec2, Vector3 planeNormal)
		{
			return Vector3.Angle(vec1, vec2) * (float)Sign(Vector3.Dot(planeNormal, Vector3.Cross(vec1, vec2)));
		}

		public static Vector3 RotateAroundPivot(Vector3 vector, Vector3 pivot, Vector3 angles)
		{
			return Quaternion.Euler(angles) * (vector - pivot) + pivot;
		}

		public static double AngleSigned(Vector3D vec1, Vector3D vec2, Vector3D planeNormal)
		{
			return Vector3D.Angle(vec1, vec2) * (double)Sign(Vector3D.Dot(planeNormal, Vector3D.Cross(vec1, vec2)));
		}

		public static Vector3D RotateAroundPivot(Vector3D vector, Vector3D pivot, Vector3D angles)
		{
			return QuaternionD.Euler(angles) * (vector - pivot) + pivot;
		}

		public static float AverageMaxValue(float a, float b, float c, float maxA, float maxB, float maxC)
		{
			return (a + b + c) / (a / maxA + b / maxB + c / maxC);
		}

		public static double Acosh(double x)
		{
			return System.Math.Log(x + System.Math.Sqrt(x * x - 1.0));
		}

		public static float RandomRange(float min, float max)
		{
			return (float)(_randGenerator.NextDouble() * (double)(max - min) + (double)min);
		}

		public static double RandomRange(double min, double max)
		{
			return _randGenerator.NextDouble() * (max - min) + min;
		}

		public static double RandomRange(double min, double max, int seed)
		{
			System.Random random = new System.Random(seed);
			return random.NextDouble() * (max - min) + min;
		}

		public static int RandomRange(int min, int max)
		{
			return _randGenerator.Next(min, max);
		}

		public static double RandomNextDouble()
		{
			return _randGenerator.NextDouble();
		}

		public static int RandomNextInt()
		{
			return _randGenerator.Next();
		}

		public static Vector3 RayPlaneIntersect(Ray ray, Vector3 planePos, Vector3 planeUp)
		{
			float num = Vector3.Dot(ray.direction, planeUp);
			float num2 = Vector3.Dot(planePos - ray.origin, planeUp) / num;
			return ray.origin + ray.direction * num2;
		}
	}
}
