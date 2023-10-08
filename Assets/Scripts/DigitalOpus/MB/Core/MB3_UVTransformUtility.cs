using UnityEngine;

namespace DigitalOpus.MB.Core
{
	public class MB3_UVTransformUtility
	{
		public static void Test()
		{
			DRect t = new DRect(0.5, 0.5, 2.0, 2.0);
			DRect t2 = new DRect(0.25, 0.25, 3.0, 3.0);
			DRect r = InverseTransform(ref t);
			DRect r2 = InverseTransform(ref t2);
			DRect r3 = CombineTransforms(ref t, ref r2);
			Debug.Log(r);
			Debug.Log(r3);
			Debug.Log("one mat trans " + TransformPoint(ref t, new Vector2(1f, 1f)));
			Debug.Log("one inv mat trans " + TransformPoint(ref r, new Vector2(1f, 1f)).ToString("f4"));
			Debug.Log("zero " + TransformPoint(ref r3, new Vector2(0f, 0f)).ToString("f4"));
			Debug.Log("one " + TransformPoint(ref r3, new Vector2(1f, 1f)).ToString("f4"));
		}

		public static float TransformX(DRect r, double x)
		{
			return (float)(r.width * x + r.x);
		}

		public static DRect CombineTransforms(ref DRect r1, ref DRect r2)
		{
			return new DRect(r1.x * r2.width + r2.x, r1.y * r2.height + r2.y, r1.width * r2.width,
				r1.height * r2.height);
		}

		public static Rect CombineTransforms(ref Rect r1, ref Rect r2)
		{
			return new Rect(r1.x * r2.width + r2.x, r1.y * r2.height + r2.y, r1.width * r2.width,
				r1.height * r2.height);
		}

		public static void Canonicalize(ref DRect r, double minX, double minY)
		{
			r.x -= Mathf.FloorToInt((float)r.x);
			if (r.x < minX)
			{
				r.x += Mathf.CeilToInt((float)minX);
			}

			r.y -= Mathf.FloorToInt((float)r.y);
			if (r.y < minY)
			{
				r.y += Mathf.CeilToInt((float)minY);
			}
		}

		public static void Canonicalize(ref Rect r, float minX, float minY)
		{
			r.x -= Mathf.FloorToInt(r.x);
			if (r.x < minX)
			{
				r.x += Mathf.CeilToInt(minX);
			}

			r.y -= Mathf.FloorToInt(r.y);
			if (r.y < minY)
			{
				r.y += Mathf.CeilToInt(minY);
			}
		}

		public static DRect InverseTransform(ref DRect t)
		{
			DRect result = default(DRect);
			result.x = (0.0 - t.x) / t.width;
			result.y = (0.0 - t.y) / t.height;
			result.width = 1.0 / t.width;
			result.height = 1.0 / t.height;
			return result;
		}

		public static DRect GetEncapsulatingRect(ref DRect uvRect1, ref DRect uvRect2)
		{
			double x = uvRect1.x;
			double y = uvRect1.y;
			double num = uvRect1.x + uvRect1.width;
			double num2 = uvRect1.y + uvRect1.height;
			double x2 = uvRect2.x;
			double y2 = uvRect2.y;
			double num3 = uvRect2.x + uvRect2.width;
			double num4 = uvRect2.y + uvRect2.height;
			double num5;
			double num6 = (num5 = x);
			double num7;
			double num8 = (num7 = y);
			if (x2 < num6)
			{
				num6 = x2;
			}

			if (x < num6)
			{
				num6 = x;
			}

			if (y2 < num8)
			{
				num8 = y2;
			}

			if (y < num8)
			{
				num8 = y;
			}

			if (num3 > num5)
			{
				num5 = num3;
			}

			if (num > num5)
			{
				num5 = num;
			}

			if (num4 > num7)
			{
				num7 = num4;
			}

			if (num2 > num7)
			{
				num7 = num2;
			}

			return new DRect(num6, num8, num5 - num6, num7 - num8);
		}

		public static bool RectContains(ref DRect bigRect, ref DRect smallToTestIfFits)
		{
			double x = smallToTestIfFits.x;
			double y = smallToTestIfFits.y;
			double num = smallToTestIfFits.x + smallToTestIfFits.width;
			double num2 = smallToTestIfFits.y + smallToTestIfFits.height;
			double num3 = bigRect.x - 0.009999999776482582;
			double num4 = bigRect.y - 0.009999999776482582;
			double num5 = bigRect.x + bigRect.width + 0.019999999552965164;
			double num6 = bigRect.y + bigRect.height + 0.019999999552965164;
			return num3 <= x && x <= num5 && num3 <= num && num <= num5 && num4 <= y && y <= num6 && num4 <= num2 &&
			       num2 <= num6;
		}

		public static bool RectContains(ref Rect bigRect, ref Rect smallToTestIfFits)
		{
			float x = smallToTestIfFits.x;
			float y = smallToTestIfFits.y;
			float num = smallToTestIfFits.x + smallToTestIfFits.width;
			float num2 = smallToTestIfFits.y + smallToTestIfFits.height;
			float num3 = bigRect.x - 0.01f;
			float num4 = bigRect.y - 0.01f;
			float num5 = bigRect.x + bigRect.width + 0.02f;
			float num6 = bigRect.y + bigRect.height + 0.02f;
			return num3 <= x && x <= num5 && num3 <= num && num <= num5 && num4 <= y && y <= num6 && num4 <= num2 &&
			       num2 <= num6;
		}

		internal static Vector2 TransformPoint(ref DRect r, Vector2 p)
		{
			return new Vector2((float)(r.width * (double)p.x + r.x), (float)(r.height * (double)p.y + r.y));
		}
	}
}
