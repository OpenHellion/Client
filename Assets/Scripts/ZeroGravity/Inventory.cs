using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity
{
	public class Inventory : ISlotContainer
	{
		[CompilerGenerated]
		private sealed class _003CGetSlotsByGroup_003Ec__AnonStorey0
		{
			internal InventorySlot.Group group;

			internal bool _003C_003Em__0(KeyValuePair<short, InventorySlot> m)
			{
				return m.Value.SlotGroup == group;
			}
		}

		private Player parentPlayer;

		private Corpse parentCorpse;

		private MyPlayer myPlayer;

		private AnimatorHelper animHelper;

		private InventorySlot _OutfitSlot;

		private InventorySlot _HandsSlot;

		[CompilerGenerated]
		private static Func<KeyValuePair<short, InventorySlot>, short> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<KeyValuePair<short, InventorySlot>, InventorySlot> _003C_003Ef__am_0024cache1;

		[CompilerGenerated]
		private static Func<KeyValuePair<short, InventorySlot>, short> _003C_003Ef__am_0024cache2;

		[CompilerGenerated]
		private static Func<KeyValuePair<short, InventorySlot>, InventorySlot> _003C_003Ef__am_0024cache3;

		public SpaceObject Parent
		{
			get
			{
				if (parentPlayer != null)
				{
					return parentPlayer;
				}
				return parentCorpse;
			}
		}

		public Outfit Outfit { get; private set; }

		public InventorySlot OutfitSlot
		{
			get
			{
				return _OutfitSlot;
			}
		}

		public InventorySlot HandsSlot
		{
			get
			{
				return _HandsSlot;
			}
		}

		public Item ItemInHands
		{
			get
			{
				return _HandsSlot.Item;
			}
		}

		public bool CanDrop
		{
			get
			{
				return animHelper.CanDrop;
			}
		}

		public Item AnimationItem { get; private set; }

		public InventorySlot AnimationItemStartSlot { get; private set; }

		public InventorySlot AnimationItemDestinationSlot { get; private set; }

		public bool AnimationItemDropQueued { get; private set; }

		public Item.ItemAnimationType AnimationItemAnimType { get; private set; }

		public Inventory(Player parent, AnimatorHelper aHelper)
		{
			animHelper = aHelper;
			parentPlayer = parent;
			parentCorpse = null;
			if (parent is MyPlayer)
			{
				myPlayer = parent as MyPlayer;
			}
			_HandsSlot = InventorySlot.CreateHandsSlot(animHelper.GetBone(AnimatorHelper.HumanBones.RightInteractBone).gameObject, this);
			_OutfitSlot = InventorySlot.CreateOutfitSlot(Parent.gameObject, this);
		}

		public Inventory(Corpse parent, AnimatorHelper aHelper)
		{
			animHelper = aHelper;
			parentPlayer = null;
			parentCorpse = parent;
			myPlayer = null;
			_HandsSlot = InventorySlot.CreateHandsSlot(animHelper.GetBone(AnimatorHelper.HumanBones.RightInteractBone).gameObject, this);
			_OutfitSlot = InventorySlot.CreateOutfitSlot(Parent.gameObject, this);
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
				return _HandsSlot;
			case -2:
				return _OutfitSlot;
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
				IEnumerable<KeyValuePair<short, InventorySlot>> source = dictionary2.Union(Outfit.InventorySlots);
				if (_003C_003Ef__am_0024cache0 == null)
				{
					_003C_003Ef__am_0024cache0 = _003CGetAllSlots_003Em__0;
				}
				Func<KeyValuePair<short, InventorySlot>, short> keySelector = _003C_003Ef__am_0024cache0;
				if (_003C_003Ef__am_0024cache1 == null)
				{
					_003C_003Ef__am_0024cache1 = _003CGetAllSlots_003Em__1;
				}
				return source.ToDictionary(keySelector, _003C_003Ef__am_0024cache1);
			}
			return dictionary2;
		}

		public Dictionary<short, InventorySlot> GetSlotsByGroup(InventorySlot.Group group)
		{
			_003CGetSlotsByGroup_003Ec__AnonStorey0 _003CGetSlotsByGroup_003Ec__AnonStorey = new _003CGetSlotsByGroup_003Ec__AnonStorey0();
			_003CGetSlotsByGroup_003Ec__AnonStorey.group = group;
			IEnumerable<KeyValuePair<short, InventorySlot>> source = GetAllSlots().Where(_003CGetSlotsByGroup_003Ec__AnonStorey._003C_003Em__0);
			if (_003C_003Ef__am_0024cache2 == null)
			{
				_003C_003Ef__am_0024cache2 = _003CGetSlotsByGroup_003Em__2;
			}
			Func<KeyValuePair<short, InventorySlot>, short> keySelector = _003C_003Ef__am_0024cache2;
			if (_003C_003Ef__am_0024cache3 == null)
			{
				_003C_003Ef__am_0024cache3 = _003CGetSlotsByGroup_003Em__3;
			}
			return source.ToDictionary(keySelector, _003C_003Ef__am_0024cache3);
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
			if (myPlayer != null)
			{
				myPlayer.CurrentOutfit = Outfit;
			}
			_OutfitSlot.SetItem(outfit, false);
			_HandsSlot.SetAttachPoint(ItemType.None, animHelper.GetBone(AnimatorHelper.HumanBones.RightInteractBone).gameObject, false);
			if (parentPlayer != null)
			{
				parentPlayer.RefreshTargetingPoints();
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
			if (myPlayer == null)
			{
				return false;
			}
			item.RequestDrop();
			if (oldSlot != null)
			{
				if (oldSlot.SlotType == InventorySlot.Type.Hands && oldSlot.Item == item)
				{
					RemoveItemFromHands(true);
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
			if (AnimationItem != null && parentPlayer != null)
			{
				AnimationItem.AttachToBone(parentPlayer, AnimatorHelper.HumanBones.RightInteractBone);
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
					if (_HandsSlot.Item.CanGoDirectlyToInventory)
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
			if (_HandsSlot.Item != null)
			{
				_HandsSlot.Item.PrimaryReleased();
			}
		}

		public void ItemAddedToHands(Item item)
		{
			if (myPlayer != null)
			{
				myPlayer.animHelper.OverrideItemAnimations(item.fpsAnimations, item.Type, item.NeedsFullAnimOverride, item);
				myPlayer.ItemAddedToHands(item);
			}
			else if (Parent is OtherPlayer)
			{
				(Parent as OtherPlayer).tpsController.animHelper.OverrideItemAnimations(item.tpsAnimations, item.Type, item.NeedsFullAnimOverride, item);
			}
		}

		public void RemoveItemFromHands(bool resetStance)
		{
			_HandsSlot.SetItem(null);
			if (myPlayer != null && resetStance)
			{
				ExitCombatStance();
				myPlayer.ResetLookingAtItem();
			}
		}

		public bool StoreItemInHands()
		{
			return _HandsSlot.Item == null || AddToInventory(_HandsSlot.Item, null, _HandsSlot);
		}

		public void ExitCombatStance()
		{
			if (myPlayer != null)
			{
				myPlayer.ChangeStance(MyPlayer.PlayerStance.Passive, 1f);
				myPlayer.ResetCameraFov();
				animHelper.SetParameter(null, null, null, null, null, null, null, null, false);
			}
		}

		public bool CheckIfItemInHandsIsType<T>()
		{
			return _HandsSlot.Item != null && typeof(T).IsAssignableFrom(_HandsSlot.Item.GetType());
		}

		[CompilerGenerated]
		private static short _003CGetAllSlots_003Em__0(KeyValuePair<short, InventorySlot> k)
		{
			return k.Key;
		}

		[CompilerGenerated]
		private static InventorySlot _003CGetAllSlots_003Em__1(KeyValuePair<short, InventorySlot> v)
		{
			return v.Value;
		}

		[CompilerGenerated]
		private static short _003CGetSlotsByGroup_003Em__2(KeyValuePair<short, InventorySlot> k)
		{
			return k.Key;
		}

		[CompilerGenerated]
		private static InventorySlot _003CGetSlotsByGroup_003Em__3(KeyValuePair<short, InventorySlot> v)
		{
			return v.Value;
		}
	}
}
