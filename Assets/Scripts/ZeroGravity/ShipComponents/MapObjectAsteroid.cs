using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Math;

namespace ZeroGravity.ShipComponents
{
	public class MapObjectAsteroid : MapObjectVessel
	{
		public Sprite AsteroidIcon;

		public override Sprite Icon
		{
			get { return AsteroidIcon; }
			set { }
		}

		public override string Description
		{
			get { return Localization.DefaultMapAsteroidDescription; }
			set { }
		}

		public override void CreateVisual()
		{
			if (MainObject != null)
			{
				gameObject.SetLayerRecursively("Map");
				SetOrbit();
				UpdateOrbitPlane();
			}
		}

		public override void UpdateObject()
		{
			Position.position = ObjectPosition;
			Orbits.localScale = Vector3.one * (float)ObjectScale;
		}

		public override void SetOrbit()
		{
			List<Vector3D> flightPathPositions = Orbit.GetFlightPathPositions(World, NumberOfOrbitPositions, 60.0, out _);
			MyOrbitRenderer.positionCount = flightPathPositions.Count;
			if (flightPathPositions.Count > 0)
			{
				for (int i = 0; i < flightPathPositions.Count; i++)
				{
					MyOrbitRenderer.SetPosition(i, flightPathPositions[i].ToVector3());
				}

				MyOrbitRenderer.SetPosition(NumberOfOrbitPositions - 1, flightPathPositions[0].ToVector3());
			}

			MyOrbitRenderer.transform.SetParent(Orbits);
			MyOrbitRenderer.startColor = OrbitColor;
			MyOrbitRenderer.endColor = OrbitColor;
		}
	}
}
