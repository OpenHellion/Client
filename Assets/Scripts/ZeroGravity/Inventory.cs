using System;
using System.Collections.Generic;
using System.Linq;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity
{
	public class Inventory : ISlotContainer
	{
		private Player m_parentPlayer;

		private Corpse m_parentCorpse;

		private MyPlayer m_myPlayer;

		private AnimatorHelper m_animHelper;

		private InventorySlot m_outfitSlot;

		private InventorySlot m_handsSlot;

		public SpaceObject Parent
		{
			get
			{
				if (m_parentPlayer != null)
				{
					return m_parentPlayer;
				}
				return m_parentCorpse;
			}
		}

		public Outfit Outfit { get; private set; }

		public InventorySlot OutfitSlot => m_outfitSlot;

		public InventorySlot HandsSlot => m_handsSlot;

		public Item ItemInHands => m_handsSlot.Item;

		public bool CanDrop => m_animHelper.CanDrop;

		public Item AnimationItem { get; private set; }

		public InventorySlot AnimationItemStartSlot { get; private set; }

		public InventorySlot AnimationItemDestinationSlot { get; private set; }

		public bool AnimationItemDropQueued { get; private set; }

		public Item.ItemAnimationType AnimationItemAnimType { get; private set; }

		public Inventory(Player parent, AnimatorHelper aHelper)
		{
			m_animHelper = aHelper;
			m_parentPlayer = parent;
			m_parentCorpse = null;
			if (parent is MyPlayer)
			{
				m_myPlayer = parent as MyPlayer;
			}
			m_handsSlot = InventorySlot.CreateHandsSlot(m_animHelper.GetBone(AnimatorHelper.HumanBones.RightInteractBone).gameObject, this);
			m_outfitSlot = InventorySlot.CreateOutfitSlot(Parent.gameObject, this);
		}

		public Inventory(Corpse parent, AnimatorHelper aHelper)
		{
			m_animHelper = aHelper;
			m_parentPlayer = null;
			m_parentCorpse = parent;
			m_myPlayer = null;
			m_handsSlot = InventorySlot.CreateHandsSlot(m_animHelper.GetBone(AnimatorHelper.HumanBones.RightInteractBone).gameObject, this);
			m_outfitSlot = InventorySlot.CreateOutfitSlot(Parent.gameObject, this);
		}

		public void SetAnimationItem(Item item, InventorySlot startSlot, InventorySlot destinationSlot, bool isDrop)
		{
			AnimationItem = item;
			AnimationItemStartSlot = startSlot;
			AnimationItemDestinationSlot = destinationSlot;
			AnimationItemDropQueued = isDrop;
		}

		public InventorySlot GetSlotByID(short id)
		{
			switch (id)
			{
			case -1:
				return m_handsSlot;
			case -2:
				return m_outfitSlot;
			default:
				if (Outfit != null)
				{
					return Outfit.GetSlotByID(id);
				}
				return null;
			}
		}

		public Dictionary<short, InventorySlot> GetAllSlots()
		{
			Dictionary<short, InventorySlot> dictionary = new Dictionary<short, InventorySlot>();
			dictionary.Add(-1, HandsSlot);
			dictionary.Add(-2, OutfitSlot);
			Dictionary<short, InventorySlot> dictionary2 = dictionary;
			if (Outfit != null)
			{
				return dictionary2.Union(Outfit.InventorySlots).ToDictionary((KeyValuePair<short, InventorySlot> k) => k.Key, (KeyValuePair<short, InventorySlot> v) => v.Value);
			}
			return dictionary2;
		}

		public Dictionary<short, InventorySlot> GetSlotsByGroup(InventorySlot.Group group)
		{
			return (from m in GetAllSlots()
				where m.Value.SlotGroup == @group
				select m).ToDictionary((KeyValuePair<short, InventorySlot> k) => k.Key, (KeyValuePair<short, InventorySlot> v) => v.Value);
		}

		public void SetOutfit(Outfit outfit)
		{
			if (Outfit != null && Outfit != outfit)
			{
				foreach (InventorySlot value in Outfit.InventorySlots.Values)
				{
					value.SetInventory(null);
				}
			}
			Outfit = outfit;
			if (Outfit != null)
			{
				foreach (InventorySlot value2 in Outfit.InventorySlots.Values)
				{
					value2.SetInventory(this);
				}
			}
			if (m_myPlayer != null)
			{
				m_myPlayer.CurrentOutfit = Outfit;
			}
			m_outfitSlot.SetItem(outfit, sendMessage: false);
			m_handsSlot.SetAttachPoint(ItemType.None, m_animHelper.GetBone(AnimatorHelper.HumanBones.RightInteractBone).gameObject, hideAttachedObject: false);
			if (m_parentPlayer != null)
			{
				m_parentPlayer.RefreshTargetingPoints();
			}
			if (Client.Instance.CanvasManager.InventoryUI.gameObject.activeInHierarchy)
			{
				Client.Instance.CanvasManager.InventoryUI.UpdateArmorAndHealth();
			}
		}

		public InventorySlot FindEmptyOutfitSlot(Item item, bool museBeEquip = false)
		{
			if (Outfit == null)
			{
				return null;
			}
			foreach (InventorySlot value in Outfit.InventorySlots.Values)
			{
				if (value.Item == null && (!museBeEquip || value.SlotType == InventorySlot.Type.Equip) && value.CanFitItem(item))
				{
					return value;
				}
			}
			return null;
		}

		public bool DropItem(Item item, InventorySlot oldSlot)
		{
			if (m_myPlayer == null)
			{
				return false;
			}
			item.RequestDrop();
			if (oldSlot != null)
			{
				if (oldSlot.SlotType == InventorySlot.Type.Hands && oldSlot.Item == item)
				{
					RemoveItemFromHands(resetStance: true);
				}
				else
				{
					oldSlot.SetItem(null);
				}
			}
			return true;
		}

		public void AnimationItem_EventStart()
		{
			if (AnimationItem != null && m_parentPlayer != null)
			{
				AnimationItem.AttachToBone(m_parentPlayer, AnimatorHelper.HumanBones.RightInteractBone);
			}
		}

		public void AnimationItem_EventEnd(int data)
		{
		}

		public bool ItemPlacedOnSlot(Item item, InventorySlot newSlot, InventorySlot oldSlot)
		{
			if (!newSlot.CanFitItem(item) || (newSlot.Item != null && !oldSlot.CanFitItem(newSlot.Item)))
			{
				oldSlot.SetItem(item);
				return true;
			}
			return AddToInventory(item, newSlot, oldSlot);
		}

		public bool AddToInventoryOrDrop(Item item, InventorySlot oldSlot)
		{
			if (item == null)
			{
				return true;
			}
			if (!AddToInventory(item, null, oldSlot))
			{
				return false;
			}
			return true;
		}

		public bool AddToInventory(Item item, InventorySlot newSlot, InventorySlot oldSlot)
		{
			if (item == null)
			{
				return true;
			}
			item.DynamicObj.CheckNearbyObjects();
			if (newSlot == null)
			{
				newSlot = FindEmptyOutfitSlot(item);
			}
			if (newSlot == null || !newSlot.CanFitItem(item))
			{
				return false;
			}
			if (newSlot.SlotType == InventorySlot.Type.Hands)
			{
				if (newSlot.Item != null)
				{
					if (m_handsSlot.Item.CanGoDirectlyToInventory)
					{
						if (oldSlot != null && oldSlot.CanFitItem(newSlot.Item))
						{
							oldSlot.SetItem(newSlot.Item);
						}
						else
						{
							AddToInventoryOrDrop(newSlot.Item, newSlot);
						}
					}
					else
					{
						DropItem(newSlot.Item, newSlot);
					}
				}
				newSlot.SetItem(item);
			}
			else
			{
				if (newSlot.Item != null)
				{
					if (oldSlot != null && oldSlot.CanFitItem(newSlot.Item))
					{
						oldSlot.SetItem(newSlot.Item);
					}
					else
					{
						AddToInventoryOrDrop(newSlot.Item, newSlot);
					}
				}
				newSlot.SetItem(item);
			}
			return true;
		}

		public bool CanAddToInventory(Item item)
		{
			return FindEmptyOutfitSlot(item) != null;
		}

		public void ReleasePrimaryForItemInHands()
		{
			if (m_handsSlot.Item != null)
			{
				m_handsSlot.Item.PrimaryReleased();
			}
		}

		public void ItemAddedToHands(Item item)
		{
			if (m_myPlayer != null)
			{
				m_myPlayer.animHelper.OverrideItemAnimations(item.fpsAnimations, item.Type, item.NeedsFullAnimOverride, item);
				m_myPlayer.ItemAddedToHands(item);
			}
			else if (Parent is OtherPlayer)
			{
				(Parent as OtherPlayer).tpsController.animHelper.OverrideItemAnimations(item.tpsAnimations, item.Type, item.NeedsFullAnimOverride, item);
			}
		}

		public void RemoveItemFromHands(bool resetStance)
		{
			m_handsSlot.SetItem(null);
			if (m_myPlayer != null && resetStance)
			{
				ExitCombatStance();
				m_myPlayer.ResetLookingAtItem();
			}
		}

		public bool StoreItemInHands()
		{
			return m_handsSlot.Item == null || AddToInventory(m_handsSlot.Item, null, m_handsSlot);
		}

		public void ExitCombatStance()
		{
			if (m_myPlayer != null)
			{
				m_myPlayer.ChangeStance(MyPlayer.PlayerStance.Passive, 1f);
				m_myPlayer.ResetCameraFov();
				m_animHelper.SetParameter(null, null, null, null, null, null, null, null, false);
			}
		}

		public bool CheckIfItemInHandsIsType<T>()
		{
			return m_handsSlot.Item != null && typeof(T).IsAssignableFrom(m_handsSlot.Item.GetType());
		}
	}
}
