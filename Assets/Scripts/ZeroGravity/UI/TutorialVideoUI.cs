using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ZeroGravity.UI
{
	public class TutorialVideoUI : MonoBehaviour
	{
		public TutorialVideoManager Manager;

		public VideoClip Video;

		public Text Name;

		public GameObject Selected;

		public void Update()
		{
			if (Manager.VideoSource.clip != Video)
			{
				Selected.Activate(false);
			}
		}

		public void SelectVideo()
		{
			Manager.SetVideo(Video);
			Selected.Activate(true);
		}
	}
}
