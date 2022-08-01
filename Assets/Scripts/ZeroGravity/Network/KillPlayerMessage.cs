using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class KillPlayerMessage : NetworkData
	{
		public long GUID;

		public HurtType CauseOfDeath;

		public VesselDamageType VesselDamageType;

		public CorpseDetails CorpseDetails;
	}
}
