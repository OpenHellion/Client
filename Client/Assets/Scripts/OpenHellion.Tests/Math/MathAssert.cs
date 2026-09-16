using NUnit.Framework;
using ZeroGravity.Math;

namespace OpenHellion.Tests
{
	internal static class MathAssert
	{
		public const double Tol = 1e-9;

		public static void AreEqual(Vector3D expected, Vector3D actual, double tol = Tol)
		{
			Assert.That(actual.X, Is.EqualTo(expected.X).Within(tol), "X");
			Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(tol), "Y");
			Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(tol), "Z");
		}

		public static void AreEqual(Matrix expected, Matrix actual, double tol = Tol)
		{
			Assert.That(actual.rows, Is.EqualTo(expected.rows), "rows");
			Assert.That(actual.cols, Is.EqualTo(expected.cols), "cols");

			for (int r = 0; r < expected.rows; r++)
			{
				for (int c = 0; c < expected.cols; c++)
				{
					Assert.That(actual[r, c], Is.EqualTo(expected[r, c]).Within(tol), $"[{r},{c}]");
				}
			}
		}

		// Compares rotations, treating q and -q as equal (unit quaternion double cover).
		public static void AreSameRotation(QuaternionD expected, QuaternionD actual, double tol = Tol)
		{
			if (QuaternionD.Dot(expected, actual) < 0.0)
			{
				actual = new QuaternionD(-actual.X, -actual.Y, -actual.Z, -actual.W);
			}

			Assert.That(actual.X, Is.EqualTo(expected.X).Within(tol), "X");
			Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(tol), "Y");
			Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(tol), "Z");
			Assert.That(actual.W, Is.EqualTo(expected.W).Within(tol), "W");
		}
	}
}