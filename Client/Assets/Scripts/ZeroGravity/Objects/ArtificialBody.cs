using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class ArtificialBody : SpaceObject
	{
		public ManeuverData Maneuver;

		public bool ManeuverExited;

		public VesselDestructionEffects DestructionEffects;

		public Rigidbody ArtificialRigidbody;

		[NonSerialized] public ArtificialBody StabilizeToTargetObj;

		[NonSerialized] public Vector3 StabilizationOffset;

		public readonly HashSet<ArtificialBody> StabilizedChildren = new HashSet<ArtificialBody>();

		public double Radius { get; protected set; }

		[NonSerialized] private Vector3 _velocity;

		[NonSerialized] private Vector3 _angularVelocity;

		public override Vector3 Velocity => _velocity;

		public Vector3 AngularVelocity => _angularVelocity;

		public virtual bool IsDistressSignalActive { get; internal set; }

		public virtual bool IsAlwaysVisible { get; internal set; }

		public virtual double RadarSignature { get; set; } = 0.0;

		public bool IsStabilized => StabilizeToTargetObj != null;

		// Called by the Ship, Asteroid, and Pivot classes by their Create methods.
		protected static ArtificialBody InitialiseArtificialBody(long guid, SpaceObjectType type, Vector3 position, Quaternion rotation)
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
			artificialBody.Radius = 30.0;
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
				artificialBody.ArtificialRigidbody = artificialBody.GeometryRoot.AddComponent<Rigidbody>();
				artificialBody.ArtificialRigidbody.isKinematic = true;
				artificialBody.ArtificialRigidbody.useGravity = false;
				artificialBody.TransferableObjectsRoot.transform.parent = artificialBody.GeometryPlaceholder.transform;
				artificialBody.TransferableObjectsRoot.transform.Reset();
			}

			// The anchor sits at the origin and is exempt from positioning.
			artificialBody.transform.parent = World.ShipExteriorRoot.transform;
			artificialBody.transform.localPosition = Vector3.zero;
			artificialBody.SetTargetPositionAndRotation(position, rotation, true);

			World.AddArtificialBody(artificialBody);
			return artificialBody;
		}

		public override void DestroyGeometry()
		{
			base.DestroyGeometry();
			if (GeometryRoot != null)
			{
				if (this is SpaceObjectVessel)
				{
					ZeroOcclusion.DestroyOcclusionObjectsFor(this as SpaceObjectVessel);
				}

				foreach (Transform child in GeometryRoot.transform.GetChildren())
				{
					if (child != null)
					{
						Destroy(child.gameObject);
					}
				}

				GeometryRoot.transform.parent = GeometryPlaceholder.transform;
				GeometryRoot.transform.Reset();
			}

			if (ArtificialRigidbody != null)
			{
				Destroy(ArtificialRigidbody);
			}

			ArtificialRigidbody = null;
		}

		public virtual void UpdateArtificialBodyPosition(bool updateChildren)
		{
			if (ArtificialRigidbody != null && GeometryPlaceholder != null)
			{
				var position = GeometryPlaceholder.transform.position;
				var rotation = GeometryPlaceholder.transform.rotation;
				GeometryRoot.transform.position = position;
				GeometryRoot.transform.rotation = rotation;
				ArtificialRigidbody.position = position;
				ArtificialRigidbody.rotation = rotation;
			}
		}

		protected virtual void UpdatePositionAndRotation(bool setLocalPositionAndRotation)
		{
			if (ArtificialRigidbody != null && GeometryPlaceholder != null && transform.hasChanged)
			{
				ArtificialRigidbody.position = GeometryPlaceholder.transform.position;
				ArtificialRigidbody.rotation = GeometryPlaceholder.transform.rotation;
				transform.hasChanged = false;
			}
		}

		public override void ModifyPositionAndRotation(Vector3? position = null, Quaternion? rotation = null)
		{
			base.ModifyPositionAndRotation(position, rotation);
			if (IsInVisibilityRange && (position.HasValue || rotation.HasValue) && ArtificialRigidbody != null)
			{
				UpdateArtificialBodyPosition(updateChildren: true);
				transform.hasChanged = false;
			}
		}

		public void SetVelocity(Vector3 velocity, Vector3 angularVelocity)
		{
			_velocity = velocity;
			_angularVelocity = angularVelocity;
		}

		protected virtual void FixedUpdate()
		{
			bool moved = SmoothPosition();
			bool rotated = SmoothRotation();

			// Smoothing moves the transform every step, so the geometry and colliders have to come along
			// every step.
			if (moved || rotated)
			{
				UpdateArtificialBodyPosition(updateChildren: true);
			}
		}

		/// <summary>
		/// 	Eases towards the position the last movement message asked for.
		/// </summary>
		private bool SmoothPosition()
		{
			if (!TargetPosition.HasValue)
			{
				return false;
			}

			// The anchor is pinned at the origin and a docked vessel is placed by whatever it is docked to.
			// Neither may be driven from here, and any target they were still carrying is stale.
			if (!ShouldSetLocalTransform)
			{
				TargetPosition = null;
				return false;
			}

			Vector3 step = Velocity * Time.fixedDeltaTime;
			TargetPosition += step;

			transform.localPosition = OpenHellion.World.VESSEL_TRANSLATION_LERP_UNCLAMPED
				? Vector3.LerpUnclamped(transform.localPosition + step, TargetPosition.Value,
					OpenHellion.World.VESSEL_TRANSLATION_LERP_VALUE)
				: Vector3.Lerp(transform.localPosition + step, TargetPosition.Value,
					OpenHellion.World.VESSEL_TRANSLATION_LERP_VALUE);

			return true;
		}

		/// <summary>
		/// 	Eases towards the rotation the last movement message asked for.
		/// </summary>
		private bool SmoothRotation()
		{
			if (!TargetRotation.HasValue)
			{
				return false;
			}

			Quaternion step = Quaternion.Euler(AngularVelocity * (Mathf.Rad2Deg * Time.fixedDeltaTime));
			TargetRotation = step * TargetRotation.Value;

			transform.localRotation = Quaternion.Slerp(step * transform.localRotation, TargetRotation.Value,
				OpenHellion.World.VESSEL_ROTATION_LERP_VALUE);

			if (!ShouldSetLocalTransform && MyPlayer.Instance != null)
			{
				MyPlayer.Instance.UpdateCameraPositions();
			}

			return true;
		}

		/// <summary>
		/// 	Repositions this body to follow the body it is stabilised to, keeping a fixed local-space
		/// 	offset.
		/// </summary>
		public void UpdateStabilizedPosition()
		{
			if (StabilizeToTargetObj == null)
			{
				return;
			}

			SetTargetPositionAndRotation(
				StabilizeToTargetObj.transform.position + StabilizationOffset, null, instant: true);
		}

		public virtual void OnStabilizationChanged(bool isStabilized)
		{
		}

		public void StabilizeToTarget(long guid, Vector3 stabilizationOffset)
		{
			if (StabilizeToTargetObj != null && StabilizeToTargetObj.Guid != guid)
			{
				StabilizeToTargetObj.StabilizedChildren.Remove(this);
			}

			StabilizeToTargetObj = null;
			if (guid > 0)
			{
				World.TryGetSpaceObject(guid, out ArtificialBody target);
				StabilizeToTargetObj = target;
			}

			if (StabilizeToTargetObj != null)
			{
				StabilizationOffset = stabilizationOffset;
				StabilizeToTargetObj.StabilizedChildren.Add(this);
				OnStabilizationChanged(true);
				UpdateStabilizedPosition();
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
