using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class CheckInRequest : NetworkData
	{
		public long ServerID;

		public string ServerName;

		public string IPAddress;

		public int GamePort;

		public int StatusPort;

		public string AltIPAddress;

		public int AltGamePort;

		public int AltStatusPort;

		public bool Private;

		public uint ServerHash;

		public bool CleanStart;
	}
}
