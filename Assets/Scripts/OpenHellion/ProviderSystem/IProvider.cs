using UnityEngine;

namespace OpenHellion.ProviderSystem
{
	/// <summary>
	/// 	Wrapper around APIs like Steamworks and Discord Game API.
	/// 	Everything here should be provider independent (or as independent as possible).
	/// </summary>
	public interface IProvider
	{
		internal bool Initialise();
		internal void Enable();
		internal void Destroy();
		internal void Update();

		// API
		struct Friend
		{
			public string Id;
			public string NativeId;
			public string Name;
			public FriendStatus Status;
		}

		enum FriendStatus
		{
			ONLINE,
			OFFLINE
		}

		bool IsInitialised();

		/// <summary>
		/// 	Used by the Discord provider to update rich presence.
		/// </summary>
		void UpdateStatus();

		bool GetAchievement(AchievementID id, out bool achieved);
		void SetAchievement(AchievementID id);
		bool GetStat(ProviderStatID id, out int value);
		void SetStat(ProviderStatID id, int value);
		void ResetStat(ProviderStatID id);
		void ChangeStatBy<T>(ProviderStatID id, T value);

		/// <summary>
		/// 	Get the username of our local player.
		/// </summary>
		string GetUsername();

		/// <summary>
		/// 	Get the id of our local player.
		/// </summary>
		string GetId();

		/// <summary>
		/// 	Get a list of all our friends.
		/// </summary>
		// TODO: Not provider independent.
		Friend[] GetFriends();
		Texture2D GetAvatar(string id);
		void InviteUser(string id, string secret);
	}
}
