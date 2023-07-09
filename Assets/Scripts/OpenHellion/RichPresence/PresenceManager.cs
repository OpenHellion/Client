// ProviderManager.cs
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
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using UnityEngine;

namespace OpenHellion.RichPresence
{
	/// <summary>
	/// 	Manages rich presence on Steam and Discord.
	///		TODO: Support Steam.
	/// </summary>
	/// <seealso cref="DiscordProvider"/>
	/// <seealso cref="SteamProvider"/>
	/// <seealso cref="IPresenceProvider"/>
	internal class PresenceManager : MonoBehaviour
	{
		private static PresenceManager _instance;
		private static PresenceManager Instance
		{
			get
			{
				if (_instance is not null) return _instance;

				return new GameObject("PresenceManager").AddComponent<PresenceManager>();
			}
		}

		public static bool HasSteam { get; private set; }

		private List<IPresenceProvider> _providers;

		void Awake()
		{
			// Only one instance can exist at a time.
			if (_instance is not null)
			{
				Destroy(gameObject, 0f);
				Dbg.Error("Tried to create new ProviderManager, but there already exists another manager.");
			}
			_instance = this;

			DontDestroyOnLoad(gameObject);

			_providers = new()
			{
				new SteamProvider(),
				new DiscordProvider()
			};

			// Initialise our providers.
			// Loop backwards to prevent an InvalidOperationException.
			for (int i = _providers.Count - 1; i >= 0; i--)
			{
				// Remove the provider if it doesn't start properly.
				if (!_providers[i].Initialise())
				{
					_providers.RemoveAt(i);
				}
			}

			// Find our main provider, Steam is preferable.
			foreach (IPresenceProvider provider in _providers)
			{
				if (provider is SteamProvider)
				{
					HasSteam = true;
					break;
				}
			}
		}

		void Start()
		{
			// Enable our providers.
			foreach (IPresenceProvider provider in _providers)
			{
				provider.Enable();
			}
		}

		void Update()
		{
			// Update our providers.
			foreach (IPresenceProvider provider in _providers)
			{
				provider.Update();
			}
		}

		void OnDestroy()
		{
			// Destroy our providers.
			foreach (IPresenceProvider provider in _providers)
			{
				provider.Destroy();
			}
		}

		/// <summary>
		/// 	Used to update rich presence.
		/// </summary>
		public static void UpdateStatus()
		{
			// Update rich presence for all providers.
			foreach (IPresenceProvider provider in Instance._providers)
			{
				provider.UpdateStatus();
			}
		}

		/// <summary>
		/// 	Get if we have achieved a specific achievement.
		/// </summary>
		public static bool GetAchievement(AchievementID id, out bool achieved)
		{
			achieved = false;
			return false;
		}

		/// <summary>
		/// 	Award the player an achievement.
		/// </summary>
		public static void SetAchievement(AchievementID id)
		{
		}

		/// <summary>
		/// 	Get the avatar of a specified user as a texture.
		/// </summary>
		public static Texture2D GetAvatar(string id)
		{
			return Resources.Load<Texture2D>("UI/default_avatar");
		}
	}
}
