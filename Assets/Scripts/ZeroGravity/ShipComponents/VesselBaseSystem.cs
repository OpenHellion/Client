using System;
using System.Linq;
using UnityEngine;
using ZeroGravity.Data;

namespace ZeroGravity.ShipComponents
{
	public class VesselBaseSystem : SubSystem, IPowerConsumer
	{
		[SerializeField]
		private ResourceRequirement[] _ResourceRequirements = new ResourceRequirement[1]
		{
			new ResourceRequirement
			{
				ResourceType = DistributionSystemType.Power,
				Nominal = 100f,
				Standby = 100f
			}
		};

		public float DecayDamageMultiplier = 0.1f;

		public float DebrisFieldDamageMultiplier = 0.5f;

		public override ResourceRequirement[] ResourceRequirements => _ResourceRequirements;

		public override SubSystemType Type => SubSystemType.VesselBasePowerConsumer;

		public override SystemAuxData GetAuxData()
		{
			if (ResourceRequirements.Length != 1 || ResourceRequirements.Count((ResourceRequirement m) => m.ResourceType == DistributionSystemType.Power) != 1)
			{
				throw new Exception("Invalid ResourceRequirements");
			}

			return new VesselBaseSystemAuxData
			{
				DebrisFieldDamageMultiplier = DebrisFieldDamageMultiplier,
				DecayDamageMultiplier = DecayDamageMultiplier
			};
		}
	}
}
