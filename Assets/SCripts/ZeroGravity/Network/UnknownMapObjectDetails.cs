using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class UnknownMapObjectDetails
	{
		public long GUID;

		public long SpawnRuleID;

		public OrbitData LastKnownOrbit;
	}
}
