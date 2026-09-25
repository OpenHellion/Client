// NakamaClient.cs
//
// Copyright (C) 2024, OpenHellion contributors
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
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using OpenHellion.IO;
using OpenHellion.Social.Message;
using UnityEngine;
using ZeroGravity;
using ZeroGravity.Network;
using ZeroGravity.UI;
using Cysharp.Threading.Tasks;

namespace OpenHellion.Social
{
	/// <summary>
	/// 	Interact with the main server. Used for authentification, cross-server data storage, chatting, and discovering servers.
	/// </summary>
	/// <remarks>
	/// 	Chat still has to be implemented.
	/// </remarks>
	public static class NakamaClient
	{
		public static Action OnRequireAuthentication;
		public static Action<string, Action> OnNakamaError;
		public static Action<string, string> OnChatMessageReceived;

		public static bool HasAuthenticated { get; private set; }

		private static Client _client;

		private static ISession _session;

		private static ISocket _socket;

		private static IChannel _chatChannel;

		private static readonly string NakamaHost = Properties.GetProperty("main_server_ip", "127.0.0.1");
		private static readonly int NakamaPort = Properties.GetProperty("main_server_port", 7350);
		private static readonly string NakamaKey = Properties.GetProperty("main_server_key", "defaultkey");

		private static readonly CancellationTokenSource _cancelToken = new();

		/// <returns>False when the main server could not be reached.</returns>
		public static async UniTask<bool> Initialise()
		{
			try
			{
				Debug.Log("Connecting to Nakama...");

				_client = new Client("http", NakamaHost, NakamaPort, NakamaKey)
				{
					Timeout = 3,
					Logger = new HellionNakamaLogger(),
					GlobalRetryConfiguration = new RetryConfiguration(0, 2)
				};

				Debug.Log("Creating/restoring Nakama session.");

				var authToken = PlayerPrefs.GetString("authToken", null);
				var refreshToken = PlayerPrefs.GetString("refreshToken", null);
				_session = Session.Restore(authToken, refreshToken);

				if (_session is null)
				{
					OnRequireAuthentication?.Invoke();
					HasAuthenticated = false;
				}
				else
				{
					try
					{
						if (_session.HasExpired(DateTime.UtcNow.AddDays(1)))
						{
							_session = await _client.SessionRefreshAsync(_session, canceller: _cancelToken.Token);
							Globals.Instance.OnHellionQuit += _cancelToken.Cancel;
						}
						else
						{
							await _client.GetAccountAsync(_session, canceller: _cancelToken.Token);
						}

						HasAuthenticated = true;
					}
					catch (ApiResponseException ex)
					{
						Debug.Log($"Stored Nakama session was refused ({ex.StatusCode}). Must reauthenticate!");
						OnRequireAuthentication?.Invoke();
						HasAuthenticated = false;
					}
				}
			}
			catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException or SocketException)
			{
				Debug.LogError("Could not connect to Nakama server.");
				return false;
			}

			return true;
		}

		/// <summary>
		///		Authenticate with Nakama using email and password.
		///		Creates a session and stores it.
		/// </summary>
		/// <param name="email">The user's email.</param>
		/// <param name="password">The user's password.</param>
		/// <returns>If we successfully authenticated.</returns>
		public static async UniTask<bool> Authenticate(string email, string password)
		{
			try
			{
				_session = await _client.AuthenticateEmailAsync(email, password, create: false,
					canceller: _cancelToken.Token);

				PlayerPrefs.SetString("authToken", _session.AuthToken);
				PlayerPrefs.SetString("refreshToken", _session.RefreshToken);

				Debug.Log("Successfully authenticated.");
				HasAuthenticated = true;
				return true;
			}
			catch (ApiResponseException ex)
			{
				Debug.Log($"Error authenticating user: {ex.StatusCode}: {ex.Message}");

				// Error code for non-existent account.
				if (ex.StatusCode is 404)
				{
					OnNakamaError?.Invoke(Localization.AccountNotFound, null);
					return false;
				}

				OnNakamaError?.Invoke(Localization.Error, null);
				return false;
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError?.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
				return false;
			}
		}

		/// <summary>
		///		Create an account on the Nakama platform using email.
		///		Creates a session and stores it.
		/// </summary>
		/// <param name="email">The user's email.</param>
		/// <param name="password">The user's password.</param>
		/// <param name="username">The user's username.</param>
		/// <param name="displayName">The user's display name.</param>
		/// <returns>If we successfully created an account.</returns>
		public static async UniTask<bool> CreateAccount(string email, string password, string username, string displayName)
		{
			try
			{
				_session = await _client.AuthenticateEmailAsync(email, password, username,
					canceller: _cancelToken.Token);

				PlayerPrefs.SetString("authToken", _session.AuthToken);
				PlayerPrefs.SetString("refreshToken", _session.RefreshToken);

				await _client.UpdateAccountAsync(_session, username, displayName, null, CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
					RegionInfo.CurrentRegion.EnglishName,
					TimeZoneInfo.Local.StandardName, canceller: _cancelToken.Token);

				Debug.Log("Account successfully created.");
				HasAuthenticated = true;
				return true;
			}
			catch (ApiResponseException ex)
			{
				// Error code for account already existing.
				if (ex.StatusCode is 401)
				{
					OnNakamaError?.Invoke(Localization.AccountAlreadyExists, null);
					return false;
				}

				if (ex.StatusCode is 400)
				{
					OnNakamaError?.Invoke(Localization.InvalidPassword, null);
					return false;
				}

				Debug.LogError($"Error creating user: {ex.StatusCode}:{ex.Message}");
				OnNakamaError?.Invoke(Localization.NoNakamaConnection, null);
				return false;
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError?.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
				return false;
			}
		}

		/// <summary>
		///		Get our Nakama user id.<br/>
		///		A session must be created before we call this method.
		/// </summary>
		/// <returns>Out user id.</returns>
		public static async UniTask<string> GetUserId()
		{
			try
			{
				var account = await _client.GetAccountAsync(_session, canceller: _cancelToken.Token);
				return account.User.Id;
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError?.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}

			return null;
		}

		/// <summary>
		///		Get our nakama display name.<br/>
		///		A session must be created before we call this method.
		/// </summary>
		/// <returns>Our display name on Nakama.</returns>
		public static async UniTask<string> GetDisplayName()
		{
			try
			{
				var account = await _client.GetAccountAsync(_session, canceller: _cancelToken.Token);
				return account.User.DisplayName;
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError?.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}

			return null;
		}

		/// <summary>
		///		Gets a list of all our friends.<br/>
		///		A session must be created before we call this method.
		/// </summary>
		/// <returns>A list of nakama friends.</returns>
		public static async UniTask<IApiFriend[]> GetFriends()
		{
			try
			{
				var friends = await _client.ListFriendsAsync(_session, 0, 0, "", canceller: _cancelToken.Token);
				return friends.Friends.ToArray();
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError?.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}

			return null;
		}

		/// <summary>
		///		Creates a socket and makes us appear as online. Makes us be able to communicate with the main server. Also initialises callbacks.<br/>
		///		A session must be created before we call this method.
		/// </summary>
		public static async UniTask CreateSocket()
		{
			try
			{
				_socket = _client.NewSocket();
				await _socket.ConnectAsync(_session, true, 30, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError?.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}

			_socket.ReceivedChannelMessage += message => { OnChatMessageReceived(message.Username, message.Content); };

			_socket.ReceivedError += exception => { OnNakamaError?.Invoke(exception.Message, null); };

			Globals.Instance.OnHellionQuit += () => _socket?.CloseAsync();
		}

		/// <summary>
		///		Get every server registered with the main server, with the details needed to connect.
		///		<see cref="CreateSocket"/> must be called before this method.
		/// </summary>
		public static async UniTask<ServerConnectionInfo[]> GetMatches()
		{
			try
			{
				IApiRpc response = await _socket.RpcAsync("client_get_matches");
				MatchInfo[] matches = JsonSerialiser.Deserialize<MatchInfo[]>(response.Payload);

				return matches.Select(static (MatchInfo m) => new ServerConnectionInfo
				{
					IpAddress = m.Ip,
					GamePort = m.GamePort,
					StatusPort = m.StatusPort
				}).ToArray();
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError?.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}

			return Array.Empty<ServerConnectionInfo>();
		}


		/// <summary>
		///		Update the <see cref="CharacterData"/> stored by Nakama.
		/// </summary>
		/// <param name="data">The character data to upload.</param>
		public static async UniTask UpdateCharacterData(CharacterData data)
		{
			try
			{
				await _client.WriteStorageObjectsAsync(_session, new IApiWriteStorageObject[]
				{
					new WriteStorageObject
					{
						Collection = "player_data",
						Key = "character_data",
						Value = JsonSerialiser.Serialize(data),
						PermissionRead = 1,
						PermissionWrite = 1
					}
				});
			}
			catch (ApiResponseException ex)
			{
				Debug.LogException(ex);
				OnNakamaError?.Invoke(Localization.Error + ex.StatusCode, null);
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError?.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}
		}

		/// <summary>
		///		Get the <see cref="CharacterData"/> stored in the cloud.
		/// </summary>
		/// <returns>A <see cref="CharacterData"/> object. Null if none is found.</returns>
		/// <exception cref="Exception">Fails if this user has several characters or if the format is wrong.</exception>
		public static async UniTask<CharacterData> GetCharacterData()
		{
			try
			{
				var response = await _client.ReadStorageObjectsAsync(_session, new IApiReadStorageObjectId[]{new StorageObjectId
				{
					Collection = "player_data",
					Key = "character_data",
					UserId = await GetUserId()
				}});

				if (!response.Objects.Any())
				{
					return null;
				}

				if (response.Objects.ToArray().Length > 1)
				{
					throw new Exception("Received response with invalid length.");
				}

				var result = JsonSerialiser.Deserialize<CharacterData>(response.Objects.First().Value);

				if (result == null)
				{
					throw new Exception("Failed to deserialise storage data into CharacterData");
				}

				return result;
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError?.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}

			return null;
		}

		public static async UniTask LogOut()
		{
			await _client.SessionLogoutAsync(_session);
			_socket?.CloseAsync();
		}

		private static void NakamaConnectionTerminated()
		{
			Debug.Log("Nakama connection failed unexpectedly. Returning to initialising screen...");
		}
	}
}
