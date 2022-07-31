using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class PowerSupplySystemItem : MonoBehaviour
	{
		private Color activeColor = new Color(83f / 85f, 19f / 85f, 19f / 85f, 20f / 51f);

		private Color defaultColor = new Color(1f, 1f, 1f, 36f / 85f);

		private float timer;

		private float treshold = 1f;

		private bool gore;

		private Image image;

		public bool IsBlinking;

		public int testState;

		private int state;

		public int State
		{
			get
			{
				return state;
			}
			set
			{
				switch (value)
				{
				case 0:
					image.color = activeColor;
					IsBlinking = false;
					break;
				case 1:
					image.color = defaultColor;
					IsBlinking = false;
					break;
				case 2:
					image.color = activeColor;
					IsBlinking = true;
					timer = 0f;
					break;
				case 3:
					image.color = defaultColor;
					IsBlinking = true;
					timer = 0f;
					break;
				}
				state = value;
			}
		}

		private void Start()
		{
			image = GetComponent<Image>();
			State = testState;
		}

		private void Update()
		{
			if (!IsBlinking)
			{
				return;
			}
			timer += Time.deltaTime;
			if (timer < treshold)
			{
				Color color = image.color;
				if (gore)
				{
					color.a = timer / treshold;
				}
				else
				{
					color.a = 1f - timer / treshold;
				}
				image.color = color;
			}
			else
			{
				gore = !gore;
				timer = 0f;
			}
		}
	}
}
