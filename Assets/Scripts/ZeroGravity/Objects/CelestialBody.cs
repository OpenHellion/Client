using UnityEngine;
using ZeroGravity.Math;

namespace ZeroGravity.Objects
{
	public class CelestialBody : IMapMainObject
	{
		public GameObject PlanetsSpaceGameObject;

		public GameObject PlanetsSpaceGameObjectVisual;

		public GameObject SunSpaceGameObject;

		public string PrefabPath;

		public float AsteroidGasBurstDmgMultiplier;

		public float[] ScanningSensitivityModifierValues;

		public float[] RadarSignatureModifierValues;

		private Vector3 targetPosition;

		public OrbitParameters Orbit { get; set; }

		public long GUID { get; set; }

		public string Name { get; set; }

		public double Radius { get; set; }

		public RadarVisibilityType RadarVisibilityType
		{
			get
			{
				return RadarVisibilityType.Visible;
			}
		}

		public Vector3D Position
		{
			get
			{
				return Orbit.Position;
			}
		}

		public CelestialBody ParentCelesitalBody
		{
			get
			{
				if (Orbit.Parent != null && Orbit.Parent.CelestialBody != null)
				{
					return Orbit.Parent.CelestialBody;
				}
				return null;
			}
			set
			{
			}
		}

		public bool IsDummyObject
		{
			get
			{
				return true;
			}
		}

		public double MaxDistanceFromSun
		{
			get
			{
				double num = Orbit.ApoapsisDistance;
				OrbitParameters parent = Orbit.Parent;
				while (((parent != null) ? parent.Parent : null) != null)
				{
					num = parent.ApoapsisDistance + num;
					parent = parent.Parent;
				}
				return num;
			}
		}

		public CelestialBody(long guid)
		{
			GUID = guid;
			Orbit = new OrbitParameters();
			Orbit.SetCelestialBody(this);
		}

		public bool Set(CelestialBody parent, string name, double mass, double radius, double rotPeriod, double eccentricity, double semiMajorAxis, double inclination, double argumentOfPeriapsis, double longitudeOfAscendingNode, string prefabPath, double solarSystemTime)
		{
			PrefabPath = prefabPath;
			Name = name;
			Radius = radius;
			if (parent != null)
			{
				Orbit.InitFromElements(parent.Orbit, mass, radius, rotPeriod, eccentricity, semiMajorAxis, inclination, argumentOfPeriapsis, longitudeOfAscendingNode, 0.0, 0.0);
			}
			else
			{
				Orbit.InitFromElements(null, mass, radius, rotPeriod, eccentricity, semiMajorAxis, inclination, argumentOfPeriapsis, longitudeOfAscendingNode, 0.0, 0.0);
			}
			return Orbit.IsOrbitValid;
		}

		public void UpdatePosition(double timeDelta, bool resetTime = false)
		{
			if (Orbit.IsOrbitValid)
			{
				if (resetTime)
				{
					Orbit.ResetOrbit(timeDelta);
				}
				else
				{
					Orbit.UpdateOrbit(timeDelta);
				}
				if (PlanetsSpaceGameObjectVisual != null)
				{
					PlanetsSpaceGameObjectVisual.transform.localRotation = Quaternion.AngleAxis((float)Orbit.GetRotationAngle(Client.Instance.SolarSystem.CurrentTime), Vector3.up);
				}
			}
		}

		public void CreateSunSpaceGameObject(Transform sunRoot)
		{
			GameObject gameObject = Object.Instantiate(Resources.Load(PrefabPath) as GameObject);
			gameObject.transform.SetParent(sunRoot);
			gameObject.SetLayerRecursively("Sun");
			gameObject.name = Name;
			float num = (float)(Orbit.Radius / 149597870.7);
			gameObject.transform.localScale = new Vector3(num, num, num);
			SunSpaceGameObject = gameObject;
		}

		public void CreatePlanetsSpaceGameObject(Transform planetsRoot)
		{
			GameObject gameObject = new GameObject(Name);
			GameObject gameObject2 = Object.Instantiate(Resources.Load(PrefabPath) as GameObject);
			gameObject2.transform.parent = gameObject.transform;
			gameObject.SetLayerRecursively("Planets");
			gameObject.transform.SetParent(planetsRoot);
			gameObject2.name = Name + "_Visual";
			float num = (float)(Orbit.Radius * 2.0 / 1000000.0);
			gameObject2.transform.localScale = new Vector3(num, num, num);
			PlanetsSpaceGameObject = gameObject;
			PlanetsSpaceGameObjectVisual = gameObject2;
		}

		public void DestroyPlanetsSpaceGameObject()
		{
			Object.Destroy(PlanetsSpaceGameObject);
			PlanetsSpaceGameObject = null;
			PlanetsSpaceGameObjectVisual = null;
		}

		public void SetPlanetSpacePosition(Vector3 position, bool forceChange)
		{
			if (!(PlanetsSpaceGameObject == null))
			{
				if (forceChange)
				{
					PlanetsSpaceGameObject.transform.localPosition = position;
					targetPosition = position;
				}
				else
				{
					targetPosition = position;
				}
			}
		}

		public void UpdatePlanetSpacePosition(float deltaTime)
		{
			if (PlanetsSpaceGameObject != null)
			{
				PlanetsSpaceGameObject.transform.localPosition = Vector3.Lerp(PlanetsSpaceGameObject.transform.localPosition, targetPosition, deltaTime);
			}
		}

		public float GetRadarSignatureModifier(ArtificialBody ab)
		{
			if (RadarSignatureModifierValues == null)
			{
				return 1f;
			}
			double magnitude = (ab.Position - Position).Magnitude;
			double num = ((Orbit.GravityInfluenceRadius != double.PositiveInfinity) ? Orbit.GravityInfluenceRadius : Client.Instance.ExposureRange);
			return RadarSignatureModifierValues[(int)(MathHelper.Clamp(magnitude / num, 0.0, 1.0) * 99.0)];
		}

		public float GetScanningSensitivityModifier(ArtificialBody ab)
		{
			if (ScanningSensitivityModifierValues == null)
			{
				return 1f;
			}
			double magnitude = (ab.Position - Position).Magnitude;
			double num = ((Orbit.GravityInfluenceRadius != double.PositiveInfinity) ? Orbit.GravityInfluenceRadius : Client.Instance.ExposureRange);
			return ScanningSensitivityModifierValues[(int)(MathHelper.Clamp(magnitude / num, 0.0, 1.0) * 99.0)];
		}
	}
}
