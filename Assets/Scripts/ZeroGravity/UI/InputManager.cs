using System.Collections.Generic;
using TeamUtility.IO;
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
			return TeamUtility.IO.InputManager.GetButton(key.ToString());
		}

		public static bool GetButtonDown(AxisNames key)
		{
			return TeamUtility.IO.InputManager.GetButtonDown(key.ToString());
		}

		public static bool GetButtonUp(AxisNames key)
		{
			return TeamUtility.IO.InputManager.GetButtonUp(key.ToString());
		}

		public static float GetAxis(AxisNames name)
		{
			return TeamUtility.IO.InputManager.GetAxis(name.ToString());
		}

		public static float GetAxisRaw(AxisNames name)
		{
			return TeamUtility.IO.InputManager.GetAxisRaw(name.ToString());
		}

		public static bool GetKey(KeyCode key)
		{
			return TeamUtility.IO.InputManager.GetKey(key);
		}

		public static bool GetKeyDown(KeyCode key)
		{
			return TeamUtility.IO.InputManager.GetKeyDown(key);
		}

		public static bool GetKeyUp(KeyCode key)
		{
			return TeamUtility.IO.InputManager.GetKeyUp(key);
		}

		public static AccelerationEvent GetAccelerationEvent(int index)
		{
			return TeamUtility.IO.InputManager.GetAccelerationEvent(index);
		}

		public static bool GetMouseButton(int index)
		{
			return TeamUtility.IO.InputManager.GetMouseButton(index);
		}

		public static bool GetMouseButtonDown(int index)
		{
			return TeamUtility.IO.InputManager.GetMouseButtonDown(index);
		}

		public static bool GetMouseButtonUp(int index)
		{
			return TeamUtility.IO.InputManager.GetMouseButtonUp(index);
		}

		public static void LoadJSON()
		{
			TeamUtility.IO.InputManager.Load(new InputLoaderJSON());
		}

		public static void LoadDefaultJSON()
		{
			TeamUtility.IO.InputManager.Load(new InputDefaultLoaderJSON());
		}

		public static void SaveJSON()
		{
		}

		public static void SaveDefaultJSON()
		{
			TeamUtility.IO.InputManager.Save(new InputDefaultSaverJSON());
		}

		public static void ResetInputAxis()
		{
			TeamUtility.IO.InputManager.ResetInputAxes();
		}

		public static string GetAxisKeyName(AxisNames key, bool getPositive = true, bool getNegative = true, bool getAlt = false)
		{
			AxisConfiguration axisConfiguration = TeamUtility.IO.InputManager.GetAxisConfiguration("KeyboardAndMouse", key.ToString());
			if (axisConfiguration.type == InputType.DigitalAxis)
			{
				if (getPositive && getNegative && !getAlt)
				{
					return axisConfiguration.positive.ToString() + "/" + axisConfiguration.negative;
				}
				if (getPositive && getNegative && getAlt)
				{
					return axisConfiguration.altPositive.ToString() + "/" + axisConfiguration.altNegative;
				}
				if (getPositive && !getNegative && !getAlt)
				{
					return axisConfiguration.positive.ToString();
				}
				if (getPositive && !getNegative && getAlt)
				{
					return axisConfiguration.altPositive.ToString();
				}
				if (!getPositive && getNegative && !getAlt)
				{
					return axisConfiguration.negative.ToString();
				}
				if (!getPositive && getNegative && getAlt)
				{
					return axisConfiguration.altNegative.ToString();
				}
				return string.Empty;
			}
			if (!getAlt)
			{
				return axisConfiguration.positive.ToString();
			}
			return axisConfiguration.altPositive.ToString();
		}

		public static void SaveDefault(TeamUtility.IO.InputManager inputManager)
		{
			for (int i = 0; i < inputManager.inputConfigurations[0].axes.Count; i++)
			{
			}
			List<InputConfiguration> list = new List<InputConfiguration>();
			list = inputManager.inputConfigurations;
			Json.SerializeDataPath(list, "Resources/Data/ControlsDefault.json");
		}

		public static void LoadDefault(TeamUtility.IO.InputManager inputManager)
		{
			InputDefaultLoaderJSON inputDefaultLoaderJSON = new InputDefaultLoaderJSON();
			TeamUtility.IO.InputManager inputManager2 = Object.FindObjectOfType(typeof(TeamUtility.IO.InputManager)) as TeamUtility.IO.InputManager;
			SaveLoadParameters saveLoadParameters = inputDefaultLoaderJSON.Load();
			inputManager2.inputConfigurations = saveLoadParameters.inputConfigurations;
		}
	}
}
