using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;

namespace ZeroGravity.ShipComponents
{
	public class SubSystemRCS : SubSystem
	{
		[SerializeField]
		private ResourceRequirement[] _ResourceRequirements = new ResourceRequirement[1]
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

		[CompilerGenerated]
		private static Predicate<ResourceRequirement> _003C_003Ef__am_0024cache0;

		public override SubSystemType Type
		{
			get
			{
				return SubSystemType.RCS;
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
				ResourceRequirement[] resourceRequirements = ResourceRequirements;
				if (_003C_003Ef__am_0024cache0 == null)
				{
					_003C_003Ef__am_0024cache0 = _003ChasPropellant_003Em__0;
				}
				float nominal = Array.Find(resourceRequirements, _003C_003Ef__am_0024cache0).Nominal;
				return ResourceContainers[0].Compartments[0].Resources[0].Quantity > nominal;
			}
			catch
			{
			}
			return false;
		}

		[CompilerGenerated]
		private static bool _003ChasPropellant_003Em__0(ResourceRequirement m)
		{
			return m.ResourceType == DistributionSystemType.RCS;
		}
	}
}
