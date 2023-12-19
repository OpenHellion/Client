using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OpenHellion.IO
{
	/// <summary>
	/// 	Class to get keys that can be changed during runtime.
	///		Has the responsibility of keeping track of and storing key bindings as well as keeping track of input.
	/// </summary>
	public static class ControlsSubsystem
	{
		/// <summary>
		/// 	Keys that can be customised.
		/// </summary>
		public enum ConfigAction
		{
			Right,
			Forward,
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
			ThrustDown,
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
			MatchVelocity,
			ToggleLights,
			WeaponMod,
			Quick1,
			Quick2,
			Quick3,
			Quick4
		}

		public static float RealSensitivity { get; set; }

		public static InputActionAsset InputActions { get; private set; }

		public const string ActionMapName = "Main";

		public static void Reset()
		{
			InputActions = InputActionAsset.FromJson(Resources.Load("Data/ControlsDefault").ToString());
			InputActions.Enable();
		}

		public static bool GetButton(ConfigAction key)
		{
			return InputActions.FindAction(key.ToString(), true).IsPressed();
		}

		public static bool GetButtonDown(ConfigAction key)
		{
			return InputActions.FindAction(key.ToString(), true).WasPressedThisFrame();
		}

		public static bool GetButtonUp(ConfigAction key)
		{
			return InputActions.FindAction(key.ToString(), true).WasReleasedThisFrame();
		}

		public static float GetAxis(ConfigAction name)
		{
			return InputActions.FindAction(name.ToString(), true).ReadValue<float>();
		}

		public static float GetAxisRaw(ConfigAction name)
		{
			return InputActions.FindAction(name.ToString(), true).ReadValue<float>();
		}

		/// <summary>
		/// 	Load the currently modified config and overwrite the default config.
		/// </summary>
		public static void LoadSavedConfig(string actionMap)
		{
			Debug.Log("Loading custom input...");

			// Get saved controls.
			InputActionMap loadedControls = InputActionMap.FromJson(actionMap)[0];
			InputActionMap defaultControls =
				InputActionAsset.FromJson(Resources.Load("Data/ControlsDefault").ToString()).actionMaps[0];

			// If settings exist, proceed.
			if (File.Exists(Path.Combine(Application.persistentDataPath, "Settings.json")))
			{
				// Quality-check settings.
				if (loadedControls.actions.Count > 0)
				{
					List<string> settingsControlsNames = loadedControls.actions.Select(action => action.name).ToList();
					List<string> defaultControlsNames = defaultControls.actions.Select(action => action.name).ToList();

					// Extra entries in the settings list.
					List<string> extraEntries = settingsControlsNames.Except(defaultControlsNames).ToList();
					if (extraEntries.Count > 0)
					{
						foreach (var loadedAction in extraEntries.SelectMany(extraEntry => loadedControls.actions.Where(loadedEntry => loadedEntry.name == extraEntry)))
						{
							loadedAction.RemoveAction();
						}
					}

					// Entries missing from the settings list.
					List<string> missingEntries = defaultControlsNames.Except(settingsControlsNames).ToList();
					if (missingEntries.Count > 0)
					{
						foreach (string missingAction in missingEntries)
						{
							foreach (var regularAction in defaultControls.actions)
							{
								if (regularAction.name == missingAction)
								{
									InputAction action = loadedControls.AddAction(regularAction.name);
									foreach (InputBinding binding in regularAction.bindings)
									{
										action.AddBinding(binding);
									}
								}
							}
						}
					}
				}
				else
				{
					loadedControls = defaultControls;
				}

				InputActions.FindActionMap(ActionMapName).Disable();
				InputActions.RemoveActionMap(ActionMapName);
				InputActions.AddActionMap(loadedControls);
				loadedControls.Enable();
			}
		}

		public static void ResetInputAxis()
		{
			Debug.Log("Reset input axes!");
			//InputManager.ResetInputAxes();
		}

		public static string GetAxisKeyName(ConfigAction key, bool getPositive = false, bool getNegative = false)
		{
			InputAction action = InputActions.FindAction(key.ToString());


			// Get main binding.
			if (getPositive && getNegative || !getPositive && !getNegative)
			{
				return action.GetBindingDisplayString();
			}

			// Get composite bindings.
			foreach (InputBinding binding in action.bindings)
			{
				if (!binding.isPartOfComposite) continue;

				if (getPositive && binding.name == "positive")
				{
					return binding.ToDisplayString();
				}

				if (getNegative && binding.name == "negative")
				{
					return binding.ToDisplayString();
				}
			}

			return string.Empty;
		}
	}
}
