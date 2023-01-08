using System;
using ZeroGravity.Network;

namespace OpenHellion.Networking.Message
{
	[Serializable]
	public class InviteMessage : DataPacket
	{
		public float Time;

		public string ServerId;

		public string Password;

		public VesselObjectID SpawnPointId;
	}
}
