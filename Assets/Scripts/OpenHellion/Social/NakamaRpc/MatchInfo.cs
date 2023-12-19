using System;
using Newtonsoft.Json;

namespace OpenHellion.Social.NakamaRpc
{
	[Serializable]
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class MatchInfo
	{
		[JsonProperty("location")]
		public string Location;
		[JsonProperty("ip")]
		public string Ip;
		[JsonProperty("gamePort")]
		public int GamePort;
		[JsonProperty("statusPort")]
		public int StatusPort;
	}
}
