using ZeroGravity.Math;

namespace ZeroGravity.Objects
{
	public interface IMapMainObject
	{
		long GUID { get; }

		CelestialBody ParentCelesitalBody { get; }

		double Radius { get; }

		Vector3D Position { get; }

		string Name { get; }

		OrbitParameters Orbit { get; }

		RadarVisibilityType RadarVisibilityType { get; }
	}
}
