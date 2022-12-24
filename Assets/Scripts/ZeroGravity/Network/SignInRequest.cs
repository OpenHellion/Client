using ProtoBuf;

namespace ZeroGravity.Network
{
	/// <summary>
	/// 	A sign in request normally sent to the main server. Server responds with a <c>SignInResponse</c>.
	/// </summary>
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class SignInRequest : NetworkData
	{
		public string SteamId;

		public string ClientVersion;

		public uint ClientHash;
	}
}
