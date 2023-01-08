using System;
using Newtonsoft.Json;

namespace OpenHellion.Networking.Message.MainServer
{
	[Serializable]
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class GetPlayerIdRequest : MSMessage
	{
		public string SteamId;
		public string DiscordId;

		public override string GetDestination()
		{
			return "getPlayerId";
		}
	}
}
