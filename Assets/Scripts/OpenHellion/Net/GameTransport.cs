// GameTransport.cs
//
// Copyright (C) 2024, OpenHellion contributors
//
// Inspiration taken from WatsonTcp.
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

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using OpenHellion.IO;
using UnityEngine;
using ZeroGravity.Network;
using Cysharp.Threading.Tasks;


namespace OpenHellion.Net
{
	/// <summary>
	/// 	Lightweight single-connection game transport with framing.
	/// </summary>
	/// <remarks>
	/// 	Largely decoupled from the program, but it does contain some references to <c>EventSystem</c>
	/// 	to invoke received messages. Might move these into callbacks, but it really isn't necessary.
	/// 	Needs TLS support. Depends upon <c>ProtoSerialiser</c> and <c>NetworkData</c>.
	/// </remarks>
	internal sealed class GameTransport
	{
		private const int TIMEOUT_MS = 4000;

		private const int MAX_MESSAGE_SIZE = 16000000;

		private Action _onDisconnected;
		private Socket _client;
		private NetworkStream _connectionStream;
		private bool _isConnectionOpen;

		private Action<NetworkData> _syncResponseReceivedEvent;

		private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

		internal GameTransport(Action onDisconnected)
		{
			_onDisconnected = onDisconnected;
		}

		/// <summary>
		/// 	Establish a connection to a specified game server.
		/// </summary>
		internal async UniTask Connect(string ip, int port)
		{
			_client = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
			{
				Blocking = true
			};

			await _client.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));
			_connectionStream = new NetworkStream(_client, true);
			_isConnectionOpen = true;

			UniTask.RunOnThreadPool(() => ListenTask(_cancellationToken.Token), false).Forget();
		}

		internal async UniTask ListenTask(CancellationToken token)
		{
			while (_isConnectionOpen)
			{
				token.ThrowIfCancellationRequested();
				try
				{
					if (_connectionStream.DataAvailable)
					{
						NetworkData networkData = await ProtoSerialiser.Unpack(_connectionStream, MAX_MESSAGE_SIZE, _cancellationToken.Token);
						if (networkData != null)
						{
							if (networkData.SyncRequest)
							{
								NetworkData res = await EventSystem.InvokeSyncRequest(networkData);
								res.ConversationGuid = networkData.ConversationGuid;
								res.SyncResponse = true;
								SendInternal(res).Forget();
							}
							else if (networkData.SyncResponse)
							{
								_syncResponseReceivedEvent(networkData);
							}
							else if (DateTime.UtcNow <= networkData.ExpirationUtc)
							{
								EventSystem.Invoke(networkData);
							}
#if UNITY_EDITOR
							Debug.LogFormat("Received game data of type {0}.", networkData.GetType());
							NetworkController.LogReceivedNetworkData(networkData.GetType());
#endif
						}
					}
				}
				catch (SocketException)
				{
					Debug.Log("Socket terminated, disconnecting client.");
					DisconnectInternal();
					break;
				}
				catch (ArgumentException ex)
				{
					Debug.LogException(ex);
				}
			}
		}

		internal async UniTaskVoid SendInternal(NetworkData data)
		{
			if (!_isConnectionOpen) return;
			try
			{
				data.ExpirationUtc = DateTime.UtcNow.AddMilliseconds(TIMEOUT_MS);
				var packedData = await ProtoSerialiser.Pack(data);
				await _connectionStream.WriteAsync(packedData).ConfigureAwait(false);

				Debug.LogFormat("Sent game data of type {0} with a size of {1} KB.", data.GetType(), (float)packedData.Length / 1000);
			}
			catch (SocketException)
			{
				Debug.LogWarning("Socket terminated, disconnecting client.");
				DisconnectInternal();
			}
		}

		// Same as SendInternal, but with a async support.
		internal async UniTask SendAsyncInternal(NetworkData data)
		{
			if (!_isConnectionOpen) return;
			try
			{
				data.ExpirationUtc = DateTime.UtcNow.AddMilliseconds(TIMEOUT_MS);
				var packedData = await ProtoSerialiser.Pack(data);
				await _connectionStream.WriteAsync(packedData).ConfigureAwait(false);

				Debug.LogFormat("Sent game data of type {0} with a size of {1} KB.", data.GetType(), (float)packedData.Length / 1000);
			}
			catch (SocketException)
			{
				Debug.LogWarning("Socket terminated, disconnecting client.");
				DisconnectInternal();
			}
		}

		internal async UniTask<NetworkData> SendReceiveAsyncInternal(NetworkData data, int timeout = TIMEOUT_MS)
		{
			if (!_isConnectionOpen) return null;

			try
			{
				data.SyncRequest = true;
				data.ExpirationUtc = DateTime.UtcNow.AddMilliseconds(timeout);
				var packedData = await ProtoSerialiser.Pack(data);

				NetworkData response = null;
				CancellationTokenSource responseCancel = new();
				void responseHandler(NetworkData responseData)
				{
					if (data.ConversationGuid == responseData.ConversationGuid)
					{
						response = responseData;
						responseCancel.Cancel();
					}
				}

				_syncResponseReceivedEvent += responseHandler;

				await _connectionStream.WriteAsync(packedData);
				Debug.LogFormat("Sent game data of type {0} with a size of {1} KB.", data.GetType(), (float)packedData.Length / 1000);

				await UniTask.Delay(timeout, true, cancellationToken: responseCancel.Token).SuppressCancellationThrow();

				_syncResponseReceivedEvent -= responseHandler;

				if (response != null)
				{
					return response;
				}
				else
				{
					throw new TimeoutException("A response to a synchronous request was not received within the timeout window.");
				}
			}
			catch (SocketException)
			{
				Debug.LogWarning("Socket terminated, disconnecting client.");
				DisconnectInternal();
			}

			return null;
		}

		internal void DisconnectInternal()
		{
			_isConnectionOpen = false;
			try
			{
				_client?.Shutdown(SocketShutdown.Both);
			}
			catch (SocketException)
			{
				// Ignored.
			}
			finally
			{
				_connectionStream?.Close();
			}
			_cancellationToken.Cancel();
			_client = null;
			_connectionStream = null;
			_onDisconnected();
		}

		internal void DisconnectImmediateInternal()
		{
			_isConnectionOpen = false;
			_connectionStream.Close();
			_cancellationToken.Cancel();
			Debug.Log("Used internal disconnect.");
		}
	}
}
