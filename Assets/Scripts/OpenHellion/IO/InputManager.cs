using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OpenHellion.IO
{
	/// <summary>
	/// 	Class to get keys that can be changed during runtime.
	/// </summary>
	public class InputManager : MonoBehaviour
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

		private static float m_MouseSensitivity;
		public static float SavedSensitivity
		{
			get
			{
				return m_MouseSensitivity;
			}
			set
			{
				m_MouseSensitivity = value;
				RealSensitivity = value;
			}
		}

		public static float RealSensitivity = SavedSensitivity;

		public InputActionAsset InputActions;

		public const string ActionMapName = "Main";

		private static InputManager s_instance;
		public static InputManager Instance
		{
			get
			{
				if (s_instance == null)
				{
					Dbg.Error("Tried to get input manager before it has been initialised.");
				}

				return s_instance;
			}
		}

		public void Awake()
		{
			if (s_instance != null)
			{
				Destroy(this);
				Dbg.Error("Input manager already exists.");
				return;
			}
			s_instance = this;

#if UNITY_EDITOR
			if (InputActions != null)
			{
				Dbg.Log("Saving default controls to", Application.dataPath + "/Resources/Data/ControlsDefault.json");
				File.WriteAllText(Application.dataPath + "/Resources/Data/ControlsDefault.json", InputActions.ToJson());
			}
#endif
			LoadDefaultConfig();
		}

		public static bool GetButton(ConfigAction key)
		{
			return Instance.InputActions.FindAction(key.ToString(), true).IsPressed();
		}

		public static bool GetButtonDown(ConfigAction key)
		{
			return Instance.InputActions.FindAction(key.ToString(), true).WasPressedThisFrame();
		}

		public static bool GetButtonUp(ConfigAction key)
		{
			return Instance.InputActions.FindAction(key.ToString(), true).WasReleasedThisFrame();
		}

		public static float GetAxis(ConfigAction name)
		{
			return Instance.InputActions.FindAction(name.ToString(), true).ReadValue<float>();
		}

		public static float GetAxisRaw(ConfigAction name)
		{
			return Instance.InputActions.FindAction(name.ToString(), true).ReadValue<float>();
		}

		/// <summary>
		/// 	Load the currently modified config and overwrite the default config.
		/// </summary>
		public static void LoadSavedConfig(string actionMap)
		{
			Dbg.Log("Loading custom input...");

			// Get saved controls.
			InputActionMap settingsControls = InputActionMap.FromJson(actionMap)[0];
			InputActionMap defaultControls = InputActionAsset.FromJson(Resources.Load("Data/ControlsDefault").ToString()).actionMaps[0];

			// If settings exist, proceed.
			if (File.Exists(Path.Combine(Application.persistentDataPath, "Settings.json")))
			{
				// Quality-check settings.
				if (settingsControls.actions.Count > 0)
				{
					List<string> settingsControlsNames = new List<string>();
					List<string> defaultControlsNames = new List<string>();
					foreach (InputAction action in settingsControls.actions)
					{
						settingsControlsNames.Add(action.name);
					}
					foreach (InputAction action in defaultControls.actions)
					{
						defaultControlsNames.Add(action.name);
					}

					// Extra entries in the settings list.
					List<string> extraEntries = settingsControlsNames.Except(defaultControlsNames).ToList();
					if (extraEntries.Count > 0)
					{
						for (int k = 0; k < extraEntries.Count; k++)
						{
							for (int l = 0; l < settingsControls.actions.Count; l++)
							{
								// This action exists in extra entries.
								if (settingsControls.actions[l].name == extraEntries[k])
								{
									settingsControls.actions[l].RemoveAction();
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
									InputAction action = settingsControls.AddAction(defaultControls.actions[m].name);
									foreach (InputBinding binding in defaultControls.actions[m].bindings)
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
					settingsControls = defaultControls;
				}

				Instance.InputActions.FindActionMap(ActionMapName).Disable();
				Instance.InputActions.RemoveActionMap(ActionMapName);
				Instance.InputActions.AddActionMap(settingsControls);
				settingsControls.Enable();
			}
		}

		public static void LoadDefaultConfig()
		{
			Instance.InputActions = InputActionAsset.FromJson(Resources.Load("Data/ControlsDefault").ToString());
			Instance.InputActions.Enable();
		}

		public static void ResetInputAxis()
		{
			Debug.Log("Reset input axes!");
			//InputManager.ResetInputAxes();
		}

		public static string GetAxisKeyName(ConfigAction key, bool getPositive = false, bool getNegative = false)
		{
			InputAction action = Instance.InputActions.FindAction(key.ToString());


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
