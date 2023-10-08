using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public static class AnimationHelperUI
	{
		public static void Rotate(Image img, float rate)
		{
			Color color = img.color;
			if (rate > 0f)
			{
				color.a = 1f;
			}
			else if (rate < 0f)
			{
				color.a = 0f;
			}
			else
			{
				color.a = 0.5f;
			}

			img.color = color;
		}
	}
}
