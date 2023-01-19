using System.Collections;
using Luminosity.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	[RequireComponent(typeof(Image))]
	public class RebindInput : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
	{
		public enum RebindType
		{
			Keyboard = 0,
			GamepadButton = 1,
			GamepadAxis = 2
		}

		public GameObject MainPanel;

		public GameObject Button;

		public ControlItem ControlItem;

		[SerializeField]
		private Sprite m_normalState;

		[SerializeField]
		private Sprite m_scanningState;

		public Text m_keyDescription;

		[SerializeField]
		private string m_inputConfigName;

		[SerializeField]
		private string m_axisConfigName;

		[SerializeField]
		private KeyCode m_cancelButton;

		[SerializeField]
		private float m_timeout;

		[SerializeField]
		private bool m_changePositiveKey;

		[SerializeField]
		private bool m_changeAltKey;

		[SerializeField]
		private bool m_allowAnalogButton;

		[SerializeField]
		[Range(0f, 4f)]
		private int m_joystick;

		[SerializeField]
		private RebindType m_rebindType;

		public InputAction m_axisConfig;

		private Image m_image;

		private static string[] m_axisNames = new string[10] { "X", "Y", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th" };

		public void SetRebinder(string inputConfigName, ControlItem contItem, Text buttonText, GameObject mainPanel, bool isAlt)
		{
			m_inputConfigName = inputConfigName;
			m_axisConfigName = contItem.Axis.ToString();
			m_changePositiveKey = contItem.IsPositive;
			m_changeAltKey = isAlt;
			m_cancelButton = KeyCode.Escape;
			MainPanel = mainPanel;
			m_keyDescription = buttonText;
			ControlItem = contItem;
			m_timeout = 10f;
		}

		private void Awake()
		{
			Button = base.transform.gameObject;
		}

		private void Start()
		{
			m_image = GetComponent<Image>();
			m_image.overrideSprite = m_normalState;
			InitializeAxisConfig();
			Luminosity.IO.InputManager.Loaded += InitializeAxisConfig;
			Luminosity.IO.InputManager.PlayerControlsChanged += HandleConfigurationDirty;
		}

		private void OnDestroy()
		{
			Luminosity.IO.InputManager.Loaded -= InitializeAxisConfig;
			Luminosity.IO.InputManager.PlayerControlsChanged -= HandleConfigurationDirty;
		}

		private void InitializeAxisConfig()
		{
			m_axisConfig = Luminosity.IO.InputManager.GetAction(m_inputConfigName, m_axisConfigName);
			if (m_axisConfig != null)
			{
				if (m_rebindType == RebindType.Keyboard || m_rebindType == RebindType.GamepadButton)
				{
					if (m_changePositiveKey)
					{
						if (m_changeAltKey)
						{
							m_keyDescription.text = ((m_axisConfig.Bindings[1].Positive != 0) ? m_axisConfig.Bindings[1].Positive.ToString() : string.Empty);
						}
						else
						{
							m_keyDescription.text = ((m_axisConfig.Bindings[0].Positive != 0) ? m_axisConfig.Bindings[0].Positive.ToString() : string.Empty);
						}
					}
					else if (m_changeAltKey)
					{
						m_keyDescription.text = ((m_axisConfig.Bindings[1].Negative != 0) ? m_axisConfig.Bindings[1].Negative.ToString() : string.Empty);
					}
					else
					{
						m_keyDescription.text = ((m_axisConfig.Bindings[0].Negative != 0) ? m_axisConfig.Bindings[0].Negative.ToString() : string.Empty);
					}
				}
				else
				{
					m_keyDescription.text = m_axisNames[m_axisConfig.Bindings[0].Axis];
				}
			}
			else
			{
				m_keyDescription.text = string.Empty;
				Debug.LogError(string.Format("Input configuration '{0}' does not exist or axis '{1}' does not exist", m_inputConfigName, m_axisConfigName));
			}
		}

		private void HandleConfigurationDirty(PlayerID configName)
		{
			if (Luminosity.IO.InputManager.GetControlScheme(configName).Name == m_inputConfigName)
			{
				InitializeAxisConfig();
			}
		}

		public void OnPointerDown(PointerEventData data)
		{
		}

		public void OnButtonPressed()
		{
			if (MainPanel.GetComponent<ControlsRebinder>().WhoIsScanning == string.Empty)
			{
				MainPanel.GetComponent<ControlsRebinder>().oldKeyRev_p = m_axisConfig.Bindings[0].Positive;
				MainPanel.GetComponent<ControlsRebinder>().oldKeyRev_n = m_axisConfig.Bindings[0].Negative;
				MainPanel.GetComponent<ControlsRebinder>().oldKeyRev_ap = m_axisConfig.Bindings[1].Positive;
				MainPanel.GetComponent<ControlsRebinder>().oldKeyRev_an = m_axisConfig.Bindings[1].Negative;
				MainPanel.GetComponent<ControlsRebinder>().actionsRev = m_axisConfig;
				MainPanel.GetComponent<ControlsRebinder>().EnableAllButtons(false);
				MainPanel.GetComponent<ControlsRebinder>().isScanning = true;
				MainPanel.GetComponent<ControlsRebinder>().WhoIsScanning = base.transform.name;
				StartCoroutine(StartInputScanDelayed());
			}
		}

		private IEnumerator StartInputScanDelayed()
		{
			yield return null;
			if (Luminosity.IO.InputManager.IsScanning || m_axisConfig == null)
			{
				yield break;
			}
			m_image.overrideSprite = m_scanningState;
			m_keyDescription.text = "...";
			ScanSettings settings = default;
			settings.Joystick = m_joystick;
			settings.CancelScanKey = m_cancelButton;
			settings.Timeout = m_timeout;
			settings.UserData = null;
			if (m_rebindType == RebindType.GamepadAxis)
			{
				settings.ScanFlags = ScanFlags.JoystickAxis;
				Luminosity.IO.InputManager.StartInputScan(settings, HandleJoystickAxisScan);
			}
			else if (m_rebindType == RebindType.GamepadButton)
			{
				settings.ScanFlags = (ScanFlags)6;
				if (m_rebindType == RebindType.GamepadButton && m_allowAnalogButton)
				{
					settings.ScanFlags |= ScanFlags.JoystickAxis;
				}
				Luminosity.IO.InputManager.StartInputScan(settings, HandleJoystickButtonScan);
			}
			else
			{
				settings.ScanFlags = ScanFlags.Key;
				Luminosity.IO.InputManager.StartInputScan(settings, HandleKeyScan);
			}
		}

		private bool HandleKeyScan(ScanResult result)
		{
			if (!IsKeyValid(result.Key))
			{
				return false;
			}
			MainPanel.GetComponent<ControlsRebinder>().OnKeyChange(result.Key, m_axisConfigName, m_changePositiveKey, m_changeAltKey, ControlItem);
			StartCoroutine(WaitAfterScan(0.4f));
			MainPanel.GetComponent<ControlsRebinder>().isScanning = false;
			MainPanel.GetComponent<ControlsRebinder>().WhoIsScanning = string.Empty;
			if (result.Key != 0)
			{
				result.Key = (result.Key != KeyCode.Backspace) ? result.Key : KeyCode.None;
				if (m_changePositiveKey)
				{
					if (m_changeAltKey)
					{
						m_axisConfig.Bindings[1].Positive = result.Key;
					}
					else
					{
						m_axisConfig.Bindings[0].Positive = result.Key;
					}
				}
				else if (m_changeAltKey)
				{
					m_axisConfig.Bindings[1].Negative = result.Key;
				}
				else
				{
					m_axisConfig.Bindings[0].Negative = result.Key;
				}
				m_keyDescription.text = (result.Key != 0) ? result.Key.ToString() : string.Empty;
			}
			else
			{
				KeyCode currentKeyCode = GetCurrentKeyCode();
				m_keyDescription.text = ((currentKeyCode != 0) ? currentKeyCode.ToString() : string.Empty);
			}
			return true;
		}

		private bool IsKeyValid(KeyCode key)
		{
			bool result = true;
			if (m_rebindType == RebindType.Keyboard)
			{
				if (key >= KeyCode.JoystickButton0)
				{
					result = false;
				}
				else
				{
					switch (key)
					{
					case KeyCode.RightCommand:
					case KeyCode.LeftCommand:
						result = false;
						break;
					case KeyCode.LeftWindows:
					case KeyCode.RightWindows:
						result = false;
						break;
					case KeyCode.Escape:
						result = false;
						break;
					}
				}
			}
			else
			{
				result = false;
			}
			return result;
		}

		private IEnumerator WaitAfterScan(float waitTime)
		{
			yield return new WaitForSeconds(waitTime);
			MainPanel.GetComponent<ControlsRebinder>().EnableAllButtons(true);
		}

		private bool HandleJoystickButtonScan(ScanResult result)
		{
			if (result.ScanFlags == ScanFlags.Key || result.ScanFlags == ScanFlags.JoystickButton)
			{
				if (!IsJoytickButtonValid(result.Key))
				{
					return false;
				}
				if (result.Key != 0)
				{
					result.Key = ((result.Key != KeyCode.Backspace) ? result.Key : KeyCode.None);
					m_axisConfig.Bindings[0].Type = InputType.Button;
					if (m_changePositiveKey)
					{
						if (m_changeAltKey)
						{
							m_axisConfig.Bindings[1].Positive = result.Key;
						}
						else
						{
							m_axisConfig.Bindings[0].Positive = result.Key;
						}
					}
					else if (m_changeAltKey)
					{
						m_axisConfig.Bindings[1].Negative = result.Key;
					}
					else
					{
						m_axisConfig.Bindings[0].Negative = result.Key;
					}
					m_keyDescription.text = ((result.Key != 0) ? result.Key.ToString() : string.Empty);
				}
				else if (m_axisConfig.Bindings[0].Type is InputType.Button)
				{
					KeyCode currentKeyCode = GetCurrentKeyCode();
					m_keyDescription.text = (currentKeyCode != 0) ? currentKeyCode.ToString() : string.Empty;
				}
				else
				{
					m_keyDescription.text = m_axisNames[m_axisConfig.Bindings[0].Axis];
				}
			}
			else if (result.JoystickAxis >= 0)
			{
				m_axisConfig.Bindings[0].Type = InputType.AnalogButton;
				//m_axisConfig.Bindings[0].SetAnalogButton(m_joystick, result.JoystickAxis);
				m_keyDescription.text = m_axisNames[m_axisConfig.Bindings[0].Axis];
			}
			else if (m_axisConfig.Bindings[0].Type == InputType.AnalogButton)
			{
				m_keyDescription.text = m_axisNames[m_axisConfig.Bindings[0].Axis];
			}
			else
			{
				KeyCode currentKeyCode2 = GetCurrentKeyCode();
				m_keyDescription.text = (currentKeyCode2 != 0) ? currentKeyCode2.ToString() : string.Empty;
			}
			return true;
		}

		private bool IsJoytickButtonValid(KeyCode key)
		{
			bool result = true;
			if (m_rebindType == RebindType.GamepadButton)
			{
				if (key < KeyCode.JoystickButton0 && key != 0 && key != KeyCode.Backspace)
				{
					result = false;
				}
			}
			else
			{
				result = false;
			}
			return result;
		}

		private bool HandleJoystickAxisScan(ScanResult result)
		{
			if (result.JoystickAxis >= 0)
			{
				//m_axisConfig.Bindings[0].SetAnalogAxis(m_joystick, result.JoystickAxis);
			}
			m_keyDescription.text = m_axisNames[m_axisConfig.Bindings[0].Axis];
			return true;
		}

		private KeyCode GetCurrentKeyCode()
		{
			if (m_rebindType == RebindType.GamepadAxis)
			{
				return KeyCode.None;
			}
			if (m_changePositiveKey)
			{
				if (m_changeAltKey)
				{
					return m_axisConfig.Bindings[1].Positive;
				}
				return m_axisConfig.Bindings[0].Positive;
			}
			if (m_changeAltKey)
			{
				return m_axisConfig.Bindings[1].Negative;
			}
			return m_axisConfig.Bindings[0].Negative;
		}
	}
}
