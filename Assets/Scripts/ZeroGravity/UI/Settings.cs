using System.IO;
using OpenHellion.Util;
using UnityEngine;
using ZeroGravity.Data;

namespace ZeroGravity.UI
{
	public class Settings : MonoBehaviour
	{
		public enum SettingsType
		{
			Game = 0,
			Controls = 1,
			Video = 2,
			Audio = 3,
			All = 4
		}

		public SettingsData SettingsData = new SettingsData();

		public GameSettings gameComponent;

		public VideoSettings videoComponent;

		public AudioSettings audioComponent;

		public ControlsSettings controlsComponent;

		private static Settings instance;

		internal bool RestartOnSave;

		public static Settings Instance
		{
			get
			{
				return instance;
			}
		}

		private void Start()
		{
			instance = this;
		}

		public void LoadSettings(SettingsType type)
		{
			if (File.Exists(Path.Combine(Application.persistentDataPath, "Settings.json")))
			{
				try
				{
					SettingsData = JsonSerialiser.LoadPersistent<SettingsData>("Settings.json");
				}
				catch
				{
					File.Delete(Path.Combine(Application.persistentDataPath, "Settings.json"));
					SaveDefaultSettingsJson();
				}
				if (SettingsData == null)
				{
					SettingsData = new SettingsData();
				}
				if (SettingsData.ControlsVersion != Client.ControlsVersion)
				{
					gameComponent.SetDefault();
					audioComponent.SetDefault();
					controlsComponent.SetDefault();
					videoComponent.SetDefault();
					SettingsData.ControlsVersion = Client.ControlsVersion;
					SaveSettings(SettingsType.All);
					return;
				}
				if (type == SettingsType.All)
				{
					videoComponent.Load(SettingsData.VideoSettings);
					gameComponent.Load(SettingsData.GameSettings);
					audioComponent.Load(SettingsData.AudioSettings);
					controlsComponent.Load(SettingsData.ControlsSettings);
				}
				if (type == SettingsType.Controls)
				{
					controlsComponent.Load(SettingsData.ControlsSettings);
				}
			}
			else
			{
				SaveDefaultSettingsJson();
			}
		}

		public void SaveSettings(SettingsType type)
		{
			if (type == SettingsType.All)
			{
				gameComponent.SaveGameSettigns();
				SettingsData.GameSettings = gameComponent.GameSettingsData;
				videoComponent.SaveVideoSettings();
				SettingsData.VideoSettings = VideoSettings.VideoData;
				audioComponent.SaveAudioSettings();
				SettingsData.AudioSettings = audioComponent.AudioData;
				controlsComponent.SaveControlsSettings();
				SettingsData.ControlsSettings = controlsComponent.ControlsData;
				SettingsData.ControlsVersion = Client.ControlsVersion;
			}
			if (type == SettingsType.Controls)
			{
				controlsComponent.SaveControlsSettings();
				SettingsData.ControlsSettings = controlsComponent.ControlsData;
				SettingsData.ControlsVersion = Client.ControlsVersion;
			}
			JsonSerialiser.SerializePersistent(SettingsData, "Settings.json");
			if (RestartOnSave)
			{
				RestartOnSave = false;
				Client.Instance.OpenMainScreen();
			}
		}

		public void DefaultSettings(SettingsType type)
		{
			if (type == SettingsType.Controls)
			{
				controlsComponent.SetDefault();
				SettingsData.ControlsSettings = controlsComponent.ControlsData;
			}
			JsonSerialiser.SerializePersistent(SettingsData, "Settings.json");
		}

		private void SaveDefaultSettingsJson()
		{
			controlsComponent.SetDefault();
			videoComponent.SetDefault();
			audioComponent.SetDefault();
			gameComponent.SetDefault();
			SaveSettings(SettingsType.All);
		}
	}
}
