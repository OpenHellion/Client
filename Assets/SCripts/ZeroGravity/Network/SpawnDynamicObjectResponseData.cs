using ProtoBuf;
using ZeroGravity.Objects;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class SpawnDynamicObjectResponseData : SpawnObjectResponseData
	{
		public DynamicObjectDetails Details;

		public override SpaceObjectType Type
		{
			get
			{
				return SpaceObjectType.DynamicObject;
			}
			set
			{
			}
		}
	}
}
