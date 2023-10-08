using System.Collections.Generic;
using OpenHellion;
using OpenHellion.IO;
using OpenHellion.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class ControlsRebinder : MonoBehaviour
	{
		public class ButtonListItem
		{
			public GameObject ButtonObject;

			public Button Button;

			public Text ButtonText;

			public bool IsAlt;

			public ControlItem ControlItem;
		}

		public static List<ControlItem> MovementControls;

		public static List<ControlItem> ShipControls;

		public static List<ControlItem> ActionControls;

		public static List<ControlItem> SuitControls;

		public static List<ControlItem> CommunicationControls;

		public static List<ControlItem> QuickActions;

		public Transform MovementHolder;

		public Transform ShipHolder;

		public Transform ActionHolder;

		public Transform SuitHolder;

		public Transform CommsHolder;

		public Transform QuickHolder;

		public bool isScanning;

		public string WhoIsScanning = string.Empty;

		public List<ButtonListItem> buttonList = new List<ButtonListItem>();

		public GameObject ControlPref;

		public InputAction _actionsRev;

		private bool _isPositiveRev;

		private bool _isAltRev;

		public KeyCode OldKeyRev_p;

		public KeyCode OldKeyRev_n;

		public KeyCode OldKeyRev_ap;

		public KeyCode OldKeyRev_an;

		private ControlItem _controlItemValRev;

		private InputAction _actionsOld;

		private bool _isPositiveOld;

		private bool _isAltOld;

		public KeyCode OldKeyOld;

		private ControlItem _controlItemValOld;

		private List<GameObject> _allElements = new List<GameObject>();

		public static void Initialize()
		{
			MovementControls = new List<ControlItem>
			{
				new ControlItem
				{
					Name = Localization.Forward.ToUpper(),
					Action = InputManager.ConfigAction.Forward
				},
				new ControlItem
				{
					Name = Localization.Backward.ToUpper(),
					Action = InputManager.ConfigAction.Forward,
					IsPositive = false
				},
				new ControlItem
				{
					Name = Localization.Right.ToUpper(),
					Action = InputManager.ConfigAction.Right
				},
				new ControlItem
				{
					Name = Localization.Left.ToUpper(),
					Action = InputManager.ConfigAction.Right,
					IsPositive = false
				},
				new ControlItem
				{
					Name = Localization.RotationClockwise.ToUpper(),
					Action = InputManager.ConfigAction.Lean
				},
				new ControlItem
				{
					Name = Localization.RotationAnticlockwise.ToUpper(),
					Action = InputManager.ConfigAction.Lean,
					IsPositive = false
				},
				new ControlItem
				{
					Name = Localization.Jump.ToUpper() + " / <color='#A0D3F8'>" + Localization.Up.ToUpper() +
					       "</color>",
					Action = InputManager.ConfigAction.Jump
				},
				new ControlItem
				{
					Name = Localization.Crouch.ToUpper() + " / <color='#A0D3F8'>" + Localization.Down.ToUpper() +
					       "</color>",
					Action = InputManager.ConfigAction.Crouch
				},
				new ControlItem
				{
					Name = Localization.Sprint.ToUpper() + " / <color='#A0D3F8'>" + Localization.Grab.ToUpper() +
					       "</color> / " + Localization.Stabilization.ToUpper(),
					Action = InputManager.ConfigAction.Sprint
				},
				new ControlItem
				{
					Name = Localization.FreeLook.ToUpper(),
					Action = InputManager.ConfigAction.FreeLook
				}
			};

			ActionControls = new List<ControlItem>
			{
				new ControlItem
				{
					Name = Localization.Inventory.ToUpper() + " / <color='#A0D3F8'>" +
					       Localization.ExitPanel.ToUpper() + "</color>",
					Action = InputManager.ConfigAction.Inventory
				},
				new ControlItem
				{
					Name = Localization.Journal.ToUpper(),
					Action = InputManager.ConfigAction.Journal
				},
				new ControlItem
				{
					Name = Localization.InteractTakeInHands.ToUpper(),
					Action = InputManager.ConfigAction.Interact
				},
				new ControlItem
				{
					Name = Localization.DropThrow.ToUpper(),
					Action = InputManager.ConfigAction.Drop
				},
				new ControlItem
				{
					Name = Localization.EquipItem.ToUpper() + " / <color='#A0D3F8'>" + Localization.Reload.ToUpper() +
					       "</color> / " + Localization.ChangeDockingPort.ToUpper(),
					Action = InputManager.ConfigAction.Equip
				},
				new ControlItem
				{
					Name = Localization.ChangeStance.ToUpper(),
					Action = InputManager.ConfigAction.ChangeStance
				},
				new ControlItem
				{
					Name = Localization.ToggleLights.ToUpper(),
					Action = InputManager.ConfigAction.ToggleLights
				},
				new ControlItem
				{
					Name = Localization.WeaponModKey.ToUpper(),
					Action = InputManager.ConfigAction.WeaponMod
				},
				new ControlItem
				{
					Name = Localization.Melee.ToUpper(),
					Action = InputManager.ConfigAction.Melee
				}
			};

			ShipControls = new List<ControlItem>
			{
				new ControlItem
				{
					Name = Localization.EngineToggle.ToUpper(),
					Action = InputManager.ConfigAction.EngineToggle
				},
				new ControlItem
				{
					Name = Localization.EngineThrustUp.ToUpper(),
					Action = InputManager.ConfigAction.ThrustUp
				},
				new ControlItem
				{
					Name = Localization.EngineThrustDown.ToUpper(),
					Action = InputManager.ConfigAction.ThrustDown
				},
				new ControlItem
				{
					Name = Localization.MatchVelocityControl.ToUpper(),
					Action = InputManager.ConfigAction.MatchVelocity
				}
			};

			SuitControls = new List<ControlItem>
			{
				new ControlItem
				{
					Name = Localization.ToggleVisor.ToUpper(),
					Action = InputManager.ConfigAction.ToggleVisor
				},
				new ControlItem
				{
					Name = Localization.ToggleJetpack.ToUpper(),
					Action = InputManager.ConfigAction.ToggleJetpack
				},
				new ControlItem
				{
					Name = Localization.HelmetRadar.ToUpper(),
					Action = InputManager.ConfigAction.HelmetRadar
				},
				new ControlItem
				{
					Name = Localization.TargetUp.ToUpper(),
					Action = InputManager.ConfigAction.TargetUp
				},
				new ControlItem
				{
					Name = Localization.TargetDown.ToUpper(),
					Action = InputManager.ConfigAction.TargetDown
				},
				new ControlItem
				{
					Name = Localization.FilterLeft.ToUpper(),
					Action = InputManager.ConfigAction.FilterLeft
				},
				new ControlItem
				{
					Name = Localization.FilterRight.ToUpper(),
					Action = InputManager.ConfigAction.FilterRight
				}
			};

			CommunicationControls = new List<ControlItem>
			{
				new ControlItem
				{
					Name = Localization.Chat.ToUpper(),
					Action = InputManager.ConfigAction.Chat
				},
				new ControlItem
				{
					Name = Localization.Talk.ToUpper(),
					Action = InputManager.ConfigAction.Talk
				},
				new ControlItem
				{
					Name = Localization.Radio.ToUpper(),
					Action = InputManager.ConfigAction.Radio
				}
			};

			QuickActions = new List<ControlItem>
			{
				new ControlItem
				{
					Name = Localization.Quick1,
					Action = InputManager.ConfigAction.Quick1
				},
				new ControlItem
				{
					Name = Localization.Quick2,
					Action = InputManager.ConfigAction.Quick2
				},
				new ControlItem
				{
					Name = Localization.Quick3,
					Action = InputManager.ConfigAction.Quick3
				},
				new ControlItem
				{
					Name = Localization.Quick4,
					Action = InputManager.ConfigAction.Quick4
				}
			};
		}

		private void Awake()
		{
			Initialize();
			UpdateUI();
		}

		public void UpdateUI()
		{
			foreach (GameObject allElement in _allElements)
			{
				Destroy(allElement);
			}

			_allElements.Clear();
			InstantiateKeyboardControls();
		}

		public void EnableAllButtons(bool val)
		{
			foreach (ButtonListItem button in buttonList)
			{
				if (button.ControlItem.CanBeChanged)
				{
					button.Button.interactable = val;
				}
			}
		}

		private void InstantiateKeyboardControls()
		{
			foreach (ControlItem movementControl in MovementControls)
			{
				InstantiateControlPref(movementControl, MovementHolder);
			}

			foreach (ControlItem actionControl in ActionControls)
			{
				InstantiateControlPref(actionControl, ActionHolder);
			}

			foreach (ControlItem shipControl in ShipControls)
			{
				InstantiateControlPref(shipControl, ShipHolder);
			}

			foreach (ControlItem suitControl in SuitControls)
			{
				InstantiateControlPref(suitControl, SuitHolder);
			}

			foreach (ControlItem communicationControl in CommunicationControls)
			{
				InstantiateControlPref(communicationControl, CommsHolder);
			}

			foreach (ControlItem quickAction in QuickActions)
			{
				InstantiateControlPref(quickAction, QuickHolder);
			}
		}

		public void InstantiateControlPref(ControlItem controlName, Transform holder)
		{
			GameObject controlPref = Instantiate(ControlPref, holder);
			_allElements.Add(controlPref);
			controlPref.transform.Find("ControlNameText").GetComponent<Text>().text = controlName.Name;
			controlPref.transform.localScale = new Vector3(1f, 1f, 1f);

			RebindInput rebinder = controlPref.transform.Find("Button").GetComponent<RebindInput>();
			Button button = rebinder.GetComponent<Button>();
			rebinder.SetRebinder(controlName, rebinder.transform.Find("Text").GetComponent<Text>(), gameObject);
			button.interactable = controlName.CanBeChanged;
			button.onClick.AddListener(rebinder.OnButtonPressed);

			buttonList.Add(new ButtonListItem
			{
				Button = button,
				ButtonObject = rebinder.gameObject,
				ButtonText = rebinder.transform.Find("Text").GetComponent<Text>(),
				IsAlt = false,
				ControlItem = rebinder.ControlItem
			});

			controlPref.transform.name = controlName.Name;
		}

		private void SaveControlForRevert(InputAction actions, bool isPositive, bool isAlt, KeyCode oldKey,
			ControlItem controlItemVal, bool isPositiveR, bool isAltR)
		{
			_actionsOld = actions;
			_isPositiveOld = isPositive;
			_isAltOld = isAlt;
			OldKeyOld = oldKey;
			_controlItemValRev = controlItemVal;
			_isPositiveRev = isPositiveR;
			_isAltRev = isAltR;
		}

		public void DuplicateControlsYes()
		{
			PauseMenu.DisableGameMenu = false;
		}

		public void DuplicateControlsNo()
		{
			for (int i = 0; i < InputManager.Instance.InputActions.actionMaps[0].actions.Count; i++)
			{
				/*if (InputController.Instance.InputActions.actionMaps[0].actions[i] == actionsRev)
				{
					if (isPositiveRev && !isAltRev)
					{
						InputController.Instance.InputActions.actionMaps[0].actions[i].bindings[0].Positive = oldKeyRev_p;
					}
					else if (!isPositiveRev && !isAltRev)
					{
						InputController.Instance.InputActions.actionMaps[0].actions[i].bindings[0].Negative = oldKeyRev_n;
					}
					else if (isPositiveRev && isAltRev)
					{
						InputController.Instance.InputActions.actionMaps[0].actions[i].bindings[1].Positive = oldKeyRev_ap;
					}
					else if (!isPositiveRev && isAltRev)
					{
						InputController.Instance.InputActions.actionMaps[0].actions[i].bindings[1].Negative = oldKeyRev_an;
					}
				}
				if (InputController.Instance.InputActions.actionMaps[0].actions[i] == actionsOld)
				{
					if (isPositiveOld && !isAltOld)
					{
						InputController.Instance.InputActions.actionMaps[0].actions[i].bindings[0].Positive = oldKeyOld;
					}
					else if (!isPositiveOld && !isAltOld)
					{
						InputController.Instance.InputActions.actionMaps[0].actions[i].bindings[0].Negative = oldKeyOld;
					}
					else if (isPositiveOld && isAltOld)
					{
						InputController.Instance.InputActions.actionMaps[0].actions[i].bindings[1].Positive = oldKeyOld;
					}
					else if (!isPositiveOld && isAltOld)
					{
						InputController.Instance.InputActions.actionMaps[0].actions[i].bindings[1].Negative = oldKeyOld;
					}
				}*/
			}

			foreach (ButtonListItem button in buttonList)
			{
				if (button.ControlItem == _controlItemValRev)
				{
					if (!button.IsAlt && !_isAltRev)
					{
						button.ButtonText.text = (!_isPositiveRev) ? OldKeyRev_n.ToString() : OldKeyRev_p.ToString();
					}
					else if (button.IsAlt && _isAltRev)
					{
						button.ButtonText.text = (!_isPositiveRev) ? OldKeyRev_an.ToString() : OldKeyRev_ap.ToString();
					}
				}
				else if (button.ControlItem.Name == _controlItemValOld.Name)
				{
					if (!_isAltOld && !button.IsAlt)
					{
						button.ButtonText.text = OldKeyOld.ToString();
					}
					else if (_isAltOld && button.IsAlt)
					{
						button.ButtonText.text = OldKeyOld.ToString();
					}
				}
			}

			PauseMenu.DisableGameMenu = false;
		}

		public void OnKeyChange(KeyCode key, string AxisName, bool changePositive, bool changeAlt,
			ControlItem controlItemVal)
		{
			if (key == KeyCode.Escape)
			{
				return;
			}

			/*foreach (InputAction action in InputController.Instance.InputActions.actionMaps[0].actions)
			{
				if (action.bindings[0].Positive == key)
				{
					SaveControlForRevert(action, isPositive: true, isAlt: false, key, controlItemVal, changePositive, changeAlt);
					action.bindings[0].Positive = KeyCode.None;
				}
				else if (action.bindings[0].Negative == key)
				{
					SaveControlForRevert(action, isPositive: false, isAlt: false, key, controlItemVal, changePositive, changeAlt);
					action.bindings[0].Negative = KeyCode.None;
				}
				else if (action.bindings[1].Positive == key)
				{
					SaveControlForRevert(action, isPositive: true, isAlt: true, key, controlItemVal, changePositive, changeAlt);
					action.bindings[1].Positive = KeyCode.None;
				}
				else if (action.bindings[1].Negative == key)
				{
					SaveControlForRevert(action, isPositive: false, isAlt: true, key, controlItemVal, changePositive, changeAlt);
					action.bindings[1].Negative = KeyCode.None;
				}
			}*/
			ButtonListItem buttonListItem = buttonList.Find((ButtonListItem m) => m.ButtonText.text == key.ToString());
			if (buttonListItem != null)
			{
				_controlItemValOld = buttonListItem.ControlItem;
				buttonListItem.ButtonText.text = string.Empty;
				if (buttonListItem.ControlItem.Name != controlItemVal.Name)
				{
					PauseMenu.DisableGameMenu = true;
					GlobalGUI.ShowConfirmMessageBox(Localization.DuplicatedControl,
						string.Format(Localization.DuplicateControlMessage, buttonListItem.ControlItem.Name),
						Localization.Yes, Localization.No, DuplicateControlsYes, DuplicateControlsNo);
					return;
				}
			}

			if (!CheckIfEmpty())
			{
				Settings.Instance.SaveSettings(Settings.SettingsType.Controls);
			}
		}

		public bool CheckIfEmpty()
		{
			List<ButtonListItem> list = new List<ButtonListItem>();
			foreach (ButtonListItem button in buttonList)
			{
				if (!button.IsAlt && button.ButtonText.text == string.Empty)
				{
					list.Add(button);
				}
			}

			if (list.Count > 0)
			{
				string text = string.Empty;
				foreach (ButtonListItem item in list)
				{
					text = text + "- " + item.ControlItem.Name + "\n";
				}

				GlobalGUI.ShowMessageBox(Localization.PleaseAssignAllControls, text);
				return true;
			}

			return false;
		}
	}
}
