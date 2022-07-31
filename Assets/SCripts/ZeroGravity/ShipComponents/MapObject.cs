using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Math;
using ZeroGravity.Objects;

namespace ZeroGravity.ShipComponents
{
	public abstract class MapObject : MonoBehaviour
	{
		public CelestialBodyData cbd;

		public IMapMainObject MainObject;

		public Transform Position;

		public Transform Visual;

		public Transform Orbits;

		public LineRenderer MyOrbitRenderer;

		public int NumberOfOrbitPositions = 750;

		private double lastOrbitUpdateTime = -1.0;

		[SerializeField]
		private Material orbitMaterial;

		public AnimationCurve OrbitFadeCurve;

		public AnimationCurve SelectedOrbitFadeCurve;

		public float orbitAlpha;

		public Transform OrbitPlane;

		public float OrbitFadeStart = 2f;

		public float OrbitFadeEnd = 2f;

		public float VisulFadeMultiplier = 0.5f;

		public bool IsDragging;

		public Transform DraggingParameter;

		public bool CanBeDragged;

		public GameObject DistressVisual;

		[HideInInspector]
		public SphereCollider PositionCollider;

		public Renderer ObjectVisibilityBackground;

		public GameObject NewObjectVisibility;

		public float NewObjectVisibilityDuration;

		protected Color newObjectColorFadeIncrement;

		[NonSerialized]
		public float CreationTime;

		[HideInInspector]
		public Map Map
		{
			get
			{
				return Client.Instance.Map;
			}
		}

		public virtual string Name
		{
			get
			{
				return MainObject.Name;
			}
			set
			{
			}
		}

		public double ObjectScale
		{
			get
			{
				return Map.Scale / 149597870700.0;
			}
		}

		public virtual Vector3D TruePosition
		{
			get
			{
				return MainObject.Position;
			}
		}

		public virtual float Radius
		{
			get
			{
				return (float)MainObject.Radius;
			}
		}

		public Vector3 ObjectPosition
		{
			get
			{
				return ((TruePosition - Map.Focus) * ObjectScale).ToVector3();
			}
		}

		public virtual double RadarSignature
		{
			get
			{
				return 0.0;
			}
		}

		public virtual OrbitParameters Orbit
		{
			get
			{
				if (MainObject != null)
				{
					return MainObject.Orbit;
				}
				return null;
			}
		}

		public abstract Sprite Icon { get; set; }

		public abstract string Description { get; set; }

		public RadarVisibilityType RadarVisibilityType
		{
			get
			{
				if (MainObject == null)
				{
					return RadarVisibilityType.AlwaysVisible;
				}
				return MainObject.RadarVisibilityType;
			}
		}

		public virtual bool IsVisibleOnMap
		{
			get
			{
				return true;
			}
		}

		public virtual Color OrbitColor
		{
			get
			{
				if (Map != null)
				{
					if (Map.SelectedObject == this)
					{
						return Colors.Cyan;
					}
					return Colors.White;
				}
				return Colors.White;
			}
		}

		public float VesselExposureDamage
		{
			get
			{
				if (MainObject != null)
				{
					return Client.Instance.GetVesselExposureDamage(MainObject.Orbit.Position.Magnitude);
				}
				return 0f;
			}
		}

		public float PlayerExposureDamage
		{
			get
			{
				return Client.Instance.GetPlayerExposureDamage(MainObject.Orbit.Position.Magnitude);
			}
		}

		private void OnEnable()
		{
			SetOrbit();
			if (DistressVisual != null)
			{
				DistressVisual.SetActive(RadarVisibilityType == RadarVisibilityType.Distress);
			}
			if (this == Map.MyShip)
			{
				Position.GetComponent<SphereCollider>().radius = 0.15f;
			}
		}

		protected virtual void Start()
		{
			CreationTime = Time.time;
			CreateVisual();
			UpdateVisibility();
			PositionCollider = Position.GetComponent<SphereCollider>();
		}

		protected virtual void Update()
		{
			if (!Map.gameObject.activeInHierarchy)
			{
				return;
			}
			UpdateObject();
			UpdateOrbitColors();
			UpdateOrbitPlane();
			if (NewObjectVisibility != null)
			{
				Renderer component = NewObjectVisibility.GetComponent<Renderer>();
				if (component != null)
				{
					Color color = component.material.GetColor("_Tint");
					component.material.SetColor("_Tint", color - newObjectColorFadeIncrement * Time.deltaTime);
				}
			}
		}

		public abstract void UpdateObject();

		public abstract void CreateVisual();

		public virtual void UpdateOrbitColors()
		{
			if (Orbit != null)
			{
				float num = 8.45228E+09f / (float)Orbit.DistanceAtTrueAnomaly * Map.ClosestSunScale / OrbitFadeEnd;
				float num2 = 8.45228E+09f / (float)Orbit.DistanceAtTrueAnomaly * Map.ClosestSunScale / OrbitFadeStart;
				Color orbitColor = OrbitColor;
				if (Map.SelectedObject != this)
				{
					orbitAlpha = OrbitFadeCurve.Evaluate(((float)Map.Scale - num2) * 1f / (num - num2));
				}
				else
				{
					orbitAlpha = SelectedOrbitFadeCurve.Evaluate(((float)Map.Scale - num2) * 1f / (num - num2));
				}
				orbitColor.a = orbitAlpha;
				MyOrbitRenderer.startColor = orbitColor;
				MyOrbitRenderer.endColor = orbitColor;
				if (Map.Scale < (double)(num2 * VisulFadeMultiplier) && Visual.gameObject.activeInHierarchy)
				{
					Visual.gameObject.SetActive(false);
					Position.GetComponent<SphereCollider>().enabled = false;
				}
				else if (Map.Scale > (double)(num2 * VisulFadeMultiplier) && !Visual.gameObject.activeInHierarchy)
				{
					Visual.gameObject.SetActive(true);
					Position.GetComponent<SphereCollider>().enabled = true;
				}
			}
		}

		public virtual void SetOrbit()
		{
			List<Vector3D> list = null;
			list = Orbit.GetOrbitPositions(NumberOfOrbitPositions, 60.0);
			MyOrbitRenderer.positionCount = list.Count;
			if (list != null && list.Count > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					MyOrbitRenderer.SetPosition(i, list[i].ToVector3());
				}
				MyOrbitRenderer.SetPosition(NumberOfOrbitPositions - 1, list[0].ToVector3());
			}
			MyOrbitRenderer.transform.SetParent(Orbits);
			MyOrbitRenderer.startColor = OrbitColor;
			MyOrbitRenderer.endColor = OrbitColor;
		}

		public void UpdateOrbitPlane()
		{
			if (!(OrbitPlane == null))
			{
				QuaternionD rotation = QuaternionD.Identity;
				Vector3D centerPosition = Vector3D.Zero;
				Orbit.GetOrbitPlaneData(out rotation, out centerPosition);
				OrbitPlane.localPosition = (centerPosition * ObjectScale).ToVector3();
				OrbitPlane.localRotation = rotation.ToQuaternion();
				OrbitPlane.localScale = (new Vector3D(Orbit.SemiMinorAxis * 2.0, Orbit.SemiMinorAxis * 2.0, Orbit.SemiMajorAxis * 2.0) * ObjectScale).ToVector3();
			}
		}

		public virtual void StartDragging(GameObject dragObject, Ray ray)
		{
			IsDragging = true;
			DraggingParameter = dragObject.transform;
		}

		public virtual void Dragging(Ray ray)
		{
		}

		public virtual void StopDragging()
		{
			IsDragging = false;
		}

		public virtual void UpdateVisibility()
		{
			base.gameObject.Activate(IsVisibleOnMap);
			if (!IsVisibleOnMap && Map.SelectedObject == this)
			{
				Map.SelectedObject = null;
			}
			if (DistressVisual != null)
			{
				DistressVisual.Activate(RadarVisibilityType == RadarVisibilityType.Distress);
			}
		}

		private void OnDestroy()
		{
			if (Map.SelectedObject == this)
			{
				Map.SelectMapObject(null);
				Map.Panel.HideHoverInfo();
			}
		}
	}
}
