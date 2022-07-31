using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;

public class SteamStats : MonoBehaviour
{
	private static bool userStatsReceived;

	private static Callback<UserStatsReceived_t> userStatsReceivedCallback;

	private static bool currentStatsRequested;

	private static bool storeStats;

	private static ConcurrentQueue<Task> pendingTasks = new ConcurrentQueue<Task>();

	private void OnEnable()
	{
		if (currentStatsRequested)
		{
			SteamUserStats.RequestCurrentStats();
		}
	}

	private void Update()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		if (!currentStatsRequested)
		{
			userStatsReceivedCallback = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
			currentStatsRequested = SteamUserStats.RequestCurrentStats();
		}
		else if (userStatsReceived)
		{
			Task result;
			while (pendingTasks.TryDequeue(out result))
			{
				result.RunSynchronously();
			}
			if (storeStats)
			{
				SteamUserStats.StoreStats();
				storeStats = false;
			}
		}
	}

	private static void OnUserStatsReceived(UserStatsReceived_t callback)
	{
		userStatsReceived = true;
		foreach (object value3 in Enum.GetValues(typeof(SteamStatID)))
		{
			if (!GetStat((SteamStatID)value3, out int _) && !GetStat((SteamStatID)value3, out float _))
			{
			}
		}
	}

	public static bool GetAchievement(SteamAchievementID id, out bool achieved)
	{
		return SteamUserStats.GetAchievement(id.ToString(), out achieved);
	}

	public static void SetAchievement(SteamAchievementID id)
	{
		pendingTasks.Enqueue(new Task(delegate
		{
			SteamUserStats.SetAchievement(id.ToString());
			storeStats = true;
		}));
	}

	public static bool GetStat(SteamStatID id, out int value)
	{
		return SteamUserStats.GetStat(id.ToString(), out value);
	}

	public static bool GetStat(SteamStatID id, out float value)
	{
		return SteamUserStats.GetStat(id.ToString(), out value);
	}

	public static void SetStat(SteamStatID id, float value)
	{
		pendingTasks.Enqueue(new Task(delegate
		{
			SteamUserStats.SetStat(id.ToString(), value);
			storeStats = true;
		}));
	}

	public static void SetStat(SteamStatID id, int value)
	{
		pendingTasks.Enqueue(new Task(delegate
		{
			SteamUserStats.SetStat(id.ToString(), value);
			storeStats = true;
		}));
	}

	public static void ResetStat(SteamStatID id)
	{
		SetStat(id, 0);
	}

	public static void ChangeStatBy<T>(SteamStatID id, T value)
	{
		if (typeof(T) == typeof(int))
		{
			ChangeStatBy(id, (int)(object)value);
		}
		else if (typeof(T) == typeof(float))
		{
			ChangeStatBy(id, (float)(object)value);
		}
	}

	public static void ChangeStatBy(SteamStatID id, int value)
	{
		pendingTasks.Enqueue(new Task(delegate
		{
			if (GetStat(id, out int value2))
			{
				SteamUserStats.SetStat(id.ToString(), value2 + value);
				storeStats = true;
			}
		}));
	}

	public static void ChangeStatBy(SteamStatID id, float value)
	{
		pendingTasks.Enqueue(new Task(delegate
		{
			if (GetStat(id, out float value2))
			{
				SteamUserStats.SetStat(id.ToString(), value2 + value);
				storeStats = true;
			}
		}));
	}
}
