using System;
using Newtonsoft.Json;

namespace OpenHellion.Networking.Message.MainServer
{
	[Serializable]
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class SignInRequest : MSMessage
	{
		public string PlayerId;
		public string Version;
		public uint Hash;
		public string JoiningId;

		public override string GetDestination()
		{
			return "signin";
		}
	}
}
