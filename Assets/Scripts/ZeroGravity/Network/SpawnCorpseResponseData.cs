using ProtoBuf;
using ZeroGravity.Objects;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class SpawnCorpseResponseData : SpawnObjectResponseData
	{
		public CorpseDetails Details;

		public override SpaceObjectType Type
		{
			get { return SpaceObjectType.Corpse; }
			set { }
		}
	}
}
