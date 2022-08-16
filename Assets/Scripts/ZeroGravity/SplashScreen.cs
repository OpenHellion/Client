using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
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

		private SoundEffect _soundEffect;

		private bool _soundPlaying;

		public GameObject PressAnyToContinue;

		public List<VideoObject> VideoClips;

		private Task _videoEndTask;

		private void EndReached(VideoPlayer vp)
		{
			VideoEnd();
		}

		private void Started(VideoPlayer vp)
		{
			if (!_soundPlaying)
			{
				_soundEffect.Play(0);
				_soundPlaying = true;
			}
		}

		private void Update()
		{
			if (Input.anyKeyDown && !PressAnyToContinue.activeInHierarchy)
			{
				PressAnyToContinue.Activate(true);
			}
			else if (Input.anyKeyDown && PressAnyToContinue.activeInHierarchy)
			{
				//StopVideo();
				VideoEnd();
			}
		}

		public void StartVideo(int clip)
		{
			if (VideoClips[clip] != null)
			{
				if (VideoClips[clip].Sound != null)
				{
					_soundEffect = VideoClips[clip].Sound;
				}
				VideoPlayer.loopPointReached += EndReached;
				VideoPlayer.started += Started;
				VideoPlayer.clip = VideoClips[clip].Video;
				base.gameObject.Activate(true);
				VideoPlayer.Play();
			}
			else
			{
				VideoEnd();
			}
		}

		public void StopVideo()
		{
			GetComponent<Animator>().SetTrigger("Stop");
		}

		public void FreshStart(Task tsk)
		{
			_videoEndTask = tsk;
			StartVideo(1);
		}

		public void VideoEnd()
		{
			if (VideoPlayer.clip == VideoClips[0].Video)
			{
				Client.Instance.SceneLoader.InitializeScenes();
			}

			// Run video end task.
			if (_videoEndTask != null)
			{
				_videoEndTask.RunSynchronously();
				_videoEndTask = null;
			}

			// Hide the text.
			PressAnyToContinue.Activate(false);
			if (_soundEffect != null)
			{
				_soundEffect.Play(1);
			}

			// Stop playing.
			VideoPlayer.Stop();
			VideoPlayer.clip = null;
			_soundPlaying = false;
			base.gameObject.Activate(false);
		}
	}
}
