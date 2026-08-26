using UnityEngine;
using ZeroGravity.Data;

namespace ZeroGravity.ShipComponents
{
	public class GeneratorPower : Generator, IPowerProvider
	{
		public float ResponseTime;

		[SerializeField] private ResourceRequirement[] _ResourceRequirements = new ResourceRequirement[1]
		{
			new ResourceRequirement
			{
				ResourceType = DistributionSystemType.Power
			}
		};

		public override GeneratorType Type
		{
			get { return GeneratorType.Power; }
		}

		public override ResourceRequirement[] ResourceRequirements
		{
			get { return _ResourceRequirements; }
		}

		public override SystemAuxData GetAuxData()
		{
			GeneratorPowerAuxData generatorPowerAuxData = new GeneratorPowerAuxData();
			generatorPowerAuxData.ResponseTime = ResponseTime;
			return generatorPowerAuxData;
		}
	}
}
