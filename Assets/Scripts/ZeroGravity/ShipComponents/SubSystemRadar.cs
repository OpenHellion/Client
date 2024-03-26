using System.Linq;
using System.Threading.Tasks;
using OpenHellion;
using UnityEngine;
using UnityEngine.Serialization;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.ShipComponents
{
	public class SubSystemRadar : SubSystem
	{
		[SerializeField] private ResourceRequirement[] _ResourceRequirements = new ResourceRequirement[1]
		{
			new ResourceRequirement
			{
				ResourceType = DistributionSystemType.Power
			}
		};

		[Title("Sensivity")] public double ActiveScanSensitivity = 3600.0;

		public double ActiveScanFuzzySensitivity = 7200.0;

		public float ActiveScanDuration = 3f;

		[FormerlySerializedAs("passiveScanSensitivity")]
		public double PassiveScanSensitivity = 20.0;

		[FormerlySerializedAs("warpDetectionSensitivity")]
		public double WarpDetectionSensitivity = 1000.0;

		[SerializeField] [Range(0.2f, 1f)] private float fuzzySphereSize = 1f;

		public Task ActiveScanTask;

		private MachineryPart SignalAmplifier;

		public override SubSystemType Type => SubSystemType.Radar;

		public override ResourceRequirement[] ResourceRequirements => _ResourceRequirements;

		public float SignalAmplification
		{
			get
			{
				if (SignalAmplifier == null || SignalAmplifier.Health <= float.Epsilon)
				{
					return 1f;
				}

				return SignalAmplifier.TierMultiplier;
			}
		}

		private bool ActiveScanObject(ArtificialBody target, float scanAngle, Vector3 scanDirection)
		{
			if (target.RadarVisibilityType == RadarVisibilityType.Distress ||
			    target.RadarVisibilityType == RadarVisibilityType.AlwaysVisible)
			{
				return false;
			}

			Vector3D vector3D = target.Position - ParentVessel.Position;
			double magnitude = vector3D.Magnitude;
			float num = (float)Vector3D.Angle(scanDirection.ToVector3D(), vector3D.Normalized);
			if (num <= scanAngle / 2f)
			{
				double sensitivityMultiplier = GetSensitivityMultiplier();
				double num2 = target.RadarSignature * CheckBehindPlanets(vector3D, magnitude);
				double num3 = ActiveScanSensitivity * 1000.0 / scanAngle * num2 * sensitivityMultiplier;
				double num4 = ActiveScanFuzzySensitivity * 1000.0 / scanAngle * num2 * sensitivityMultiplier;
				bool flag = magnitude < num3;
				bool flag2 = magnitude < num4;
				bool flag3 = magnitude < PassiveScanSensitivity * 1000.0 * num2 * sensitivityMultiplier;
				SpaceObjectVessel spaceObjectVessel = target as SpaceObjectVessel;
				if (target.RadarVisibilityType != RadarVisibilityType.Unknown && flag && !flag3)
				{
					target.RadarVisibilityType = RadarVisibilityType.Unknown;
					if (spaceObjectVessel != null)
					{
						spaceObjectVessel.SetLastKnownMapOrbit();
						if (spaceObjectVessel.VesselData != null && spaceObjectVessel.VesselData.SpawnRuleID != 0)
						{
							World.Map.UnknownVisibilityOrbits[spaceObjectVessel.VesselData.SpawnRuleID] =
								spaceObjectVessel.LastKnownMapOrbit;
						}
					}

					return true;
				}

				if (target.RadarVisibilityType == RadarVisibilityType.Invisible && flag2 && !flag3 && !flag &&
				    spaceObjectVessel != null)
				{
					double num5 = System.Math.Cos((scanAngle / 2.0 - num) * (System.Math.PI / 180.0)) *
					              magnitude;
					double num6 = num5 / System.Math.Cos(scanAngle / 2.0 * (System.Math.PI / 180.0));
					double num7 = System.Math.Sqrt(num6 * num6 - num5 * num5);
					Vector3D vector3D2 = ParentVessel.Position + scanDirection.ToVector3D().Normalized * num6;
					if (fuzzySphereSize == 1f)
					{
						World.Map.InitializeMapObjectFuzzyScan(vector3D2, num7 * 2.0, spaceObjectVessel);
					}
					else
					{
						Vector3D vector3D3 = vector3D - vector3D2;
						double num8 = num7 * fuzzySphereSize;
						int hashCode = target.GetHashCode();
						Vector3D vector3D4 = new Vector3D(MathHelper.RandomRange(-1.0, 1.0, hashCode),
								MathHelper.RandomRange(-1.0, 1.0, hashCode),
								MathHelper.RandomRange(-1.0, 1.0, hashCode))
							.Normalized * num8;
						Vector3D position = ((!(vector3D3.Magnitude > num7 - num8))
							? (target.Position + vector3D4)
							: (target.Position + Vector3D.Project(vector3D4, -vector3D3)));
						World.Map.InitializeMapObjectFuzzyScan(position, num8 * 2.0, spaceObjectVessel);
					}
				}
			}

			return false;
		}

		public double GetSensitivityMultiplier()
		{
			double num = SignalAmplification * GetCelestialSensitivityModifier();
			foreach (DebrisField debrisField in World.DebrisFields)
			{
				if (debrisField.CheckObject(ParentVessel))
				{
					num *= debrisField.ScanningSensitivityMultiplier;
				}
			}

			return num;
		}

		public bool PassiveScanObject(ArtificialBody target)
		{
			if (target.RadarVisibilityType == RadarVisibilityType.Distress ||
			    target.RadarVisibilityType == RadarVisibilityType.AlwaysVisible)
			{
				return false;
			}

			if (target == ParentVessel ||
			    (MyPlayer.Instance != null && target.Guid == MyPlayer.Instance.HomeStationGUID))
			{
				if (target.RadarVisibilityType != RadarVisibilityType.Visible)
				{
					target.RadarVisibilityType = RadarVisibilityType.Visible;
					return true;
				}

				return false;
			}

			Vector3D relTargetPosition = target.Position - ParentVessel.Position;
			double magnitude = relTargetPosition.Magnitude;
			double sensitivityMultiplier = GetSensitivityMultiplier();
			double num = target.RadarSignature * CheckBehindPlanets(relTargetPosition, magnitude);
			double num2 = PassiveScanSensitivity * 1000.0 * num * sensitivityMultiplier;
			double num3 = WarpDetectionSensitivity * 1000.0 * num * sensitivityMultiplier;
			bool flag = magnitude < num2;
			bool flag2 = magnitude < num3;
			SpaceObjectVessel spaceObjectVessel = target as SpaceObjectVessel;
			if (target.RadarVisibilityType != RadarVisibilityType.Warp && target.Maneuver?.Type == ManeuverType.Warp)
			{
				target.RadarVisibilityType = RadarVisibilityType.Warp;
				if (spaceObjectVessel is not null)
				{
					spaceObjectVessel.ResetLastKnownMapOrbit();
				}

				if (flag2 && !flag)
				{
					World.Map.InitializeMapObjectStartWarpFar(target.Position, (magnitude - num2) / (num3 - num2));
				}
				else if (flag)
				{
					World.Map.InitializeMapObjectStartWarpNear(target.Position,
						Quaternion.LookRotation(target.Forward, target.Up));
				}

				return true;
			}

			if (target.RadarVisibilityType == RadarVisibilityType.Warp && target.Maneuver == null && flag2 && !flag)
			{
				target.RadarVisibilityType = RadarVisibilityType.Invisible;
				if (spaceObjectVessel is not null)
				{
					spaceObjectVessel.ResetLastKnownMapOrbit();
				}

				World.Map.InitializeMapObjectEndWarp(target.Position, (magnitude - num2) / (num3 - num2));
				return true;
			}

			if (target.Maneuver == null)
			{
				if (target.RadarVisibilityType == RadarVisibilityType.Visible && !flag)
				{
					target.RadarVisibilityType = RadarVisibilityType.Unknown;
					if (spaceObjectVessel is not null)
					{
						spaceObjectVessel.SetLastKnownMapOrbit();
						if (spaceObjectVessel.VesselData != null && spaceObjectVessel.VesselData.SpawnRuleID != 0)
						{
							World.Map.UnknownVisibilityOrbits[spaceObjectVessel.VesselData.SpawnRuleID] =
								spaceObjectVessel.LastKnownMapOrbit;
						}
					}

					return true;
				}

				if (target.RadarVisibilityType != RadarVisibilityType.Visible && flag)
				{
					target.RadarVisibilityType = RadarVisibilityType.Visible;
					if (spaceObjectVessel?.VesselData != null && spaceObjectVessel.VesselData.SpawnRuleID != 0)
					{
						World.Map.UnknownVisibilityOrbits.Remove(spaceObjectVessel.VesselData.SpawnRuleID);
						spaceObjectVessel.ResetLastKnownMapOrbit();
					}

					return true;
				}

				if (target.RadarVisibilityType == RadarVisibilityType.Unknown &&
				    spaceObjectVessel?.LastKnownMapOrbit != null &&
				    (spaceObjectVessel.LastKnownMapOrbit.Position - ParentVessel.Position).Magnitude < num2)
				{
					target.RadarVisibilityType = RadarVisibilityType.Invisible;
					if (spaceObjectVessel is not null)
					{
						spaceObjectVessel.ResetLastKnownMapOrbit();
					}

					return true;
				}

				if (target.RadarVisibilityType == RadarVisibilityType.Invisible &&
				    spaceObjectVessel?.VesselData != null && spaceObjectVessel.VesselData.SpawnRuleID != 0 &&
				    World.Map.UnknownVisibilityOrbits.TryGetValue(spaceObjectVessel.VesselData.SpawnRuleID,
					    out var value))
				{
					target.RadarVisibilityType = RadarVisibilityType.Unknown;
					spaceObjectVessel.LastKnownMapOrbit = value;
					return true;
				}
			}

			return false;
		}

		private double CheckBehindPlanets(Vector3D relTargetPosition, double distance)
		{
			foreach (CelestialBody celestialBody in World.SolarSystem.GetCelestialBodies())
			{
				Vector3D from = celestialBody.Position - ParentVessel.Position;
				double magnitude = from.Magnitude;
				double num = System.Math.Asin(celestialBody.Radius / magnitude) * (180.0 / System.Math.PI);
				double num2 = Vector3D.Angle(from, relTargetPosition);
				double num3 = System.Math.Sqrt(magnitude * magnitude - celestialBody.Radius * celestialBody.Radius);
				if (distance > num3 && num2 < num)
				{
					return 0.0;
				}
			}

			return 1.0;
		}

		public void PassiveScan()
		{
			bool flag = false;
			foreach (ArtificialBody artificialBody in SolarSystem.ArtificialBodyReferences)
			{
				if (!(artificialBody is Pivot) &&
				    (!(artificialBody is SpaceObjectVessel) || (artificialBody as SpaceObjectVessel).IsMainVessel) &&
				    PassiveScanObject(artificialBody))
				{
					flag = true;
				}
			}

			if (flag)
			{
				World.SolarSystem.ArtificialBodiesVisibilityModified();
				World.Map.SaveMapDetails();
				World.Map.IsInitializing = false;
			}
		}

		public void ActiveScan(float angle, Vector3 scanDirection)
		{
			if (MyPlayer.Instance.ShipControlMode == ShipControlMode.None)
			{
				return;
			}

			bool flag = false;
			foreach (ArtificialBody item in SolarSystem.ArtificialBodyReferences
				         .OrderBy((ArtificialBody m) => (m.Position - ParentVessel.Position).SqrMagnitude)
				         .Reverse())
			{
				if (item is not Pivot && (item is not SpaceObjectVessel || (item as SpaceObjectVessel).IsMainVessel) &&
				    ActiveScanObject(item, angle, scanDirection))
				{
					flag = true;
				}
			}

			if (flag)
			{
				World.SolarSystem.ArtificialBodiesVisibilityModified();
				World.Map.SaveMapDetails();
			}
		}

		public override SystemAuxData GetAuxData()
		{
			RadarAuxData radarAuxData = new RadarAuxData();
			radarAuxData.ActiveScanDuration = ActiveScanDuration;
			return radarAuxData;
		}

		public override void SetDetails(SubSystemDetails details, bool instant = false)
		{
			if (Status != details.Status && ActiveScanTask != null)
			{
				if (details.Status == SystemStatus.Online)
				{
					World.Map.ShowScanningEffect();
				}
				else
				{
					if (details.SecondaryStatus != SystemSecondaryStatus.Defective &&
					    details.SecondaryStatus != SystemSecondaryStatus.Malfunction)
					{
						ActiveScanTask.RunSynchronously();
					}

					ActiveScanTask = null;
					World.Map.HideScanningEffect();
				}
			}

			base.SetDetails(details, instant);
		}

		public double GetCelestialSignatureModifier(ArtificialBody target)
		{
			double num = 1.0;
			for (OrbitParameters parent = target.Orbit.Parent; parent != null; parent = parent.Parent)
			{
				CelestialBody celestialBody = parent.CelestialBody;
				num *= celestialBody.GetRadarSignatureModifier(World, target);
			}

			return num;
		}

		public double GetCelestialSensitivityModifier()
		{
			double num = 1.0;
			for (OrbitParameters parent = ParentVessel.Orbit.Parent; parent != null; parent = parent.Parent)
			{
				CelestialBody celestialBody = parent.CelestialBody;
				num *= celestialBody.GetScanningSensitivityModifier(World, ParentVessel);
			}

			return num;
		}

		public override void MachineryPartAttached(SceneMachineryPartSlot slot)
		{
			base.MachineryPartAttached(slot);
			if (slot.Scope == MachineryPartSlotScope.RadarSignalAmplifier)
			{
				SignalAmplifier = slot.Item as MachineryPart;
			}
		}

		public override void MachineryPartDetached(SceneMachineryPartSlot slot)
		{
			base.MachineryPartDetached(slot);
			if (slot.Scope == MachineryPartSlotScope.RadarSignalAmplifier)
			{
				SignalAmplifier = null;
			}
		}
	}
}
