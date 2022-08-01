using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CargoResourceForCraftingUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public Text Name;

	public Text Value;

	public Image Icon;

	public GameObject Tooltip;

	private void Start()
	{
		Tooltip.SetActive(false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Tooltip.SetActive(true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Tooltip.SetActive(false);
	}
}
