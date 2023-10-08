using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ZeroGravity.UI
{
	public class TutorialVideoManager : MonoBehaviour
	{
		public VideoPlayer VideoSource;

		public GameObject Holder;

		public List<TutorialVideo> TutorialVideos;

		public GameObject VideoObject;

		public Transform VideoListTransform;

		public Button Play;

		public Button Pause;

		private void Start()
		{
			foreach (TutorialVideo tutorialVideo in TutorialVideos)
			{
				GameObject gameObject = Object.Instantiate(VideoObject, VideoListTransform);
				gameObject.Activate(true);
				TutorialVideoUI component = gameObject.GetComponent<TutorialVideoUI>();
				component.Video = tutorialVideo.Video;
				component.Manager = this;
				component.Name.text = tutorialVideo.Name;
			}
		}

		private void Update()
		{
			if (VideoSource.clip == null)
			{
				Holder.Activate(false);
				return;
			}

			Holder.Activate(true);
			if (VideoSource.isPlaying)
			{
				Play.interactable = false;
				Pause.interactable = true;
			}
			else
			{
				Play.interactable = true;
				Pause.interactable = false;
			}
		}

		public void SetVideo(VideoClip video)
		{
			VideoSource.clip = video;
		}

		public void PlayVideo()
		{
			VideoSource.Play();
		}

		public void StopVideo()
		{
			VideoSource.Pause();
		}
	}
}
