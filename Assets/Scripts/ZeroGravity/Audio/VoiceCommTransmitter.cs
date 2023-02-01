using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using POpusCodec;
using POpusCodec.Enums;
using Steamworks;
using UnityEngine;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.UI;
using OpenHellion.ProviderSystem;
using OpenHellion.Util;

namespace ZeroGravity.Audio
{
	public class VoiceCommTransmitter : MonoBehaviour
	{
		private AudioClip inAudioClip;

		private string micDevice;

		private int samplerate = 48000;

		private bool talk;

		private bool radio;

		private OpusEncoder opusEncoder;

		private int lastSample;

		[SerializeField]
		private float maxRadioDistance = 1000f;

		private void Start()
		{
			opusEncoder = new OpusEncoder(SamplingRate.Sampling48000, Channels.Mono, 56000, OpusApplicationType.Voip, Delay.Delay40ms);
		}

		private void Update()
		{
			if (ProviderManager.SteamId.IsNullOrEmpty())
			{
				return;
			}
			if (InputController.GetButtonDown(InputController.AxisNames.CapsLock) && !talk)
			{
				if (Microphone.devices.Length > 0)
				{
					if (MyPlayer.isAudioDebug)
					{
						Dbg.Info("Start " + AudioListener.volume);
					}
					micDevice = Microphone.devices[0];
					inAudioClip = Microphone.Start(micDevice, loop: true, 5, samplerate);
					lastSample = 0;
					radio = false;
					talk = true;
				}
			}
			else if (InputController.GetButtonDown(InputController.AxisNames.Tilda) && !talk)
			{
				if (Microphone.devices.Length > 0)
				{
					micDevice = Microphone.devices[0];
					inAudioClip = Microphone.Start(micDevice, loop: true, 5, samplerate);
					lastSample = 0;
					radio = true;
					talk = true;
				}
			}
			else if ((InputController.GetButtonUp(InputController.AxisNames.CapsLock) || InputController.GetButtonUp(InputController.AxisNames.Tilda)) && talk)
			{
				if (MyPlayer.isAudioDebug)
				{
					Dbg.Info("Stop " + AudioListener.volume);
				}
				inAudioClip = null;
				talk = false;
			}
		}

		private void FixedUpdate()
		{
			if (ProviderManager.SteamId.IsNullOrEmpty())
			{
				return;
			}
			SceneTriggerRoom currentRoomTrigger = MyPlayer.Instance.CurrentRoomTrigger;
			Helmet currentHelmet = MyPlayer.Instance.CurrentHelmet;
			if (!talk || !(inAudioClip != null) || ((!(currentRoomTrigger != null) || !(currentRoomTrigger.AirPressure > 0f)) && (!(currentHelmet != null) || !currentHelmet.IsVisorActive)))
			{
				return;
			}
			int num = -1;
			List<byte[]> list = new List<byte[]>();
			while (true)
			{
				int num2 = Microphone.GetPosition(micDevice);
				if (num2 == num)
				{
					break;
				}
				if (num2 < lastSample)
				{
					num2 += inAudioClip.samples;
				}
				int num3 = num2 - lastSample;
				int num4 = opusEncoder.FrameSizePerChannel * inAudioClip.channels;
				if (num3 >= num4)
				{
					float[] array = new float[opusEncoder.FrameSizePerChannel * inAudioClip.channels];
					if (lastSample + num4 > inAudioClip.samples)
					{
						float[] array2 = new float[inAudioClip.samples - lastSample];
						float[] array3 = new float[num4 - (inAudioClip.samples - lastSample)];
						if (array2.Length > 0)
						{
							inAudioClip.GetData(array2, lastSample);
							Array.Copy(array2, 0, array, array3.Length, array2.Length);
						}
						inAudioClip.GetData(array3, 0);
						Array.Copy(array3, 0, array, 0, array3.Length);
					}
					else
					{
						inAudioClip.GetData(array, lastSample);
					}
					lastSample += opusEncoder.FrameSizePerChannel * inAudioClip.channels;
					if (lastSample > inAudioClip.samples)
					{
						lastSample %= inAudioClip.samples;
					}
					ArraySegment<byte> arraySegment = opusEncoder.Encode(array);
					byte[] array4 = new byte[arraySegment.Count];
					Array.Copy(arraySegment.Array, array4, arraySegment.Count);
					list.Add(array4);
				}
				num = num2;
			}
			if (list.Count > 0)
			{
				VoiceCommDataMessage voiceCommDataMessage = new VoiceCommDataMessage
				{
					SourceGUID = MyPlayer.Instance.GUID,
					IsRadioComm = radio,
					AudioPackets = list
				};
				byte[] msgBytes = ProtoSerialiser.Package(voiceCommDataMessage);
				HashSet<OtherPlayer> hashSet = new HashSet<OtherPlayer>();
				hashSet.UnionWith(GetAllPlayersFromSameVessel());
				if (radio)
				{
					hashSet.UnionWith(GetAllPlayersWithinRadius(transform.position, maxRadioDistance));
				}
				SendP2PPacketToPlayers(hashSet, msgBytes);
			}
		}

		private static IEnumerable<OtherPlayer> GetAllPlayersFromSameVessel()
		{
			if (MyPlayer.Instance.Parent is SpaceObjectVessel)
			{
				SpaceObjectVessel mainVessel = (MyPlayer.Instance.Parent as SpaceObjectVessel).MainVessel;
				return mainVessel.GetComponentsInChildren<OtherPlayer>();
			}
			return new OtherPlayer[0];
		}

		private static IEnumerable<OtherPlayer> GetAllPlayersWithinRadius(Vector3 position, float radius)
		{
			float num = radius * radius;
			HashSet<OtherPlayer> hashSet = new HashSet<OtherPlayer>();
			foreach (OtherPlayer value in Client.Instance.Players.Values)
			{
				if ((value.transform.position - position).sqrMagnitude < num)
				{
					hashSet.Add(value);
				}
			}
			return hashSet;
		}

		private static void SendP2PPacketToPlayers(ICollection<OtherPlayer> otherPlayers, byte[] msgBytes)
		{
			new Thread((ThreadStart)delegate
			{
				// Create int pointer to copy the data to.
				IntPtr msg = IntPtr.Zero;
				Marshal.Copy(msgBytes, 0, msg, msgBytes.Length);

				foreach (OtherPlayer otherPlayer in otherPlayers)
				{
					try
					{
						// Get player's steam identity.
						SteamNetworkingIdentity sni = new SteamNetworkingIdentity();
						sni.SetSteamID(new CSteamID(ulong.Parse(otherPlayer.NativeId)));

						// https://partner.steamgames.com/doc/api/steamnetworkingtypes#message_sending_flags
						SteamNetworkingMessages.SendMessageToUser(ref sni, msg, (uint) msgBytes.Length, 8, 0);
					}
					catch
					{
						Dbg.Error("Couldn't send audio packet.");
					}
				}
			}).Start();
		}
	}
}
