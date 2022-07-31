using UnityEngine;
using UnityEngine.EventSystems;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class ItemSlotUI : AbstractSlotUI
	{
		public ItemSlot Slot;

		public override bool IsDisabled
		{
			get
			{
				if ((InventoryUI.DraggingItem != null && InventoryUI.DraggingItem is Outfit && InventoryUI.DraggingItem.Slot.Parent is Corpse) || (InventoryUI.DraggingItem != null && InventoryUI.DraggingItem.Slot is BaseSceneAttachPoint && Slot.Item != null))
				{
					return true;
				}
				if (InventoryUI.IsDragging && InventoryUI.DraggingItem != null)
				{
					return !Slot.CanFitItem(InventoryUI.DraggingItem);
				}
				return false;
			}
		}

		public override Item Item
		{
			get
			{
				if (Slot != null)
				{
					return Slot.Item;
				}
				return null;
			}
		}

		public override void UpdateSlot()
		{
			if (Item == null)
			{
				if (IsLootSlot)
				{
					InventoryUI.AllSlots.Remove(this);
					InventoryUI.LootSlots.Remove(this);
					Object.Destroy(base.gameObject);
				}
				else
				{
					Icon.sprite = Slot.GetIcon();
					Icon.color = Colors.Gray;
				}
			}
			else
			{
				Icon.sprite = Item.Icon;
				Icon.color = Colors.White;
				if (InventoryUI.SelectedItem == Item)
				{
					InventoryUI.UpdateSelectedItemInfo(Item);
				}
			}
			UpdateHealth();
			RefreshQuantity();
		}

		public override void OnDrop(PointerEventData eventData)
		{
			if (!IsDisabled)
			{
				if (InventoryUI.DraggingItem != null)
				{
					InventoryUI.DraggingItem.RequestAttach(Slot);
				}
				if (Item == InventoryUI.SelectedItem)
				{
					InventoryUI.DeselectItem();
				}
				InventoryUI.RefreshSlots();
			}
		}
	}
}
