using System;
using ZeroGravity.Network;

namespace OpenHellion.Net
{
	public class ServerData
	{
		public CharacterData CharacterData;

		public string Id;

		public string Name;

		public string IpAddress;

		public int GamePort;

		public int StatusPort;

		public uint Hash;

		public int CurrentPlayers;

		public int MaxPlayers;

		public string Description;
	}
}
