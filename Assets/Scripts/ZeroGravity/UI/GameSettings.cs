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

		public GameSettingsData GameSettingsData = new GameSettingsData();

		private void Start()
		{
			headBobSlider.maxValue = 1f;
			headBobSlider.wholeNumbers = false;
			headBobSlider.value = Client.Instance.HeadbobStrength;
			headBobSlider.onValueChanged.AddListener(HeadBobSlider);
			headBobLevel.text = headBobSlider.value.ToString("0.00");
			if (Properties.GetProperty("custom_localization_file", string.Empty) != string.Empty)
			{
				LanguageDropdown.options.Add(new Dropdown.OptionData
				{
					text = "Custom"
				});
			}
			LanguageDropdown.value = ((GameSettingsData.LanguageIndex < LanguageDropdown.options.Count) ? GameSettingsData.LanguageIndex : 0);
			LanguageDropdown.RefreshShownValue();
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
			HeadBobSlider(GameSettingsData.HeadBobStrenth);
			Client.Instance.HeadbobStrength = GameSettingsData.HeadBobStrenth;
			Client.Instance.CanvasManager.ShowTutorial = GameSettingsData.ShowTutorial;
			DisableChatToggle.isOn = GameSettingsData.DisableChat;
			Client.Instance.CanvasManager.DisableChat = GameSettingsData.DisableChat;
			HideTipsToggle.isOn = !GameSettingsData.ShowTips;
			Client.Instance.CanvasManager.ShowTips = GameSettingsData.ShowTips;
			ShowCrosshairToggle.isOn = GameSettingsData.ShowCrosshair;
			Client.Instance.CanvasManager.ShowCrosshair = GameSettingsData.ShowCrosshair;
			AutoStabilization.isOn = GameSettingsData.AutoStabilization;
			Client.Instance.CanvasManager.AutoStabilization = GameSettingsData.AutoStabilization;
			LanguageDropdown.value = GameSettingsData.LanguageIndex;
			Client.Instance.CurrentLanguageIndex = GameSettingsData.LanguageIndex;
		}

		public void SaveGameSettigns()
		{
			GameSettingsData.HeadBobStrenth = headBobSlider.value;
			Client.Instance.HeadbobStrength = headBobSlider.value;
			GameSettingsData.ShowTutorial = true;
			Client.Instance.CanvasManager.ShowTutorial = false;
			GameSettingsData.DisableChat = DisableChatToggle.isOn;
			Client.Instance.CanvasManager.DisableChat = DisableChatToggle.isOn;
			GameSettingsData.ShowTips = !HideTipsToggle.isOn;
			Client.Instance.CanvasManager.ShowTips = !HideTipsToggle.isOn;
			GameSettingsData.AutoStabilization = AutoStabilization.isOn;
			Client.Instance.CanvasManager.AutoStabilization = AutoStabilization.isOn;
			GameSettingsData.ShowCrosshair = ShowCrosshairToggle.isOn;
			Client.Instance.CanvasManager.ShowCrosshair = ShowCrosshairToggle.isOn;
			GameSettingsData.LanguageIndex = LanguageDropdown.value;
			Client.Instance.CurrentLanguageIndex = LanguageDropdown.value;
		}

		public void HeadBobSlider(float val)
		{
			headBobLevel.text = val.ToString("0.00");
			Client.Instance.HeadbobStrength = val;
			GameSettingsData.HeadBobStrenth = val;
			headBobSlider.value = val;
		}

		public void SetDefault()
		{
			GameSettingsData.HeadBobStrenth = 0f;
			HeadBobSlider(GameSettingsData.HeadBobStrenth);
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
			GameSettingsData.ShowTutorial = (Client.Instance.CanvasManager.ShowTutorial = !HideTutorialToggle.isOn);
		}

		public void OnDisableChatChange(bool value)
		{
			GameSettingsData.DisableChat = (Client.Instance.CanvasManager.DisableChat = DisableChatToggle.isOn);
		}

		public void OnHideTipsChange(bool value)
		{
			GameSettingsData.ShowTips = (Client.Instance.CanvasManager.ShowTips = !HideTipsToggle.isOn);
		}

		public void OnAutoStabilizationChange(bool value)
		{
			GameSettingsData.AutoStabilization = (Client.Instance.CanvasManager.AutoStabilization = AutoStabilization.isOn);
		}

		public void OnShowCrosshairChange(bool value)
		{
			GameSettingsData.ShowCrosshair = (Client.Instance.CanvasManager.ShowCrosshair = ShowCrosshairToggle.isOn);
			Client.Instance.CanvasManager.CanvasUI.CheckDotCroshair();
		}

		public void OnLanguageChange(int value)
		{
			GameSettingsData.LanguageIndex = (Client.Instance.CurrentLanguageIndex = LanguageDropdown.value);
			Settings.Instance.RestartOnSave = true;
		}

		public void UpdateUI()
		{
		}
	}
}
