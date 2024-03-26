using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity.ShipComponents
{
	public abstract class MapObjectVessel : MapObject
	{
		protected override bool IsVisibleOnMap
		{
			get
			{
				return (RadarVisibilityType != 0 && RadarVisibilityType != RadarVisibilityType.Warp) ||
				       this == Map.MyShip || Map.AllObjectsVisible;
			}
		}

		public virtual double RadarSignature
		{
			get { return (MainObject == null) ? 0.0 : (MainObject as SpaceObjectVessel).RadarSignature; }
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
				ObjectVisibilityBackground.material.color = Colors.RadarVisibility[RadarVisibilityType];
			}

			if (!gameObject.activeSelf || !(NewObjectVisibility != null) || NewObjectVisibility.activeSelf)
			{
				return;
			}

			long num = 0L;
			SpaceObjectVessel obj = MainObject as SpaceObjectVessel;
			if ((((object)obj != null) ? obj.VesselData : null) != null)
			{
				num = (MainObject as SpaceObjectVessel).VesselData.SpawnRuleID;
			}

			if (Map.IsInitializing || (num != 0 &&
			                                Map.KnownSpawnRuleIDs.Contains((MainObject as SpaceObjectVessel)
				                                .VesselData.SpawnRuleID)))
			{
				Destroy(NewObjectVisibility);
			}
			else
			{
				NewObjectVisibility.Activate(true);
				Renderer component = NewObjectVisibility.GetComponent<Renderer>();
				if (component != null)
				{
					Color color = component.material.GetColor("_Tint");
					NewObjectColorFadeIncrement = color / NewObjectVisibilityDuration;
				}

				Destroy(NewObjectVisibility, NewObjectVisibilityDuration);
			}

			if (num != 0)
			{
				Map.KnownSpawnRuleIDs.Add(num);
			}
		}
	}
}
