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
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenHellion.ProviderSystem
{
	// Should be self-contained, and it should therefore not reference any gameplay-related classes.
	/// <summary>
	/// 	A system to make the underlying connections to external game providers, such as Steam and Discord.
	/// 	The point is to make the game be interoperable with any provider of gaming services.
	/// </summary>
	/// <seealso cref="DiscordProvider"/>
	/// <seealso cref="SteamProvider"/>
	/// <seealso cref="IProvider"/>
	internal class ProviderManager : MonoBehaviour
	{
		private static ProviderManager s_instance;
		private static ProviderManager Instance
		{
			get
			{
				if (s_instance != null) return s_instance;

				return new GameObject("ExternalProvider").AddComponent<ProviderManager>();
			}
		}

		private List<IProvider> m_allProviders;

		// The most important of the two providers.
		private IProvider m_mainProvider;

		/// <summary>
		/// 	Get our stream id without a prefix. Returns null if steam is inaccessible.
		/// </summary>
		public static string SteamId
		{
			get
			{
				foreach (IProvider provider in Instance.m_allProviders)
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
				foreach (IProvider provider in Instance.m_allProviders)
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
				if (Instance.m_mainProvider == null) return false;
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
				return Instance.m_mainProvider.GetPrefixedNativeId();
			}
		}

		/// <summary>
		/// 	Get the username of our local player.
		/// </summary>
		public static string Username
		{
			get
			{
				return Instance.m_mainProvider.GetUsername();
			}
		}

		/// <summary>
		/// 	Get a list of all our friends.
		/// </summary>
		public static IProvider.Friend[] Friends
		{
			get
			{
				return Instance.m_mainProvider.GetFriends();
			}
		}

		void Awake()
		{
			// Only one instance can exist at a time.
			if (s_instance != null)
			{
				Destroy(gameObject, 0f);
				Dbg.Error("Tried to create new ProviderManager, but there already exists another manager.");
			}
			s_instance = this;

			DontDestroyOnLoad(gameObject);

			m_allProviders = new()
			{
				new SteamProvider(),
				new DiscordProvider()
			};

			// Initialise our providers.
			// Loop backwards to prevent an InvalidOperationException.
			for (int i = m_allProviders.Count - 1; i >= 0; i--)
			{
				if (!m_allProviders[i].Initialise())
				{
					m_allProviders.RemoveAt(i);
				}
			}

			// Find our main provider, Steam is preferable.
			foreach (IProvider provider in m_allProviders)
			{
				m_mainProvider = provider;
				if (provider is SteamProvider)
				{
					break;
				}
			}

			// Exit game if no providers could be found.
			if (m_mainProvider == null)
			{
				Dbg.Error("No provider could be found.");
				Application.Quit();
				return;
			}

			Dbg.Log("Setting main provider to", m_mainProvider);
		}

		void Start()
		{
			// Enable our providers.
			foreach (IProvider provider in m_allProviders)
			{
				provider.Enable();
			}
		}

		void Update()
		{
			// Update our providers.
			foreach (IProvider provider in m_allProviders)
			{
				provider.Update();
			}
		}

		void OnDestroy()
		{
			// Destroy our providers.
			foreach (IProvider provider in m_allProviders)
			{
				provider.Destroy();
			}
		}

		void OnApplicationQuit()
		{
			// Destroy our providers.
			foreach (IProvider provider in m_allProviders)
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
			foreach (IProvider provider in Instance.m_allProviders)
			{
				provider.UpdateStatus();
			}
		}

		/// <summary>
		/// 	Get if we have achieved a specific achievement.
		/// </summary>
		public static bool GetAchievement(AchievementID id, out bool achieved)
		{
			return Instance.m_mainProvider.GetAchievement(id, out achieved);
		}

		/// <summary>
		/// 	Award the player an achievement.
		/// </summary>
		public static void SetAchievement(AchievementID id)
		{
			Instance.m_mainProvider.SetAchievement(id);
		}

		/// <summary>
		/// 	Get the avatar of a specified user as a texture.
		/// </summary>
		public static Texture2D GetAvatar(string id)
		{
			foreach (IProvider provider in Instance.m_allProviders)
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
			foreach (IProvider provider in Instance.m_allProviders)
			{
				provider.InviteUser(id, secret);
			}
		}
	}
}
