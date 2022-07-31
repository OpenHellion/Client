using System.IO;
using UnityEngine;

namespace CinemaDirector
{
	public class ScreenshotCapture : MonoBehaviour
	{
		public string Folder = "CaptureOutput";

		public int FrameRate = 24;

		public int UpSample = 1;

		private void Start()
		{
			Time.captureFramerate = FrameRate;
			Directory.CreateDirectory(Folder);
		}

		private void Update()
		{
			string filename = string.Format("{0}/shot {1:D04}.png", Folder, Time.frameCount);
			ScreenCapture.CaptureScreenshot(filename, UpSample);
		}
	}
}
