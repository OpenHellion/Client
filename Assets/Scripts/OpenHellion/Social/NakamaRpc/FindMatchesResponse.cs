using System;
using Newtonsoft.Json;

namespace OpenHellion.Social.NakamaRpc
{
	[Serializable]
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class FindMatchesResponse
	{
		public string[] MatchesId;
	}
}
