using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ZeroGravity.Audio
{
	public class SoundSource : MonoBehaviour
	{
		public List<AudioClip> Sounds = new List<AudioClip>();

		public AudioClip CloseSound;

		public AudioClip DistantSound;

		public AudioClip ObscuredSound;

		public AudioMixerGroup CloseMixerGroup;

		public AudioMixerGroup DistantMixerGroup;

		public AudioMixerGroup ObscuredMixerGroup;

		private AudioSource CloseAudioSource;

		private AudioSource DistantAudioSource;

		private AudioSource ObscuredAudioSource;

		public bool PlayOnAwake;

		public bool Loop;

		[Range(0f, 1f)]
		public float Volume = 1f;

		[Range(0f, 1f)]
		public float Pitch = 1f;

		public float MinDistance = 2f;

		public float MaxDistance = 5f;

		[Range(0f, 1f)]
		public float SpatialBlend = 1f;

		public AnimationCurve CloseVolumeCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.5f, 0f), new Keyframe(1f, 0f));

		public AnimationCurve DistantVolumeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

		public AnimationCurve ObscuredVolumeCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.5f, 0f), new Keyframe(1f, 0f));

		private float fadeSpeed;

		private int closeIndex;

		private void Awake()
		{
			if ((bool)CloseSound)
			{
				CloseAudioSource = base.gameObject.AddComponent<AudioSource>();
				CloseAudioSource.clip = CloseSound;
				CloseAudioSource.outputAudioMixerGroup = CloseMixerGroup;
				CloseAudioSource.playOnAwake = PlayOnAwake;
				CloseAudioSource.loop = Loop;
				CloseAudioSource.minDistance = MinDistance;
				CloseAudioSource.maxDistance = MaxDistance;
				CloseAudioSource.spatialBlend = SpatialBlend;
				CloseAudioSource.rolloffMode = AudioRolloffMode.Custom;
				CloseAudioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, CloseVolumeCurve);
				CloseAudioSource.volume = Volume;
				CloseAudioSource.pitch = Pitch;
			}
			if ((bool)DistantSound)
			{
				DistantAudioSource = base.gameObject.AddComponent<AudioSource>();
				DistantAudioSource.clip = DistantSound;
				DistantAudioSource.outputAudioMixerGroup = DistantMixerGroup;
				DistantAudioSource.playOnAwake = PlayOnAwake;
				DistantAudioSource.loop = Loop;
				DistantAudioSource.minDistance = MinDistance;
				DistantAudioSource.maxDistance = MaxDistance;
				DistantAudioSource.spatialBlend = SpatialBlend;
				DistantAudioSource.rolloffMode = AudioRolloffMode.Custom;
				DistantAudioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, DistantVolumeCurve);
				DistantAudioSource.volume = Volume;
				DistantAudioSource.pitch = Pitch;
			}
			if ((bool)ObscuredSound)
			{
				ObscuredAudioSource = base.gameObject.AddComponent<AudioSource>();
				ObscuredAudioSource.clip = ObscuredSound;
				ObscuredAudioSource.outputAudioMixerGroup = ObscuredMixerGroup;
				ObscuredAudioSource.playOnAwake = PlayOnAwake;
				ObscuredAudioSource.loop = Loop;
				ObscuredAudioSource.minDistance = MinDistance;
				ObscuredAudioSource.maxDistance = MaxDistance;
				ObscuredAudioSource.spatialBlend = SpatialBlend;
				ObscuredAudioSource.rolloffMode = AudioRolloffMode.Custom;
				ObscuredAudioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, ObscuredVolumeCurve);
				ObscuredAudioSource.volume = Volume;
				ObscuredAudioSource.pitch = Pitch;
			}
			if (PlayOnAwake && ((bool)CloseSound || (bool)DistantSound || (bool)ObscuredSound))
			{
				PlaySound();
			}
		}

		public void PlaySound()
		{
			if ((bool)CloseSound && CloseAudioSource.enabled)
			{
				if (Sounds.Count > 0)
				{
					RandomIndexClose();
					CloseAudioSource.clip = Sounds[closeIndex];
				}
				CloseAudioSource.Play();
			}
			if ((bool)DistantSound && DistantAudioSource.enabled)
			{
				DistantAudioSource.Play();
			}
			if ((bool)ObscuredSound && ObscuredAudioSource.enabled)
			{
				ObscuredAudioSource.Play();
			}
		}

		public void StopSound()
		{
			if ((bool)CloseSound)
			{
				CloseAudioSource.Stop();
			}
			if ((bool)DistantSound)
			{
				DistantAudioSource.Stop();
			}
			if ((bool)ObscuredSound)
			{
				ObscuredAudioSource.Stop();
			}
		}

		public bool IsPlaying()
		{
			bool flag = false;
			if ((bool)CloseSound)
			{
				flag = CloseAudioSource.isPlaying;
			}
			if ((bool)DistantSound && !flag)
			{
				flag = DistantAudioSource.isPlaying;
			}
			if ((bool)ObscuredSound && !flag)
			{
				flag = DistantAudioSource.isPlaying;
			}
			return flag;
		}

		public void FadeIn(float duration)
		{
			if (duration > 0f)
			{
				float currentVolume = getCurrentVolume();
				fadeSpeed = Volume - currentVolume / duration;
			}
			else
			{
				setVolume(Volume);
			}
		}

		public void FadeOut(float duration)
		{
			if (duration > 0f)
			{
				float currentVolume = getCurrentVolume();
				fadeSpeed = (0f - currentVolume) / duration;
			}
			else
			{
				setVolume(0f);
			}
		}

		public void Mute()
		{
			FadeOut(0f);
		}

		public void UnMute()
		{
			FadeIn(0f);
		}

		private void Update()
		{
			if (fadeSpeed != 0f)
			{
				float num = getCurrentVolume() + fadeSpeed * Time.deltaTime;
				if (num > 1f || num < 0f)
				{
					num = Mathf.Clamp01(num);
					fadeSpeed = 0f;
				}
				setVolume(num);
			}
		}

		private float getCurrentVolume()
		{
			if ((bool)CloseSound)
			{
				return CloseAudioSource.volume;
			}
			if ((bool)DistantSound)
			{
				return DistantAudioSource.volume;
			}
			if ((bool)ObscuredSound)
			{
				return ObscuredAudioSource.volume;
			}
			return Volume;
		}

		private void setVolume(float newVolume)
		{
			if ((bool)CloseSound)
			{
				CloseAudioSource.volume = newVolume;
			}
			if ((bool)DistantSound)
			{
				DistantAudioSource.volume = newVolume;
			}
			if ((bool)ObscuredSound)
			{
				ObscuredAudioSource.volume = newVolume;
			}
		}

		private void RandomIndexClose()
		{
			int num = Random.Range(0, Sounds.Count);
			if (num != closeIndex)
			{
				closeIndex = num;
			}
			else
			{
				RandomIndexClose();
			}
		}
	}
}
