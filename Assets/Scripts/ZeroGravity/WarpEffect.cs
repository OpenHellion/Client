using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity
{
	public class WarpEffect : MonoBehaviour
	{
		public GameObject WarpBlackHole;

		private float lerpValue;

		private int direction = 1;

		public float Speed = 1f;

		private void Update()
		{
			if ((lerpValue < 1f && direction > 0) || (lerpValue > 0f && direction < 0))
			{
				lerpValue = Mathf.Clamp01(lerpValue + (float)direction * Speed * Time.deltaTime);
			}

			if (lerpValue <= 0f)
			{
				base.gameObject.Activate(false);
			}
		}

		public void SetActive(bool value, bool instant = false)
		{
			direction = (value ? 1 : (-1));
			if (instant)
			{
				lerpValue = Mathf.Clamp01(direction);
			}

			Lens component = MyPlayer.Instance.FpsController.FarCamera.GetComponent<Lens>();
			if (component != null)
			{
				if (value)
				{
					component.BH = WarpBlackHole;
					component.enabled = true;
				}
				else
				{
					component.enabled = false;
					component.BH = null;
				}
			}

			base.gameObject.Activate(true);
		}
	}
}
