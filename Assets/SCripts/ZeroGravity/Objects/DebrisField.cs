using System;
using ZeroGravity.Math;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class DebrisField : IMapMainObject
	{
		public DebrisFieldType Type;

		public float FragmentsDensity;

		public float FragmentsVelocity;

		private int steps;

		private int maxSteps = 100;

		private double radiusSq;

		public float ScanningSensitivityMultiplier;

		public float RadarSignatureMultiplier;

		public string Name { get; set; }

		public double Radius { get; set; }

		public OrbitParameters Orbit { get; set; }

		public RadarVisibilityType RadarVisibilityType
		{
			get
			{
				return RadarVisibilityType.Visible;
			}
		}

		public long GUID { get; set; }

		public CelestialBody ParentCelesitalBody
		{
			get
			{
				return Orbit.Parent.CelestialBody;
			}
			set
			{
			}
		}

		public Vector3D Position
		{
			get
			{
				return Orbit.Position;
			}
		}

		public bool IsDummyObject
		{
			get
			{
				return false;
			}
		}

		public DebrisField(DebrisFieldDetails data)
		{
			Orbit = new OrbitParameters();
			Orbit.ParseNetworkData(data.Orbit);
			Name = data.Name;
			Type = data.Type;
			Radius = data.Radius;
			FragmentsDensity = data.FragmentsDensity;
			FragmentsVelocity = data.FragmentsVelocity;
			ScanningSensitivityMultiplier = data.ScanningSensitivityMultiplier;
			RadarSignatureMultiplier = data.RadarSignatureMultiplier;
			radiusSq = Radius * Radius;
			steps = (int)MathHelper.Clamp(System.Math.Ceiling(Orbit.Circumference / Radius / 2.0), 1.0, maxSteps);
		}

		public bool CheckObject(ArtificialBody ab)
		{
			Vector3D orbitVelocity;
			return CheckObject(ab, out orbitVelocity);
		}

		public bool CheckObject(ArtificialBody ab, out Vector3D orbitVelocity)
		{
			orbitVelocity = Vector3D.Zero;
			if (ab.Orbit.Parent != Orbit.Parent)
			{
				return false;
			}
			QuaternionD rotation;
			Vector3D centerPosition;
			Orbit.GetOrbitPlaneData(out rotation, out centerPosition);
			Vector3D vector3D = ab.Position - (Orbit.Parent.Position + centerPosition);
			Vector3D vector3D2 = Vector3D.ProjectOnPlane(vector3D, rotation * Vector3D.Up);
			Vector3D vector3D3 = vector3D2.Normalized * Orbit.SemiMajorAxis;
			if ((vector3D3 - vector3D).SqrMagnitude <= radiusSq)
			{
				orbitVelocity = Orbit.VelocityAtEccentricAnomaly(Vector3D.Angle(rotation * Vector3D.Forward, vector3D2.Normalized) * (System.Math.PI / 180.0), false).Normalized;
				return true;
			}
			return false;
		}
	}
}
