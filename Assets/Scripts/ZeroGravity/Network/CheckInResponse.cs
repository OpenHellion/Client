using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class CheckInResponse : NetworkData
	{
		public OldResponseResult Response = OldResponseResult.Success;

		public string Message = string.Empty;

		public long ServerID;

		public IPAddressRange[] AdminIPAddressRanges;
	}
}
