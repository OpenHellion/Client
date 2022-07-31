using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class PlayerOnServerData
	{
		public string SteamID;

		public string Name;

		public bool AlreadyHasInvite;
	}
}
