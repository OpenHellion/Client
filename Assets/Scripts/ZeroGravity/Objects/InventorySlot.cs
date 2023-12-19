using System;
using System.Collections.Generic;
using OpenHellion.IO;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.UI;

namespace ZeroGravity.Objects
{
	public class InventorySlot : IItemSlot
	{
		[Serializable]
		public class AttachData
		{
			public ItemType ItemType;

			public GameObject Point;

			public bool HideAttachedObject;
		}

		public enum Type
		{
			Hands = 0,
			General = 1,
			Equip = 2
		}

		public enum Size
		{
			One = 1,
			Two = 2,
			Three = 3,
			Four = 4
		}

		public enum Group
		{
			Hands = -1,
			Outfit = -2,
			Primary = 1,
			Secondary = 2,
			Helmet = 3,
			Jetpack = 4,
			Tool = 5,
			Ammo = 6,
			Consumable = 7,
			Utility = 8
		}

		public const short NoneSlotID = -1111;

		public const short HandsSlotID = -1;

		public const short StartSlotID = 1;

		public const short OutfitSlotID = -2;

		private bool mustBeEmptyToRemoveOutfit;

		private Type slotType;

		private Size slotSize;

		private Group slotGroup;

		private Inventory slotInventory;

		public Outfit Outfit;

		public InventorySlotUI UI;

		private Dictionary<ItemType, AttachData> attachPoints = new Dictionary<ItemType, AttachData>();

		private Item _Item;

		public short SlotID { get; private set; }

		public ControlsSubsystem.ConfigAction? ShortcutKey { get; private set; }

		public bool MustBeEmptyToRemoveOutfit => mustBeEmptyToRemoveOutfit;

		public Type SlotType => slotType;

		public int SlotSizeInt => (int)slotSize;

		public Size SlotSize => slotSize;

		public Inventory Inventory => slotInventory;

		public Group SlotGroup => slotGroup;

		public Item Item
		{
			get { return _Item; }
			private set
			{
				_Item = value;
				if (UI != null)
				{
					UI.UpdateSlot();
				}

				//_world.InGameGUI.HelmetHud.UpdateQuickSlots();
			}
		}

		public SpaceObject Parent
		{
			get
			{
				if (slotInventory != null)
				{
					return slotInventory.Parent;
				}

				if (Outfit != null)
				{
					return Outfit.DynamicObj;
				}

				throw new Exception("Inventory slot has no parent");
			}
		}

		public InventorySlot(Outfit outfit, short slotID, Type type, Size size, Group group,
			ControlsSubsystem.ConfigAction? shortKey, int cycleIndex, bool mustBeEmptyToRemove, Item item,
			List<AttachData> points, Inventory inv)
		{
			SlotID = slotID;
			slotType = type;
			slotSize = size;
			slotGroup = group;
			ShortcutKey = shortKey;
			mustBeEmptyToRemoveOutfit = mustBeEmptyToRemove;
			slotInventory = inv;
			Outfit = outfit;
			foreach (AttachData point in points)
			{
				if (!attachPoints.ContainsKey(point.ItemType))
				{
					attachPoints.Add(point.ItemType, point);
				}
				else
				{
					Debug.LogWarning(
						"Inventory slot has same item type in multiple attach points, ommited other attach points");
				}
			}

			SetItem(item);
		}

		public static bool IsGroupable(Group group)
		{
			return group >= (Group)100;
		}

		public void SetInventory(Inventory inv)
		{
			slotInventory = inv;
		}

		public void SetItem(Item item, bool sendMessage = true)
		{
			if (slotInventory != null && slotInventory.Parent is MyPlayer)
			{
				if (Item == item)
				{
					return;
				}

				Item = item;
				if (item != null)
				{
					item.AttachToObject(this, sendMessage && slotInventory != null && slotInventory.Parent is MyPlayer);
					Item.EquipType type = Item.EquipType.None;
					if (slotType == Type.Hands)
					{
						type = Item.EquipType.Hands;
					}
					else if (slotType == Type.Equip)
					{
						type = Item.EquipType.EquipInventory;
					}
					else if (slotType == Type.General)
					{
						type = Item.EquipType.Inventory;
					}

					item.ChangeEquip(type, slotInventory.Parent as Player);
				}

				InventoryCharacterPreview.Instance.RefreshPreviewCharacter(Inventory);
				return;
			}

			Item = item;
			if (!(item != null))
			{
				return;
			}

			item.AttachToObject(this, false);
			if (slotInventory != null && slotInventory.Parent is Player)
			{
				Item.EquipType type2 = Item.EquipType.None;
				if (slotType == Type.Hands)
				{
					type2 = Item.EquipType.Hands;
				}
				else if (slotType == Type.Equip)
				{
					type2 = Item.EquipType.EquipInventory;
				}
				else if (slotType == Type.General)
				{
					type2 = Item.EquipType.Inventory;
				}

				item.ChangeEquip(type2, slotInventory.Parent as Player);
			}
		}

		public bool CanFitItem(Item item)
		{
			if (item is Outfit && SlotType == Type.Hands && Item != null)
			{
				return false;
			}

			if (item is GenericItem && (item as GenericItem).SubType == GenericItemSubType.TransportBox)
			{
				return false;
			}

			return slotType == Type.Hands || attachPoints.ContainsKey(item.Type);
		}

		public bool CanFitAnyItemType(params ItemType[] itemTypes)
		{
			if (slotType == Type.Hands)
			{
				return true;
			}

			foreach (ItemType key in itemTypes)
			{
				if (attachPoints.ContainsKey(key))
				{
					return true;
				}
			}

			return false;
		}

		public AttachData GetAttachPoint(Item item)
		{
			if (slotType == Type.Hands || SlotID == -2)
			{
				return attachPoints[ItemType.None];
			}

			if (attachPoints.ContainsKey(item.Type))
			{
				return attachPoints[item.Type];
			}

			return null;
		}

		public static InventorySlot CreateHandsSlot(GameObject handsAttachPoint, Inventory inv)
		{
			return new InventorySlot(null, -1, Type.Hands, Size.One, Group.Hands, null, -1, true, null,
				new List<AttachData>
				{
					new AttachData
					{
						HideAttachedObject = false,
						ItemType = ItemType.None,
						Point = handsAttachPoint
					}
				}, inv);
		}

		public static InventorySlot CreateOutfitSlot(GameObject outfitAttachPoint, Inventory inv)
		{
			return new InventorySlot(null, -2, Type.Equip, Size.One, Group.Outfit, null, -1, false, null,
				new List<AttachData>
				{
					new AttachData
					{
						HideAttachedObject = true,
						ItemType = ItemType.None,
						Point = outfitAttachPoint
					}
				}, inv);
		}

		public void SetAttachPoint(ItemType itemType, GameObject point, bool hideAttachedObject)
		{
			if (attachPoints.ContainsKey(itemType))
			{
				attachPoints[itemType].Point = point;
				attachPoints[itemType].HideAttachedObject = hideAttachedObject;
			}
		}

		public Sprite GetIcon()
		{
			if (Item != null)
			{
				return Item.Icon;
			}

			return SpriteManager.Instance.GetSprite(SlotGroup);
		}
	}
}
