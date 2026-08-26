using UnityEngine;

namespace ProBuilder2.Common
{
	public class pb_HandleConstraint2D
	{
		public int x;

		public int y;

		public static readonly pb_HandleConstraint2D None = new pb_HandleConstraint2D(1, 1);

		public pb_HandleConstraint2D(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public pb_HandleConstraint2D Inverse()
		{
			return new pb_HandleConstraint2D((x != 1) ? 1 : 0, (y != 1) ? 1 : 0);
		}

		public Vector2 Mask(Vector2 v)
		{
			v.x *= x;
			v.y *= y;
			return v;
		}

		public Vector2 InverseMask(Vector2 v)
		{
			v.x *= ((x != 1) ? 1f : 0f);
			v.y *= ((y != 1) ? 1f : 0f);
			return v;
		}

		public static bool operator ==(pb_HandleConstraint2D a, pb_HandleConstraint2D b)
		{
			return a.x == b.x && a.y == b.y;
		}

		public static bool operator !=(pb_HandleConstraint2D a, pb_HandleConstraint2D b)
		{
			return a.x != b.x || a.y != b.y;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object o)
		{
			return o is pb_HandleConstraint2D && ((pb_HandleConstraint2D)o).x == x && ((pb_HandleConstraint2D)o).y == y;
		}

		public override string ToString()
		{
			return "(" + x + ", " + y + ")";
		}
	}
}
