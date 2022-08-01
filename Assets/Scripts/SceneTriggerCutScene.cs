using ZeroGravity;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

public class SceneTriggerCutScene : SceneTrigger
{
	public QuestCutSceneData CutScene;

	public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
	{
		if (!base.Interact(MyPlayer.Instance, true))
		{
			return false;
		}
		PlayCutScene();
		return true;
	}

	public void PlayCutScene()
	{
		Client.Instance.CanvasManager.CanvasUI.QuestCutScene.PlayCutScene(CutScene);
	}
}
