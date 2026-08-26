using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class LogInRequest : NetworkData
	{
		public string ServerID;

		public uint ClientHash;

		public string PlayerId;

		public CharacterData CharacterData;
	}
}
