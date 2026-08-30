using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class ShipStatsMessage : NetworkData
	{
		public long Guid;

		public float[] Thrust;

		public float[] Rotation;

		public float[] AutoStabilize;

		public float? EngineThrustPercentage;

		public VesselObjects VesselObjects;

		public RcsThrustStats ThrustStats;

		public float? Temperature;

		public float? Health;

		public float? Armor;

		public float? SelfDestructTime;

		public long? TargetStabilizationGuid;
	}
}
