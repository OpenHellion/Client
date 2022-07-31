using ProtoBuf;
using ZeroGravity.Objects;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class SpawnCharacterResponseData : SpawnObjectResponseData
	{
		public CharacterDetails Details;

		public override SpaceObjectType Type
		{
			get
			{
				return SpaceObjectType.Player;
			}
			set
			{
			}
		}
	}
}
