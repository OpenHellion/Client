namespace OpenHellion.ProviderSystem
{
	// TODO: Make internal
	/// <summary>
	/// 	Abstraction of functions used by both Steam and Discord.
	/// </summary>
	public interface IProvider
	{
		/// <summary>
		/// 	Initialize to see if provider can be used.
		/// </summary>
		bool Initialise();

		/// <summary>
		/// 	Enable provider, and use.
		/// </summary>
		void Enable();
		void Destroy();
		void Update();

		// API
		bool IsInitialised();
		void UpdateStatus();

		bool GetAchievement(AchievementID id, out bool achieved);
		void SetAchievement(AchievementID id);
		bool GetStat(ProviderStatID id, out int value);
		void SetStat(ProviderStatID id, int value);
		void ResetStat(ProviderStatID id);
		void ChangeStatBy<T>(ProviderStatID id, T value);
		string GetUsername();
	}
}
