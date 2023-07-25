using System;
using ZeroGravity.Network;

namespace OpenHellion.Networking {
	public class ServerData
	{
		public CharacterData CharacterData;

		public string Id;

		public string Name;

		public string IpAddress;

		public uint GamePort;

		public uint StatusPort;

		public uint Hash;

		public int CurrentPlayers;

		public int MaxPlayers;

		public string Description;
	}
}
