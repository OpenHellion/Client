using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	[RequireComponent(typeof(Image))]
	public class CargoResourceUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler,
		IBeginDragHandler, IDragHandler, IEndDragHandler, IEventSystemHandler
	{
		public ICargoCompartment Compartment;

		public CargoResourceData Resource;

		public float Quantity;

		public Text Name;

		public Text Value;

		public Image Icon;

		public GameObject Hover;

		public CargoPanel MyCargoPanel
		{
			get { return GetComponentInParent<CargoPanel>(); }
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			Hover.SetActive(true);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			Hover.SetActive(false);
		}

		public void OnDrop(PointerEventData eventData)
		{
			CargoTransferingResource component = MyCargoPanel.DragingItem.GetComponent<CargoTransferingResource>();
			if (component.FromCompartment != null && component.FromResource != null)
			{
				component.ToCompartment = Compartment;
				MyCargoPanel.SetTransferBox();
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			MyCargoPanel.DragingItem.SetActive(false);
			MyCargoPanel.isDragigng = false;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (Quantity > 0f)
			{
				MyCargoPanel.DragingItem.SetActive(true);
				MyCargoPanel.isDragigng = true;
				RectTransform component = MyCargoPanel.DragingItem.GetComponent<RectTransform>();
				component.transform.position = Mouse.current.position.ReadValue();
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (Quantity > 0f)
			{
				CargoTransferingResource component = MyCargoPanel.DragingItem.GetComponent<CargoTransferingResource>();
				component.Icon.sprite = SpriteManager.Instance.GetSprite(Resource.ResourceType);
				component.FromCompartment = Compartment;
				component.FromResource = Resource;
				component.Quantity = Quantity;
			}
		}

		public void SetName()
		{
			if (Resource != null)
			{
				Name.text = Resource.ResourceType.ToLocalizedString().CamelCaseToSpaced();
			}
			else
			{
				Name.text = "-";
			}
		}
	}
}
