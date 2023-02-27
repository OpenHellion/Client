#pragma warning disable UNT0013

using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
using OpenHellion.ProviderSystem;
using ZeroGravity.Network;
using TriInspector;
using System.Collections.Concurrent;
using System.Linq;
using ZeroGravity;
using UnityEditor;
using System.Net.Sockets;
using OpenHellion.IO;

namespace OpenHellion.Networking
{
	public class NetworkController : MonoBehaviour
	{
		public static CharacterData CharacterData;

		public static string NameOfCurrentServer = string.Empty;

		private GSConnection _gameConnection;

		private bool GetP2PPacketsThreadActive;

		private readonly HashSet<long> _spawnObjectsList = new HashSet<long>();

		private readonly HashSet<long> _subscribeToObjectsList = new HashSet<long>();

		private readonly HashSet<long> _unsubscribeFromObjectsList = new HashSet<long>();

		private readonly ConcurrentQueue<Tuple<float, Type>> _sentLog = new ConcurrentQueue<Tuple<float, Type>>();

		private readonly ConcurrentQueue<Tuple<float, Type>> _receivedLog = new ConcurrentQueue<Tuple<float, Type>>();

		private const int _maxNetworkDataLogsSize = 3000;

		private readonly DateTime _clientStartTime = DateTime.UtcNow.ToUniversalTime();

		[Title("Diagnostics"), ReadOnly]
		public long PlayerGUID;

		[ReadOnly, ShowInInspector]
		public int UnprocessedPackets
		{
			get;
			private set;
		}

		[ReadOnly, TextArea(1, 20)]
		public string DataLogs = "";

		private static NetworkController s_instance;
		public static NetworkController Instance
		{
			get
			{
				if (s_instance == null)
				{
					Dbg.Error("Tried to get network controller before it has been initialised.");
				}

				return s_instance;
			}
		}

		[ReadOnly, SerializeField]
		private string _playerId = null;
		public static string PlayerId
		{
			get
			{
				if (Instance._playerId.IsNullOrEmpty())
				{
					Instance._playerId = PlayerPrefs.GetString("player_id", null);

					// Generate new player id.
					if (Instance._playerId.IsNullOrEmpty())
					{
						string uuid = Guid.NewGuid().ToString();
						Instance._playerId = uuid;
						PlayerPrefs.SetString("player_id", uuid);

						Dbg.Log("Generated new player id: " + uuid);
					}
				}

				return Instance._playerId;
			}
			set
			{
				Instance._playerId = value;
				PlayerPrefs.SetString("player_id", value);
			}
		}

		private void Awake()
		{
			// Only one instance allowed.
			if (s_instance != null)
			{
				Destroy(this);
				return;
			}

			s_instance = this;
		}

		private void FixedUpdate()
		{
			EventSystem.Instance.InvokeQueuedData();

			if (_spawnObjectsList.Count > 0)
			{
				SpawnObjectsRequest spawnObjectsRequest = new SpawnObjectsRequest
				{
					GUIDs = new List<long>(_spawnObjectsList)
				};

				SendToGameServer(spawnObjectsRequest);
				_spawnObjectsList.Clear();
			}
			if (_subscribeToObjectsList.Count > 0)
			{
				SubscribeToObjectsRequest subscribeToObjectsRequest = new SubscribeToObjectsRequest
				{
					GUIDs = new List<long>(_subscribeToObjectsList)
				};

				SendToGameServer(subscribeToObjectsRequest);
				_subscribeToObjectsList.Clear();
			}
			if (_unsubscribeFromObjectsList.Count > 0)
			{
				UnsubscribeFromObjectsRequest unsubscribeFromObjectsRequest = new UnsubscribeFromObjectsRequest
				{
					GUIDs = new List<long>(_unsubscribeFromObjectsList)
				};

				SendToGameServer(unsubscribeFromObjectsRequest);
				_unsubscribeFromObjectsList.Clear();
			}

			if (_gameConnection != null)
				UnprocessedPackets = _gameConnection.Tick();

			DataLogs = GetNetworkDataLogs();

			// Handle Steam P2P packets.
			if (!ProviderManager.SteamId.IsNullOrEmpty() && !GetP2PPacketsThreadActive)
			{
				new Thread(P2PPacketListener).Start();
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

		public void ConnectToGame(ServerData serverData, CharacterData charData, string password)
		{
			CharacterData = charData;
			if (_gameConnection != null)
			{
				_gameConnection.Disconnect();
			}
			_gameConnection = new GSConnection();

			NameOfCurrentServer = serverData.Name;
			_gameConnection.Connect(serverData.IpAddress, serverData.GamePort, serverData.Id, password);
		}

		public void ConnectToGameSP(int port, CharacterData charData)
		{
			CharacterData = charData;
			if (_gameConnection != null)
			{
				_gameConnection.Disconnect();
			}

			Dbg.Info("Connecting to singleplayer server with port", port);
			_gameConnection = new GSConnection();
			_gameConnection.Connect("127.0.0.1", port, string.Empty, string.Empty);
		}

		public void SendToGameServer(NetworkData data)
		{
			_gameConnection.Send(data);
		}

		/// <summary>
		/// 	Send a request directly to a TCP endpoint.<br />
		/// 	Useful for status requests.
		/// </summary>
		public static NetworkData SendTCP(NetworkData data, string address, int port, out int latency, bool getResponse = true, bool logException = false)
		{
			latency = -1;
			try
			{
				TcpClient tcpClient = new TcpClient();
				IAsyncResult asyncResult = tcpClient.BeginConnect(address, port, null, null);
				WaitHandle asyncWaitHandle = asyncResult.AsyncWaitHandle;

				// Check if connection has timed out.
				try
				{
					if (!asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(4.0), exitContext: false))
					{
						tcpClient.Close();
						throw new TimeoutException();
					}
					tcpClient.EndConnect(asyncResult);
				}
				finally
				{
					asyncWaitHandle.Close();
				}

				NetworkStream networkStream = tcpClient.GetStream();
				networkStream.ReadTimeout = 1000;
				networkStream.WriteTimeout = 1000;

				byte[] rawData = ProtoSerialiser.Package(data);
				DateTime dateTime = DateTime.UtcNow.ToUniversalTime();

				// Send data.
				networkStream.Write(rawData, 0, rawData.Length);
				networkStream.Flush();

				if (getResponse)
				{
					latency = (int)(DateTime.UtcNow - dateTime).TotalMilliseconds;
					NetworkData result = ProtoSerialiser.Unpackage(networkStream);
					return result;
				}
			}
			catch (Exception ex)
			{
				if (logException)
				{
					Dbg.Error(ex.Message, ex.StackTrace);
				}
			}
			return null;
		}

		private void OnDestroy()
		{
			_gameConnection?.Disconnect();
		}

		public void Disconnect()
		{
			_gameConnection?.Disconnect();
		}

		/// <summary>
		/// 	Read and invoke P2P packets sent though Steam.<br/>
		/// 	TODO: Create a new class.
		/// </summary>
		private void P2PPacketListener() {
			GetP2PPacketsThreadActive = true;

			// Create pointer array and put data in it.
			IntPtr[] ptr = new IntPtr[1];
			int msgSize = SteamNetworkingMessages.ReceiveMessagesOnChannel(0, ptr, 1);

			if (msgSize == 0) {
				return;
			}

			try {
				SteamNetworkingMessage_t netMessage = Marshal.PtrToStructure<SteamNetworkingMessage_t>(ptr[0]);
				if (netMessage.m_cbSize != 0)
				{
					// Copy payload of the message and put it in a byte array.
					byte[] message = new byte[netMessage.m_cbSize];
					Marshal.Copy(netMessage.m_pData, message, 0, message.Length);

					// Deseralise data and invoke code.
					NetworkData networkData = ProtoSerialiser.Unpackage((Stream)new MemoryStream(message));
					Debug.Log(networkData);
					if (networkData is ISteamP2PMessage)
					{
						EventSystem.Instance.Invoke(networkData);
					}
				}
			} finally {
				Marshal.DestroyStructure<SteamNetworkingMessage_t>(ptr[0]);
			}
			GetP2PPacketsThreadActive = false;
		}

		public static void LogReceivedNetworkData(Type type)
		{
			Instance._receivedLog.Enqueue(new Tuple<float, Type>((float)(DateTime.UtcNow.ToUniversalTime() - Instance._clientStartTime).TotalSeconds, type));
			while (Instance._receivedLog.Count > _maxNetworkDataLogsSize)
			{
				Instance._receivedLog.TryDequeue(out var _);
			}
		}

		public static void LogSentNetworkData(Type type)
		{
			Instance._sentLog.Enqueue(new Tuple<float, Type>((float)(DateTime.UtcNow.ToUniversalTime() - Instance._clientStartTime).TotalSeconds, type));
			while (Instance._sentLog.Count > _maxNetworkDataLogsSize)
			{
				Instance._sentLog.TryDequeue(out var _);
			}
		}

		public static string GetNetworkDataLogs()
		{
			if (Instance._receivedLog.IsEmpty || Instance._sentLog.IsEmpty) return "";

			Tuple<float, Type>[] source = Instance._receivedLog.ToArray();
			float lastRecvdTime = source.Last().Item1;
			IEnumerable<Tuple<float, Type>> recvd = source.Where((Tuple<float, Type> m) => lastRecvdTime - m.Item1 <= 300f);
			float item = recvd.First().Item1;
			string text = "Received packets (in last " + (lastRecvdTime - item).ToString("0") + "s):\n";
			text += string.Join("\n", from z in (from x in recvd.Select((Tuple<float, Type> m) => m.Item2).Distinct()
					select new Tuple<string, int>(x.Name, recvd.Count((Tuple<float, Type> n) => n.Item2 == x)) into y
					orderby y.Item2
					select y).Reverse()
				select z.Item1 + ": " + z.Item2);

			Tuple<float, Type>[] source2 = Instance._sentLog.ToArray();
			float lastSentTime = source2.Last().Item1;
			IEnumerable<Tuple<float, Type>> sent = source2.Where((Tuple<float, Type> m) => lastSentTime - m.Item1 <= 300f);
			float item2 = sent.First().Item1;
			text = text + "\n\nSent packets (in last " + (lastSentTime - item2).ToString("0") + "s):\n";
			return text + string.Join("\n", from z in (from x in sent.Select((Tuple<float, Type> m) => m.Item2).Distinct()
					select new Tuple<string, int>(x.Name, sent.Count((Tuple<float, Type> n) => n.Item2 == x)) into y
					orderby y.Item2
					select y).Reverse()
				select z.Item1 + ": " + z.Item2);
		}
	}
}
