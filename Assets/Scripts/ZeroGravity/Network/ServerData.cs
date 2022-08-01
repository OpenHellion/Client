using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class ServerData
	{
		public long Id;

		public string Name;

		public string IPAddress;

		public int GamePort;

		public int StatusPort;

		public string AltIPAddress;

		public int AltGamePort;

		public int AltStatusPort;

		public bool Locked;

		public ServerTag Tag;

		public uint Hash;
	}
}
