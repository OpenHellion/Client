using System;
using Newtonsoft.Json;

namespace OpenHellion.Networking.Message.MainServer
{
	[Serializable]
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class ServerDataPacket : DataPacket
	{
		public string Id;
		public string IpAddress;
		public ushort Port;
		public uint Hash;
		public Region Region;
	}
}
