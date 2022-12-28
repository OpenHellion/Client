using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
using OpenHellion.ProviderSystem;

namespace ZeroGravity.Network
{
	public class NetworkController : MonoBehaviour
	{
		public static string MainServerAddress;

		public static int MainServerPort;

		public static CharacterData CharacterData;

		public EventSystem EventSystem;

		public long SenderID;

		private MainServerThreads _mainServerThread;

		private ConnectionThread _connectionThread;

		private HashSet<long> _spawnObjectsList = new HashSet<long>();

		private HashSet<long> _subscribeToObjectsList = new HashSet<long>();

		private HashSet<long> _unsubscribeFromObjectsList = new HashSet<long>();

		public static string NameOfCurrentServer = string.Empty;

		private bool GetP2PPacketsThreadActive;

		private void Awake()
		{
			EventSystem = new EventSystem();
			_mainServerThread = new MainServerThreads();

		}

		private void FixedUpdate()
		{
			EventSystem.InvokeQueuedData();

			if (_spawnObjectsList.Count > 0)
			{
				SpawnObjectsRequest spawnObjectsRequest = new SpawnObjectsRequest();
				spawnObjectsRequest.GUIDs = new List<long>(_spawnObjectsList);
				SendToGameServer(spawnObjectsRequest);
				_spawnObjectsList.Clear();
			}
			if (_subscribeToObjectsList.Count > 0)
			{
				SubscribeToObjectsRequest subscribeToObjectsRequest = new SubscribeToObjectsRequest();
				subscribeToObjectsRequest.GUIDs = new List<long>(_subscribeToObjectsList);
				SendToGameServer(subscribeToObjectsRequest);
				_subscribeToObjectsList.Clear();
			}
			if (_unsubscribeFromObjectsList.Count > 0)
			{
				UnsubscribeFromObjectsRequest unsubscribeFromObjectsRequest = new UnsubscribeFromObjectsRequest();
				unsubscribeFromObjectsRequest.GUIDs = new List<long>(_unsubscribeFromObjectsList);
				SendToGameServer(unsubscribeFromObjectsRequest);
				_unsubscribeFromObjectsList.Clear();
			}

			// Handle Steam P2P packets.
			if (ProviderManager.MainProvider is SteamProvider && !GetP2PPacketsThreadActive)
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

		public void SendToMainServer(NetworkData data)
		{
			_mainServerThread.Send(data);
		}

		public void ConnectToGame(GameServerUI serverData, string userId, CharacterData charData, string password)
		{
			CharacterData = charData;
			if (_connectionThread != null)
			{
				_connectionThread.Disconnect();
			}
			_connectionThread = new ConnectionThread();
			NameOfCurrentServer = serverData.Name;
			if (!serverData.UseAltIPAddress)
			{
				_connectionThread.Start(serverData.IPAddress, serverData.GamePort, serverData.Id, password, userId);
			}
			else
			{
				_connectionThread.Start(serverData.AltIPAddress, serverData.AltGamePort, serverData.Id, password, userId);
			}
		}

		public void ConnectToGameSP(int port, string userId, CharacterData charData)
		{
			CharacterData = charData;
			if (_connectionThread != null)
			{
				_connectionThread.Disconnect();
			}
			_connectionThread = new ConnectionThread();
			_connectionThread.Start("127.0.0.1", port, 0L, string.Empty, userId);
		}

		public void SendToGameServer(NetworkData data)
		{
			_connectionThread.Send(data);
		}

		private void OnDestroy()
		{
			if (_connectionThread != null)
			{
				_connectionThread.Disconnect();
			}
		}

		public void DisconnectImmediate()
		{
			if (_connectionThread != null)
			{
				_connectionThread.DisconnectImmediate();
			}
		}

		public void Disconnect()
		{
			if (_connectionThread != null)
			{
				_connectionThread.Disconnect();
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
