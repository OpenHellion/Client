using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity.ShipComponents
{
	public abstract class MapObjectVessel : MapObject
	{
		public override bool IsVisibleOnMap
		{
			get
			{
				return (base.RadarVisibilityType != 0 && base.RadarVisibilityType != RadarVisibilityType.Warp) || this == base.Map.MyShip || base.Map.AllObjectsVisible;
			}
		}

		public override double RadarSignature
		{
			get
			{
				return (MainObject == null) ? 0.0 : (MainObject as SpaceObjectVessel).RadarSignature;
			}
		}

		public override void UpdateVisibility()
		{
			base.UpdateVisibility();
			if (!(MainObject is SpaceObjectVessel))
			{
				return;
			}
			if (ObjectVisibilityBackground != null)
			{
				ObjectVisibilityBackground.material.color = Colors.RadarVisibility[base.RadarVisibilityType];
			}
			if (!base.gameObject.activeSelf || !(NewObjectVisibility != null) || NewObjectVisibility.activeSelf)
			{
				return;
			}
			long num = 0L;
			SpaceObjectVessel obj = MainObject as SpaceObjectVessel;
			if ((((object)obj != null) ? obj.VesselData : null) != null)
			{
				num = (MainObject as SpaceObjectVessel).VesselData.SpawnRuleID;
			}
			if (base.Map.IsInitializing || (num != 0 && base.Map.KnownSpawnRuleIDs.Contains((MainObject as SpaceObjectVessel).VesselData.SpawnRuleID)))
			{
				Object.Destroy(NewObjectVisibility);
			}
			else
			{
				NewObjectVisibility.Activate(true);
				Renderer component = NewObjectVisibility.GetComponent<Renderer>();
				if (component != null)
				{
					Color color = component.material.GetColor("_Tint");
					newObjectColorFadeIncrement = color / NewObjectVisibilityDuration;
				}
				Object.Destroy(NewObjectVisibility, NewObjectVisibilityDuration);
			}
			if (num != 0)
			{
				base.Map.KnownSpawnRuleIDs.Add(num);
			}
		}
	}
}
