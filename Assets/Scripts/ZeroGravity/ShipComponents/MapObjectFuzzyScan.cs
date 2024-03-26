using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity.ShipComponents
{
	public class MapObjectFuzzyScan : MapObjectFixedPosition
	{
		public List<SpaceObjectVessel> Vessels = new List<SpaceObjectVessel>();

		public override string Name => gameObject.name;

		public override void CreateVisual()
		{
			foreach (Collider item in from m in Physics.OverlapSphere(ObjectPosition, 0.05f)
			         where m.GetComponentInParent<MapObjectFuzzyScan>() != null
			         select m)
			{
				MapObjectFuzzyScan componentInParent = item.GetComponentInParent<MapObjectFuzzyScan>();
				if (componentInParent != null)
				{
					if (componentInParent.CreationTime < CreationTime)
					{
						Destroy(componentInParent.gameObject);
					}
					else if (componentInParent.MinMaxScale < MinMaxScale)
					{
						Vessels = Vessels.Union(componentInParent.Vessels).Distinct().ToList();
						Destroy(componentInParent.gameObject);
					}
					else
					{
						componentInParent.Vessels = componentInParent.Vessels.Union(Vessels).Distinct().ToList();
						Destroy(gameObject);
					}
				}
			}
			base.CreateVisual();
		}
	}
}
