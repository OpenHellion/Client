using System;
using Newtonsoft.Json;

namespace OpenHellion.Networking.Message.MainServer
{
	[Serializable]
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class SignInResponse : DataPacket
	{
		public ResponseResult Result;
		public ServerDataPacket Server;
		public string LastSignIn;
	}
}
