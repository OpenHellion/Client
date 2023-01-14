using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity
{
	public class InventoryUI : MonoBehaviour
	{
		public GameObject NoSuit;

		public GameObject ItemsHolder;

		public List<AbstractSlotUI> AllSlots = new List<AbstractSlotUI>();

		public List<AbstractSlotUI> LootSlots = new List<AbstractSlotUI>();

		public List<AbstractSlotUI> ApSlots = new List<AbstractSlotUI>();

		public List<AbstractSlotUI> ContainerItemSlots = new List<AbstractSlotUI>();

		public List<AbstractSlotUI> SelectedItemSlots = new List<AbstractSlotUI>();

		public InventoryGroupUI ProximityGroup;

		public InventoryGroupUI LootGroup;

		public InventoryGroupUI SlotGroup;

		[Title("Slots")]
		public Transform GroupsHolder;

		public InventoryGroupUI GroupPrefab;

		public InventorySlotUI SlotPrefab;

		public ItemSlotUI ItemSlotPrefab;

		public AttachPointSlotUI AttachPointPrefab;

		[Title("PlayerInfo")]
		public Text PlayerName;

		public Image CharacterPreview;

		public Text ArmorValue;

		public Image HeartBeat;

		public Animator Hb;

		public Text HealthValue;

		[Title("CharacterSlots")]
		public Transform HandSlot;

		public Transform PrimarySlot;

		public Transform SecondarySlot;

		public Transform SuitSlot;

		public Transform HelmetSlot;

		public Transform JetpackSlot;

		[Title("SelectedItem")]
		public Item SelectedItem;

		public GameObject SelectedItemHolder;

		public TooltipInventory SelectedItemData;

		public GameObject RemoveOutfitButton;

		public GameObject DropButton;

		public GameObject ContainerSlotsButton;

		public Transform ItemSlotsHolder;

		[Title("Tooltip")]
		public AbstractSlotUI HoveredSlot;

		public TooltipInventory ToolTip;

		public bool CanShowTooltip;

		[Title("Dragging")]
		public bool IsDragging;

		public Item DraggingItem;

		public GameObject DraggingObject;

		[Title("Other")]
		public GameObject OtherHolder;

		public Transform OtherGroupsHolder;

		public object LootingTarget;

		public HashSet<BaseSceneAttachPoint> APLootingList = new HashSet<BaseSceneAttachPoint>();

		private float lootRadius = 1.65f;

		[Title("ItemSlots")]
		public bool ShowItemSlots;

		public Inventory Inventory => MyPlayer.Instance.Inventory;

		private void Start()
		{
			PlayerName.text = MyPlayer.Instance.PlayerName;
		}

		private void Update()
		{
			if (HoveredSlot == null && ToolTip.gameObject.activeInHierarchy)
			{
				HideTooltip();
			}
			if (OtherHolder.activeInHierarchy && LootSlots.Count <= 0 && ApSlots.Count <= 0 && ContainerItemSlots.Count <= 0)
			{
				OtherHolder.SetActive(value: false);
			}
			else if (!OtherHolder.activeInHierarchy && (LootSlots.Count > 0 || (ApSlots.Count > 0 && ContainerItemSlots.Count > 0)))
			{
				OtherHolder.SetActive(value: true);
			}
			if (ApSlots.Count > 0)
			{
				ProximityLootRefresh();
			}
		}

		public void Toggle(bool val)
		{
			InitializeSlots();
			base.gameObject.SetActive(val);
			if (val)
			{
				ClearOther();
				UpdateUI();
				CheckLooting();
				CheckProximityAttachPoints();
				UpdateArmorAndHealth();
				InventoryCharacterPreview.instance.ResetPosition();
				this.CancelInvoke(RefreshPreviewCharacter);
				this.Invoke(RefreshPreviewCharacter, 0.1f);
			}
		}

		private void RefreshPreviewCharacter()
		{
			InventoryCharacterPreview.instance.RefreshPreviewCharacter(Inventory);
		}

		private void ClearOther()
		{
			OtherHolder.DestroyAll<InventoryGroupUI>();
			LootSlots.Clear();
			ApSlots.Clear();
			ContainerItemSlots.Clear();
			OtherHolder.SetActive(value: false);
		}

		public void UpdateUI()
		{
			NoSuit.SetActive(Inventory.OutfitSlot.Item == null);
			ItemsHolder.SetActive(Inventory.OutfitSlot.Item != null);
			DeselectItem();
			HideTooltip();
			DraggingItem = null;
			DraggingObject.SetActive(value: false);
			IsDragging = false;
		}

		public void SelectItem(Item itm)
		{
			UpdateSelectedItemInfo(itm);
			RefreshSlots();
		}

		public void UpdateSelectedItemInfo(Item itm)
		{
			SelectedItem = itm;
			SelectedItemData.CurrentItem = itm;
			SelectedItemData.FillData(itm, recycle: true);
			SelectedItemHolder.Activate(value: true);
			RemoveOutfitButton.SetActive(SelectedItem == SuitSlot.GetComponentInChildren<InventorySlotUI>().Item);
			RemoveOutfitButton.GetComponent<Button>().interactable = itm is Outfit && (itm as Outfit).CanRemoveOutfit();
			DropButton.SetActive(SelectedItem != SuitSlot.GetComponentInChildren<InventorySlotUI>().Item);
			ContainerSlotsButton.SetActive(SelectedItem.IsSlotContainer);
			ItemSlotsHolder.DestroyAll<AbstractSlotUI>();
			foreach (AbstractSlotUI selectedItemSlot in SelectedItemSlots)
			{
				AllSlots.Remove(selectedItemSlot);
			}
			SelectedItemSlots.Clear();
			if (itm.IsSlotContainer || itm.Slots.Count <= 0)
			{
				return;
			}
			foreach (ItemSlot value in itm.Slots.Values)
			{
				CreateItemSlot(value, ItemSlotsHolder);
			}
		}

		public void DeselectItem()
		{
			SelectedItem = null;
			ShowItemSlots = false;
			CheckItemSlots();
			SelectedItemHolder.SetActive(value: false);
			RefreshSlots();
		}

		public void GetTooltip()
		{
			if (!IsDragging && CanShowTooltip && !(HoveredSlot == null) && !(HoveredSlot.Item == null))
			{
				ToolTip.gameObject.SetActive(value: false);
				ToolTip.SetTooltip(HoveredSlot.Item);
				float x = ((HoveredSlot.GetComponent<RectTransform>().position.x > 0f) ? 1 : 0);
				float y = ((HoveredSlot.GetComponent<RectTransform>().position.y > 0f) ? 1 : 0);
				ToolTip.GetComponent<RectTransform>().pivot = new Vector2(x, y);
				ToolTip.GetComponent<RectTransform>().position = HoveredSlot.GetComponent<RectTransform>().position;
				ToolTip.gameObject.SetActive(value: true);
			}
		}

		public void HideTooltip()
		{
			CanShowTooltip = false;
			HoveredSlot = null;
			ToolTip.gameObject.SetActive(value: false);
		}

		public void RemoveOutfit()
		{
			if (MyPlayer.Instance.CurrentOutfit != null && MyPlayer.Instance.CurrentOutfit.CanRemoveOutfit())
			{
				MyPlayer.Instance.CurrentOutfit.RequestAttach(MyPlayer.Instance.Inventory.HandsSlot);
			}
		}

		public void DropItem()
		{
			SelectedItem.RequestDrop();
			UpdateUI();
		}

		public void RefreshSlots()
		{
			foreach (AbstractSlotUI allSlot in AllSlots)
			{
				if (allSlot != null)
				{
					if (SelectedItem != null && allSlot.Item == SelectedItem)
					{
						allSlot.Selected.SetActive(value: true);
					}
					else
					{
						allSlot.Selected.SetActive(value: false);
					}
				}
			}
		}

		public void InitializeSlots()
		{
			base.transform.DestroyAll<InventorySlotUI>();
			base.transform.DestroyAll<InventoryGroupUI>();
			AllSlots.Clear();
			foreach (InventorySlot.Group item in Inventory.GetAllSlots().Values.Select((InventorySlot m) => m.SlotGroup).Distinct())
			{
				switch (item)
				{
				case InventorySlot.Group.Outfit:
					CreateInventorySlot(Inventory.GetSlotsByGroup(item).First().Value, SuitSlot);
					continue;
				case InventorySlot.Group.Hands:
					CreateInventorySlot(Inventory.GetSlotsByGroup(item).First().Value, HandSlot);
					continue;
				case InventorySlot.Group.Helmet:
					CreateInventorySlot(Inventory.GetSlotsByGroup(item).First().Value, HelmetSlot);
					continue;
				case InventorySlot.Group.Jetpack:
					CreateInventorySlot(Inventory.GetSlotsByGroup(item).First().Value, JetpackSlot);
					continue;
				case InventorySlot.Group.Primary:
					CreateInventorySlot(Inventory.GetSlotsByGroup(item).First().Value, PrimarySlot);
					continue;
				case InventorySlot.Group.Secondary:
					CreateInventorySlot(Inventory.GetSlotsByGroup(item).First().Value, SecondarySlot);
					continue;
				}
				InventoryGroupUI inventoryGroupUI = Instantiate(GroupPrefab, GroupsHolder);
				inventoryGroupUI.transform.Reset();
				inventoryGroupUI.Name.text = item.ToLocalizedString().ToUpper();
				inventoryGroupUI.Inventory = this;
				foreach (InventorySlot value in Inventory.GetSlotsByGroup(item).Values)
				{
					CreateInventorySlot(value, inventoryGroupUI.SlotHolder);
				}
			}
		}

		private void SetDefaultSlots(InventorySlotUI slotUI, InventorySlot.Group group)
		{
			slotUI.Slot = Inventory.GetSlotsByGroup(group).First().Value;
			slotUI.Slot.UI = slotUI;
			slotUI.UpdateSlot();
			AllSlots.Add(slotUI);
		}

		private void CreateInventorySlot(InventorySlot slot, Transform holder, bool lootSlot = false)
		{
			GameObject gameObject = Instantiate(SlotPrefab.gameObject, holder);
			gameObject.transform.Reset();
			InventorySlotUI componentInChildren = gameObject.GetComponentInChildren<InventorySlotUI>();
			componentInChildren.Draggable = slot.SlotGroup != InventorySlot.Group.Outfit || !(slot.Parent is MyPlayer);
			componentInChildren.InventoryUI = this;
			componentInChildren.Slot = slot;
			componentInChildren.UpdateSlot();
			slot.UI = componentInChildren;
			slot.UI.IsLootSlot = lootSlot;
			if (Colors.ItemSlot.TryGetValue(slot.SlotGroup, out var value))
			{
				slot.UI.GetComponent<Image>().color = value;
			}
			else
			{
				slot.UI.GetComponent<Image>().color = Colors.Gray;
			}
			AllSlots.Add(componentInChildren);
			LootSlots.Add(componentInChildren);
		}

		private void CreateItemSlot(ItemSlot slot, Transform holder, bool looting = false, bool isContainer = false)
		{
			GameObject gameObject = Instantiate(ItemSlotPrefab.gameObject, holder);
			gameObject.transform.Reset();
			ItemSlotUI componentInChildren = gameObject.GetComponentInChildren<ItemSlotUI>();
			componentInChildren.InventoryUI = this;
			componentInChildren.Slot = slot;
			componentInChildren.UpdateSlot();
			slot.UI = componentInChildren;
			slot.UI.IsLootSlot = false;
			slot.UI.GetComponent<Image>().color = Colors.Gray;
			AllSlots.Add(componentInChildren);
			if (isContainer)
			{
				ContainerItemSlots.Add(componentInChildren);
			}
			else if (looting)
			{
				LootSlots.Add(componentInChildren);
			}
			else
			{
				SelectedItemSlots.Add(componentInChildren);
			}
		}

		private void CreateAPSlot(BaseSceneAttachPoint ap, Transform holder, bool lootSlot = false)
		{
			GameObject gameObject = Instantiate(AttachPointPrefab.gameObject, holder);
			gameObject.transform.Reset();
			AttachPointSlotUI componentInChildren = gameObject.GetComponentInChildren<AttachPointSlotUI>();
			componentInChildren.InventoryUI = this;
			componentInChildren.AttachPoint = ap;
			componentInChildren.UpdateSlot();
			ap.UI = componentInChildren;
			ap.UI.IsLootSlot = lootSlot;
			if (Colors.AttachPointSlotColor.TryGetValue(ap.GetType(), out var value))
			{
				ap.UI.GetComponent<Image>().color = value;
			}
			else
			{
				ap.UI.GetComponent<Image>().color = Colors.Gray;
			}
			AllSlots.Add(componentInChildren);
			ApSlots.Add(componentInChildren);
		}

		public void CheckLooting()
		{
			if (LootGroup != null)
			{
				LootGroup.SlotHolder.DestroyAll<AbstractSlotUI>();
				Destroy(LootGroup.gameObject);
				LootGroup = null;
			}
			foreach (AbstractSlotUI lootSlot in LootSlots)
			{
				AllSlots.Remove(lootSlot);
			}
			LootSlots.Clear();
			if (LootingTarget == null)
			{
				return;
			}
			InventoryGroupUI inventoryGroupUI = Instantiate(GroupPrefab, OtherGroupsHolder.transform);
			inventoryGroupUI.transform.Reset();
			inventoryGroupUI.transform.SetAsFirstSibling();
			inventoryGroupUI.Name.text = Localization.Loot.ToUpper();
			inventoryGroupUI.Inventory = this;
			inventoryGroupUI.LootAllButton.SetActive(value: true);
			LootGroup = inventoryGroupUI;
			if (LootingTarget is ISlotContainer)
			{
				foreach (InventorySlot value in (LootingTarget as ISlotContainer).GetAllSlots().Values)
				{
					if (!(value.Item == null))
					{
						CreateInventorySlot(value, LootGroup.SlotHolder, lootSlot: true);
					}
				}
			}
			else if (LootingTarget is Item)
			{
				foreach (ItemSlot value2 in (LootingTarget as Item).Slots.Values)
				{
					CreateItemSlot(value2, LootGroup.SlotHolder, looting: true);
				}
			}
			OtherHolder.SetActive(value: true);
		}

		public void CheckItemSlots()
		{
			if (SlotGroup != null)
			{
				SlotGroup.SlotHolder.DestroyAll<AbstractSlotUI>();
				Destroy(SlotGroup.gameObject);
				SlotGroup = null;
			}
			foreach (AbstractSlotUI containerItemSlot in ContainerItemSlots)
			{
				AllSlots.Remove(containerItemSlot);
			}
			ContainerItemSlots.Clear();
			if (!(SelectedItem != null) || !ShowItemSlots)
			{
				return;
			}
			InventoryGroupUI inventoryGroupUI = Instantiate(GroupPrefab, OtherGroupsHolder.transform);
			inventoryGroupUI.transform.Reset();
			inventoryGroupUI.transform.SetAsFirstSibling();
			inventoryGroupUI.Name.text = SelectedItem.Name;
			inventoryGroupUI.Inventory = this;
			inventoryGroupUI.LootAllButton.SetActive(value: true);
			SlotGroup = inventoryGroupUI;
			foreach (ItemSlot value in SelectedItem.Slots.Values)
			{
				CreateItemSlot(value, SlotGroup.SlotHolder, looting: false, isContainer: true);
			}
			OtherHolder.SetActive(value: true);
		}

		public void CheckProximityAttachPoints()
		{
			if (ProximityGroup != null)
			{
				ProximityGroup.SlotHolder.DestroyAll<AbstractSlotUI>();
				Destroy(ProximityGroup.gameObject);
				ProximityGroup = null;
			}
			foreach (AttachPointSlotUI apSlot in ApSlots)
			{
				AllSlots.Remove(apSlot);
			}
			ApSlots.Clear();
			APLootingList.Clear();
			fillAPList(APLootingList);
			if (APLootingList.Count <= 0)
			{
				return;
			}
			InventoryGroupUI inventoryGroupUI = Instantiate(GroupPrefab, OtherGroupsHolder.transform);
			inventoryGroupUI.transform.Reset();
			inventoryGroupUI.transform.SetAsFirstSibling();
			inventoryGroupUI.Name.text = Localization.ProximityLoot.ToUpper();
			inventoryGroupUI.LootAllButton.SetActive(value: true);
			inventoryGroupUI.Inventory = this;
			ProximityGroup = inventoryGroupUI;
			foreach (BaseSceneAttachPoint item in APLootingList.OrderBy((BaseSceneAttachPoint m) => m.StandardTip))
			{
				if (item.Item == null || (Inventory.Parent is MyPlayer && item.Item.CanPlayerPickUp(Inventory.Parent as MyPlayer)))
				{
					CreateAPSlot(item, ProximityGroup.SlotHolder);
				}
			}
			OtherHolder.SetActive(value: true);
		}

		private void ProximityLootRefresh()
		{
			HashSet<BaseSceneAttachPoint> hashSet = new HashSet<BaseSceneAttachPoint>();
			fillAPList(hashSet);
			if (hashSet.Except(APLootingList).Count() > 0 || APLootingList.Except(hashSet).Count() > 0)
			{
				CheckProximityAttachPoints();
			}
		}

		private void fillAPList(HashSet<BaseSceneAttachPoint> list)
		{
			Collider[] array = Physics.OverlapSphere(MyPlayer.Instance.FpsController.MainCamera.transform.position, lootRadius * 1.5f);
			foreach (Collider collider in array)
			{
				BaseSceneAttachPoint componentInParent = collider.GetComponentInParent<BaseSceneAttachPoint>();
				if (!(componentInParent != null))
				{
					continue;
				}
				Transform[] componentsInChildren = componentInParent.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform in componentsInChildren)
				{
					Vector3 rhs = transform.position - MyPlayer.Instance.FpsController.MainCamera.transform.position;
					if (rhs.magnitude <= lootRadius && Vector3.Dot(MyPlayer.Instance.FpsController.MainCamera.transform.forward, rhs) > 0f)
					{
						list.Add(componentInParent);
						break;
					}
				}
			}
		}

		public void LootAll(Transform holder)
		{
			List<InventorySlot> skipSlots = new List<InventorySlot>();
			foreach (AbstractSlotUI slot in from m in holder.GetComponentsInChildren<AbstractSlotUI>()
				where m.Item != null
				select m)
			{
				InventorySlot inventorySlot = Inventory.GetAllSlots().Values.OrderBy((InventorySlot m) => m.SlotType == InventorySlot.Type.Hands).FirstOrDefault((InventorySlot m) => m.CanFitItem(slot.Item) && m.Item == null && !skipSlots.Contains(m));
				if (inventorySlot != null)
				{
					slot.Item.RequestAttach(inventorySlot);
					skipSlots.Add(inventorySlot);
				}
			}
		}

		public void ToggleItemSlots()
		{
			ShowItemSlots = !ShowItemSlots;
			CheckItemSlots();
		}

		public void UpdateArmorAndHealth()
		{
			if (Inventory.OutfitSlot.Item == null)
			{
				ArmorValue.text = "0 HP/s";
			}
			else
			{
				ArmorValue.text = Inventory.OutfitSlot.Item.Armor.ToString("0.0") + " HP/s";
			}
			Hb.speed = 3f - (float)MyPlayer.Instance.Health / 100f * 2f;
			if (MyPlayer.Instance.Health < 70 && MyPlayer.Instance.Health > 30)
			{
				HeartBeat.color = Colors.Yellow;
			}
			else if (MyPlayer.Instance.Health <= 30)
			{
				HeartBeat.color = Colors.Red;
			}
			else
			{
				HeartBeat.color = Colors.White;
			}
		}
	}
}
