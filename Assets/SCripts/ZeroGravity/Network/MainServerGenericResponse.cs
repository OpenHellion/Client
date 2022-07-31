using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class MainServerGenericResponse : NetworkData
	{
		public ResponseResult Response = ResponseResult.Success;

		public string Message = string.Empty;
	}
}
