// NetworkController.cs
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

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using OpenHellion.IO;
using OpenHellion.Social.RichPresence;
using ZeroGravity.Network;
using Cysharp.Threading.Tasks;

namespace OpenHellion.Net
{
	public class NetworkController : MonoBehaviour
	{
		private static GameTransport _gameTransport;

		private bool _getP2PPacketsThreadActive;

		private readonly HashSet<long> _spawnObjectsList = new HashSet<long>();

		private readonly HashSet<long> _subscribeToObjectsList = new HashSet<long>();

		private readonly HashSet<long> _unsubscribeFromObjectsList = new HashSet<long>();

		private readonly ConcurrentQueue<Tuple<float, Type>> _sentLog = new ConcurrentQueue<Tuple<float, Type>>();

		private readonly ConcurrentQueue<Tuple<float, Type>> _receivedLog = new ConcurrentQueue<Tuple<float, Type>>();

		private const int MaxNetworkDataLogsSize = 3000;

		private readonly DateTime _clientStartTime = DateTime.UtcNow.ToUniversalTime();

		private static NetworkController _instance;

		public static NetworkController Instance
		{
			get
			{
				if (_instance is null)
				{
					Debug.LogError("Tried to get network controller before it has been initialised.");
				}

				return _instance;
			}
		}

		private void Awake()
		{
			// Only one instance allowed.
			if (_instance != null)
			{
				Destroy(this);
				return;
			}

			_instance = this;
		}

		private void FixedUpdate()
		{
			EventSystem.InvokeQueuedData();

			if (_spawnObjectsList.Count > 0)
			{
				SpawnObjectsRequest spawnObjectsRequest = new SpawnObjectsRequest
				{
					GUIDs = new List<long>(_spawnObjectsList)
				};

				Send(spawnObjectsRequest);
				_spawnObjectsList.Clear();
			}

			if (_subscribeToObjectsList.Count > 0)
			{
				SubscribeToObjectsRequest subscribeToObjectsRequest = new SubscribeToObjectsRequest
				{
					GUIDs = new List<long>(_subscribeToObjectsList)
				};

				Send(subscribeToObjectsRequest);
				_subscribeToObjectsList.Clear();
			}

			if (_unsubscribeFromObjectsList.Count > 0)
			{
				UnsubscribeFromObjectsRequest unsubscribeFromObjectsRequest = new UnsubscribeFromObjectsRequest
				{
					GUIDs = new List<long>(_unsubscribeFromObjectsList)
				};

				Send(unsubscribeFromObjectsRequest);
				_unsubscribeFromObjectsList.Clear();
			}

			// Handle Steam P2P packets.
			if (RichPresenceManager.HasSteam && !_getP2PPacketsThreadActive)
			{
				//UniTask.Void(P2PPacketListener);
			}
		}

		public void RequestObjectSpawn(long guid)
		{
			_spawnObjectsList.Add(guid);
		}

		public void RequestObjectSubscribe(long guid)
		{
			_subscribeToObjectsList.Add(guid);
			_unsubscribeFromObjectsList.Remove(guid);
		}

		public void RequestObjectUnsubscribe(long guid)
		{
			_unsubscribeFromObjectsList.Add(guid);
			_subscribeToObjectsList.Remove(guid);
		}

		public async static UniTask ConnectToGame(ServerData serverData, Action onDisconnected)
		{
			_gameTransport?.DisconnectImmediateInternal();
			_gameTransport = new GameTransport(() =>
			{
				_gameTransport = null;
				onDisconnected();
			});

			await _gameTransport.Connect(serverData.IpAddress, serverData.GamePort);
		}


		/// <summary>
		/// 	Send network data to the server.
		/// </summary>
		/// <param name="data">The data to send.</param>
		public static void Send(NetworkData data)
		{
			_gameTransport.SendInternal(data).Forget();
		}

		/// <summary>
		/// 	Use request/response-like communication with async support.
		/// 	A <a cref="TimeoutException"/> is thrown when no response is received within the configured timeframe.
		/// </summary>
		/// <param name="data">The data to send.</param>
		/// <exception cref="TimeoutException"/>
		public static UniTask<NetworkData> SendReceiveAsync(NetworkData data)
		{
			return _gameTransport.SendReceiveAsyncInternal(data);
		}

		/// <summary>
		/// 	Checks the latency between the client and server.
		/// </summary>
		public static async UniTask<int> LatencyTest(string address, int port, bool logException = false)
		{
			try
			{
				TcpClient tcpClient = new TcpClient(address, port);

				NetworkStream networkStream = tcpClient.GetStream();
				networkStream.ReadTimeout = 1000;
				networkStream.WriteTimeout = 1000;

				byte[] rawData = await ProtoSerialiser.Pack(new LatencyTestMessage());
				DateTime dateTime = DateTime.UtcNow.ToUniversalTime();

				// Send data.
				await networkStream.WriteAsync(rawData, 0, rawData.Length);
				await networkStream.FlushAsync();

				return (int)(DateTime.UtcNow - dateTime).TotalMilliseconds;
			}
			catch (SocketException)
			{
				Disconnect();
				return -1;
			}
			catch (Exception ex)
			{
				if (logException)
				{
					Debug.LogException(ex);
				}

				return -1;
			}
		}

		/// <summary>
		/// 	Send a request directly to a TCP endpoint.<br />
		/// 	Useful for status requests.
		/// </summary>
		public static async UniTask<NetworkData> SendTcp(NetworkData data, string address, int port,
			bool getResponse = true, bool logException = false)
		{
			try
			{
				TcpClient tcpClient = new TcpClient(address, port);

				NetworkStream networkStream = tcpClient.GetStream();
				networkStream.ReadTimeout = 1000;
				networkStream.WriteTimeout = 1000;

				byte[] rawData = await ProtoSerialiser.Pack(data);

				// Send data.
				await networkStream.WriteAsync(rawData, 0, rawData.Length);
				await networkStream.FlushAsync();

				if (getResponse)
				{
					NetworkData result = await ProtoSerialiser.Unpack(networkStream, 10000);
					return result;
				}
			}
			catch (Exception ex)
			{
				if (logException)
				{
					Debug.LogException(ex);
				}
			}

			return null;
		}

		private void OnDestroy()
		{
			_gameTransport?.DisconnectInternal();
		}

		/// <summary>
		/// 	Terminate connection cancelling all queued data.
		/// </summary>
		public static void Disconnect()
		{
			_gameTransport?.DisconnectInternal();
		}

		/// <summary>
		/// 	Read and invoke P2P packets sent though Steam.<br/>
		/// </summary>
		private async UniTaskVoid P2PPacketListener()
		{
			_getP2PPacketsThreadActive = true;

			// Create pointer array and put data in it.
			IntPtr[] ptr = new IntPtr[1];
			int msgSize = SteamNetworkingMessages.ReceiveMessagesOnChannel(0, ptr, 1);
			if (msgSize == 0)
			{
				return;
			}

			try
			{
				SteamNetworkingMessage_t netMessage = Marshal.PtrToStructure<SteamNetworkingMessage_t>(ptr[0]);
				if (netMessage.m_cbSize != 0)
				{
					// Copy payload of the message and put it in a byte array.
					byte[] message = new byte[netMessage.m_cbSize];
					Marshal.Copy(netMessage.m_pData, message, 0, message.Length);

					// Deseralise data and invoke code.
					NetworkData networkData = await ProtoSerialiser.Unpack(new MemoryStream(message), 1000000);
					Debug.Log(networkData);
					if (networkData is ISteamP2PMessage)
					{
						EventSystem.Invoke(networkData);
					}
				}
			}
			finally
			{
				Marshal.DestroyStructure<SteamNetworkingMessage_t>(ptr[0]);
			}

			_getP2PPacketsThreadActive = false;
		}
#if UNITY_DEBUG

		private void OnGUI()
		{
			GUILayout.ExpandHeight(true);
			GUILayout.ExpandWidth(true);
			GUILayout.Label(GetNetworkDataLogs());
		}

		public static void LogReceivedNetworkData(Type type)
		{
			Instance._receivedLog.Enqueue(new Tuple<float, Type>(
				(float)(DateTime.UtcNow.ToUniversalTime() - Instance._clientStartTime).TotalSeconds, type));
			while (Instance._receivedLog.Count > MaxNetworkDataLogsSize)
			{
				Instance._receivedLog.TryDequeue(out _);
			}
		}

		public static void LogSentNetworkData(Type type)
		{
			Instance._sentLog.Enqueue(new Tuple<float, Type>(
				(float)(DateTime.UtcNow.ToUniversalTime() - Instance._clientStartTime).TotalSeconds, type));
			while (Instance._sentLog.Count > MaxNetworkDataLogsSize)
			{
				Instance._sentLog.TryDequeue(out var _);
			}
		}

		public static string GetNetworkDataLogs()
		{
			if (Instance._receivedLog.IsEmpty || Instance._sentLog.IsEmpty) return "";

			Tuple<float, Type>[] source = Instance._receivedLog.ToArray();
			float lastRecvdTime = source.Last().Item1;
			IEnumerable<Tuple<float, Type>> recvd =
				source.Where((Tuple<float, Type> m) => lastRecvdTime - m.Item1 <= 300f);
			float item = recvd.First().Item1;
			string text = "Received packets (in last " + (lastRecvdTime - item).ToString("0") + "s):\n";
			text += string.Join("\n", from z in (from x in recvd.Select((Tuple<float, Type> m) => m.Item2).Distinct()
					select new Tuple<string, int>(x.Name, recvd.Count((Tuple<float, Type> n) => n.Item2 == x))
					into y
					orderby y.Item2
					select y).Reverse()
				select z.Item1 + ": " + z.Item2);

			Tuple<float, Type>[] source2 = Instance._sentLog.ToArray();
			float lastSentTime = source2.Last().Item1;
			IEnumerable<Tuple<float, Type>> sent =
				source2.Where((Tuple<float, Type> m) => lastSentTime - m.Item1 <= 300f);
			float item2 = sent.First().Item1;
			text = text + "\n\nSent packets (in last " + (lastSentTime - item2).ToString("0") + "s):\n";
			return text + string.Join("\n",
				from z in (from x in sent.Select((Tuple<float, Type> m) => m.Item2).Distinct()
					select new Tuple<string, int>(x.Name, sent.Count((Tuple<float, Type> n) => n.Item2 == x))
					into y
					orderby y.Item2
					select y).Reverse()
				select z.Item1 + ": " + z.Item2);
		}
#endif
	}
}
