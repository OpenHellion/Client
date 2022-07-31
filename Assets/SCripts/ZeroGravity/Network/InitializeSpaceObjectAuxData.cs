using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	[ProtoInclude(20000, typeof(InitializeShipAuxData))]
	public interface InitializeSpaceObjectAuxData
	{
	}
}
