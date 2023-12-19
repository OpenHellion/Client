using UnityEngine;
using UnityEngine.EventSystems;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class InventorySlotUI : AbstractSlotUI
	{
		public InventorySlot Slot;

		public override bool IsDisabled
		{
			get
			{
				if (InventoryUI.DraggingItem == null || InventoryUI.DraggingItem.Slot == Slot)
				{
					return false;
				}

				if (IsLootSlot || (InventoryUI.DraggingItem != null &&
				                   InventoryUI.DraggingItem.Slot is BaseSceneAttachPoint && Slot.Item != null))
				{
					return true;
				}

				if (InventoryUI.DraggingItem is Outfit)
				{
					if (Slot == InventoryUI.Inventory.OutfitSlot && (InventoryUI.DraggingItem.Slot.Parent is MyPlayer ||
					                                                 InventoryUI.DraggingItem.Slot is
						                                                 BaseSceneAttachPoint))
					{
						return Slot.Item != null;
					}

					if (InventoryUI.DraggingItem.Slot.Parent is Corpse)
					{
						return Slot.Item != null || Slot != InventoryUI.Inventory.HandsSlot;
					}

					return true;
				}

				if (Slot.CanFitItem(InventoryUI.DraggingItem))
				{
					if (Slot.Item != null && InventoryUI.DraggingItem.Slot.Parent is Corpse)
					{
						return true;
					}

					return false;
				}

				return true;
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
			if (Item == null && IsLootSlot)
			{
				InventoryUI.AllSlots.Remove(this);
				InventoryUI.LootSlots.Remove(this);
				Object.Destroy(base.gameObject);
			}
			else
			{
				Icon.sprite = Slot.GetIcon();
			}

			if (Item != null && InventoryUI.SelectedItem == Item)
			{
				InventoryUI.UpdateSelectedItemInfo(Item);
			}

			UpdateHealth();
			RefreshQuantity();
			CheckItemSlots();
			if (Slot == InventoryUI.Inventory.HandsSlot)
			{
				InventoryCharacterPreview.Instance.RefreshPreviewCharacter(InventoryUI.Inventory);
			}
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
