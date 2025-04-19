// SettingsMenuViewModel.cs
//
// Copyright (C) 2024, OpenHellion contributors
//
// SPDX-License-Identifier: GPL-3.0-or-later
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using OpenHellion.Data;
using OpenHellion.IO;
using UnityEngine;

namespace OpenHellion
{
	public class SettingsMenuViewModel : MonoBehaviour
	{
		[Title("The data that corresponds to the currently viewable settings.")]
		// Game settings
		[SerializeField]
		private bool _hideTutorial;

		[SerializeField]
		private bool _disableChat;

		[SerializeField]
		private bool _hideTips;

		[SerializeField]
		private bool _showCrosshair;

		[SerializeField]
		private bool _autoStabilisation;

		[SerializeField]
		private float _headBob;

		// TODO: Changing this should set Settings.RestartOnSave = true;
		[SerializeField]
		private int _languageIndex;

		// Controls settings
		[SerializeField] private float _mouseSensitivity;

		[SerializeField] private bool _invertMouse;

		[SerializeField] private bool _invertMouseWhileDriving;

		// Video settings
		[SerializeField] private bool _fullScreen;

		[SerializeField] private int _resolutionIndex;

		[SerializeField] private int _qualityIndex;

		[SerializeField] private int _textureIndex;

		[SerializeField] private int _shadowQualityIndex;

		[SerializeField] private int _antialiasingIndex;

		[SerializeField] private bool _volumetricLighting;

		[SerializeField] private bool _ambientOcclusion;

		[SerializeField] private bool _motionBlur;

		[SerializeField] private bool _eyeAdaptation;

		[SerializeField] private bool _bloom;

		[SerializeField] private bool _chromaticAberration;

		// Audio settings
		[SerializeField] private float _masterVolume;

		[SerializeField] private float _ambienceVolume;

		[SerializeField] private float _voiceVolume;

		public GameSettingsData GetGameSettings()
		{
			return new GameSettingsData
			{
				ShowTutorial = !_hideTutorial,
				DisableChat = _disableChat,
				ShowTips = !_hideTips,
				ShowCrosshair = _showCrosshair,
				AutoStabilization = _autoStabilisation,
				HeadBobStrength = _headBob,
				LanguageIndex = _languageIndex
			};
		}

		public void SetGameSettings(GameSettingsData data)
		{
			_hideTutorial = !data.ShowTutorial;
			_hideTutorial = !data.ShowTutorial;
			_disableChat = data.DisableChat;
			_hideTips = !data.ShowTips;
			_showCrosshair = data.ShowCrosshair;
			_autoStabilisation = data.AutoStabilization;
			_headBob = data.HeadBobStrength;
			_languageIndex = data.LanguageIndex;
		}

		public void ResetGameSettings()
		{
			_headBob = 0f;
			_showCrosshair = true;
			_hideTutorial = false;
			_disableChat = false;
			_hideTips = false;
			_autoStabilisation = true;
			_languageIndex = 0;
		}

		public ControlsSettingsData GetControlsSettings()
		{
			return new ControlsSettingsData
			{
				MouseSensitivity = _mouseSensitivity,
				InvertMouse = _invertMouse,
				InvertMouseWhileDriving = _invertMouseWhileDriving
			};
		}

		public void SetControlsSettings(ControlsSettingsData data)
		{
			_mouseSensitivity = data.MouseSensitivity;
			_invertMouse = data.InvertMouse;
			_invertMouseWhileDriving = data.InvertMouseWhileDriving;
		}

		public void ResetControlsSettings()
		{
			_mouseSensitivity = 1f;
			_invertMouse = false;
			_invertMouseWhileDriving = false;
			ControlsSubsystem.Reset();
		}

		public VideoSettingsData GetVideoSettings()
		{
			return new VideoSettingsData
			{
				Fullscreen = _fullScreen,
				ResolutionIndex = _resolutionIndex,
				QualityIndex = _qualityIndex,
				TextureIndex = _textureIndex,
				ShadowIndex = _shadowQualityIndex,
				AntialiasingIndex = _antialiasingIndex,
				VolumetricLights = _volumetricLighting,
				AmbientOcclusion = _ambientOcclusion,
				MotionBlur = _motionBlur,
				EyeAdaptation = _eyeAdaptation,
				Bloom = _bloom,
				ChromaticAberration = _chromaticAberration
			};
		}

		public void SetVideoSettings(VideoSettingsData data)
		{
			_fullScreen = data.Fullscreen;
			_resolutionIndex = data.ResolutionIndex;
			_qualityIndex = data.QualityIndex;
			_textureIndex = data.TextureIndex;
			_shadowQualityIndex = data.ShadowIndex;
			_antialiasingIndex = data.AntialiasingIndex;
			_volumetricLighting = data.VolumetricLights;
			_ambientOcclusion = data.AmbientOcclusion;
			_motionBlur = data.MotionBlur;
			_eyeAdaptation = data.EyeAdaptation;
			_bloom = data.Bloom;
			_chromaticAberration = data.ChromaticAberration;
		}

		public void ResetVideoSettings()
		{
			_resolutionIndex = Screen.resolutions.Length - 1;
			_fullScreen = true;
			Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, _fullScreen);
			_ambientOcclusion = true;
			_motionBlur = false;
			_eyeAdaptation = true;
			_bloom = true;
			_chromaticAberration = true;
			_qualityIndex = 2;
			QualitySettings.SetQualityLevel(_qualityIndex, applyExpensiveChanges: false);
			_textureIndex = QualitySettings.masterTextureLimit;
			_shadowQualityIndex = (int)QualitySettings.shadows;
			_antialiasingIndex = 1;

			// TODO: Off until I can fix it.
			_volumetricLighting = false;
		}

		public AudioSettingsData GetAudioSettings()
		{
			return new AudioSettingsData
			{
				Volume = _masterVolume,
				AmbienceVolume = _ambienceVolume,
				VoiceVolume = _voiceVolume
			};
		}

		public void SetAudioSettings(AudioSettingsData data)
		{
			_masterVolume = data.Volume;
			_ambienceVolume = data.AmbienceVolume;
			_voiceVolume = data.VoiceVolume;
		}

		public void ResetAudioSettings()
		{
			_masterVolume = 80f;
			_ambienceVolume = 100f;
			_voiceVolume = 100f;
		}
	}
}
