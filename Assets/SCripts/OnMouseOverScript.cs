using UnityEngine;
using UnityEngine.EventSystems;

public class OnMouseOverScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	private GameObject OnHoverImage;

	private void Start()
	{
		OnHoverImage = base.transform.Find("Active").gameObject;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnHoverImage.SetActive(true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnHoverImage.SetActive(false);
	}
}
