using System.Collections.Generic;
using UnityEngine;

namespace ZeroGravity.Audio
{
	public class SoundRandomizer : MonoBehaviour
	{
		public List<SoundSource> SoundSources;

		public float Frequency = 1f;

		public float RandomFrequencyOffset = 1f;

		private float CurrentFrequency = 1f;

		private void Update()
		{
			PlayRandomSound();
		}

		private void PlayRandomSound()
		{
			if (CurrentFrequency > 0f)
			{
				CurrentFrequency -= Time.deltaTime;
				return;
			}
			int index = Random.Range(0, SoundSources.Count - 1);
			if (!SoundSources[index].IsPlaying())
			{
				SoundSources[index].PlaySound();
			}
			CurrentFrequency = Random.Range(Frequency - RandomFrequencyOffset, Frequency + RandomFrequencyOffset);
		}
	}
}
