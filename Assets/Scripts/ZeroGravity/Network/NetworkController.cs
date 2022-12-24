using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;

namespace ZeroGravity.Network
{
	public class NetworkController : MonoBehaviour
	{
		public static string MainServerAddress;

		public static int MainServerPort;

		public static CharacterData CharacterData;

		public MainServerThreads mainThreads;

		public EventSystem EventSystem;

		private bool mainSocketReady;

		public long SenderID;

		public ConnectionThread gameConnectionThreads;

		private HashSet<long> spawnObjectsList = new HashSet<long>();

		private HashSet<long> subscribeToObjectsList = new HashSet<long>();

		private HashSet<long> unsubscribeFromObjectsList = new HashSet<long>();

		public static string NameOfCurrentServer = string.Empty;

		private bool GetP2PPacketsThreadActive;

		private SteamNetworkingMessagesSessionRequest_t _sessionRequest;

		private void Awake()
		{
			EventSystem = new EventSystem();
			mainThreads = new MainServerThreads();

		}

		private void FixedUpdate()
		{
			EventSystem.InvokeQueuedData();

			if (spawnObjectsList.Count > 0)
			{
				SpawnObjectsRequest spawnObjectsRequest = new SpawnObjectsRequest();
				spawnObjectsRequest.GUIDs = new List<long>(spawnObjectsList);
				SendToGameServer(spawnObjectsRequest);
				spawnObjectsList.Clear();
			}
			if (subscribeToObjectsList.Count > 0)
			{
				SubscribeToObjectsRequest subscribeToObjectsRequest = new SubscribeToObjectsRequest();
				subscribeToObjectsRequest.GUIDs = new List<long>(subscribeToObjectsList);
				SendToGameServer(subscribeToObjectsRequest);
				subscribeToObjectsList.Clear();
			}
			if (unsubscribeFromObjectsList.Count > 0)
			{
				UnsubscribeFromObjectsRequest unsubscribeFromObjectsRequest = new UnsubscribeFromObjectsRequest();
				unsubscribeFromObjectsRequest.GUIDs = new List<long>(unsubscribeFromObjectsList);
				SendToGameServer(unsubscribeFromObjectsRequest);
				unsubscribeFromObjectsList.Clear();
			}

			// Handle Steam P2P packets.
			if (SteamManager.Initialized && !GetP2PPacketsThreadActive)
			{
				new Thread(P2PPacketListener).Start();
			}
		}

		public void RequestObjectSpawn(long guid)
		{
			spawnObjectsList.Add(guid);
		}

		public void RequestObjectSubscribe(long guid)
		{
			subscribeToObjectsList.Add(guid);
			unsubscribeFromObjectsList.Remove(guid);
		}

		public void RequestObjectUnsubscribe(long guid)
		{
			unsubscribeFromObjectsList.Add(guid);
			subscribeToObjectsList.Remove(guid);
		}

		public void SendToMainServer(NetworkData data)
		{
			mainThreads.Send(data);
		}

		public void ConnectToGame(GameServerUI serverData, string steamId, CharacterData charData, string password)
		{
			CharacterData = charData;
			if (gameConnectionThreads != null)
			{
				gameConnectionThreads.Disconnect();
			}
			gameConnectionThreads = new ConnectionThread();
			NameOfCurrentServer = serverData.Name;
			if (!serverData.UseAltIPAddress)
			{
				gameConnectionThreads.Start(serverData.IPAddress, serverData.GamePort, serverData.Id, password, steamId);
			}
			else
			{
				gameConnectionThreads.Start(serverData.AltIPAddress, serverData.AltGamePort, serverData.Id, password, steamId);
			}
		}

		public void ConnectToGameSP(int port, string steamId, CharacterData charData)
		{
			CharacterData = charData;
			if (gameConnectionThreads != null)
			{
				gameConnectionThreads.Disconnect();
			}
			gameConnectionThreads = new ConnectionThread();
			gameConnectionThreads.Start("127.0.0.1", port, 0L, string.Empty, steamId);
		}

		public void SendToGameServer(NetworkData data)
		{
			if (SteamManager.Initialized)
			{
				gameConnectionThreads.Send(data);
			}
		}

		private void OnDestroy()
		{
			if (gameConnectionThreads != null)
			{
				gameConnectionThreads.Disconnect();
			}
		}

		public void DisconnectImmediate()
		{
			if (gameConnectionThreads != null)
			{
				gameConnectionThreads.DisconnectImmediate();
			}
		}

		public void Disconnect()
		{
			if (gameConnectionThreads != null)
			{
				gameConnectionThreads.Disconnect();
			}
		}

		/// <summary>
		/// 	Read and invoke P2P packets sent though Steam.<br/>
		/// 	TODO: Integrate this into <c>ZeroGravity.Network.ConnectionThread</c> or create a new class.
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
					NetworkData networkData = Serializer.ReceiveData(new MemoryStream(message));
					Debug.Log(networkData);
					if (networkData is ISteamP2PMessage)
					{
						EventSystem.Invoke(networkData);
					}
				}
			} finally {
				Marshal.DestroyStructure<SteamNetworkingMessage_t>(ptr[0]);
			}
			GetP2PPacketsThreadActive = false;
		}
	}
}
