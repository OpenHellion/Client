using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.UI;

namespace ZeroGravity.Objects
{
	public class ItemSlot : MonoBehaviour, IItemSlot
	{
		public short ID;

		public List<ItemType> ItemTypes;

		public List<GenericItemSubType> GenericSubTypes;

		public List<MachineryPartType> MachineryPartTypes;

		[SerializeField] private Transform _ItemPlacement;

		public ItemCompoundType SpawnItem;

		[NonSerialized] public ItemSlotUI UI;

		private SpaceObject _Parent;

		private Item _Item;

		public Transform ItemPlacement => !(_ItemPlacement != null) ? transform : _ItemPlacement;

		public SpaceObject Parent
		{
			get => _Parent;
			set => _Parent = value;
		}

		public Item Item
		{
			get => _Item;
			private set
			{
				_Item = value;
				if (UI != null)
				{
					UI.UpdateSlot();
					if (Parent is DynamicObject && (Parent as DynamicObject).Item != null)
					{
						(Parent as DynamicObject).Item.UpdateUI();
					}

					UI.UpdateSlot();
				}
			}
		}

		private void Start()
		{
			_Parent = GetComponentInParent<DynamicObject>();
		}

		public bool FitItem(Item item)
		{
			if (CanFitItem(item))
			{
				Item = item;
				item.Slot = this;
				item.transform.parent = ItemPlacement;
				item.transform.Reset();
				item.gameObject.SetActive(ItemPlacement != transform);
				if (UI != null)
				{
					UI.UpdateSlot();
				}

				return true;
			}

			return false;
		}

		public Item RemoveItem()
		{
			Item item = Item;
			Item.Slot = null;
			Item = null;
			return item;
		}

		public bool CanFitItem(Item item)
		{
			if (item.IsSlotContainer && Parent is DynamicObject && (Parent as DynamicObject).Item.IsSlotContainer)
			{
				return false;
			}

			if (ItemTypes.Count == 0 && GenericSubTypes.Count == 0 && MachineryPartTypes.Count == 0)
			{
				return true;
			}

			if (item is GenericItem && GenericSubTypes.Contains((item as GenericItem).SubType))
			{
				return true;
			}

			if (item is MachineryPart && MachineryPartTypes.Contains((item as MachineryPart).PartType))
			{
				return true;
			}

			return ItemTypes.Contains(item.Type);
		}

		public Sprite GetIcon()
		{
			Sprite result = null;
			if (Item != null)
			{
				result = Item.Icon;
			}
			else if (ItemTypes.Count == 0 && GenericSubTypes.Count == 0 && MachineryPartTypes.Count == 0)
			{
				result = SpriteManager.Instance.DefaultItemSlot;
			}
			else if (ItemTypes.Count == 0 && MachineryPartTypes.Count == 0 && GenericSubTypes.Count == 1)
			{
				result = SpriteManager.Instance.GetSprite(GenericSubTypes[0]);
			}
			else if (ItemTypes.Count == 0 && MachineryPartTypes.Count == 1 && GenericSubTypes.Count == 0)
			{
				result = SpriteManager.Instance.GetSprite(MachineryPartTypes[0]);
			}
			else if (ItemTypes.Count == 1 && MachineryPartTypes.Count == 0 && GenericSubTypes.Count == 0)
			{
				result = SpriteManager.Instance.GetSprite(ItemTypes[0]);
			}

			return result;
		}

		public ItemSlotData GetData()
		{
			ItemSlotData itemSlotData = new ItemSlotData();
			itemSlotData.ID = ID;
			itemSlotData.ItemTypes = ItemTypes;
			itemSlotData.GenericSubTypes = GenericSubTypes;
			itemSlotData.MachineryPartTypes = MachineryPartTypes;
			itemSlotData.SpawnItem = SpawnItem;
			return itemSlotData;
		}
	}
}
