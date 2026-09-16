using System;
using NUnit.Framework;
using UnityEngine;
using ZeroGravity;
using ZeroGravity.Math;

namespace OpenHellion.Tests
{
	[TestFixture]
	public class Vector3DTests
	{
		[Test]
		public void TwoArgConstructor_SetsZToZero()
		{
			Vector3D twoArg = new Vector3D(4.0, 5.0);
			Assert.That(twoArg.X, Is.EqualTo(4.0));
			Assert.That(twoArg.Y, Is.EqualTo(5.0));
			Assert.That(twoArg.Z, Is.EqualTo(0.0));
		}

		[Test]
		public void StaticDirections_HaveExpectedValues()
		{
			MathAssert.AreEqual(new Vector3D(0.0, 0.0, 0.0), Vector3D.Zero);
			MathAssert.AreEqual(new Vector3D(1.0, 1.0, 1.0), Vector3D.One);
			MathAssert.AreEqual(new Vector3D(0.0, 1.0, 0.0), Vector3D.Up);
			MathAssert.AreEqual(new Vector3D(0.0, -1.0, 0.0), Vector3D.Down);
			MathAssert.AreEqual(new Vector3D(1.0, 0.0, 0.0), Vector3D.Right);
			MathAssert.AreEqual(new Vector3D(-1.0, 0.0, 0.0), Vector3D.Left);
			MathAssert.AreEqual(new Vector3D(0.0, 0.0, 1.0), Vector3D.Forward);
			MathAssert.AreEqual(new Vector3D(0.0, 0.0, -1.0), Vector3D.Back);
		}

		[Test]
		public void Magnitude_And_SqrMagnitude()
		{
			Vector3D v = new Vector3D(3.0, 4.0, 0.0);
			Assert.That(v.Magnitude, Is.EqualTo(5.0).Within(MathAssert.Tol));
			Assert.That(v.SqrMagnitude, Is.EqualTo(25.0).Within(MathAssert.Tol));

			Vector3D w = new Vector3D(1.0, -2.0, 2.0);
			Assert.That(w.Magnitude, Is.EqualTo(3.0).Within(MathAssert.Tol));
			Assert.That(w.SqrMagnitude, Is.EqualTo(9.0).Within(MathAssert.Tol));
		}

		[Test]
		public void Arithmetic_Operators()
		{
			Vector3D a = new Vector3D(1.0, 2.0, 3.0);
			Vector3D b = new Vector3D(4.0, 5.0, 6.0);

			MathAssert.AreEqual(new Vector3D(5.0, 7.0, 9.0), a + b);
			MathAssert.AreEqual(new Vector3D(-3.0, -3.0, -3.0), a - b);
			MathAssert.AreEqual(new Vector3D(-1.0, -2.0, -3.0), -a);
			MathAssert.AreEqual(new Vector3D(2.0, 4.0, 6.0), a * 2.0);
			MathAssert.AreEqual(new Vector3D(2.0, 4.0, 6.0), 2.0 * a);
			MathAssert.AreEqual(new Vector3D(0.5, 1.0, 1.5), a / 2.0);
		}

		[Test]
		public void EqualityOperator_UsesSquaredDistanceThreshold()
		{
			Assert.That(new Vector3D(0.0, 0.0, 0.0) == new Vector3D(1e-6, 0.0, 0.0), Is.True);
			Assert.That(new Vector3D(0.0, 0.0, 0.0) == new Vector3D(1e-5, 0.0, 0.0), Is.False);
			Assert.That(new Vector3D(0.0, 0.0, 0.0) != new Vector3D(1e-5, 0.0, 0.0), Is.True);
		}

		[Test]
		public void Indexer_ReadsAndWrites()
		{
			Vector3D v = new Vector3D(7.0, 8.0, 9.0);
			Assert.That(v[0], Is.EqualTo(7.0));
			Assert.That(v[1], Is.EqualTo(8.0));
			Assert.That(v[2], Is.EqualTo(9.0));

			v[1] = 42.0;
			Assert.That(v.Y, Is.EqualTo(42.0));
			Assert.That(v.X, Is.EqualTo(7.0));
			Assert.That(v.Z, Is.EqualTo(9.0));
		}

		[Test]
		public void Indexer_OutOfRange_Throws()
		{
			Vector3D v = Vector3D.One;
			Assert.Throws<IndexOutOfRangeException>(() => { double _ = v[3]; });
			Assert.Throws<IndexOutOfRangeException>(() => { v[3] = 1.0; });
		}

		[Test]
		public void Dot_And_Cross()
		{
			Assert.That(Vector3D.Dot(new Vector3D(1.0, 2.0, 3.0), new Vector3D(4.0, 5.0, 6.0)),
				Is.EqualTo(32.0).Within(MathAssert.Tol));

			MathAssert.AreEqual(Vector3D.Forward, Vector3D.Cross(Vector3D.Right, Vector3D.Up));
			MathAssert.AreEqual(Vector3D.Back, Vector3D.Cross(Vector3D.Up, Vector3D.Right));
			MathAssert.AreEqual(new Vector3D(-3.0, 6.0, -3.0),
				Vector3D.Cross(new Vector3D(1.0, 2.0, 3.0), new Vector3D(4.0, 5.0, 6.0)));
		}

		[Test]
		public void Distance()
		{
			Assert.That(Vector3D.Distance(Vector3D.Zero, new Vector3D(3.0, 4.0, 0.0)),
				Is.EqualTo(5.0).Within(MathAssert.Tol));
			Assert.That(Vector3D.Distance(new Vector3D(1.0, 1.0, 1.0), new Vector3D(2.0, -1.0, 3.0)),
				Is.EqualTo(3.0).Within(MathAssert.Tol));
		}

		[Test]
		public void Angle()
		{
			Assert.That(Vector3D.Angle(Vector3D.Right, Vector3D.Up), Is.EqualTo(90.0).Within(1e-9));
			Assert.That(Vector3D.Angle(Vector3D.Right, Vector3D.Right), Is.EqualTo(0.0).Within(1e-9));
			Assert.That(Vector3D.Angle(Vector3D.Right, Vector3D.Left), Is.EqualTo(180.0).Within(1e-9));
			Assert.That(Vector3D.Angle(new Vector3D(3.0, 3.0, 0.0), Vector3D.Right), Is.EqualTo(45.0).Within(1e-9));
		}

		[Test]
		public void Normalize_ReturnsUnitVector()
		{
			MathAssert.AreEqual(Vector3D.Up, new Vector3D(0.0, 3.0, 0.0).Normalized);
			MathAssert.AreEqual(Vector3D.Up, Vector3D.Normalize(new Vector3D(0.0, 3.0, 0.0)));
			MathAssert.AreEqual(new Vector3D(1.0 / 3.0, -2.0 / 3.0, 2.0 / 3.0), new Vector3D(1.0, -2.0, 2.0).Normalized);

			Vector3D v = new Vector3D(0.0, 3.0, 0.0);
			v.Normalize();
			MathAssert.AreEqual(Vector3D.Up, v);
		}

		[Test]
		public void Normalize_BelowEpsilon_ReturnsZero()
		{
			MathAssert.AreEqual(Vector3D.Zero, new Vector3D(1e-7, 0.0, 0.0).Normalized);

			Vector3D v = new Vector3D(1e-7, 0.0, 0.0);
			v.Normalize();
			MathAssert.AreEqual(Vector3D.Zero, v);
		}

		[Test]
		public void ClampMagnitude()
		{
			MathAssert.AreEqual(new Vector3D(5.0, 0.0, 0.0), Vector3D.ClampMagnitude(new Vector3D(10.0, 0.0, 0.0), 5.0));
			MathAssert.AreEqual(new Vector3D(3.0, 0.0, 0.0), Vector3D.ClampMagnitude(new Vector3D(3.0, 0.0, 0.0), 5.0));
			MathAssert.AreEqual(new Vector3D(0.6, 0.0, -0.8), Vector3D.ClampMagnitude(new Vector3D(6.0, 0.0, -8.0), 1.0));
		}

		[Test]
		public void Lerp_IsClamped()
		{
			MathAssert.AreEqual(new Vector3D(5.0, 0.0, 0.0), Vector3D.Lerp(Vector3D.Zero, new Vector3D(10.0, 0.0, 0.0), 0.5));
			MathAssert.AreEqual(new Vector3D(10.0, 0.0, 0.0), Vector3D.Lerp(Vector3D.Zero, new Vector3D(10.0, 0.0, 0.0), 2.0));
			MathAssert.AreEqual(Vector3D.Zero, Vector3D.Lerp(Vector3D.Zero, new Vector3D(10.0, 0.0, 0.0), -1.0));
			MathAssert.AreEqual(new Vector3D(1.5, 2.5, -1.5),
				Vector3D.Lerp(new Vector3D(1.0, 2.0, -3.0), new Vector3D(3.0, 4.0, 3.0), 0.25));
		}

		[Test]
		public void LerpUnclamped_Extrapolates()
		{
			MathAssert.AreEqual(new Vector3D(20.0, 0.0, 0.0),
				Vector3D.LerpUnclamped(Vector3D.Zero, new Vector3D(10.0, 0.0, 0.0), 2.0));
			MathAssert.AreEqual(new Vector3D(-10.0, 0.0, 0.0),
				Vector3D.LerpUnclamped(Vector3D.Zero, new Vector3D(10.0, 0.0, 0.0), -1.0));
		}

		[Test]
		public void MinMax()
		{
			MathAssert.AreEqual(new Vector3D(1.0, 2.0, 3.0),
				Vector3D.Min(new Vector3D(1.0, 5.0, 3.0), new Vector3D(4.0, 2.0, 9.0)));
			MathAssert.AreEqual(new Vector3D(4.0, 5.0, 9.0),
				Vector3D.Max(new Vector3D(1.0, 5.0, 3.0), new Vector3D(4.0, 2.0, 9.0)));
		}

		[Test]
		public void MoveTowards()
		{
			MathAssert.AreEqual(new Vector3D(3.0, 0.0, 0.0),
				Vector3D.MoveTowards(Vector3D.Zero, new Vector3D(10.0, 0.0, 0.0), 3.0));
			MathAssert.AreEqual(new Vector3D(10.0, 0.0, 0.0),
				Vector3D.MoveTowards(Vector3D.Zero, new Vector3D(10.0, 0.0, 0.0), 999.0));
			MathAssert.AreEqual(Vector3D.One, Vector3D.MoveTowards(Vector3D.One, Vector3D.One, 5.0));
			MathAssert.AreEqual(new Vector3D(1.6, 2.0, 1.8),
				Vector3D.MoveTowards(new Vector3D(1.0, 2.0, 1.0), new Vector3D(4.0, 2.0, 5.0), 1.0));
		}

		[Test]
		public void Project_And_ProjectOnPlane()
		{
			MathAssert.AreEqual(new Vector3D(1.0, 0.0, 0.0), Vector3D.Project(new Vector3D(1.0, 1.0, 0.0), Vector3D.Right));
			MathAssert.AreEqual(Vector3D.Zero, Vector3D.Project(new Vector3D(1.0, 1.0, 0.0), Vector3D.Zero));
			MathAssert.AreEqual(new Vector3D(3.0, 0.0, 0.0), Vector3D.Project(new Vector3D(3.0, 4.0, 5.0), new Vector3D(2.0, 0.0, 0.0)));
			MathAssert.AreEqual(new Vector3D(2.5, 2.5, 0.0), Vector3D.Project(new Vector3D(1.0, 4.0, 7.0), new Vector3D(3.0, 3.0, 0.0)));

			MathAssert.AreEqual(new Vector3D(1.0, 0.0, 0.0),
				Vector3D.ProjectOnPlane(new Vector3D(1.0, 1.0, 0.0), Vector3D.Up));
			MathAssert.AreEqual(new Vector3D(0.0, 4.0, 5.0),
				Vector3D.ProjectOnPlane(new Vector3D(3.0, 4.0, 5.0), new Vector3D(2.0, 0.0, 0.0)));
		}

		[Test]
		public void Reflect()
		{
			MathAssert.AreEqual(new Vector3D(1.0, 1.0, 0.0),
				Vector3D.Reflect(new Vector3D(1.0, -1.0, 0.0), Vector3D.Up));
			MathAssert.AreEqual(new Vector3D(0.0, -1.0, 0.0),
				Vector3D.Reflect(Vector3D.Right, new Vector3D(1.0, 1.0, 0.0).Normalized));
		}

		[Test]
		public void Scale()
		{
			MathAssert.AreEqual(new Vector3D(10.0, 18.0, 28.0),
				Vector3D.Scale(new Vector3D(2.0, 3.0, 4.0), new Vector3D(5.0, 6.0, 7.0)));

			Vector3D v = new Vector3D(2.0, 3.0, 4.0);
			v.Scale(new Vector3D(5.0, 6.0, 7.0));
			MathAssert.AreEqual(new Vector3D(10.0, 18.0, 28.0), v);
		}

		[Test]
		public void OrthoNormalize_ThreeArg_ProducesOrthonormalBasis()
		{
			Vector3D a = new Vector3D(2.0, 1.0, -1.0);
			Vector3D b = new Vector3D(0.5, 3.0, 1.0);
			Vector3D c = new Vector3D(-1.0, 0.5, 4.0);
			Vector3D originalA = a;
			Vector3D originalB = b;

			Vector3D.OrthoNormalize(ref a, ref b, ref c);

			MathAssert.AreEqual(originalA.Normalized, a);
			Assert.That(a.Magnitude, Is.EqualTo(1.0).Within(MathAssert.Tol));
			Assert.That(b.Magnitude, Is.EqualTo(1.0).Within(MathAssert.Tol));
			Assert.That(c.Magnitude, Is.EqualTo(1.0).Within(MathAssert.Tol));
			Assert.That(Vector3D.Dot(a, b), Is.EqualTo(0.0).Within(MathAssert.Tol));
			Assert.That(Vector3D.Dot(a, c), Is.EqualTo(0.0).Within(MathAssert.Tol));
			Assert.That(Vector3D.Dot(b, c), Is.EqualTo(0.0).Within(MathAssert.Tol));
			Assert.That(Vector3D.Dot(Vector3D.Cross(originalA, originalB), b), Is.EqualTo(0.0).Within(MathAssert.Tol));
			Assert.That(Vector3D.Dot(originalB, b), Is.GreaterThan(0.0));
		}

		[Test]
		public void ToVector3_And_ToVector3D_PreserveComponentOrder()
		{
			Vector3 unity = new Vector3D(1.5, -2.25, 3.125).ToVector3();
			Assert.That(unity.x, Is.EqualTo(1.5f));
			Assert.That(unity.y, Is.EqualTo(-2.25f));
			Assert.That(unity.z, Is.EqualTo(3.125f));

			MathAssert.AreEqual(new Vector3D(1.5, -2.25, 3.125), new Vector3(1.5f, -2.25f, 3.125f).ToVector3D());
			MathAssert.AreEqual(new Vector3D(1.5, -2.25, 3.125), new Vector3D(new Vector3(1.5f, -2.25f, 3.125f)));
		}

		[Test]
		public void ArrayToVector3D_PreservesOrder_AndFallsBackToZeroOnWrongLength()
		{
			MathAssert.AreEqual(new Vector3D(1.0, 2.0, 3.0), new[] { 1.0, 2.0, 3.0 }.ToVector3D());
			MathAssert.AreEqual(new Vector3D(1.0, 2.0, 3.0), new[] { 1f, 2f, 3f }.ToVector3D());
			MathAssert.AreEqual(Vector3D.Zero, new[] { 1.0, 2.0 }.ToVector3D());
			MathAssert.AreEqual(Vector3D.Zero, new[] { 1f, 2f, 3f, 4f }.ToVector3D());
		}
	}
}
