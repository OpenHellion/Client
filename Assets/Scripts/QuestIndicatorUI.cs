using TMPro;
using UnityEngine;
using ZeroGravity.LevelDesign;

public class QuestIndicatorUI : MonoBehaviour
{
	public QuestIndicators QuestIndicators;

	public SceneQuestTrigger SceneQuestTrigger;

	public Animator Animator;

	public GameObject OnScreen;

	public GameObject OffScreen;

	public Transform Arrow;

	public TextMeshProUGUI TaskName;

	public void OnFinished()
	{
		QuestIndicators.RemoveQuestIndicator(SceneQuestTrigger);
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}

	public void SetOffScreen(bool offScreen)
	{
		OnScreen.SetActive(!offScreen);
		OffScreen.SetActive(offScreen);
	}
}
