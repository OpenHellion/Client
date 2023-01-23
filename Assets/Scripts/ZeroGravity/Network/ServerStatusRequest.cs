using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class ServerStatusRequest : NetworkData
	{
		public string PlayerId;

		public bool SendDetails;
	}
}
