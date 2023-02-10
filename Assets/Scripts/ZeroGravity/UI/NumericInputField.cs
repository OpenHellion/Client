using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class NumericInputField : MonoBehaviour
	{
		public delegate void OnChangeDelegate(float val);

		private bool hovering;

		public float Increment = 1f;

		public float MinValue = float.MinValue;

		public float MaxValue = float.MaxValue;

		private InputField inputField;

		public bool AttachListener = true;

		public OnChangeDelegate OnChange;

		public bool ApplyChanges = true;

		public bool LoopValue;

		public float Value
		{
			get
			{
				return ParseValue(inputField.text);
			}
			set
			{
				inputField.text = value.ToString();
			}
		}

		private void Awake()
		{
			inputField = GetComponent<InputField>();
			if (AttachListener)
			{
				inputField.onValueChanged.AddListener(OnValueChanged);
			}
		}

		private void Update()
		{
			if (!hovering || Mouse.current.scroll.y.ReadValue() == 0f)
			{
				return;
			}
			float result = 0f;
			float.TryParse(inputField.text, out result);
			if (inputField.interactable)
			{
				if (Mouse.current.scroll.y.ReadValue() > 0f)
				{
					result += Increment;
				}
				else if (Mouse.current.scroll.y.ReadValue() < 0f)
				{
					result -= Increment;
				}
			}
			if (!LoopValue)
			{
				if (result >= MinValue && result <= MaxValue)
				{
					inputField.text = result.ToString();
				}
			}
			else if (result >= MinValue && result <= MaxValue)
			{
				inputField.text = result.ToString();
			}
			else if (result <= MinValue)
			{
				inputField.text = MaxValue.ToString();
			}
			else if (result >= MaxValue)
			{
				inputField.text = MinValue.ToString();
			}
		}

		private void UpdateField(float value)
		{
			if (inputField.text != value.ToString())
			{
				ApplyChanges = false;
				inputField.text = value.ToString();
			}
			if (OnChange != null)
			{
				OnChange(value);
			}
		}

		private float ParseValue(string strValue)
		{
			float result = 0f;
			float.TryParse(strValue, out result);
			if (result < MinValue)
			{
				result = MinValue;
			}
			else if (result > MaxValue)
			{
				result = MaxValue;
			}
			return result;
		}

		public void OnValueChanged(string strValue)
		{
			if (!ApplyChanges)
			{
				ApplyChanges = true;
				return;
			}
			float value = ParseValue(strValue);
			UpdateField(value);
		}

		public void MouseHoverEnter()
		{
			hovering = true;
			inputField.Select();
		}

		public void MouseHoverExit()
		{
			hovering = false;
		}
	}
}
