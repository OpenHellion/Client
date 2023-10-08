using ProtoBuf;
using ZeroGravity.Objects;

namespace ZeroGravity.Network
{
	[ProtoInclude(4000, typeof(SpawnShipResponseData))]
	[ProtoInclude(4001, typeof(SpawnAsteroidResponseData))]
	[ProtoInclude(4002, typeof(SpawnDynamicObjectResponseData))]
	[ProtoInclude(4003, typeof(SpawnCorpseResponseData))]
	[ProtoInclude(4004, typeof(SpawnCharacterResponseData))]
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public abstract class SpawnObjectResponseData
	{
		public long GUID;

		public virtual SpaceObjectType Type
		{
			get { return SpaceObjectType.None; }
			set { }
		}
	}
}
