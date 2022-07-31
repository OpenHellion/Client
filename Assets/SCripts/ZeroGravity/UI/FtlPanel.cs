using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class FtlPanel : MonoBehaviour
	{
		public Image ZeroCells;

		public Image HexaRed;

		private int colorLerp = -1;

		private float korak = 0.1f;

		private void Start()
		{
		}

		private void Update()
		{
			Color color = ZeroCells.color;
			if (colorLerp > 0)
			{
				color.a = ZeroCells.color.a + (float)colorLerp * korak;
				if (color.a > 1f)
				{
					colorLerp = -1;
				}
			}
			else
			{
				color.a = ZeroCells.color.a + (float)colorLerp * korak;
				if (color.a < 0f)
				{
					colorLerp = 1;
				}
			}
			ZeroCells.color = color;
			HexaRed.color = color;
		}
	}
}
