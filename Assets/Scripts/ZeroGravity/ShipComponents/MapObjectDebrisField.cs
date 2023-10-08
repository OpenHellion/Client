using UnityEngine;

namespace ZeroGravity.ShipComponents
{
	public class MapObjectDebrisField : MapObject
	{
		public MapDebrisFieldEffect mapDebrisFieldEffect;

		public override Sprite Icon
		{
			get { return null; }
			set { }
		}

		public override string Description
		{
			get { return "debris"; }
			set { }
		}

		public override void CreateVisual()
		{
			if (MainObject != null)
			{
				base.gameObject.SetLayerRecursively("Map");
				UpdateOrbitPlane();
				mapDebrisFieldEffect.SetRadius(Radius, (float)Orbit.SemiMinorAxis);
			}
		}

		public override void UpdateObject()
		{
			Orbits.localScale = Vector3.one * (float)base.ObjectScale;
		}
	}
}
