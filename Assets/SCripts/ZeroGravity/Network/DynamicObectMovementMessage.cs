using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class DynamicObectMovementMessage : NetworkData
	{
		public long GUID;

		public float[] LocalPosition;

		public float[] LocalRotation;

		public float[] Velocity;

		public float[] AngularVelocity;

		public float ImpactVelocity;

		public float Timestamp;
	}
}
