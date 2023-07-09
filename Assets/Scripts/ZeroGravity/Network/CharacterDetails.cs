using System.Collections.Generic;
using ProtoBuf;
using ZeroGravity.Objects;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class CharacterDetails
	{
		public long ParentID;

		public SpaceObjectType ParentType;

		public Gender Gender;

		public byte HeadType;

		public byte HairType;

		public long GUID;

		public string Name;

		public string PlayerId;

		public CharacterTransformData TransformData;

		public int SpawnPointID;

		public List<DynamicObjectDetails> DynamicObjects;

		public int AnimationStatsMask;

		public VesselObjectID LockedToTriggerID;
	}
}
