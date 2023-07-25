using System;
using Newtonsoft.Json;

namespace OpenHellion.Social.NakamaRpc
{
	[Serializable]
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class FindMatchesRequest
	{
		public string Version;
		public string Location;
		public uint Hash;
	}
}
