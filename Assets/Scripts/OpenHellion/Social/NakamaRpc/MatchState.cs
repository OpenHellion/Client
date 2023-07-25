using System;
using Newtonsoft.Json;

namespace OpenHellion.Social.NakamaRpc
{
	[Serializable]
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class MatchState
	{
		public string Id;
		public string Ip;
		public uint GamePort;
		public uint StatusPort;
	}
}
