using UnityEngine;
using ZeroGravity.Data;

namespace ZeroGravity.ShipComponents
{
	public class SubSystemGeneric : SubSystem
	{
		[SerializeField]
		private ResourceRequirement[] _ResourceRequirements = new ResourceRequirement[1]
		{
			new ResourceRequirement
			{
				ResourceType = DistributionSystemType.Power
			}
		};

		public override SubSystemType Type
		{
			get
			{
				return SubSystemType.Generic;
			}
		}

		public override ResourceRequirement[] ResourceRequirements
		{
			get
			{
				return _ResourceRequirements;
			}
		}
	}
}
