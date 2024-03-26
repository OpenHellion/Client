using System;
using System.IO;
using OpenHellion.Data;
using OpenHellion.IO;
using OpenHellion.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZeroGravity.UI;

namespace OpenHellion
{
	/// <summary>
	///		The settings class handles the underlying code and data required for changing the settings in-game.
	/// </summary>
	public static class Settings
	{
		public enum SettingsType
		{
			Game = 0,
			Controls = 1,
			Video = 2,
			Audio = 3,
			All = 4
		}

		public static SettingsData SettingsData = new SettingsData();

		public static ControlsRebinder ControlsRebind;

		internal static bool RestartOnSave;

		public static Action OnSaveAction;

		public static void LoadSettings(SettingsType type)
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
					SaveDefaultSettingsJson();
					return;
				}

				if (type == SettingsType.All)
				{
					if (Screen.resolutions.Length <= SettingsData.VideoSettings.ResolutionIndex)
					{
						SettingsData.VideoSettings.ResolutionIndex = Screen.resolutions.Length - 1;
					}

					GlobalGUI.SetGameSettings(SettingsData.GameSettings);
					GlobalGUI.SetControlsSettings(SettingsData.ControlsSettings);
					GlobalGUI.SetVideoSettings(SettingsData.VideoSettings);
					GlobalGUI.SetAudioSettings(SettingsData.AudioSettings);

					ControlsSubsystem.RealSensitivity = SettingsData.ControlsSettings.MouseSensitivity;
					QualitySettings.SetQualityLevel(SettingsData.VideoSettings.QualityIndex, applyExpensiveChanges: true);
					QualitySettings.masterTextureLimit = SettingsData.VideoSettings.TextureIndex;
					QualitySettings.shadows = (ShadowQuality)SettingsData.VideoSettings.ShadowIndex;
					var resolution = Screen.resolutions[SettingsData.VideoSettings.ResolutionIndex];
					Screen.SetResolution(resolution.width, resolution.height, SettingsData.VideoSettings.Fullscreen);
					AkSoundEngine.SetRTPCValue(SoundManager.MasterVolume, SettingsData.AudioSettings.Volume);
					AkSoundEngine.SetRTPCValue(SoundManager.AmbienceVolume, SettingsData.AudioSettings.AmbienceVolume);
					AudioListener.volume = SettingsData.AudioSettings.VoiceVolume / 100f;

					GlobalGUI.UpdatePostEffects(SettingsData.VideoSettings);
				}

				if (type == SettingsType.Controls)
				{
					GlobalGUI.SetControlsSettings(SettingsData.ControlsSettings);
				}

				if (File.Exists(Path.Combine(Application.persistentDataPath, "Controls.json")))
				{
					ControlsSubsystem.LoadSavedConfig(File.ReadAllText(Path.Combine(Application.persistentDataPath,
						"Controls.json")));
				}
			}
			else
			{
				SaveDefaultSettingsJson();
			}
		}

		public static void SaveSettings(SettingsType type)
		{
			if (type == SettingsType.All)
			{
				SettingsData.GameSettings = GlobalGUI.GetGameSettings();
				SettingsData.ControlsSettings = GlobalGUI.GetControlsSettings();
				SettingsData.VideoSettings = GlobalGUI.GetVideoSettings();
				SettingsData.AudioSettings = GlobalGUI.GetAudioSettings();
				SettingsData.SettingsVersion = Globals.SettingsVersion;

				ControlsSubsystem.RealSensitivity = SettingsData.ControlsSettings.MouseSensitivity;
				QualitySettings.SetQualityLevel(SettingsData.VideoSettings.QualityIndex, applyExpensiveChanges: true);
				QualitySettings.masterTextureLimit = SettingsData.VideoSettings.TextureIndex;
				QualitySettings.shadows = (ShadowQuality)SettingsData.VideoSettings.ShadowIndex;
				var resolution = Screen.resolutions[SettingsData.VideoSettings.ResolutionIndex];
				Screen.SetResolution(resolution.width, resolution.height, SettingsData.VideoSettings.Fullscreen);
				AkSoundEngine.SetRTPCValue(SoundManager.MasterVolume, SettingsData.AudioSettings.Volume);
				AkSoundEngine.SetRTPCValue(SoundManager.AmbienceVolume, SettingsData.AudioSettings.AmbienceVolume);
				AudioListener.volume = SettingsData.AudioSettings.VoiceVolume / 100f;

				GlobalGUI.UpdatePostEffects(SettingsData.VideoSettings);
			}

			if (type == SettingsType.Controls)
			{
				SettingsData.ControlsSettings = GlobalGUI.GetControlsSettings();
				SettingsData.SettingsVersion = Globals.SettingsVersion;
			}

			if (ControlsRebind != null && !ControlsRebind.CheckIfEmpty())
			{
				File.WriteAllText(Path.Combine(Application.persistentDataPath, "Controls.json"),
					ControlsSubsystem.InputActions.FindActionMap(ControlsSubsystem.ActionMapName).ToJson());
			}

			OnSaveAction?.Invoke();

			JsonSerialiser.SerializePersistent(SettingsData, "Settings.json");
			if (RestartOnSave)
			{
				RestartOnSave = false;
				SceneManager.LoadScene(1);
			}
		}

		private static void SaveDefaultSettingsJson()
		{
			GlobalGUI.ResetSettings();
			SaveSettings(SettingsType.All);
		}
	}
}
