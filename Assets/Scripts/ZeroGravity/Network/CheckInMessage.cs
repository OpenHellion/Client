using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class CheckInMessage : NetworkData
	{
		public long ServerID;
	}
}
