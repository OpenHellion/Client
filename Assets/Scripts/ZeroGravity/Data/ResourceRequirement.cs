using System;
using UnityEngine.Serialization;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.Data
{
	[Serializable]
	public class ResourceRequirement : ISceneData
	{
		public DistributionSystemType ResourceType;

		[FormerlySerializedAs("Requirement")] public float Nominal;

		public float Standby;
	}
}
