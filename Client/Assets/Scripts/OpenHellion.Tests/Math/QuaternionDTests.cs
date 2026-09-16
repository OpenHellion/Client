using System;
using NUnit.Framework;
using UnityEngine;
using ZeroGravity;
using ZeroGravity.Math;

namespace OpenHellion.Tests
{
	[TestFixture]
	public class QuaternionDTests
	{
		[Test]
		public void Identity_IsZeroZeroZeroOne()
		{
			QuaternionD q = QuaternionD.Identity;
			Assert.That(q.X, Is.EqualTo(0.0));
			Assert.That(q.Y, Is.EqualTo(0.0));
			Assert.That(q.Z, Is.EqualTo(0.0));
			Assert.That(q.W, Is.EqualTo(1.0));
		}

		[Test]
		public void Indexer_ReadsComponents_AndThrowsOutOfRange()
		{
			QuaternionD q = new QuaternionD(1.0, 2.0, 3.0, 4.0);
			Assert.That(q[0], Is.EqualTo(1.0));
			Assert.That(q[1], Is.EqualTo(2.0));
			Assert.That(q[2], Is.EqualTo(3.0));
			Assert.That(q[3], Is.EqualTo(4.0));
			Assert.Throws<IndexOutOfRangeException>(() => { double _ = q[4]; });
		}

		[Test]
		public void Multiply_ByIdentity_IsNoOp()
		{
			QuaternionD q = QuaternionD.Euler(10.0, 20.0, 30.0);
			MathAssert.AreSameRotation(q, QuaternionD.Identity * q);
			MathAssert.AreSameRotation(q, q * QuaternionD.Identity);
			MathAssert.AreEqual(new Vector3D(1.0, 2.0, 3.0), QuaternionD.Identity * new Vector3D(1.0, 2.0, 3.0));
		}

		[Test]
		public void Multiply_AppliesRightOperandFirst()
		{
			QuaternionD a = QuaternionD.AngleAxis(90.0, Vector3D.Right);
			QuaternionD b = QuaternionD.AngleAxis(90.0, Vector3D.Up);
			Vector3D v = new Vector3D(1.0, 2.0, 3.0);

			MathAssert.AreEqual(a * (b * v), (a * b) * v);
			MathAssert.AreEqual(new Vector3D(3.0, 1.0, 2.0), (a * b) * v);
			Assert.That(Math.Abs(QuaternionD.Dot(a * b, b * a)), Is.LessThan(0.9));
		}

		[Test]
		public void RotateVector_90DegreesAboutEachAxis()
		{
			MathAssert.AreEqual(Vector3D.Back, QuaternionD.AngleAxis(90.0, Vector3D.Up) * Vector3D.Right);
			MathAssert.AreEqual(Vector3D.Down, QuaternionD.AngleAxis(90.0, Vector3D.Right) * Vector3D.Forward);
			MathAssert.AreEqual(Vector3D.Up, QuaternionD.AngleAxis(90.0, Vector3D.Forward) * Vector3D.Right);
		}

		[Test]
		public void RotateVector_PreservesLength()
		{
			QuaternionD q = QuaternionD.Euler(17.0, 43.0, -88.0);
			Vector3D v = new Vector3D(1.0, -2.0, 3.0);
			Assert.That((q * v).Magnitude, Is.EqualTo(v.Magnitude).Within(1e-9));
		}

		[Test]
		public void Euler_Zero_IsIdentity()
		{
			MathAssert.AreSameRotation(QuaternionD.Identity, QuaternionD.Euler(0.0, 0.0, 0.0));
		}

		[Test]
		public void Euler_ScalarAndVectorOverloadsAgree()
		{
			MathAssert.AreSameRotation(QuaternionD.Euler(11.0, 22.0, 33.0),
				QuaternionD.Euler(new Vector3D(11.0, 22.0, 33.0)));
		}

		[Test]
		public void Euler_AppliesZThenXThenY()
		{
			QuaternionD expected = QuaternionD.AngleAxis(20.0, Vector3D.Up) * QuaternionD.AngleAxis(10.0, Vector3D.Right) *
				QuaternionD.AngleAxis(30.0, Vector3D.Forward);

			MathAssert.AreSameRotation(expected, QuaternionD.Euler(10.0, 20.0, 30.0));
		}

		[Test]
		public void Euler_MatchesUnity()
		{
			MathAssert.AreSameRotation(Quaternion.Euler(10f, 20f, 30f).ToQuaternionD(), QuaternionD.Euler(10.0, 20.0, 30.0), 1e-5);
			MathAssert.AreSameRotation(Quaternion.Euler(-65f, 140f, -12f).ToQuaternionD(), QuaternionD.Euler(-65.0, 140.0, -12.0), 1e-5);
		}

		[Test]
		public void EulerAngles_RoundTrips()
		{
			MathAssert.AreEqual(new Vector3D(30.0, 0.0, 0.0), QuaternionD.Euler(30.0, 0.0, 0.0).EulerAngles, 1e-6);
			MathAssert.AreEqual(new Vector3D(0.0, 20.0, 0.0), QuaternionD.Euler(0.0, 20.0, 0.0).EulerAngles, 1e-6);
			MathAssert.AreEqual(new Vector3D(0.0, 0.0, 40.0), QuaternionD.Euler(0.0, 0.0, 40.0).EulerAngles, 1e-6);
			MathAssert.AreEqual(new Vector3D(10.0, 20.0, 30.0), QuaternionD.Euler(10.0, 20.0, 30.0).EulerAngles, 1e-6);
			MathAssert.AreEqual(new Vector3D(-45.0, 120.0, -170.0), QuaternionD.Euler(-45.0, 120.0, -170.0).EulerAngles, 1e-6);
		}

		[Test]
		public void EulerAngles_Setter_MatchesEuler()
		{
			QuaternionD q = default;
			q.EulerAngles = new Vector3D(15.0, 35.0, -60.0);
			MathAssert.AreSameRotation(QuaternionD.Euler(15.0, 35.0, -60.0), q);
		}

		[Test]
		public void Inverse_UndoesRotation()
		{
			MathAssert.AreSameRotation(QuaternionD.Identity, QuaternionD.Inverse(QuaternionD.Identity));

			QuaternionD q = QuaternionD.Euler(10.0, 20.0, 30.0);
			MathAssert.AreSameRotation(QuaternionD.Identity, q * QuaternionD.Inverse(q));
			MathAssert.AreSameRotation(QuaternionD.Identity, QuaternionD.Inverse(q) * q);
		}

		[Test]
		public void Inverse_RoundTripsAVector()
		{
			QuaternionD q = QuaternionD.Euler(10.0, 20.0, 30.0);
			QuaternionD qi = QuaternionD.Inverse(q);
			Vector3D v = new Vector3D(3.0, -1.0, 2.0);
			MathAssert.AreEqual(v, qi * (q * v), 1e-9);
		}

		[Test]
		public void Dot_IsCosineOfHalfAngle()
		{
			Assert.That(QuaternionD.Dot(QuaternionD.Identity, QuaternionD.Identity), Is.EqualTo(1.0).Within(1e-12));
			Assert.That(QuaternionD.Dot(QuaternionD.Identity, QuaternionD.AngleAxis(60.0, Vector3D.Up)),
				Is.EqualTo(Math.Cos(30.0 * MathHelper.DegToRad)).Within(1e-12));

			QuaternionD q = QuaternionD.Euler(15.0, 25.0, 35.0);
			Assert.That(QuaternionD.Dot(q, q), Is.EqualTo(1.0).Within(1e-9));
		}

		[Test]
		public void Angle()
		{
			Assert.That(QuaternionD.Angle(QuaternionD.Identity, QuaternionD.Identity), Is.EqualTo(0.0).Within(1e-9));
			Assert.That(QuaternionD.Angle(QuaternionD.Identity, QuaternionD.AngleAxis(90.0, Vector3D.Up)),
				Is.EqualTo(90.0).Within(1e-6));
			Assert.That(QuaternionD.Angle(QuaternionD.AngleAxis(30.0, Vector3D.Up), QuaternionD.AngleAxis(90.0, Vector3D.Up)),
				Is.EqualTo(60.0).Within(1e-6));
		}

		[Test]
		public void AngleAxis_ZeroAngle_OrZeroAxis_IsIdentity()
		{
			MathAssert.AreSameRotation(QuaternionD.Identity, QuaternionD.AngleAxis(0.0, Vector3D.Up));
			MathAssert.AreSameRotation(QuaternionD.Identity, QuaternionD.AngleAxis(90.0, Vector3D.Zero));
		}

		[Test]
		public void AngleAxis_FullTurn_ComposesToIdentity()
		{
			QuaternionD q = QuaternionD.AngleAxis(90.0, Vector3D.Up);
			MathAssert.AreSameRotation(QuaternionD.Identity, q * q * q * q);
		}

		[Test]
		public void AngleAxis_NonUnitAxis_MatchesUnity()
		{
			Vector3D axis = new Vector3D(2.0, -3.0, 6.0);

			MathAssert.AreSameRotation(QuaternionD.AngleAxis(50.0, axis.Normalized), QuaternionD.AngleAxis(50.0, axis));
			MathAssert.AreSameRotation(Quaternion.AngleAxis(50f, axis.ToVector3()).ToQuaternionD(), QuaternionD.AngleAxis(50.0, axis), 1e-5);
			MathAssert.AreSameRotation(Quaternion.AngleAxis(-125f, axis.ToVector3()).ToQuaternionD(), QuaternionD.AngleAxis(-125.0, axis), 1e-5);
			MathAssert.AreEqual(axis, QuaternionD.AngleAxis(50.0, axis) * axis);
		}

		[Test]
		public void ToAngleAxis_RecoversAngleAndUnitAxis()
		{
			Vector3D expectedAxis = new Vector3D(1.0, 2.0, 3.0).Normalized;
			QuaternionD.AngleAxis(60.0, expectedAxis).ToAngleAxis(out double angle, out Vector3D axis);

			Assert.That(angle, Is.EqualTo(60.0).Within(1e-6));
			MathAssert.AreEqual(expectedAxis, axis);
		}

		[Test]
		public void Slerp_IsClamped_AndInterpolatesAlongShortestArc()
		{
			QuaternionD a = QuaternionD.Identity;
			QuaternionD b = QuaternionD.AngleAxis(80.0, Vector3D.Up);
			QuaternionD negatedB = new QuaternionD(-b.X, -b.Y, -b.Z, -b.W);

			MathAssert.AreSameRotation(a, QuaternionD.Slerp(a, b, 0.0));
			MathAssert.AreSameRotation(b, QuaternionD.Slerp(a, b, 1.0));
			MathAssert.AreSameRotation(a, QuaternionD.Slerp(a, b, -1.0));
			MathAssert.AreSameRotation(b, QuaternionD.Slerp(a, b, 2.0));
			MathAssert.AreSameRotation(QuaternionD.AngleAxis(40.0, Vector3D.Up), QuaternionD.Slerp(a, b, 0.5));
			MathAssert.AreSameRotation(QuaternionD.AngleAxis(20.0, Vector3D.Up), QuaternionD.Slerp(a, b, 0.25));
			MathAssert.AreSameRotation(QuaternionD.AngleAxis(40.0, Vector3D.Up), QuaternionD.Slerp(a, negatedB, 0.5));
		}

		[Test]
		public void Lerp_HitsEndpointsAndMidpoint_AndStaysNormalised()
		{
			QuaternionD a = QuaternionD.Identity;
			QuaternionD b = QuaternionD.AngleAxis(40.0, Vector3D.Up);

			MathAssert.AreSameRotation(a, QuaternionD.Lerp(a, b, 0.0));
			MathAssert.AreSameRotation(b, QuaternionD.Lerp(a, b, 1.0));
			MathAssert.AreSameRotation(QuaternionD.AngleAxis(20.0, Vector3D.Up), QuaternionD.Lerp(a, b, 0.5));

			QuaternionD partial = QuaternionD.Lerp(QuaternionD.Euler(10.0, 50.0, -20.0), QuaternionD.Euler(-70.0, 5.0, 90.0), 0.3);
			Assert.That(QuaternionD.Dot(partial, partial), Is.EqualTo(1.0).Within(1e-12));
		}

		[Test]
		public void RotateTowards_StepsByMaxDegrees()
		{
			QuaternionD a = QuaternionD.Identity;
			QuaternionD b = QuaternionD.AngleAxis(90.0, Vector3D.Up);

			MathAssert.AreSameRotation(a, QuaternionD.RotateTowards(a, b, 0.0));
			MathAssert.AreSameRotation(QuaternionD.AngleAxis(30.0, Vector3D.Up), QuaternionD.RotateTowards(a, b, 30.0));
			MathAssert.AreSameRotation(b, QuaternionD.RotateTowards(a, b, 1000.0));
		}

		[Test]
		public void LookRotation_ForwardUp_IsIdentity()
		{
			MathAssert.AreSameRotation(QuaternionD.Identity, QuaternionD.LookRotation(Vector3D.Forward, Vector3D.Up));
			MathAssert.AreSameRotation(QuaternionD.Identity, QuaternionD.LookRotation(Vector3D.Forward));
		}

		[TestCase(0.2, -0.1, 1.0, 0.0, 1.0, 0.0)]
		[TestCase(0.0, 0.0, -1.0, 0.0, -1.0, 0.0)]
		[TestCase(0.0, 0.0, -1.0, 0.0, 1.0, 0.0)]
		[TestCase(0.0, 0.0, 1.0, 0.0, -1.0, 0.0)]
		[TestCase(1.0, 2.0, -3.0, 0.0, 1.0, 0.0)]
		[TestCase(0.0, -1.0, 0.0, 0.0, 0.0, 1.0)]
		public void LookRotation_AlignsForwardAndUp_AndMatchesUnity(double fx, double fy, double fz, double ux, double uy, double uz)
		{
			Vector3D forward = new Vector3D(fx, fy, fz);
			Vector3D up = new Vector3D(ux, uy, uz);

			QuaternionD q = QuaternionD.LookRotation(forward, up);

			Assert.That(QuaternionD.Dot(q, q), Is.EqualTo(1.0).Within(1e-12));
			MathAssert.AreEqual(forward.Normalized, q * Vector3D.Forward);
			Assert.That(Vector3D.Dot(q * Vector3D.Up, forward), Is.EqualTo(0.0).Within(MathAssert.Tol));
			Assert.That(Vector3D.Dot(q * Vector3D.Up, up), Is.GreaterThan(0.0));
			MathAssert.AreSameRotation(Quaternion.LookRotation(forward.ToVector3(), up.ToVector3()).ToQuaternionD(), q, 1e-5);
		}

		[Test]
		public void ToQuaternion_And_ToQuaternionD_PreserveComponentOrder()
		{
			Quaternion unity = new QuaternionD(0.5, -0.25, 0.125, 0.75).ToQuaternion();
			Assert.That(unity.x, Is.EqualTo(0.5f));
			Assert.That(unity.y, Is.EqualTo(-0.25f));
			Assert.That(unity.z, Is.EqualTo(0.125f));
			Assert.That(unity.w, Is.EqualTo(0.75f));

			QuaternionD back = new Quaternion(0.5f, -0.25f, 0.125f, 0.75f).ToQuaternionD();
			Assert.That(back.X, Is.EqualTo(0.5));
			Assert.That(back.Y, Is.EqualTo(-0.25));
			Assert.That(back.Z, Is.EqualTo(0.125));
			Assert.That(back.W, Is.EqualTo(0.75));
		}

		[Test]
		public void ToQuaternion_RotatesVectorsLikeQuaternionD()
		{
			System.Random random = new System.Random(4242);
			for (int i = 0; i < 50; i++)
			{
				Vector3D axis = new Vector3D(random.NextDouble() * 2.0 - 1.0, random.NextDouble() * 2.0 - 1.0, random.NextDouble() * 2.0 - 1.0);
				QuaternionD q = QuaternionD.AngleAxis(random.NextDouble() * 720.0 - 360.0, axis);
				Vector3D v = new Vector3D(random.NextDouble() * 20.0 - 10.0, random.NextDouble() * 20.0 - 10.0, random.NextDouble() * 20.0 - 10.0);

				MathAssert.AreEqual(q * v, (q.ToQuaternion() * v.ToVector3()).ToVector3D(), 1e-4);
			}
		}

		[Test]
		public void ArrayToQuaternionD_PreservesOrder_AndFallsBackToIdentityOnWrongLength()
		{
			MathAssert.AreSameRotation(new QuaternionD(0.1, 0.2, 0.3, 0.4), new[] { 0.1, 0.2, 0.3, 0.4 }.ToQuaternionD());
			MathAssert.AreSameRotation(new QuaternionD(0.5, -0.25, 0.125, 0.75), new[] { 0.5f, -0.25f, 0.125f, 0.75f }.ToQuaternionD());
			MathAssert.AreSameRotation(QuaternionD.Identity, new[] { 0.1, 0.2, 0.3 }.ToQuaternionD());
			MathAssert.AreSameRotation(QuaternionD.Identity, new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f }.ToQuaternionD());
		}
	}
}
