using UnityEngine;
using UnityEngine.UI;
using ZeroGravity;
using ZeroGravity.Objects;

public class QuestTitleUI : MonoBehaviour
{
	public Text TitleText;

	public Animator Animator;

	private void Awake()
	{
		Animator = GetComponent<Animator>();
	}

	private void Update()
	{
	}

	public void ShowTitle(Quest quest)
	{
		TitleText.text = Localization.GetLocalizedField(quest.Name, true).ToUpper();
		base.gameObject.SetActive(true);
		Animator.SetBool("Completed", false);
	}

	public void CompleteQuest(Quest quest)
	{
		TitleText.text = Localization.GetLocalizedField(quest.Name, true).ToUpper();
		base.gameObject.SetActive(true);
		Animator.SetBool("Completed", true);
	}
}
