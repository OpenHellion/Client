using Luminosity.IO;
using UnityEngine;

namespace ZeroGravity.UI
{
	public class InputController
	{
		public enum AxisNames
		{
			Right = 0,
			Forward = 1,
			LookHorizontal = 2,
			LookVertical = 3,
			Mouse1 = 4,
			Mouse2 = 5,
			Mouse3 = 6,
			MouseWheel = 7,
			Space = 8,
			Submit = 9,
			Cancel = 10,
			LeftCtrl = 11,
			LeftShift = 12,
			LeftAlt = 13,
			MenuHorizontal = 14,
			MenuVertical = 15,
			Escape = 16,
			F = 17,
			Lean = 18,
			Tab = 19,
			G = 20,
			R = 21,
			Z = 22,
			Enter = 23,
			NumPlus = 24,
			NumMinus = 25,
			Y = 27,
			T = 28,
			CapsLock = 29,
			Tilda = 30,
			UpArrow = 31,
			DownArrow = 32,
			LeftArrow = 33,
			RightArrow = 34,
			X = 35,
			H = 36,
			V = 37,
			J = 38,
			M = 39,
			L = 40,
			Alpha1 = 41,
			Alpha2 = 42,
			Alpha3 = 43,
			Alpha4 = 44,
			Alpha5 = 45,
			Alpha6 = 46,
			Alpha7 = 47,
			Alpha8 = 48,
			Alpha9 = 49,
			Alpha0 = 50,
			B = 51,
			O = 52
		}

		private static float _mouseSensitivity;
		public static float MouseSensitivity
		{
			get
			{
				return _mouseSensitivity;
			}
			set
			{
				_mouseSensitivity = value;
				InputManager.GetAction("KeyboardAndMouse", "LookVertical").GetBinding(0).Sensitivity = _mouseSensitivity / 10f;
				InputManager.GetAction("KeyboardAndMouse", "LookHorizontal").GetBinding(0).Sensitivity = _mouseSensitivity / 10f;
			}
		}

		public static bool GetButton(AxisNames key)
		{
			return InputManager.GetButton(key.ToString());
		}

		public static bool GetButtonDown(AxisNames key)
		{
			return InputManager.GetButtonDown(key.ToString());
		}

		public static bool GetButtonUp(AxisNames key)
		{
			return InputManager.GetButtonUp(key.ToString());
		}

		public static float GetAxis(AxisNames name)
		{
			return InputManager.GetAxis(name.ToString());
		}

		public static float GetAxisRaw(AxisNames name)
		{
			return InputManager.GetAxisRaw(name.ToString());
		}

		public static bool GetKey(KeyCode key)
		{
			return InputManager.GetKey(key);
		}

		public static bool GetKeyDown(KeyCode key)
		{
			return InputManager.GetKeyDown(key);
		}

		public static bool GetKeyUp(KeyCode key)
		{
			return InputManager.GetKeyUp(key);
		}

		public static AccelerationEvent GetAccelerationEvent(int index)
		{
			return InputManager.GetAccelerationEvent(index);
		}

		public static bool GetMouseButton(int index)
		{
			return InputManager.GetMouseButton(index);
		}

		public static bool GetMouseButtonDown(int index)
		{
			return InputManager.GetMouseButtonDown(index);
		}

		public static bool GetMouseButtonUp(int index)
		{
			return InputManager.GetMouseButtonUp(index);
		}

		public static void LoadJSON()
		{
			InputManager.Load(new InputLoaderJSON());
		}

		public static void LoadDefaultJSON()
		{
			InputManager.Load(new InputDefaultLoaderJSON());
		}

		public static void SaveDefaultJSON()
		{
			InputManager.Save(new InputDefaultSaverJSON());
		}

		public static void ResetInputAxis()
		{
			InputManager.ResetInputAxes();
		}

		public static string GetAxisKeyName(AxisNames key, bool getPositive = true, bool getNegative = true, bool getAlt = false)
		{
			InputAction actions = InputManager.GetAction("KeyboardAndMouse", key.ToString());
			if (!getAlt)
			{
				// Get main binding.
				if (actions.Bindings[0].Type == InputType.DigitalAxis)
				{
					if (getPositive && getNegative)
					{
						return actions.Bindings[0].Positive.ToString() + "/" + actions.Bindings[0].Negative;
					}
					if (getPositive && !getNegative)
					{
						return actions.Bindings[0].Positive.ToString();
					}
					if (!getPositive && getNegative)
					{
						return actions.Bindings[0].Negative.ToString();
					}
				}

				return actions.Bindings[0].Positive.ToString();
			}
			else
			{
				// Get alternative binding.
				if (actions.Bindings[1].Type == InputType.DigitalAxis)
				{
					if (getPositive && getNegative)
					{
						return actions.Bindings[1].Positive.ToString() + "/" + actions.Bindings[1].Negative;
					}
					if (getPositive && !getNegative)
					{
						return actions.Bindings[1].Positive.ToString();
					}
					if (!getPositive && getNegative)
					{
						return actions.Bindings[1].Negative.ToString();
					}
				}
			}

			return string.Empty;
		}
	}
}
