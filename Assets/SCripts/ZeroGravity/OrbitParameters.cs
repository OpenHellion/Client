using System;
using System.Collections.Generic;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity
{
	public class OrbitParameters
	{
		public const double AstronomicalUnitLength = 149597870700.0;

		public const double GravitationalConstant = 6.67384E-11;

		public const double MaxObjectDistance = 897587224200.0;

		public bool PrintDetails;

		private double mass;

		private double radius;

		private double rotationPeriod = 86400.0;

		private double gravParameter;

		private double gravityInfluenceRadius;

		private double gravityInfluenceRadiusSquared;

		private OrbitParameters parent;

		private CelestialBody celestialBodyObj;

		private ArtificialBody artificialBodyObj;

		private double eccentricity;

		private double semiMajorAxis;

		private double semiMinorAxis;

		private double inclination;

		private double argumentOfPeriapsis;

		private double longitudeOfAscendingNode;

		private double orbitalPeriod;

		private double timeSincePeriapsis;

		private double solarSystemTimeAtPeriapsis;

		private double lastChangeTime;

		public Vector3D RelativePosition = Vector3D.Zero;

		public Vector3D RelativeVelocity = Vector3D.Zero;

		private double lastValidTrueAnomaly;

		private double lastValidTimeSincePeriapsis;

		public Vector3D Position
		{
			get
			{
				return (parent == null) ? RelativePosition : (parent.Position + RelativePosition);
			}
		}

		public Vector3D Velocity
		{
			get
			{
				return (parent == null) ? RelativeVelocity : (parent.Velocity + RelativeVelocity);
			}
		}

		public double OrbitalPeriod
		{
			get
			{
				return orbitalPeriod;
			}
		}

		public double Radius
		{
			get
			{
				return radius;
			}
		}

		public double GravParameter
		{
			get
			{
				return gravParameter;
			}
		}

		public double GravityInfluenceRadius
		{
			get
			{
				return gravityInfluenceRadius;
			}
		}

		public double GravityInfluenceRadiusSquared
		{
			get
			{
				return gravityInfluenceRadiusSquared;
			}
		}

		public double LastChangeTime
		{
			get
			{
				return lastChangeTime;
			}
		}

		public OrbitParameters Parent
		{
			get
			{
				return parent;
			}
		}

		public bool IsOrbitValid
		{
			get
			{
				return semiMajorAxis != 0.0 && semiMinorAxis != 0.0;
			}
		}

		public double TimeSincePeriapsis
		{
			get
			{
				return timeSincePeriapsis;
			}
		}

		public double SolarSystemTimeAtPeriapsis
		{
			get
			{
				return solarSystemTimeAtPeriapsis;
			}
		}

		public double Eccentricity
		{
			get
			{
				return eccentricity;
			}
		}

		public CelestialBody CelestialBody
		{
			get
			{
				return celestialBodyObj;
			}
		}

		public ArtificialBody ArtificialBody
		{
			get
			{
				return artificialBodyObj;
			}
		}

		public double LongitudeOfAscendingNode
		{
			get
			{
				return longitudeOfAscendingNode;
			}
		}

		public double ArgumentOfPeriapsis
		{
			get
			{
				return argumentOfPeriapsis;
			}
		}

		public double Inclination
		{
			get
			{
				return inclination;
			}
		}

		public double PeriapsisDistance
		{
			get
			{
				return CalculatePeriapsisDistance(this);
			}
		}

		public double ApoapsisDistance
		{
			get
			{
				return CalculateApoapsisDistance(this);
			}
		}

		public double SemiMajorAxis
		{
			get
			{
				return semiMajorAxis;
			}
		}

		public double SemiMinorAxis
		{
			get
			{
				return semiMinorAxis;
			}
		}

		public double Circumference
		{
			get
			{
				return CalculateCircumference(this);
			}
		}

		public double DistanceAtTrueAnomaly
		{
			get
			{
				return CalculateDistanceAtTrueAnomaly(this, CalculateTrueAnomaly(this, timeSincePeriapsis));
			}
		}

		public long GUID
		{
			get
			{
				if (artificialBodyObj != null)
				{
					return artificialBodyObj.GUID;
				}
				if (celestialBodyObj != null)
				{
					return celestialBodyObj.GUID;
				}
				return 0L;
			}
		}

		public void InitFromElements(OrbitParameters parent, double mass, double radius, double rotationPeriod, double eccentricity, double semiMajorAxis, double inclination, double argumentOfPeriapsis, double longitudeOfAscendingNode, double timeSincePeriapsis, double solarSystemTime)
		{
			FixEccentricity(ref eccentricity);
			this.parent = parent;
			if (mass > 0.0)
			{
				this.mass = mass;
			}
			if (radius > 0.0)
			{
				this.radius = radius;
			}
			if (rotationPeriod > 0.0)
			{
				this.rotationPeriod = rotationPeriod;
			}
			this.eccentricity = eccentricity;
			this.semiMajorAxis = semiMajorAxis;
			this.inclination = inclination;
			this.argumentOfPeriapsis = argumentOfPeriapsis;
			this.longitudeOfAscendingNode = longitudeOfAscendingNode;
			this.timeSincePeriapsis = timeSincePeriapsis;
			solarSystemTimeAtPeriapsis = solarSystemTime - timeSincePeriapsis;
			lastChangeTime = solarSystemTime;
			gravParameter = 6.67384E-11 * this.mass;
			if (parent != null)
			{
				if (eccentricity < 1.0)
				{
					orbitalPeriod = System.Math.PI * 2.0 * System.Math.Sqrt(System.Math.Pow(semiMajorAxis, 3.0) / parent.gravParameter);
				}
				else
				{
					orbitalPeriod = System.Math.PI * 2.0 * System.Math.Sqrt(System.Math.Pow(0.0 - semiMajorAxis, 3.0) / parent.gravParameter);
				}
				while (this.timeSincePeriapsis < 0.0 - orbitalPeriod)
				{
					this.timeSincePeriapsis += orbitalPeriod;
				}
				if (eccentricity < 1.0)
				{
					semiMinorAxis = semiMajorAxis * System.Math.Sqrt(1.0 - eccentricity * eccentricity);
				}
				else
				{
					semiMinorAxis = semiMajorAxis * System.Math.Sqrt(eccentricity * eccentricity - 1.0);
				}
				double num = CalculateTrueAnomaly(this, timeSincePeriapsis);
				RelativePosition = PositionAtTrueAnomaly(num, true);
				RelativeVelocity = VelocityAtTrueAnomaly(num, true);
				gravityInfluenceRadius = semiMajorAxis * (1.0 - eccentricity) * System.Math.Pow(mass / (3.0 * parent.mass), 1.0 / 3.0);
				gravityInfluenceRadiusSquared = gravityInfluenceRadius * gravityInfluenceRadius;
			}
			else
			{
				RelativePosition = Vector3D.Zero;
				RelativeVelocity = Vector3D.Zero;
				orbitalPeriod = 0.0;
				this.timeSincePeriapsis = 0.0;
				this.semiMajorAxis = 0.0;
				semiMinorAxis = 0.0;
				gravityInfluenceRadius = double.PositiveInfinity;
				gravityInfluenceRadiusSquared = double.PositiveInfinity;
			}
		}

		public void InitFromPeriapsis(OrbitParameters parent, double mass, double radius, double rotationPeriod, double eccentricity, double periapsisDistance, double inclination, double argumentOfPeriapsis, double longitudeOfAscendingNode, double timeSincePeriapsis, double solarSystemTime)
		{
			FixEccentricity(ref eccentricity);
			double num = periapsisDistance / (1.0 - eccentricity);
			InitFromElements(parent, mass, radius, rotationPeriod, eccentricity, num, inclination, argumentOfPeriapsis, longitudeOfAscendingNode, timeSincePeriapsis, solarSystemTime);
		}

		public void InitFromPeriapisAndApoapsis(OrbitParameters parent, double periapsisDistance, double apoapsisDistance, double inclination, double argumentOfPeriapsis, double longitudeOfAscendingNode, double trueAnomalyAngleDeg, double solarSystemTime)
		{
			this.parent = parent;
			this.inclination = inclination;
			this.argumentOfPeriapsis = argumentOfPeriapsis;
			this.longitudeOfAscendingNode = longitudeOfAscendingNode;
			semiMajorAxis = (periapsisDistance + apoapsisDistance) / 2.0;
			eccentricity = (apoapsisDistance - periapsisDistance) / (apoapsisDistance + periapsisDistance);
			if (eccentricity < 1.0)
			{
				orbitalPeriod = System.Math.PI * 2.0 * System.Math.Sqrt(System.Math.Pow(semiMajorAxis, 3.0) / parent.gravParameter);
			}
			else
			{
				orbitalPeriod = System.Math.PI * 2.0 * System.Math.Sqrt(System.Math.Pow(0.0 - semiMajorAxis, 3.0) / parent.gravParameter);
			}
			timeSincePeriapsis = CalculateTimeSincePeriapsis(this, CalculateMeanAnomalyFromTrueAnomaly(this, trueAnomalyAngleDeg * (System.Math.PI / 180.0)));
			InitFromElements(parent, 0.0, 0.0, 0.0, eccentricity, semiMajorAxis, inclination, argumentOfPeriapsis, longitudeOfAscendingNode, timeSincePeriapsis, solarSystemTime);
		}

		public void InitFromTrueAnomaly(OrbitParameters parent, double periapsisDistance, double apoapsisDistance, double inclination, double argumentOfPeriapsis, double longitudeOfAscendingNode, double trueAnomalyAngleDeg, double solarSystemTime)
		{
			this.parent = parent;
			this.inclination = inclination;
			this.argumentOfPeriapsis = argumentOfPeriapsis;
			this.longitudeOfAscendingNode = longitudeOfAscendingNode;
			semiMajorAxis = (periapsisDistance + apoapsisDistance) / 2.0;
			eccentricity = (apoapsisDistance - periapsisDistance) / (apoapsisDistance + periapsisDistance);
			if (eccentricity < 1.0)
			{
				orbitalPeriod = System.Math.PI * 2.0 * System.Math.Sqrt(System.Math.Pow(semiMajorAxis, 3.0) / parent.gravParameter);
			}
			else
			{
				orbitalPeriod = System.Math.PI * 2.0 * System.Math.Sqrt(System.Math.Pow(0.0 - semiMajorAxis, 3.0) / parent.gravParameter);
			}
			timeSincePeriapsis = CalculateTimeSincePeriapsis(this, CalculateMeanAnomalyFromTrueAnomaly(this, trueAnomalyAngleDeg * (System.Math.PI / 180.0))) + Client.Instance.SolarSystem.CurrentTime;
			InitFromElements(parent, 0.0, 0.0, 0.0, eccentricity, semiMajorAxis, inclination, argumentOfPeriapsis, longitudeOfAscendingNode, timeSincePeriapsis, solarSystemTime);
		}

		public void InitFromStateVectors(OrbitParameters parent, Vector3D position, Vector3D velocity, double solarSystemTime, bool areValuesRelative)
		{
			if (parent == null)
			{
				throw new Exception("Parent object cannot be null only sun has no parent.");
			}
			if (parent.gravParameter == 0.0)
			{
				throw new Exception("Parent object grav parameter is not set.");
			}
			this.parent = parent;
			RelativePosition = position;
			RelativeVelocity = velocity;
			if (!areValuesRelative)
			{
				RelativePosition -= parent.Position;
				RelativeVelocity -= parent.Velocity;
			}
			double magnitude = RelativePosition.Magnitude;
			Vector3D rhs = Vector3D.Cross(RelativePosition, RelativeVelocity);
			Vector3D right = Vector3D.Right;
			if (rhs.SqrMagnitude.IsEpsilonEqualD(0.0))
			{
				inclination = 180.0 - System.Math.Acos(RelativePosition.Y / magnitude) * (180.0 / System.Math.PI);
				right = Vector3D.Cross(RelativePosition, Vector3D.Up);
				if (right.SqrMagnitude.IsEpsilonEqualD(0.0))
				{
					right = Vector3D.Right;
				}
			}
			else
			{
				inclination = 180.0 - System.Math.Acos(rhs.Y / rhs.Magnitude) * (180.0 / System.Math.PI);
				right = Vector3D.Cross(Vector3D.Up, rhs);
			}
			double magnitude2 = right.Magnitude;
			Vector3D rhs2 = Vector3D.Cross(RelativeVelocity, rhs) / parent.gravParameter - RelativePosition / magnitude;
			eccentricity = rhs2.Magnitude;
			FixEccentricity(ref eccentricity);
			double num = RelativeVelocity.SqrMagnitude / 2.0 - parent.gravParameter / magnitude;
			if (eccentricity < 1.0)
			{
				semiMajorAxis = (0.0 - parent.gravParameter) / (2.0 * num);
				semiMinorAxis = semiMajorAxis * System.Math.Sqrt(1.0 - eccentricity * eccentricity);
			}
			else
			{
				semiMajorAxis = (0.0 - rhs.SqrMagnitude / parent.gravParameter) / (eccentricity * eccentricity - 1.0);
				semiMinorAxis = semiMajorAxis * System.Math.Sqrt(eccentricity * eccentricity - 1.0);
			}
			if (magnitude2.IsEpsilonEqualD(0.0))
			{
				longitudeOfAscendingNode = 0.0;
				double num2 = CalculateTrueAnomaly(this, RelativePosition, RelativeVelocity) * (180.0 / System.Math.PI);
				double num3 = 0.0 - MathHelper.AngleSigned(Vector3D.Right, RelativePosition, Vector3D.Up);
				if (num3 < 0.0)
				{
					num3 += 360.0;
				}
				argumentOfPeriapsis = num3 - num2;
			}
			else
			{
				longitudeOfAscendingNode = 180.0 - System.Math.Acos(right.X / magnitude2) * (180.0 / System.Math.PI);
				if (right.Z > 0.0)
				{
					longitudeOfAscendingNode = 360.0 - longitudeOfAscendingNode;
				}
				if (eccentricity.IsEpsilonEqualD(0.0, 1E-10))
				{
					argumentOfPeriapsis = 0.0;
				}
				else
				{
					argumentOfPeriapsis = 180.0 - System.Math.Acos(MathHelper.Clamp(Vector3D.Dot(right, rhs2) / (magnitude2 * eccentricity), -1.0, 1.0)) * (180.0 / System.Math.PI);
				}
				if (rhs2.Y > 0.0 && !argumentOfPeriapsis.IsEpsilonEqualD(0.0))
				{
					argumentOfPeriapsis = 360.0 - argumentOfPeriapsis;
				}
			}
			if (eccentricity < 1.0)
			{
				orbitalPeriod = System.Math.PI * 2.0 * System.Math.Sqrt(System.Math.Pow(semiMajorAxis, 3.0) / parent.gravParameter);
			}
			else
			{
				orbitalPeriod = System.Math.PI * 2.0 * System.Math.Sqrt(System.Math.Pow(0.0 - semiMajorAxis, 3.0) / parent.gravParameter);
			}
			timeSincePeriapsis = CalculateTimeSincePeriapsis(this, RelativePosition, RelativeVelocity);
			solarSystemTimeAtPeriapsis = solarSystemTime - timeSincePeriapsis;
			lastChangeTime = solarSystemTime;
		}

		public void InitFromCurrentStateVectors(double solarSystemTime)
		{
			InitFromStateVectors(parent, RelativePosition, RelativeVelocity, solarSystemTime, true);
		}

		public void SetCelestialBody(CelestialBody body)
		{
			celestialBodyObj = body;
		}

		public void SetArtificialBody(ArtificialBody body)
		{
			artificialBodyObj = body;
		}

		private static void FixEccentricity(ref double eccentricity)
		{
			if (eccentricity == 1.0)
			{
				eccentricity += 1E-11;
			}
		}

		private static double CalculateTrueAnomaly(OrbitParameters o, double timeSincePeriapsis)
		{
			double num = timeSincePeriapsis % o.orbitalPeriod;
			double num2;
			if (o.eccentricity < 1.0)
			{
				double meanAnomaly = num / o.orbitalPeriod * 2.0 * System.Math.PI;
				double d = CalculateEccentricAnomaly(o, meanAnomaly);
				num2 = System.Math.Acos((System.Math.Cos(d) - o.eccentricity) / (1.0 - o.eccentricity * System.Math.Cos(d)));
			}
			else
			{
				double num3 = System.Math.PI * 2.0 * System.Math.Abs(num) / o.orbitalPeriod;
				if (num < 0.0)
				{
					num3 *= -1.0;
				}
				double value = CalculateEccentricAnomaly(o, System.Math.Abs(num3));
				num2 = System.Math.Atan2(System.Math.Sqrt(o.eccentricity * o.eccentricity - 1.0) * System.Math.Sinh(value), o.eccentricity - System.Math.Cosh(value));
			}
			if (num > o.orbitalPeriod / 2.0)
			{
				num2 = System.Math.PI * 2.0 - num2;
			}
			return num2;
		}

		private static double CalculateTrueAnomaly(OrbitParameters o, Vector3D position, Vector3D velocity)
		{
			if (o.eccentricity.IsEpsilonEqualD(0.0, 1E-10))
			{
				Vector3D vec = QuaternionD.AngleAxis(0.0 - o.longitudeOfAscendingNode, Vector3D.Up) * Vector3D.Right;
				double num = MathHelper.AngleSigned(vec, position, Vector3D.Cross(position, velocity).Normalized);
				if (num < 0.0)
				{
					num += 360.0;
				}
				return num * (System.Math.PI / 180.0);
			}
			Vector3D lhs = Vector3D.Cross(velocity, Vector3D.Cross(position, velocity)) / o.parent.gravParameter - position / position.Magnitude;
			double num2 = System.Math.Acos(MathHelper.Clamp(Vector3D.Dot(lhs, position) / (o.eccentricity * position.Magnitude), -1.0, 1.0));
			if (Vector3D.Dot(position, velocity) < 0.0)
			{
				num2 = System.Math.PI * 2.0 - num2;
			}
			if (double.IsNaN(num2))
			{
				num2 = System.Math.PI;
			}
			return num2;
		}

		private static double CalculateTrueAnomalyFromEccentricAnomaly(OrbitParameters o, double eccentricAnomaly)
		{
			double num;
			if (o.eccentricity < 1.0)
			{
				num = System.Math.Acos((System.Math.Cos(eccentricAnomaly) - o.eccentricity) / (1.0 - o.eccentricity * System.Math.Cos(eccentricAnomaly)));
				if (eccentricAnomaly > System.Math.PI)
				{
					num = System.Math.PI * 2.0 - num;
				}
			}
			else
			{
				num = System.Math.Atan2(System.Math.Sqrt(o.eccentricity * o.eccentricity - 1.0) * System.Math.Sinh(eccentricAnomaly), o.eccentricity - System.Math.Cosh(eccentricAnomaly));
				if (eccentricAnomaly < 0.0)
				{
					num = System.Math.PI * 2.0 - num;
				}
			}
			return num;
		}

		private static double CalculateEccentricAnomaly(OrbitParameters o, double meanAnomaly, double maxDeltaDiff = 1E-06, double maxCalculations = 50.0, double maxCalculationsExtremeEcc = 10.0)
		{
			if (o.eccentricity < 1.0)
			{
				if (o.eccentricity < 0.9)
				{
					double num = 1.0;
					double num2 = meanAnomaly + o.eccentricity * System.Math.Sin(meanAnomaly) + 0.5 * o.eccentricity * o.eccentricity * System.Math.Sin(2.0 * meanAnomaly);
					int num3 = 0;
					while (System.Math.Abs(num) > maxDeltaDiff && (double)num3 < maxCalculations)
					{
						num = (meanAnomaly - (num2 - o.eccentricity * System.Math.Sin(num2))) / (1.0 - o.eccentricity * System.Math.Cos(num2));
						num2 += num;
						num3++;
					}
					return num2;
				}
				double num4 = meanAnomaly + 0.85 * o.eccentricity * (double)System.Math.Sign(System.Math.Sin(meanAnomaly));
				for (int i = 0; (double)i < maxCalculationsExtremeEcc; i++)
				{
					double num5 = o.eccentricity * System.Math.Sin(num4);
					double num6 = num4 - num5 - meanAnomaly;
					double num7 = 1.0 - o.eccentricity * System.Math.Cos(num4);
					num4 += -5.0 * num6 / (num7 + (double)System.Math.Sign(num7) * System.Math.Sqrt(System.Math.Abs(16.0 * num7 * num7 - 20.0 * num6 * num5)));
				}
				return num4;
			}
			if (double.IsInfinity(meanAnomaly))
			{
				return meanAnomaly;
			}
			double num8 = 1.0;
			double num9 = System.Math.Log(2.0 * meanAnomaly / o.eccentricity + 1.8);
			int num10 = 0;
			while (System.Math.Abs(num8) > maxDeltaDiff && (double)num10 < maxCalculations)
			{
				num8 = (o.eccentricity * System.Math.Sinh(num9) - num9 - meanAnomaly) / (o.eccentricity * System.Math.Cosh(num9) - 1.0);
				num9 -= num8;
				num10++;
			}
			return num9;
		}

		private static double CalculateEccentricAnomalyFromTrueAnomaly(OrbitParameters o, double trueAnomaly)
		{
			double num = System.Math.Cos(trueAnomaly);
			double num2;
			if (!(o.eccentricity < 1.0))
			{
				num2 = ((System.Math.Abs(o.eccentricity * num + 1.0) >= 1E-05) ? ((!(o.eccentricity * num >= -1.0)) ? double.NaN : MathHelper.Acosh((o.eccentricity + num) / (1.0 + o.eccentricity * num))) : ((!(trueAnomaly >= System.Math.PI)) ? double.PositiveInfinity : double.NegativeInfinity));
			}
			else
			{
				num2 = System.Math.Acos((o.eccentricity + num) / (1.0 + o.eccentricity * num));
				if (trueAnomaly > System.Math.PI)
				{
					num2 = System.Math.PI * 2.0 - num2;
				}
			}
			return num2;
		}

		private static double CalculateMeanAnomalyFromTrueAnomaly(OrbitParameters o, double trueAnomaly)
		{
			double eccentricAnomaly = CalculateEccentricAnomalyFromTrueAnomaly(o, trueAnomaly);
			return CalculateMeanAnomaly(o, trueAnomaly, eccentricAnomaly);
		}

		private static double CalculateMeanAnomaly(OrbitParameters o, double trueAnomaly, double eccentricAnomaly)
		{
			double result = eccentricAnomaly;
			if (o.eccentricity < 1.0)
			{
				result = eccentricAnomaly - o.eccentricity * System.Math.Sin(eccentricAnomaly);
			}
			else if (!double.IsInfinity(eccentricAnomaly))
			{
				result = (o.eccentricity * System.Math.Sinh(eccentricAnomaly) - eccentricAnomaly) * ((!(trueAnomaly >= System.Math.PI)) ? 1.0 : (-1.0));
			}
			return result;
		}

		private static double CalculateDistanceAtTrueAnomaly(OrbitParameters o, double trueAnomaly)
		{
			if (o.eccentricity < 1.0)
			{
				return o.semiMajorAxis * (1.0 - o.eccentricity * o.eccentricity) / (1.0 + o.eccentricity * System.Math.Cos(trueAnomaly));
			}
			return (0.0 - o.semiMajorAxis) * (o.eccentricity * o.eccentricity - 1.0) / (1.0 + o.eccentricity * System.Math.Cos(trueAnomaly));
		}

		private static double CalculateTimeSincePeriapsis(OrbitParameters o, double meanAnomaly)
		{
			if (o.eccentricity < 1.0)
			{
				return meanAnomaly / (System.Math.PI * 2.0) * o.orbitalPeriod;
			}
			return System.Math.Sqrt(System.Math.Pow(0.0 - o.semiMajorAxis, 3.0) / o.parent.gravParameter) * meanAnomaly;
		}

		private static double CalculateTimeSincePeriapsis(OrbitParameters o, Vector3D relPosition, Vector3D relVelocity)
		{
			double trueAnomaly = CalculateTrueAnomaly(o, relPosition, relVelocity);
			double eccentricAnomaly = CalculateEccentricAnomalyFromTrueAnomaly(o, trueAnomaly);
			double meanAnomaly = CalculateMeanAnomaly(o, trueAnomaly, eccentricAnomaly);
			return CalculateTimeSincePeriapsis(o, meanAnomaly);
		}

		private static double CalculatePeriapsisDistance(OrbitParameters o)
		{
			return o.semiMajorAxis * (1.0 - o.eccentricity);
		}

		private static double CalculateApoapsisDistance(OrbitParameters o)
		{
			return o.semiMajorAxis * (1.0 + o.eccentricity);
		}

		private static double CalculateCircumference(OrbitParameters o)
		{
			if (o.eccentricity.IsEpsilonEqualD(0.0))
			{
				return 2.0 * o.semiMajorAxis * System.Math.PI;
			}
			return System.Math.PI * (3.0 * (o.semiMajorAxis + o.semiMinorAxis) - System.Math.Sqrt((3.0 * o.semiMajorAxis + o.semiMinorAxis) * (o.semiMajorAxis + 3.0 * o.semiMinorAxis)));
		}

		public Vector3D PositionAtTrueAnomaly(double angleRad, bool getRelativePosition)
		{
			double num = CalculateDistanceAtTrueAnomaly(this, angleRad);
			Vector3D vector3D = QuaternionD.AngleAxis(0.0 - longitudeOfAscendingNode, Vector3D.Up) * Vector3D.Right;
			Vector3D axis = QuaternionD.AngleAxis(inclination, vector3D) * Vector3D.Up;
			Vector3D vector3D2 = QuaternionD.AngleAxis(0.0 - argumentOfPeriapsis - angleRad * (180.0 / System.Math.PI), axis) * vector3D * num;
			if (getRelativePosition)
			{
				return vector3D2;
			}
			return vector3D2 + parent.Position;
		}

		public Vector3D PositionAtTimeAfterPeriapsis(double timeAfterPeriapsis, bool getRelativePosition)
		{
			double angleRad = CalculateTrueAnomaly(this, timeAfterPeriapsis);
			return PositionAtTrueAnomaly(angleRad, getRelativePosition);
		}

		public Vector3D PositionAfterTime(double time, bool getRelativePosition)
		{
			if (!IsOrbitValid)
			{
				return Vector3D.Zero;
			}
			double angleRad = CalculateTrueAnomaly(this, timeSincePeriapsis + time);
			if (getRelativePosition)
			{
				return PositionAtTrueAnomaly(angleRad, true);
			}
			return parent.PositionAfterTime(time, false) + PositionAtTrueAnomaly(angleRad, true);
		}

		public Vector3D PositionAtEccentricAnomaly(double angleRad, bool getRelativePosition)
		{
			return PositionAtTrueAnomaly(CalculateTrueAnomalyFromEccentricAnomaly(this, angleRad), getRelativePosition);
		}

		public Vector3D VelocityAtTrueAnomaly(double trueAnomaly, bool getRelativeVelocity)
		{
			double num = System.Math.Cos(trueAnomaly);
			double num2 = System.Math.Sin(trueAnomaly);
			double num3 = System.Math.Sqrt(parent.gravParameter / (semiMajorAxis * (1.0 - eccentricity * eccentricity)));
			Vector3D vector3D = QuaternionD.AngleAxis(0.0 - longitudeOfAscendingNode, Vector3D.Up) * Vector3D.Right;
			Vector3D axis = QuaternionD.AngleAxis(inclination, vector3D) * Vector3D.Up;
			Vector3D vector3D2 = QuaternionD.AngleAxis(0.0 - argumentOfPeriapsis, axis) * vector3D;
			Vector3D vector3D3 = QuaternionD.AngleAxis(0.0 - argumentOfPeriapsis - 90.0, axis) * vector3D;
			Vector3D vector3D4 = vector3D2 * ((0.0 - num2) * num3) + vector3D3 * ((eccentricity + num) * num3);
			if (getRelativeVelocity)
			{
				return vector3D4;
			}
			return vector3D4 + parent.Velocity;
		}

		public Vector3D VelocityAtTimeAfterPeriapsis(double timeAfterPeriapsis, bool getRelativeVelocity)
		{
			double trueAnomaly = CalculateTrueAnomaly(this, timeAfterPeriapsis);
			return VelocityAtTrueAnomaly(trueAnomaly, getRelativeVelocity);
		}

		public Vector3D VelocityAfterTime(double time, bool getRelativeVelocity)
		{
			double trueAnomaly = CalculateTrueAnomaly(this, timeSincePeriapsis + time);
			if (!IsOrbitValid)
			{
				return Vector3D.Zero;
			}
			if (getRelativeVelocity)
			{
				return VelocityAtTrueAnomaly(trueAnomaly, getRelativeVelocity);
			}
			return parent.VelocityAfterTime(time, false) + VelocityAtTrueAnomaly(trueAnomaly, true);
		}

		public Vector3D VelocityAtEccentricAnomaly(double angleRad, bool getRelativePosition)
		{
			return VelocityAtTrueAnomaly(CalculateTrueAnomalyFromEccentricAnomaly(this, angleRad), getRelativePosition);
		}

		public void FillPositionAndVelocityAtTrueAnomaly(double angleRad, bool fillRelativeData, ref Vector3D position, ref Vector3D velocity)
		{
			position = PositionAtTrueAnomaly(angleRad, fillRelativeData);
			velocity = VelocityAtTrueAnomaly(angleRad, fillRelativeData);
		}

		public void FillPositionAndVelocityAfterTime(double time, bool fillRelativeData, ref Vector3D position, ref Vector3D velocity)
		{
			position = PositionAfterTime(time, fillRelativeData);
			velocity = VelocityAfterTime(time, fillRelativeData);
		}

		public double GetRotationAngle(double solarSystemTime)
		{
			if (rotationPeriod == 0.0)
			{
				return 0.0;
			}
			return 360.0 * (solarSystemTime % rotationPeriod / rotationPeriod);
		}

		public double CalculatesMeanAnomalyFromTrueAnomaly(double trueAnomaly)
		{
			return CalculateMeanAnomalyFromTrueAnomaly(this, trueAnomaly);
		}

		public void UpdateOrbit(double timeDelta)
		{
			if (parent == null)
			{
				return;
			}
			if (IsOrbitValid)
			{
				timeSincePeriapsis += timeDelta;
				if (eccentricity < 1.0 && timeSincePeriapsis > orbitalPeriod)
				{
					solarSystemTimeAtPeriapsis += orbitalPeriod;
					timeSincePeriapsis %= orbitalPeriod;
				}
				if (eccentricity < 1.0)
				{
					double num = CalculateTrueAnomaly(this, timeSincePeriapsis);
					RelativePosition = PositionAtTrueAnomaly(num, true);
					RelativeVelocity = VelocityAtTrueAnomaly(num, true);
					return;
				}
				double num2 = CalculateTrueAnomaly(this, timeSincePeriapsis);
				RelativePosition = PositionAtTrueAnomaly(num2, true);
				RelativeVelocity = VelocityAtTrueAnomaly(num2, true);
				if (RelativePosition.IsInfinity() || RelativePosition.IsNaN())
				{
					Vector3D vector3D = PositionAtTrueAnomaly(lastValidTrueAnomaly, true);
					Vector3D vector3D2 = VelocityAtTrueAnomaly(lastValidTrueAnomaly, true);
					RelativePosition = vector3D + vector3D2 * (timeSincePeriapsis - lastValidTimeSincePeriapsis);
					RelativeVelocity = vector3D2;
				}
				else
				{
					lastValidTrueAnomaly = num2;
					lastValidTimeSincePeriapsis = timeSincePeriapsis;
				}
			}
			else
			{
				Vector3D vector3D3 = (-RelativePosition).Normalized * (parent.gravParameter / RelativePosition.SqrMagnitude);
				RelativeVelocity += vector3D3 * timeDelta;
				RelativePosition += Velocity * timeDelta;
			}
		}

		public void ResetOrbit(double solarSystemTime)
		{
			if (!IsOrbitValid)
			{
				return;
			}
			if (eccentricity < 1.0)
			{
				for (timeSincePeriapsis = solarSystemTime - solarSystemTimeAtPeriapsis; timeSincePeriapsis < 0.0 - orbitalPeriod; timeSincePeriapsis += orbitalPeriod)
				{
				}
				if (timeSincePeriapsis > orbitalPeriod)
				{
					timeSincePeriapsis %= orbitalPeriod;
					solarSystemTimeAtPeriapsis = solarSystemTime - timeSincePeriapsis;
				}
			}
			else
			{
				timeSincePeriapsis = solarSystemTime - solarSystemTimeAtPeriapsis;
			}
			double num = CalculateTrueAnomaly(this, timeSincePeriapsis);
			RelativePosition = PositionAtTrueAnomaly(num, true);
			RelativeVelocity = VelocityAtTrueAnomaly(num, true);
		}

		public List<Vector3D> GetOrbitPositions(int numberOfPositions, double timeStep)
		{
			if (!IsOrbitValid)
			{
				return new List<Vector3D>();
			}
			List<Vector3D> list = new List<Vector3D>();
			if (eccentricity < 1.0)
			{
				double num = System.Math.PI * 2.0 / (double)numberOfPositions;
				for (int i = 0; i < numberOfPositions; i++)
				{
					list.Add(PositionAtEccentricAnomaly((double)i * num, true));
				}
			}
			else
			{
				for (int j = 0; j < numberOfPositions; j++)
				{
					list.Add(PositionAtTimeAfterPeriapsis(timeSincePeriapsis + (double)j * timeStep, true));
				}
			}
			return list;
		}

		public List<Vector3D> GetOrbitVelocities(int numberOfPositions, bool getRelativeVelocities, double timeStep)
		{
			if (!IsOrbitValid)
			{
				return new List<Vector3D>();
			}
			List<Vector3D> list = new List<Vector3D>();
			if (eccentricity < 1.0)
			{
				double num = System.Math.PI * 2.0 / (double)numberOfPositions;
				for (int i = 0; i < numberOfPositions; i++)
				{
					list.Add(VelocityAtEccentricAnomaly((double)i * num, getRelativeVelocities));
				}
			}
			else
			{
				for (int j = 0; j < numberOfPositions; j++)
				{
					list.Add(VelocityAtTimeAfterPeriapsis(timeSincePeriapsis + (double)j * timeStep, getRelativeVelocities));
				}
			}
			return list;
		}

		public double GetTimeAfterPeriapsis(Vector3D position, Vector3D velocity, bool areValuesRelative)
		{
			if (!areValuesRelative)
			{
				position -= parent.Position;
				velocity -= parent.Velocity;
			}
			return CalculateTimeSincePeriapsis(this, position, velocity);
		}

		public void ChangeOrbitParent(OrbitParameters newParent)
		{
			RelativePosition = Position - newParent.Position;
			RelativeVelocity = Velocity - newParent.Velocity;
			parent = newParent;
		}

		public void GetOrbitPlaneData(out QuaternionD rotation, out Vector3D centerPosition)
		{
			Vector3D vector3D = QuaternionD.AngleAxis(0.0 - longitudeOfAscendingNode, Vector3D.Up) * Vector3D.Right;
			Vector3D axis = QuaternionD.AngleAxis(inclination, vector3D) * Vector3D.Up;
			Vector3D normalized = (QuaternionD.AngleAxis(0.0 - argumentOfPeriapsis, axis) * vector3D).Normalized;
			rotation = QuaternionD.LookRotation(normalized, Vector3D.Cross(-RelativePosition, RelativeVelocity).Normalized);
			centerPosition = normalized * (CalculatePeriapsisDistance(this) - semiMajorAxis);
		}

		public double TrueAnomalyAtZeroTime()
		{
			return CalculateTrueAnomaly(this, orbitalPeriod - solarSystemTimeAtPeriapsis % orbitalPeriod);
		}

		public double TrueAnomalyAtZeroTimePlusTime(double extraTime)
		{
			return CalculateTrueAnomaly(this, extraTime);
		}

		public double TrueAnomalyAtZeroTimeFromCurrent(double extraTime)
		{
			return CalculateTrueAnomaly(this, timeSincePeriapsis + extraTime);
		}

		public double TrueAnomalyAtCurrentTime()
		{
			return CalculateTrueAnomaly(this, timeSincePeriapsis);
		}

		public void FillOrbitData(ref OrbitData data, SpaceObjectVessel targetVessel = null)
		{
			data.ParentGUID = parent.celestialBodyObj.GUID;
			if (targetVessel != null)
			{
				data.GUID = targetVessel.GUID;
				if (targetVessel is Ship)
				{
					data.ObjectType = SpaceObjectType.Ship;
				}
				else if (targetVessel is Asteroid)
				{
					data.ObjectType = SpaceObjectType.Asteroid;
				}
			}
			data.Eccentricity = eccentricity;
			data.SemiMajorAxis = semiMajorAxis;
			data.Inclination = inclination;
			data.ArgumentOfPeriapsis = argumentOfPeriapsis;
			data.LongitudeOfAscendingNode = longitudeOfAscendingNode;
			data.TimeSincePeriapsis = timeSincePeriapsis;
			data.SolarSystemPeriapsisTime = solarSystemTimeAtPeriapsis;
		}

		public OrbitData GetOrbitData(SpaceObjectVessel vessel = null)
		{
			OrbitData data = new OrbitData();
			FillOrbitData(ref data, vessel);
			return data;
		}

		private void CheckParent(long parentGUID)
		{
			if (parent == null || parent.celestialBodyObj == null || parent.celestialBodyObj.GUID != parentGUID)
			{
				parent = Client.Instance.SolarSystem.FindCelestialBody(parentGUID).Orbit;
			}
		}

		public void ParseNetworkData(OrbitData data, bool resetOrbit = false)
		{
			CheckParent(data.ParentGUID);
			solarSystemTimeAtPeriapsis = data.SolarSystemPeriapsisTime;
			double currentTime = Client.Instance.SolarSystem.CurrentTime;
			timeSincePeriapsis = currentTime - solarSystemTimeAtPeriapsis;
			InitFromElements(parent, -1.0, -1.0, -1.0, data.Eccentricity, data.SemiMajorAxis, data.Inclination, data.ArgumentOfPeriapsis, data.LongitudeOfAscendingNode, timeSincePeriapsis, currentTime);
			if (resetOrbit)
			{
				ResetOrbit(currentTime);
			}
		}

		public void ParseNetworkData(RealtimeData data)
		{
			CheckParent(data.ParentGUID);
			RelativePosition = data.Position.ToVector3D();
			RelativeVelocity = data.Velocity.ToVector3D();
		}

		public void ParseNetworkData(ManeuverData data)
		{
			CheckParent(data.ParentGUID);
			RelativePosition = data.RelPosition.ToVector3D();
			RelativeVelocity = data.RelVelocity.ToVector3D();
		}

		public bool AreOrbitsEqual(OrbitParameters orbit)
		{
			return parent == orbit.parent && eccentricity.IsEpsilonEqualD(orbit.eccentricity, 1E-08) && semiMajorAxis.IsEpsilonEqualD(orbit.semiMajorAxis, 1E-08) && inclination.IsEpsilonEqualD(orbit.inclination, 1E-08) && argumentOfPeriapsis.IsEpsilonEqualD(orbit.argumentOfPeriapsis, 1E-08) && longitudeOfAscendingNode.IsEpsilonEqualD(orbit.longitudeOfAscendingNode, 1E-08) && solarSystemTimeAtPeriapsis.IsEpsilonEqualD(orbit.solarSystemTimeAtPeriapsis, 0.001);
		}

		private static bool AreAnglesEqualDeg(double angle1, double angle2, double anglePrecissionDeg)
		{
			angle1 %= 360.0;
			angle2 %= 360.0;
			if (angle1 < 0.0)
			{
				angle1 += 360.0;
			}
			if (angle2 < 0.0)
			{
				angle2 += 360.0;
			}
			return angle1.IsEpsilonEqualD(angle2, anglePrecissionDeg) || (angle1 >= 360.0 - anglePrecissionDeg && angle2 <= anglePrecissionDeg - 360.0 + angle1) || (angle2 >= 360.0 - anglePrecissionDeg && angle1 <= anglePrecissionDeg - 360.0 + angle2);
		}

		public bool AreOrbitsOverlapping(OrbitParameters orbit, double axisPrecision = 1.0, double eccentricityPrecision = 1E-08, double anglePrecissionDeg = 1.0, double eccentricityZero = 0.001)
		{
			if (parent != orbit.parent || !eccentricity.IsEpsilonEqualD(orbit.eccentricity, eccentricityPrecision) || !semiMajorAxis.IsEpsilonEqualD(orbit.semiMajorAxis, axisPrecision))
			{
				return false;
			}
			if (eccentricity.IsEpsilonEqualD(0.0, eccentricityZero))
			{
				bool flag = AreAnglesEqualDeg(longitudeOfAscendingNode, orbit.longitudeOfAscendingNode, anglePrecissionDeg);
				bool flag2 = AreAnglesEqualDeg(inclination, orbit.inclination, anglePrecissionDeg);
				if (flag2 && (flag || inclination.IsEpsilonEqualD(0.0, eccentricityZero)))
				{
					return true;
				}
				if (flag && (flag2 || AreAnglesEqualDeg(inclination, 180.0 - orbit.inclination, anglePrecissionDeg)))
				{
					return true;
				}
				return false;
			}
			return AreAnglesEqualDeg(longitudeOfAscendingNode, orbit.longitudeOfAscendingNode, anglePrecissionDeg) && AreAnglesEqualDeg(argumentOfPeriapsis, orbit.argumentOfPeriapsis, anglePrecissionDeg) && AreAnglesEqualDeg(inclination, orbit.inclination, anglePrecissionDeg);
		}

		public void CopyDataFrom(OrbitParameters orbit, double solarSystemTime, bool exactCopy = false)
		{
			parent = orbit.parent;
			eccentricity = orbit.eccentricity;
			semiMajorAxis = orbit.semiMajorAxis;
			semiMinorAxis = orbit.semiMinorAxis;
			inclination = orbit.inclination;
			argumentOfPeriapsis = orbit.argumentOfPeriapsis;
			longitudeOfAscendingNode = orbit.longitudeOfAscendingNode;
			solarSystemTimeAtPeriapsis = orbit.solarSystemTimeAtPeriapsis;
			orbitalPeriod = orbit.orbitalPeriod;
			if (exactCopy)
			{
				timeSincePeriapsis = orbit.timeSincePeriapsis;
				if (lastChangeTime < orbit.lastChangeTime)
				{
					lastChangeTime = orbit.lastChangeTime;
				}
				RelativePosition = orbit.RelativePosition;
				RelativeVelocity = orbit.RelativeVelocity;
			}
			else
			{
				ResetOrbit(solarSystemTime);
			}
		}

		public void SetLastChangeTime(double time)
		{
			lastChangeTime = time;
		}

		public List<Vector3D> GetFlightPathPositions(int numberOfPositions, double timeStep, out bool parentChanged)
		{
			parentChanged = false;
			List<Vector3D> posList = new List<Vector3D>();
			CalculateFlightPathPositions(parent, this, numberOfPositions, timeStep, 0.0, ref posList, ref parentChanged);
			return posList;
		}

		private static OrbitParameters CheckGravityInfluenceRadius(OrbitParameters o, double trueAnomaly, double timePassed, Vector3D position, Vector3D parentPos, ref List<OrbitParameters> celBodies)
		{
			if (!double.IsInfinity(o.parent.gravityInfluenceRadiusSquared) && position.DistanceSquared(parentPos) > o.parent.gravityInfluenceRadiusSquared)
			{
				Vector3D vector3D = o.VelocityAtTrueAnomaly(trueAnomaly, true) + o.parent.VelocityAfterTime(timePassed, false);
				Vector3D vector3D2 = o.parent.parent.PositionAfterTime(timePassed, false);
				Vector3D vector3D3 = o.parent.parent.VelocityAfterTime(timePassed, false);
				OrbitParameters orbitParameters = new OrbitParameters();
				orbitParameters.InitFromStateVectors(o.parent.parent, position - vector3D2, vector3D - vector3D3, Client.Instance.SolarSystem.CurrentTime, true);
				return orbitParameters;
			}
			if (celBodies.Count > 0)
			{
				foreach (OrbitParameters celBody in celBodies)
				{
					if (position.DistanceSquared(celBody.PositionAfterTime(timePassed, false)) < celBody.gravityInfluenceRadiusSquared)
					{
						Vector3D vector3D4 = o.VelocityAtTrueAnomaly(trueAnomaly, true) + o.parent.VelocityAfterTime(timePassed, false);
						Vector3D vector3D5 = celBody.PositionAfterTime(timePassed, false);
						Vector3D vector3D6 = celBody.VelocityAfterTime(timePassed, false);
						OrbitParameters orbitParameters2 = new OrbitParameters();
						orbitParameters2.InitFromStateVectors(celBody, position - vector3D5, vector3D4 - vector3D6, Client.Instance.SolarSystem.CurrentTime, true);
						return orbitParameters2;
					}
				}
			}
			return null;
		}

		private static void CalculateFlightPathPositions(OrbitParameters positionsParent, OrbitParameters o, int numberOfPositions, double timeStep, double timePassed, ref List<Vector3D> posList, ref bool parentChanged)
		{
			if (posList == null)
			{
				posList = new List<Vector3D>();
			}
			List<OrbitParameters> celBodies = new List<OrbitParameters>();
			foreach (CelestialBody celestialBody in Client.Instance.SolarSystem.GetCelestialBodies())
			{
				if (celestialBody.Orbit.parent == o.parent)
				{
					celBodies.Add(celestialBody.Orbit);
				}
			}
			if (o.eccentricity < 1.0)
			{
				double meanAnomaly = o.timeSincePeriapsis / o.orbitalPeriod * 2.0 * System.Math.PI;
				double num = CalculateEccentricAnomaly(o, meanAnomaly);
				double num2 = o.timeSincePeriapsis;
				double num3 = o.timeSincePeriapsis;
				double num4 = System.Math.PI * 2.0 / (double)numberOfPositions;
				for (int i = 0; i < numberOfPositions; i++)
				{
					num += num4;
					if (num > System.Math.PI * 2.0)
					{
						num -= System.Math.PI * 2.0;
					}
					double num5 = System.Math.Acos((System.Math.Cos(num) - o.eccentricity) / (1.0 - o.eccentricity * System.Math.Cos(num)));
					if (num3 > o.orbitalPeriod / 2.0)
					{
						num5 = System.Math.PI * 2.0 - num5;
					}
					meanAnomaly = CalculateMeanAnomaly(o, num5, num);
					num3 = CalculateTimeSincePeriapsis(o, meanAnomaly);
					timePassed = ((!(num3 < num2)) ? (timePassed + (num3 - num2)) : (timePassed + (o.orbitalPeriod - num2 + num3)));
					num2 = num3;
					Vector3D vector3D = o.parent.PositionAfterTime(timePassed, false);
					Vector3D vector3D2 = o.PositionAtTrueAnomaly(num5, true) + vector3D;
					posList.Add(vector3D2 - positionsParent.PositionAfterTime(timePassed, false));
					OrbitParameters orbitParameters = CheckGravityInfluenceRadius(o, num5, timePassed, vector3D2, vector3D, ref celBodies);
					if (orbitParameters != null)
					{
						parentChanged = true;
						CalculateFlightPathPositions(positionsParent, orbitParameters, numberOfPositions - i - 1, timeStep, timePassed, ref posList, ref parentChanged);
						break;
					}
				}
				return;
			}
			double num6 = o.timeSincePeriapsis;
			double num7 = o.timeSincePeriapsis;
			double num8 = 0.0;
			double num9 = 0.0;
			for (int j = 0; j < numberOfPositions; j++)
			{
				num7 = o.timeSincePeriapsis + (double)j * timeStep;
				double num10 = CalculateTrueAnomaly(o, num7);
				timePassed += num7 - num6;
				num6 = num7;
				Vector3D vector3D3 = o.PositionAtTrueAnomaly(num10, true);
				if (vector3D3.IsInfinity() || vector3D3.IsNaN())
				{
					Vector3D vector3D4 = o.PositionAtTrueAnomaly(num8, true);
					Vector3D vector3D5 = o.VelocityAtTrueAnomaly(num8, true);
					vector3D3 = vector3D4 + vector3D5 * (num7 - num9);
				}
				else
				{
					num8 = num10;
					num9 = num7;
				}
				Vector3D vector3D6 = o.parent.PositionAfterTime(timePassed, false);
				vector3D3 += vector3D6;
				posList.Add(vector3D3 - positionsParent.PositionAfterTime(timePassed, false));
				OrbitParameters orbitParameters2 = CheckGravityInfluenceRadius(o, num10, timePassed, vector3D3, vector3D6, ref celBodies);
				if (orbitParameters2 != null)
				{
					parentChanged = true;
					CalculateFlightPathPositions(positionsParent, orbitParameters2, numberOfPositions - j - 1, timeStep, timePassed, ref posList, ref parentChanged);
					break;
				}
			}
		}

		public double CircularOrbitVelocityMagnitudeAtDistance(double distance)
		{
			return System.Math.Sqrt(gravParameter / distance);
		}

		public double RandomOrbitVelocityMagnitudeAtDistance(double distance)
		{
			double num = MathHelper.RandomRange(0.0, 0.8);
			double num2 = distance / (1.0 - num);
			if (num2 + num2 - distance > gravityInfluenceRadius)
			{
				num2 = gravityInfluenceRadius * 0.8 / 2.0;
				num = 1.0 - distance / num2;
			}
			if (num2 + num2 - distance > 897587224200.0)
			{
				num2 = 359034889680.0;
				num = 1.0 - distance / num2;
			}
			if (num < 0.0)
			{
				num = 0.0;
			}
			return System.Math.Sqrt((num + 1.0) / distance * gravParameter);
		}

		public string DebugString()
		{
			return string.Format("P {0}, ECC {1}, SMA {2}, INC {3}, AOP {4}, LOAN {5}, SSTAP {6}, TSP {7}", (parent == null) ? (-1) : parent.GUID, eccentricity, semiMajorAxis, inclination, argumentOfPeriapsis, longitudeOfAscendingNode, solarSystemTimeAtPeriapsis, timeSincePeriapsis);
		}
	}
}
