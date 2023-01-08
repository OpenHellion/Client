using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class DeleteCharacterResponse : NetworkData
	{
		public OldResponseResult Response = OldResponseResult.Success;

		public string Message = string.Empty;
	}
}
