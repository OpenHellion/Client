namespace ZeroGravity.Math
{
	public class BezierD
	{
		public Vector3D P0;

		public Vector3D P1;

		public Vector3D P2;

		public Vector3D P3;

		public double Length;

		public BezierD(Vector3D p0, Vector3D p1, Vector3D p2, Vector3D p3)
		{
			P0 = p0;
			P1 = p1;
			P2 = p2;
			P3 = p3;
		}

		public void SetPoints(Vector3D p0, Vector3D p1, Vector3D p2, Vector3D p3)
		{
			P0 = p0;
			P1 = p1;
			P2 = p2;
			P3 = p3;
		}

		public void FillDataAtPart(double t, ref Vector3D point, ref Vector3D tangent)
		{
			Vector3D a = Vector3D.Lerp(P0, P1, t);
			Vector3D vector3D = Vector3D.Lerp(P1, P2, t);
			Vector3D b = Vector3D.Lerp(P2, P3, t);
			Vector3D vector3D2 = Vector3D.Lerp(a, vector3D, t);
			Vector3D vector3D3 = Vector3D.Lerp(vector3D, b, t);
			point = Vector3D.Lerp(vector3D2, vector3D3, t);
			tangent = vector3D3 - vector3D2;
		}
	}
}
