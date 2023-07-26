using System;
using System.Collections.Generic;
using Photon.Voice;
using POpusCodec;
using POpusCodec.Enums;
using UnityEngine;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using OpenHellion.Net;

namespace ZeroGravity.Audio
{
	public class VoiceCommReceiver : MonoBehaviour
	{
		[SerializeField]
		public AudioSource audioSource;

		[SerializeField]
		private AnimationCurve volumeCurve = new AnimationCurve(new Keyframe
		{
			time = 0f,
			value = 1f,
			inTangent = -2f,
			outTangent = -3f
		}, new Keyframe
		{
			time = 1f,
			value = 0f,
			inTangent = 0f,
			outTangent = 0f
		});

		[SerializeField]
		private float maxAudibleDistance = 100f;

		[SerializeField]
		private float bulkheadFactor = 0.1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float naturalVoiceSpatialBlend = 0.8f;

		[SerializeField]
		private AudioReverbFilter naturalVoiceReverbFilter;

		[SerializeField]
		[Range(-10000f, 2000f)]
		private float reverbLeveNear;

		[SerializeField]
		[Range(-10000f, 2000f)]
		private float reverbLeveFar;

		[Space(10f)]
		[SerializeField]
		[Range(0f, 1f)]
		private float radioVoiceMaxVolume = 1f;

		[SerializeField]
		private Behaviour[] radioVoiceFilters;

		[SerializeField]
		private AnimationCurve NoiseCurve = new AnimationCurve(new Keyframe
		{
			time = 0f,
			value = 0f,
			inTangent = 0f,
			outTangent = 0f
		}, new Keyframe
		{
			time = 1f,
			value = 1f,
			inTangent = 2f,
			outTangent = 2f
		});

		[SerializeField]
		private float maxRadioDistance = 1000f;

		private AudioClip outAudioClip;

		private int samplerate = 48000;

		private OpusDecoder<float> opusDecoder;

		private List<float> audioData = new List<float>();

		private OtherPlayer thisPlayer;

		private bool isRadioComm;

		private float distance;

		private int emptyAudioFramesCounter;

		private void Start()
		{
			thisPlayer = GetComponentInParent<OtherPlayer>();
			if (audioSource == null)
			{
				audioSource = GetComponent<AudioSource>();
			}
			opusDecoder = new OpusDecoder<float>(SamplingRate.Sampling48000, Channels.Mono);
			outAudioClip = AudioClip.Create("VoiceCommAudioClip", samplerate * 10, 1, samplerate, true, OnAudioRead);
			audioSource.clip = outAudioClip;
			EventSystem.AddListener(typeof(VoiceCommDataMessage), VoiceCommDataMessageListener);
		}

		private void OnDestroy()
		{
			EventSystem.RemoveListener(typeof(VoiceCommDataMessage), VoiceCommDataMessageListener);
		}

		private void VoiceCommDataMessageListener(NetworkData data)
		{
			VoiceCommDataMessage voiceCommDataMessage = data as VoiceCommDataMessage;
			if (voiceCommDataMessage.SourceGUID != thisPlayer.GUID || voiceCommDataMessage.AudioPackets == null)
			{
				return;
			}
			foreach (byte[] audioPacket in voiceCommDataMessage.AudioPackets)
			{
				DecodeAudioPacket(audioPacket);
			}
			isRadioComm = voiceCommDataMessage.IsRadioComm;
			UpdateAudioParameters();
			if (MyPlayer.isAudioDebug)
			{
				Dbg.Log("Voice message arrived.");
			}
		}

		private void DecodeAudioPacket(byte[] bytes)
		{
			if (bytes.Length == 0 && audioSource.isPlaying)
			{
				audioSource.Stop();
				audioData.Clear();
				return;
			}
			FrameBuffer buf = new FrameBuffer(bytes, FrameFlags.PartialFrame);
			float[] collection = opusDecoder.DecodePacket(ref buf);
			// float[] collection = opusDecoder.DecodePacketFloat(bytes);
			audioData.AddRange(collection);
			if (!audioSource.isPlaying)
			{
				audioData.Clear();
				audioSource.Play();
				emptyAudioFramesCounter = 0;
			}
		}

		private void UpdateAudioParameters()
		{
			if (!(thisPlayer != null))
			{
				return;
			}
			if (isRadioComm)
			{
				distance = (MyPlayer.Instance.transform.position - thisPlayer.transform.position).magnitude;
				if (MyPlayer.isAudioDebug)
				{
					Dbg.Log("Voice distance " + distance);
				}
				if (distance <= maxRadioDistance)
				{
					naturalVoiceReverbFilter.enabled = false;
					Behaviour[] array = radioVoiceFilters;
					foreach (Behaviour behaviour in array)
					{
						behaviour.enabled = true;
					}
					audioSource.volume = radioVoiceMaxVolume;
					audioSource.spatialBlend = 0f;
				}
				else
				{
					audioSource.volume = 0f;
				}
			}
			else if (thisPlayer.CurrentRoomTrigger != null && thisPlayer.CurrentRoomTrigger.AirPressure > 0f)
			{
				float? num = MyPlayer.Instance.GetDistance(thisPlayer, out bool throughBulkhead);
				if (MyPlayer.isAudioDebug)
				{
					Dbg.Log("Voice distance " + num);
				}
				if (num.HasValue && num.Value <= maxAudibleDistance)
				{
					naturalVoiceReverbFilter.enabled = true;
					Behaviour[] array2 = radioVoiceFilters;
					foreach (Behaviour behaviour2 in array2)
					{
						behaviour2.enabled = false;
					}
					audioSource.volume = volumeCurve.Evaluate(Mathf.Clamp01(num.Value / maxAudibleDistance)) * ((!throughBulkhead) ? 1f : bulkheadFactor) * thisPlayer.CurrentRoomTrigger.AirPressure;
					naturalVoiceReverbFilter.reverbLevel = Mathf.Lerp(reverbLeveNear, reverbLeveFar, num.Value / maxAudibleDistance);
					audioSource.spatialBlend = naturalVoiceSpatialBlend;
				}
			}
			else
			{
				audioSource.volume = 0f;
			}
		}

		private void OnAudioRead(float[] data)
		{
			if (audioData.Count > 0)
			{
				if (audioData.Count >= data.Length)
				{
					if (maxRadioDistance > 0f && MathHelper.RandomRange(maxAudibleDistance / maxRadioDistance, 1f) > NoiseCurve.Evaluate(Mathf.Clamp01(distance / maxRadioDistance)))
					{
						Array.Copy(audioData.GetRange(0, data.Length).ToArray(), data, data.Length);
					}
					else
					{
						for (int i = 0; i < data.Length; i++)
						{
							data[i] = MathHelper.RandomRange(-0.1f, 0.1f);
						}
					}
					audioData.RemoveRange(0, data.Length);
				}
				else
				{
					Array.Copy(audioData.GetRange(0, audioData.Count).ToArray(), data, audioData.Count);
					Array.Clear(data, 0, data.Length - audioData.Count);
					audioData.Clear();
				}
				emptyAudioFramesCounter = 0;
			}
			else
			{
				Array.Clear(data, 0, data.Length);
				emptyAudioFramesCounter++;
			}
		}

		private void FixedUpdate()
		{
			if (emptyAudioFramesCounter > 10 && audioSource.isPlaying)
			{
				audioSource.Stop();
				audioData.Clear();
			}
		}
	}
}
