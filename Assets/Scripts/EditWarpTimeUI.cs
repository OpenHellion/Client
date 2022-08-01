using UnityEngine;
using UnityEngine.EventSystems;
using ZeroGravity.ShipComponents;

public class EditWarpTimeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public NavigationPanel Panel;

	public GameObject Active;

	public bool startTime;

	public bool endTime;

	private void Start()
	{
		Panel = GetComponentInParent<NavigationPanel>();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Active.SetActive(true);
		Panel.StartTimeEdit = startTime;
		Panel.EndTimeEdit = endTime;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Active.SetActive(false);
		Panel.StartTimeEdit = false;
		Panel.EndTimeEdit = false;
	}
}
