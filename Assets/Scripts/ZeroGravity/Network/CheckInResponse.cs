using OpenHellion.Networking.Message;
using OpenHellion.Networking.Message.MainServer;
using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class CheckInResponse : NetworkData
	{
		public ResponseResult Response = ResponseResult.Success;

		public string Message = string.Empty;

		public long ServerID;

		public IPAddressRange[] AdminIPAddressRanges;
	}
}
