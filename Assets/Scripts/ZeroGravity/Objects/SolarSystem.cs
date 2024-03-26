using System;
using System.Collections.Generic;
using System.Linq;
using OpenHellion;
using OpenHellion.IO;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Math;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.Objects
{
	public class SolarSystem : MonoBehaviour
	{
		private struct UpdateAbPositionParallelJob : IJobParallelFor
		{
			[NonSerialized] public double TimeDelta;

			public void Execute(int i)
			{
				ArtificialBodyReferences[i].UpdateOrbitPosition(TimeDelta);
			}
		}

		public const double VisibilityLimitDestroySqr = 225000000.0;

		public const double VisibilityLimitLoadSqr = 100000000.0;

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

		private readonly List<CelestialBody> _celestialBodyReferences = new List<CelestialBody>();

		// Is static because it is used in UpdateAbPositionParallelJob. Can this be done in any other way?
		public static readonly List<ArtificialBody> ArtificialBodyReferences = new List<ArtificialBody>();

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

		public void AddArtificialBody(ArtificialBody body)
		{
			if (!ArtificialBodyReferences.Contains(body))
			{
				ArtificialBodyReferences.Add(body);
			}
			else
			{
				Debug.LogWarning("Tried to add reference to artificial body to system, but a reference already exists.");
			}
		}

		public ArtificialBody GetArtificialBody(long guid)
		{
			return ArtificialBodyReferences.FirstOrDefault((ArtificialBody m) => m.Guid == guid);
		}

		public void RemoveArtificialBody(long guid)
		{
			if (ArtificialBodyReferences.RemoveAll((ArtificialBody m) => m.Guid == guid) > 0)
			{
			}
		}

		public void RemoveArtificialBody(ArtificialBody body)
		{
			if (body is not null && ArtificialBodyReferences.Remove(body))
			{
			}
		}

		public void ArtificialBodiesVisibilityModified()
		{
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

			foreach (ArtificialBody artificialBody in ArtificialBodyReferences)
			{
				artificialBody.UpdateOrbitPosition(time, resetTime: true);
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

			UpdateAbPositionParallelJob jobData = default(UpdateAbPositionParallelJob);
			jobData.TimeDelta = deltaTime;
			jobData.Schedule(ArtificialBodyReferences.Count, 10).Complete();
		}

		public void Set(Transform sunRoot, Transform planetsRoot, double time)
		{
			_sunRoot = sunRoot;
			_planetsRoot = planetsRoot;
			_currentTime = time;
			_timeCorrection = HiResTime.Milliseconds / 1000.0 - time;
		}

		public void CenterPlanets()
		{
			if ((MyPlayer.Instance.Parent is not SpaceObjectVessel
				    ? MyPlayer.Instance.Parent
				    : ((SpaceObjectVessel)MyPlayer.Instance.Parent).MainVessel) is not ArtificialBody playerVessel)
			{
				return;
			}

			Vector3D playerVesselPosition = playerVessel.Position;
			if (_celestialBodyReferences.Count > 0)
			{
				foreach (CelestialBody celestialBody in _celestialBodyReferences)
				{
					double distanceFromPlayer = Vector3D.Distance(celestialBody.Position, playerVesselPosition) - celestialBody.Orbit.Radius;
					if (distanceFromPlayer <= PlanetsToShowDistance)
					{
						if (celestialBody.PlanetsSpaceGameObject is null)
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
					else if (celestialBody.PlanetsSpaceGameObject is not null && distanceFromPlayer > PlanetsToDestroyDistance)
					{
						celestialBody.DestroyPlanetsSpaceGameObject();
					}
				}
			}

			foreach (ArtificialBody artificialBody in ArtificialBodyReferences)
			{
				if (artificialBody is null || artificialBody.IsMainObject ||
				    artificialBody is SpaceObjectVessel { IsMainVessel: false })
				{
					continue;
				}

				Vector3D value = artificialBody.Position - playerVesselPosition;
				double sqrMagnitude = value.SqrMagnitude;
				if (sqrMagnitude < 2250000.0 && !artificialBody.IsSubscribedTo && artificialBody.SceneObjectsLoaded)
				{
					if (!artificialBody.DelayedSubscribeRequested && artificialBody.SceneObjectsLoaded)
					{
						artificialBody.DelayedSubscribeRequested = true;
						artificialBody.Invoke(artificialBody.Subscribe, MathHelper.RandomRange(0f, 3f));
					}
				}
				else if (sqrMagnitude > 6250000.0 && artificialBody.IsSubscribedTo)
				{
					artificialBody.Unsubscribe();
				}
				else if (sqrMagnitude < 100000000.0 && artificialBody.IsDummyObject)
				{
					artificialBody.LoadGeometry();
				}
				else if (sqrMagnitude > 225000000.0 && !artificialBody.IsDummyObject)
				{
					artificialBody.DestroyGeometry();
				}

				if (sqrMagnitude < 225000000.0)
				{
					if (!artificialBody.gameObject.activeInHierarchy)
					{
						artificialBody.gameObject.SetActive(value: true);
					}

					Vector3 vector = value.ToVector3();
					if ((vector - artificialBody.transform.localPosition).sqrMagnitude < 10000f)
					{
						if (World.VESSEL_TRANSLATION_LERP_UNCLAMPED)
						{
							var localPosition = artificialBody.transform.localPosition;
							localPosition = Vector3.Lerp(
								Vector3.LerpUnclamped(vector, localPosition, Time.deltaTime),
								Vector3.LerpUnclamped(localPosition, vector, Time.deltaTime),
								World.VESSEL_TRANSLATION_LERP_VALUE);
							artificialBody.transform.localPosition = localPosition;
						}
						else
						{
							var localPosition = artificialBody.transform.localPosition;
							localPosition = Vector3.Lerp(
								Vector3.Lerp(vector, localPosition, Time.deltaTime),
								Vector3.Lerp(localPosition, vector, Time.deltaTime),
								World.VESSEL_TRANSLATION_LERP_VALUE);
							artificialBody.transform.localPosition = localPosition;
						}
					}
					else
					{
						artificialBody.transform.localPosition = vector;
					}

					if (artificialBody is Ship { WarpEndEffectTask: not null } ship)
					{
						ship.WarpEndEffectTask.RunSynchronously();
					}
				}
				else if (artificialBody.IsInVisibilityRange)
				{
					if (artificialBody.gameObject.activeInHierarchy)
					{
						artificialBody.gameObject.SetActive(value: false);
					}

					artificialBody.IsInVisibilityRange = false;
				}
			}

			if (playerVessel.ManeuverExited)
			{
				playerVessel.ManeuverExited = false;
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

		private void OnDestroy()
		{
			ArtificialBodyReferences.Clear();
		}
	}
}
