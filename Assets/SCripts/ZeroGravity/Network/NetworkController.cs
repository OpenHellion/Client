using System.Collections.Generic;
using UnityEngine;

namespace ZeroGravity.Network
{
	public class NetworkController : MonoBehaviour
	{
		public static string MainServerAddres;

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

		private void Awake()
		{
			EventSystem = new EventSystem();
			mainThreads = new MainServerThreads();
		}

		private void Start()
		{
		}

		private void FixedUpdate()
		{
			if (spawnObjectsList.Count > 0)
			{
				SpawnObjectsRequest spawnObjectsRequest = new SpawnObjectsRequest();
				spawnObjectsRequest.GUIDs = new List<long>(spawnObjectsList);
				SpawnObjectsRequest data = spawnObjectsRequest;
				SendToGameServer(data);
				spawnObjectsList.Clear();
			}
			if (subscribeToObjectsList.Count > 0)
			{
				SubscribeToObjectsRequest subscribeToObjectsRequest = new SubscribeToObjectsRequest();
				subscribeToObjectsRequest.GUIDs = new List<long>(subscribeToObjectsList);
				SubscribeToObjectsRequest data2 = subscribeToObjectsRequest;
				SendToGameServer(data2);
				subscribeToObjectsList.Clear();
			}
			if (unsubscribeFromObjectsList.Count > 0)
			{
				UnsubscribeFromObjectsRequest unsubscribeFromObjectsRequest = new UnsubscribeFromObjectsRequest();
				unsubscribeFromObjectsRequest.GUIDs = new List<long>(unsubscribeFromObjectsList);
				UnsubscribeFromObjectsRequest data3 = unsubscribeFromObjectsRequest;
				SendToGameServer(data3);
				unsubscribeFromObjectsList.Clear();
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
			if (SteamManager.Initialized)
			{
				mainThreads.Send(data);
			}
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
	}
}
