// IProvider.cs
//
// Copyright (C) 2023, OpenHellion contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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
			public string NativeId;
			public string Name;
			public FriendStatus Status;
		}

		enum FriendStatus
		{
			ONLINE,
			OFFLINE
		}

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
		string GetNativeId();

		/// <summary>
		/// 	Get a list of all our friends.
		/// </summary>
		// TODO: Not provider independent.
		Friend[] GetFriends();
		Texture2D GetAvatar(string id);
		void InviteUser(string id, string secret);
	}
}
