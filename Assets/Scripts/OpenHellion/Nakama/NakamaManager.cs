// NakamaManager.cs
//
// Copyright (C) 2023, OpenHellion contributors
//
// SPDX-License-Identifier: GPL-3.0-or-later
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

using Nakama;
using System;
using UnityEngine;

namespace OpenHellion.Nakama
{
	public class NakamaManager : MonoBehaviour
	{
		public static Action<string, string> OnError;
		public static Action OnRequireAuthentification;
		public static Action<Tuple<string, string>> OnAuthentificationComplete;

		private ISession _Session;

		public static NakamaManager Instance;
		public static string UserId => Instance._Session.UserId;
		public static string Username => Instance._Session.Username;


		private void Awake()
		{
			if (Instance is not null)
			{
				Dbg.Log("Destroying NakamaManager on " + Instance.name);
				Destroy(Instance);
			}
			Instance = this;

			DontDestroyOnLoad(gameObject);
		}

		async void Start()
		{
			var client = new Client("http", "127.0.0.1", 7350, "defaultkey");
			client.Timeout = 20;

			string authToken = PlayerPrefs.GetString("authToken");
			string refreshToken = PlayerPrefs.GetString("refreshToken");
			_Session = Session.Restore(authToken, refreshToken);

			// Create new or refresh session.
			if (_Session is null)
			{
				OnRequireAuthentification();
			}
			else if (_Session.HasExpired(DateTime.UtcNow.AddDays(1)))
			{
				try
				{
					_Session = await client.SessionRefreshAsync(_Session);
				}
				catch (ApiResponseException)
				{
					Dbg.Log("Nakama session can no longer be refreshed. Must reauthenticate!");

					OnRequireAuthentification();
				}
			}
		}

		// Authenticates and saves the result.
		async void Authenticate(Client client, string email, string password)
		{
			_Session = await client.AuthenticateEmailAsync(email, password);
			Dbg.Log(_Session);

			PlayerPrefs.SetString("authToken", _Session.AuthToken);
			PlayerPrefs.SetString("refreshToken", _Session.RefreshToken);
		}
	}
}
