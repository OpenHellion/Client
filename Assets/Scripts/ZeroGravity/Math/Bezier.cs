using UnityEngine;

namespace ZeroGravity.Math
{
	public class Bezier
	{
		public Vector3 P0;

		public Vector3 P1;

		public Vector3 P2;

		public Vector3 P3;

		public double Length;

		public Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
		{
			P0 = p0;
			P1 = p1;
			P2 = p2;
			P3 = p3;
		}

		public void SetPoints(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
		{
			P0 = p0;
			P1 = p1;
			P2 = p2;
			P3 = p3;
		}

		public void FillDataAtPart(float t, ref Vector3 point, ref Vector3 tangent)
		{
			Vector3 a = Vector3.Lerp(P0, P1, t);
			Vector3 vector = Vector3.Lerp(P1, P2, t);
			Vector3 b = Vector3.Lerp(P2, P3, t);
			Vector3 vector2 = Vector3.Lerp(a, vector, t);
			Vector3 vector3 = Vector3.Lerp(vector, b, t);
			point = Vector3.Lerp(vector2, vector3, t);
			tangent = vector3 - vector2;
		}
	}
}
