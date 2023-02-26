using System;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;

namespace ZeroGravity.ShipComponents
{
	public class GeneratorCapacitor : Generator, IPowerConsumer, IPowerProvider
	{
		[SerializeField]
		private ResourceRequirement[] _ResourceRequirements = new ResourceRequirement[1]
		{
			new ResourceRequirement
			{
				ResourceType = DistributionSystemType.Power
			}
		};

		public float NominalCapacity;

		public float MaxCapacity;

		public float Capacity;

		public float CapacityChangeRate;

		public override GeneratorType Type
		{
			get
			{
				return GeneratorType.Capacitor;
			}
		}

		public override ResourceRequirement[] ResourceRequirements
		{
			get
			{
				return _ResourceRequirements;
			}
		}

		public override SystemAuxData GetAuxData()
		{
			if (Capacity < 0f || Capacity > NominalCapacity)
			{
				throw new Exception("Invalid Capacity");
			}
			if (NominalCapacity <= 0f)
			{
				throw new Exception("Invalid NominalCapacity");
			}
			GeneratorCapacitorAuxData generatorCapacitorAuxData = new GeneratorCapacitorAuxData();
			generatorCapacitorAuxData.NominalCapacity = NominalCapacity;
			generatorCapacitorAuxData.Capacity = Capacity;
			return generatorCapacitorAuxData;
		}

		public override void SetAuxDetails(IAuxDetails auxDetails)
		{
			base.SetAuxDetails(auxDetails);
			GeneratorCapacitorAuxDetails generatorCapacitorAuxDetails = auxDetails as GeneratorCapacitorAuxDetails;
			Capacity = generatorCapacitorAuxDetails.Capacity;
			MaxCapacity = generatorCapacitorAuxDetails.MaxCapacity;
			CapacityChangeRate = generatorCapacitorAuxDetails.CapacityChangeRate;
		}

		public override float GetConsumption(DistributionSystemType resourceType)
		{
			if (Status != SystemStatus.Online)
			{
				return 0f;
			}
			ResourceRequirement[] resourceRequirements = ResourceRequirements;
			foreach (ResourceRequirement resourceRequirement in resourceRequirements)
			{
				if (resourceRequirement.ResourceType == resourceType)
				{
					return resourceRequirement.Nominal * InputFactor + resourceRequirement.Standby * InputFactorStandby;
				}
			}
			return 0f;
		}
	}
}
