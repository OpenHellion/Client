using UnityEngine;

namespace ProBuilder2.Common
{
	public class pb_HsvColor
	{
		public float h;

		public float s;

		public float v;

		public pb_HsvColor(float h, float s, float v)
		{
			this.h = h;
			this.s = s;
			this.v = v;
		}

		public pb_HsvColor(float h, float s, float v, float sv_modifier)
		{
			this.h = h;
			this.s = s * sv_modifier;
			this.v = v * sv_modifier;
		}

		public static pb_HsvColor FromRGB(Color col)
		{
			return pb_ColorUtil.RGBtoHSV(col);
		}

		public override string ToString()
		{
			return $"( {h}, {s}, {v} )";
		}

		public float SqrDistance(pb_HsvColor InColor)
		{
			return InColor.h / 360f - h / 360f + (InColor.s - s) + (InColor.v - v);
		}
	}
}
