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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nakama.TinyJson;
using OpenHellion.Data;
using OpenHellion.IO;
using OpenHellion.Net;
using OpenHellion.Social.NakamaRpc;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZeroGravity;
using ZeroGravity.Network;
using ZeroGravity.UI;

namespace OpenHellion.Social
{
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

		private const string NakamaHost = "127.0.0.1";
		private const int NakamaPort = 7350;

		private static readonly CancellationTokenSource CancelToken = new();

		public static string NakamaIdCached { get; private set; }

		public static async void Initialise()
		{
			try
			{
				Debug.Log("Connecting to Nakama...");

				_client = new Client("http", NakamaHost, NakamaPort, "defaultkey")
				{
					Timeout = 3,
					Logger = new HellionNakamaLogger(),
					GlobalRetryConfiguration = new RetryConfiguration(0, 2)
				};


				Debug.Log("Creating/restoring Nakama session.");

				var authToken = PlayerPrefs.GetString("authToken", null);
				var refreshToken = PlayerPrefs.GetString("refreshToken", null);
				_session = Session.Restore(authToken, refreshToken);


				// Create new or refresh session.
				if (_session is null)
				{
					OnRequireAuthentication.Invoke();
					HasAuthenticated = false;
				}
				else if (_session.HasExpired(DateTime.UtcNow.AddDays(1)))
				{
					try
					{
						_session = await _client.SessionRefreshAsync(_session, canceller: CancelToken.Token);
						NakamaIdCached = await GetUserId();
						HasAuthenticated = true;
						Globals.Instance.OnHellionQuit += CancelToken.Cancel;
					}
					catch (ApiResponseException ex)
					{
						if (ex.StatusCode is 401)
						{
							//OnNakamaError.Invoke(Localization.SessionExpired, null);
							Debug.Log("Nakama session can no longer be refreshed. Must reauthenticate!");
						}

						OnRequireAuthentication.Invoke();
						HasAuthenticated = false;
					}
				}
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Could not connect to Nakama server.");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}
		}

		/// <summary>
		///		Authenticate with Nakama using email and password.
		///		Creates a session and stores it.
		/// </summary>
		/// <param name="email">The user's email.</param>
		/// <param name="password">The user's password.</param>
		/// <returns>If we successfully authenticated.</returns>
		public static async Task<bool> Authenticate(string email, string password)
		{
			try
			{
				_session = await _client.AuthenticateEmailAsync(email, password, create: false,
					canceller: CancelToken.Token);

				PlayerPrefs.SetString("authToken", _session.AuthToken);
				PlayerPrefs.SetString("refreshToken", _session.RefreshToken);

				Debug.Log("Successfully authenticated.");
				HasAuthenticated = true;
				NakamaIdCached = await GetUserId();
				return true;
			}
			catch (ApiResponseException ex)
			{
				Debug.LogError($"Error authenticating user: {ex.StatusCode}: {ex.Message}");

				// Error code for non-existent account.
				if (ex.StatusCode is 404)
				{
					OnNakamaError.Invoke(Localization.AccountNotFound, null);
					return false;
				}

				OnNakamaError.Invoke(Localization.Error, null);
				return false;
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
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
		public static async Task<bool> CreateAccount(string email, string password, string username, string displayName)
		{
			try
			{
				_session = await _client.AuthenticateEmailAsync(email, password, username,
					canceller: CancelToken.Token);

				PlayerPrefs.SetString("authToken", _session.AuthToken);
				PlayerPrefs.SetString("refreshToken", _session.RefreshToken);

				await _client.UpdateAccountAsync(_session, username, displayName, null, CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
					RegionInfo.CurrentRegion.EnglishName,
					TimeZoneInfo.Local.StandardName, canceller: CancelToken.Token);

				Debug.Log("Account successfully created.");
				HasAuthenticated = true;
				return true;
			}
			catch (ApiResponseException ex)
			{
				// Error code for account already existing.
				if (ex.StatusCode is 401)
				{
					OnNakamaError.Invoke(Localization.AccountAlreadyExists, null);
					return false;
				}

				Debug.LogError($"Error creating user: {ex.StatusCode}:{ex.Message}");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, null);
				return false;
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
				return false;
			}
		}

		/// <summary>
		///		Get our Nakama user id.<br/>
		///		A session must be created before we call this method.
		/// </summary>
		/// <returns>Out user id.</returns>
		public static async Task<String> GetUserId()
		{
			try
			{
				var account = await _client.GetAccountAsync(_session, canceller: CancelToken.Token);
				return account.User.Id;
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}

			return null;
		}

		/// <summary>
		///		Get our nakama display name.<br/>
		///		A session must be created before we call this method.
		/// </summary>
		/// <returns>Our display name on Nakama.</returns>
		public static async Task<String> GetDisplayName()
		{
			try
			{
				var account = await _client.GetAccountAsync(_session, canceller: CancelToken.Token);
				return account.User.DisplayName;
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}

			return null;
		}

		/// <summary>
		///		Gets a list of all our friends.<br/>
		///		A session must be created before we call this method.
		/// </summary>
		/// <returns>A list of nakama friends.</returns>
		public static async Task<IApiFriend[]> GetFriends()
		{
			try
			{
				var friends = await _client.ListFriendsAsync(_session, 0, 0, "", canceller: CancelToken.Token);
				return friends.Friends.ToArray();
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				SceneManager.LoadScene(0);
			}

			return null;
		}

		/// <summary>
		///		Creates a socket and makes us appear as online. Makes us be able to communicate with the main server. Also initialises callbacks.<br/>
		///		A session must be created before we call this method.
		/// </summary>
		public static async Task CreateSocket()
		{
			try
			{
				_socket = _client.NewSocket();
				await _socket.ConnectAsync(_session, true, 30, CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}

			_socket.ReceivedChannelMessage += message => { OnChatMessageReceived(message.Username, message.Content); };

			_socket.ReceivedError += exception => { OnNakamaError.Invoke(exception.Message, null); };

			Globals.Instance.OnHellionQuit += () => _socket?.CloseAsync();
		}

		/// <summary>
		///		Search and try to find a fitting match for us to join.
		///		<see cref="CreateSocket"/> must be called before this method.
		/// </summary>
		/// <returns>Returns an array of match ids, which we can use to display a set of match options.</returns>
		public static async Task<string[]> FindMatches(FindMatchesRequest request)
		{
			try
			{
				IApiRpc response = await _socket.RpcAsync("client_find_match",
					JsonSerialiser.Serialize(request, JsonSerialiser.Formatting.None));
				var result = JsonSerialiser.Deserialize<FindMatchesResponse>(response.Payload);

				return result.MatchesId;
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}

			return null;
		}

		/// <summary>
		///		Join a match by using its id.
		///		<see cref="CreateSocket"/> must be called before this method.
		/// </summary>
		/// <param name="matchId">The id of the match to join.</param>
		/// <returns>The connection information of the server to connect to.</returns>
		public static async Task<ServerData> GetMatchConnectionInfo(string matchId)
		{
			try
			{
				var response = await _socket.RpcAsync("client_get_match_info", matchId);
				var result = JsonSerialiser.Deserialize<MatchInfo>(response.Payload);

				return new ServerData()
				{
					Id = matchId,
					Location = result.Location,
					IpAddress = result.Ip,
					GamePort = result.GamePort,
					StatusPort = result.StatusPort,
				};
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}

			return null;
		}

		/// <summary>
		///		Update the <see cref="CharacterData"/> stored by Nakama.
		/// </summary>
		/// <param name="data">The character data to upload.</param>
		public static async void UpdateCharacterData(CharacterData data)
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
				OnNakamaError.Invoke(Localization.Error + ex.StatusCode, null);
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}
		}

		/// <summary>
		///		Get the <see cref="CharacterData"/> stored in the cloud.
		/// </summary>
		/// <returns>A <see cref="CharacterData"/> object. Null if none is found.</returns>
		/// <exception cref="Exception">Fails if this user has several characters or if the format is wrong.</exception>
		public static async Task<CharacterData> GetCharacterData()
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
				OnNakamaError.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}

			return null;
		}

		/// <summary>
		///		Update the <see cref="VesselObjectID"/> stored by Nakama.
		/// </summary>
		/// <param name="data">The character data to upload.</param>
		public static async void UpdateSpawnPointData(VesselObjectID data)
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
				OnNakamaError.Invoke(Localization.Error + ex.StatusCode, null);
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}
		}

		/// <summary>
		///		Join a chat room. May only execute when we are connected to match.
		///		<see cref="CreateSocket"/> must be called before this method.
		/// </summary>
		/// <param name="chatState">The type of chat we are looking for.</param>
		public static async Task<bool> JoinChatRoom(Chat.ChatState chatState)
		{
			string id;
			ChannelType channelType;

			switch (chatState)
			{
				case Chat.ChatState.Global:
					id = "global";
					channelType = ChannelType.Group;
					break;
				// TODO: Implement this.
				case Chat.ChatState.Party:
					throw new NotImplementedException();
				default:
					return false;
			}

			try
			{
				_chatChannel = await _socket.JoinChatAsync(id, channelType);
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
				return false;
			}

			return true;
		}

		/// <summary>
		///		Send a message to the chat room we are currently connected to.
		///		<see cref="JoinChatRoom"/> must be called before this.
		/// </summary>
		/// <param name="chatText">The text we want to send.</param>
		public static async void SendChat(string chatText)
		{
			try
			{
				await _socket.WriteChatMessageAsync(_chatChannel, chatText);
			}
			catch (TaskCanceledException)
			{
				Debug.LogError("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, NakamaConnectionTerminated);
			}
		}

		public static async void LogOut()
		{
			await _client.SessionLogoutAsync(_session);
			_socket?.CloseAsync();
		}

		private static void NakamaConnectionTerminated()
		{
			Debug.Log("Nakama connection failed unexpectedly. Returning to initialising screen...");
			Globals.ExitGame();
		}
	}
}
