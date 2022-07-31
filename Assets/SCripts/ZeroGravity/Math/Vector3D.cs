using System;
using UnityEngine;

namespace ZeroGravity.Math
{
	[Serializable]
	public struct Vector3D
	{
		private const double epsilon = 1E-06;

		public double X;

		public double Y;

		public double Z;

		public static Vector3D Back
		{
			get
			{
				return new Vector3D(0.0, 0.0, -1.0);
			}
		}

		public static Vector3D Down
		{
			get
			{
				return new Vector3D(0.0, -1.0, 0.0);
			}
		}

		public static Vector3D Forward
		{
			get
			{
				return new Vector3D(0.0, 0.0, 1.0);
			}
		}

		public static Vector3D Left
		{
			get
			{
				return new Vector3D(-1.0, 0.0, 0.0);
			}
		}

		public static Vector3D One
		{
			get
			{
				return new Vector3D(1.0, 1.0, 1.0);
			}
		}

		public static Vector3D Right
		{
			get
			{
				return new Vector3D(1.0, 0.0, 0.0);
			}
		}

		public static Vector3D Up
		{
			get
			{
				return new Vector3D(0.0, 1.0, 0.0);
			}
		}

		public static Vector3D Zero
		{
			get
			{
				return new Vector3D(0.0, 0.0, 0.0);
			}
		}

		public double Magnitude
		{
			get
			{
				return System.Math.Sqrt(X * X + Y * Y + Z * Z);
			}
		}

		public double SqrMagnitude
		{
			get
			{
				return X * X + Y * Y + Z * Z;
			}
		}

		public Vector3D Normalized
		{
			get
			{
				return Normalize(this);
			}
		}

		public double this[int index]
		{
			get
			{
				switch (index)
				{
				case 0:
					return X;
				case 1:
					return Y;
				case 2:
					return Z;
				default:
					throw new IndexOutOfRangeException("Invalid Vector3 index!");
				}
			}
			set
			{
				switch (index)
				{
				case 0:
					X = value;
					break;
				case 1:
					Y = value;
					break;
				case 2:
					Z = value;
					break;
				default:
					throw new IndexOutOfRangeException("Invalid Vector3 index!");
				}
			}
		}

		public Vector3D(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public Vector3D(double x, double y)
		{
			X = x;
			Y = y;
			Z = 0.0;
		}

		public Vector3D(Vector3 other)
		{
			X = other.x;
			Y = other.y;
			Z = other.z;
		}

		public static double Angle(Vector3D from, Vector3D to)
		{
			return System.Math.Acos(MathHelper.Clamp(Dot(from.Normalized, to.Normalized), -1.0, 1.0)) * (180.0 / System.Math.PI);
		}

		public static Vector3D ClampMagnitude(Vector3D vector, double maxLength)
		{
			if (vector.SqrMagnitude > maxLength * maxLength)
			{
				return vector.Normalized * maxLength;
			}
			return vector;
		}

		public static Vector3D Cross(Vector3D lhs, Vector3D rhs)
		{
			return new Vector3D(lhs.Y * rhs.Z - lhs.Z * rhs.Y, lhs.Z * rhs.X - lhs.X * rhs.Z, lhs.X * rhs.Y - lhs.Y * rhs.X);
		}

		public static double Distance(Vector3D a, Vector3D b)
		{
			Vector3D vector3D = new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
			return System.Math.Sqrt(vector3D.X * vector3D.X + vector3D.Y * vector3D.Y + vector3D.Z * vector3D.Z);
		}

		public static double Dot(Vector3D lhs, Vector3D rhs)
		{
			return lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z;
		}

		private static void Internal_OrthoNormalize2(ref Vector3D a, ref Vector3D b)
		{
			INTERNAL_CALL_Internal_OrthoNormalize2(ref a, ref b);
		}

		private static void Internal_OrthoNormalize3(ref Vector3D a, ref Vector3D b, ref Vector3D c)
		{
			INTERNAL_CALL_Internal_OrthoNormalize3(ref a, ref b, ref c);
		}

		public static Vector3D Lerp(Vector3D a, Vector3D b, double t)
		{
			t = MathHelper.Clamp(t, 0.0, 1.0);
			return new Vector3D(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);
		}

		public static Vector3D LerpUnclamped(Vector3D a, Vector3D b, double t)
		{
			return new Vector3D(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);
		}

		public static Vector3D Max(Vector3D lhs, Vector3D rhs)
		{
			return new Vector3D(System.Math.Max(lhs.X, rhs.X), System.Math.Max(lhs.Y, rhs.Y), System.Math.Max(lhs.Z, rhs.Z));
		}

		public static Vector3D Min(Vector3D lhs, Vector3D rhs)
		{
			return new Vector3D(System.Math.Min(lhs.X, rhs.X), System.Math.Min(lhs.Y, rhs.Y), System.Math.Min(lhs.Z, rhs.Z));
		}

		public static Vector3D MoveTowards(Vector3D current, Vector3D target, double maxDistanceDelta)
		{
			Vector3D vector3D = target - current;
			double magnitude = vector3D.Magnitude;
			if (magnitude <= maxDistanceDelta || magnitude == 0.0)
			{
				return target;
			}
			return current + vector3D / magnitude * maxDistanceDelta;
		}

		public static Vector3D Normalize(Vector3D value)
		{
			double magnitude = value.Magnitude;
			if (magnitude > 1E-06)
			{
				return value / magnitude;
			}
			return Zero;
		}

		public static void OrthoNormalize(ref Vector3D normal, ref Vector3D tangent)
		{
			Internal_OrthoNormalize2(ref normal, ref tangent);
		}

		public static void OrthoNormalize(ref Vector3D normal, ref Vector3D tangent, ref Vector3D binormal)
		{
			Internal_OrthoNormalize3(ref normal, ref tangent, ref binormal);
		}

		public static Vector3D Project(Vector3D vector, Vector3D onNormal)
		{
			double num = Dot(onNormal, onNormal);
			if (num < double.Epsilon)
			{
				return Zero;
			}
			return onNormal * Dot(vector, onNormal) / num;
		}

		public static Vector3D ProjectOnPlane(Vector3D vector, Vector3D planeNormal)
		{
			return vector - Project(vector, planeNormal);
		}

		public static Vector3D Reflect(Vector3D inDirection, Vector3D inNormal)
		{
			return -2.0 * Dot(inNormal, inDirection) * inNormal + inDirection;
		}

		public static Vector3D RotateTowards(Vector3D current, Vector3D target, double maxRadiansDelta, double maxMagnitudeDelta)
		{
			Vector3D value;
			INTERNAL_CALL_RotateTowards(ref current, ref target, maxRadiansDelta, maxMagnitudeDelta, out value);
			return value;
		}

		public static Vector3D Scale(Vector3D a, Vector3D b)
		{
			return new Vector3D(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
		}

		public static Vector3D Slerp(Vector3D a, Vector3D b, double t)
		{
			Vector3D value;
			INTERNAL_CALL_Slerp(ref a, ref b, t, out value);
			return value;
		}

		public static Vector3D SlerpUnclamped(Vector3D a, Vector3D b, double t)
		{
			Vector3D value;
			INTERNAL_CALL_SlerpUnclamped(ref a, ref b, t, out value);
			return value;
		}

		public static Vector3D SmoothDamp(Vector3D current, Vector3D target, ref Vector3D currentVelocity, double smoothTime, double deltaTime)
		{
			return SmoothDamp(current, target, ref currentVelocity, smoothTime, double.PositiveInfinity, deltaTime);
		}

		public static Vector3D SmoothDamp(Vector3D current, Vector3D target, ref Vector3D currentVelocity, double smoothTime, double maxSpeed, double deltaTime)
		{
			smoothTime = System.Math.Max(0.0001, smoothTime);
			double num = 2.0 / smoothTime;
			double num2 = num * deltaTime;
			double num3 = 1.0 / (1.0 + num2 + 0.48 * num2 * num2 + 0.235 * num2 * num2 * num2);
			Vector3D vector = current - target;
			Vector3D vector3D = target;
			double maxLength = maxSpeed * smoothTime;
			vector = ClampMagnitude(vector, maxLength);
			target = current - vector;
			Vector3D vector3D2 = (currentVelocity + num * vector) * deltaTime;
			currentVelocity = (currentVelocity - num * vector3D2) * num3;
			Vector3D vector3D3 = target + (vector + vector3D2) * num3;
			if (Dot(vector3D - current, vector3D3 - vector3D) > 0.0)
			{
				vector3D3 = vector3D;
				currentVelocity = (vector3D3 - vector3D) / deltaTime;
			}
			return vector3D3;
		}

		public override bool Equals(object other)
		{
			if (!(other is Vector3D))
			{
				return false;
			}
			Vector3D vector3D = (Vector3D)other;
			return X.Equals(vector3D.X) && Y.Equals(vector3D.Y) && Z.Equals(vector3D.Z);
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() >> 2);
		}

		public void Normalize()
		{
			double magnitude = Magnitude;
			if (magnitude > 1E-06)
			{
				this /= magnitude;
			}
			else
			{
				this = Zero;
			}
		}

		public void Scale(Vector3D scale)
		{
			X *= scale.X;
			Y *= scale.Y;
			Z *= scale.Z;
		}

		public void Set(double new_x, double new_y, double new_z)
		{
			X = new_x;
			Y = new_y;
			Z = new_z;
		}

		public string ToString(string format)
		{
			return string.Format("({0}, {1}, {2})", X.ToString(format), Y.ToString(format), Z.ToString(format));
		}

		public override string ToString()
		{
			return string.Format("({0:0.###}, {1:0.###}, {2:0.###})", X, Y, Z);
		}

		public static Vector3D operator +(Vector3D a, Vector3D b)
		{
			return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}

		public static Vector3D operator /(Vector3D a, double d)
		{
			return new Vector3D(a.X / d, a.Y / d, a.Z / d);
		}

		public static bool operator ==(Vector3D lhs, Vector3D rhs)
		{
			return (lhs - rhs).SqrMagnitude < 9.999999E-11;
		}

		public static bool operator !=(Vector3D lhs, Vector3D rhs)
		{
			return (lhs - rhs).SqrMagnitude >= 9.999999E-11;
		}

		public static Vector3D operator *(double d, Vector3D a)
		{
			return new Vector3D(a.X * d, a.Y * d, a.Z * d);
		}

		public static Vector3D operator *(Vector3D a, double d)
		{
			return new Vector3D(a.X * d, a.Y * d, a.Z * d);
		}

		public static Vector3D operator -(Vector3D a, Vector3D b)
		{
			return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}

		public static Vector3D operator -(Vector3D a)
		{
			return new Vector3D(0.0 - a.X, 0.0 - a.Y, 0.0 - a.Z);
		}

		private static void INTERNAL_CALL_Internal_OrthoNormalize2(ref Vector3D a, ref Vector3D b)
		{
			throw new Exception("INTERNAL_CALL_Internal_OrthoNormalize2 IS NOT IMPLEMENTED");
		}

		private static void INTERNAL_CALL_Internal_OrthoNormalize3(ref Vector3D a, ref Vector3D b, ref Vector3D c)
		{
			a.Normalize();
			double num = Dot(a, b);
			b -= num * a;
			b.Normalize();
			double num2 = Dot(b, c);
			num = Dot(a, c);
			c -= num * a + num2 * b;
			c.Normalize();
		}

		private static void INTERNAL_CALL_RotateTowards(ref Vector3D current, ref Vector3D target, double maxRadiansDelta, double maxMagnitudeDelta, out Vector3D value)
		{
			value = Zero;
			throw new Exception("INTERNAL_CALL_RotateTowards IS NOT IMPLEMENTED");
		}

		private static void INTERNAL_CALL_Slerp(ref Vector3D a, ref Vector3D b, double t, out Vector3D value)
		{
			value = Zero;
			throw new Exception("INTERNAL_CALL_Slerp IS NOT IMPLEMENTED");
		}

		private static void INTERNAL_CALL_SlerpUnclamped(ref Vector3D a, ref Vector3D b, double t, out Vector3D value)
		{
			value = Zero;
			throw new Exception("INTERNAL_CALL_SlerpUnclamped IS NOT IMPLEMENTED");
		}
	}
}
