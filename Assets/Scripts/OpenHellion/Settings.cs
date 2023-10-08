using System.IO;
using OpenHellion.Data;
using OpenHellion.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZeroGravity.UI;
using AudioSettings = ZeroGravity.UI.AudioSettings;

namespace OpenHellion
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

		public GameSettings GameComponent;

		public VideoSettings VideoComponent;

		public AudioSettings AudioComponent;

		public ControlsSettings ControlsComponent;

		private static Settings _instance;

		internal bool RestartOnSave;

		public static Settings Instance => _instance;

		private void Start()
		{
			_instance = this;
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

				if (SettingsData.SettingsVersion != Globals.SettingsVersion)
				{
					GameComponent.SetDefault();
					AudioComponent.SetDefault();
					ControlsComponent.SetDefault();
					VideoComponent.SetDefault();
					SettingsData.SettingsVersion = Globals.SettingsVersion;
					SaveSettings(SettingsType.All);
					return;
				}

				if (type == SettingsType.All)
				{
					VideoComponent.Load(SettingsData.VideoSettings);
					GameComponent.Load(SettingsData.GameSettings);
					AudioComponent.Load(SettingsData.AudioSettings);
					ControlsComponent.Load(SettingsData.ControlsSettings);
				}

				if (type == SettingsType.Controls)
				{
					ControlsComponent.Load(SettingsData.ControlsSettings);
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
				GameComponent.SaveGameSettigns();
				SettingsData.GameSettings = GameComponent.GameSettingsData;
				VideoComponent.SaveVideoSettings();
				SettingsData.VideoSettings = VideoComponent.VideoData;
				AudioComponent.SaveAudioSettings();
				SettingsData.AudioSettings = AudioComponent.AudioData;
				ControlsComponent.SaveControlsSettings();
				SettingsData.ControlsSettings = ControlsComponent.ControlsData;
				SettingsData.SettingsVersion = Globals.SettingsVersion;
			}

			if (type == SettingsType.Controls)
			{
				ControlsComponent.SaveControlsSettings();
				SettingsData.ControlsSettings = ControlsComponent.ControlsData;
				SettingsData.SettingsVersion = Globals.SettingsVersion;
			}

			JsonSerialiser.SerializePersistent(SettingsData, "Settings.json");
			if (RestartOnSave)
			{
				RestartOnSave = false;
				SceneManager.LoadScene(1);
			}
		}

		public void DefaultSettings(SettingsType type)
		{
			if (type == SettingsType.Controls)
			{
				ControlsComponent.SetDefault();
				SettingsData.ControlsSettings = ControlsComponent.ControlsData;
			}

			JsonSerialiser.SerializePersistent(SettingsData, "Settings.json");
		}

		private void SaveDefaultSettingsJson()
		{
			ControlsComponent.SetDefault();
			VideoComponent.SetDefault();
			AudioComponent.SetDefault();
			GameComponent.SetDefault();
			SaveSettings(SettingsType.All);
		}
	}
}
