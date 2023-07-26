using OpenHellion.Net.Message;
using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class LogOutResponse : NetworkData
	{
		public ResponseResult Response = ResponseResult.Success;

		public LogOutResponse()
		{
			Response = ResponseResult.Success;
		}
	}
}
