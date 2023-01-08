using System;
using Newtonsoft.Json;

namespace OpenHellion.Networking.Message.MainServer
{
	[Serializable]
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class CreatePlayerRequest : MSMessage
	{
		public string Name;
		public Region Region;
		public string SteamId;
		public string DiscordId;

		public override string GetDestination()
		{
			return "createPlayer";
		}
	}
}
