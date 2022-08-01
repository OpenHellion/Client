using ZeroGravity.LevelDesign;

namespace ZeroGravity.Objects
{
	public class BasketballHoop : GenericItem
	{
		private SceneNameTag[] scoreTexts;

		public override void OnAttach(bool isAttached, bool isOnPlayer)
		{
			base.OnAttach(isAttached, isOnPlayer);
			BasketballScoreCounter componentInChildren = GetComponentInChildren<BasketballScoreCounter>();
			if (!(componentInChildren != null))
			{
				return;
			}
			if (DynamicObj.Parent is SpaceObjectVessel)
			{
				if (base.AttachPoint != null)
				{
					scoreTexts = base.AttachPoint.GetComponentsInChildren<SceneNameTag>();
					if (scoreTexts.Length > 0)
					{
						int result;
						if (int.TryParse(scoreTexts[0].NameTagText, out result))
						{
							componentInChildren.Counter = result;
						}
						else
						{
							componentInChildren.ResetCounter();
						}
					}
				}
				else
				{
					componentInChildren.ResetCounter();
				}
			}
			else if (scoreTexts != null)
			{
				SceneNameTag[] array = scoreTexts;
				foreach (SceneNameTag sceneNameTag in array)
				{
					sceneNameTag.SetNameTagText(string.Empty);
				}
				scoreTexts = null;
			}
		}
	}
}
