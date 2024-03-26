using System.Collections.Generic;
using ProtoBuf;
using ZeroGravity.Objects;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class SpawnAsteroidResponseData : SpawnObjectResponseData
	{
		public VesselData Data;

		public double Radius;

		public bool IsDummy;

		public List<AsteroidMiningPointDetails> MiningPoints;

		public override SpaceObjectType Type
		{
			get => SpaceObjectType.Asteroid;
			set { }
		}
	}
}
