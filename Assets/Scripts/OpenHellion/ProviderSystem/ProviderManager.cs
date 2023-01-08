using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity;

namespace OpenHellion.ProviderSystem
{
	/// <summary>
	/// 	A system to make the underlying connections to external game providers, such as Steam and Discord.
	/// 	The point is to make the game be interoperable with any provider of gaming services.
	/// </summary>
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

		// Keep all backends acive.
		// Mainly used to update Discord rich presence while using steam.
		private List<IProvider> _allProviders;

		// The provider we are actually going to use.
		private IProvider _mainProvider;
		public static IProvider MainProvider
		{
			get
			{
				return Instance._mainProvider;
			}
		}

		public static string SteamId
		{
			get
			{
				foreach (IProvider provider in Instance._allProviders)
				{
					if (provider is SteamProvider)
					{
						return provider.GetNativeId();
					}
				}

				return null;
			}
		}

		public static string DiscordId
		{
			get
			{
				foreach (IProvider provider in Instance._allProviders)
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
				return Instance._mainProvider.IsInitialised();
			}
		}


		void Awake()
		{
			// Only one instance can exist at a time.
			if (s_instance != null)
			{
				GameObject.Destroy(gameObject, 0f);
				Dbg.Error("Tried to create new ProviderManager, but there already exists another manager.");
			}
			s_instance = this;

			DontDestroyOnLoad(gameObject);

			_allProviders = new();

			_allProviders.Add(new SteamProvider());
			_allProviders.Add(new DiscordProvider());

			// Initialise our providers.
			// Loop backwards to prevent InvalidOperationException.
			for (int i = _allProviders.Count - 1; i >= 0; i--)
			{
				if (!_allProviders[i].Initialise())
				{
					_allProviders.RemoveAt(i);
				}
			}

			// Find our main provider, Steam is preferable.
			foreach (IProvider provider in _allProviders)
			{
				_mainProvider = provider;
				if (provider is SteamProvider)
				{
					break;
				}
			}

			// Exit game if no providers could be found.
			if (_mainProvider == null)
			{
				Debug.LogError("No provider could be found.");
				Client.Instance.ExitGame();
				return;
			}

			Dbg.Log("Using provider", _mainProvider);
		}

		void Start()
		{
			// Enable our providers.
			foreach (IProvider provider in _allProviders)
			{
				provider.Enable();
			}
		}

		void Update()
		{
			// Update our providers.
			foreach (IProvider provider in _allProviders)
			{
				provider.Update();
			}
		}

		void OnDestroy()
		{
			// Destroy our providers.
			foreach (IProvider provider in _allProviders)
			{
				provider.Destroy();
			}
		}
	}
}
