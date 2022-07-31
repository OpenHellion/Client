using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
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

		private void Update()
		{
		}

		private void Awake()
		{
		}

		private void Start()
		{
			qualitySettingsDropdown.value = VideoData.QualityIndex;
			qualitySettingsDropdown.RefreshShownValue();
			fullScreenToggle.onValueChanged.AddListener(_003CStart_003Em__0);
			resolutionDropdown.onValueChanged.AddListener(_003CStart_003Em__1);
			qualitySettingsDropdown.onValueChanged.AddListener(_003CStart_003Em__2);
			textureQualityDropdown.onValueChanged.AddListener(_003CStart_003Em__3);
			shadowQualityDropdown.onValueChanged.AddListener(_003CStart_003Em__4);
			antialiasingDropdown.onValueChanged.AddListener(_003CStart_003Em__5);
			VolumetricLighting.onValueChanged.AddListener(_003CStart_003Em__6);
			AmbientOcclusion.onValueChanged.AddListener(_003CStart_003Em__7);
			MotionBlur.onValueChanged.AddListener(_003CStart_003Em__8);
			EyeAdaptation.onValueChanged.AddListener(_003CStart_003Em__9);
			Bloom.onValueChanged.AddListener(_003CStart_003Em__A);
			ChromaticAberration.onValueChanged.AddListener(_003CStart_003Em__B);
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
			bool flag = true;
			fullScreenToggle.isOn = flag;
			Screen.fullScreen = flag;
			VideoData.Fullscreen = true;
			VideoSettingsData videoData = VideoData;
			flag = true;
			AmbientOcclusion.isOn = flag;
			videoData.AmbientOcclusion = flag;
			VideoSettingsData videoData2 = VideoData;
			flag = true;
			MotionBlur.isOn = flag;
			videoData2.MotionBlur = flag;
			VideoSettingsData videoData3 = VideoData;
			flag = true;
			EyeAdaptation.isOn = flag;
			videoData3.EyeAdaptation = flag;
			VideoSettingsData videoData4 = VideoData;
			flag = true;
			Bloom.isOn = flag;
			videoData4.Bloom = flag;
			VideoSettingsData videoData5 = VideoData;
			flag = true;
			ChromaticAberration.isOn = flag;
			videoData5.ChromaticAberration = flag;
			qualitySettingsDropdown.value = 2;
			VideoData.QualityIndex = qualitySettingsDropdown.value;
			QualitySettings.SetQualityLevel(VideoData.QualityIndex, false);
			qualitySettingsDropdown.RefreshShownValue();
			qualitySettingsDropdown.onValueChanged.AddListener(_003CSetDefault_003Em__C);
			VideoData.TextureIndex = QualitySettings.masterTextureLimit;
			textureQualityDropdown.value = VideoData.TextureIndex;
			textureQualityDropdown.RefreshShownValue();
			textureQualityDropdown.onValueChanged.AddListener(_003CSetDefault_003Em__D);
			VideoData.ShadowIndex = (int)QualitySettings.shadows;
			shadowQualityDropdown.value = VideoData.ShadowIndex;
			shadowQualityDropdown.RefreshShownValue();
			shadowQualityDropdown.onValueChanged.AddListener(_003CSetDefault_003Em__E);
			VideoData.AntialiasingIndex = 1;
			antialiasingDropdown.value = VideoData.AntialiasingIndex;
			antialiasingDropdown.RefreshShownValue();
			antialiasingDropdown.onValueChanged.AddListener(_003CSetDefault_003Em__F);
			VideoSettingsData videoData6 = VideoData;
			flag = true;
			VolumetricLighting.isOn = flag;
			videoData6.VolumetricLights = flag;
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
			VolumetricLighting.isOn = VideoData.VolumetricLights;
			Client.Instance.VolumetricOption = VideoData.VolumetricLights;
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
			QualitySettings.SetQualityLevel(VideoData.QualityIndex, false);
			VideoSettingsData videoData = VideoData;
			int masterTextureLimit = QualitySettings.masterTextureLimit;
			textureQualityDropdown.value = masterTextureLimit;
			videoData.TextureIndex = masterTextureLimit;
			VideoSettingsData videoData2 = VideoData;
			masterTextureLimit = (int)QualitySettings.shadows;
			shadowQualityDropdown.value = masterTextureLimit;
			videoData2.ShadowIndex = masterTextureLimit;
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
			QualitySettings.SetQualityLevel(VideoData.QualityIndex, true);
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

		[CompilerGenerated]
		private void _003CStart_003Em__0(bool P_0)
		{
			OnFullscreenChange();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__1(int P_0)
		{
			OnResolutionChange();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__2(int P_0)
		{
			OnQualityChange();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__3(int P_0)
		{
			OnTextureChange();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__4(int P_0)
		{
			OnShadowChange();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__5(int P_0)
		{
			OnAliasingChange();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__6(bool P_0)
		{
			OnVolumetricChange();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__7(bool P_0)
		{
			AmbientOcclusionSetter();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__8(bool P_0)
		{
			MotionBlurSetter();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__9(bool P_0)
		{
			EyeAdaptationSetter();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__A(bool P_0)
		{
			BloomSetter();
		}

		[CompilerGenerated]
		private void _003CStart_003Em__B(bool P_0)
		{
			ChromaticAberrationSetter();
		}

		[CompilerGenerated]
		private void _003CSetDefault_003Em__C(int P_0)
		{
			OnQualityChange();
		}

		[CompilerGenerated]
		private void _003CSetDefault_003Em__D(int P_0)
		{
			OnTextureChange();
		}

		[CompilerGenerated]
		private void _003CSetDefault_003Em__E(int P_0)
		{
			OnShadowChange();
		}

		[CompilerGenerated]
		private void _003CSetDefault_003Em__F(int P_0)
		{
			OnAliasingChange();
		}
	}
}
