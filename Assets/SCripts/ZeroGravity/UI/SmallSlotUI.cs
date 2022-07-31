using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class SmallSlotUI : MonoBehaviour, IDropHandler, IEventSystemHandler
	{
		public InventoryUI Inventory;

		public ItemSlot Slot;

		public Image Icon;

		public void UpdateUI()
		{
			Icon.sprite = Slot.GetIcon();
			if (Slot.Item != null)
			{
				Icon.color = Colors.White;
			}
			else
			{
				Icon.color = Colors.Gray;
			}
		}

		public void OnDrop(PointerEventData eventData)
		{
			if (Inventory.DraggingItem != null && Slot.CanFitItem(Inventory.DraggingItem))
			{
				Inventory.DraggingItem.RequestAttach(Slot);
			}
		}
	}
}
