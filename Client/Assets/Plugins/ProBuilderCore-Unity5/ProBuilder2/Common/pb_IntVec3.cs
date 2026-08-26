using System;
using UnityEngine;

namespace ProBuilder2.Common
{
	public struct pb_IntVec3 : IEquatable<pb_IntVec3>
	{
		public Vector3 vec;

		public const float RESOLUTION = 1000f;

		public float x => vec.x;

		public float y => vec.y;

		public float z => vec.z;

		public pb_IntVec3(Vector3 vector)
		{
			vec = vector;
		}

		public override string ToString()
		{
			return $"({x:F2}, {y:F2}, {z:F2})";
		}

		public static bool operator ==(pb_IntVec3 a, pb_IntVec3 b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(pb_IntVec3 a, pb_IntVec3 b)
		{
			return !(a == b);
		}

		public bool Equals(pb_IntVec3 p)
		{
			return round(x) == round(p.x) && round(y) == round(p.y) && round(z) == round(p.z);
		}

		public bool Equals(Vector3 p)
		{
			return round(x) == round(p.x) && round(y) == round(p.y) && round(z) == round(p.z);
		}

		public override bool Equals(object b)
		{
			return (b is pb_IntVec3 && Equals((pb_IntVec3)b)) || (b is Vector3 && Equals((Vector3)b));
		}

		public override int GetHashCode()
		{
			return pb_Vector.GetHashCode(vec);
		}

		private static int round(float v)
		{
			return Convert.ToInt32(v * 1000f);
		}

		public static implicit operator Vector3(pb_IntVec3 p)
		{
			return p.vec;
		}

		public static implicit operator pb_IntVec3(Vector3 p)
		{
			return new pb_IntVec3(p);
		}
	}
}
