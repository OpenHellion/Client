using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public abstract class AbstractSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler,
		IDragHandler, IEndDragHandler, IPointerClickHandler, IEventSystemHandler
	{
		public InventoryUI InventoryUI;

		public Image Icon;

		public GameObject Hovered;

		public GameObject Selected;

		public GameObject Disabled;

		public GameObject Available;

		public bool Draggable = true;

		public bool IsLootSlot;

		public Text QuantityCount;

		public Image Health;

		public Transform ItemSlotHolder;

		public SmallSlotUI SlotUI;

		protected float lootTimer;

		protected float hoverTimer;

		protected float lootTreshold = 0.8f;

		protected float hoverTreshold = 0.4f;

		public abstract bool IsDisabled { get; }

		public abstract Item Item { get; }

		private void Update()
		{
			if (!Disabled.activeInHierarchy && InventoryUI.IsDragging && IsDisabled)
			{
				if (CanFitToChild(InventoryUI.DraggingItem))
				{
					Disabled.GetComponent<Image>().color = Colors.ChildSlotAvailable;
				}
				else
				{
					Disabled.GetComponent<Image>().color = Colors.DisabledSlot;
				}

				Disabled.SetActive(true);
			}
			else if (Disabled.activeInHierarchy && !IsDisabled)
			{
				Disabled.SetActive(false);
			}

			if (InventoryUI.IsDragging && !IsDisabled)
			{
				Available.Activate(true);
			}
			else if (Available.activeInHierarchy && !InventoryUI.IsDragging)
			{
				Available.Activate(false);
			}

			if (Selected.activeInHierarchy)
			{
				lootTimer += Time.deltaTime;
			}
			else if (lootTimer != 0f && !Selected.activeInHierarchy)
			{
				lootTimer = 0f;
			}

			if (Hovered.activeInHierarchy && InventoryUI.HoveredSlot == this)
			{
				hoverTimer += Time.deltaTime;
				if (hoverTimer > hoverTreshold && !InventoryUI.ToolTip.gameObject.activeInHierarchy)
				{
					InventoryUI.GetTooltip();
				}
			}
			else if (hoverTimer != 0f && InventoryUI.HoveredSlot != this)
			{
				hoverTimer = 0f;
			}
		}

		public abstract void UpdateSlot();

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (InventoryUI.DraggingItem != null && CanFitToChild(InventoryUI.DraggingItem))
			{
				ToggleItemSlots(true);
			}

			if (!(Item == null) && !IsDisabled)
			{
				Hovered.SetActive(true);
				InventoryUI.CanShowTooltip = true;
				InventoryUI.HoveredSlot = this;
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			ToggleItemSlots(false);
			Hovered.SetActive(false);
			InventoryUI.HideTooltip();
		}

		public abstract void OnDrop(PointerEventData eventData);

		public void OnDrag(PointerEventData eventData)
		{
			if (Item != null && Draggable && !IsDisabled)
			{
				InventoryUI.DraggingItem = Item;
				InventoryUI.DraggingObject.GetComponent<Image>().sprite = Icon.sprite;
				InventoryUI.IsDragging = true;
				RectTransform component = InventoryUI.DraggingObject.GetComponent<RectTransform>();
				Vector2 localPoint = Vector2.zero;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(InventoryUI.GetComponent<RectTransform>(),
					Mouse.current.position.ReadValue(), Camera.main, out localPoint);
				component.transform.localPosition = localPoint;
				InventoryUI.DraggingObject.SetActive(true);
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (!(InventoryUI.DraggingItem == null))
			{
				if (InventoryUI.DraggingItem == Item && InventoryUI.SelectedItem == Item)
				{
					InventoryUI.DeselectItem();
				}

				InventoryUI.IsDragging = false;
				InventoryUI.DraggingItem = null;
				InventoryUI.DraggingObject.SetActive(false);
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (IsDisabled || Item == null)
			{
				return;
			}

			if (InventoryUI.SelectedItem == Item)
			{
				if (lootTimer < lootTreshold)
				{
					if (IsLootSlot || Item.AttachPoint != null)
					{
						Item.RequestPickUp();
					}
					else if (Item.Slot == InventoryUI.Inventory.HandsSlot)
					{
						InventorySlot inventorySlot = InventoryUI.Inventory.GetAllSlots().Values
							.FirstOrDefault(_003COnPointerClick_003Em__0);
						if (inventorySlot != null)
						{
							Item.RequestAttach(inventorySlot);
						}
					}
					else
					{
						if (Item is Outfit && InventoryUI.Inventory.HandsSlot.Item != null)
						{
							Selected.SetActive(false);
							InventoryUI.DeselectItem();
							return;
						}

						Item.RequestAttach(InventoryUI.Inventory.HandsSlot);
					}
				}

				Selected.SetActive(false);
				InventoryUI.DeselectItem();
			}
			else
			{
				Selected.SetActive(true);
				InventoryUI.SelectItem(Item);
			}
		}

		public void RefreshQuantity()
		{
			if (Item != null)
			{
				string text = Item.QuantityCheck();
				if (text != null)
				{
					QuantityCount.gameObject.SetActive(true);
					QuantityCount.text = text;
				}
			}
			else if (QuantityCount.gameObject.activeInHierarchy)
			{
				QuantityCount.text = string.Empty;
				QuantityCount.gameObject.SetActive(false);
			}
		}

		public void UpdateHealth()
		{
			Health.gameObject.Activate(Item != null && Item.Expendable);
			if (Item != null && Item.Expendable)
			{
				Health.fillAmount = Item.Health / Item.MaxHealth;
				if (Item.Health >= Item.MaxHealth * 0.7f)
				{
					Health.color = Colors.HealthHigh;
				}
				else if (Item.Health < Item.MaxHealth * 0.7f && Item.Health >= Item.MaxHealth * 0.3f)
				{
					Health.color = Colors.HealthMid;
				}
				else
				{
					Health.color = Colors.HealthLow;
				}
			}
		}

		public void CheckItemSlots()
		{
			SmallSlotUI[] componentsInChildren = ItemSlotHolder.GetComponentsInChildren<SmallSlotUI>();
			foreach (SmallSlotUI smallSlotUI in componentsInChildren)
			{
				Destroy(smallSlotUI.gameObject);
			}

			if (Item == null)
			{
				return;
			}

			if (!Item.IsSlotContainer && Item.Slots.Count > 0)
			{
				foreach (ItemSlot value in Item.Slots.Values)
				{
					CreateSmallSlot(value, ItemSlotHolder);
				}
			}

			if (Item.Slot == InventoryUI.Inventory.HandsSlot)
			{
				InventoryCharacterPreview.Instance.RefreshPreviewCharacter(InventoryUI.Inventory);
			}
		}

		public void CreateSmallSlot(ItemSlot slot, Transform holder)
		{
			GameObject gameObject = Instantiate(SlotUI.gameObject, holder);
			gameObject.transform.Reset();
			gameObject.Activate(true);
			SmallSlotUI componentInChildren = gameObject.GetComponentInChildren<SmallSlotUI>();
			componentInChildren.Inventory = InventoryUI;
			componentInChildren.Slot = slot;
			componentInChildren.UpdateUI();
		}

		public void ToggleItemSlots(bool expand)
		{
			ItemSlotHolder.GetComponent<Animator>().SetBool("expand", expand);
		}

		public bool CanFitToChild(Item item)
		{
			if (Item == null)
			{
				return false;
			}

			if (!Item.IsSlotContainer && Item.Slots.Count > 0)
			{
				foreach (ItemSlot value in Item.Slots.Values)
				{
					if (value.CanFitItem(item))
					{
						return true;
					}
				}
			}

			return false;
		}

		[CompilerGenerated]
		private bool _003COnPointerClick_003Em__0(InventorySlot m)
		{
			return m.CanFitItem(Item) && m.SlotGroup != InventorySlot.Group.Primary &&
			       m.SlotGroup != InventorySlot.Group.Secondary && m.SlotGroup != InventorySlot.Group.Helmet &&
			       m.SlotGroup != InventorySlot.Group.Jetpack && m.Item == null;
		}
	}
}
