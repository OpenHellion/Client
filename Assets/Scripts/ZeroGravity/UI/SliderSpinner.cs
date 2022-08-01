using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class SliderSpinner : MonoBehaviour
	{
		public delegate void OnChangeDelegate(float val);

		public OnChangeDelegate OnChange;

		private bool hovering;

		private Slider slider;

		public bool AttachListener = true;

		public float Increment = 1f;

		private float minValue;

		private float maxValue;

		private bool applyChanges = true;

		private void Start()
		{
			slider = GetComponent<Slider>();
			minValue = slider.minValue;
			maxValue = slider.maxValue;
			if (AttachListener)
			{
				slider.onValueChanged.AddListener(OnValueChanged);
			}
		}

		private void Update()
		{
			if (hovering && InputManager.GetAxis(InputManager.AxisNames.MouseWheel) != 0f)
			{
				float num = 0f;
				slider.value = num;
				if (InputManager.GetAxis(InputManager.AxisNames.MouseWheel) > 0f)
				{
					num += Increment;
				}
				else if (InputManager.GetAxis(InputManager.AxisNames.MouseWheel) < 0f)
				{
					num -= Increment;
				}
				if (num >= minValue && num <= maxValue)
				{
					slider.value = num;
				}
			}
		}

		private void UpdateField(float value)
		{
			if (slider.value != value)
			{
				applyChanges = false;
				slider.value = value;
			}
			if (OnChange != null)
			{
				OnChange(value);
			}
		}

		public void OnValueChanged(float inputValue)
		{
			if (!applyChanges)
			{
				applyChanges = true;
				return;
			}
			float num = 0f;
			num += inputValue;
			if (num < minValue)
			{
				num = minValue;
			}
			else if (num > maxValue)
			{
				num = maxValue;
			}
			UpdateField(num);
		}

		public void MouseHoverEnter()
		{
			hovering = true;
			slider.Select();
		}

		public void MouseHoverExit()
		{
			hovering = false;
		}
	}
}
