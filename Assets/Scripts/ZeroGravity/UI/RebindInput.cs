using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	[RequireComponent(typeof(Image))]
	public class RebindInput : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
	{
		public GameObject MainPanel;

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
		private bool m_changePositiveKey;

		[SerializeField]
		private bool m_changeAltKey;

		public InputAction m_axisConfig;

		private Image m_image;

		public void SetRebinder(string inputConfigName, ControlItem contItem, Text buttonText, GameObject mainPanel, bool isAlt)
		{
			m_inputConfigName = inputConfigName;
			m_axisConfigName = contItem.Actions.ToString();
			m_changePositiveKey = contItem.IsPositive;
			m_changeAltKey = isAlt;
			MainPanel = mainPanel;
			m_keyDescription = buttonText;
			ControlItem = contItem;
		}

		private void Start()
		{
			m_image = GetComponent<Image>();
			m_image.overrideSprite = m_normalState;
			InitializeAxisConfig();
			InputManager.Loaded += InitializeAxisConfig;
			InputManager.PlayerControlsChanged += HandleConfigurationDirty;
		}

		private void OnDestroy()
		{
			InputManager.Loaded -= InitializeAxisConfig;
			InputManager.PlayerControlsChanged -= HandleConfigurationDirty;
		}

		private void InitializeAxisConfig()
		{
			m_axisConfig = InputController.Instance.InputActions.FindAction(m_axisConfigName);
			if (m_axisConfig != null)
			{
				if (m_changePositiveKey)
				{
					if (m_changeAltKey)
					{
						m_keyDescription.text = (m_axisConfig.bindings[1].Positive != 0) ? m_axisConfig.bindings[1].Positive.ToString() : string.Empty;
					}
					else
					{
						m_keyDescription.text = (m_axisConfig.bindings[0].Positive != 0) ? m_axisConfig.bindings[0].Positive.ToString() : string.Empty;
					}
				}
				else if (m_changeAltKey)
				{
					m_keyDescription.text = (m_axisConfig.bindings[1].Negative != 0) ? m_axisConfig.bindings[1].Negative.ToString() : string.Empty;
				}
				else
				{
					m_keyDescription.text = (m_axisConfig.bindings[0].Negative != 0) ? m_axisConfig.bindings[0].Negative.ToString() : string.Empty;
				}
			}
			else
			{
				m_keyDescription.text = string.Empty;
				Debug.LogError(string.Format("Input configuration '{0}' does not exist or axis '{1}' does not exist", m_inputConfigName, m_axisConfigName));
			}
		}

		private void OnControlsChanged()
		{
			InitializeAxisConfig();
		}

		public void OnPointerDown(PointerEventData data)
		{
		}

		public void OnButtonPressed()
		{
			if (MainPanel.GetComponent<ControlsRebinder>().WhoIsScanning == string.Empty)
			{
				//MainPanel.GetComponent<ControlsRebinder>().oldKeyRev_p = m_axisConfig.bindings[0].Positive;
				//MainPanel.GetComponent<ControlsRebinder>().oldKeyRev_n = m_axisConfig.bindings[0].Negative;
				//MainPanel.GetComponent<ControlsRebinder>().oldKeyRev_ap = m_axisConfig.bindings[1].Positive;
				//MainPanel.GetComponent<ControlsRebinder>().oldKeyRev_an = m_axisConfig.bindings[1].Negative;
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

			if (m_axisConfig == null)
			{
				yield break;
			}

			m_image.overrideSprite = m_scanningState;
			m_keyDescription.text = "...";

			// Start scanning for key.
		}

		private bool HandleKeyScan()
		{
			//if (!IsKeyValid(result.Key))
			{
				return false;
			}

			//MainPanel.GetComponent<ControlsRebinder>().OnKeyChange(result.Key, m_axisConfigName, m_changePositiveKey, m_changeAltKey, ControlItem);
			StartCoroutine(WaitAfterScan(0.4f));
			MainPanel.GetComponent<ControlsRebinder>().isScanning = false;
			MainPanel.GetComponent<ControlsRebinder>().WhoIsScanning = string.Empty;

			// Change key.

			return true;
		}

		private bool IsKeyValid(KeyCode key)
		{
			bool result = true;
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
			return result;
		}

		private IEnumerator WaitAfterScan(float waitTime)
		{
			yield return new WaitForSeconds(waitTime);
			MainPanel.GetComponent<ControlsRebinder>().EnableAllButtons(true);
		}

		private bool HandleJoystickButtonScan()
		{
			return false;
		}

		private bool HandleJoystickAxisScan()
		{
			return false;
		}
	}
}
