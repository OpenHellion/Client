using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class LogOutResponse : NetworkData
	{
		public OldResponseResult Response = OldResponseResult.Success;

		public LogOutResponse()
		{
			Response = OldResponseResult.Success;
		}
	}
}
