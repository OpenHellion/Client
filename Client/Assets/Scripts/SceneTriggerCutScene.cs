using OpenHellion;
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
		World.InGameGUI.QuestCutScene.PlayCutScene(CutScene);
	}
}
