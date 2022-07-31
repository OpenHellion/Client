using UnityEngine;
using UnityEngine.EventSystems;

namespace ZeroGravity.UI
{
	public class InventoryUIDrop : MonoBehaviour, IDropHandler, IPointerClickHandler, IEventSystemHandler
	{
		public InventoryUI Inventory;

		public void OnDrop(PointerEventData data)
		{
			if (Inventory.IsDragging && Inventory.DraggingItem != null)
			{
				Inventory.DraggingItem.RequestDrop();
				Inventory.UpdateUI();
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (Inventory.SelectedItem != null)
			{
				Inventory.DeselectItem();
			}
		}
	}
}
