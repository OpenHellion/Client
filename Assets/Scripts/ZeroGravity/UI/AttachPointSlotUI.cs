using UnityEngine;
using UnityEngine.EventSystems;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class AttachPointSlotUI : AbstractSlotUI
	{
		public BaseSceneAttachPoint AttachPoint;

		public Sprite DefaultSprite;

		public override bool IsDisabled
		{
			get
			{
				if ((Item != null && !Item.CanPlayerPickUp(MyPlayer.Instance)) ||
				    (InventoryUI.DraggingItem != null && InventoryUI.DraggingItem is Outfit &&
				     InventoryUI.DraggingItem.Slot.Parent is Corpse) || (InventoryUI.DraggingItem != null &&
				                                                         !(InventoryUI.DraggingItem.Slot is
					                                                         BaseSceneAttachPoint) &&
				                                                         AttachPoint.Item != null))
				{
					return true;
				}

				if (InventoryUI.IsDragging && InventoryUI.DraggingItem != null)
				{
					if (AttachPoint.CanFitItem(InventoryUI.DraggingItem) || IsLootSlot)
					{
						return false;
					}

					return true;
				}

				return false;
			}
		}

		public override Item Item
		{
			get
			{
				if (AttachPoint != null)
				{
					return AttachPoint.Item;
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
				else if (AttachPoint.GetIcon() == null)
				{
					Icon.sprite = SpriteManager.Instance.GetSprite(AttachPoint.StandardTip);
					Icon.color = Colors.SlotGray;
				}
				else
				{
					Icon.sprite = AttachPoint.GetIcon();
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
			CheckItemSlots();
		}

		public override void OnDrop(PointerEventData eventData)
		{
			if (!IsDisabled && !(Item != null))
			{
				if (InventoryUI.DraggingItem != null)
				{
					InventoryUI.DraggingItem.RequestAttach(AttachPoint);
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
