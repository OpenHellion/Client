using System;
using ZeroGravity.Network;
using Newtonsoft.Json;

namespace OpenHellion.Networking.Message
{
	[Serializable]
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class InviteMessage : DataPacket
	{
		public float Time;

		public string ServerId;

		public VesselObjectID SpawnPointId;
	}
}
