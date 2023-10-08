using UnityEngine;
using ZeroGravity.Data;

namespace ZeroGravity.ShipComponents
{
	public class GeneratorScrubbedAir : Generator, IPowerConsumer, ILifeProvider
	{
		[Tooltip("Amount of HP used to fully scrub 1m3 of air")]
		public float ScrubberCartridgeConsumption;

		[SerializeField] private ResourceRequirement[] _ResourceRequirements = new ResourceRequirement[1]
		{
			new ResourceRequirement
			{
				ResourceType = DistributionSystemType.Power
			}
		};

		public override GeneratorType Type
		{
			get { return GeneratorType.AirScrubber; }
		}

		public override ResourceRequirement[] ResourceRequirements
		{
			get { return _ResourceRequirements; }
		}

		public override SystemAuxData GetAuxData()
		{
			GeneratorScrubbedAirAuxData generatorScrubbedAirAuxData = new GeneratorScrubbedAirAuxData();
			generatorScrubbedAirAuxData.ScrubberCartridgeConsumption = ScrubberCartridgeConsumption;
			return generatorScrubbedAirAuxData;
		}
	}
}
