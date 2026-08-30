using System.Collections.Generic;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class VesselRequestButton : SceneTrigger
	{
		private SpaceObjectVessel parentBody;

		public float ArriveTime = 60f;

		public List<SceneTagObject> RescueShipTags;

		public GameScenes.SceneId RescueShipType = GameScenes.SceneId.AltCorp_Shuttle_CECA;

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!base.Interact(MyPlayer.Instance, true))
			{
				return false;
			}

			CallShip();
			return true;
		}

		public void CallShip()
		{
			parentBody = GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
			if (parentBody != null)
			{
				World.SendVesselRequest(parentBody, ArriveTime, RescueShipType,
					SceneTagObject.TagsToString(RescueShipTags));
			}
		}
	}
}
