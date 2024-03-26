using UnityEngine;
using ZeroGravity.Math;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class Pivot : ArtificialBody
	{
		public Vector3D Acceleration = Vector3D.Zero;

		public Vector3 Rotation = Vector3.zero;

		public SpaceObjectType ChildType;

		private SpaceObjectType pivotType;

		public override SpaceObjectType Type => pivotType;

		public static Pivot Create(SpaceObjectType pivotType, ObjectTransform trans, bool isMainObject)
		{
			Pivot pivot = CreateImpl(pivotType, trans.GUID, trans, isMainObject) as Pivot;
			pivot.pivotType = pivotType;
			switch (pivotType)
			{
				case SpaceObjectType.PlayerPivot:
					pivot.ChildType = SpaceObjectType.Player;
					break;
				case SpaceObjectType.DynamicObjectPivot:
					pivot.ChildType = SpaceObjectType.DynamicObject;
					break;
				case SpaceObjectType.CorpsePivot:
					pivot.ChildType = SpaceObjectType.Corpse;
					break;
				default:
					Debug.LogError("Unknown pivot type " + pivotType);
					break;
			}

			return pivot;
		}

		public static Pivot Create(SpaceObjectType pivotType, long guid, ArtificialBody ab, bool isMainObject)
		{
			OrbitData data = new OrbitData();
			if (ab.StabilizeToTargetObj != null)
			{
				ab.UpdateStabilizedPosition();
			}

			ab.Orbit.FillOrbitData(ref data);
			Pivot pivot = Create(pivotType, new ObjectTransform
			{
				GUID = guid,
				AngularVelocity = ab.AngularVelocity.ToArray(),
				Type = pivotType,
				Orbit = data,
				Forward = Vector3.forward.ToArray(),
				Up = Vector3.up.ToArray()
			}, isMainObject);
			if (guid == MyPlayer.Instance.Guid)
			{
				pivot.transform.parent = null;
			}
			else
			{
				pivot.transform.parent = World.ShipExteriorRoot.transform;
			}

			pivot.transform.position = ab.transform.position;
			pivot.transform.localRotation = Quaternion.identity;
			return pivot;
		}

		private void FixedUpdate()
		{
			if (MyPlayer.Instance.Parent != null)
			{
				UpdatePositionAndRotation(MyPlayer.Instance.Parent != this);
			}
		}

		public override void SetTargetPositionAndRotation(Vector3? localPosition, Quaternion? localRotation,
			bool instant = false, double time = -1.0)
		{
			base.SetTargetPositionAndRotation(localPosition, localRotation, true, time);
		}

		private void OnDrawGizmos()
		{
			Color color = ChildType == SpaceObjectType.Player
				? new Color(1f, 0f, 0f, 0.05f)
				: ChildType != SpaceObjectType.Corpse
					? new Color(0f, 1f, 0f, 0.05f)
					: new Color(0f, 0f, 1f, 0.05f);
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = color;
			Gizmos.DrawSphere(transform.position, 0.5f);
			color.a = 0.3f;
			Gizmos.color = color;
			Gizmos.DrawWireSphere(transform.position, 0.5f);
		}
	}
}
