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

		public SoundEffect SoundEffect;

		private bool soundPlaying;

		public GameObject PressAnyToContinue;

		public List<VideoObject> VideoClips;

		private Task runAfterVideo;

		private void EndReached(VideoPlayer vp)
		{
			VideoEnd();
		}

		private void Started(VideoPlayer vp)
		{
			if (!soundPlaying)
			{
				SoundEffect.Play(0);
				soundPlaying = true;
			}
		}

		private void Update()
		{
			if (Input.anyKeyDown && PressAnyToContinue.activeInHierarchy)
			{
				//StopVideo();
				VideoEnd();
			}
			else if (Input.anyKeyDown && !PressAnyToContinue.activeInHierarchy)
			{
				PressAnyToContinue.Activate(true);
			}
		}

		public void StartVideo(int clip)
		{
			if (VideoClips[clip] != null)
			{
				if (VideoClips[clip].Sound != null)
				{
					SoundEffect = VideoClips[clip].Sound;
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
			runAfterVideo = tsk;
			StartVideo(1);
		}

		public void VideoEnd()
		{
			if (VideoPlayer.clip == VideoClips[0].Video)
			{
				Client.Instance.SceneLoader.InitializeScenes();
			}
			if (runAfterVideo != null)
			{
				runAfterVideo.RunSynchronously();
				runAfterVideo = null;
			}
			PressAnyToContinue.Activate(false);
			if (SoundEffect != null)
			{
				SoundEffect.Play(1);
			}
			VideoPlayer.Stop();
			VideoPlayer.clip = null;
			soundPlaying = false;
			base.gameObject.Activate(false);
		}
	}
}
