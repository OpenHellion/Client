using System;
using System.Text.RegularExpressions;

namespace ZeroGravity.Math
{
	public class Matrix
	{
		public int rows;

		public int cols;

		public double[,] mat;

		public Matrix L;

		public Matrix U;

		private int[] pi;

		private double detOfP = 1.0;

		public double this[int iRow, int iCol]
		{
			get { return mat[iRow, iCol]; }
			set { mat[iRow, iCol] = value; }
		}

		public Matrix(int iRows, int iCols)
		{
			rows = iRows;
			cols = iCols;
			mat = new double[rows, cols];
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < cols; j++)
				{
					mat[i, j] = 0.0;
				}
			}
		}

		public bool IsSquare()
		{
			return rows == cols;
		}

		public Matrix GetCol(int k)
		{
			Matrix matrix = new Matrix(rows, 1);
			for (int i = 0; i < rows; i++)
			{
				matrix[i, 0] = mat[i, k];
			}

			return matrix;
		}

		public void SetCol(Matrix v, int k)
		{
			for (int i = 0; i < rows; i++)
			{
				mat[i, k] = v[i, 0];
			}
		}

		public void MakeLU()
		{
			if (!IsSquare())
			{
				throw new MException("The matrix is not square!");
			}

			L = IdentityMatrix(rows, cols);
			U = Duplicate();
			pi = new int[rows];
			for (int i = 0; i < rows; i++)
			{
				pi[i] = i;
			}

			double num = 0.0;
			int num2 = 0;
			int num3 = 0;
			for (int j = 0; j < cols - 1; j++)
			{
				num = 0.0;
				for (int k = j; k < rows; k++)
				{
					if (System.Math.Abs(U[k, j]) > num)
					{
						num = System.Math.Abs(U[k, j]);
						num2 = k;
					}
				}

				if (num == 0.0)
				{
					throw new MException("The matrix is singular!");
				}

				num3 = pi[j];
				pi[j] = pi[num2];
				pi[num2] = num3;
				for (int l = 0; l < j; l++)
				{
					double value = L[j, l];
					L[j, l] = L[num2, l];
					L[num2, l] = value;
				}

				if (j != num2)
				{
					detOfP *= -1.0;
				}

				for (int m = 0; m < cols; m++)
				{
					double value = U[j, m];
					U[j, m] = U[num2, m];
					U[num2, m] = value;
				}

				for (int n = j + 1; n < rows; n++)
				{
					L[n, j] = U[n, j] / U[j, j];
					for (int num4 = j; num4 < cols; num4++)
					{
						U[n, num4] -= L[n, j] * U[j, num4];
					}
				}
			}
		}

		public Matrix SolveWith(Matrix v)
		{
			if (rows != cols)
			{
				throw new MException("The matrix is not square!");
			}

			if (rows != v.rows)
			{
				throw new MException("Wrong number of results in solution vector!");
			}

			if (L == null)
			{
				MakeLU();
			}

			Matrix matrix = new Matrix(rows, 1);
			for (int i = 0; i < rows; i++)
			{
				matrix[i, 0] = v[pi[i], 0];
			}

			Matrix b = SubsForth(L, matrix);
			return SubsBack(U, b);
		}

		public Matrix Invert()
		{
			if (L == null)
			{
				MakeLU();
			}

			Matrix matrix = new Matrix(rows, cols);
			for (int i = 0; i < rows; i++)
			{
				Matrix matrix2 = ZeroMatrix(rows, 1);
				matrix2[i, 0] = 1.0;
				Matrix v = SolveWith(matrix2);
				matrix.SetCol(v, i);
			}

			return matrix;
		}

		public double Det()
		{
			if (L == null)
			{
				MakeLU();
			}

			double num = detOfP;
			for (int i = 0; i < rows; i++)
			{
				num *= U[i, i];
			}

			return num;
		}

		public Matrix GetP()
		{
			if (L == null)
			{
				MakeLU();
			}

			Matrix matrix = ZeroMatrix(rows, cols);
			for (int i = 0; i < rows; i++)
			{
				matrix[pi[i], i] = 1.0;
			}

			return matrix;
		}

		public Matrix Duplicate()
		{
			Matrix matrix = new Matrix(rows, cols);
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < cols; j++)
				{
					matrix[i, j] = mat[i, j];
				}
			}

			return matrix;
		}

		public static Matrix SubsForth(Matrix A, Matrix b)
		{
			if (A.L == null)
			{
				A.MakeLU();
			}

			int num = A.rows;
			Matrix matrix = new Matrix(num, 1);
			for (int i = 0; i < num; i++)
			{
				matrix[i, 0] = b[i, 0];
				for (int j = 0; j < i; j++)
				{
					matrix[i, 0] -= A[i, j] * matrix[j, 0];
				}

				matrix[i, 0] /= A[i, i];
			}

			return matrix;
		}

		public static Matrix SubsBack(Matrix A, Matrix b)
		{
			if (A.L == null)
			{
				A.MakeLU();
			}

			int num = A.rows;
			Matrix matrix = new Matrix(num, 1);
			for (int num2 = num - 1; num2 > -1; num2--)
			{
				matrix[num2, 0] = b[num2, 0];
				for (int num3 = num - 1; num3 > num2; num3--)
				{
					matrix[num2, 0] -= A[num2, num3] * matrix[num3, 0];
				}

				matrix[num2, 0] /= A[num2, num2];
			}

			return matrix;
		}

		public static Matrix ZeroMatrix(int iRows, int iCols)
		{
			Matrix matrix = new Matrix(iRows, iCols);
			for (int i = 0; i < iRows; i++)
			{
				for (int j = 0; j < iCols; j++)
				{
					matrix[i, j] = 0.0;
				}
			}

			return matrix;
		}

		public static Matrix IdentityMatrix(int iRows, int iCols)
		{
			Matrix matrix = ZeroMatrix(iRows, iCols);
			for (int i = 0; i < System.Math.Min(iRows, iCols); i++)
			{
				matrix[i, i] = 1.0;
			}

			return matrix;
		}

		public static Matrix RandomMatrix(int iRows, int iCols, int dispersion)
		{
			Random random = new Random();
			Matrix matrix = new Matrix(iRows, iCols);
			for (int i = 0; i < iRows; i++)
			{
				for (int j = 0; j < iCols; j++)
				{
					matrix[i, j] = random.Next(-dispersion, dispersion);
				}
			}

			return matrix;
		}

		public static Matrix Parse(string ps)
		{
			string input = NormalizeMatrixString(ps);
			string[] array = Regex.Split(input, "\r\n");
			string[] array2 = array[0].Split(' ');
			Matrix matrix = new Matrix(array.Length, array2.Length);
			try
			{
				for (int i = 0; i < array.Length; i++)
				{
					array2 = array[i].Split(' ');
					for (int j = 0; j < array2.Length; j++)
					{
						matrix[i, j] = double.Parse(array2[j]);
					}
				}

				return matrix;
			}
			catch (FormatException)
			{
				throw new MException("Wrong input format!");
			}
		}

		public override string ToString()
		{
			string text = string.Empty;
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < cols; j++)
				{
					text = text + string.Format("{0,5:0.00}", mat[i, j]) + " ";
				}

				text += "\r\n";
			}

			return text;
		}

		public static Matrix Transpose(Matrix m)
		{
			Matrix matrix = new Matrix(m.cols, m.rows);
			for (int i = 0; i < m.rows; i++)
			{
				for (int j = 0; j < m.cols; j++)
				{
					matrix[j, i] = m[i, j];
				}
			}

			return matrix;
		}

		public static Matrix Power(Matrix m, int pow)
		{
			if (pow == 0)
			{
				return IdentityMatrix(m.rows, m.cols);
			}

			if (pow == 1)
			{
				return m.Duplicate();
			}

			if (pow == -1)
			{
				return m.Invert();
			}

			Matrix matrix;
			if (pow < 0)
			{
				matrix = m.Invert();
				pow *= -1;
			}
			else
			{
				matrix = m.Duplicate();
			}

			Matrix result = IdentityMatrix(m.rows, m.cols);
			while (pow != 0)
			{
				if ((pow & 1) == 1)
				{
					result *= matrix;
				}

				matrix *= matrix;
				pow >>= 1;
			}

			return result;
		}

		private static void SafeAplusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
		{
			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					C[i, j] = 0.0;
					if (xa + j < A.cols && ya + i < A.rows)
					{
						C[i, j] += A[ya + i, xa + j];
					}

					if (xb + j < B.cols && yb + i < B.rows)
					{
						C[i, j] += B[yb + i, xb + j];
					}
				}
			}
		}

		private static void SafeAminusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
		{
			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					C[i, j] = 0.0;
					if (xa + j < A.cols && ya + i < A.rows)
					{
						C[i, j] += A[ya + i, xa + j];
					}

					if (xb + j < B.cols && yb + i < B.rows)
					{
						C[i, j] -= B[yb + i, xb + j];
					}
				}
			}
		}

		private static void SafeACopytoC(Matrix A, int xa, int ya, Matrix C, int size)
		{
			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					C[i, j] = 0.0;
					if (xa + j < A.cols && ya + i < A.rows)
					{
						C[i, j] += A[ya + i, xa + j];
					}
				}
			}
		}

		private static void AplusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
		{
			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					C[i, j] = A[ya + i, xa + j] + B[yb + i, xb + j];
				}
			}
		}

		private static void AminusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
		{
			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					C[i, j] = A[ya + i, xa + j] - B[yb + i, xb + j];
				}
			}
		}

		private static void ACopytoC(Matrix A, int xa, int ya, Matrix C, int size)
		{
			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					C[i, j] = A[ya + i, xa + j];
				}
			}
		}

		private static Matrix StrassenMultiply(Matrix A, Matrix B)
		{
			if (A.cols != B.rows)
			{
				throw new MException("Wrong dimension of matrix!");
			}

			int num = System.Math.Max(System.Math.Max(A.rows, A.cols), System.Math.Max(B.rows, B.cols));
			Matrix matrix;
			if (num < 32)
			{
				matrix = ZeroMatrix(A.rows, B.cols);
				for (int i = 0; i < matrix.rows; i++)
				{
					for (int j = 0; j < matrix.cols; j++)
					{
						for (int k = 0; k < A.cols; k++)
						{
							matrix[i, j] += A[i, k] * B[k, j];
						}
					}
				}

				return matrix;
			}

			int num2 = 1;
			int num3 = 0;
			while (num > num2)
			{
				num2 *= 2;
				num3++;
			}

			int num4 = num2 / 2;
			Matrix[,] array = new Matrix[num3, 9];
			for (int l = 0; l < num3 - 4; l++)
			{
				int num5 = (int)System.Math.Pow(2.0, num3 - l - 1);
				for (int m = 0; m < 9; m++)
				{
					array[l, m] = new Matrix(num5, num5);
				}
			}

			SafeAplusBintoC(A, 0, 0, A, num4, num4, array[0, 0], num4);
			SafeAplusBintoC(B, 0, 0, B, num4, num4, array[0, 1], num4);
			StrassenMultiplyRun(array[0, 0], array[0, 1], array[0, 2], 1, array);
			SafeAplusBintoC(A, 0, num4, A, num4, num4, array[0, 0], num4);
			SafeACopytoC(B, 0, 0, array[0, 1], num4);
			StrassenMultiplyRun(array[0, 0], array[0, 1], array[0, 3], 1, array);
			SafeACopytoC(A, 0, 0, array[0, 0], num4);
			SafeAminusBintoC(B, num4, 0, B, num4, num4, array[0, 1], num4);
			StrassenMultiplyRun(array[0, 0], array[0, 1], array[0, 4], 1, array);
			SafeACopytoC(A, num4, num4, array[0, 0], num4);
			SafeAminusBintoC(B, 0, num4, B, 0, 0, array[0, 1], num4);
			StrassenMultiplyRun(array[0, 0], array[0, 1], array[0, 5], 1, array);
			SafeAplusBintoC(A, 0, 0, A, num4, 0, array[0, 0], num4);
			SafeACopytoC(B, num4, num4, array[0, 1], num4);
			StrassenMultiplyRun(array[0, 0], array[0, 1], array[0, 6], 1, array);
			SafeAminusBintoC(A, 0, num4, A, 0, 0, array[0, 0], num4);
			SafeAplusBintoC(B, 0, 0, B, num4, 0, array[0, 1], num4);
			StrassenMultiplyRun(array[0, 0], array[0, 1], array[0, 7], 1, array);
			SafeAminusBintoC(A, num4, 0, A, num4, num4, array[0, 0], num4);
			SafeAplusBintoC(B, 0, num4, B, num4, num4, array[0, 1], num4);
			StrassenMultiplyRun(array[0, 0], array[0, 1], array[0, 8], 1, array);
			matrix = new Matrix(A.rows, B.cols);
			for (int n = 0; n < System.Math.Min(num4, matrix.rows); n++)
			{
				for (int num6 = 0; num6 < System.Math.Min(num4, matrix.cols); num6++)
				{
					matrix[n, num6] = array[0, 2][n, num6] + array[0, 5][n, num6] - array[0, 6][n, num6] +
					                  array[0, 8][n, num6];
				}
			}

			for (int num7 = 0; num7 < System.Math.Min(num4, matrix.rows); num7++)
			{
				for (int num8 = num4; num8 < System.Math.Min(2 * num4, matrix.cols); num8++)
				{
					matrix[num7, num8] = array[0, 4][num7, num8 - num4] + array[0, 6][num7, num8 - num4];
				}
			}

			for (int num9 = num4; num9 < System.Math.Min(2 * num4, matrix.rows); num9++)
			{
				for (int num10 = 0; num10 < System.Math.Min(num4, matrix.cols); num10++)
				{
					matrix[num9, num10] = array[0, 3][num9 - num4, num10] + array[0, 5][num9 - num4, num10];
				}
			}

			for (int num11 = num4; num11 < System.Math.Min(2 * num4, matrix.rows); num11++)
			{
				for (int num12 = num4; num12 < System.Math.Min(2 * num4, matrix.cols); num12++)
				{
					matrix[num11, num12] = array[0, 2][num11 - num4, num12 - num4] -
					                       array[0, 3][num11 - num4, num12 - num4] +
					                       array[0, 4][num11 - num4, num12 - num4] +
					                       array[0, 7][num11 - num4, num12 - num4];
				}
			}

			return matrix;
		}

		private static void StrassenMultiplyRun(Matrix A, Matrix B, Matrix C, int l, Matrix[,] f)
		{
			int num = A.rows;
			int num2 = num / 2;
			if (num < 32)
			{
				for (int i = 0; i < C.rows; i++)
				{
					for (int j = 0; j < C.cols; j++)
					{
						C[i, j] = 0.0;
						for (int k = 0; k < A.cols; k++)
						{
							C[i, j] += A[i, k] * B[k, j];
						}
					}
				}

				return;
			}

			AplusBintoC(A, 0, 0, A, num2, num2, f[l, 0], num2);
			AplusBintoC(B, 0, 0, B, num2, num2, f[l, 1], num2);
			StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 2], l + 1, f);
			AplusBintoC(A, 0, num2, A, num2, num2, f[l, 0], num2);
			ACopytoC(B, 0, 0, f[l, 1], num2);
			StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 3], l + 1, f);
			ACopytoC(A, 0, 0, f[l, 0], num2);
			AminusBintoC(B, num2, 0, B, num2, num2, f[l, 1], num2);
			StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 4], l + 1, f);
			ACopytoC(A, num2, num2, f[l, 0], num2);
			AminusBintoC(B, 0, num2, B, 0, 0, f[l, 1], num2);
			StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 5], l + 1, f);
			AplusBintoC(A, 0, 0, A, num2, 0, f[l, 0], num2);
			ACopytoC(B, num2, num2, f[l, 1], num2);
			StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 6], l + 1, f);
			AminusBintoC(A, 0, num2, A, 0, 0, f[l, 0], num2);
			AplusBintoC(B, 0, 0, B, num2, 0, f[l, 1], num2);
			StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 7], l + 1, f);
			AminusBintoC(A, num2, 0, A, num2, num2, f[l, 0], num2);
			AplusBintoC(B, 0, num2, B, num2, num2, f[l, 1], num2);
			StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 8], l + 1, f);
			for (int m = 0; m < num2; m++)
			{
				for (int n = 0; n < num2; n++)
				{
					C[m, n] = f[l, 2][m, n] + f[l, 5][m, n] - f[l, 6][m, n] + f[l, 8][m, n];
				}
			}

			for (int num3 = 0; num3 < num2; num3++)
			{
				for (int num4 = num2; num4 < num; num4++)
				{
					C[num3, num4] = f[l, 4][num3, num4 - num2] + f[l, 6][num3, num4 - num2];
				}
			}

			for (int num5 = num2; num5 < num; num5++)
			{
				for (int num6 = 0; num6 < num2; num6++)
				{
					C[num5, num6] = f[l, 3][num5 - num2, num6] + f[l, 5][num5 - num2, num6];
				}
			}

			for (int num7 = num2; num7 < num; num7++)
			{
				for (int num8 = num2; num8 < num; num8++)
				{
					C[num7, num8] = f[l, 2][num7 - num2, num8 - num2] - f[l, 3][num7 - num2, num8 - num2] +
					                f[l, 4][num7 - num2, num8 - num2] + f[l, 7][num7 - num2, num8 - num2];
				}
			}
		}

		public static Matrix StupidMultiply(Matrix m1, Matrix m2)
		{
			if (m1.cols != m2.rows)
			{
				throw new MException("Wrong dimensions of matrix!");
			}

			Matrix matrix = ZeroMatrix(m1.rows, m2.cols);
			for (int i = 0; i < matrix.rows; i++)
			{
				for (int j = 0; j < matrix.cols; j++)
				{
					for (int k = 0; k < m1.cols; k++)
					{
						matrix[i, j] += m1[i, k] * m2[k, j];
					}
				}
			}

			return matrix;
		}

		private static Matrix Multiply(double n, Matrix m)
		{
			Matrix matrix = new Matrix(m.rows, m.cols);
			for (int i = 0; i < m.rows; i++)
			{
				for (int j = 0; j < m.cols; j++)
				{
					matrix[i, j] = m[i, j] * n;
				}
			}

			return matrix;
		}

		private static Matrix Add(Matrix m1, Matrix m2)
		{
			if (m1.rows != m2.rows || m1.cols != m2.cols)
			{
				throw new MException("Matrices must have the same dimensions!");
			}

			Matrix matrix = new Matrix(m1.rows, m1.cols);
			for (int i = 0; i < matrix.rows; i++)
			{
				for (int j = 0; j < matrix.cols; j++)
				{
					matrix[i, j] = m1[i, j] + m2[i, j];
				}
			}

			return matrix;
		}

		public static string NormalizeMatrixString(string matStr)
		{
			while (matStr.IndexOf("  ") != -1)
			{
				matStr = matStr.Replace("  ", " ");
			}

			matStr = matStr.Replace(" \r\n", "\r\n");
			matStr = matStr.Replace("\r\n ", "\r\n");
			matStr = matStr.Replace("\r\n", "|");
			while (matStr.LastIndexOf("|") == matStr.Length - 1)
			{
				matStr = matStr.Substring(0, matStr.Length - 1);
			}

			matStr = matStr.Replace("|", "\r\n");
			return matStr.Trim();
		}

		public static Matrix operator -(Matrix m)
		{
			return Multiply(-1.0, m);
		}

		public static Matrix operator +(Matrix m1, Matrix m2)
		{
			return Add(m1, m2);
		}

		public static Matrix operator -(Matrix m1, Matrix m2)
		{
			return Add(m1, -m2);
		}

		public static Matrix operator *(Matrix m1, Matrix m2)
		{
			return StrassenMultiply(m1, m2);
		}

		public static Matrix operator *(double n, Matrix m)
		{
			return Multiply(n, m);
		}
	}
}
