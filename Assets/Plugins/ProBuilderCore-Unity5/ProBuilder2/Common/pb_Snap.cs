using UnityEngine;

namespace ProBuilder2.Common
{
	public static class pb_Snap
	{
		public static Vector3 SnapValue(Vector3 vertex, float snpVal)
		{
			return new Vector3(snpVal * Mathf.Round(vertex.x / snpVal), snpVal * Mathf.Round(vertex.y / snpVal), snpVal * Mathf.Round(vertex.z / snpVal));
		}

		public static float SnapValue(float val, float snpVal)
		{
			return snpVal * Mathf.Round(val / snpVal);
		}

		public static Vector3 SnapValue(Vector3 vertex, Vector3 snap)
		{
			float x = vertex.x;
			float y = vertex.y;
			float z = vertex.z;
			Vector3 result = new Vector3((!(Mathf.Abs(snap.x) < 0.0001f)) ? (snap.x * Mathf.Round(x / snap.x)) : x, (!(Mathf.Abs(snap.y) < 0.0001f)) ? (snap.y * Mathf.Round(y / snap.y)) : y, (!(Mathf.Abs(snap.z) < 0.0001f)) ? (snap.z * Mathf.Round(z / snap.z)) : z);
			return result;
		}
	}
}
