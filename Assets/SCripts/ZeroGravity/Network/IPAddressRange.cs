using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class IPAddressRange
	{
		public string StartAddress;

		public string EndAddress;
	}
}
