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
	// Should be self-contained, and it should therefore not reference any gameplay-related classes.
	/// <summary>
	/// 	A system to make the underlying connections to external game providers, such as Steam and Discord.
	/// 	The point is to make the game be interoperable with any provider of gaming services.
	/// </summary>
	/// <seealso cref="DiscordProvider"/>
	/// <seealso cref="SteamProvider"/>
	/// <seealso cref="IPresenceProvider"/>
	internal class PresenceManager : MonoBehaviour
	{
		private static PresenceManager s_Instance;
		private static PresenceManager Instance
		{
			get
			{
				if (s_Instance != null) return s_Instance;

				return new GameObject("ExternalProvider").AddComponent<PresenceManager>();
			}
		}

		private List<IPresenceProvider> m_AllProviders;

		// The most important of the two providers.
		private IPresenceProvider m_MainProvider = null;

		/// <summary>
		/// 	Get our stream id without a prefix. Returns null if steam is inaccessible.
		/// </summary>
		public static string SteamId
		{
			get
			{
				foreach (IPresenceProvider provider in Instance.m_AllProviders)
				{
					if (provider is SteamProvider)
					{
						return provider.GetNativeId();
					}
				}

				return null;
			}
		}

		/// <summary>
		/// 	Get our discord id without a prefix. Returns null if discord is inaccessible.
		/// </summary>
		public static string DiscordId
		{
			get
			{
				foreach (IPresenceProvider provider in Instance.m_AllProviders)
				{
					if (provider is DiscordProvider)
					{
						return provider.GetNativeId();
					}
				}

				return null;
			}
		}

		/// <summary>
		/// 	If any of the underlying providers is initialized.
		/// </summary>
		public static bool AnyInitialised
		{
			get
			{
				if (Instance.m_MainProvider == null) return false;
				return true;
			}
		}

		/// <summary>
		/// 	Get the id of our local player with a prefix. The prefix tells us what provider it is from.
		/// </summary>
		public static string NativeId
		{
			get
			{
				return Instance.m_MainProvider.GetPrefixedNativeId();
			}
		}

		/// <summary>
		/// 	Get the username of our local player.
		/// </summary>
		public static string Username
		{
			get
			{
				return Instance.m_MainProvider.GetUsername();
			}
		}

		/// <summary>
		/// 	Get a list of all our friends.
		/// </summary>
		public static IPresenceProvider.Friend[] Friends
		{
			get
			{
				return Instance.m_MainProvider.GetFriends();
			}
		}

		public static bool HasStarted { get; private set; }

		void Awake()
		{
			HasStarted = false;

			// Only one instance can exist at a time.
			if (s_Instance != null)
			{
				Destroy(gameObject, 0f);
				Dbg.Error("Tried to create new ProviderManager, but there already exists another manager.");
			}
			s_Instance = this;

			DontDestroyOnLoad(gameObject);

			m_AllProviders = new()
			{
				new SteamProvider(),
				new DiscordProvider()
			};

			// Initialise our providers.
			// Loop backwards to prevent an InvalidOperationException.
			for (int i = m_AllProviders.Count - 1; i >= 0; i--)
			{
				// Remove the provider if it doesn't start properly.
				if (!m_AllProviders[i].Initialise())
				{
					// Only the discord provider actually sets the HasStarted to true, so if the provider isn't
					// enabled, we have to enable HasStarted to make sure the program doesn't get stuck loading.
					if (m_AllProviders[i] is DiscordProvider)
					{
						HasStarted = true;
					}

					m_AllProviders.RemoveAt(i);

					continue;
				}

				// Enable the custom start callback for the DiscordProvider.
				if (m_AllProviders[i] is DiscordProvider)
				{
					((DiscordProvider)m_AllProviders[i]).OnUserUpdate = () =>
					{
						HasStarted = true;
					};
				}
			}

			// Find our main provider, Steam is preferable.
			foreach (IPresenceProvider provider in m_AllProviders)
			{
				m_MainProvider = provider;
				if (provider is SteamProvider)
				{
					break;
				}
			}

			// Exit game if no providers could be found.
			if (m_MainProvider == null)
			{
				Dbg.Error("No provider could be found.");
				Application.Quit();
				return;
			}

			Dbg.Log("Setting main provider to", m_MainProvider);
		}

		void Start()
		{
			// Enable our providers.
			foreach (IPresenceProvider provider in m_AllProviders)
			{
				provider.Enable();
			}
		}

		void Update()
		{
			// Update our providers.
			foreach (IPresenceProvider provider in m_AllProviders)
			{
				provider.Update();
			}
		}

		void OnDestroy()
		{
			// Destroy our providers.
			foreach (IPresenceProvider provider in m_AllProviders)
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
			foreach (IPresenceProvider provider in Instance.m_AllProviders)
			{
				provider.UpdateStatus();
			}
		}

		/// <summary>
		/// 	Get if we have achieved a specific achievement.
		/// </summary>
		public static bool GetAchievement(AchievementID id, out bool achieved)
		{
			return Instance.m_MainProvider.GetAchievement(id, out achieved);
		}

		/// <summary>
		/// 	Award the player an achievement.
		/// </summary>
		public static void SetAchievement(AchievementID id)
		{
			Instance.m_MainProvider.SetAchievement(id);
		}

		/// <summary>
		/// 	Get the avatar of a specified user as a texture.
		/// </summary>
		public static Texture2D GetAvatar(string id)
		{
			foreach (IPresenceProvider provider in Instance.m_AllProviders)
			{
				return provider.GetAvatar(id);
			}

			return Resources.Load<Texture2D>("UI/default_avatar");
		}

		/// <summary>
		/// 	Send an invite to a user with a specified id.
		/// </summary>
		public static void InviteUser(string id, string secret)
		{
			foreach (IPresenceProvider provider in Instance.m_AllProviders)
			{
				provider.InviteUser(id, secret);
			}
		}
	}
}
