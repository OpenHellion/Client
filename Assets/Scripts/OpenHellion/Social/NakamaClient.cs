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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using ZeroGravity;

namespace OpenHellion.Social
{
	public class NakamaClient : MonoBehaviour
	{
		[Tooltip("Called when Nakama requires authentication.")]
		public UnityEvent _OnRequireAuthentication;
		public UnityEvent<string, Action> _OnError;

		public bool HasAuthenticated { get; private set; }

		private Nakama.Client _client;

		private ISession _session;

		private const string NakamaHost = "127.0.0.1";
		private const int NakamaPort = 7350;

		private readonly CancellationTokenSource _cancelToken = new();

		private void Awake()
		{
			HasAuthenticated = false;
			DontDestroyOnLoad(this);
		}

		private async void Start()
		{
			try
			{
				Dbg.Log("Connecting to Nakama...");

				_client = new Nakama.Client("http", NakamaHost, NakamaPort, "defaultkey")
				{
					Timeout = 3,
					Logger = new HellionNakamaLogger(),
					GlobalRetryConfiguration = new RetryConfiguration(0, 2)
				};


				Dbg.Log("Creating/restoring Nakama session.");

				var authToken = PlayerPrefs.GetString("authToken", null);
				var refreshToken = PlayerPrefs.GetString("refreshToken", null);
				_session = Session.Restore(authToken, refreshToken);


				// Create new or refresh session.
				if (_session is null)
				{
					_OnRequireAuthentication.Invoke();
					HasAuthenticated = false;
				}
				else if (_session.HasExpired(DateTime.UtcNow.AddDays(1)))
				{
					try
					{
						_session = await _client.SessionRefreshAsync(_session, canceller: _cancelToken.Token);
						HasAuthenticated = true;
					}
					catch (ApiResponseException)
					{
						Dbg.Log("Nakama session can no longer be refreshed. Must reauthenticate!");

						_OnRequireAuthentication.Invoke();
						HasAuthenticated = false;
					}
				}
			}
			catch (TaskCanceledException)
			{
				Dbg.Error("Could not connect to Nakama server.");
				_OnError.Invoke(Localization.NoNakamaConnection, Application.Quit);
			}
		}

		// Authenticates and saves the result.
		public async Task<bool> Authenticate(string email, string password)
		{
			try
			{
				_session = await _client.AuthenticateEmailAsync(email, password, create: false, canceller: _cancelToken.Token);

				PlayerPrefs.SetString("authToken", _session.AuthToken);
				PlayerPrefs.SetString("refreshToken", _session.RefreshToken);

				Dbg.Log("Sucessfully authenticated.");
				HasAuthenticated = true;
				return true;
			}
			catch (ApiResponseException ex)
			{
				Dbg.Error($"Error authenticating user: {ex.StatusCode}:{ex.Message}");

				// Error code for non-existant account.
				if (ex.StatusCode is 404)
				{
					_OnError.Invoke(Localization.AccountNotFound, null);
					return false;
				}

				_OnError.Invoke(Localization.Error, null);
				return false;
			}
			catch (TaskCanceledException)
			{
				Dbg.Error("Nakama disconnected when doing task");
				_OnError.Invoke(Localization.NoNakamaConnection, Application.Quit);
				return false;
			}
		}

		public async Task<bool> CreateAccount(string email, string password, string username, string displayName)
		{
			try
			{
				_session = await _client.AuthenticateEmailAsync(email, password, username, canceller: _cancelToken.Token);

				PlayerPrefs.SetString("authToken", _session.AuthToken);
				PlayerPrefs.SetString("refreshToken", _session.RefreshToken);

				await _client.UpdateAccountAsync(_session, username, displayName, null, CultureInfo.CurrentCulture.Name, RegionInfo.CurrentRegion.EnglishName, TimeZoneInfo.Local.StandardName,
					canceller: _cancelToken.Token);

				Dbg.Log("Account successfully created.");
				HasAuthenticated = true;
				return true;
			}
			catch (ApiResponseException ex)
			{
				// Error code for account already existing.
				if (ex.StatusCode is 401)
				{
					_OnError.Invoke(Localization.AccountAlreadyExists, null);
					return false;
				}

				Dbg.Error($"Error creating user: {ex.StatusCode}:{ex.Message}");
				_OnError.Invoke(Localization.NoNakamaConnection, null);
				return false;
			}
			catch (TaskCanceledException)
			{
				Dbg.Error("Nakama disconnected when doing task");
				_OnError.Invoke(Localization.NoNakamaConnection, Application.Quit);
				return false;
			}
		}

		public async Task<String> GetUserId()
		{
			try
			{
				var account = await _client.GetAccountAsync(_session);
				return account.User.Id;
			}
			catch (TaskCanceledException)
			{
				Dbg.Error("Nakama disconnected when doing task");
				_OnError.Invoke(Localization.NoNakamaConnection, Application.Quit);
			}
			return null;
		}

		public async Task<String> GetUsername()
		{
			try
			{
				var account = await _client.GetAccountAsync(_session);
				return account.User.Username;
			}
			catch (TaskCanceledException)
			{
				Dbg.Error("Nakama disconnected when doing task");
				_OnError.Invoke(Localization.NoNakamaConnection, Application.Quit);
			}
			return null;
		}

		public async Task<IApiFriend[]> GetFriends()
		{
			try
			{
				var friends = await _client.ListFriendsAsync(_session, 0, 0, "");
				return friends.Friends.ToArray();
			}
			catch (TaskCanceledException)
			{
				Dbg.Error("Nakama disconnected when doing task");
				_OnError.Invoke(Localization.NoNakamaConnection, Application.Quit);
			}
			return null;
		}

		private void OnApplicationQuit()
		{
			_cancelToken.Cancel();
		}
	}
}
