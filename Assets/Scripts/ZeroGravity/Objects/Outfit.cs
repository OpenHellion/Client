using System;
using System.Collections.Generic;
using System.Linq;
using OpenHellion.IO;
using UnityEngine;
using ZeroGravity.Data;

namespace ZeroGravity.Objects
{
	public class Outfit : Item, ISlotContainer
	{
		[Serializable]
		public class SlotGroup
		{
			[HideInInspector]
			public InventorySlot.Size Size = InventorySlot.Size.One;

			public InventorySlot.Group Group;

			public List<SlotInfo> Slots;
		}

		[Serializable]
		public class SlotInfo
		{
			public InventorySlot.Type Type = InventorySlot.Type.General;

			public bool MustBeEmptyToRemoveOutfit;

			public InputManager.ConfigAction ShortcutKey;

			public int CycleIndex = -1;

			public List<InventorySlot.AttachData> ItemsToAttach = new List<InventorySlot.AttachData>();

			public List<ItemType> GetAttachableItemTypes()
			{
				List<ItemType> list = new List<ItemType>();
				if (ItemsToAttach != null && ItemsToAttach.Count > 0)
				{
					foreach (InventorySlot.AttachData item in ItemsToAttach)
					{
						list.Add(item.ItemType);
					}
					return list;
				}
				return list;
			}
		}

		[Serializable]
		public class Stats
		{
			public float DmgReductionTorso;

			public float DmgReductionAbdomen;

			public float DmgReductionArms;

			public float DmgReductionLegs;

			public float DmgResistanceTorso = 1f;

			public float DmgResistanceAbdomen = 1f;

			public float DmgResistanceArms = 1f;

			public float DmgResistanceLegs = 1f;

			public float CollisionResistance = 1f;

			public float MaxAngularVelocity = 20f;
		}

		public Transform OutfitTrans;

		public Transform FoldedOutfitTrans;

		public List<SkinnedMeshRenderer> ArmSkins;

		public Stats OutfitStats;

		private static Quaternion rootRotation = Quaternion.Euler(-0.0281f, 90f, -90f);

		[SerializeField]
		public List<SlotGroup> slotGroups;

		public Dictionary<short, InventorySlot> InventorySlots = new Dictionary<short, InventorySlot>();

		public int MaximumCycleIndex;

		public override bool IsInvetoryEquipable => true;

		public override EquipType EquipTo => EquipType.None;

		private new void Awake()
		{
			base.Awake();
			if (OutfitTrans == null)
			{
				OutfitTrans = base.transform.Find("Outfit");
			}
			if (FoldedOutfitTrans == null)
			{
				FoldedOutfitTrans = base.transform.Find("Folded");
			}
			short num = 1;
			foreach (SlotGroup slotGroup in slotGroups)
			{
				if (slotGroup.Size == (InventorySlot.Size)0)
				{
					slotGroup.Size = InventorySlot.Size.One;
				}
				InventorySlot.Group group = slotGroup.Group;
				foreach (SlotInfo slot in slotGroup.Slots)
				{
					if (MaximumCycleIndex < slot.CycleIndex)
					{
						MaximumCycleIndex = slot.CycleIndex;
					}
					if (slot.Type == InventorySlot.Type.Hands)
					{
						slot.Type = InventorySlot.Type.General;
					}
					foreach (InventorySlot.AttachData item in slot.ItemsToAttach)
					{
						if (item.Point == null)
						{
							item.Point = OutfitTrans.gameObject;
							item.HideAttachedObject = true;
						}
					}
					InputManager.ConfigAction? shortKey = null;
					if (slot.ShortcutKey != 0)
					{
						shortKey = slot.ShortcutKey;
					}
					int cycleIndex = slot.CycleIndex;
					InventorySlots.Add(num, new InventorySlot(this, num, slot.Type, slotGroup.Size, group, shortKey, cycleIndex, slot.MustBeEmptyToRemoveOutfit, null, slot.ItemsToAttach, null));
					num = (short)(num + 1);
				}
			}
			MaximumCycleIndex++;
		}

		public InventorySlot GetSlotByID(short slotID)
		{
			if (InventorySlots.ContainsKey(slotID))
			{
				return InventorySlots[slotID];
			}
			return null;
		}

		public Dictionary<short, InventorySlot> GetAllSlots()
		{
			return InventorySlots;
		}

		public Dictionary<short, InventorySlot> GetSlotsByGroup(InventorySlot.Group group)
		{
			return (from m in GetAllSlots()
				where m.Value.SlotGroup == @group
				select m).ToDictionary((KeyValuePair<short, InventorySlot> k) => k.Key, (KeyValuePair<short, InventorySlot> v) => v.Value);
		}

		public bool CanRemoveOutfit()
		{
			return base.InvSlot == null || base.InvSlot.Inventory.HandsSlot.Item == null;
		}

		public override void Special()
		{
			RequestAttach(MyPlayer.Instance.Inventory.OutfitSlot);
		}

		public void EquipOutfit(Player pl, bool checkHands)
		{
			if (pl.Inventory.Outfit != null || (checkHands && pl.Inventory.ItemInHands != this))
			{
				return;
			}
			FoldedOutfitTrans.gameObject.SetActive(value: false);
			if (pl is MyPlayer)
			{
				MyPlayer myPlayer = pl as MyPlayer;
				if (checkHands)
				{
					myPlayer.Inventory.RemoveItemFromHands(resetStance: true);
				}
				ReparentCurrentOutfit();
				myPlayer.CurrentOutfit = this;
				SetOutfitParent(OutfitTrans.GetChildren(), myPlayer.Outfit, activateGeometry: true);
				myPlayer.RefreshOutfitData();
				myPlayer.Inventory.SetOutfit(this);
				myPlayer.FpsController.RefreshMaxAngularVelocity();
				InventorySlot inventorySlot = myPlayer.CurrentOutfit.GetSlotsByGroup(InventorySlot.Group.Helmet).Values.FirstOrDefault();
				if (inventorySlot != null && inventorySlot.Item != null)
				{
					Helmet helmet = inventorySlot.Item as Helmet;
					helmet.ChangeEquip(EquipType.EquipInventory, myPlayer);
					helmet.gameObject.SetActive(value: true);
				}
				InventorySlot inventorySlot2 = myPlayer.CurrentOutfit.GetSlotsByGroup(InventorySlot.Group.Jetpack).Values.FirstOrDefault();
				if (inventorySlot2 != null && inventorySlot2.Item != null)
				{
					Jetpack jetpack = inventorySlot2.Item as Jetpack;
					jetpack.ChangeEquip(EquipType.EquipInventory, myPlayer);
					jetpack.gameObject.SetActive(value: true);
				}
				ImpactDetector componentInChildren = MyPlayer.Instance.GetComponentInChildren<ImpactDetector>(includeInactive: true);
				if (componentInChildren != null)
				{
					ImpactDetector impactDetector = MyPlayer.Instance.FpsController.ragdollChestRigidbody.gameObject.AddComponent<ImpactDetector>();
					impactDetector.ImpactSound = componentInChildren.ImpactSound;
					impactDetector.VelocityThrashold = componentInChildren.VelocityThrashold;
					impactDetector.CooldownTime = componentInChildren.CooldownTime;
				}
			}
			if (Client.Instance.CanvasManager.PlayerOverview.Inventory.gameObject.activeInHierarchy)
			{
				Client.Instance.CanvasManager.PlayerOverview.Inventory.InitializeSlots();
				Client.Instance.CanvasManager.PlayerOverview.Inventory.UpdateUI();
			}
		}

		private void ReparentCurrentOutfit()
		{
			MyPlayer instance = MyPlayer.Instance;
			instance.ToggleMeshRendereres(enableMesh: true);
			if (instance.CurrentOutfit != null)
			{
				instance.CurrentOutfit.SetOutfitParent(instance.Outfit.GetChildren(), instance.CurrentOutfit.OutfitTrans, activateGeometry: false);
				instance.CurrentOutfit.FoldedOutfitTrans.gameObject.SetActive(value: true);
				return;
			}
			foreach (Transform child in instance.Outfit.GetChildren())
			{
				child.parent = instance.BasicOutfitHolder;
				child.gameObject.SetActive(value: false);
			}
		}

		public void SetOutfitParent(List<Transform> children, Transform parentTransform, bool activateGeometry)
		{
			foreach (Transform child in children)
			{
				child.parent = parentTransform;
				child.localScale = Vector3.one;
				child.localPosition = Vector3.zero;
				child.localRotation = ((!(child.name == "Root")) ? Quaternion.identity : rootRotation);
				if (!child.GetComponent<Item>())
				{
					child.gameObject.SetActive(activateGeometry);
				}
			}
		}

		public void TakeOffOutfit(bool isDeath = false, bool sendToServer = true)
		{
			if (base.InvSlot != null && base.InvSlotID != -2)
			{
				return;
			}
			Player player = base.InvSlot.Parent as Player;
			if (player == null || player.Inventory.Outfit != this || (!isDeath && !player.Inventory.Outfit.CanRemoveOutfit()))
			{
				return;
			}
			if (player is MyPlayer)
			{
				MyPlayer myPlayer = player as MyPlayer;
				InventorySlot inventorySlot = myPlayer.CurrentOutfit.GetSlotsByGroup(InventorySlot.Group.Helmet).Values.FirstOrDefault();
				if (inventorySlot != null && inventorySlot.Item != null)
				{
					Helmet helmet = inventorySlot.Item as Helmet;
					helmet.ChangeEquip(EquipType.Inventory, myPlayer);
					helmet.gameObject.SetActive(value: false);
					helmet.transform.parent = base.transform;
				}
				InventorySlot inventorySlot2 = myPlayer.CurrentOutfit.GetSlotsByGroup(InventorySlot.Group.Jetpack).Values.FirstOrDefault();
				if (inventorySlot2 != null && inventorySlot2.Item != null)
				{
					Jetpack jetpack = inventorySlot2.Item as Jetpack;
					jetpack.ChangeEquip(EquipType.Inventory, myPlayer);
					jetpack.gameObject.SetActive(value: false);
					jetpack.transform.parent = base.transform;
				}
				ReparentCurrentOutfit();
				foreach (Transform child in myPlayer.BasicOutfitHolder.GetChildren())
				{
					child.parent = myPlayer.Outfit;
					child.localPosition = Vector3.zero;
					child.localRotation = ((!(child.name == "Root")) ? Quaternion.identity : rootRotation);
					child.gameObject.SetActive(value: true);
				}
				if (!isDeath && myPlayer.Inventory.ItemInHands != null)
				{
					myPlayer.Inventory.ItemInHands.RequestDrop();
				}
				myPlayer.RefreshOutfitData();
				myPlayer.Inventory.SetOutfit(null);
				if (!isDeath)
				{
					myPlayer.Inventory.AddToInventory(this, myPlayer.Inventory.HandsSlot, null);
				}
				myPlayer.CurrentOutfit = null;
				myPlayer.FpsController.RefreshMaxAngularVelocity();
			}
			if (Client.Instance.CanvasManager.PlayerOverview.Inventory.gameObject.activeInHierarchy)
			{
				Client.Instance.CanvasManager.PlayerOverview.Inventory.InitializeSlots();
				Client.Instance.CanvasManager.PlayerOverview.Inventory.UpdateUI();
			}
		}

		public override DynamicObjectAuxData GetAuxData()
		{
			List<InventorySlotData> list = new List<InventorySlotData>();
			short num = 1;
			foreach (SlotGroup slotGroup in slotGroups)
			{
				foreach (SlotInfo slot in slotGroup.Slots)
				{
					list.Add(new InventorySlotData
					{
						SlotID = num++,
						SlotType = slot.Type,
						MustBeEmptyToRemoveOutfit = slot.MustBeEmptyToRemoveOutfit,
						ItemTypes = slot.GetAttachableItemTypes()
					});
				}
			}
			OutfitData baseAuxData = GetBaseAuxData<OutfitData>();
			baseAuxData.InventorySlots = list;
			baseAuxData.DamageReductionTorso = OutfitStats.DmgReductionTorso;
			baseAuxData.DamageReductionAbdomen = OutfitStats.DmgReductionAbdomen;
			baseAuxData.DamageReductionArms = OutfitStats.DmgReductionArms;
			baseAuxData.DamageReductionLegs = OutfitStats.DmgReductionLegs;
			baseAuxData.DamageResistanceTorso = OutfitStats.DmgResistanceTorso;
			baseAuxData.DamageResistanceAbdomen = OutfitStats.DmgResistanceAbdomen;
			baseAuxData.DamageResistanceArms = OutfitStats.DmgResistanceArms;
			baseAuxData.DamageResistanceLegs = OutfitStats.DmgResistanceLegs;
			baseAuxData.CollisionResistance = OutfitStats.CollisionResistance;
			return baseAuxData;
		}

		public List<Item> AllItems()
		{
			List<Item> list = new List<Item>();
			foreach (KeyValuePair<short, InventorySlot> inventorySlot in InventorySlots)
			{
				if (inventorySlot.Value.Item != null)
				{
					list.Add(inventorySlot.Value.Item);
				}
			}
			if (list.Count == 0)
			{
				return null;
			}
			return list;
		}
	}
}
