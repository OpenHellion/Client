using System;
using Newtonsoft.Json;
using ZeroGravity.Network;

namespace OpenHellion.Net.Message
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
