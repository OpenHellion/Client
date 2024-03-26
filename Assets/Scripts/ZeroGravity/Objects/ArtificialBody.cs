using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.LevelDesign;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.Objects
{
	public class ArtificialBody : SpaceObject
	{
		public ManeuverData Maneuver;

		public bool ManeuverExited;

		public VesselDestructionEffects DestructionEffects;

		private RadarVisibilityType _radarVisibilityType;

		[NonSerialized] public ArtificialBody StabilizeToTargetObj;

		[NonSerialized] public Vector3D StabilizationOffset;

		public readonly HashSet<ArtificialBody> StabilizedChildren = new HashSet<ArtificialBody>();

		public virtual OrbitParameters Orbit { get; set; }

		public virtual CelestialBody ParentCelestialBody => Orbit.Parent.CelestialBody;

		public double Radius { get; protected set; }

		public override Vector3D Velocity => Orbit.Velocity;

		public override Vector3D Position => Orbit.Position;

		public RadarVisibilityType RadarVisibilityType
		{
			get
			{
				if (IsAlwaysVisible)
				{
					return RadarVisibilityType.AlwaysVisible;
				}

				return !IsDistressSignalActive ? _radarVisibilityType : RadarVisibilityType.Distress;
			}
			set
			{
				_radarVisibilityType = value;
				if (this is IMapMainObject &&
				    World.Map.AllMapObjects.TryGetValue(this as IMapMainObject, out MapObject value2))
				{
					value2.UpdateVisibility();
				}
			}
		}

		public virtual bool IsDistressSignalActive => false;

		public virtual bool IsAlwaysVisible => false;

		public virtual double RadarSignature => 0.0;

		public bool IsStabilized => StabilizeToTargetObj is not null;

		public static ArtificialBody CreateDummy(ObjectTransform trans)
		{
			switch (trans.Type)
			{
				case SpaceObjectType.Ship:
					return Ship.Create(trans.GUID, null, trans, false);
				case SpaceObjectType.PlayerPivot:
				case SpaceObjectType.DynamicObjectPivot:
				case SpaceObjectType.CorpsePivot:
					return Pivot.Create(trans.Type, trans, false);
				case SpaceObjectType.Asteroid:
					return Asteroid.Create(trans, null, false);
				default:
					Debug.LogError("Unknown artificial body type " + trans.Type + " " + trans.GUID);
					return null;
			}
		}

		// Called by the Ship, Asteroid, and Pivot classes. The actual creating is done here.
		protected static ArtificialBody CreateImpl(SpaceObjectType type, long guid, ObjectTransform trans, bool isMainObject)
		{
			GameObject gameObject = new GameObject(type + "_" + guid);
			ArtificialBody artificialBody;
			switch (type)
			{
				case SpaceObjectType.Ship:
					artificialBody = gameObject.AddComponent<Ship>();
					break;
				case SpaceObjectType.Asteroid:
					artificialBody = gameObject.AddComponent<Asteroid>();
					break;
				case SpaceObjectType.PlayerPivot:
				case SpaceObjectType.DynamicObjectPivot:
				case SpaceObjectType.CorpsePivot:
					artificialBody = gameObject.AddComponent<Pivot>();
					break;
				case SpaceObjectType.Station:
					artificialBody = gameObject.AddComponent<Station>();
					break;
				default:
					Debug.LogError("Cannot create artificial body of invalid type:" + type);
					return null;
			}

			if (type is SpaceObjectType.Ship or SpaceObjectType.Asteroid)
			{
				artificialBody.gameObject.SetActive(false);
			}

			artificialBody.Guid = guid;
			artificialBody.Orbit = new OrbitParameters();
			artificialBody.Orbit.SetArtificialBody(artificialBody);
			artificialBody.Radius = 30.0;
			if (trans.Orbit != null)
			{
				artificialBody.Orbit.ParseNetworkData(World, trans.Orbit);
			}
			else if (trans.Realtime != null)
			{
				artificialBody.Orbit.ParseNetworkData(World, trans.Realtime);
			}
			else if (trans.StabilizeToTargetGUID is > 0)
			{
				ArtificialBody artificialBody2 =
					World.SolarSystem.GetArtificialBody(trans.StabilizeToTargetGUID.Value);
				if (artificialBody2 is not null)
				{
					artificialBody.Orbit.CopyDataFrom(artificialBody2.Orbit, World.SolarSystem.CurrentTime, true);
				}
			}
			else
			{
				Debug.LogError("Artificial bodies should always have orbit or realtime data.");
			}

			artificialBody.Forward = trans.Forward?.ToVector3() ?? Vector3.forward;
			artificialBody.Up = trans.Up?.ToVector3() ?? Vector3.up;
			artificialBody.TransferableObjectsRoot = new GameObject("TransferableObjectsRoot");
			artificialBody.TransferableObjectsRoot.transform.parent = artificialBody.transform;
			artificialBody.TransferableObjectsRoot.transform.Reset();
			artificialBody.ConnectedObjectsRoot = new GameObject("ConnectedObjectsRoot");
			artificialBody.ConnectedObjectsRoot.transform.parent = artificialBody.transform;
			artificialBody.ConnectedObjectsRoot.transform.Reset();
			if (type is SpaceObjectType.Asteroid or SpaceObjectType.Ship or SpaceObjectType.Station)
			{
				artificialBody.GeometryPlaceholder = new GameObject("GeometryPlaceholder");
				artificialBody.GeometryPlaceholder.transform.parent = artificialBody.transform;
				artificialBody.GeometryPlaceholder.transform.Reset();
				artificialBody.GeometryRoot = new GameObject("GeometryRoot");
				GeometryRoot geometryRoot = artificialBody.GeometryRoot.AddComponent<GeometryRoot>();
				artificialBody.GeometryRoot.transform.parent = artificialBody.GeometryPlaceholder.transform;
				artificialBody.GeometryRoot.transform.Reset();
				geometryRoot.MainObject = artificialBody;
				artificialBody.TransferableObjectsRoot.transform.parent = artificialBody.GeometryPlaceholder.transform;
				artificialBody.TransferableObjectsRoot.transform.Reset();
			}

			if (isMainObject)
			{
				artificialBody.transform.parent = null;
				artificialBody.SetTargetPositionAndRotation(Vector3.zero, artificialBody.Forward, artificialBody.Up,
					true);
				artificialBody.transform.Reset();
			}
			else
			{
				artificialBody.transform.parent = World.ShipExteriorRoot.transform;
				artificialBody.SetTargetPositionAndRotation(
					(artificialBody.Position - MyPlayer.Instance.Parent.Position).ToVector3(), artificialBody.Forward,
					artificialBody.Up, true);
				if ((artificialBody.Position - MyPlayer.Instance.Parent.Position).SqrMagnitude < 100000000.0)
				{
					artificialBody.LoadGeometry();
				}
				else if (type is SpaceObjectType.Asteroid or SpaceObjectType.Ship or SpaceObjectType.Station)
				{
					artificialBody.RequestSpawn();
				}
			}

			World.SolarSystem.AddArtificialBody(artificialBody);
			return artificialBody;
		}

		public override void DestroyGeometry()
		{
			base.DestroyGeometry();
			if (GeometryRoot is not null)
			{
				if (this is SpaceObjectVessel)
				{
					ZeroOcclusion.DestroyOcclusionObjectsFor(this as SpaceObjectVessel);
				}

				foreach (Transform child in GeometryRoot.transform.GetChildren())
				{
					if (child is not null)
					{
						Destroy(child.gameObject);
					}
				}

				GeometryRoot.transform.parent = GeometryPlaceholder.transform;
				GeometryRoot.transform.Reset();
			}

			if (ArtificialRigidbody is not null)
			{
				Destroy(ArtificialRigidbody);
			}

			ArtificialRigidbody = null;
			IsDummyObject = true;
		}

		public void UpdateOrbitPosition(double time, bool resetTime = false)
		{
			if (Maneuver != null || StabilizeToTargetObj is not null)
			{
				return;
			}

			if (resetTime)
			{
				Orbit.ResetOrbit(time);
			}
			else
			{
				Orbit.UpdateOrbit(time);
			}

			if (StabilizedChildren is not { Count: > 0 })
			{
				return;
			}

			foreach (ArtificialBody stabilizedChild in StabilizedChildren)
			{
				stabilizedChild.UpdateStabilizedPosition();
			}
		}

		public override void OnSubscribe()
		{
		}

		public override void OnUnsubscribe()
		{
			TransferableObjectsRoot.DestroyAll(true);
		}

		public void UpdateStabilizedPosition()
		{
			if (!(StabilizeToTargetObj == null))
			{
				Orbit.CopyDataFrom(StabilizeToTargetObj.Orbit, World.SolarSystem.CurrentTime, true);
				Orbit.RelativePosition += StabilizationOffset;
				Orbit.InitFromCurrentStateVectors(World.SolarSystem.CurrentTime);
			}
		}

		public virtual void OnStabilizationChanged(bool isStabilized)
		{
		}

		public void StabilizeToTarget(long guid, Vector3D stabilizationOffset)
		{
			if (StabilizeToTargetObj != null && StabilizeToTargetObj.Guid != guid)
			{
				StabilizeToTargetObj.StabilizedChildren.Remove(this);
			}

			StabilizeToTargetObj = null;
			if (guid > 0)
			{
				StabilizeToTargetObj = World.SolarSystem.GetArtificialBody(guid);
			}

			if (StabilizeToTargetObj != null)
			{
				StabilizationOffset = stabilizationOffset;
				StabilizeToTargetObj.StabilizedChildren.Add(this);
				OnStabilizationChanged(true);
			}
		}

		public void DisableStabilization()
		{
			if (!(StabilizeToTargetObj == null))
			{
				StabilizeToTargetObj.StabilizedChildren.Remove(this);
				StabilizeToTargetObj = null;
				OnStabilizationChanged(false);
			}
		}

		public void SendDistressCall(bool isActive)
		{
			World.SendDistressCall(this, isActive);
		}
	}
}
