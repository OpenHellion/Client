namespace OpenHellion.Networking.MainServerMessage
{
	public abstract class ServerData : DataContainer
	{
		public string id;
		public string ipAddress;
		public ushort port;
		public uint hash;
		public Region region;
	}
}
