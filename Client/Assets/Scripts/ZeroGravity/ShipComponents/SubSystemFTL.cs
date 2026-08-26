using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;

namespace ZeroGravity.ShipComponents
{
	public class SubSystemFTL : SubSystem, IPowerConsumer
	{
		[SerializeField] private ResourceRequirement[] _ResourceRequirements = new ResourceRequirement[1]
		{
			new ResourceRequirement
			{
				ResourceType = DistributionSystemType.Power
			}
		};

		[Tooltip("Towable mass in tons")] public float BaseTowingCapacity;

		public WarpData[] WarpsData;

		public List<SoundEffect> WarpSounds;

		public int MaxWarp;

		public float TowingCapacity;

		public override SubSystemType Type
		{
			get { return SubSystemType.FTL; }
		}

		public override ResourceRequirement[] ResourceRequirements
		{
			get { return _ResourceRequirements; }
		}

		public Dictionary<int, float?> WarpCellsFuel { get; private set; }

		public override SystemAuxData GetAuxData()
		{
			if (WarpsData.Length == 0)
			{
				throw new Exception("At least 1 Warp must be defined.");
			}

			SubSystemFTLAuxData subSystemFTLAuxData = new SubSystemFTLAuxData();
			subSystemFTLAuxData.BaseTowingCapacity = BaseTowingCapacity;
			subSystemFTLAuxData.WarpsData = WarpsData;
			return subSystemFTLAuxData;
		}

		public override void SetAuxDetails(IAuxDetails auxDetails)
		{
			base.SetAuxDetails(auxDetails);
			FTLAuxDetails fTLAuxDetails = auxDetails as FTLAuxDetails;
			WarpCellsFuel = fTLAuxDetails.WarpCellsFuel;
			MaxWarp = fTLAuxDetails.MaxWarp;
			TowingCapacity = fTLAuxDetails.TowingCapacity;
		}
	}
}
