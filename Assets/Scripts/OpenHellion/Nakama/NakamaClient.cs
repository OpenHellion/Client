// NakamaClient.cs
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
	public class NakamaClient : MonoBehaviour
	{
		public static UnityEvent OnRequireAuthentification;

		private Nakama.Client _Client;

		private ISession _Session;

		async void Start()
		{
			_Client = new Client("http", "127.0.0.1", 7350, "defaultkey");
			_Client.Timeout = 20;

			string authToken = PlayerPrefs.GetString("authToken", null);
			string refreshToken = PlayerPrefs.GetString("refreshToken", null);
			_Session = Session.Restore(authToken, refreshToken);

			// Create new or refresh session.
			if (_Session is null)
			{
				OnRequireAuthentification.Invoke();
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

					OnRequireAuthentification.Invoke();
				}
			}
		}

		// Authenticates and saves the result.
		public async void Authenticate(string email, string password)
		{
			_Session = await _Client.AuthenticateEmailAsync(email, password);
			Dbg.Log(_Session);

			PlayerPrefs.SetString("authToken", _Session.AuthToken);
			PlayerPrefs.SetString("refreshToken", _Session.RefreshToken);
		}

		public async String GetUserId()
		{
			var account = await _Client.GetAccountAsync(session);
			return account.User.UserId;
		}

		public async String GetUsername()
		{
			var account = await _Client.GetAccountAsync(session);
			return account.User.Username;
		}
	}
}
