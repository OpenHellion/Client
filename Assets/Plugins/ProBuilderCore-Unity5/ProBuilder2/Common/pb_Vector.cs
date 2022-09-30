using UnityEngine;

namespace ProBuilder2.Common
{
	public static class pb_Vector
	{
		public const float FLT_COMPARE_RESOLUTION = 1000f;

		private static int HashFloat(float f)
		{
			ulong num = (ulong)(f * 1000f);
			return (int)(num % 2147483647uL);
		}

		public static int GetHashCode(Vector2 v)
		{
			int num = 27;
			num = num * 29 + HashFloat(v.x);
			return num * 29 + HashFloat(v.y);
		}

		public static int GetHashCode(Vector3 v)
		{
			int num = 27;
			num = num * 29 + HashFloat(v.x);
			num = num * 29 + HashFloat(v.y);
			return num * 29 + HashFloat(v.z);
		}

		public static int GetHashCode(Vector4 v)
		{
			int num = 27;
			num = num * 29 + HashFloat(v.x);
			num = num * 29 + HashFloat(v.y);
			num = num * 29 + HashFloat(v.z);
			return num * 29 + HashFloat(v.w);
		}
	}
}
