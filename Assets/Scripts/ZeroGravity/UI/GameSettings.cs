using System;
using OpenHellion;
using OpenHellion.Data;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;

namespace ZeroGravity.UI
{
	public class GameSettings : MonoBehaviour
	{
		public Toggle HideTutorialToggle;

		public Toggle DisableChatToggle;

		public Toggle HideTipsToggle;

		public Toggle ShowCrosshairToggle;

		public Toggle AutoStabilization;

		public Text headBobLevel;

		public Slider headBobSlider;

		public Dropdown LanguageDropdown;

		public Action<bool> OnCrosshairToggle;

		public GameSettingsData GameSettingsData = new GameSettingsData();

		private void Start()
		{
			headBobLevel.text = headBobSlider.value.ToString("0.00");
			if (Properties.GetProperty("custom_localization_file", string.Empty) != string.Empty)
			{
				LanguageDropdown.options.Add(new Dropdown.OptionData
				{
					text = "Custom"
				});
			}

			LanguageDropdown.value = GameSettingsData.LanguageIndex < LanguageDropdown.options.Count
				? GameSettingsData.LanguageIndex
				: 0;
			LanguageDropdown.RefreshShownValue();
			headBobSlider.maxValue = 1f;
			headBobSlider.wholeNumbers = false;
			headBobSlider.onValueChanged.AddListener(HeadBobSlider);
			DisableChatToggle.onValueChanged.AddListener(OnDisableChatChange);
			HideTipsToggle.onValueChanged.AddListener(OnHideTipsChange);
			ShowCrosshairToggle.onValueChanged.AddListener(OnShowCrosshairChange);
			AutoStabilization.onValueChanged.AddListener(OnAutoStabilizationChange);
			LanguageDropdown.onValueChanged.AddListener(OnLanguageChange);
		}

		public void Load(GameSettingsData gameSettings)
		{
			if (gameSettings == null)
			{
				SetDefault();
				return;
			}

			GameSettingsData = gameSettings;
			HeadBobSlider(GameSettingsData.HeadBobStrength);
			DisableChatToggle.isOn = GameSettingsData.DisableChat;
			HideTipsToggle.isOn = !GameSettingsData.ShowTips;
			ShowCrosshairToggle.isOn = GameSettingsData.ShowCrosshair;
			AutoStabilization.isOn = GameSettingsData.AutoStabilization;
			LanguageDropdown.value = GameSettingsData.LanguageIndex;
		}

		public void SaveGameSettigns()
		{
			GameSettingsData.HeadBobStrength = headBobSlider.value;
			GameSettingsData.ShowTutorial = true;
			GameSettingsData.DisableChat = DisableChatToggle.isOn;
			GameSettingsData.ShowTips = !HideTipsToggle.isOn;
			GameSettingsData.AutoStabilization = AutoStabilization.isOn;
			GameSettingsData.ShowCrosshair = ShowCrosshairToggle.isOn;
			GameSettingsData.LanguageIndex = LanguageDropdown.value;
		}

		public void HeadBobSlider(float val)
		{
			headBobLevel.text = val.ToString("0.00");
			GameSettingsData.HeadBobStrength = val;
			headBobSlider.value = val;
		}

		public void SetDefault()
		{
			GameSettingsData.HeadBobStrength = 0f;
			HeadBobSlider(GameSettingsData.HeadBobStrength);
			GameSettingsData.ShowCrosshair = true;
			ShowCrosshairToggle.isOn = GameSettingsData.ShowCrosshair;
			GameSettingsData.ShowTutorial = true;
			GameSettingsData.DisableChat = false;
			DisableChatToggle.isOn = GameSettingsData.DisableChat;
			GameSettingsData.ShowTips = true;
			HideTipsToggle.isOn = !GameSettingsData.ShowTips;
			GameSettingsData.AutoStabilization = true;
			AutoStabilization.isOn = GameSettingsData.AutoStabilization;
			GameSettingsData.LanguageIndex = 0;
			LanguageDropdown.value = GameSettingsData.LanguageIndex;
		}

		public void OnHideTutorialChange()
		{
			GameSettingsData.ShowTutorial = !HideTutorialToggle.isOn;
		}

		public void OnDisableChatChange(bool value)
		{
			GameSettingsData.DisableChat = DisableChatToggle.isOn;
		}

		public void OnHideTipsChange(bool value)
		{
			GameSettingsData.ShowTips = !HideTipsToggle.isOn;
		}

		public void OnAutoStabilizationChange(bool value)
		{
			GameSettingsData.AutoStabilization = AutoStabilization.isOn;
		}

		public void OnShowCrosshairChange(bool value)
		{
			GameSettingsData.ShowCrosshair = ShowCrosshairToggle.isOn;
			OnCrosshairToggle(value);
		}

		public void OnLanguageChange(int value)
		{
			GameSettingsData.LanguageIndex = LanguageDropdown.value;
			Settings.Instance.RestartOnSave = true;
		}

		public void UpdateUI()
		{
		}
	}
}
