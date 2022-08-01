using UnityEngine;

namespace DigitalOpus.MB.Core
{
	public struct DRect
	{
		public double x;

		public double y;

		public double width;

		public double height;

		public Vector2 min
		{
			get
			{
				return new Vector2((float)x, (float)y);
			}
		}

		public Vector2 max
		{
			get
			{
				return new Vector2((float)(x + width), (float)(y + width));
			}
		}

		public Vector2 size
		{
			get
			{
				return new Vector2((float)width, (float)width);
			}
		}

		public DRect(Rect r)
		{
			x = r.x;
			y = r.y;
			width = r.width;
			height = r.height;
		}

		public DRect(Vector2 o, Vector2 s)
		{
			x = o.x;
			y = o.y;
			width = s.x;
			height = s.y;
		}

		public DRect(float xx, float yy, float w, float h)
		{
			x = xx;
			y = yy;
			width = w;
			height = h;
		}

		public DRect(double xx, double yy, double w, double h)
		{
			x = xx;
			y = yy;
			width = w;
			height = h;
		}

		public Rect GetRect()
		{
			return new Rect((float)x, (float)y, (float)width, (float)height);
		}

		public override bool Equals(object obj)
		{
			DRect dRect = (DRect)obj;
			if (dRect.x == x && dRect.y == y && dRect.width == width && dRect.height == height)
			{
				return true;
			}
			return false;
		}

		public static bool operator ==(DRect a, DRect b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(DRect a, DRect b)
		{
			return !a.Equals(b);
		}

		public override string ToString()
		{
			return string.Format("(x={0},y={1},w={2},h={3})", x.ToString("F5"), y.ToString("F5"), width.ToString("F5"), height.ToString("F5"));
		}

		public bool Encloses(DRect smallToTestIfFits)
		{
			double num = smallToTestIfFits.x;
			double num2 = smallToTestIfFits.y;
			double num3 = smallToTestIfFits.x + smallToTestIfFits.width;
			double num4 = smallToTestIfFits.y + smallToTestIfFits.height;
			double num5 = x;
			double num6 = y;
			double num7 = x + width;
			double num8 = y + height;
			return num5 <= num && num <= num7 && num5 <= num3 && num3 <= num7 && num6 <= num2 && num2 <= num8 && num6 <= num4 && num4 <= num8;
		}

		public override int GetHashCode()
		{
			return x.GetHashCode() ^ y.GetHashCode() ^ width.GetHashCode() ^ height.GetHashCode();
		}
	}
}
