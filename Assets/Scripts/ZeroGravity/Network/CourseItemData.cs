using System.Collections.Generic;
using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class CourseItemData
	{
		public long GUID;

		public ManeuverType Type;

		public float StartOrbitAngle;

		public float EndOrbitAngle;

		public double StartSolarSystemTime;

		public double EndSolarSystemTime;

		public double TravelTime;

		public int WarpIndex;

		public List<int> WarpCells;

		public OrbitData StartOrbit;

		public OrbitData EndOrbit;
	}
}
