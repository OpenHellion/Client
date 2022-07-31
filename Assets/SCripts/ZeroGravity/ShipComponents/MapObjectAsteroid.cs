using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Math;

namespace ZeroGravity.ShipComponents
{
	public class MapObjectAsteroid : MapObjectVessel
	{
		public GameObject ObjectVisual;

		public Sprite AsteroidIcon;

		public override Sprite Icon
		{
			get
			{
				return AsteroidIcon;
			}
			set
			{
			}
		}

		public override string Description
		{
			get
			{
				return Localization.DefaultMapAsteroidDescription;
			}
			set
			{
			}
		}

		public override void CreateVisual()
		{
			if (MainObject != null)
			{
				base.gameObject.SetLayerRecursively("Map");
				SetOrbit();
				UpdateOrbitPlane();
			}
		}

		public override void UpdateObject()
		{
			Position.position = base.ObjectPosition;
			Orbits.localScale = Vector3.one * (float)base.ObjectScale;
		}

		public override void SetOrbit()
		{
			List<Vector3D> list = null;
			bool parentChanged = false;
			list = Orbit.GetFlightPathPositions(NumberOfOrbitPositions, 60.0, out parentChanged);
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
	}
}
