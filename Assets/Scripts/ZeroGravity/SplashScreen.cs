using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

namespace ZeroGravity
{
	public class SplashScreen : MonoBehaviour
	{
		[Serializable]
		public class VideoObject
		{
			public VideoClip Video;

			public SoundEffect Sound;
		}

		public VideoPlayer VideoPlayer;

		public GameObject PressAnyToContinue;

		public List<VideoObject> VideoClips;

		private bool m_soundPlaying;

		private SoundEffect m_soundEffect;

		private Task m_videoEndTask;

		private void EndReached(VideoPlayer vp)
		{
			VideoEnd();
		}

		private void Started(VideoPlayer vp)
		{
			if (!m_soundPlaying)
			{
				m_soundEffect.Play(0);
				m_soundPlaying = true;
			}
		}

		private void Update()
		{
			// If any key, including left mouse button, have been pressed.
			if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
			{
				if (!PressAnyToContinue.activeInHierarchy) // First time clicking.
				{
					// Enable the text to continue.
					PressAnyToContinue.Activate(true);
				}
				else if (PressAnyToContinue.activeInHierarchy) // Second time clicking.
				{
					// End the video.
					VideoEnd();
				}
			}
		}

		public void StartVideo(int clip)
		{
			if (VideoClips[clip] != null)
			{
				if (VideoClips[clip].Sound != null)
				{
					m_soundEffect = VideoClips[clip].Sound;
				}
				VideoPlayer.loopPointReached += EndReached;
				VideoPlayer.started += Started;
				VideoPlayer.clip = VideoClips[clip].Video;
				gameObject.Activate(true);
				VideoPlayer.Play();
			}
			else
			{
				VideoEnd();
			}
		}

		public void FreshStart(Task tsk)
		{
			m_videoEndTask = tsk;
			StartVideo(1);
		}

		public void VideoEnd()
		{
			// Run video end task.
			if (m_videoEndTask != null)
			{
				m_videoEndTask.RunSynchronously();
				m_videoEndTask = null;
			}

			// Hide the text.
			PressAnyToContinue.Activate(false);
			if (m_soundEffect != null)
			{
				m_soundEffect.Play(1);
			}

			// Stop playing.
			VideoPlayer.Stop();
			VideoPlayer.clip = null;
			m_soundPlaying = false;
			gameObject.Activate(false);
		}
	}
}
