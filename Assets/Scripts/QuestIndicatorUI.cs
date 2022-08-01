using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;

public class QuestIndicatorUI : MonoBehaviour
{
	public QuestIndicators QuestIndicators;

	public SceneQuestTrigger SceneQuestTrigger;

	public Animator Animator;

	public GameObject OnScreen;

	public GameObject OffScreen;

	public Transform Arrow;

	public Text TaskName;

	public void OnFinished()
	{
		QuestIndicators.RemoveQuestIndicator(SceneQuestTrigger);
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
	}

	public void SetOffScreen(bool offScreen)
	{
		OnScreen.SetActive(!offScreen);
		OffScreen.SetActive(offScreen);
	}
}
