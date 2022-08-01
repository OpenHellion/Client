using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class InitializeShipAuxData : InitializeSpaceObjectAuxData
	{
		public VesselObjects VesselObjects;
	}
}
