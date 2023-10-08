using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerVesselSystemControl : SceneTrigger
	{
		public VesselSystem VesselSystem;

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!base.Interact(MyPlayer.Instance, true))
			{
				return false;
			}

			ToggleGenerator();
			return true;
		}

		public void ToggleGenerator()
		{
			VesselSystem.Toggle();
		}
	}
}
