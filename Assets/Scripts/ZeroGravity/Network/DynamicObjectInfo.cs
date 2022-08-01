using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class DynamicObjectInfo
	{
		public long GUID;

		public DynamicObjectStats Stats;
	}
}
