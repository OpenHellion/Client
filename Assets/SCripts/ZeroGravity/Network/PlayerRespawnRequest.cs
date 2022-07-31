using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class PlayerRespawnRequest : NetworkData
	{
		public long GUID;
	}
}
