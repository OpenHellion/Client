using System;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;

namespace ZeroGravity.ShipComponents
{
	public class SubSystemRCS : SubSystem
	{
		[SerializeField] private ResourceRequirement[] _ResourceRequirements = new ResourceRequirement[1]
		{
			new ResourceRequirement
			{
				ResourceType = DistributionSystemType.RCS
			}
		};

		public float Acceleration = 2f;

		public float RotationAcceleration = 10f;

		public float RotationStabilization = 9f;

		public float MaxOperationRate = 1f;

		public override SubSystemType Type => SubSystemType.RCS;

		public override ResourceRequirement[] ResourceRequirements => _ResourceRequirements;

		public override SystemAuxData GetAuxData()
		{
			SubSystemRCSAuxData subSystemRCSAuxData = new SubSystemRCSAuxData();
			subSystemRCSAuxData.Acceleration = Acceleration;
			subSystemRCSAuxData.RotationAcceleration = RotationAcceleration;
			subSystemRCSAuxData.RotationStabilization = RotationStabilization;
			return subSystemRCSAuxData;
		}

		public override void SetAuxDetails(IAuxDetails auxDetails)
		{
			MaxOperationRate = (auxDetails as RCSAuxDetails).MaxOperationRate;
		}

		public bool CanRotate()
		{
			return hasPropellant() && RotationAcceleration > 0f;
		}

		public bool CanThrust()
		{
			return hasPropellant() && Acceleration > 0f;
		}

		private bool hasPropellant()
		{
			try
			{
				float nominal = Array.Find(ResourceRequirements,
					(ResourceRequirement m) => m.ResourceType == DistributionSystemType.RCS).Nominal;
				return ResourceContainers[0].Compartments[0].Resources[0].Quantity > nominal;
			}
			catch
			{
			}

			return false;
		}
	}
}
