using System;
using UnityEngine;
using ZeroGravity.Math;
using ZeroGravity.UI;

namespace ZeroGravity.ShipComponents
{
	public class MapObjectWarpStart : MapObject
	{
		public GameObject WarpCone;

		public Vector3 WarpConeScale;

		public float TimeToLive = 120f;

		[SerializeField] private Sprite _Icon;

		[NonSerialized] public Vector3D WarpStartPosition;

		[NonSerialized] public Quaternion WarpConeRotation;

		public override Sprite Icon
		{
			get => !(_Icon != null) ? SpriteManager.Instance.DefaultRadarObject : _Icon;
			set { }
		}

		public override string Description { get; set; }

		public override Vector3D TruePosition => WarpStartPosition;

		public override void CreateVisual()
		{
			GameObject thisObject = gameObject;
			thisObject.SetLayerRecursively("Map");
			WarpCone.transform.rotation = WarpConeRotation;
			Destroy(thisObject, TimeToLive);
		}

		public override void UpdateObject()
		{
			Position.position = ObjectPosition;
			WarpCone.transform.localScale = WarpConeScale * (float)ObjectScale;
		}

		public override void UpdateVisibility()
		{
		}

		public override void SetOrbit()
		{
		}
	}
}
