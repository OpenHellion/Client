using System;

namespace ZeroGravity.Math
{
	public struct QuaternionD
	{
		private const double epsilon = 1E-06;

		public double X;

		public double Y;

		public double Z;

		public double W;

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
					case 3:
						return W;
					default:
						throw new IndexOutOfRangeException("Invalid Quaternion index!");
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
					case 3:
						W = value;
						break;
					default:
						throw new IndexOutOfRangeException("Invalid Quaternion index!");
				}
			}
		}

		public static QuaternionD Identity
		{
			get { return new QuaternionD(0.0, 0.0, 0.0, 1.0); }
		}

		public Vector3D EulerAngles
		{
			get { return Internal_ToEulerRad(this) * (180.0 / System.Math.PI); }
			set { this = Internal_FromEulerRad(value * (System.Math.PI / 180.0)); }
		}

		public QuaternionD(double x, double y, double z, double w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public static QuaternionD operator *(QuaternionD lhs, QuaternionD rhs)
		{
			return new QuaternionD(lhs.W * rhs.X + lhs.X * rhs.W + lhs.Y * rhs.Z - lhs.Z * rhs.Y,
				lhs.W * rhs.Y + lhs.Y * rhs.W + lhs.Z * rhs.X - lhs.X * rhs.Z,
				lhs.W * rhs.Z + lhs.Z * rhs.W + lhs.X * rhs.Y - lhs.Y * rhs.X,
				lhs.W * rhs.W - lhs.X * rhs.X - lhs.Y * rhs.Y - lhs.Z * rhs.Z);
		}

		public static Vector3D operator *(QuaternionD rotation, Vector3D point)
		{
			double num = rotation.X * 2.0;
			double num2 = rotation.Y * 2.0;
			double num3 = rotation.Z * 2.0;
			double num4 = rotation.X * num;
			double num5 = rotation.Y * num2;
			double num6 = rotation.Z * num3;
			double num7 = rotation.X * num2;
			double num8 = rotation.X * num3;
			double num9 = rotation.Y * num3;
			double num10 = rotation.W * num;
			double num11 = rotation.W * num2;
			double num12 = rotation.W * num3;
			Vector3D result = default(Vector3D);
			result.X = (1.0 - (num5 + num6)) * point.X + (num7 - num12) * point.Y + (num8 + num11) * point.Z;
			result.Y = (num7 + num12) * point.X + (1.0 - (num4 + num6)) * point.Y + (num9 - num10) * point.Z;
			result.Z = (num8 - num11) * point.X + (num9 + num10) * point.Y + (1.0 - (num4 + num5)) * point.Z;
			return result;
		}

		public static bool operator ==(QuaternionD lhs, QuaternionD rhs)
		{
			return Dot(lhs, rhs) > 0.999998986721039;
		}

		public static bool operator !=(QuaternionD lhs, QuaternionD rhs)
		{
			return Dot(lhs, rhs) <= 0.999998986721039;
		}

		public void Set(double new_x, double new_y, double new_z, double new_w)
		{
			X = new_x;
			Y = new_y;
			Z = new_z;
			W = new_w;
		}

		public static double Dot(QuaternionD a, QuaternionD b)
		{
			return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
		}

		public static QuaternionD AngleAxis(double angle, Vector3D axis)
		{
			axis.Normalize();
			QuaternionD value;
			INTERNAL_CALL_AngleAxis(angle, ref axis, out value);
			return value;
		}

		public void ToAngleAxis(out double angle, out Vector3D axis)
		{
			Internal_ToAxisAngleRad(this, out axis, out angle);
			angle *= 180.0 / System.Math.PI;
		}

		public static QuaternionD FromToRotation(Vector3D fromDirection, Vector3D toDirection)
		{
			QuaternionD value;
			INTERNAL_CALL_FromToRotation(ref fromDirection, ref toDirection, out value);
			return value;
		}

		public void SetFromToRotation(Vector3D fromDirection, Vector3D toDirection)
		{
			this = FromToRotation(fromDirection, toDirection);
		}

		public static QuaternionD LookRotation(Vector3D forward, Vector3D upwards)
		{
			QuaternionD value;
			INTERNAL_CALL_LookRotation(ref forward, ref upwards, out value);
			return value;
		}

		public static QuaternionD LookRotation(Vector3D forward)
		{
			Vector3D up = Vector3D.Up;
			QuaternionD value;
			INTERNAL_CALL_LookRotation(ref forward, ref up, out value);
			return value;
		}

		public void SetLookRotation(Vector3D view)
		{
			SetLookRotation(view, Vector3D.Up);
		}

		public void SetLookRotation(Vector3D view, Vector3D up)
		{
			this = LookRotation(view, up);
		}

		public static QuaternionD Slerp(QuaternionD a, QuaternionD b, double t)
		{
			QuaternionD value;
			INTERNAL_CALL_Slerp(ref a, ref b, t, out value);
			return value;
		}

		public static QuaternionD SlerpUnclamped(QuaternionD a, QuaternionD b, double t)
		{
			QuaternionD value;
			INTERNAL_CALL_SlerpUnclamped(ref a, ref b, t, out value);
			return value;
		}

		public static QuaternionD Lerp(QuaternionD a, QuaternionD b, double t)
		{
			QuaternionD value;
			INTERNAL_CALL_Lerp(ref a, ref b, t, out value);
			return value;
		}

		public static QuaternionD LerpUnclamped(QuaternionD a, QuaternionD b, double t)
		{
			QuaternionD value;
			INTERNAL_CALL_LerpUnclamped(ref a, ref b, t, out value);
			return value;
		}

		public static QuaternionD RotateTowards(QuaternionD from, QuaternionD to, double maxDegreesDelta)
		{
			double num = Angle(from, to);
			if (num == 0.0)
			{
				return to;
			}

			double t = System.Math.Min(1.0, maxDegreesDelta / num);
			return SlerpUnclamped(from, to, t);
		}

		public static QuaternionD Inverse(QuaternionD rotation)
		{
			QuaternionD value;
			INTERNAL_CALL_Inverse(ref rotation, out value);
			return value;
		}

		public override string ToString()
		{
			return string.Format("({0:0.###}, {1:0.###}, {2:0.###}, {3:0.###})", X, Y, Z, W);
		}

		public string ToString(string format)
		{
			return string.Format("({0}, {1}, {2}, {3})", X.ToString(format), Y.ToString(format), Z.ToString(format),
				W.ToString(format));
		}

		public static double Angle(QuaternionD a, QuaternionD b)
		{
			return System.Math.Acos(System.Math.Min(System.Math.Abs(Dot(a, b)), 1.0)) * 2.0 * (180.0 / System.Math.PI);
		}

		public static QuaternionD Euler(double x, double y, double z)
		{
			return Internal_FromEulerRad(new Vector3D(x, y, z) * (System.Math.PI / 180.0));
		}

		public static QuaternionD Euler(Vector3D euler)
		{
			return Internal_FromEulerRad(euler * (System.Math.PI / 180.0));
		}

		private static Vector3D Internal_ToEulerRad(QuaternionD rotation)
		{
			Vector3D value;
			INTERNAL_CALL_ToEulerRad(ref rotation, out value);
			return value;
		}

		private static QuaternionD Internal_FromEulerRad(Vector3D euler)
		{
			QuaternionD value;
			INTERNAL_CALL_FromEulerRad(ref euler, out value);
			return value;
		}

		private static void Internal_ToAxisAngleRad(QuaternionD q, out Vector3D axis, out double angle)
		{
			INTERNAL_CALL_ToAxisAngleRad(ref q, out axis, out angle);
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() >> 2) ^ (W.GetHashCode() >> 1);
		}

		public override bool Equals(object other)
		{
			if (!(other is QuaternionD))
			{
				return false;
			}

			QuaternionD quaternionD = (QuaternionD)other;
			if (X.Equals(quaternionD.X) && Y.Equals(quaternionD.Y) && Z.Equals(quaternionD.Z))
			{
				return W.Equals(quaternionD.W);
			}

			return false;
		}

		private static void INTERNAL_CALL_AngleAxis(double angle, ref Vector3D axis, out QuaternionD value)
		{
			value = Identity;
			if (axis.SqrMagnitude > 1E-06)
			{
				double num = System.Math.Sin(angle * (System.Math.PI / 180.0) * 0.5);
				double new_w = System.Math.Cos(angle * (System.Math.PI / 180.0) * 0.5);
				value.Set(axis.X * num, axis.Y * num, axis.Z * num, new_w);
			}
		}

		private static void INTERNAL_CALL_ToAxisAngleRad(ref QuaternionD q, out Vector3D axis, out double angle)
		{
			axis = Vector3D.Zero;
			angle = 0.0;
			double num = q.X * q.X + q.Y * q.Y + q.Z * q.Z;
			if (num > 1E-06)
			{
				angle = 2.0 * System.Math.Acos(q.W);
				axis = new Vector3D(q.X, q.Y, q.Z) / num;
			}
			else
			{
				angle = 0.0;
				axis = new Vector3D(1.0, 0.0, 0.0);
			}
		}

		private static void INTERNAL_CALL_FromEulerRad(ref Vector3D euler, out QuaternionD value)
		{
			double num = euler.X * 0.5;
			double num2 = euler.Y * 0.5;
			double num3 = euler.Z * 0.5;
			double w = System.Math.Cos(num);
			double x = System.Math.Sin(num);
			double w2 = System.Math.Cos(num2);
			double y = System.Math.Sin(num2);
			double w3 = System.Math.Cos(num3);
			double z = System.Math.Sin(num3);
			QuaternionD[] array = new QuaternionD[3]
			{
				new QuaternionD(x, 0.0, 0.0, w),
				new QuaternionD(0.0, y, 0.0, w2),
				new QuaternionD(0.0, 0.0, z, w3)
			};
			value = array[2] * array[0] * array[1];
		}

		private static void INTERNAL_CALL_ToEulerRad(ref QuaternionD rotation, out Vector3D value)
		{
			double[,] array = new double[3, 3]
			{
				{
					1.0 - (2.0 * rotation.Y * rotation.Y + 2.0 * rotation.Z * rotation.Z),
					2.0 * rotation.Y * rotation.X - 2.0 * rotation.Z * rotation.W,
					2.0 * rotation.Z * rotation.X + 2.0 * rotation.Y * rotation.W
				},
				{
					2.0 * rotation.Y * rotation.X + 2.0 * rotation.Z * rotation.W,
					1.0 - (2.0 * rotation.X * rotation.X + 2.0 * rotation.Z * rotation.Z),
					2.0 * rotation.Z * rotation.Y - 2.0 * rotation.X * rotation.W
				},
				{
					2.0 * rotation.Z * rotation.X - 2.0 * rotation.Y * rotation.W,
					2.0 * rotation.Z * rotation.Y + 2.0 * rotation.X * rotation.W,
					1.0 - (2.0 * rotation.X * rotation.X + 2.0 * rotation.Y * rotation.Y)
				}
			};
			value = Vector3D.Zero;
			double num = 0.0 - System.Math.Asin(array[1, 2]);
			if (num >= System.Math.PI / 2.0)
			{
				value.Set(System.Math.PI / 2.0, System.Math.Atan2(array[0, 1], array[0, 0]), 0.0);
			}
			else if (num <= -System.Math.PI / 2.0)
			{
				value.Set(-System.Math.PI / 2.0, System.Math.Atan2(0.0 - array[0, 1], array[0, 0]), 0.0);
			}
			else
			{
				value.Set(num, System.Math.Atan2(array[0, 2], array[2, 2]),
					System.Math.Atan2(array[1, 0], array[1, 1]));
			}
		}

		private static void INTERNAL_CALL_Inverse(ref QuaternionD rotation, out QuaternionD value)
		{
			value = rotation;
			double num = rotation.X * rotation.X + rotation.Y * rotation.Y + rotation.Z * rotation.Z +
			             rotation.W * rotation.W;
			if (num > 1E-06)
			{
				double num2 = 1.0 / num;
				value.X = (0.0 - rotation.X) * num2;
				value.Y = (0.0 - rotation.Y) * num2;
				value.Z = (0.0 - rotation.Z) * num2;
				value.W = rotation.W * num2;
			}
		}

		private static void INTERNAL_CALL_LookRotation(ref Vector3D forward, ref Vector3D up, out QuaternionD value)
		{
			forward = Vector3D.Normalize(forward);
			Vector3D rhs = Vector3D.Normalize(Vector3D.Cross(up, forward));
			up = Vector3D.Cross(forward, rhs);
			double x = rhs.X;
			double y = rhs.Y;
			double z = rhs.Z;
			double x2 = up.X;
			double y2 = up.Y;
			double z2 = up.Z;
			double x3 = forward.X;
			double y3 = forward.Y;
			double z3 = forward.Z;
			double num = x + y2 + z3;
			if (num > 0.0)
			{
				double num2 = System.Math.Sqrt(num + 1.0);
				value.W = num2 * 0.5;
				num2 = 0.5 / num2;
				value.X = (z2 - y3) * num2;
				value.Y = (x3 - z) * num2;
				value.Z = (y - x2) * num2;
			}
			else if (x >= y2 && x >= z3)
			{
				double num3 = System.Math.Sqrt(1.0 + x - y2 - z3);
				double num4 = 0.5 / num3;
				value.X = 0.5 * num3;
				value.Y = (y + x2) * num4;
				value.Z = (z + x3) * num4;
				value.W = (z2 - y3) * num4;
			}
			else if (y2 > z3)
			{
				double num5 = System.Math.Sqrt(1.0 + y2 - x - z3);
				double num6 = 0.5 / num5;
				value.X = (x2 + y) * num6;
				value.Y = 0.5 * num5;
				value.Z = (y3 + z2) * num6;
				value.W = (x3 - z) * num6;
			}
			else
			{
				double num7 = System.Math.Sqrt(1.0 + z3 - x - y2);
				double num8 = 0.5 / num7;
				value.X = (x3 + z) * num8;
				value.Y = (y3 + z2) * num8;
				value.Z = 0.5 * num7;
				value.W = (y - x2) * num8;
			}
		}

		private static void INTERNAL_CALL_FromToRotation(ref Vector3D fromDirection, ref Vector3D toDirection,
			out QuaternionD value)
		{
			value = RotateTowards(LookRotation(fromDirection), LookRotation(toDirection), double.MaxValue);
		}

		private static void INTERNAL_CALL_Slerp(ref QuaternionD from, ref QuaternionD to, double t,
			out QuaternionD value)
		{
			INTERNAL_CALL_SlerpUnclamped(ref from, ref to, MathHelper.Clamp(t, 0.0, 1.0), out value);
		}

		private static void INTERNAL_CALL_SlerpUnclamped(ref QuaternionD from, ref QuaternionD to, double t,
			out QuaternionD value)
		{
			double num = from.X * to.X + from.Y * to.Y + from.Z * to.Z + from.W * to.W;
			bool flag = false;
			if (num < 0.0)
			{
				flag = true;
				num = 0.0 - num;
			}

			double num2;
			double num3;
			if (num > 0.999999)
			{
				num2 = 1.0 - t;
				num3 = ((!flag) ? t : (0.0 - t));
			}
			else
			{
				double num4 = System.Math.Acos(num);
				double num5 = 1.0 / System.Math.Sin(num4);
				num2 = System.Math.Sin((1.0 - t) * num4) * num5;
				num3 = ((!flag) ? (System.Math.Sin(t * num4) * num5) : ((0.0 - System.Math.Sin(t * num4)) * num5));
			}

			value.X = num2 * from.X + num3 * to.X;
			value.Y = num2 * from.Y + num3 * to.Y;
			value.Z = num2 * from.Z + num3 * to.Z;
			value.W = num2 * from.W + num3 * to.W;
		}

		private static void INTERNAL_CALL_Lerp(ref QuaternionD from, ref QuaternionD to, double t,
			out QuaternionD value)
		{
			INTERNAL_CALL_LerpUnclamped(ref from, ref to, MathHelper.Clamp(t, 0.0, 1.0), out value);
		}

		private static void INTERNAL_CALL_LerpUnclamped(ref QuaternionD from, ref QuaternionD to, double t,
			out QuaternionD value)
		{
			double num = 1.0 - t;
			double num2 = from.X * to.X + from.Y * to.Y + from.Z * to.Z + from.W * to.W;
			if (num2 >= 0.0)
			{
				value.X = num * from.X + t * to.X;
				value.Y = num * from.Y + t * to.Y;
				value.Z = num * from.Z + t * to.Z;
				value.W = num * from.W + t * to.W;
			}
			else
			{
				value.X = num * from.X - t * to.X;
				value.Y = num * from.Y - t * to.Y;
				value.Z = num * from.Z - t * to.Z;
				value.W = num * from.W - t * to.W;
			}

			double num3 = 1.0 /
			              System.Math.Sqrt(
				              value.X * value.X + value.Y * value.Y + value.Z * value.Z + value.W * value.W);
			value.X *= num3;
			value.Y *= num3;
			value.Z *= num3;
			value.W *= num3;
		}
	}
}
