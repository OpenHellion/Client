using ProtoBuf;
using ZeroGravity.Objects;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class CorpseStatsMessage : NetworkData
	{
		public long GUID;

		public bool DestroyCorpse;

		public long ParentGUID = -1L;

		public SpaceObjectType ParentType;

		public float[] LocalPosition;

		public float[] LocalRotation;
	}
}
