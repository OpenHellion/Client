using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;

namespace ZeroGravity.ShipComponents
{
	public class SubSystemEngine : SubSystem, IPowerConsumer
	{
		[SerializeField]
		private ResourceRequirement[] _ResourceRequirements = new ResourceRequirement[2]
		{
			new ResourceRequirement
			{
				ResourceType = DistributionSystemType.Hydrogen
			},
			new ResourceRequirement
			{
				ResourceType = DistributionSystemType.Power
			}
		};

		public float Acceleration = 1200f;

		public float ReverseAcceleration = 1000f;

		public float AccelerationBuildup = 2f;

		public override SubSystemType Type
		{
			get
			{
				return SubSystemType.Engine;
			}
		}

		public override ResourceRequirement[] ResourceRequirements
		{
			get
			{
				return _ResourceRequirements;
			}
		}

		public bool ThrustActive
		{
			get
			{
				return Status == SystemStatus.OnLine && OperationRate > 0f;
			}
		}

		public override SystemAuxData GetAuxData()
		{
			SubSystemEngineAuxData subSystemEngineAuxData = new SubSystemEngineAuxData();
			subSystemEngineAuxData.Acceleration = Acceleration;
			subSystemEngineAuxData.ReverseAcceleration = ReverseAcceleration;
			subSystemEngineAuxData.AccelerationBuildup = AccelerationBuildup;
			return subSystemEngineAuxData;
		}
	}
}
