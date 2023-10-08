using System.IO;
using OpenHellion.Data;
using OpenHellion.IO;
using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class ControlsSettings : MonoBehaviour
	{
		public ControlsRebinder ControlsRebind;

		public ControlsSettingsData ControlsData = new ControlsSettingsData();

		public Text mouseSensitivityLevel;

		public Slider mouseSensitivitySlider;

		public Toggle InvertMouseToggle;

		public Toggle InvertMouseWhileDrivingToggle;

		private void Start()
		{
			mouseSensitivitySlider.minValue = 0.1f;
			mouseSensitivitySlider.maxValue = 10f;
			mouseSensitivitySlider.value = ControlsData.MouseSensitivity;
			mouseSensitivityLevel.text = mouseSensitivitySlider.value.ToString("0.0");
			mouseSensitivitySlider.onValueChanged.AddListener(MouseSensitivity);
			InvertMouseToggle.onValueChanged.AddListener(delegate { InvertMouseSetter(); });
			InvertMouseWhileDrivingToggle.onValueChanged.AddListener(delegate { InvertMouseWhileDrivingSetter(); });
		}

		public void MouseSensitivity(float val)
		{
			mouseSensitivityLevel.text = val.ToString("0.0");
			ControlsData.MouseSensitivity = val;
			mouseSensitivitySlider.value = val;
		}

		public void InvertMouseSetter()
		{
			ControlsData.InvertMouse = InvertMouseToggle.isOn;
		}

		public void InvertMouseWhileDrivingSetter()
		{
			ControlsData.InvertMouseWhileDriving = InvertMouseWhileDrivingToggle.isOn;
		}

		public void SetDefault()
		{
			ControlsData.MouseSensitivity = 1f;
			MouseSensitivity(ControlsData.MouseSensitivity);
			InvertMouseToggle.isOn = false;
			ControlsData.InvertMouse = false;
			InvertMouseWhileDrivingToggle.isOn = false;
			ControlsData.InvertMouseWhileDriving = false;
			InputManager.LoadDefaultConfig();
		}

		public void SaveControlsSettings()
		{
			if (!ControlsRebind.CheckIfEmpty())
			{
				File.WriteAllText(Path.Combine(Application.persistentDataPath, "Controls.json"),
					InputManager.Instance.InputActions.FindActionMap(InputManager.ActionMapName).ToJson());
			}
		}

		public void Load(ControlsSettingsData controlsSettings)
		{
			if (controlsSettings == null)
			{
				SetDefault();
				return;
			}

			ControlsData = controlsSettings;
			MouseSensitivity(ControlsData.MouseSensitivity);
			InvertMouseToggle.isOn = ControlsData.InvertMouse;
			InvertMouseWhileDrivingToggle.isOn = ControlsData.InvertMouseWhileDriving;

			if (File.Exists(Path.Combine(Application.persistentDataPath, "Controls.json")))
			{
				InputManager.LoadSavedConfig(File.ReadAllText(Path.Combine(Application.persistentDataPath,
					"Controls.json")));
			}
		}
	}
}
