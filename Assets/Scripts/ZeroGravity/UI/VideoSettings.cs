using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using ZeroGravity;
using ZeroGravity.Data;

namespace ZeroGravity.UI
{
	public class VideoSettings : MonoBehaviour
	{
		public static VideoSettingsData VideoData = new VideoSettingsData();

		public Resolution[] resolutions;

		public string[] qualityLevels;

		public Toggle fullScreenToggle;

		public Dropdown resolutionDropdown;

		public Dropdown qualitySettingsDropdown;

		public Dropdown textureQualityDropdown;

		public Dropdown shadowQualityDropdown;

		public Dropdown antialiasingDropdown;

		public Toggle VolumetricLighting;

		public Toggle AmbientOcclusion;

		public Toggle MotionBlur;

		public Toggle EyeAdaptation;

		public Toggle Bloom;

		public Toggle ChromaticAberration;

		public GameObject CantChangeTexture;

		public GameObject CantChangeQuality;

		public List<PostProcessProfile> PostEffectProfiles;

		[SerializeField]
		private GameObject _resolutionElement;

		private void Start()
		{
			qualitySettingsDropdown.value = VideoData.QualityIndex;
			qualitySettingsDropdown.RefreshShownValue();
			fullScreenToggle.onValueChanged.AddListener(delegate
			{
				OnFullscreenChange();
			});
			resolutionDropdown.onValueChanged.AddListener(delegate
			{
				OnResolutionChange();
			});
			qualitySettingsDropdown.onValueChanged.AddListener(delegate
			{
				OnQualityChange();
			});
			textureQualityDropdown.onValueChanged.AddListener(delegate
			{
				OnTextureChange();
			});
			shadowQualityDropdown.onValueChanged.AddListener(delegate
			{
				OnShadowChange();
			});
			antialiasingDropdown.onValueChanged.AddListener(delegate
			{
				OnAliasingChange();
			});
			VolumetricLighting.onValueChanged.AddListener(delegate
			{
				OnVolumetricChange();
			});
			AmbientOcclusion.onValueChanged.AddListener(delegate
			{
				AmbientOcclusionSetter();
			});
			MotionBlur.onValueChanged.AddListener(delegate
			{
				MotionBlurSetter();
			});
			EyeAdaptation.onValueChanged.AddListener(delegate
			{
				EyeAdaptationSetter();
			});
			Bloom.onValueChanged.AddListener(delegate
			{
				BloomSetter();
			});
			ChromaticAberration.onValueChanged.AddListener(delegate
			{
				ChromaticAberrationSetter();
			});

			// Hide resolution dropdown if we are not in fullscreen.
			_resolutionElement.SetActive(VideoData.Fullscreen);
		}

		public void SetDefault()
		{
			resolutions = Screen.resolutions;
			resolutionDropdown.options.Clear();
			Resolution[] array = resolutions;
			for (int i = 0; i < array.Length; i++)
			{
				Resolution resolution = array[i];
				resolutionDropdown.options.Add(new Dropdown.OptionData(resolution.ToString()));
			}
			qualityLevels = QualitySettings.names;
			qualitySettingsDropdown.options.Clear();
			string[] array2 = qualityLevels;
			foreach (string text in array2)
			{
				qualitySettingsDropdown.options.Add(new Dropdown.OptionData(text.ToString()));
			}

			qualitySettingsDropdown.onValueChanged.RemoveAllListeners();
			textureQualityDropdown.onValueChanged.RemoveAllListeners();
			shadowQualityDropdown.onValueChanged.RemoveAllListeners();
			antialiasingDropdown.onValueChanged.RemoveAllListeners();
			VolumetricLighting.onValueChanged.RemoveAllListeners();

			resolutionDropdown.value = resolutionDropdown.options.Count - 1;
			Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, Screen.fullScreen);

			// Resolution.
			fullScreenToggle.isOn = true;
			Screen.fullScreen = true;
			VideoData.Fullscreen = true;
			_resolutionElement.SetActive(true);

			// Ambient occlusion.
			AmbientOcclusion.isOn = true;
			VideoData.AmbientOcclusion = true;

			// Motion blur.
			MotionBlur.isOn = false;
			VideoData.MotionBlur = false;

			// Eye adaptation.
			EyeAdaptation.isOn = true;
			VideoData.EyeAdaptation = true;

			// Bloom
			Bloom.isOn = true;
			VideoData.Bloom = true;

			// Chromatic aberration.
			ChromaticAberration.isOn = true;
			VideoData.ChromaticAberration = true;

			// Quality.
			qualitySettingsDropdown.value = 2;
			VideoData.QualityIndex = qualitySettingsDropdown.value;
			QualitySettings.SetQualityLevel(VideoData.QualityIndex, applyExpensiveChanges: false);
			qualitySettingsDropdown.RefreshShownValue();
			qualitySettingsDropdown.onValueChanged.AddListener(delegate
			{
				OnQualityChange();
			});

			// Texture quality.
			VideoData.TextureIndex = QualitySettings.masterTextureLimit;
			textureQualityDropdown.value = VideoData.TextureIndex;
			textureQualityDropdown.RefreshShownValue();
			textureQualityDropdown.onValueChanged.AddListener(delegate
			{
				OnTextureChange();
			});

			// Shadow quality.
			VideoData.ShadowIndex = (int)QualitySettings.shadows;
			shadowQualityDropdown.value = VideoData.ShadowIndex;
			shadowQualityDropdown.RefreshShownValue();
			shadowQualityDropdown.onValueChanged.AddListener(delegate
			{
				OnShadowChange();
			});

			// Anti-aliasing.
			VideoData.AntialiasingIndex = 1;
			antialiasingDropdown.value = VideoData.AntialiasingIndex;
			antialiasingDropdown.RefreshShownValue();
			antialiasingDropdown.onValueChanged.AddListener(delegate
			{
				OnAliasingChange();
			});

			// Volumetric lighing. TODO: Off until I can fix it.
			VolumetricLighting.isOn = false;
			VideoData.VolumetricLights = false;
		}

		public void Load(VideoSettingsData videoData)
		{
			resolutions = Screen.resolutions;
			Resolution[] array = resolutions;
			for (int i = 0; i < array.Length; i++)
			{
				Resolution resolution = array[i];
				resolutionDropdown.options.Add(new Dropdown.OptionData(resolution.ToString()));
			}
			qualityLevels = QualitySettings.names;
			string[] array2 = qualityLevels;
			foreach (string text in array2)
			{
				qualitySettingsDropdown.options.Add(new Dropdown.OptionData(text.ToString()));
			}
			if (videoData == null)
			{
				SetDefault();
				return;
			}
			VideoData = videoData;
			resolutionDropdown.value = VideoData.ResolutionIndex;
			Screen.SetResolution(resolutions[resolutionDropdown.value].width, resolutions[resolutionDropdown.value].height, VideoData.Fullscreen);
			fullScreenToggle.isOn = VideoData.Fullscreen;
			SetQualityOnStart();

			textureQualityDropdown.value = VideoData.TextureIndex;
			textureQualityDropdown.RefreshShownValue();

			shadowQualityDropdown.value = VideoData.ShadowIndex;
			shadowQualityDropdown.RefreshShownValue();

			antialiasingDropdown.value = VideoData.AntialiasingIndex;
			antialiasingDropdown.RefreshShownValue();
			//VolumetricLighting.isOn = VideoData.VolumetricLights;
			//Client.Instance.VolumetricOption = VideoData.VolumetricLights;
			// Disabled.
			VolumetricLighting.isOn = false;
			Client.Instance.VolumetricOption = false;

			AmbientOcclusion.isOn = VideoData.AmbientOcclusion;
			MotionBlur.isOn = VideoData.MotionBlur;
			EyeAdaptation.isOn = VideoData.EyeAdaptation;
			Bloom.isOn = VideoData.Bloom;
			ChromaticAberration.isOn = VideoData.ChromaticAberration;
			UpdatePostEffects();
		}

		public void SaveVideoSettings()
		{
			VideoData.Fullscreen = fullScreenToggle.isOn;
			VideoData.QualityIndex = qualitySettingsDropdown.value;
			VideoData.ResolutionIndex = resolutionDropdown.value;
			VideoData.TextureIndex = textureQualityDropdown.value;
			VideoData.ShadowIndex = shadowQualityDropdown.value;
			VideoData.AntialiasingIndex = antialiasingDropdown.value;
			VideoData.VolumetricLights = VolumetricLighting.isOn;
			Client.Instance.VolumetricOption = VideoData.VolumetricLights;
			Client.Instance.AntialiasingOption = VideoData.AntialiasingIndex;
			QualitySettings.masterTextureLimit = VideoData.TextureIndex;
			QualitySettings.shadows = (ShadowQuality)VideoData.ShadowIndex;
			VideoData.AmbientOcclusion = AmbientOcclusion.isOn;
			VideoData.MotionBlur = MotionBlur.isOn;
			VideoData.EyeAdaptation = EyeAdaptation.isOn;
			VideoData.Bloom = Bloom.isOn;
			VideoData.ChromaticAberration = ChromaticAberration.isOn;
			VideoData.isSaved = true;
			UpdatePostEffects();
		}

		private void UpdatePostEffects()
		{
			foreach (PostProcessProfile postEffectProfile in PostEffectProfiles)
			{
				foreach (PostProcessEffectSettings setting in postEffectProfile.settings)
				{
					if (setting.name == "AmbientOcclusion")
					{
						setting.active = VideoData.AmbientOcclusion;
					}
					else if (setting.name == "MotionBlur")
					{
						setting.active = VideoData.MotionBlur;
					}
					else if (setting.name == "AutoExposure")
					{
						setting.active = VideoData.EyeAdaptation;
					}
					else if (setting.name == "Bloom")
					{
						setting.active = VideoData.Bloom;
					}
					else if (setting.name == "ChromaticAberration")
					{
						setting.active = VideoData.ChromaticAberration;
					}
				}
			}
		}

		public void OnFullscreenChange()
		{
			VideoSettingsData videoData = VideoData;
			bool fullscreen = (Screen.fullScreen = fullScreenToggle.isOn);
			videoData.Fullscreen = fullscreen;

			// Hide resolution dropdown if we are not in fullscreen.
			_resolutionElement.gameObject.SetActive(fullscreen);
		}

		public void OnResolutionChange()
		{
			Screen.SetResolution(resolutions[resolutionDropdown.value].width, resolutions[resolutionDropdown.value].height, VideoData.Fullscreen);
			VideoData.ResolutionIndex = resolutionDropdown.value;
			resolutionDropdown.RefreshShownValue();
		}

		public void SetResolutionOnStart()
		{
			if (resolutions != null)
			{
				Screen.SetResolution(resolutions[VideoData.ResolutionIndex].width, resolutions[VideoData.ResolutionIndex].height, VideoData.Fullscreen);
			}
			else
			{
				Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, VideoData.Fullscreen);
			}
		}

		public void OnQualityChange()
		{
			Client.Instance.ShowConfirmMessageBox(Localization.Video, Localization.SaveQualitySettings, Localization.Yes, Localization.No, ConfirmQualityChange);
		}

		public void OnTextureChange()
		{
			VideoData.TextureIndex = textureQualityDropdown.value;
		}

		public void OnShadowChange()
		{
			VideoData.ShadowIndex = shadowQualityDropdown.value;
		}

		public void OnAliasingChange()
		{
			VideoData.AntialiasingIndex = antialiasingDropdown.value;
		}

		public void OnVolumetricChange()
		{
			VideoData.VolumetricLights = VolumetricLighting.isOn;
		}

		public void ConfirmQualityChange()
		{
			VideoData.QualityIndex = qualitySettingsDropdown.value;
			QualitySettings.SetQualityLevel(VideoData.QualityIndex, applyExpensiveChanges: false);

			int masterTextureLimit = QualitySettings.masterTextureLimit;
			textureQualityDropdown.value = masterTextureLimit;
			VideoData.TextureIndex = masterTextureLimit;

			masterTextureLimit = (int)QualitySettings.shadows;
			shadowQualityDropdown.value = masterTextureLimit;
			VideoData.ShadowIndex = masterTextureLimit;

			VideoData.AntialiasingIndex = antialiasingDropdown.value;
			Client.Instance.AntialiasingOption = VideoData.AntialiasingIndex;

			VideoData.VolumetricLights = VolumetricLighting.isOn;
			Client.Instance.VolumetricOption = VideoData.VolumetricLights;

			qualitySettingsDropdown.RefreshShownValue();
			textureQualityDropdown.RefreshShownValue();
			shadowQualityDropdown.RefreshShownValue();
			antialiasingDropdown.RefreshShownValue();
		}

		public void SetQualityOnStart()
		{
			QualitySettings.SetQualityLevel(VideoData.QualityIndex, applyExpensiveChanges: true);
		}

		public void AmbientOcclusionSetter()
		{
			VideoData.AmbientOcclusion = AmbientOcclusion.isOn;
		}

		public void MotionBlurSetter()
		{
			VideoData.MotionBlur = MotionBlur.isOn;
		}

		public void EyeAdaptationSetter()
		{
			VideoData.EyeAdaptation = EyeAdaptation.isOn;
		}

		public void BloomSetter()
		{
			VideoData.Bloom = Bloom.isOn;
		}

		public void ChromaticAberrationSetter()
		{
			VideoData.ChromaticAberration = ChromaticAberration.isOn;
		}
	}
}
