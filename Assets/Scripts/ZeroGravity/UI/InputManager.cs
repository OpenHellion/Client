using System.Collections.Generic;
using Luminosity.IO;
using UnityEngine;

namespace ZeroGravity.UI
{
	public class InputManager
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
			Modifier = 26,
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

		public static bool GetButton(AxisNames key)
		{
			return Luminosity.IO.InputManager.GetButton(key.ToString());
		}

		public static bool GetButtonDown(AxisNames key)
		{
			return Luminosity.IO.InputManager.GetButtonDown(key.ToString());
		}

		public static bool GetButtonUp(AxisNames key)
		{
			return Luminosity.IO.InputManager.GetButtonUp(key.ToString());
		}

		public static float GetAxis(AxisNames name)
		{
			return Luminosity.IO.InputManager.GetAxis(name.ToString());
		}

		public static float GetAxisRaw(AxisNames name)
		{
			return Luminosity.IO.InputManager.GetAxisRaw(name.ToString());
		}

		public static bool GetKey(KeyCode key)
		{
			return Luminosity.IO.InputManager.GetKey(key);
		}

		public static bool GetKeyDown(KeyCode key)
		{
			return Luminosity.IO.InputManager.GetKeyDown(key);
		}

		public static bool GetKeyUp(KeyCode key)
		{
			return Luminosity.IO.InputManager.GetKeyUp(key);
		}

		public static AccelerationEvent GetAccelerationEvent(int index)
		{
			return Luminosity.IO.InputManager.GetAccelerationEvent(index);
		}

		public static bool GetMouseButton(int index)
		{
			return Luminosity.IO.InputManager.GetMouseButton(index);
		}

		public static bool GetMouseButtonDown(int index)
		{
			return Luminosity.IO.InputManager.GetMouseButtonDown(index);
		}

		public static bool GetMouseButtonUp(int index)
		{
			return Luminosity.IO.InputManager.GetMouseButtonUp(index);
		}

		public static void LoadJSON()
		{
			Luminosity.IO.InputManager.Load(new InputLoaderJSON());
		}

		public static void LoadDefaultJSON()
		{
			Luminosity.IO.InputManager.Load(new InputDefaultLoaderJSON());
		}

		public static void SaveJSON()
		{
		}

		public static void SaveDefaultJSON()
		{
			Luminosity.IO.InputManager.Save(new InputDefaultSaverJSON());
		}

		public static void ResetInputAxis()
		{
			Luminosity.IO.InputManager.ResetInputAxes();
		}

		public static string GetAxisKeyName(AxisNames key, bool getPositive = true, bool getNegative = true, bool getAlt = false)
		{
			InputAction axisConfiguration = Luminosity.IO.InputManager.GetAction("KeyboardAndMouse", key.ToString());
			foreach (InputBinding binding in axisConfiguration.Bindings)
			{
				if (binding.Type == InputType.DigitalAxis)
				{
					if (getPositive && getNegative && !getAlt)
					{
						return binding.Positive.ToString() + "/" + binding.Negative;
					}
					if (getPositive && !getNegative && !getAlt)
					{
						return binding.Positive.ToString();
					}
					if (!getPositive && getNegative && !getAlt)
					{
						return binding.Negative.ToString();
					}
					return string.Empty;
				}
				if (!getAlt)
				{
					return binding.Positive.ToString();
				}
			}
			return string.Empty;
		}

		public static void SaveDefault(Luminosity.IO.InputManager inputManager)
		{
			List<ControlScheme> list = inputManager.ControlSchemes;
			Json.SerializeDataPath(list, "Resources/Data/ControlsDefault.json");
		}

		public static void LoadDefault(Luminosity.IO.InputManager inputManager)
		{
			Luminosity.IO.InputManager.Load(new InputDefaultLoaderJSON());
		}
	}
}
