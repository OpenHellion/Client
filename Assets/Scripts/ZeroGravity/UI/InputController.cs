using UnityEngine;

namespace ZeroGravity.UI
{
	public class InputController
	{
		/// <summary>
		/// 	Keys that can be customised.
		/// </summary>
		public enum Actions
		{
			Right,
			Forward,
			PrimaryMouse,
			SecondaryMouse,
			ThirdMouse,
			Jump,
			Crouch,
			Sprint,
			FreeLook,
			Interact,
			Lean,
			Inventory,
			Journal,
			Drop,
			Equip,
			ChangeStance,
			EngineToggle,
			ThrustUp,
			ThustDown,
			Chat,
			Talk,
			Radio,
			TargetUp,
			TargetDown,
			FilterLeft,
			FilterRight,
			HelmetRadar,
			ToggleVisor,
			Melee,
			ToggleJetpack,
			MachVelocity,
			ToggleLights,
			WeaponMod,
			Quick1,
			Quick2,
			Quick3,
			Quick4
		}

		// TODO: Implement mouse sensitivity.
		public static float MouseSensitivity
		{
			get
			{
				return 1;
			}
			set
			{
			}
		}

		public static bool GetButton(Actions key)
		{
			return InputManager.GetButton(key.ToString());
		}

		public static bool GetButtonDown(Actions key)
		{
			return InputManager.GetButtonDown(key.ToString());
		}

		public static bool GetButtonUp(Actions key)
		{
			return InputManager.GetButtonUp(key.ToString());
		}

		public static float GetAxis(Actions name)
		{
			return InputManager.GetAxis(name.ToString());
		}

		public static float GetAxisRaw(Actions name)
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

		public static void LoadJSON()
		{
			InputManager.Load(new InputLoaderJSON());
		}

		public static void LoadDefaultJSON()
		{
			InputManager.Load(new InputDefaultLoaderJSON());
		}

		public static void ResetInputAxis()
		{
			InputManager.ResetInputAxes();
		}

		public static string GetAxisKeyName(Actions key, bool getPositive = true, bool getNegative = true, bool getAlt = false)
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
