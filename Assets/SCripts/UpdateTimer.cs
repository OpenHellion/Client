using UnityEngine;

public class UpdateTimer
{
	private float timeInterval;

	private float timePassed;

	public UpdateTimer(float intervalSeconds)
	{
		timeInterval = intervalSeconds;
	}

	private bool IncrementTime(float time)
	{
		timePassed += time;
		if (timePassed >= timeInterval)
		{
			timePassed = 0f;
			return true;
		}
		return false;
	}

	public bool Update()
	{
		return IncrementTime(Time.deltaTime);
	}

	public bool FixedUpdate()
	{
		return IncrementTime(Time.fixedDeltaTime);
	}

	public void SetTimeInterval(float intervalSeconds)
	{
		timeInterval = intervalSeconds;
	}
}
