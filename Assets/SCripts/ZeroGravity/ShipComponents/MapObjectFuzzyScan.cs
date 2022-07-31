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

		[CompilerGenerated]
		private static Func<Collider, bool> _003C_003Ef__am_0024cache0;

		public override void CreateVisual()
		{
			Collider[] source = Physics.OverlapSphere(base.ObjectPosition, 0.05f);
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CCreateVisual_003Em__0;
			}
			foreach (Collider item in source.Where(_003C_003Ef__am_0024cache0))
			{
				MapObjectFuzzyScan componentInParent = item.GetComponentInParent<MapObjectFuzzyScan>();
				if (componentInParent != null)
				{
					if (componentInParent.CreationTime < CreationTime)
					{
						UnityEngine.Object.Destroy(componentInParent.gameObject);
					}
					else if (componentInParent.MinMaxScale < MinMaxScale)
					{
						Vessels = Vessels.Union(componentInParent.Vessels).Distinct().ToList();
						UnityEngine.Object.Destroy(componentInParent.gameObject);
					}
					else
					{
						componentInParent.Vessels = componentInParent.Vessels.Union(Vessels).Distinct().ToList();
						UnityEngine.Object.Destroy(base.gameObject);
					}
				}
			}
			base.CreateVisual();
		}

		[CompilerGenerated]
		private static bool _003CCreateVisual_003Em__0(Collider m)
		{
			return m.GetComponentInParent<MapObjectFuzzyScan>() != null;
		}
	}
}
