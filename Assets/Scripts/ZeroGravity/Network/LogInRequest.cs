using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class LogInRequest : NetworkData
	{
		public string ServerID;

		public string Password;

		public uint ClientHash;

		public string PlayerId;

		public string NativeId;

		public CharacterData CharacterData;
	}
}
