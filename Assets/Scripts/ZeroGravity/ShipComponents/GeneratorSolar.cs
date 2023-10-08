using System;
using UnityEngine;
using UnityEngine.Serialization;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;

namespace ZeroGravity.ShipComponents
{
	public class GeneratorSolar : Generator, IPowerProvider
	{
		public float Efficiency = 1f;

		public double ThresholdDistanceMax = 300000000000.0;

		[SerializeField] private ResourceRequirement[] _ResourceRequirements = new ResourceRequirement[0];

		[FormerlySerializedAs("DeployRetractExecuter")] [Space(10f)]
		public SceneTriggerExecutor DeployRetractExecutor;

		public string DeplayState = "deploy";

		public string RetractState = "retract";

		[NonSerialized] public float ExposureToSunlight;

		public override GeneratorType Type
		{
			get { return GeneratorType.Solar; }
		}

		public override ResourceRequirement[] ResourceRequirements
		{
			get { return _ResourceRequirements; }
		}

		public override void SetDetails(GeneratorDetails details, bool instant = false)
		{
			SystemStatus status = Status;
			base.SetDetails(details, instant);
			if (DeployRetractExecutor != null && status != Status)
			{
				if (Status == SystemStatus.Powerup || Status == SystemStatus.Online)
				{
					DeployRetractExecutor.ChangeState(DeplayState);
				}
				else if (Status == SystemStatus.Cooldown || Status == SystemStatus.Offline)
				{
					DeployRetractExecutor.ChangeState(RetractState);
				}
			}
		}

		public override void SetAuxDetails(IAuxDetails auxDetails)
		{
			base.SetAuxDetails(auxDetails);
			ExposureToSunlight = (auxDetails as GeneratorSolarAuxDetails).ExposureToSunlight;
		}

		public override SystemAuxData GetAuxData()
		{
			GeneratorSolarAuxData generatorSolarAuxData = new GeneratorSolarAuxData();
			generatorSolarAuxData.Efficiency = Efficiency;
			return generatorSolarAuxData;
		}
	}
}
