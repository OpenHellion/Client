using System;

namespace OpenHellion.Networking.Message.MainServer
{
	public abstract class MSMessage : DataPacket
	{
		public abstract string GetDestination();
	}
}
