using System;
using ZeroGravity.Network;

namespace OpenHellion.Networking {
	public class ServerData
	{
		public CharacterData CharacterData;

		public DateTime LastUpdateTime;

		public string Id;

		public string Name;

		public string IpAddress;

		public int GamePort;

		public int StatusPort;

		public bool Locked;

		public uint Hash;

		public int CurrentPlayers;

		public int AlivePlayers;

		public int MaxPlayers;

		public bool Online;

		public int PingTime;

		public string Description;

		public int Ping;
	}
}
