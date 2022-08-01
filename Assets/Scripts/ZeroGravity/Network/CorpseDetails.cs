using System.Collections.Generic;
using ProtoBuf;
using ZeroGravity.Objects;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class CorpseDetails
	{
		public long GUID;

		public float[] LocalPosition;

		public float[] LocalRotation;

		public bool IsInsideSpaceObject;

		public Dictionary<byte, RagdollItemData> RagdollDataList;

		public List<DynamicObjectDetails> DynamicObjectData;

		public long ParentGUID = -1L;

		public SpaceObjectType ParentType;

		public Gender Gender;
	}
}
