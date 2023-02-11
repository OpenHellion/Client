using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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

		public InputActionAsset InputActions = Resources.Load<InputActionAsset>("InputConfig");

		public const string ActionMapName = "Main";

		private static InputController _instance;
		public static InputController Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new InputController();
					_instance.InputActions.Enable();
				}

				return _instance;
			}
		}

		public static bool GetButton(Actions key)
		{
			return Instance.InputActions.FindAction(key.ToString(), true).IsPressed();
		}

		public static bool GetButtonDown(Actions key)
		{
			return Instance.InputActions.FindAction(key.ToString(), true).WasPressedThisFrame();
		}

		public static bool GetButtonUp(Actions key)
		{
			return Instance.InputActions.FindAction(key.ToString(), true).WasReleasedThisFrame();
		}

		public static float GetAxis(Actions name)
		{
			return Instance.InputActions.FindAction(name.ToString(), true).ReadValue<float>();
		}

		public static float GetAxisRaw(Actions name)
		{
			return Instance.InputActions.FindAction(name.ToString(), true).ReadValue<float>();
		}

		/// <summary>
		/// 	Load the currently modified config and overwrite the default config.
		/// </summary>
		public static void LoadSavedConfig(InputActionMap actionMap)
		{
			Dbg.Log("Loading custom input...");

			// Get saved controls.
			InputActionMap defaultControls = InputActionMap.FromJson(File.ReadAllText(Path.Combine(Application.persistentDataPath, "Data/ControlsDefault")))[0];
			if (File.Exists(Path.Combine(Application.persistentDataPath, "Settings.json")))
			{
				// Quality-check settings.
				if (actionMap.actions.Count > 0)
				{
					List<string> settingsControlsNames = new List<string>();
					List<string> defaultControlsNames = new List<string>();
					for (int i = 0; i < actionMap.actions.Count; i++)
					{
						settingsControlsNames.Add(actionMap.actions[i].name);
					}
					for (int j = 0; j < defaultControls.actions.Count; j++)
					{
						defaultControlsNames.Add(defaultControls.actions[j].name);
					}

					// Extra entries in the settings list.
					List<string> extraEntries = settingsControlsNames.Except(defaultControlsNames).ToList();
					if (extraEntries.Count > 0)
					{
						for (int k = 0; k < extraEntries.Count; k++)
						{
							for (int l = 0; l < actionMap.actions.Count; l++)
							{
								if (actionMap.actions[l].name == extraEntries[k])
								{
									actionMap.actions[l].RemoveAction();
								}
							}
						}
					}

					// Entries missing from the settings list.
					List<string> missingEntries = defaultControlsNames.Except(settingsControlsNames).ToList();
					if (missingEntries.Count > 0)
					{
						foreach (string item in missingEntries)
						{
							for (int m = 0; m < defaultControls.actions.Count; m++)
							{
								if (defaultControls.actions[m].name == item)
								{
									InputAction action = actionMap.AddAction(defaultControls.actions[m].name);
									foreach (InputBinding binding in defaultControls.actions[m].bindings)
									{
										action.AddBinding(binding);
									}
								}
							}
						}
					}
				}
			}
			else
			{
				actionMap = defaultControls;
			}

			Instance.InputActions.RemoveActionMap(ActionMapName);
			Instance.InputActions.AddActionMap(actionMap);
			actionMap.Enable();
		}

		public static void LoadDefaultConfig()
		{
			Instance.InputActions.RemoveActionMap(ActionMapName);
			Instance.InputActions.AddActionMap(InputActionMap.FromJson(File.ReadAllText(Path.Combine(Application.persistentDataPath, "Data/ControlsDefault")))[0]);
		}

		public static void ResetInputAxis()
		{
			//InputManager.ResetInputAxes();
		}

		public static string GetAxisKeyName(Actions key, bool getAlt = false)
		{
			InputAction actions = Instance.InputActions.FindAction(key.ToString());
			//if (!getAlt)
			{
				// Get main binding.
				return actions.bindings[0].ToString();
			}
			/*else
			{
				// Get alternative binding.
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

			return string.Empty;*/
		}
	}
}
