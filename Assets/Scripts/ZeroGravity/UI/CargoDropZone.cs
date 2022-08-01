using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	[RequireComponent(typeof(Image))]
	public class CargoDropZone : MonoBehaviour, IDropHandler, IEventSystemHandler
	{
		public CargoPanel MyCargoPanel
		{
			get
			{
				return GetComponentInParent<CargoPanel>();
			}
		}

		public void OnDrop(PointerEventData eventData)
		{
			CargoTransferingResource component = MyCargoPanel.DragingItem.GetComponent<CargoTransferingResource>();
			component.ToCompartment = MyCargoPanel.CurrentlySelectedCargoBay.CargoBayResources;
			MyCargoPanel.SetTransferBox();
		}
	}
}
