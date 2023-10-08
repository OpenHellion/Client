using System;
using Newtonsoft.Json;

namespace OpenHellion.Social.NakamaRpc
{
	[Serializable]
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class MatchInfo
	{
		public string Id;
		public string Ip;
		public int GamePort;
		public int StatusPort;
	}
}
