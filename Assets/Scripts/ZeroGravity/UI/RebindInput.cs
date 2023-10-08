using System.Collections;
using OpenHellion.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	// TODO: This is really broken right now.
	[RequireComponent(typeof(Image))]
	public class RebindInput : MonoBehaviour, IEventSystemHandler
	{
		public GameObject MainPanel;

		public ControlItem ControlItem;

		[SerializeField] private Sprite m_normalState;

		[SerializeField] private Sprite m_scanningState;

		public Text m_keyDescription;

		[SerializeField] private string m_axisConfigName;

		[SerializeField] private bool m_isPositiveKey;

		public InputAction m_axisConfig;

		private Image m_image;

		public void SetRebinder(ControlItem contItem, Text buttonText, GameObject mainPanel)
		{
			m_axisConfigName = contItem.Action.ToString();
			m_isPositiveKey = contItem.IsPositive;
			MainPanel = mainPanel;
			m_keyDescription = buttonText;
			ControlItem = contItem;
		}

		private void Start()
		{
			m_image = GetComponent<Image>();
			m_image.overrideSprite = m_normalState;
			InitializeAxisConfig();
		}

		private void InitializeAxisConfig()
		{
			m_axisConfig = InputManager.Instance.InputActions.FindAction(m_axisConfigName);
			if (m_axisConfig != null)
			{
				m_keyDescription.text = m_axisConfig.GetBindingDisplayString();
				// Loop through bindings and find the positive/negative binding.
				foreach (InputBinding binding in m_axisConfig.bindings)
				{
					if (!binding.isPartOfComposite) continue;

					if (m_isPositiveKey && binding.name == "positive")
					{
						m_keyDescription.text = binding.ToDisplayString();
						break;
					}
					else if (!m_isPositiveKey && binding.name == "negative")
					{
						m_keyDescription.text = binding.ToDisplayString();
						break;
					}
				}
			}
			else
			{
				m_keyDescription.text = string.Empty;
				Debug.LogError(string.Format("Axis '{0}' does not exist", m_axisConfigName));
			}
		}

		public void OnButtonPressed()
		{
			ControlsRebinder rebinder = MainPanel.GetComponent<ControlsRebinder>();
			if (rebinder.WhoIsScanning == string.Empty)
			{
				//rebinder.oldKeyRev_p = m_axisConfig.bindings[0].Positive;
				//rebinder.oldKeyRev_n = m_axisConfig.bindings[0].Negative;
				//rebinder.oldKeyRev_ap = m_axisConfig.bindings[1].Positive;
				//rebinder.oldKeyRev_an = m_axisConfig.bindings[1].Negative;
				rebinder._actionsRev = m_axisConfig;
				rebinder.EnableAllButtons(false);
				rebinder.isScanning = true;
				rebinder.WhoIsScanning = transform.name;
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
			/*if (!IsKeyValid(result.Key))
			{
				return false;
			}

			MainPanel.GetComponent<ControlsRebinder>().OnKeyChange(result.Key, m_axisConfigName, m_changePositiveKey, m_changeAltKey, ControlItem);
			StartCoroutine(WaitAfterScan(0.4f));
			MainPanel.GetComponent<ControlsRebinder>().isScanning = false;
			MainPanel.GetComponent<ControlsRebinder>().WhoIsScanning = string.Empty;

			// Change key.

			return true;*/
			return false;
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
	}
}
