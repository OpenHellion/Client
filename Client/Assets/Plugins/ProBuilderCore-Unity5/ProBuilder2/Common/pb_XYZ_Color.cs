using UnityEngine;

namespace ProBuilder2.Common
{
	public class pb_XYZ_Color
	{
		public float x;

		public float y;

		public float z;

		public pb_XYZ_Color(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public static pb_XYZ_Color FromRGB(Color col)
		{
			return pb_ColorUtil.RGBToXYZ(col);
		}

		public static pb_XYZ_Color FromRGB(float R, float G, float B)
		{
			return pb_ColorUtil.RGBToXYZ(R, G, B);
		}

		public override string ToString()
		{
			return $"( {x}, {y}, {z} )";
		}
	}
}
