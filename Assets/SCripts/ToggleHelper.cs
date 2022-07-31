using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleHelper : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private Toggle button;

	private GameObject checkmark;

	private Animator anim;

	private void Start()
	{
		button = GetComponent<Toggle>();
		checkmark = button.graphic.gameObject;
		anim = checkmark.GetComponent<Animator>();
		checkmark.SetActive(button.isOn);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (button.isOn)
		{
			checkmark.SetActive(true);
		}
		else
		{
			checkmark.GetComponent<Animator>().SetTrigger("Off");
		}
	}
}
