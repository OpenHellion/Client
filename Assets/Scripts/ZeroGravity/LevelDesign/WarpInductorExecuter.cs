using UnityEngine;

namespace ZeroGravity.LevelDesign
{
	public class WarpInductorExecuter : MonoBehaviour
	{
		[SerializeField]
		private SceneTriggerExecuter warpInductor;

		[SerializeField]
		private string warpInductorOpenState = "Open";

		[SerializeField]
		private string warpInductorCloseState = "Close";

		public void ToggleInductor(bool isActive, bool isInstant)
		{
			if (!(warpInductor != null))
			{
				return;
			}
			if (isActive && !warpInductorOpenState.IsNullOrEmpty())
			{
				if (isInstant)
				{
					warpInductor.ChangeStateImmediate(warpInductorOpenState);
				}
				else
				{
					warpInductor.ChangeState(warpInductorOpenState);
				}
			}
			else if (!isActive && !warpInductorCloseState.IsNullOrEmpty())
			{
				if (isInstant)
				{
					warpInductor.ChangeStateImmediate(warpInductorCloseState);
				}
				else
				{
					warpInductor.ChangeState(warpInductorCloseState);
				}
			}
		}
	}
}
