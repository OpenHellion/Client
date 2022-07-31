using System.Collections.Generic;
using ProtoBuf;
using ZeroGravity.Objects;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class CharacterMovementMessage : NetworkData
	{
		public CharacterTransformData TransformData;

		public CharacterAnimationData AnimationData;

		public long ParentGUID = -1L;

		public SpaceObjectType ParentType;

		public long GUID;

		public float[] Gravity;

		public Dictionary<byte, RagdollItemData> RagdollData;

		public sbyte[] JetpackDirection;

		public bool PivotReset;

		public long NearestVesselGUID;

		public float NearestVesselDistance;

		public bool StickToVessel;

		public float[] PivotPositionCorrection;

		public float[] PivotVelocityCorrection;

		public float? ImpactVelocity;

		public bool? DockUndockMsg;
	}
}
