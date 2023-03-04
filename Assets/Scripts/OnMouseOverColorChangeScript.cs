using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnMouseOverColorChangeScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public Color DefaultColor;

	public Color HoverColor;

	public void OnPointerEnter(PointerEventData eventData)
	{
		GetComponent<Image>().color = HoverColor;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		GetComponent<Image>().color = DefaultColor;
	}
}
