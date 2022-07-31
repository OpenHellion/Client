using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class SignInRequest : NetworkData
	{
		public string SteamId;

		public string ClientVersion;

		public uint ClientHash;
	}
}
