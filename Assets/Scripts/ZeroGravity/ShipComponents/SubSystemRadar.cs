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

		[System.NonSerialized] public bool ActiveScanPending;

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

		public override SystemAuxData GetAuxData()
		{
			RadarAuxData radarAuxData = new RadarAuxData
			{
				ActiveScanDuration = ActiveScanDuration
			};
			return radarAuxData;
		}

		public override void SetDetails(SubSystemDetails details, bool instant = false)
		{
			if (Status != details.Status && ActiveScanPending)
			{
				if (details.Status == SystemStatus.Online)
				{
					World.Map.ShowScanningEffect();
				}
				else
				{
					ActiveScanPending = false;
					World.Map.HideScanningEffect();
				}
			}

			base.SetDetails(details, instant);
		}

		public double GetCelestialSensitivityModifier()
		{
			double num = 1.0;
			Vector3D vesselWorldPosition = World.LocalToWorldPosition(ParentVessel.transform.position);
			for (CelestialBody celestialBody = World.SolarSystem.GetParentCelestialBody(vesselWorldPosition);
			     celestialBody != null;
			     celestialBody = celestialBody.ParentCelesitalBody)
			{
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
