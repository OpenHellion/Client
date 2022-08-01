using System.Collections.Generic;
using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class CorpseMovementMessage : NetworkData
	{
		public long GUID;

		public float[] LocalPosition;

		public float[] LocalRotation;

		public float[] Velocity;

		public float[] AngularVelocity;

		public float Timestamp;

		public bool IsInsideSpaceObject;

		public Dictionary<byte, RagdollItemData> RagdollDataList;
	}
}
