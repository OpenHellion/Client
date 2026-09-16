using NUnit.Framework;
using ZeroGravity.Math;

namespace OpenHellion.Tests
{
	[TestFixture]
	public class BezierDTests
	{
		private static readonly Vector3D P0 = new Vector3D(0.0, 0.0, 0.0);
		private static readonly Vector3D P1 = new Vector3D(0.0, 1.0, 0.0);
		private static readonly Vector3D P2 = new Vector3D(1.0, 1.0, 0.0);
		private static readonly Vector3D P3 = new Vector3D(1.0, 0.0, 0.0);

		[Test]
		public void FillDataAtPart_StartAndEnd_MatchEndpoints()
		{
			BezierD curve = new BezierD(P0, P1, P2, P3);

			Vector3D point = Vector3D.Zero;
			Vector3D tangent = Vector3D.Zero;

			curve.FillDataAtPart(0.0, ref point, ref tangent);
			MathAssert.AreEqual(P0, point);
			MathAssert.AreEqual(P1 - P0, tangent);

			curve.FillDataAtPart(1.0, ref point, ref tangent);
			MathAssert.AreEqual(P3, point);
			MathAssert.AreEqual(P3 - P2, tangent);
		}

		[Test]
		public void FillDataAtPart_Interior_IsCubicBezier()
		{
			BezierD curve = new BezierD(P0, P1, P2, P3);

			Vector3D point = Vector3D.Zero;
			Vector3D tangent = Vector3D.Zero;

			curve.FillDataAtPart(0.5, ref point, ref tangent);
			MathAssert.AreEqual(new Vector3D(0.5, 0.75, 0.0), point, 1e-12);
			MathAssert.AreEqual(new Vector3D(0.5, 0.0, 0.0), tangent, 1e-12);

			curve.FillDataAtPart(0.25, ref point, ref tangent);
			MathAssert.AreEqual(new Vector3D(0.15625, 0.5625, 0.0), point, 1e-12);
			MathAssert.AreEqual(new Vector3D(0.375, 0.5, 0.0), tangent, 1e-12);
		}

		[Test]
		public void FillDataAtPart_TangentIsOneThirdOfDerivative()
		{
			BezierD curve = new BezierD(new Vector3D(-2.0, 1.0, 4.0), new Vector3D(3.0, 5.0, -1.0), new Vector3D(0.0, -4.0, 2.0), new Vector3D(6.0, 0.0, 1.0));

			const double t = 0.37;
			const double h = 1e-6;
			Vector3D point = Vector3D.Zero;
			Vector3D tangent = Vector3D.Zero;
			Vector3D before = Vector3D.Zero;
			Vector3D after = Vector3D.Zero;
			Vector3D unused = Vector3D.Zero;

			curve.FillDataAtPart(t, ref point, ref tangent);
			curve.FillDataAtPart(t - h, ref before, ref unused);
			curve.FillDataAtPart(t + h, ref after, ref unused);

			MathAssert.AreEqual((after - before) / (2.0 * h) / 3.0, tangent, 1e-6);
		}

		[Test]
		public void SetPoints_ReplacesControlPoints()
		{
			BezierD curve = new BezierD(Vector3D.One, Vector3D.One, Vector3D.One, Vector3D.One);
			curve.SetPoints(P0, P1, P2, P3);

			Vector3D point = Vector3D.Zero;
			Vector3D tangent = Vector3D.Zero;
			curve.FillDataAtPart(0.5, ref point, ref tangent);

			MathAssert.AreEqual(new Vector3D(0.5, 0.75, 0.0), point, 1e-12);
			MathAssert.AreEqual(new Vector3D(0.5, 0.0, 0.0), tangent, 1e-12);
		}
	}
}
