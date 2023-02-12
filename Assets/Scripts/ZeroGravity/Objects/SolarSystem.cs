using System;
using System.Collections.Generic;
using System.Linq;
using OpenHellion.IO;
using Unity.Jobs;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Math;

namespace ZeroGravity.Objects
{
	public class SolarSystem : MonoBehaviour
	{
		public struct UpdateABPositionParallelJob : IJobParallelFor
		{
			public double TimeDelta;

			public void Execute(int i)
			{
				Client.Instance.SolarSystem.ArtificialBodies[i].UpdateOrbitPosition(TimeDelta);
			}
		}

		public const double VisibilityLimitDestroySqr = 225000000.0;

		public const double VisibilityLimitLoadSqr = 100000000.0;

		public const double DetailsLimitUnsubscribe = 6250000.0;

		public const double DetailsLimitSubscribe = 2250000.0;

		public const double RadarVisibilityDistance = 1000000000000.0;

		public const double SunScale = 149597870.7;

		public const double PlanetsScale = 1000000.0;

		private double currentTime;

		public bool AutoUpdate = true;

		private Transform sunRoot;

		private Transform planetsRoot;

		private double planetsToShowDistance = 10000000000.0;

		private double planetsToDestroyDistance = 11000000000.0;

		private List<CelestialBody> celesitalBodies = new List<CelestialBody>();

		[NonSerialized]
		public List<ArtificialBody> ArtificialBodies = new List<ArtificialBody>();

		private double timeCorrection;

		public double CurrentTime => currentTime;

		public float LastArtificialBodyListChangeTime { get; private set; }

		public void AddCelestialBody(CelestialBody body)
		{
			if (body.GUID == 1 && Client.IsGameBuild)
			{
				body.CreateSunSpaceGameObject(sunRoot);
			}
			celesitalBodies.Add(body);
		}

		public void RemoveCelestialBody(CelestialBody body)
		{
			celesitalBodies.Remove(body);
		}

		public CelestialBody FindCelestialBody(long guid)
		{
			return celesitalBodies.Find((CelestialBody m) => m.GUID == guid);
		}

		public CelestialBody FindCelestialBodyParent(Vector3D position)
		{
			CelestialBody result = celesitalBodies[0];
			double num = celesitalBodies[0].Position.DistanceSquared(position);
			for (int i = 1; i < celesitalBodies.Count; i++)
			{
				double num2 = celesitalBodies[i].Position.DistanceSquared(position);
				if (num2 < celesitalBodies[i].Orbit.GravityInfluenceRadiusSquared && num2 < num)
				{
					result = celesitalBodies[i];
					num = num2;
				}
			}
			return result;
		}

		public void AddArtificialBody(ArtificialBody body)
		{
			if (!ArtificialBodies.Contains(body))
			{
				ArtificialBodies.Add(body);
				LastArtificialBodyListChangeTime = Time.time;
			}
		}

		public ArtificialBody GetArtificialBody(long guid)
		{
			return ArtificialBodies.FirstOrDefault((ArtificialBody m) => m.GUID == guid);
		}

		public void RemoveArtificialBody(long GUID)
		{
			if (ArtificialBodies.RemoveAll((ArtificialBody m) => m.GUID == GUID) > 0)
			{
				LastArtificialBodyListChangeTime = Time.time;
			}
		}

		public void RemoveArtificialBody(ArtificialBody body)
		{
			if (body != null && ArtificialBodies.Remove(body))
			{
				LastArtificialBodyListChangeTime = Time.time;
			}
		}

		public void ArtificialBodiesVisiblityModified()
		{
			LastArtificialBodyListChangeTime = Time.time;
		}

		public List<ArtificialBody> GetArtificialBodies(CelestialBody parent)
		{
			return ArtificialBodies.Where((ArtificialBody m) => m.ParentCelesitalBody == parent).ToList();
		}

		public List<CelestialBody> GetCelestialBodies()
		{
			return celesitalBodies;
		}

		public CelestialBody GetSunCelestialBody()
		{
			return celesitalBodies[0];
		}

		public void CalculatePositionsAfterTime(double time)
		{
			currentTime = time;
			timeCorrection = (double)Time.realtimeSinceStartup - currentTime;
			foreach (CelestialBody celesitalBody in celesitalBodies)
			{
				celesitalBody.UpdatePosition(time, resetTime: true);
			}
			foreach (ArtificialBody artificialBody in ArtificialBodies)
			{
				artificialBody.UpdateOrbitPosition(time, resetTime: true);
			}
		}

		public void UpdatePositions(double timeDelta)
		{
			double num = (double)Time.realtimeSinceStartup - timeCorrection;
			timeDelta = num - currentTime;
			currentTime = num;
			foreach (CelestialBody celesitalBody in celesitalBodies)
			{
				celesitalBody.UpdatePlanetSpacePosition((float)timeDelta);
				celesitalBody.UpdatePosition(timeDelta);
			}
			UpdateABPositionParallelJob jobData = default(UpdateABPositionParallelJob);
			jobData.TimeDelta = timeDelta;
			jobData.Schedule(ArtificialBodies.Count, 10).Complete();
		}

		public void Set(Transform sunRoot, Transform planetsRoot, double currentTime)
		{
			this.sunRoot = sunRoot;
			this.planetsRoot = planetsRoot;
			this.currentTime = currentTime;
			timeCorrection = (double)Time.time - currentTime;
		}

		public void CenterPlanets()
		{
			ArtificialBody artificialBody = ((!(MyPlayer.Instance.Parent is SpaceObjectVessel)) ? MyPlayer.Instance.Parent : (MyPlayer.Instance.Parent as SpaceObjectVessel).MainVessel) as ArtificialBody;
			if (artificialBody == null)
			{
				return;
			}
			float num = Time.realtimeSinceStartup - Client.Instance.LastMovementMessageTime;
			Vector3D position = artificialBody.Position;
			if (celesitalBodies.Count > 0)
			{
				foreach (CelestialBody celesitalBody in celesitalBodies)
				{
					double num2 = Vector3D.Distance(celesitalBody.Position, position) - celesitalBody.Orbit.Radius;
					if (num2 <= planetsToShowDistance)
					{
						if (celesitalBody.PlanetsSpaceGameObject == null)
						{
							celesitalBody.CreatePlanetsSpaceGameObject(planetsRoot);
							celesitalBody.SetPlanetSpacePosition(((celesitalBody.Position - position) / 1000000.0).ToVector3(), forceChange: true);
						}
						else
						{
							celesitalBody.SetPlanetSpacePosition(((celesitalBody.Position - position) / 1000000.0).ToVector3(), forceChange: false);
						}
					}
					else if (celesitalBody.PlanetsSpaceGameObject != null && num2 > planetsToDestroyDistance)
					{
						celesitalBody.DestroyPlanetsSpaceGameObject();
					}
				}
			}
			foreach (ArtificialBody artificialBody2 in ArtificialBodies)
			{
				if (!(artificialBody2 != null) || artificialBody2.IsMainObject || (artificialBody2 is SpaceObjectVessel && !(artificialBody2 as SpaceObjectVessel).IsMainVessel))
				{
					continue;
				}
				Vector3D value = artificialBody2.Position - position;
				double sqrMagnitude = value.SqrMagnitude;
				if (sqrMagnitude < 2250000.0 && !artificialBody2.IsSubscribedTo && artificialBody2.SceneObjectsLoaded)
				{
					if (!artificialBody2.DelayedSubscribeRequested && artificialBody2.SceneObjectsLoaded)
					{
						artificialBody2.DelayedSubscribeRequested = true;
						artificialBody2.Invoke(artificialBody2.Subscribe, MathHelper.RandomRange(0f, 3f));
					}
				}
				else if (sqrMagnitude > 6250000.0 && artificialBody2.IsSubscribedTo)
				{
					artificialBody2.Unsubscribe();
				}
				else if (sqrMagnitude < 100000000.0 && artificialBody2.IsDummyObject)
				{
					artificialBody2.LoadGeometry();
				}
				else if (sqrMagnitude > 225000000.0 && !artificialBody2.IsDummyObject)
				{
					artificialBody2.DestroyGeometry();
				}
				if (sqrMagnitude < 225000000.0)
				{
					if (!artificialBody2.gameObject.activeInHierarchy)
					{
						artificialBody2.gameObject.SetActive(value: true);
					}
					Vector3 vector = value.ToVector3();
					if ((vector - artificialBody2.transform.localPosition).sqrMagnitude < 10000f)
					{
						if (Client.VESSEL_TRANSLATION_LERP_UNCLAMPED)
						{
							artificialBody2.transform.localPosition = Vector3.Lerp(Vector3.LerpUnclamped(vector, artificialBody2.transform.localPosition, Time.deltaTime), Vector3.LerpUnclamped(artificialBody2.transform.localPosition, vector, Time.deltaTime), Client.VESSEL_TRANSLATION_LERP_VALUE);
						}
						else
						{
							artificialBody2.transform.localPosition = Vector3.Lerp(Vector3.Lerp(vector, artificialBody2.transform.localPosition, Time.deltaTime), Vector3.Lerp(artificialBody2.transform.localPosition, vector, Time.deltaTime), Client.VESSEL_TRANSLATION_LERP_VALUE);
						}
					}
					else
					{
						artificialBody2.transform.localPosition = vector;
					}
					if (artificialBody2 is Ship && (artificialBody2 as Ship).WarpEndEffectTask != null)
					{
						(artificialBody2 as Ship).WarpEndEffectTask.RunSynchronously();
					}
				}
				else if (artificialBody2.IsInVisibilityRange)
				{
					if (artificialBody2.gameObject.activeInHierarchy)
					{
						artificialBody2.gameObject.SetActive(value: false);
					}
					artificialBody2.IsInVisibilityRange = false;
				}
			}
			if (artificialBody.ManeuverExited)
			{
				artificialBody.ManeuverExited = false;
			}
		}

		public void LoadDataFromResources()
		{
			SolarSystemData solarSystemData = JsonSerialiser.LoadResource<SolarSystemData>("Data/SolarSystem");
			foreach (CelestialBodyData data in solarSystemData.CelestialBodies)
			{
				CelestialBody celestialBody = new CelestialBody(data.GUID);

				celestialBody.Set((data.ParentGUID != -1) ? FindCelestialBody(data.ParentGUID) : null,
				data.Name,
				data.Mass,
				data.Radius * Client.CELESTIAL_BODY_RADIUS_MULTIPLIER,
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
					Client.Instance.Map.InitializeMapObject(celestialBody, data);
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
