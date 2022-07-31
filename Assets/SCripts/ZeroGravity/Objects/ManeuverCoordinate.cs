using System;
using UnityEngine;

namespace ZeroGravity.Objects
{
	public class ManeuverCoordinate
	{
		public static long NextGUID = 1L;

		public long GUID;

		public bool CanBeChanged = true;

		public CelestialBody TargetCel;

		public SpaceObjectVessel TargetVessel;

		public string Name;

		public float LongitudeOfAscendingNode;

		public float ArgumentOfPeriapsis;

		public float InclinationAngle;

		public float PeriapsisHeight;

		public float ApoapsisHeight;

		public float PositionOnOrbit;

		public float TrueAnomaly;

		public bool IsArtificialBody;

		public Transform OrbitPlane;

		public static void MovePositionOnOrbitByTime(ref ManeuverCoordinate coordinate, double time)
		{
			double periapsisDistance = coordinate.PeriapsisHeight;
			double apoapsisDistance = coordinate.ApoapsisHeight;
			double inclination = (double)coordinate.InclinationAngle % 360.0;
			double argumentOfPeriapsis = (double)coordinate.ArgumentOfPeriapsis % 360.0;
			double longitudeOfAscendingNode = (double)coordinate.LongitudeOfAscendingNode % 360.0;
			double num = (double)coordinate.LongitudeOfAscendingNode % 360.0;
			double trueAnomalyAngleDeg = (double)coordinate.PositionOnOrbit % 360.0;
			OrbitParameters orbitParameters = new OrbitParameters();
			orbitParameters.InitFromPeriapisAndApoapsis(coordinate.TargetCel.Orbit, periapsisDistance, apoapsisDistance, inclination, argumentOfPeriapsis, longitudeOfAscendingNode, trueAnomalyAngleDeg, 0.0);
			coordinate.PositionOnOrbit = (float)(orbitParameters.TrueAnomalyAtZeroTimeFromCurrent(time) * (180.0 / System.Math.PI));
		}
	}
}
