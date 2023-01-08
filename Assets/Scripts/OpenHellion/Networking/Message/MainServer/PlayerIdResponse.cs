using System;
using Newtonsoft.Json;

namespace OpenHellion.Networking.Message.MainServer
{
	[Serializable]
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class PlayerIdResponse : DataPacket
	{
		public ResponseResult Result;
		public string PlayerId;
	}
}
