using System;
using System.IO;
using OpenHellion.IO;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;

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
			mouseSensitivitySlider.value = InputManager.SavedSensitivity;
			mouseSensitivityLevel.text = mouseSensitivitySlider.value.ToString("0.0");
			mouseSensitivitySlider.onValueChanged.AddListener(MouseSensitivity);
			InvertMouseToggle.onValueChanged.AddListener(delegate
			{
				InvertMouseSetter();
			});
			InvertMouseWhileDrivingToggle.onValueChanged.AddListener(delegate
			{
				InvertMouseWhileDrivingSetter();
			});
		}

		public void MouseSensitivity(float val)
		{
			mouseSensitivityLevel.text = val.ToString("0.0");
			ControlsData.MouseSensitivity = val;
			mouseSensitivitySlider.value = val;

			InputManager.SavedSensitivity = val;
		}

		public void InvertMouseSetter()
		{
			Client.Instance.InvertedMouse = InvertMouseToggle.isOn;
			ControlsData.InvertMouse = Client.Instance.InvertedMouse;
		}

		public void InvertMouseWhileDrivingSetter()
		{
			Client.Instance.InvertMouseWhileDriving = InvertMouseWhileDrivingToggle.isOn;
			ControlsData.InverMouseWhileDriving = Client.Instance.InvertMouseWhileDriving;
		}

		public void SetDefault()
		{
			ControlsData.MouseSensitivity = 1f;
			MouseSensitivity(ControlsData.MouseSensitivity);
			ControlsSettingsData controlsData = ControlsData;
			bool flag = false;
			InvertMouseToggle.isOn = flag;
			controlsData.InvertMouse = flag;
			ControlsSettingsData controlsData2 = ControlsData;
			flag = false;
			InvertMouseWhileDrivingToggle.isOn = flag;
			controlsData2.InverMouseWhileDriving = flag;
			InputManager.LoadDefaultConfig();
		}

		public void SaveControlsSettings()
		{
			ControlsData.MouseSensitivity = InputManager.SavedSensitivity;
			ControlsData.InvertMouse = Client.Instance.InvertedMouse;
			ControlsData.InverMouseWhileDriving = Client.Instance.InvertMouseWhileDriving;
			if (!ControlsRebind.CheckIfEmpty())
			{
				File.WriteAllText(Path.Combine(Application.persistentDataPath, "Controls.json"), InputManager.Instance.InputActions.FindActionMap(InputManager.ActionMapName).ToJson());
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
			InvertMouseWhileDrivingToggle.isOn = ControlsData.InverMouseWhileDriving;
			Client.Instance.InvertedMouse = ControlsData.InvertMouse;
			Client.Instance.InvertMouseWhileDriving = ControlsData.InverMouseWhileDriving;

			if (File.Exists(Path.Combine(Application.persistentDataPath, "Controls.json")))
			{
				InputManager.LoadSavedConfig(File.ReadAllText(Path.Combine(Application.persistentDataPath, "Controls.json")));
			}
		}
	}
}
