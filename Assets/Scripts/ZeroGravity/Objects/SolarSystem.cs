using System.Collections.Generic;
using OpenHellion;
using OpenHellion.IO;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Math;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.Objects
{
	public class SolarSystem : MonoBehaviour
	{
		public const double VisibilityLimitDestroySqr = 225000000.0; // 15km

		public const double VisibilityLimitLoadSqr = 100000000.0; // 10km

		public const double DetailsLimitUnsubscribe = 6250000.0;

		public const double DetailsLimitSubscribe = 2250000.0;

		public const double RadarVisibilityDistance = 1000000000000.0;

		public const double SunScale = 149597870.7;

		public const double PlanetsScale = 1000000.0;

		private const double PlanetsToShowDistance = 10000000000.0;

		private const double PlanetsToDestroyDistance = 11000000000.0;

		private double _currentTime;

		private double _timeCorrection;

		private Transform _sunRoot;

		private Transform _planetsRoot;

		[SerializeField] private Map _map;

		private World _world;

		private readonly List<CelestialBody> _celestialBodyReferences = new List<CelestialBody>();

		public double CurrentTime => _currentTime;

		public void AddCelestialBody(CelestialBody body)
		{
			if (body.Guid == 1)
			{
				body.CreateSunSpaceGameObject(_sunRoot);
			}

			_celestialBodyReferences.Add(body);
		}

		public void RemoveCelestialBody(CelestialBody body)
		{
			_celestialBodyReferences.Remove(body);
		}

		public CelestialBody FindCelestialBody(long guid)
		{
			return _celestialBodyReferences.Find((CelestialBody m) => m.Guid == guid);
		}

		public CelestialBody FindCelestialBodyParent(Vector3D position)
		{
			CelestialBody result = _celestialBodyReferences[0];
			double num = _celestialBodyReferences[0].Position.DistanceSquared(position);
			for (int i = 1; i < _celestialBodyReferences.Count; i++)
			{
				double num2 = _celestialBodyReferences[i].Position.DistanceSquared(position);
				if (num2 < _celestialBodyReferences[i].Orbit.GravityInfluenceRadiusSquared && num2 < num)
				{
					result = _celestialBodyReferences[i];
					num = num2;
				}
			}

			return result;
		}

		public List<CelestialBody> GetCelestialBodies()
		{
			return _celestialBodyReferences;
		}

		public void CalculatePositionsAfterTime(double time)
		{
			_currentTime = time;
			_timeCorrection = HiResTime.Milliseconds / 1000.0 - time;
			foreach (CelestialBody celestialBody in _celestialBodyReferences)
			{
				celestialBody.UpdatePosition(this, time, resetTime: true);
			}
		}

		// Updates time and positions of celestial bodies after every tick is done.
		public void UpdatePositions()
		{
			double updatedCurrentTime = HiResTime.Milliseconds / 1000.0 - _timeCorrection;
			double deltaTime = updatedCurrentTime - _currentTime;
			_currentTime = updatedCurrentTime;
			foreach (CelestialBody celestialBody in _celestialBodyReferences)
			{
				celestialBody.UpdatePlanetSpacePosition((float)deltaTime);
				celestialBody.UpdatePosition(this, deltaTime);
			}
		}

		public void Set(World world, Transform sunRoot, Transform planetsRoot, double time)
		{
			_world = world;
			_sunRoot = sunRoot;
			_planetsRoot = planetsRoot;
			_currentTime = time;
			_timeCorrection = HiResTime.Milliseconds / 1000.0 - time;
		}

		/// <summary>
		/// 	Gets the most local or nearest celestial body based on our solar system position.
		/// </summary>
		public CelestialBody GetParentCelestialBody(Vector3D worldPosition)
		{
			CelestialBody dominant = null;
			double dominantInfluence = double.PositiveInfinity;
			CelestialBody nearest = null;
			double nearestDistance = double.PositiveInfinity;
			foreach (CelestialBody celestialBody in _celestialBodyReferences)
			{
				double distance = (worldPosition - celestialBody.Position).Magnitude;
				if (distance < nearestDistance)
				{
					nearestDistance = distance;
					nearest = celestialBody;
				}

				double influence = celestialBody.Orbit.GravityInfluenceRadius;
				if (distance <= influence && influence < dominantInfluence)
				{
					dominantInfluence = influence;
					dominant = celestialBody;
				}
			}

			return dominant ?? nearest;
		}

		public void CenterPlanets()
		{
			if ((MyPlayer.Instance.Parent is not SpaceObjectVessel
				    ? MyPlayer.Instance.Parent
				    : ((SpaceObjectVessel)MyPlayer.Instance.Parent).MainVessel) is not ArtificialBody playerVessel)
			{
				return;
			}

			Vector3D playerVesselPosition = _world.LocalToWorldPosition(playerVessel.transform.position);
			if (_celestialBodyReferences.Count > 0)
			{
				foreach (CelestialBody celestialBody in _celestialBodyReferences)
				{
					double distanceFromPlayer = Vector3D.Distance(celestialBody.Position, playerVesselPosition) - celestialBody.Orbit.Radius;
					if (distanceFromPlayer <= PlanetsToShowDistance)
					{
						if (celestialBody.PlanetsSpaceGameObject == null)
						{
							celestialBody.CreatePlanetsSpaceGameObject(_planetsRoot);
							celestialBody.SetPlanetSpacePosition(
								((celestialBody.Position - playerVesselPosition) / 1000000.0).ToVector3(), forceChange: true);
						}
						else
						{
							celestialBody.SetPlanetSpacePosition(
								((celestialBody.Position - playerVesselPosition) / 1000000.0).ToVector3(), forceChange: false);
						}
					}
					else if (celestialBody.PlanetsSpaceGameObject != null && distanceFromPlayer > PlanetsToDestroyDistance)
					{
						celestialBody.DestroyPlanetsSpaceGameObject();
					}
				}
			}
		}

		public void LoadDataFromResources()
		{
			SolarSystemData solarSystemData = JsonSerialiser.LoadResource<SolarSystemData>("Data/SolarSystem");
			foreach (CelestialBodyData data in solarSystemData.CelestialBodies)
			{
				CelestialBody celestialBody = new CelestialBody(data.GUID);

				celestialBody.Set(data.ParentGUID != -1 ? FindCelestialBody(data.ParentGUID) : null,
					data.Name,
					data.Mass,
					data.Radius * World.CELESTIAL_BODY_RADIUS_MULTIPLIER,
					data.RotationPeriod,
					data.Eccentricity,
					data.SemiMajorAxis,
					data.Inclination,
					data.ArgumentOfPeriapsis,
					data.LongitudeOfAscendingNode,
					data.PlanetsPrefabPath,
					CurrentTime);

				if (!string.IsNullOrEmpty(data.NavigationPrefabPath))
				{
					_map.InitialiseMapObject(celestialBody, data);
				}

				celestialBody.AsteroidGasBurstDmgMultiplier = data.AsteroidGasBurstDmgMultiplier;
				celestialBody.ScanningSensitivityModifierValues = data.ScanningSensitivityModifierValues;
				celestialBody.RadarSignatureModifierValues = data.RadarSignatureModifierValues;
				AddCelestialBody(celestialBody);
			}

			CalculatePositionsAfterTime(CurrentTime);
		}
	}
}
