using System;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class ManeuverPanelUI : MonoBehaviour
	{
		private UpdateTimer Timer = new UpdateTimer(0.4f);

		public Text MessageText;

		private string message = string.Empty;

		public Text TimerText;

		public float TimerLeft;

		public bool TurnOffAfterTimer;

		public NavigationPanel NavPanel;

		public string Message
		{
			get { return message; }
			set
			{
				message = value;
				MessageText.text = value;
			}
		}

		private void Update()
		{
			if (Timer.Update())
			{
				TimerText.text = GetTimeFormat(TimerLeft);
			}

			if (TimerLeft >= 0f)
			{
				TimerLeft -= Time.deltaTime;
			}
			else if (TurnOffAfterTimer)
			{
				TurnOffAfterTimer = false;
				gameObject.SetActive(false);
			}
		}

		private string GetTimeFormat(float time)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(time);
			return timeSpan.Hours.ToString("f0") + ": " + timeSpan.Minutes.ToString("f0") + ": " +
			       timeSpan.Seconds.ToString("f0");
		}
	}
}
