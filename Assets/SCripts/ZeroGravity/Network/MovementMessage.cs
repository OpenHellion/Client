using System.Collections.Generic;
using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class MovementMessage : NetworkData
	{
		public double SolarSystemTime;

		public float Timestamp;

		public List<ObjectTransform> Transforms;
	}
}
