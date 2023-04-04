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

		private GSConnection m_GameConnection;

		private bool m_GetP2PPacketsThreadActive;

		private readonly HashSet<long> m_SpawnObjectsList = new HashSet<long>();

		private readonly HashSet<long> m_SubscribeToObjectsList = new HashSet<long>();

		private readonly HashSet<long> m_UnsubscribeFromObjectsList = new HashSet<long>();

		private readonly ConcurrentQueue<Tuple<float, Type>> m_SentLog = new ConcurrentQueue<Tuple<float, Type>>();

		private readonly ConcurrentQueue<Tuple<float, Type>> m_ReceivedLog = new ConcurrentQueue<Tuple<float, Type>>();

		private const int k_MaxNetworkDataLogsSize = 3000;

		private readonly DateTime m_ClientStartTime = DateTime.UtcNow.ToUniversalTime();

		[Title("Diagnostics")]
		public int UnprocessedPackets;

		public string DataLogs = "";

		private static NetworkController s_Instance;
		public static NetworkController Instance
		{
			get
			{
				if (s_Instance == null)
				{
					Dbg.Error("Tried to get network controller before it has been initialised.");
				}

				return s_Instance;
			}
		}

		private string m_PlayerId = null;
		public static string PlayerId
		{
			get
			{
				if (Instance.m_PlayerId.IsNullOrEmpty())
				{
					Instance.m_PlayerId = PlayerPrefs.GetString("player_id", null);

					// Generate new player id.
					if (Instance.m_PlayerId.IsNullOrEmpty())
					{
						string uuid = Guid.NewGuid().ToString();
						Instance.m_PlayerId = uuid;
						PlayerPrefs.SetString("player_id", uuid);

						Dbg.Log("Generated new player id: " + uuid);
					}
				}

				return Instance.m_PlayerId;
			}
			set
			{
				Instance.m_PlayerId = value;
				PlayerPrefs.SetString("player_id", value);
			}
		}

		private void Awake()
		{
			// Only one instance allowed.
			if (s_Instance != null)
			{
				Destroy(this);
				return;
			}

			s_Instance = this;
		}

		private void FixedUpdate()
		{
			EventSystem.Instance.InvokeQueuedData();

			if (m_SpawnObjectsList.Count > 0)
			{
				SpawnObjectsRequest spawnObjectsRequest = new SpawnObjectsRequest
				{
					GUIDs = new List<long>(m_SpawnObjectsList)
				};

				SendToGameServer(spawnObjectsRequest);
				m_SpawnObjectsList.Clear();
			}
			if (m_SubscribeToObjectsList.Count > 0)
			{
				SubscribeToObjectsRequest subscribeToObjectsRequest = new SubscribeToObjectsRequest
				{
					GUIDs = new List<long>(m_SubscribeToObjectsList)
				};

				SendToGameServer(subscribeToObjectsRequest);
				m_SubscribeToObjectsList.Clear();
			}
			if (m_UnsubscribeFromObjectsList.Count > 0)
			{
				UnsubscribeFromObjectsRequest unsubscribeFromObjectsRequest = new UnsubscribeFromObjectsRequest
				{
					GUIDs = new List<long>(m_UnsubscribeFromObjectsList)
				};

				SendToGameServer(unsubscribeFromObjectsRequest);
				m_UnsubscribeFromObjectsList.Clear();
			}

			if (m_GameConnection != null)
				UnprocessedPackets = m_GameConnection.Tick();

			DataLogs = GetNetworkDataLogs();

			// Handle Steam P2P packets.
			if (!ProviderManager.SteamId.IsNullOrEmpty() && !m_GetP2PPacketsThreadActive)
			{
				new Thread(P2PPacketListener).Start();
			}
		}

		public void RequestObjectSpawn(long guid)
		{
			m_SpawnObjectsList.Add(guid);
		}

		public void RequestObjectSubscribe(long guid)
		{
			m_SubscribeToObjectsList.Add(guid);
			m_UnsubscribeFromObjectsList.Remove(guid);
		}

		public void RequestObjectUnsubscribe(long guid)
		{
			m_UnsubscribeFromObjectsList.Add(guid);
			m_SubscribeToObjectsList.Remove(guid);
		}

		public void ConnectToGame(ServerData serverData, CharacterData characterData, string password)
		{
			CharacterData = characterData;
			m_GameConnection?.Disconnect();
			m_GameConnection = new GSConnection();

			NameOfCurrentServer = serverData.Name;
			m_GameConnection.Connect(serverData.IpAddress, serverData.GamePort, serverData.Id, password);
		}

		public void ConnectToGameSP(int port, CharacterData characterData)
		{
			CharacterData = characterData;
			m_GameConnection?.Disconnect();

			Dbg.Log("Connecting to singleplayer server with port", port);
			m_GameConnection = new GSConnection();
			m_GameConnection.Connect("127.0.0.1", port, string.Empty, string.Empty);
		}

		public void SendToGameServer(NetworkData data)
		{
			m_GameConnection.Send(data);
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
			m_GameConnection?.Disconnect();
		}

		public void Disconnect()
		{
			m_GameConnection?.Disconnect();
		}

		/// <summary>
		/// 	Read and invoke P2P packets sent though Steam.<br/>
		/// 	TODO: Create a new class.
		/// </summary>
		private void P2PPacketListener()
		{
			m_GetP2PPacketsThreadActive = true;

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
					NetworkData networkData = ProtoSerialiser.Unpackage((Stream)new MemoryStream(message));
					Debug.Log(networkData);
					if (networkData is ISteamP2PMessage)
					{
						EventSystem.Instance.Invoke(networkData);
					}
				}
			}
			finally
			{
				Marshal.DestroyStructure<SteamNetworkingMessage_t>(ptr[0]);
			}
			m_GetP2PPacketsThreadActive = false;
		}

		public static void LogReceivedNetworkData(Type type)
		{
			Instance.m_ReceivedLog.Enqueue(new Tuple<float, Type>((float)(DateTime.UtcNow.ToUniversalTime() - Instance.m_ClientStartTime).TotalSeconds, type));
			while (Instance.m_ReceivedLog.Count > k_MaxNetworkDataLogsSize)
			{
				Instance.m_ReceivedLog.TryDequeue(out var _);
			}
		}

		public static void LogSentNetworkData(Type type)
		{
			Instance.m_SentLog.Enqueue(new Tuple<float, Type>((float)(DateTime.UtcNow.ToUniversalTime() - Instance.m_ClientStartTime).TotalSeconds, type));
			while (Instance.m_SentLog.Count > k_MaxNetworkDataLogsSize)
			{
				Instance.m_SentLog.TryDequeue(out var _);
			}
		}

		public static string GetNetworkDataLogs()
		{
			if (Instance.m_ReceivedLog.IsEmpty || Instance.m_SentLog.IsEmpty) return "";

			Tuple<float, Type>[] source = Instance.m_ReceivedLog.ToArray();
			float lastRecvdTime = source.Last().Item1;
			IEnumerable<Tuple<float, Type>> recvd = source.Where((Tuple<float, Type> m) => lastRecvdTime - m.Item1 <= 300f);
			float item = recvd.First().Item1;
			string text = "Received packets (in last " + (lastRecvdTime - item).ToString("0") + "s):\n";
			text += string.Join("\n", from z in (from x in recvd.Select((Tuple<float, Type> m) => m.Item2).Distinct()
					select new Tuple<string, int>(x.Name, recvd.Count((Tuple<float, Type> n) => n.Item2 == x)) into y
					orderby y.Item2
					select y).Reverse()
				select z.Item1 + ": " + z.Item2);

			Tuple<float, Type>[] source2 = Instance.m_SentLog.ToArray();
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
