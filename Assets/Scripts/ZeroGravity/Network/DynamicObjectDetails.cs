using System.Collections.Generic;
using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class DynamicObjectDetails
	{
		public long GUID;

		public short ItemID;

		public float[] LocalPosition;

		public float[] LocalRotation;

		public float[] Velocity;

		public float[] AngularVelocity;

		public DynamicObjectAttachData AttachData;

		public DynamicObjectStats StatsData;

		public List<DynamicObjectDetails> ChildObjects;
	}
}
