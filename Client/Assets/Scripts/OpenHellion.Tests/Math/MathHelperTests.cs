using NUnit.Framework;
using UnityEngine;
using ZeroGravity.Math;

namespace OpenHellion.Tests
{
	[TestFixture]
	public class MathHelperTests
	{
		[Test]
		public void Clamp_Int()
		{
			Assert.That(MathHelper.Clamp(5, 0, 10), Is.EqualTo(5));
			Assert.That(MathHelper.Clamp(-3, 0, 10), Is.EqualTo(0));
			Assert.That(MathHelper.Clamp(42, 0, 10), Is.EqualTo(10));
		}

		[Test]
		public void Clamp_Float()
		{
			Assert.That(MathHelper.Clamp(0.5f, 0f, 1f), Is.EqualTo(0.5f));
			Assert.That(MathHelper.Clamp(-1f, 0f, 1f), Is.EqualTo(0f));
			Assert.That(MathHelper.Clamp(2f, 0f, 1f), Is.EqualTo(1f));
		}

		[Test]
		public void Clamp_Double()
		{
			Assert.That(MathHelper.Clamp(0.5, 0.0, 1.0), Is.EqualTo(0.5));
			Assert.That(MathHelper.Clamp(-1.0, 0.0, 1.0), Is.EqualTo(0.0));
			Assert.That(MathHelper.Clamp(2.0, 0.0, 1.0), Is.EqualTo(1.0));
		}

		[Test]
		public void Lerp_IsClamped()
		{
			Assert.That(MathHelper.Lerp(0f, 10f, 0.5f), Is.EqualTo(5f).Within(1e-5f));
			Assert.That(MathHelper.Lerp(0f, 10f, 2f), Is.EqualTo(10f));
			Assert.That(MathHelper.Lerp(0f, 10f, -1f), Is.EqualTo(0f));

			Assert.That(MathHelper.Lerp(0.0, 10.0, 0.25), Is.EqualTo(2.5).Within(1e-12));
			Assert.That(MathHelper.Lerp(4.0, -6.0, 0.3), Is.EqualTo(1.0).Within(1e-12));
		}

		[Test]
		public void LerpValue_SnapsWithinEpsilon()
		{
			Assert.That(MathHelper.LerpValue(0f, 10f, 0.5f), Is.EqualTo(5f).Within(1e-4f));
			Assert.That(MathHelper.LerpValue(10f, 0f, 0.5f), Is.EqualTo(5f).Within(1e-4f));
			Assert.That(MathHelper.LerpValue(9.999f, 10f, 0.5f), Is.EqualTo(10f), "within epsilon -> snap to target");
			Assert.That(MathHelper.LerpValue(0.005f, 0f, 0.1f), Is.EqualTo(0f), "within epsilon from above -> snap to target");
			Assert.That(MathHelper.LerpValue(7f, 7f, 0.5f), Is.EqualTo(7f), "from == to -> unchanged");
			Assert.That(MathHelper.LerpValue(0f, 10f, 3f), Is.EqualTo(10f));
		}

		[Test]
		public void SmoothStep_FollowsHermiteCurve_AndIsClamped()
		{
			Assert.That(MathHelper.SmoothStep(0.0, 10.0, 0.0), Is.EqualTo(0.0).Within(1e-12));
			Assert.That(MathHelper.SmoothStep(0.0, 10.0, 0.25), Is.EqualTo(1.5625).Within(1e-12));
			Assert.That(MathHelper.SmoothStep(0.0, 10.0, 0.5), Is.EqualTo(5.0).Within(1e-12));
			Assert.That(MathHelper.SmoothStep(0.0, 10.0, 1.0), Is.EqualTo(10.0).Within(1e-12));
			Assert.That(MathHelper.SmoothStep(0.0, 10.0, 2.0), Is.EqualTo(10.0).Within(1e-12));
			Assert.That(MathHelper.SmoothStep(0.0, 10.0, -1.0), Is.EqualTo(0.0).Within(1e-12));
		}

		[Test]
		public void Hermite_InterpolatesWithTangents()
		{
			Assert.That(MathHelper.Hermite(2.0, 3.0, 8.0, -1.0, 0.0), Is.EqualTo(2.0).Within(1e-12));
			Assert.That(MathHelper.Hermite(2.0, 3.0, 8.0, -1.0, 0.25), Is.EqualTo(3.40625).Within(1e-12));
			Assert.That(MathHelper.Hermite(2.0, 3.0, 8.0, -1.0, 0.5), Is.EqualTo(5.5).Within(1e-12));
			Assert.That(MathHelper.Hermite(2.0, 3.0, 8.0, -1.0, 1.0 - 1e-9), Is.EqualTo(8.0).Within(1e-6));
			Assert.That(MathHelper.Hermite(2.0, 3.0, 8.0, -1.0, 1.0), Is.EqualTo(8.0).Within(1e-12));
		}

		[Test]
		public void ProportionalValue_Scalar()
		{
			Assert.That(MathHelper.ProportionalValue(5f, 0f, 10f, 0f, 100f), Is.EqualTo(50f).Within(1e-3f));
			Assert.That(MathHelper.ProportionalValue(0f, 0f, 10f, 20f, 120f), Is.EqualTo(20f).Within(1e-3f));
			Assert.That(MathHelper.ProportionalValue(2.5f, 0f, 10f, 100f, 0f), Is.EqualTo(75f).Within(1e-3f));
			Assert.That(MathHelper.ProportionalValue(15f, 0f, 10f, 0f, 100f), Is.EqualTo(150f).Within(1e-3f));
			Assert.That(MathHelper.ProportionalValue(-3f, -5f, 5f, 1f, 2f), Is.EqualTo(1.2f).Within(1e-5f));
		}

		[Test]
		public void ProportionalValue_Vector()
		{
			Vector3D result = MathHelper.ProportionalValue(
				new Vector3D(5.0, 0.0, 0.0),
				new Vector3D(0.0, 0.0, 0.0),
				new Vector3D(10.0, 0.0, 0.0),
				new Vector3D(0.0, 0.0, 0.0),
				new Vector3D(100.0, 0.0, 0.0));
			MathAssert.AreEqual(new Vector3D(50.0, 0.0, 0.0), result, 1e-9);

			Vector3D offAxis = MathHelper.ProportionalValue(
				new Vector3D(1.0, 1.0, 1.0),
				new Vector3D(0.0, 0.0, 0.0),
				new Vector3D(2.0, 2.0, 2.0),
				new Vector3D(10.0, -10.0, 0.0),
				new Vector3D(20.0, 10.0, 4.0));
			MathAssert.AreEqual(new Vector3D(15.0, 0.0, 2.0), offAxis, 1e-9);
		}

		[Test]
		public void SetEpsilonZero()
		{
			Assert.That(MathHelper.SetEpsilonZero(0f), Is.EqualTo(0f));
			Assert.That(MathHelper.SetEpsilonZero(1.5f), Is.EqualTo(1.5f));
			Assert.That(MathHelper.SetEpsilonZero(-1.5f), Is.EqualTo(-1.5f));
			Assert.That(MathHelper.SetEpsilonZero(0.3f, 0.5f), Is.EqualTo(0f), "below custom epsilon -> zero");
			Assert.That(MathHelper.SetEpsilonZero(-0.3f, 0.5f), Is.EqualTo(0f), "negative below custom epsilon -> zero");
			Assert.That(MathHelper.SetEpsilonZero(0.7f, 0.5f), Is.EqualTo(0.7f));
		}

		[Test]
		public void LongRandom_CoversHalfOpenRange()
		{
			System.Random random = new System.Random(1234);
			bool[] seen = new bool[10];
			for (int i = 0; i < 2000; i++)
			{
				long value = MathHelper.LongRandom(-5L, 5L, random);
				Assert.That(value, Is.InRange(-5L, 4L));
				seen[value + 5] = true;
			}

			Assert.That(seen, Is.All.True);
		}

		[Test]
		public void RandomRange_WithSeed_ScalesTheSameSampleIntoRange()
		{
			for (int seed = 0; seed < 20; seed++)
			{
				double unit = MathHelper.RandomRange(0.0, 1.0, seed);
				double scaled = MathHelper.RandomRange(10.0, 20.0, seed);

				Assert.That(unit, Is.GreaterThanOrEqualTo(0.0).And.LessThan(1.0));
				Assert.That(scaled, Is.GreaterThanOrEqualTo(10.0).And.LessThan(20.0));
				Assert.That((scaled - 10.0) / 10.0, Is.EqualTo(unit).Within(1e-12));
			}
		}

		[Test]
		public void RandomRange_Unseeded_StaysInRange()
		{
			bool[] seen = new bool[4];
			for (int i = 0; i < 1000; i++)
			{
				Assert.That(MathHelper.RandomRange(-2f, 3f), Is.InRange(-2f, 3f));
				Assert.That(MathHelper.RandomRange(-2.0, 3.0), Is.GreaterThanOrEqualTo(-2.0).And.LessThan(3.0));

				int value = MathHelper.RandomRange(0, 4);
				Assert.That(value, Is.InRange(0, 3));
				seen[value] = true;
			}

			Assert.That(seen, Is.All.True);
		}

		[Test]
		public void AverageMaxValue()
		{
			Assert.That(MathHelper.AverageMaxValue(1f, 1f, 1f, 2f, 2f, 2f), Is.EqualTo(2f).Within(1e-4f));
			Assert.That(MathHelper.AverageMaxValue(2f, 4f, 6f, 4f, 8f, 12f), Is.EqualTo(8f).Within(1e-4f));
			Assert.That(MathHelper.AverageMaxValue(3f, 1f, 0f, 3f, 4f, 10f), Is.EqualTo(3.2f).Within(1e-4f));
		}

		[Test]
		public void Acosh_IsInverseOfCosh()
		{
			Assert.That(MathHelper.Acosh(1.0), Is.EqualTo(0.0).Within(1e-12));
			Assert.That(MathHelper.Acosh(System.Math.Cosh(1.3)), Is.EqualTo(1.3).Within(1e-9));
			Assert.That(MathHelper.Acosh(System.Math.Cosh(5.0)), Is.EqualTo(5.0).Within(1e-9));
		}

		[Test]
		public void RadToDeg_And_DegToRad_AreReciprocal()
		{
			Assert.That(MathHelper.RadToDeg * MathHelper.DegToRad, Is.EqualTo(1.0).Within(1e-15));
			Assert.That(MathHelper.RadToDeg, Is.EqualTo(57.2957795130823).Within(1e-9));
		}

		[Test]
		public void AngleSigned_Vector3D_UsesPlaneNormalForSign()
		{
			Assert.That(MathHelper.AngleSigned(Vector3D.Right, Vector3D.Up, Vector3D.Forward),
				Is.EqualTo(90.0).Within(1e-9));
			Assert.That(MathHelper.AngleSigned(Vector3D.Right, Vector3D.Up, Vector3D.Back),
				Is.EqualTo(-90.0).Within(1e-9));
			Assert.That(MathHelper.AngleSigned(Vector3D.Forward, new Vector3D(-1.0, 0.0, 1.0), Vector3D.Up),
				Is.EqualTo(-45.0).Within(1e-9));
		}

		[Test]
		public void AngleSigned_Vector3_UsesPlaneNormalForSign()
		{
			Assert.That(MathHelper.AngleSigned(Vector3.right, Vector3.up, Vector3.forward), Is.EqualTo(90f).Within(1e-4f));
			Assert.That(MathHelper.AngleSigned(Vector3.right, Vector3.up, Vector3.back), Is.EqualTo(-90f).Within(1e-4f));
			Assert.That(MathHelper.AngleSigned(Vector3.forward, new Vector3(-1f, 0f, 1f), Vector3.up), Is.EqualTo(-45f).Within(1e-4f));
		}

		[Test]
		public void AngleSigned_Vector3_MatchesVector3DOverload()
		{
			System.Random random = new System.Random(99);
			for (int i = 0; i < 50; i++)
			{
				Vector3D a = new Vector3D(random.NextDouble() * 2.0 - 1.0, random.NextDouble() * 2.0 - 1.0, random.NextDouble() * 2.0 - 1.0);
				Vector3D b = new Vector3D(random.NextDouble() * 2.0 - 1.0, random.NextDouble() * 2.0 - 1.0, random.NextDouble() * 2.0 - 1.0);
				Vector3D normal = new Vector3D(random.NextDouble() * 2.0 - 1.0, random.NextDouble() * 2.0 - 1.0, random.NextDouble() * 2.0 - 1.0);

				double expected = MathHelper.AngleSigned(a, b, normal);
				float actual = MathHelper.AngleSigned(
					new Vector3((float)a.X, (float)a.Y, (float)a.Z),
					new Vector3((float)b.X, (float)b.Y, (float)b.Z),
					new Vector3((float)normal.X, (float)normal.Y, (float)normal.Z));

				Assert.That(actual, Is.EqualTo(expected).Within(1e-2), $"sample {i}");
			}
		}

		[Test]
		public void RotateAroundPivot_Vector()
		{
			MathAssert.AreEqual(new Vector3D(3.0, 4.0, 5.0),
				MathHelper.RotateAroundPivot(new Vector3D(3.0, 4.0, 5.0), new Vector3D(1.0, 1.0, 1.0), Vector3D.Zero),
				1e-9);
			MathAssert.AreEqual(new Vector3D(1.0, 1.0, -1.0),
				MathHelper.RotateAroundPivot(new Vector3D(2.0, 1.0, 0.0), new Vector3D(1.0, 1.0, 0.0), new Vector3D(0.0, 90.0, 0.0)),
				1e-9);
			MathAssert.AreEqual(new Vector3D(1.0, 1.0, 1.0),
				MathHelper.RotateAroundPivot(new Vector3D(1.0, 1.0, 1.0), new Vector3D(1.0, 1.0, 1.0), new Vector3D(37.0, -80.0, 12.0)),
				1e-9);
		}
	}
}
