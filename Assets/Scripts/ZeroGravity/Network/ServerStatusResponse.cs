using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class ServerStatusResponse : NetworkData
	{
		public OldResponseResult Response = OldResponseResult.Success;

		public short CurrentPlayers;

		public short AlivePlayers;

		public short MaxPlayers;

		public CharacterData CharacterData;

		public string Description;

		public string NotificationEMail;
	}
}
