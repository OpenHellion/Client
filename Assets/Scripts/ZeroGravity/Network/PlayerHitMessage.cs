using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class PlayerHitMessage : NetworkData
	{
		public int HitIndentifier;

		public bool HitSuccessfull;
	}
}
