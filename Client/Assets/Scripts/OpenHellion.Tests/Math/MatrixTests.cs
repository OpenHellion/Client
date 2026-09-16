using System;
using NUnit.Framework;
using ZeroGravity.Math;

namespace OpenHellion.Tests
{
	[TestFixture]
	public class MatrixTests
	{
		private static Matrix Make(int rows, int cols, params double[] values)
		{
			Matrix m = new Matrix(rows, cols);
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < cols; j++)
				{
					m[i, j] = values[i * cols + j];
				}
			}

			return m;
		}

		[Test]
		public void IdentityMatrix_HasUnitDiagonal()
		{
			Matrix id = Matrix.IdentityMatrix(3, 3);
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					Assert.That(id[i, j], Is.EqualTo(i == j ? 1.0 : 0.0));
				}
			}
		}

		[Test]
		public void ZeroMatrix_IsAllZero()
		{
			Matrix z = Matrix.ZeroMatrix(2, 4);
			Assert.That(z.rows, Is.EqualTo(2));
			Assert.That(z.cols, Is.EqualTo(4));
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					Assert.That(z[i, j], Is.EqualTo(0.0));
				}
			}
		}

		[Test]
		public void Duplicate_CopiesEveryEntry_AndIsIndependent()
		{
			Matrix m = Make(2, 3, 1, 2, 3, 4, 5, 6);
			Matrix copy = m.Duplicate();
			m[0, 0] = 99.0;

			MathAssert.AreEqual(Make(2, 3, 1, 2, 3, 4, 5, 6), copy);
		}

		[Test]
		public void Transpose_SwapsIndices()
		{
			Matrix t = Matrix.Transpose(Make(2, 3, 1, 2, 3, 4, 5, 6));
			MathAssert.AreEqual(Make(3, 2, 1, 4, 2, 5, 3, 6), t);
		}

		[Test]
		public void Multiply_ProducesRowByColumnProduct()
		{
			Matrix a = Make(2, 3, 1, 2, 3, 4, 5, 6);
			Matrix b = Make(3, 2, 7, 8, 9, 10, 11, 12);

			MathAssert.AreEqual(Make(2, 2, 58, 64, 139, 154), a * b);
			MathAssert.AreEqual(Make(2, 2, 58, 64, 139, 154), Matrix.StupidMultiply(a, b));
			MathAssert.AreEqual(Make(3, 3, 39, 54, 69, 49, 68, 87, 59, 82, 105), b * a);
		}

		[Test]
		public void Invert_ProducesTheInverse()
		{
			Matrix m = Make(2, 2, 4, 7, 2, 6);
			Matrix inv = m.Invert();

			MathAssert.AreEqual(Make(2, 2, 0.6, -0.7, -0.2, 0.4), inv, 1e-9);
			MathAssert.AreEqual(Matrix.IdentityMatrix(2, 2), m * inv, 1e-9);

			Matrix pivoted = Make(3, 3, 0, 2, 1, 1, 1, 1, 2, 1, 0);
			MathAssert.AreEqual(Matrix.IdentityMatrix(3, 3), pivoted * pivoted.Invert(), 1e-9);
			MathAssert.AreEqual(Matrix.IdentityMatrix(3, 3), Make(3, 3, 0, 2, 1, 1, 1, 1, 2, 1, 0).Invert() * pivoted, 1e-9);
		}

		[Test]
		public void Det()
		{
			Assert.That(Make(2, 2, 4, 7, 2, 6).Det(), Is.EqualTo(10.0).Within(1e-9));
			Assert.That(Make(3, 3, 0, 2, 1, 1, 1, 1, 2, 1, 0).Det(), Is.EqualTo(3.0).Within(1e-9));
			Assert.That(Make(3, 3, 1, 1, 1, 0, 2, 1, 2, 1, 0).Det(), Is.EqualTo(-3.0).Within(1e-9));
			Assert.That(Make(3, 3, 2, 0, 0, 0, 3, 0, 0, 0, -4).Det(), Is.EqualTo(-24.0).Within(1e-9));
		}

		[Test]
		public void SolveWith_SolvesLinearSystem()
		{
			Matrix a = Make(2, 2, 2, 0, 0, 4);
			Matrix b = Make(2, 1, 6, 8);

			Matrix x = a.SolveWith(b);

			MathAssert.AreEqual(Make(2, 1, 3, 2), x, 1e-9);
			MathAssert.AreEqual(b, Matrix.StupidMultiply(a, x), 1e-9);

			Matrix pivoted = Make(3, 3, 0, 2, 1, 1, 1, 1, 2, 1, 0);
			Matrix rhs = Make(3, 1, 7, 6, 4);
			MathAssert.AreEqual(Make(3, 1, 1, 2, 3), pivoted.SolveWith(rhs), 1e-9);
		}

		[Test]
		public void MakeLU_NonSquare_Throws()
		{
			Assert.Catch<Exception>(() => new Matrix(2, 3).MakeLU());
		}

		[Test]
		public void MakeLU_Singular_Throws()
		{
			Matrix singular = Make(3, 3, 1, 1, 1, 1, 1, 2, 1, 1, 3);
			Assert.Catch<Exception>(() => singular.MakeLU());
		}

		[Test]
		public void Parse_ReadsRowsAndColumns()
		{
			Matrix m = Matrix.Parse("1 2\r\n3 4");
			MathAssert.AreEqual(Make(2, 2, 1, 2, 3, 4), m);
		}

		[Test]
		public void Power()
		{
			Matrix m = Make(2, 2, 1, 1, 0, 1);

			MathAssert.AreEqual(Matrix.IdentityMatrix(2, 2), Matrix.Power(m, 0), 1e-9);
			MathAssert.AreEqual(m, Matrix.Power(m, 1), 1e-9);
			MathAssert.AreEqual(Make(2, 2, 1, 2, 0, 1), Matrix.Power(m, 2), 1e-9);
			MathAssert.AreEqual(Make(2, 2, 1, 3, 0, 1), Matrix.Power(m, 3), 1e-9);
			MathAssert.AreEqual(Make(2, 2, 1, 5, 0, 1), Matrix.Power(m, 5), 1e-9);
			MathAssert.AreEqual(Make(2, 2, 1, -1, 0, 1), Matrix.Power(Make(2, 2, 1, 1, 0, 1), -1), 1e-9);
		}
	}
}
