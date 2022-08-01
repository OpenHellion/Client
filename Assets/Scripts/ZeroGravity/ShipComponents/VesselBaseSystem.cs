using System;
using System.Linq;
using System.Runtime.CompilerServices;
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

		[CompilerGenerated]
		private static Func<ResourceRequirement, bool> _003C_003Ef__am_0024cache0;

		public override ResourceRequirement[] ResourceRequirements
		{
			get
			{
				return _ResourceRequirements;
			}
		}

		public override SubSystemType Type
		{
			get
			{
				return SubSystemType.VesselBasePowerConsumer;
			}
		}

		public override SystemAuxData GetAuxData()
		{
			if (ResourceRequirements.Length == 1)
			{
				ResourceRequirement[] resourceRequirements = ResourceRequirements;
				if (_003C_003Ef__am_0024cache0 == null)
				{
					_003C_003Ef__am_0024cache0 = _003CGetAuxData_003Em__0;
				}
				if (resourceRequirements.Count(_003C_003Ef__am_0024cache0) == 1)
				{
					VesselBaseSystemAuxData vesselBaseSystemAuxData = new VesselBaseSystemAuxData();
					vesselBaseSystemAuxData.DebrisFieldDamageMultiplier = DebrisFieldDamageMultiplier;
					vesselBaseSystemAuxData.DecayDamageMultiplier = DecayDamageMultiplier;
					return vesselBaseSystemAuxData;
				}
			}
			throw new Exception("Invalid ResourceRequirements");
		}

		[CompilerGenerated]
		private static bool _003CGetAuxData_003Em__0(ResourceRequirement m)
		{
			return m.ResourceType == DistributionSystemType.Power;
		}
	}
}
