using UnityEngine;
using UnityEngine.UI;

public class ManeuverWindow : MonoBehaviour
{
	public Text StartTimeSeconds;

	public Text StartTimeMinutes;

	public Text StartTimeHours;

	public Text EndTimeSeconds;

	public Text EndTimeMinutes;

	public Text EndTimeHours;

	public void SetStartTime(float time)
	{
		int num = (int)Mathf.Floor(time % 60f);
		if (num < 10)
		{
			StartTimeSeconds.text = "0" + num;
		}
		else
		{
			StartTimeSeconds.text = num.ToString();
		}

		int num2 = (int)Mathf.Floor(time / 60f % 60f);
		if (num2 < 10)
		{
			StartTimeMinutes.text = "0" + num2;
		}
		else
		{
			StartTimeMinutes.text = num2.ToString();
		}

		int num3 = (int)Mathf.Floor(time / 3600f);
		if (num3 < 10)
		{
			StartTimeHours.text = "0" + num3;
		}
		else
		{
			StartTimeHours.text = num3.ToString();
		}
	}

	public void SetEndTime(float time)
	{
		int num = (int)Mathf.Floor(time % 60f);
		if (num < 10)
		{
			EndTimeSeconds.text = "0" + num;
		}
		else
		{
			EndTimeSeconds.text = num.ToString();
		}

		int num2 = (int)Mathf.Floor(time / 60f % 60f);
		if (num2 < 10)
		{
			EndTimeMinutes.text = "0" + num2;
		}
		else
		{
			EndTimeMinutes.text = num2.ToString();
		}

		int num3 = (int)Mathf.Floor(time / 3600f);
		if (num3 < 10)
		{
			EndTimeHours.text = "0" + num3;
		}
		else
		{
			EndTimeHours.text = num3.ToString();
		}
	}
}
