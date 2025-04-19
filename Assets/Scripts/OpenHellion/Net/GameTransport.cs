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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using OpenHellion.IO;
using ProtoBuf;
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
		private const int TIMEOUT_MS = 2000;

		private const int MAX_MESSAGE_SIZE = 16000000;

		private Action _onDisconnected;
		private Socket _client;
		private NetworkStream _connectionStream;

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

			UniTask.RunOnThreadPool(() => ListenTask(_cancellationToken.Token), false).Forget();
		}

		internal async UniTask ListenTask(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					if (_connectionStream.DataAvailable)
					{
						NetworkData networkData = await ProtoSerialiser.Unpack(_connectionStream, MAX_MESSAGE_SIZE);
						if (networkData.SyncRequest)
						{
							NetworkData res = await EventSystem.InvokeSyncRequest(networkData);

							if (res is null)
							{
								res.Status = NetworkData.MessageStatus.Failure;
							}

							res.ConversationGuid = networkData.ConversationGuid;
							res.SyncResponse = true;
							SendInternal(res).Forget();
						}
						else if (networkData.SyncResponse)
						{
							_syncResponseReceivedEvent(networkData);
						}
						else if (DateTime.Now <= networkData.ExpirationUtc)
						{
							EventSystem.Invoke(networkData);
						}

						#if UNITY_DEBUG
						Debug.LogFormat("Received game data of type {0}.", networkData.GetType());
						NetworkController.LogReceivedNetworkData(networkData.GetType());
						#endif
					}
				}
				catch (NullReferenceException)
				{
					Debug.Log("Socket terminated, disconnecting client.");
					DisconnectInternal().Forget();
				}
				catch (SocketException)
				{
					Debug.Log("Socket terminated, disconnecting client.");
					DisconnectInternal().Forget();
				}
				catch (ProtoException)
				{
					Debug.LogWarning("Protobuf encountered an error when reading from stream.");
				}
				catch (ArgumentNullException)
				{
					Debug.LogErrorFormat("Serialized data buffer is null.");
				}
			}
		}

		/// <summary>
		/// 	Send network data to the server.
		/// </summary>
		/// <param name="data">The data to send.</param>
		internal async UniTaskVoid SendInternal(NetworkData data)
		{
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
				DisconnectInternal().Forget();
			}
			catch (NullReferenceException)
			{
				Debug.Log("Tried to send data to non-existant client.");
				DisconnectInternal().Forget();
			}
			catch (IOException)
			{
				Debug.Log("Socket terminated, disconnecting client.");
				DisconnectInternal().Forget();
			}
			catch (ArgumentNullException)
			{
				Debug.LogErrorFormat("Serialized data buffer is null. Type: {0}. Data:\n{1}", data.GetType().ToString(), data);
			}
		}

		/// <summary>
		/// 	Use request/response-like communication with async support.
		/// </summary>
		/// <param name="data">The data to send.</param>
		internal async UniTask<NetworkData> SendReceiveAsyncInternal(NetworkData data)
		{
			try
			{
				data.SyncRequest = true;
				data.ExpirationUtc = DateTime.UtcNow.AddMilliseconds(TIMEOUT_MS);
				var packedData = await ProtoSerialiser.Pack(data);

				NetworkData response = null;
				void responseHandler(NetworkData responseData)
				{
					if (data.ConversationGuid == responseData.ConversationGuid)
					{
						response = responseData;
					}
				}

				_syncResponseReceivedEvent += responseHandler;

				await _connectionStream.WriteAsync(packedData);
				Debug.LogFormat("Sent game data of type {0} with a size of {1} KB.", data.GetType(), (float)packedData.Length / 1000);

				await UniTask.Delay(TIMEOUT_MS, true);

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
			catch (ProtoException)
			{
				Debug.LogWarning("Protobuf encountered an error when reading from stream.");
			}
			catch (NullReferenceException)
			{
				Debug.Log("Tried to send data to non-existant client.");
				DisconnectInternal().Forget();
			}
			catch (IOException)
			{
				Debug.Log("Socket terminated, disconnecting client.");
				DisconnectInternal().Forget();
			}
			catch (SocketException)
			{
				Debug.LogWarning("Socket terminated, disconnecting client.");
				DisconnectInternal().Forget();
			}
			catch (ArgumentNullException)
			{
				Debug.LogErrorFormat("Serialized data buffer is null. Type: {0}. Data:\n{1}", data.GetType().ToString(), data);
			}

			return null;
		}

		/// <summary>
		/// 	Terminate connection cancelling all queued data.
		/// </summary>
		internal async UniTaskVoid DisconnectInternal()
		{
			_cancellationToken.Cancel();

			if (_client != null && _client.Connected)
			{
				_client?.Disconnect(false);
				_client?.Close();
			}
			if (_connectionStream != null)
			{
				await _connectionStream.DisposeAsync().ConfigureAwait(false);
			}
			_client = null;
			_connectionStream = null;
			_onDisconnected();

			Debug.Log("Successfully disconnected from server...");
		}

		/// <summary>
		/// 	Terminate connection without calling callback.
		/// </summary>
		internal void DisconnectImmediateInternal()
		{
			_cancellationToken.Cancel();

			if (_client != null && _client.Connected)
			{
				_client?.Disconnect(false);
				_client?.Close();
			}
			if (_connectionStream != null)
			{
				_connectionStream.Dispose();
			}

			_client = null;
			_connectionStream = null;

			Debug.Log("Successfully disconnected from server...");
		}
	}
}
