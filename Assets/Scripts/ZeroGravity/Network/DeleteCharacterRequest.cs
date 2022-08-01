using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class DeleteCharacterRequest : NetworkData
	{
		public string SteamId;

		public long ServerId;
	}
}
