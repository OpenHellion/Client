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
using OpenHellion.IO;
using OpenHellion.Net;
using OpenHellion.Social.NakamaRpc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using ZeroGravity;
using ZeroGravity.UI;

namespace OpenHellion.Social
{
	public class NakamaClient : MonoBehaviour
	{
		[Tooltip("Called when Nakama requires authentication.")]
		public UnityEvent OnRequireAuthentication;
		public UnityEvent<string, Action> OnNakamaError;
		public Action<String, String> OnChatMessageReceived;

		public bool HasAuthenticated { get; private set; }

		private Nakama.Client _client;

		private ISession _session;

		private ISocket _socket;

		private IMatch _match;

		private IChannel _chatChannel;

		private const string NakamaHost = "127.0.0.1";
		private const int NakamaPort = 7350;

		private readonly CancellationTokenSource _cancelToken = new();
		private TaskCompletionSource<MatchState> _matchStateReceivedCompletionSource;

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
					OnRequireAuthentication.Invoke();
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

						OnRequireAuthentication.Invoke();
						HasAuthenticated = false;
					}
				}
			}
			catch (TaskCanceledException)
			{
				Dbg.Error("Could not connect to Nakama server.");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, Application.Quit);
			}
		}

		/// <summary>
		///		Authenticate with Nakama using email and password.
		///		Creates a session and stores it.
		/// </summary>
		/// <param name="email">The user's email.</param>
		/// <param name="password">The user's password.</param>
		/// <returns>If we successfully authenticated.</returns>
		public async Task<bool> Authenticate(string email, string password)
		{
			try
			{
				_session = await _client.AuthenticateEmailAsync(email, password, create: false, canceller: _cancelToken.Token);

				PlayerPrefs.SetString("authToken", _session.AuthToken);
				PlayerPrefs.SetString("refreshToken", _session.RefreshToken);

				Dbg.Log("Successfully authenticated.");
				HasAuthenticated = true;
				return true;
			}
			catch (ApiResponseException ex)
			{
				Dbg.Error($"Error authenticating user: {ex.StatusCode}:{ex.Message}");

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
				Dbg.Error("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, Application.Quit);
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
		public async Task<bool> CreateAccount(string email, string password, string username, string displayName)
		{
			try
			{
				_session = await _client.AuthenticateEmailAsync(email, password, username, canceller: _cancelToken.Token);

				PlayerPrefs.SetString("authToken", _session.AuthToken);
				PlayerPrefs.SetString("refreshToken", _session.RefreshToken);

				await _client.UpdateAccountAsync(_session, username, displayName, null, CultureInfo.CurrentCulture.Name, RegionInfo.CurrentRegion.EnglishName,
					TimeZoneInfo.Local.StandardName, canceller: _cancelToken.Token);

				Dbg.Log("Account successfully created.");
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

				Dbg.Error($"Error creating user: {ex.StatusCode}:{ex.Message}");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, null);
				return false;
			}
			catch (TaskCanceledException)
			{
				Dbg.Error("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, Application.Quit);
				return false;
			}
		}

		/// <summary>
		///		Get our Nakama user id.<br/>
		///		A session must be created before we call this method.
		/// </summary>
		/// <returns>Out user id.</returns>
		public async Task<String> GetUserId()
		{
			try
			{
				var account = await _client.GetAccountAsync(_session, canceller: _cancelToken.Token);
				return account.User.Id;
			}
			catch (TaskCanceledException)
			{
				Dbg.Error("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, Application.Quit);
			}
			return null;
		}

		/// <summary>
		///		Get our nakama display name.<br/>
		///		A session must be created before we call this method.
		/// </summary>
		/// <returns>Our display name on Nakama.</returns>
		public async Task<String> GetDisplayName()
		{
			try
			{
				var account = await _client.GetAccountAsync(_session, canceller: _cancelToken.Token);
				return account.User.DisplayName;
			}
			catch (TaskCanceledException)
			{
				Dbg.Error("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, Application.Quit);
			}
			return null;
		}

		/// <summary>
		///		Gets a list of all our friends.<br/>
		///		A session must be created before we call this method.
		/// </summary>
		/// <returns>A list of nakama friends.</returns>
		public async Task<IApiFriend[]> GetFriends()
		{
			try
			{
				var friends = await _client.ListFriendsAsync(_session, 0, 0, "", canceller: _cancelToken.Token);
				return friends.Friends.ToArray();
			}
			catch (TaskCanceledException)
			{
				Dbg.Error("Nakama disconnected when doing task");
				SceneManager.LoadScene(0);
			}
			return null;
		}

		/// <summary>
		///		Creates a socket and makes us appear as online. Makes us be able to communicate with the main server. Also initialises callbacks.<br/>
		///		A session must be created before we call this method.
		/// </summary>
		public async Task CreateSocket()
		{
			try
			{
				_socket = _client.NewSocket();
				await _socket.ConnectAsync(_session, true, 30, CultureInfo.CurrentCulture.Name);
			}
			catch (TaskCanceledException)
			{
				Dbg.Error("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, Application.Quit);
			}

			_socket.ReceivedChannelMessage += message =>
			{
				OnChatMessageReceived(message.Username, message.Content);
			};

			_socket.ReceivedError += exception =>
			{
				OnNakamaError.Invoke(exception.Message, null);
			};

			_socket.ReceivedMatchState += matchState =>
			{
				_matchStateReceivedCompletionSource = new();
				var result = JsonSerialiser.Deserialize<MatchState>(Encoding.UTF8.GetString(matchState.State));
				if (result is not null)
				{
					_matchStateReceivedCompletionSource.SetResult(result);
				}
				else
				{
					_matchStateReceivedCompletionSource.SetException(new Exception("Failed to deserialise match state."));
				}
			};
		}

		/// <summary>
		///		Search and try to find a fitting match for us to join.
		///		<see cref="CreateSocket"/> must be called before this method.
		/// </summary>
		/// <returns>Returns an array of match ids, which we can use to display a set of match options.</returns>
		public async Task<string[]> FindMatches(FindMatchesRequest request)
		{
			try
			{
				IApiRpc response = await _socket.RpcAsync("client_find_match", JsonSerialiser.Serialize(request, JsonSerialiser.Formatting.None));
				var result = JsonSerialiser.Deserialize<FindMatchesResponse>(response.Payload);

				return result.MatchesId;
			}
			catch (TaskCanceledException)
			{
				Dbg.Error("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, Application.Quit);
			}

			return null;
		}

		/// <summary>
		///		Join a match by using its id.
		///		<see cref="CreateSocket"/> must be called before this method.
		/// </summary>
		/// <param name="matchId">The id of the match to join.</param>
		/// <returns>The connection information of the server to connect to.</returns>
		public async Task<ServerData> JoinMatch(string matchId)
		{
			try
			{
				_match = await _socket.JoinMatchAsync(matchId);

				var result = await _matchStateReceivedCompletionSource.Task;

				return new ServerData()
				{
					Id = result.Id,
					IpAddress = result.Ip,
					GamePort = result.GamePort,
					StatusPort = result.StatusPort,
					CurrentPlayers = _match.Size
				};
			}
			catch (TaskCanceledException)
			{
				Dbg.Error("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, Application.Quit);
			}

			return null;
		}

		/// <summary>
		///		Join a chat room. May only execute when we are connected to match.
		///		<see cref="CreateSocket"/> must be called before this method.
		/// </summary>
		/// <param name="chatState">The type of chat we are looking for.</param>
		public async Task<bool> JoinChatRoom(Chat.ChatState chatState)
		{
			string id;
			ChannelType channelType;

			switch (chatState)
			{
				case Chat.ChatState.Global:
					id = _match.Id;
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
				Dbg.Error("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, Application.Quit);
				return false;
			}

			return true;
		}

		/// <summary>
		///		Send a message to the chat room we are currently connected to.
		///		<see cref="JoinChatRoom"/> must be called before this.
		/// </summary>
		/// <param name="chatText">The text we want to send.</param>
		public async void SendChat(string chatText)
		{
			try
			{
				await _socket.WriteChatMessageAsync(_chatChannel, chatText);

			}
			catch (TaskCanceledException)
			{
				Dbg.Error("Nakama disconnected when doing task");
				OnNakamaError.Invoke(Localization.NoNakamaConnection, Application.Quit);
			}
		}

		public async void LogOut()
		{
			await _client.SessionLogoutAsync(_session);
			_socket?.CloseAsync();
		}

		private void OnApplicationQuit()
		{
			_cancelToken.Cancel();
			_socket?.CloseAsync();
		}
	}
}
