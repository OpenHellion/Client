using System;

namespace OpenHellion.Networking.MainServerMessage
{
	public abstract class NetworkMessage : DataContainer
	{
		public abstract string GetDestination();
	}
}
