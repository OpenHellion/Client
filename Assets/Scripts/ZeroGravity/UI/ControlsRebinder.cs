using System.Collections.Generic;
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

		public static List<ControlItem> Controls;

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

		public List<string> ControlsList = new List<string>();

		private string InputConfName = "KeyboardAndMouse";

		public InputAction actionsRev;

		private bool isPositiveRev;

		private bool isAltRev;

		public KeyCode oldKeyRev_p;

		public KeyCode oldKeyRev_n;

		public KeyCode oldKeyRev_ap;

		public KeyCode oldKeyRev_an;

		private ControlItem controlItemValRev;

		private InputAction actionsOld;

		private bool isPositiveOld;

		private bool isAltOld;

		public KeyCode oldKeyOld;

		private ControlItem controlItemValOld;

		private List<GameObject> AllElements = new List<GameObject>();

		public static void Initialize()
		{
			MovementControls = new List<ControlItem>
			{
				new ControlItem
				{
					Name = Localization.Forward.ToUpper(),
					Actions = InputController.Actions.Forward
				},
				new ControlItem
				{
					Name = Localization.Backward.ToUpper(),
					Actions = InputController.Actions.Forward,
					IsPositive = false
				},
				new ControlItem
				{
					Name = Localization.Right.ToUpper(),
					Actions = InputController.Actions.Right
				},
				new ControlItem
				{
					Name = Localization.Left.ToUpper(),
					Actions = InputController.Actions.Right,
					IsPositive = false
				},
				new ControlItem
				{
					Name = Localization.RotationClockwise.ToUpper(),
					Actions = InputController.Actions.Lean
				},
				new ControlItem
				{
					Name = Localization.RotationAnticlockwise.ToUpper(),
					Actions = InputController.Actions.Lean,
					IsPositive = false
				},
				new ControlItem
				{
					Name = Localization.Jump.ToUpper() + " / <color='#A0D3F8'>" + Localization.Up.ToUpper() + "</color>",
					Actions = InputController.Actions.Jump
				},
				new ControlItem
				{
					Name = Localization.Crouch.ToUpper() + " / <color='#A0D3F8'>" + Localization.Down.ToUpper() + "</color>",
					Actions = InputController.Actions.Crouch
				},
				new ControlItem
				{
					Name = Localization.Sprint.ToUpper() + " / <color='#A0D3F8'>" + Localization.Grab.ToUpper() + "</color> / " + Localization.Stabilization.ToUpper(),
					Actions = InputController.Actions.Sprint
				},
				new ControlItem
				{
					Name = Localization.FreeLook.ToUpper(),
					Actions = InputController.Actions.FreeLook
				}
			};

			ActionControls = new List<ControlItem>
			{
				new ControlItem
				{
					Name = Localization.Inventory.ToUpper() + " / <color='#A0D3F8'>" + Localization.ExitPanel.ToUpper() + "</color>",
					Actions = InputController.Actions.Inventory
				},
				new ControlItem
				{
					Name = Localization.Journal.ToUpper(),
					Actions = InputController.Actions.Journal
				},
				new ControlItem
				{
					Name = Localization.InteractTakeInHands.ToUpper(),
					Actions = InputController.Actions.Interact
				},
				new ControlItem
				{
					Name = Localization.DropThrow.ToUpper(),
					Actions = InputController.Actions.Drop
				},
				new ControlItem
				{
					Name = Localization.EquipItem.ToUpper() + " / <color='#A0D3F8'>" + Localization.Reload.ToUpper() + "</color> / " + Localization.ChangeDockingPort.ToUpper(),
					Actions = InputController.Actions.Equip
				},
				new ControlItem
				{
					Name = Localization.ChangeStance.ToUpper(),
					Actions = InputController.Actions.ChangeStance
				},
				new ControlItem
				{
					Name = Localization.ToggleLights.ToUpper(),
					Actions = InputController.Actions.ToggleLights
				},
				new ControlItem
				{
					Name = Localization.WeaponModKey.ToUpper(),
					Actions = InputController.Actions.WeaponMod
				},
				new ControlItem
				{
					Name = Localization.Melee.ToUpper(),
					Actions = InputController.Actions.Melee
				}
			};

			ShipControls = new List<ControlItem>
			{
				new ControlItem
				{
					Name = Localization.EngineToggle.ToUpper(),
					Actions = InputController.Actions.EngineToggle
				},
				new ControlItem
				{
					Name = Localization.EngineThrustUp.ToUpper(),
					Actions = InputController.Actions.ThrustUp
				},
				new ControlItem
				{
					Name = Localization.EngineThrustDown.ToUpper(),
					Actions = InputController.Actions.ThrustDown
				},
				new ControlItem
				{
					Name = Localization.MatchVelocityControl.ToUpper(),
					Actions = InputController.Actions.MachVelocity
				}
			};

			SuitControls = new List<ControlItem>
			{
				new ControlItem
				{
					Name = Localization.ToggleVisor.ToUpper(),
					Actions = InputController.Actions.ToggleVisor
				},
				new ControlItem
				{
					Name = Localization.ToggleJetpack.ToUpper(),
					Actions = InputController.Actions.ToggleJetpack
				},
				new ControlItem
				{
					Name = Localization.HelmetRadar.ToUpper(),
					Actions = InputController.Actions.HelmetRadar
				},
				new ControlItem
				{
					Name = Localization.TargetUp.ToUpper(),
					Actions = InputController.Actions.TargetUp
				},
				new ControlItem
				{
					Name = Localization.TargetDown.ToUpper(),
					Actions = InputController.Actions.TargetDown
				},
				new ControlItem
				{
					Name = Localization.FilterLeft.ToUpper(),
					Actions = InputController.Actions.FilterLeft
				},
				new ControlItem
				{
					Name = Localization.FilterRight.ToUpper(),
					Actions = InputController.Actions.FilterRight
				}
			};

			CommunicationControls = new List<ControlItem>
			{
				new ControlItem
				{
					Name = Localization.Chat.ToUpper(),
					Actions = InputController.Actions.Chat
				},
				new ControlItem
				{
					Name = Localization.Talk.ToUpper(),
					Actions = InputController.Actions.Talk
				},
				new ControlItem
				{
					Name = Localization.Radio.ToUpper(),
					Actions = InputController.Actions.Radio
				}
			};

			QuickActions = new List<ControlItem>
			{
				new ControlItem
				{
					Name = Localization.Quick1,
					Actions = InputController.Actions.Quick1
				},
				new ControlItem
				{
					Name = Localization.Quick2,
					Actions = InputController.Actions.Quick2
				},
				new ControlItem
				{
					Name = Localization.Quick3,
					Actions = InputController.Actions.Quick3
				},
				new ControlItem
				{
					Name = Localization.Quick4,
					Actions = InputController.Actions.Quick4
				}
			};
		}

		private void Awake()
		{
			Initialize();
			UpdateUI();
		}

		private void Start()
		{
		}

		private void OnEnable()
		{
		}

		public void UpdateUI()
		{
			foreach (GameObject allElement in AllElements)
			{
				Object.Destroy(allElement);
			}
			AllElements.Clear();
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
			AllElements.Add(controlPref);
			controlPref.transform.Find("ControlNameText").GetComponent<Text>().text = controlName.Name;
			controlPref.transform.localScale = new Vector3(1f, 1f, 1f);

			RebindInput rebinder = controlPref.transform.Find("Button").GetComponent<RebindInput>();
			Button button = controlPref.transform.Find("Button").GetComponent<Button>();
			rebinder.SetRebinder(InputConfName, controlName, rebinder.transform.Find("Text").GetComponent<Text>(), gameObject, isAlt: false);
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

			RebindInput altRebind = controlPref.transform.Find("AltButton").GetComponent<RebindInput>();
			Button altButton = controlPref.transform.Find("AltButton").GetComponent<Button>();
			altRebind.SetRebinder(InputConfName, controlName, altRebind.transform.Find("Text").GetComponent<Text>(), gameObject, isAlt: true);
			altButton.interactable = controlName.CanBeChanged;
			altButton.onClick.AddListener(altRebind.OnButtonPressed);

			buttonList.Add(new ButtonListItem
			{
				Button = altButton,
				ButtonObject = altRebind.gameObject,
				ButtonText = altRebind.transform.Find("Text").GetComponent<Text>(),
				IsAlt = true,
				ControlItem = altRebind.ControlItem
			});
		}

		private void SaveControlForRevert(InputAction actions, bool isPositive, bool isAlt, KeyCode oldKey, ControlItem controlItemVal, bool isPositiveR, bool isAltR)
		{
			actionsOld = actions;
			isPositiveOld = isPositive;
			isAltOld = isAlt;
			oldKeyOld = oldKey;
			controlItemValRev = controlItemVal;
			isPositiveRev = isPositiveR;
			isAltRev = isAltR;
		}

		public void DuplicateControlsYes()
		{
			GameMenu.DisableGameMenu = false;
		}

		public void DuplicateControlsNo()
		{
			for (int i = 0; i < InputManager.PlayerOneControlScheme.Actions.Count; i++)
			{
				if (InputManager.PlayerOneControlScheme.Actions[i] == actionsRev)
				{
					if (isPositiveRev && !isAltRev)
					{
						InputManager.PlayerOneControlScheme.Actions[i].Bindings[0].Positive = oldKeyRev_p;
					}
					else if (!isPositiveRev && !isAltRev)
					{
						InputManager.PlayerOneControlScheme.Actions[i].Bindings[0].Negative = oldKeyRev_n;
					}
					else if (isPositiveRev && isAltRev)
					{
						InputManager.PlayerOneControlScheme.Actions[i].Bindings[1].Positive = oldKeyRev_ap;
					}
					else if (!isPositiveRev && isAltRev)
					{
						InputManager.PlayerOneControlScheme.Actions[i].Bindings[1].Negative = oldKeyRev_an;
					}
				}
				if (InputManager.PlayerOneControlScheme.Actions[i] == actionsOld)
				{
					if (isPositiveOld && !isAltOld)
					{
						InputManager.PlayerOneControlScheme.Actions[i].Bindings[0].Positive = oldKeyOld;
					}
					else if (!isPositiveOld && !isAltOld)
					{
						InputManager.PlayerOneControlScheme.Actions[i].Bindings[0].Negative = oldKeyOld;
					}
					else if (isPositiveOld && isAltOld)
					{
						InputManager.PlayerOneControlScheme.Actions[i].Bindings[1].Positive = oldKeyOld;
					}
					else if (!isPositiveOld && isAltOld)
					{
						InputManager.PlayerOneControlScheme.Actions[i].Bindings[1].Negative = oldKeyOld;
					}
				}
			}
			foreach (ButtonListItem button in buttonList)
			{
				if (button.ControlItem == controlItemValRev)
				{
					if (!button.IsAlt && !isAltRev)
					{
						button.ButtonText.text = (!isPositiveRev) ? oldKeyRev_n.ToString() : oldKeyRev_p.ToString();
					}
					else if (button.IsAlt && isAltRev)
					{
						button.ButtonText.text = (!isPositiveRev) ? oldKeyRev_an.ToString() : oldKeyRev_ap.ToString();
					}
				}
				else if (button.ControlItem.Name == controlItemValOld.Name)
				{
					if (!isAltOld && !button.IsAlt)
					{
						button.ButtonText.text = oldKeyOld.ToString();
					}
					else if (isAltOld && button.IsAlt)
					{
						button.ButtonText.text = oldKeyOld.ToString();
					}
				}
			}
			GameMenu.DisableGameMenu = false;
		}

		public void OnKeyChange(KeyCode key, string AxisName, bool changePositive, bool changeAlt, ControlItem controlItemVal)
		{
			if (key == KeyCode.Escape)
			{
				return;
			}
			foreach (InputAction action in InputManager.PlayerOneControlScheme.Actions)
			{
				if (action.Bindings[0].Positive == key)
				{
					SaveControlForRevert(action, isPositive: true, isAlt: false, key, controlItemVal, changePositive, changeAlt);
					action.Bindings[0].Positive = KeyCode.None;
				}
				else if (action.Bindings[0].Negative == key)
				{
					SaveControlForRevert(action, isPositive: false, isAlt: false, key, controlItemVal, changePositive, changeAlt);
					action.Bindings[0].Negative = KeyCode.None;
				}
				else if (action.Bindings[1].Positive == key)
				{
					SaveControlForRevert(action, isPositive: true, isAlt: true, key, controlItemVal, changePositive, changeAlt);
					action.Bindings[1].Positive = KeyCode.None;
				}
				else if (action.Bindings[1].Negative == key)
				{
					SaveControlForRevert(action, isPositive: false, isAlt: true, key, controlItemVal, changePositive, changeAlt);
					action.Bindings[1].Negative = KeyCode.None;
				}
			}
			ButtonListItem buttonListItem = buttonList.Find((ButtonListItem m) => m.ButtonText.text == key.ToString());
			if (buttonListItem != null)
			{
				controlItemValOld = buttonListItem.ControlItem;
				buttonListItem.ButtonText.text = string.Empty;
				if (buttonListItem.ControlItem.Name != controlItemVal.Name)
				{
					GameMenu.DisableGameMenu = true;
					Client.Instance.ShowConfirmMessageBox(Localization.DuplicatedControl, string.Format(Localization.DuplicateControlMessage, buttonListItem.ControlItem.Name), Localization.Yes, Localization.No, DuplicateControlsYes, DuplicateControlsNo);
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
				Client.Instance.ShowMessageBox(Localization.PleaseAssignAllControls, text);
				return true;
			}
			return false;
		}
	}
}
