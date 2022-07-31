using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity
{
	public class InventoryUI : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003CLootAll_003Ec__AnonStorey1
		{
			internal List<InventorySlot> skipSlots;
		}

		[CompilerGenerated]
		private sealed class _003CLootAll_003Ec__AnonStorey0
		{
			internal AbstractSlotUI slot;

			internal _003CLootAll_003Ec__AnonStorey1 _003C_003Ef__ref_00241;

			internal bool _003C_003Em__0(InventorySlot m)
			{
				return m.CanFitItem(slot.Item) && m.Item == null && !_003C_003Ef__ref_00241.skipSlots.Contains(m);
			}
		}

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

		[Header("Slots")]
		public Transform GroupsHolder;

		public InventoryGroupUI GroupPrefab;

		public InventorySlotUI SlotPrefab;

		public ItemSlotUI ItemSlotPrefab;

		public AttachPointSlotUI AttachPointPrefab;

		[Header("PlayerInfo")]
		public Text PlayerName;

		public Image CharacterPreview;

		public Text ArmorValue;

		public Image HeartBeat;

		public Animator Hb;

		public Text HealthValue;

		[Header("CharacterSlots")]
		public Transform HandSlot;

		public Transform PrimarySlot;

		public Transform SecondarySlot;

		public Transform SuitSlot;

		public Transform HelmetSlot;

		public Transform JetpackSlot;

		[Header("SelectedItem")]
		public Item SelectedItem;

		public GameObject SelectedItemHolder;

		public TooltipInventory SelectedItemData;

		public GameObject RemoveOutfitButton;

		public GameObject DropButton;

		public GameObject ContainerSlotsButton;

		public Transform ItemSlotsHolder;

		[Header("Tooltip")]
		public AbstractSlotUI HoveredSlot;

		public TooltipInventory ToolTip;

		public bool CanShowTooltip;

		[Header("Dragging")]
		public bool IsDragging;

		public Item DraggingItem;

		public GameObject DraggingObject;

		[Header("Other")]
		public GameObject OtherHolder;

		public Transform OtherGroupsHolder;

		public object LootingTarget;

		public HashSet<BaseSceneAttachPoint> APLootingList = new HashSet<BaseSceneAttachPoint>();

		private float lootRadius = 1.65f;

		[Header("ItemSlots")]
		public bool ShowItemSlots;

		[CompilerGenerated]
		private static Func<InventorySlot, InventorySlot.Group> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<BaseSceneAttachPoint, Localization.StandardInteractionTip> _003C_003Ef__am_0024cache1;

		[CompilerGenerated]
		private static Func<AbstractSlotUI, bool> _003C_003Ef__am_0024cache2;

		[CompilerGenerated]
		private static Func<InventorySlot, bool> _003C_003Ef__am_0024cache3;

		public Inventory Inventory
		{
			get
			{
				return MyPlayer.Instance.Inventory;
			}
		}

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
				OtherHolder.SetActive(false);
			}
			else if (!OtherHolder.activeInHierarchy && (LootSlots.Count > 0 || (ApSlots.Count > 0 && ContainerItemSlots.Count > 0)))
			{
				OtherHolder.SetActive(true);
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
			OtherHolder.SetActive(false);
		}

		public void UpdateUI()
		{
			NoSuit.SetActive(Inventory.OutfitSlot.Item == null);
			ItemsHolder.SetActive(Inventory.OutfitSlot.Item != null);
			DeselectItem();
			HideTooltip();
			DraggingItem = null;
			DraggingObject.SetActive(false);
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
			SelectedItemData.FillData(itm, true);
			SelectedItemHolder.Activate(true);
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
			SelectedItemHolder.SetActive(false);
			RefreshSlots();
		}

		public void GetTooltip()
		{
			if (!IsDragging && CanShowTooltip && !(HoveredSlot == null) && !(HoveredSlot.Item == null))
			{
				ToolTip.gameObject.SetActive(false);
				ToolTip.SetTooltip(HoveredSlot.Item);
				float x = ((HoveredSlot.GetComponent<RectTransform>().position.x > 0f) ? 1 : 0);
				float y = ((HoveredSlot.GetComponent<RectTransform>().position.y > 0f) ? 1 : 0);
				ToolTip.GetComponent<RectTransform>().pivot = new Vector2(x, y);
				ToolTip.GetComponent<RectTransform>().position = HoveredSlot.GetComponent<RectTransform>().position;
				ToolTip.gameObject.SetActive(true);
			}
		}

		public void HideTooltip()
		{
			CanShowTooltip = false;
			HoveredSlot = null;
			ToolTip.gameObject.SetActive(false);
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
						allSlot.Selected.SetActive(true);
					}
					else
					{
						allSlot.Selected.SetActive(false);
					}
				}
			}
		}

		public void InitializeSlots()
		{
			base.transform.DestroyAll<InventorySlotUI>();
			base.transform.DestroyAll<InventoryGroupUI>();
			AllSlots.Clear();
			Dictionary<short, InventorySlot>.ValueCollection values = Inventory.GetAllSlots().Values;
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CInitializeSlots_003Em__0;
			}
			foreach (InventorySlot.Group item in values.Select(_003C_003Ef__am_0024cache0).Distinct())
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
				InventoryGroupUI inventoryGroupUI = UnityEngine.Object.Instantiate(GroupPrefab, GroupsHolder);
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
			GameObject gameObject = UnityEngine.Object.Instantiate(SlotPrefab.gameObject, holder);
			gameObject.transform.Reset();
			InventorySlotUI componentInChildren = gameObject.GetComponentInChildren<InventorySlotUI>();
			componentInChildren.Draggable = slot.SlotGroup != InventorySlot.Group.Outfit || !(slot.Parent is MyPlayer);
			componentInChildren.InventoryUI = this;
			componentInChildren.Slot = slot;
			componentInChildren.UpdateSlot();
			slot.UI = componentInChildren;
			slot.UI.IsLootSlot = lootSlot;
			Color value;
			if (Colors.ItemSlot.TryGetValue(slot.SlotGroup, out value))
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
			GameObject gameObject = UnityEngine.Object.Instantiate(ItemSlotPrefab.gameObject, holder);
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
			GameObject gameObject = UnityEngine.Object.Instantiate(AttachPointPrefab.gameObject, holder);
			gameObject.transform.Reset();
			AttachPointSlotUI componentInChildren = gameObject.GetComponentInChildren<AttachPointSlotUI>();
			componentInChildren.InventoryUI = this;
			componentInChildren.AttachPoint = ap;
			componentInChildren.UpdateSlot();
			ap.UI = componentInChildren;
			ap.UI.IsLootSlot = lootSlot;
			Color value;
			if (Colors.AttachPointSlotColor.TryGetValue(ap.GetType(), out value))
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
				UnityEngine.Object.Destroy(LootGroup.gameObject);
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
			InventoryGroupUI inventoryGroupUI = UnityEngine.Object.Instantiate(GroupPrefab, OtherGroupsHolder.transform);
			inventoryGroupUI.transform.Reset();
			inventoryGroupUI.transform.SetAsFirstSibling();
			inventoryGroupUI.Name.text = Localization.Loot.ToUpper();
			inventoryGroupUI.Inventory = this;
			inventoryGroupUI.LootAllButton.SetActive(true);
			LootGroup = inventoryGroupUI;
			if (LootingTarget is ISlotContainer)
			{
				foreach (InventorySlot value in (LootingTarget as ISlotContainer).GetAllSlots().Values)
				{
					if (!(value.Item == null))
					{
						CreateInventorySlot(value, LootGroup.SlotHolder, true);
					}
				}
			}
			else if (LootingTarget is Item)
			{
				foreach (ItemSlot value2 in (LootingTarget as Item).Slots.Values)
				{
					CreateItemSlot(value2, LootGroup.SlotHolder, true);
				}
			}
			OtherHolder.SetActive(true);
		}

		public void CheckItemSlots()
		{
			if (SlotGroup != null)
			{
				SlotGroup.SlotHolder.DestroyAll<AbstractSlotUI>();
				UnityEngine.Object.Destroy(SlotGroup.gameObject);
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
			InventoryGroupUI inventoryGroupUI = UnityEngine.Object.Instantiate(GroupPrefab, OtherGroupsHolder.transform);
			inventoryGroupUI.transform.Reset();
			inventoryGroupUI.transform.SetAsFirstSibling();
			inventoryGroupUI.Name.text = SelectedItem.Name;
			inventoryGroupUI.Inventory = this;
			inventoryGroupUI.LootAllButton.SetActive(true);
			SlotGroup = inventoryGroupUI;
			foreach (ItemSlot value in SelectedItem.Slots.Values)
			{
				CreateItemSlot(value, SlotGroup.SlotHolder, false, true);
			}
			OtherHolder.SetActive(true);
		}

		public void CheckProximityAttachPoints()
		{
			if (ProximityGroup != null)
			{
				ProximityGroup.SlotHolder.DestroyAll<AbstractSlotUI>();
				UnityEngine.Object.Destroy(ProximityGroup.gameObject);
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
			InventoryGroupUI inventoryGroupUI = UnityEngine.Object.Instantiate(GroupPrefab, OtherGroupsHolder.transform);
			inventoryGroupUI.transform.Reset();
			inventoryGroupUI.transform.SetAsFirstSibling();
			inventoryGroupUI.Name.text = Localization.ProximityLoot.ToUpper();
			inventoryGroupUI.LootAllButton.SetActive(true);
			inventoryGroupUI.Inventory = this;
			ProximityGroup = inventoryGroupUI;
			HashSet<BaseSceneAttachPoint> aPLootingList = APLootingList;
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CCheckProximityAttachPoints_003Em__1;
			}
			foreach (BaseSceneAttachPoint item in aPLootingList.OrderBy(_003C_003Ef__am_0024cache1))
			{
				if (item.Item == null || (Inventory.Parent is MyPlayer && item.Item.CanPlayerPickUp(Inventory.Parent as MyPlayer)))
				{
					CreateAPSlot(item, ProximityGroup.SlotHolder);
				}
			}
			OtherHolder.SetActive(true);
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
				Transform[] componentsInChildren = componentInParent.GetComponentsInChildren<Transform>(true);
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
			_003CLootAll_003Ec__AnonStorey1 _003CLootAll_003Ec__AnonStorey = new _003CLootAll_003Ec__AnonStorey1();
			_003CLootAll_003Ec__AnonStorey.skipSlots = new List<InventorySlot>();
			AbstractSlotUI[] componentsInChildren = holder.GetComponentsInChildren<AbstractSlotUI>();
			if (_003C_003Ef__am_0024cache2 == null)
			{
				_003C_003Ef__am_0024cache2 = _003CLootAll_003Em__2;
			}
			using (IEnumerator<AbstractSlotUI> enumerator = componentsInChildren.Where(_003C_003Ef__am_0024cache2).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					_003CLootAll_003Ec__AnonStorey0 _003CLootAll_003Ec__AnonStorey2 = new _003CLootAll_003Ec__AnonStorey0();
					_003CLootAll_003Ec__AnonStorey2._003C_003Ef__ref_00241 = _003CLootAll_003Ec__AnonStorey;
					_003CLootAll_003Ec__AnonStorey2.slot = enumerator.Current;
					Dictionary<short, InventorySlot>.ValueCollection values = Inventory.GetAllSlots().Values;
					if (_003C_003Ef__am_0024cache3 == null)
					{
						_003C_003Ef__am_0024cache3 = _003CLootAll_003Em__3;
					}
					InventorySlot inventorySlot = values.OrderBy(_003C_003Ef__am_0024cache3).FirstOrDefault(_003CLootAll_003Ec__AnonStorey2._003C_003Em__0);
					if (inventorySlot != null)
					{
						_003CLootAll_003Ec__AnonStorey2.slot.Item.RequestAttach(inventorySlot);
						_003CLootAll_003Ec__AnonStorey.skipSlots.Add(inventorySlot);
					}
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

		[CompilerGenerated]
		private static InventorySlot.Group _003CInitializeSlots_003Em__0(InventorySlot m)
		{
			return m.SlotGroup;
		}

		[CompilerGenerated]
		private static Localization.StandardInteractionTip _003CCheckProximityAttachPoints_003Em__1(BaseSceneAttachPoint m)
		{
			return m.StandardTip;
		}

		[CompilerGenerated]
		private static bool _003CLootAll_003Em__2(AbstractSlotUI m)
		{
			return m.Item != null;
		}

		[CompilerGenerated]
		private static bool _003CLootAll_003Em__3(InventorySlot m)
		{
			return m.SlotType == InventorySlot.Type.Hands;
		}
	}
}
