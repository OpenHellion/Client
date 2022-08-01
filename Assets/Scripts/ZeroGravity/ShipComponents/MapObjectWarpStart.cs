using System;
using UnityEngine;
using ZeroGravity.Math;

namespace ZeroGravity.ShipComponents
{
	public class MapObjectWarpStart : MapObject
	{
		public GameObject WarpCone;

		public Vector3 WarpConeScale;

		public float TimeToLive = 120f;

		[SerializeField]
		private Sprite _Icon;

		[NonSerialized]
		public Vector3D WarpStartPosition;

		[NonSerialized]
		public Quaternion WarpConeRotation;

		public new string Name { get; set; }

		public override Sprite Icon
		{
			get
			{
				return (!(_Icon != null)) ? Client.Instance.SpriteManager.DefaultRadarObject : _Icon;
			}
			set
			{
			}
		}

		public override string Description { get; set; }

		public override Vector3D TruePosition
		{
			get
			{
				return WarpStartPosition;
			}
		}

		public override void CreateVisual()
		{
			base.gameObject.SetLayerRecursively("Map");
			WarpCone.transform.rotation = WarpConeRotation;
			UnityEngine.Object.Destroy(base.gameObject, TimeToLive);
		}

		public override void UpdateObject()
		{
			Position.position = base.ObjectPosition;
			WarpCone.transform.localScale = WarpConeScale * (float)base.ObjectScale;
		}

		public override void UpdateVisibility()
		{
		}

		public override void SetOrbit()
		{
		}
	}
}
