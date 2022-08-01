using System.Collections;
using TeamUtility.IO;
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
		private string m_cancelButton;

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

		public AxisConfiguration m_axisConfig;

		private Image m_image;

		private static string[] m_axisNames = new string[10] { "X", "Y", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th" };

		public void SetRebinder(string inputConfigName, ControlItem contItem, Text buttonText, GameObject mainPanel, bool isAlt)
		{
			m_inputConfigName = inputConfigName;
			m_axisConfigName = contItem.Axis.ToString();
			m_changePositiveKey = contItem.IsPositive;
			m_changeAltKey = isAlt;
			m_cancelButton = "Escape";
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
			TeamUtility.IO.InputManager.Instance.Loaded += InitializeAxisConfig;
			TeamUtility.IO.InputManager.Instance.ConfigurationDirty += HandleConfigurationDirty;
		}

		private void OnDestroy()
		{
			if (TeamUtility.IO.InputManager.Instance != null)
			{
				TeamUtility.IO.InputManager.Instance.Loaded -= InitializeAxisConfig;
				TeamUtility.IO.InputManager.Instance.ConfigurationDirty -= HandleConfigurationDirty;
			}
		}

		private void InitializeAxisConfig()
		{
			m_axisConfig = TeamUtility.IO.InputManager.GetAxisConfiguration(m_inputConfigName, m_axisConfigName);
			if (m_axisConfig != null)
			{
				if (m_rebindType == RebindType.Keyboard || m_rebindType == RebindType.GamepadButton)
				{
					if (m_changePositiveKey)
					{
						if (m_changeAltKey)
						{
							m_keyDescription.text = ((m_axisConfig.altPositive != 0) ? m_axisConfig.altPositive.ToString() : string.Empty);
						}
						else
						{
							m_keyDescription.text = ((m_axisConfig.positive != 0) ? m_axisConfig.positive.ToString() : string.Empty);
						}
					}
					else if (m_changeAltKey)
					{
						m_keyDescription.text = ((m_axisConfig.altNegative != 0) ? m_axisConfig.altNegative.ToString() : string.Empty);
					}
					else
					{
						m_keyDescription.text = ((m_axisConfig.negative != 0) ? m_axisConfig.negative.ToString() : string.Empty);
					}
				}
				else
				{
					m_keyDescription.text = m_axisNames[m_axisConfig.axis];
				}
			}
			else
			{
				m_keyDescription.text = string.Empty;
				Debug.LogError(string.Format("Input configuration '{0}' does not exist or axis '{1}' does not exist", m_inputConfigName, m_axisConfigName));
			}
		}

		private void HandleConfigurationDirty(string configName)
		{
			if (configName == m_inputConfigName)
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
				MainPanel.GetComponent<ControlsRebinder>().oldKeyRev_p = m_axisConfig.positive;
				MainPanel.GetComponent<ControlsRebinder>().oldKeyRev_n = m_axisConfig.negative;
				MainPanel.GetComponent<ControlsRebinder>().oldKeyRev_ap = m_axisConfig.altPositive;
				MainPanel.GetComponent<ControlsRebinder>().oldKeyRev_an = m_axisConfig.altNegative;
				MainPanel.GetComponent<ControlsRebinder>().axesRev = m_axisConfig;
				MainPanel.GetComponent<ControlsRebinder>().EnableAllButtons(false);
				MainPanel.GetComponent<ControlsRebinder>().isScanning = true;
				MainPanel.GetComponent<ControlsRebinder>().WhoIsScanning = base.transform.name;
				StartCoroutine(StartInputScanDelayed());
			}
		}

		private IEnumerator StartInputScanDelayed()
		{
			yield return null;
			if (TeamUtility.IO.InputManager.IsScanning || m_axisConfig == null)
			{
				yield break;
			}
			m_image.overrideSprite = m_scanningState;
			m_keyDescription.text = "...";
			ScanSettings settings = default(ScanSettings);
			settings.joystick = m_joystick;
			settings.cancelScanButton = m_cancelButton;
			settings.timeout = m_timeout;
			settings.userData = null;
			if (m_rebindType == RebindType.GamepadAxis)
			{
				settings.scanFlags = ScanFlags.JoystickAxis;
				TeamUtility.IO.InputManager.StartScan(settings, HandleJoystickAxisScan);
			}
			else if (m_rebindType == RebindType.GamepadButton)
			{
				settings.scanFlags = (ScanFlags)6;
				if (m_rebindType == RebindType.GamepadButton && m_allowAnalogButton)
				{
					settings.scanFlags |= ScanFlags.JoystickAxis;
				}
				TeamUtility.IO.InputManager.StartScan(settings, HandleJoystickButtonScan);
			}
			else
			{
				settings.scanFlags = ScanFlags.Key;
				TeamUtility.IO.InputManager.StartScan(settings, HandleKeyScan);
			}
		}

		private bool HandleKeyScan(ScanResult result)
		{
			if (!IsKeyValid(result.key))
			{
				return false;
			}
			MainPanel.GetComponent<ControlsRebinder>().OnKeyChange(result.key, m_axisConfigName, m_changePositiveKey, m_changeAltKey, ControlItem);
			StartCoroutine(WaitAfterScan(0.4f));
			MainPanel.GetComponent<ControlsRebinder>().isScanning = false;
			MainPanel.GetComponent<ControlsRebinder>().WhoIsScanning = string.Empty;
			if (result.key != 0)
			{
				result.key = ((result.key != KeyCode.Backspace) ? result.key : KeyCode.None);
				if (m_changePositiveKey)
				{
					if (m_changeAltKey)
					{
						m_axisConfig.altPositive = result.key;
					}
					else
					{
						m_axisConfig.positive = result.key;
					}
				}
				else if (m_changeAltKey)
				{
					m_axisConfig.altNegative = result.key;
				}
				else
				{
					m_axisConfig.negative = result.key;
				}
				m_keyDescription.text = ((result.key != 0) ? result.key.ToString() : string.Empty);
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
			if (result.scanFlags == ScanFlags.Key || result.scanFlags == ScanFlags.JoystickButton)
			{
				if (!IsJoytickButtonValid(result.key))
				{
					return false;
				}
				if (result.key != 0)
				{
					result.key = ((result.key != KeyCode.Backspace) ? result.key : KeyCode.None);
					m_axisConfig.type = InputType.Button;
					if (m_changePositiveKey)
					{
						if (m_changeAltKey)
						{
							m_axisConfig.altPositive = result.key;
						}
						else
						{
							m_axisConfig.positive = result.key;
						}
					}
					else if (m_changeAltKey)
					{
						m_axisConfig.altNegative = result.key;
					}
					else
					{
						m_axisConfig.negative = result.key;
					}
					m_keyDescription.text = ((result.key != 0) ? result.key.ToString() : string.Empty);
				}
				else if (m_axisConfig.type == InputType.Button)
				{
					KeyCode currentKeyCode = GetCurrentKeyCode();
					m_keyDescription.text = ((currentKeyCode != 0) ? currentKeyCode.ToString() : string.Empty);
				}
				else
				{
					m_keyDescription.text = m_axisNames[m_axisConfig.axis];
				}
			}
			else if (result.joystickAxis >= 0)
			{
				m_axisConfig.type = InputType.AnalogButton;
				m_axisConfig.SetAnalogButton(m_joystick, result.joystickAxis);
				m_keyDescription.text = m_axisNames[m_axisConfig.axis];
			}
			else if (m_axisConfig.type == InputType.AnalogButton)
			{
				m_keyDescription.text = m_axisNames[m_axisConfig.axis];
			}
			else
			{
				KeyCode currentKeyCode2 = GetCurrentKeyCode();
				m_keyDescription.text = ((currentKeyCode2 != 0) ? currentKeyCode2.ToString() : string.Empty);
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
			if (result.joystickAxis >= 0)
			{
				m_axisConfig.SetAnalogAxis(m_joystick, result.joystickAxis);
			}
			m_keyDescription.text = m_axisNames[m_axisConfig.axis];
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
					return m_axisConfig.altPositive;
				}
				return m_axisConfig.positive;
			}
			if (m_changeAltKey)
			{
				return m_axisConfig.altNegative;
			}
			return m_axisConfig.negative;
		}
	}
}
