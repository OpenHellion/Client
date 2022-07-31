using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class WDServerStatusResponse : NetworkData
	{
		public ResponseResult Response = ResponseResult.Success;

		public int ProcessId;

		public short CurrentPlayers;

		public short MaxPlayers;

		public bool MainLoopActive;
	}
}
