using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;

namespace ZeroGravity.UI
{
	public class AudioSettings : MonoBehaviour
	{
		public AudioSettingsData AudioData = new AudioSettingsData();

		public Text volumeLevel;

		public Slider volumeSlider;

		public Text AmbienceVolume;

		public Slider AmbienceSlider;

		public Text voiceVolume;

		public Slider voiceVolSlider;

		private float currentVoice;

		private float currentAmbience;

		private float currentVolume;

		public float VoiceVolumeSetter
		{
			get
			{
				return currentVoice;
			}
			set
			{
				currentVoice = value;
				voiceVolSlider.value = value;
				voiceVolume.text = value.ToString();
			}
		}

		public float AmbienceVolumeSetter
		{
			get
			{
				return currentAmbience;
			}
			set
			{
				currentAmbience = value;
				AmbienceSlider.value = value;
				AmbienceVolume.text = value.ToString();
			}
		}

		public float VolumeSetter
		{
			get
			{
				return currentVolume;
			}
			set
			{
				currentVolume = value;
				volumeSlider.value = value;
				volumeLevel.text = value.ToString();
			}
		}

		private void Start()
		{
			volumeSlider.onValueChanged.AddListener(VolumeSlider);
			voiceVolSlider.onValueChanged.AddListener(VoiceVolumeSlider);
			AmbienceSlider.onValueChanged.AddListener(AmbienceVolumeSlider);
		}

		public void VolumeSlider(float val)
		{
			VolumeSetter = val;
			AkSoundEngine.SetRTPCValue(SoundManager.Instance.MasterVolume, VolumeSetter);
			AudioData.Volume = VolumeSetter;
		}

		public void VoiceVolumeSlider(float val)
		{
			VoiceVolumeSetter = val;
			AudioListener.volume = VolumeSetter;
			AudioData.VoiceVolume = VoiceVolumeSetter;
		}

		public void AmbienceVolumeSlider(float val)
		{
			AmbienceVolumeSetter = val;
			AkSoundEngine.SetRTPCValue(SoundManager.Instance.AmbienceVolume, AmbienceVolumeSetter);
			AudioData.AmbienceVolume = AmbienceVolumeSetter;
		}

		public void SetDefault()
		{
			VoiceVolumeSetter = 100f;
			VolumeSetter = 80f;
			AmbienceVolumeSetter = 100f;
			AudioData.Volume = VolumeSetter;
			AudioData.VoiceVolume = VoiceVolumeSetter;
			AudioData.AmbienceVolume = AmbienceVolumeSetter;
		}

		public void Load(AudioSettingsData audioData)
		{
			if (audioData == null)
			{
				SetDefault();
				return;
			}
			AudioData = audioData;
			VolumeSetter = AudioData.Volume;
			VoiceVolumeSetter = AudioData.VoiceVolume;
			AmbienceVolumeSetter = AudioData.AmbienceVolume;
			AkSoundEngine.SetRTPCValue(SoundManager.Instance.MasterVolume, AudioData.Volume);
			AkSoundEngine.SetRTPCValue(SoundManager.Instance.AmbienceVolume, AudioData.AmbienceVolume);
			AudioListener.volume = AudioData.VoiceVolume / 100f;
		}

		private void Update()
		{
		}

		public void SaveAudioSettings()
		{
			AkSoundEngine.SetRTPCValue(SoundManager.Instance.MasterVolume, AudioData.Volume);
			AkSoundEngine.SetRTPCValue(SoundManager.Instance.AmbienceVolume, AudioData.AmbienceVolume);
			AudioData.VoiceVolume = VoiceVolumeSetter;
			AudioData.Volume = VolumeSetter;
			AudioData.AmbienceVolume = AmbienceVolumeSetter;
		}
	}
}
