using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class FillOnStart : MonoBehaviour
	{
		public float fillSpeed = 0.3f;

		public float targetValue = 0.1f;

		public Image bar;

		private float helper;

		private void Start()
		{
		}

		private void Update()
		{
			if (bar.fillAmount != targetValue)
			{
				RadiNesto(bar, targetValue, fillSpeed);
			}
		}

		private void RadiNesto(Image bar, float target, float speed)
		{
			helper += speed * Time.deltaTime;
			bar.fillAmount = Mathf.Clamp01(Mathf.Lerp(bar.fillAmount, targetValue, helper));
			if (helper > 1f)
			{
				helper = 0f;
			}
		}
	}
}
