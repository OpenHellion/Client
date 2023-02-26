using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;
using OpenHellion.Networking;

namespace ZeroGravity.UI
{
	public class CargoPanel : AbstractPanelUI
	{
		[Space(20f)]
		public SubSystemRefinery Refinery;

		public SceneResourcesTransferPoint AttachPoint;

		public SubSystemFabricator Fabricator;

		public GameObject MainScreen;

		[Space(20f)]
		public Transform vesselListTransform;

		public GameObject VesselObject;

		public VesselObjectScript SelectedVessel;

		private static Dictionary<SpaceObjectVessel, VesselObjectScript> CargoVessels = new Dictionary<SpaceObjectVessel, VesselObjectScript>();

		public Text SelectedVesselName;

		public Text SelctedVesselCapacity;

		public Image SelectedVesselFiller;

		public Image SelectedVesselIcon;

		public GameObject AuthorizationFail;

		[Space(20f)]
		public Toggle RawToggle;

		public Toggle RefinedToggle;

		public Toggle CraftingToggle;

		[Space(20f)]
		public SceneCargoBay CurrentlySelectedCargoBay;

		private List<CargoResourceData> MainCargoItems = new List<CargoResourceData>();

		public GameObject CargoResourceHolder;

		public GameObject MainCargoResourcePref;

		public Scrollbar MainCargoScrollbar;

		[Space(20f)]
		public Dropdown ConnectedVesselsDropdown;

		public Dictionary<ResourceContainer, CargoSystemObject> ConnectedContainerResourcesUI = new Dictionary<ResourceContainer, CargoSystemObject>();

		public GameObject SystemHolder;

		public GameObject CargoVesselObject;

		public GameObject CargoSystemObject;

		public GameObject CargoSystemResource;

		[Space(20f)]
		public GameObject DragingItem;

		public bool isDragigng;

		public GameObject TransferingBox;

		public Text From;

		public Text To;

		public Text FromName;

		public Image FromIcon;

		public Slider TransferSlider;

		public InputField TransferInput;

		public Text CompartmentValue;

		public Button ConfirmTransferButton;

		[Space(20f)]
		public GameObject RefineryAction;

		public GameObject FabricatorAction;

		public GameObject ActiveSystemsHolder;

		public GameObject SystemButtonActive;

		public GameObject AttachItemHolder;

		public GameObject AttachButtonActive;

		public GameObject RefineryHolder;

		public GameObject RefineryButtonActive;

		public GameObject CraftingHolder;

		public GameObject CraftingButtonActive;

		public GameObject CancelCrafingBox;

		[Space(20f)]
		public Transform AttachedItemContent;

		private ICargo itemCargos;

		public GameObject HaveAttachItem;

		public GameObject NoItemAttached;

		public GameObject AttachPointActive;

		public GameObject UnloadAttachedButton;

		public GameObject RefillAttachedButton;

		public Text AttachItemName;

		public GameObject AttachedItemCompartment;

		public GameObject AttachedCargoResourceItem;

		public List<CargoAttachPointContainer> AttachedItemContainersUI = new List<CargoAttachPointContainer>();

		[Space(20f)]
		public GameObject NoRefineryAvailable;

		public GameObject RefineryActive;

		public Text RefineryCapacity;

		private Dictionary<ResourceType, CargoResourceUI> RefineryItems = new Dictionary<ResourceType, CargoResourceUI>();

		public Transform RefineryItemContent;

		public GameObject UnloadRefineryButton;

		public Button RefineButton;

		public Text RefineButtonStatus;

		public Text RefineryPowerConsumption;

		public Text RefiningTimer;

		public Transform PartsTransform;

		[Space(20f)]
		public GameObject NoCraftingStation;

		public GameObject CraftingActive;

		public Text CraftingCapacity;

		private Dictionary<ResourceType, CargoResourceUI> CraftingItems = new Dictionary<ResourceType, CargoResourceUI>();

		public Transform CraftingItemsTransform;

		public Button CraftButton;

		public Text CraftingStatus;

		public Text CraftingPowerConsumption;

		public Text CraftingTime;

		public Transform CraftingPartsTransform;

		public GameObject UnloadFabricatorButton;

		public GameObject CraftingInProgress;

		public Text InProgressName;

		public Image InProgressIcon;

		public Text TimerForCrafting;

		public CraftingItem CurrentCraftingItem;

		private ItemCompoundType CraftingItemInProgress;

		public Text CurrentItemName;

		public Image CurrentItemIcon;

		public Transform CurrentItemResources;

		public CargoResourceForCraftingUI CurrentItemResourcesUI;

		[Space(20f)]
		public GameObject PartPref;

		public List<PartSlotUI> PartsUI = new List<PartSlotUI>();

		public GameObject ResourceItem;

		[Space(20f)]
		public Toggle ShowAvailable;

		public GameObject SelectItem;

		public GameObject CraftingScreen;

		public Transform CraftingItemsContent;

		public GameObject CraftingItemUI;

		private void Start()
		{
		}

		private void Update()
		{
			GetPowerStatus();
		}

		public override void OnInteract()
		{
			base.OnInteract();
			RefreshCargoPanel();
			Refinery = ParentVessel.GeometryRoot.GetComponentInChildren<SubSystemRefinery>();
			Fabricator = ParentVessel.GeometryRoot.GetComponentInChildren<SubSystemFabricator>();
			SetItemForCrafting();
			if (AttachPoint != null)
			{
				AttachPoint.Initialize(this);
			}
			ConnectedVesselsDropdown.onValueChanged.AddListener(delegate
			{
				SlectCurrentVesselSystems();
			});
			RawToggle.onValueChanged.AddListener(delegate
			{
				RefreshMainCargoResources();
			});
			RefinedToggle.onValueChanged.AddListener(delegate
			{
				RefreshMainCargoResources();
			});
			CraftingToggle.onValueChanged.AddListener(delegate
			{
				RefreshMainCargoResources();
			});
			MainScreen.Activate(value: true);
			CraftingScreen.Activate(value: false);
			ToggleCancelCrafting(cancel: false);
			base.gameObject.Activate(value: true);
			RefineryAction.Activate(Refinery != null);
			FabricatorAction.Activate(Fabricator != null);
			HaveAttachItem.Activate(AttachPoint.Item != null);
		}

		public override void OnDetach()
		{
			base.OnDetach();
			base.gameObject.SetActive(value: false);
			RawToggle.onValueChanged.RemoveAllListeners();
			RefinedToggle.onValueChanged.RemoveAllListeners();
			CraftingToggle.onValueChanged.RemoveAllListeners();
			ConnectedVesselsDropdown.onValueChanged.RemoveAllListeners();
			CancelTransfer();
		}

		public void RefreshCargoPanel()
		{
			CargoVessels.Clear();
			Initialize();
			GetConnectedVessels();
			if (CargoVessels.ContainsKey(ParentVessel))
			{
				SelectVessel(ParentVessel);
			}
			else if (CargoVessels.Count > 0)
			{
				SelectVessel(CargoVessels.Keys.First());
			}
			ActivateOther(0);
			GetPowerStatus();
		}

		private void GetConnectedVessels()
		{
			vesselListTransform.DestroyAll<VesselObjectScript>();
			foreach (SpaceObjectVessel allVessel in AllVessels)
			{
				if (!(allVessel.CargoBay == null))
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(VesselObject, vesselListTransform);
					gameObject.SetActive(value: true);
					gameObject.transform.Reset();
					VesselObjectScript component = gameObject.GetComponent<VesselObjectScript>();
					component.MyCargoPanel = this;
					component.Vessel = allVessel;
					component.Cargo = allVessel.CargoBay;
					component.CapacityFiller.fillAmount = component.CargoCapacityFillerValue();
					CargoVessels[component.Vessel] = component;
				}
			}
		}

		public void SelectVessel(SpaceObjectVessel vessel)
		{
			foreach (VesselObjectScript value in CargoVessels.Values)
			{
				value.CapacityFiller.fillAmount = value.CargoCapacityFillerValue();
				if (vessel == value.Vessel)
				{
					SelectedVessel = value;
					value.Selected.SetActive(value: true);
				}
				else
				{
					value.Selected.SetActive(value: false);
				}
			}
			if (SelectedVessel != null)
			{
				UpdateSelectedVesselUI();
				CurrentlySelectedCargoBay = SelectedVessel.Cargo;
				RefreshMainCargoResources();
			}
			else
			{
				AuthorizationFail.SetActive(value: false);
			}
		}

		public void UpdateVesselObjects(SpaceObjectVessel vessel)
		{
			if (CargoVessels.TryGetValue(vessel, out var value) && !(value == null))
			{
				value.CapacityFiller.fillAmount = value.CargoCapacityFillerValue();
				if (SelectedVessel == value)
				{
					UpdateSelectedVesselUI();
				}
			}
		}

		private void UpdateSelectedVesselUI()
		{
			SelectedVesselName.text = SelectedVessel.Vessel.CustomName;
			SelectedVesselIcon.sprite = Client.Instance.SpriteManager.GetSprite(SelectedVessel.Vessel);
			SelectedVesselFiller.fillAmount = SelectedVessel.CargoCapacityFillerValue();
			AuthorizationFail.SetActive(!SelectedVessel.IsAuthorized);
			SelctedVesselCapacity.text = SelectedVessel.CargoCapacityText();
		}

		public void RefreshMainCargoResources()
		{
			CargoResourceHolder.DestroyAll<CargoResourceUI>();
			MainCargoItems.Clear();
			if (CurrentlySelectedCargoBay.CargoBayResources.Resources != null && CurrentlySelectedCargoBay.CargoBayResources.Resources.Count > 0)
			{
				foreach (CargoResourceData resource in CurrentlySelectedCargoBay.CargoBayResources.Resources)
				{
					if (resource.ResourceType != ResourceType.Reserved && !(resource.Quantity <= 0f) && ((CargoCompartment.IsRawResource(resource.ResourceType) && RawToggle.isOn) || (CargoCompartment.IsRefinedResource(resource.ResourceType) && RefinedToggle.isOn) || (CargoCompartment.IsCraftingResource(resource.ResourceType) && CraftingToggle.isOn) || CargoCompartment.IsOther(resource.ResourceType)))
					{
						MainCargoItems.Add(resource);
						GameObject gameObject = UnityEngine.Object.Instantiate(MainCargoResourcePref, CargoResourceHolder.transform);
						gameObject.transform.localScale = Vector3.one;
						gameObject.SetActive(value: true);
						CargoResourceUI component = gameObject.GetComponent<CargoResourceUI>();
						component.Compartment = CurrentlySelectedCargoBay.CargoBayResources;
						component.Resource = resource;
						component.Quantity = resource.Quantity;
						component.SetName();
						component.Value.text = FormatHelper.FormatValue(resource.Quantity);
						component.Icon.sprite = Client.Instance.SpriteManager.GetSprite(resource.ResourceType);
						component.gameObject.transform.localScale = Vector3.one;
					}
				}
			}
			MainCargoScrollbar.value = 1f;
			UpdateVesselObjects(CurrentlySelectedCargoBay.ParentVessel);
			if (ParentVessel.GeometryRoot.GetComponentInChildren<SceneItemRecycler>() != null)
			{
				ParentVessel.GeometryRoot.GetComponentInChildren<SceneItemRecycler>().RecyclerUI.UpdateUI();
			}
		}

		private void GetAllSystems()
		{
			ConnectedVesselsDropdown.ClearOptions();
			ConnectedVesselsDropdown.options.Add(new Dropdown.OptionData(Localization.AllVessels.ToUpper()));
			foreach (SpaceObjectVessel allVessel in AllVessels)
			{
				ConnectedVesselsDropdown.options.Add(new Dropdown.OptionData(allVessel.CustomName));
			}
			ConnectedVesselsDropdown.value = 0;
			ConnectedVesselsDropdown.RefreshShownValue();
			SlectCurrentVesselSystems();
		}

		public void SlectCurrentVesselSystems()
		{
			SystemHolder.DestroyAll<ConnectedVessel>();
			if (ConnectedVesselsDropdown.value == 0)
			{
				MakeSystems();
			}
			else
			{
				MakeActiveSystem(AllVessels[ConnectedVesselsDropdown.value - 1]);
			}
		}

		public void MakeSystems()
		{
			foreach (SpaceObjectVessel allVessel in AllVessels)
			{
				MakeActiveSystem(allVessel);
			}
		}

		private void MakeActiveSystem(SpaceObjectVessel vessel)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(CargoVesselObject, SystemHolder.transform);
			gameObject.transform.localScale = Vector3.one;
			gameObject.SetActive(value: true);
			ConnectedVessel component = gameObject.GetComponent<ConnectedVessel>();
			component.Vessel = vessel;
			component.CargoPanel = this;
			if (!vessel.IsPlayerAuthorizedOrNoSecurity(MyPlayer.Instance))
			{
				return;
			}
			foreach (ResourceContainer value in vessel.ResourceContainers.Values)
			{
				MakeSystemItem(gameObject, value);
			}
		}

		private void MakeSystemItem(GameObject newVessel, ResourceContainer rc)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(CargoSystemObject, newVessel.transform);
			gameObject.transform.localScale = Vector3.one;
			gameObject.SetActive(value: true);
			CargoSystemObject component = gameObject.GetComponent<CargoSystemObject>();
			component.Capacity = rc.Capacity;
			component.Quantity = rc.Quantity;
			GameObject gameObject2 = UnityEngine.Object.Instantiate(CargoSystemResource, newVessel.transform);
			gameObject2.transform.localScale = Vector3.one;
			gameObject2.SetActive(value: true);
			CargoResourceData cargoResourceData = rc.CargoCompartment.Resources[0];
			CargoResourceUI component2 = gameObject2.GetComponent<CargoResourceUI>();
			component2.Compartment = rc.CargoCompartment;
			component2.Resource = cargoResourceData;
			component2.Quantity = cargoResourceData.Quantity;
			component2.Value.text = FormatHelper.CurrentMax(component2.Quantity, component.Capacity);
			component2.SetName();
			component2.Icon.sprite = Client.Instance.SpriteManager.GetSprite(cargoResourceData.ResourceType);
			component.Resource = component2;
			ConnectedContainerResourcesUI[rc] = component;
		}

		public void RefreshSystemObject(ResourceContainer rc)
		{
			if (ConnectedContainerResourcesUI.TryGetValue(rc, out var value) && value.gameObject.activeInHierarchy)
			{
				value.Capacity = rc.Capacity;
				value.Quantity = rc.Quantity;
				CargoResourceUI resource = value.Resource;
				CargoResourceData cargoResourceData = rc.CargoCompartment.Resources[0];
				resource.Compartment = rc.CargoCompartment;
				resource.Resource = cargoResourceData;
				resource.Quantity = cargoResourceData.Quantity;
				resource.Value.text = FormatHelper.CurrentMax(resource.Quantity, value.Capacity);
				resource.SetName();
				resource.Icon.sprite = Client.Instance.SpriteManager.GetSprite(cargoResourceData.ResourceType);
			}
		}

		public void AttachPointCheck()
		{
			if (AttachPoint.Item != null)
			{
				itemCargos = AttachPoint.Item as ICargo;
				HaveAttachItem.Activate(value: true);
				NoItemAttached.SetActive(value: false);
				AttachItemName.text = AttachPoint.Item.Name.ToUpper();
				AttachPointActive.SetActive(value: true);
				UnloadAttachedButton.SetActive(value: false);
				CreateAttachItemUI();
			}
			else
			{
				itemCargos = null;
				AttachItemName.text = Localization.AttachPoint.ToUpper();
				HaveAttachItem.Activate(value: false);
				NoItemAttached.SetActive(value: true);
				AttachPointActive.SetActive(value: false);
			}
		}

		public void CreateAttachItemUI()
		{
			if (itemCargos == null)
			{
				return;
			}
			AttachedItemContainersUI.Clear();
			CargoAttachPointContainer[] componentsInChildren = AttachedItemContent.GetComponentsInChildren<CargoAttachPointContainer>(includeInactive: true);
			foreach (CargoAttachPointContainer cargoAttachPointContainer in componentsInChildren)
			{
				UnityEngine.Object.DestroyImmediate(cargoAttachPointContainer.gameObject);
			}
			CargoResourceUI[] componentsInChildren2 = AttachedItemContent.GetComponentsInChildren<CargoResourceUI>(includeInactive: true);
			foreach (CargoResourceUI cargoResourceUI in componentsInChildren2)
			{
				UnityEngine.Object.DestroyImmediate(cargoResourceUI.gameObject);
			}
			foreach (CargoCompartment compartment in itemCargos.Compartments)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(AttachedItemCompartment, AttachedItemContent);
				gameObject.transform.localScale = Vector3.one;
				gameObject.SetActive(value: true);
				CargoAttachPointContainer component = gameObject.GetComponent<CargoAttachPointContainer>();
				component.ItemCompartment = compartment;
				component.Name.text = compartment.Name.ToUpper();
				component.Capacity = compartment.Capacity;
				component.Quantity = ((compartment.Resources == null) ? 0f : compartment.Resources.Sum((CargoResourceData m) => m.Quantity));
				component.Value.text = System.Math.Round(component.Quantity, 2).ToString("0.#") + " / " + component.Capacity.ToString("0.#");
				AttachedItemContainersUI.Add(component);
				UpdateAttachedResources(compartment);
			}
		}

		public void RefreshAttachedItemResources()
		{
			CargoResourceUI[] componentsInChildren = AttachedItemContent.GetComponentsInChildren<CargoResourceUI>(includeInactive: true);
			foreach (CargoResourceUI cargoResourceUI in componentsInChildren)
			{
				UnityEngine.Object.DestroyImmediate(cargoResourceUI.gameObject);
			}
			foreach (CargoAttachPointContainer item in AttachedItemContainersUI)
			{
				item.Capacity = item.ItemCompartment.Capacity;
				item.Quantity = ((item.ItemCompartment.Resources == null) ? 0f : item.ItemCompartment.Resources.Sum((CargoResourceData m) => m.Quantity));
				item.Value.text = System.Math.Round(item.Quantity, 2).ToString("0.#") + " / " + item.Capacity.ToString("0.#");
				UpdateAttachedResources(item.ItemCompartment);
			}
		}

		private void UpdateAttachedResources(CargoCompartment itemCompartment)
		{
			if ((AttachPoint.Item.Type == ItemType.AltairHandDrillCanister || AttachPoint.Item.Type == ItemType.AltairRefinedCanister || AttachPoint.Item.Type == ItemType.AltairResourceContainer) && itemCompartment.AvailableCapacity > 0f)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(AttachedCargoResourceItem, AttachedItemContent);
				gameObject.transform.localScale = Vector3.one;
				gameObject.SetActive(value: true);
				CargoResourceUI component = gameObject.GetComponent<CargoResourceUI>();
				component.Compartment = itemCompartment;
				component.Name.text = Localization.Slot.ToUpper();
				component.Value.text = string.Empty;
				component.Icon.sprite = Client.Instance.SpriteManager.GetSprite(ResourceType.None);
			}
			UnloadAttachedButton.SetActive(itemCompartment.Resources != null);
			if (itemCompartment.Resources != null)
			{
				foreach (CargoResourceData resource in itemCompartment.Resources)
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate(AttachedCargoResourceItem, AttachedItemContent);
					gameObject2.transform.localScale = Vector3.one;
					gameObject2.SetActive(value: true);
					CargoResourceUI component2 = gameObject2.GetComponent<CargoResourceUI>();
					component2.Compartment = itemCompartment;
					component2.Resource = resource;
					component2.SetName();
					component2.Quantity = resource.Quantity;
					component2.Value.text = System.Math.Round(component2.Quantity, 2).ToString("0.#");
					component2.Icon.sprite = Client.Instance.SpriteManager.GetSprite(resource.ResourceType);
				}
			}
			if (AttachPoint.Item is Jetpack)
			{
				Jetpack jetpack = AttachPoint.Item as Jetpack;
				RefillAttachedButton.Activate(jetpack.CurrentOxygen != jetpack.MaxAngularVelocity || jetpack.CurrentFuel != jetpack.MaxFuel);
			}
			else
			{
				RefillAttachedButton.Activate(AttachPoint.Item.Quantity != AttachPoint.Item.MaxQuantity);
			}
		}

		public void RefreshAttachItemUI()
		{
		}

		public void ActivateOther(int oth)
		{
			ActiveSystemsHolder.SetActive(oth == 0);
			SystemButtonActive.SetActive(oth == 0);
			AttachItemHolder.SetActive(oth == 1);
			AttachButtonActive.SetActive(oth == 1);
			RefineryHolder.SetActive(oth == 2);
			RefineryButtonActive.SetActive(oth == 2);
			CraftingHolder.SetActive(oth == 3);
			CraftingButtonActive.SetActive(oth == 3);
			switch (oth)
			{
			case 0:
				GetAllSystems();
				break;
			case 1:
				AttachPointCheck();
				break;
			case 2:
				ActivateRefinery();
				break;
			case 3:
				ActivateCrafting();
				break;
			}
		}

		public void ActivateCrafting()
		{
			CraftingActive.SetActive(Fabricator != null);
			NoCraftingStation.SetActive(Fabricator == null);
			CraftingItems.Clear();
			CraftingItemsTransform.DestroyAll<CargoResourceUI>();
			if (Fabricator == null)
			{
				return;
			}
			if (Fabricator.CargoResources.Resources != null)
			{
				foreach (CargoResourceData resource in Fabricator.CargoResources.Resources)
				{
					InstantiateResource(resource, CraftingItemsTransform);
				}
			}
			MakeParts(CraftingPartsTransform, Fabricator.MachineryPartSlots);
			CraftingStatusUpdate();
		}

		public void UpdateCraftingResources()
		{
			if (Fabricator.CargoResources.Resources != null)
			{
				foreach (CargoResourceData resource in Fabricator.CargoResources.Resources)
				{
					if (CraftingItems.TryGetValue(resource.ResourceType, out var value))
					{
						value.Quantity = resource.Quantity;
						value.Value.text = FormatHelper.FormatValue(resource.Quantity);
					}
					else
					{
						InstantiateResource(resource, CraftingItemsTransform);
					}
				}
				List<ResourceType> list = new List<ResourceType>();
				foreach (KeyValuePair<ResourceType, CargoResourceUI> kv in CraftingItems)
				{
					if (Fabricator.CargoResources.Resources.Count((CargoResourceData m) => m.ResourceType == kv.Value.Resource.ResourceType) == 0)
					{
						list.Add(kv.Key);
					}
				}
				foreach (ResourceType item in list)
				{
					UnityEngine.Object.Destroy(CraftingItems[item].gameObject);
					CraftingItems.Remove(item);
				}
			}
			else
			{
				CraftingItems.Clear();
				CraftingItemsTransform.DestroyAll<CargoResourceUI>();
			}
			CraftingStatusUpdate();
		}

		private void CraftingStatusUpdate()
		{
			if (Fabricator == null)
			{
				return;
			}
			if (Fabricator.ItemsInQueue != null && (Fabricator.Status == SystemStatus.Online || (Fabricator.Status == SystemStatus.Offline && Fabricator.SecondaryStatus == SystemSecondaryStatus.Malfunction)))
			{
				if (CraftingItemInProgress == null)
				{
					CraftingItemInProgress = Fabricator.ItemsInQueue[0];
				}
				CraftingInProgress.SetActive(value: true);
				InProgressName.text = CurrentItemName.text;
				InProgressName.color = CurrentItemName.color;
				InProgressIcon.sprite = CurrentItemIcon.sprite;
				TimerForCrafting.text = FormatHelper.Timer(Fabricator.CurrentTimeLeft);
			}
			else
			{
				SelectItem.SetActive(CurrentCraftingItem == null);
				CraftingInProgress.SetActive(value: false);
				if (CurrentCraftingItem != null)
				{
					CheckCraftingResources();
				}
				if (CurrentCraftingItem != null && Fabricator.AttachPoints.Count((SceneAttachPoint m) => m.Item == null) > 0 && CurrentCraftingItem.CanFabricate())
				{
					CraftButton.interactable = true;
					CraftingStatus.text = string.Empty;
				}
				else
				{
					CraftButton.interactable = false;
					if (Fabricator.AttachPoints.Count((SceneAttachPoint m) => m.Item == null) == 0)
					{
						CraftingStatus.text = Localization.RemoveItem;
					}
					else if (CurrentCraftingItem == null)
					{
						CraftingStatus.text = Localization.ChooseAnItem;
					}
					else if (!CurrentCraftingItem.CanFabricate())
					{
						CraftingStatus.text = Localization.NotEnoughResources;
					}
					else
					{
						CraftingStatus.text = Localization.NoPower;
					}
				}
			}
			UnloadFabricatorButton.SetActive(Fabricator.CargoResources.Resources != null);
			if (Fabricator.CargoResources.Resources != null)
			{
				CraftingCapacity.text = FormatHelper.CurrentMax(Fabricator.CargoResources.Resources.Sum((CargoResourceData m) => m.Quantity), Fabricator.CargoResources.Capacity);
			}
			else
			{
				CraftingCapacity.text = FormatHelper.CurrentMax(0f, Fabricator.CargoResources.Capacity);
			}
		}

		public void ActivateRefinery()
		{
			RefineryActive.SetActive(Refinery != null);
			NoRefineryAvailable.SetActive(Refinery == null);
			if (!(Refinery != null))
			{
				return;
			}
			RefineryItems.Clear();
			RefineryItemContent.DestroyAll<CargoResourceUI>();
			UnloadRefineryButton.SetActive(Refinery.CargoResources.Resources != null);
			if (Refinery.CargoResources.Resources != null)
			{
				foreach (CargoResourceData resource in Refinery.CargoResources.Resources)
				{
					InstantiateResource(resource, RefineryItemContent);
				}
			}
			RefineryPowerConsumption.text = FormatHelper.FormatValue(Refinery.GetResourceRequirement(DistributionSystemType.Power, true)) + " / s";
			MakeParts(PartsTransform, Refinery.MachineryPartSlots);
			RefineryStatusUpdate();
		}

		private void MakeParts(Transform loc, SceneMachineryPartSlot[] systemSlots)
		{
			PartsUI.Clear();
			PartSlotUI[] componentsInChildren = loc.GetComponentsInChildren<PartSlotUI>(includeInactive: true);
			foreach (PartSlotUI partSlotUI in componentsInChildren)
			{
				UnityEngine.Object.DestroyImmediate(partSlotUI.gameObject);
			}
			foreach (SceneMachineryPartSlot partSlot in systemSlots)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(PartPref, PartsTransform);
				gameObject.SetActive(value: true);
				gameObject.transform.Reset();
				PartSlotUI component = gameObject.GetComponent<PartSlotUI>();
				component.Panel = gameObject;
				PartsUI.Add(component);
				component.PartSlot = partSlot;
			}
			foreach (PartSlotUI item in PartsUI)
			{
				item.UpdateUI();
			}
		}

		public void ToggleRefinery()
		{
			Refinery.Toggle();
			RefineryStatusUpdate();
		}

		public void UpdateRefineryResources()
		{
			if (Refinery.CargoResources.Resources != null)
			{
				foreach (CargoResourceData resource in Refinery.CargoResources.Resources)
				{
					if (RefineryItems.TryGetValue(resource.ResourceType, out var value))
					{
						value.Quantity = resource.Quantity;
						value.Value.text = FormatHelper.FormatValue(resource.Quantity);
					}
					else
					{
						InstantiateResource(resource, RefineryItemContent);
					}
				}
				List<ResourceType> list = new List<ResourceType>();
				foreach (KeyValuePair<ResourceType, CargoResourceUI> kv in RefineryItems)
				{
					if (Refinery.CargoResources.Resources.Count((CargoResourceData m) => m.ResourceType == kv.Value.Resource.ResourceType) == 0)
					{
						list.Add(kv.Key);
					}
				}
				foreach (ResourceType item in list)
				{
					UnityEngine.Object.DestroyImmediate(RefineryItems[item].gameObject);
					RefineryItems.Remove(item);
				}
			}
			else
			{
				RefineryItems.Clear();
				CargoResourceUI[] componentsInChildren = RefineryItemContent.GetComponentsInChildren<CargoResourceUI>(includeInactive: true);
				foreach (CargoResourceUI cargoResourceUI in componentsInChildren)
				{
					UnityEngine.Object.DestroyImmediate(cargoResourceUI.gameObject);
				}
			}
			RefineryStatusUpdate();
		}

		private void RefineryStatusUpdate()
		{
			UnloadRefineryButton.SetActive(Refinery.CargoResources.Resources != null);
			if (Refinery.Status == SystemStatus.Online)
			{
				RefineButtonStatus.text = Localization.Cancel.ToUpper();
			}
			else
			{
				RefineButtonStatus.text = Localization.Refine.ToUpper();
			}
			if (Refinery.CargoResources.Resources != null)
			{
				float num = Refinery.CargoResources.Resources.Where((CargoResourceData m) => Refinery.Resources.Select((RefinedResourcesData n) => n.RawResource).Contains(m.ResourceType)).Sum((CargoResourceData m) => m.Quantity);
				float val = Refinery.ProcessingTime * (num / Refinery.Capacity);
				RefineButton.interactable = num != 0f && curCap >= Refinery.ProcessingTime;
				RefineryCapacity.text = FormatHelper.CurrentMax(Refinery.CargoResources.Resources.Sum((CargoResourceData m) => m.Quantity), Refinery.CargoResources.Capacity);
				if (num > 0f)
				{
					RefiningTimer.text = FormatHelper.Timer(val);
				}
				else
				{
					RefiningTimer.text = string.Empty;
				}
			}
			else
			{
				RefineryCapacity.text = FormatHelper.CurrentMax(0f, Refinery.CargoResources.Capacity);
				RefiningTimer.text = string.Empty;
				RefineButton.interactable = false;
			}
		}

		private void InstantiateResource(CargoResourceData resource, Transform loc)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(ResourceItem, loc);
			gameObject.transform.localScale = Vector3.one;
			gameObject.SetActive(value: true);
			CargoResourceUI component = gameObject.GetComponent<CargoResourceUI>();
			component.gameObject.transform.localScale = Vector3.one;
			if (loc == RefineryItemContent)
			{
				component.Compartment = Refinery.CargoResources;
				RefineryItems[resource.ResourceType] = component;
			}
			else
			{
				component.Compartment = Fabricator.CargoResources;
				CraftingItems[resource.ResourceType] = component;
			}
			component.Resource = resource;
			component.Quantity = resource.Quantity;
			component.SetName();
			component.Value.text = FormatHelper.FormatValue(resource.Quantity);
			component.Icon.sprite = Client.Instance.SpriteManager.GetSprite(resource.ResourceType);
		}

		public void UnloadAttached()
		{
			bool flag = false;
			ICargo cargo = AttachPoint.Item as ICargo;
			foreach (CargoCompartment compartment in cargo.Compartments)
			{
				foreach (CargoResourceData resource in compartment.Resources)
				{
					if (resource.Quantity > 0f)
					{
						TransferResourceMessage transferResourceMessage = null;
						TransferResourceMessage transferResourceMessage2 = new TransferResourceMessage();
						transferResourceMessage2.ToLocationType = ResourceLocationType.CargoBay;
						transferResourceMessage2.ToVesselGuid = CurrentlySelectedCargoBay.ParentVessel.GUID;
						transferResourceMessage2.ToInSceneID = CurrentlySelectedCargoBay.InSceneID;
						transferResourceMessage2.ToCompartmentID = CurrentlySelectedCargoBay.Compartments[0].ID;
						transferResourceMessage2.ResourceType = resource.ResourceType;
						transferResourceMessage2.Quantity = resource.Quantity;
						transferResourceMessage = transferResourceMessage2;
						transferResourceMessage.FromLocationType = ResourceLocationType.ResourcesTransferPoint;
						transferResourceMessage.FromVesselGuid = AttachPoint.ParentVessel.GUID;
						transferResourceMessage.FromInSceneID = AttachPoint.InSceneID;
						transferResourceMessage.FromCompartmentID = compartment.ID;
						if (transferResourceMessage != null)
						{
							NetworkController.Instance.SendToGameServer(transferResourceMessage);
						}
						flag = true;
					}
				}
			}
			if (flag)
			{
				RefreshMainCargoResources();
			}
		}

		public void FillAttached()
		{
			bool flag = false;
			ICargo cargo = AttachPoint.Item as ICargo;
			foreach (CargoCompartment cc in cargo.Compartments)
			{
				if (cc.Resources != null && cc.Resources.Count > 0)
				{
					foreach (CargoResourceData resource in cc.Resources)
					{
						if (CurrentlySelectedCargoBay.CargoBayResources.Resources == null || CurrentlySelectedCargoBay.CargoBayResources.Resources.Count <= 0)
						{
							continue;
						}
						foreach (CargoResourceData item in CurrentlySelectedCargoBay.CargoBayResources.Resources.Where((CargoResourceData m) => m.ResourceType == resource.ResourceType))
						{
							if (item.Quantity > 0f)
							{
								TransferResourceMessage transferResourceMessage = null;
								TransferResourceMessage transferResourceMessage2 = new TransferResourceMessage();
								transferResourceMessage2.ToLocationType = ResourceLocationType.ResourcesTransferPoint;
								transferResourceMessage2.ToVesselGuid = AttachPoint.ParentVessel.GUID;
								transferResourceMessage2.ToInSceneID = AttachPoint.InSceneID;
								transferResourceMessage2.ToCompartmentID = cc.ID;
								transferResourceMessage2.ResourceType = item.ResourceType;
								transferResourceMessage2.Quantity = item.Quantity;
								transferResourceMessage = transferResourceMessage2;
								transferResourceMessage.FromLocationType = ResourceLocationType.CargoBay;
								transferResourceMessage.FromVesselGuid = CurrentlySelectedCargoBay.ParentVessel.GUID;
								transferResourceMessage.FromInSceneID = CurrentlySelectedCargoBay.InSceneID;
								transferResourceMessage.FromCompartmentID = CurrentlySelectedCargoBay.Compartments[0].ID;
								if (transferResourceMessage != null)
								{
									NetworkController.Instance.SendToGameServer(transferResourceMessage);
								}
								flag = true;
							}
						}
					}
				}
				else
				{
					foreach (CargoResourceData item2 in CurrentlySelectedCargoBay.CargoBayResources.Resources.Where((CargoResourceData m) => cc.IsAllowed(m.ResourceType)))
					{
						if (item2.Quantity > 0f)
						{
							TransferResourceMessage transferResourceMessage3 = null;
							TransferResourceMessage transferResourceMessage2 = new TransferResourceMessage();
							transferResourceMessage2.ToLocationType = ResourceLocationType.ResourcesTransferPoint;
							transferResourceMessage2.ToVesselGuid = AttachPoint.ParentVessel.GUID;
							transferResourceMessage2.ToInSceneID = AttachPoint.InSceneID;
							transferResourceMessage2.ToCompartmentID = cc.ID;
							transferResourceMessage2.ResourceType = item2.ResourceType;
							transferResourceMessage2.Quantity = item2.Quantity;
							transferResourceMessage3 = transferResourceMessage2;
							transferResourceMessage3.FromLocationType = ResourceLocationType.CargoBay;
							transferResourceMessage3.FromVesselGuid = CurrentlySelectedCargoBay.ParentVessel.GUID;
							transferResourceMessage3.FromInSceneID = CurrentlySelectedCargoBay.InSceneID;
							transferResourceMessage3.FromCompartmentID = CurrentlySelectedCargoBay.Compartments[0].ID;
							if (transferResourceMessage3 != null)
							{
								NetworkController.Instance.SendToGameServer(transferResourceMessage3);
							}
							flag = true;
						}
					}
				}
				if (!flag)
				{
					break;
				}
				RefreshMainCargoResources();
			}
		}

		public void UnloadRefinery()
		{
			bool flag = false;
			ICargo currentlySelectedCargoBay = CurrentlySelectedCargoBay;
			foreach (CargoResourceData resource in Refinery.CargoResources.Resources)
			{
				if (resource.Quantity > 0f)
				{
					TransferResourceMessage transferResourceMessage = null;
					TransferResourceMessage transferResourceMessage2 = new TransferResourceMessage();
					transferResourceMessage2.ToLocationType = ResourceLocationType.CargoBay;
					transferResourceMessage2.ToVesselGuid = CurrentlySelectedCargoBay.ParentVessel.GUID;
					transferResourceMessage2.ToInSceneID = CurrentlySelectedCargoBay.InSceneID;
					transferResourceMessage2.ToCompartmentID = CurrentlySelectedCargoBay.Compartments[0].ID;
					transferResourceMessage2.ResourceType = resource.ResourceType;
					transferResourceMessage2.Quantity = resource.Quantity;
					transferResourceMessage = transferResourceMessage2;
					transferResourceMessage.FromLocationType = ResourceLocationType.Refinery;
					transferResourceMessage.FromVesselGuid = Refinery.ParentVessel.GUID;
					transferResourceMessage.FromInSceneID = Refinery.InSceneID;
					transferResourceMessage.FromCompartmentID = Refinery.Compartments[0].ID;
					if (transferResourceMessage != null)
					{
						NetworkController.Instance.SendToGameServer(transferResourceMessage);
					}
					flag = true;
				}
			}
			if (flag)
			{
				RefreshMainCargoResources();
				UpdateRefineryResources();
			}
		}

		public void UnloadCrafting()
		{
			bool flag = false;
			ICargo currentlySelectedCargoBay = CurrentlySelectedCargoBay;
			foreach (CargoResourceData resource in Fabricator.CargoResources.Resources)
			{
				if (resource.Quantity > 0f)
				{
					TransferResourceMessage transferResourceMessage = null;
					TransferResourceMessage transferResourceMessage2 = new TransferResourceMessage();
					transferResourceMessage2.ToLocationType = ResourceLocationType.CargoBay;
					transferResourceMessage2.ToVesselGuid = CurrentlySelectedCargoBay.ParentVessel.GUID;
					transferResourceMessage2.ToInSceneID = CurrentlySelectedCargoBay.InSceneID;
					transferResourceMessage2.ToCompartmentID = CurrentlySelectedCargoBay.Compartments[0].ID;
					transferResourceMessage2.ResourceType = resource.ResourceType;
					transferResourceMessage2.Quantity = resource.Quantity;
					transferResourceMessage = transferResourceMessage2;
					transferResourceMessage.FromLocationType = ResourceLocationType.Fabricator;
					transferResourceMessage.FromVesselGuid = Fabricator.ParentVessel.GUID;
					transferResourceMessage.FromInSceneID = Fabricator.InSceneID;
					transferResourceMessage.FromCompartmentID = Fabricator.Compartments[0].ID;
					if (transferResourceMessage != null)
					{
						NetworkController.Instance.SendToGameServer(transferResourceMessage);
					}
					flag = true;
				}
			}
			if (flag)
			{
				RefreshMainCargoResources();
				UpdateCraftingResources();
			}
		}

		public void CancelTransfer()
		{
			if (TransferingBox.activeInHierarchy)
			{
				TransferingBox.SetActive(value: false);
			}
			CargoTransferingResource component = DragingItem.GetComponent<CargoTransferingResource>();
			component.FromCompartment = null;
			component.FromResource = null;
			component.ToCompartment = null;
			component.Icon.sprite = null;
			TransferSlider.onValueChanged.RemoveAllListeners();
			TransferInput.onValueChanged.RemoveAllListeners();
		}

		public void SetTransferBox()
		{
			ConfirmTransferButton.onClick.RemoveAllListeners();
			ConfirmTransferButton.onClick.AddListener(delegate
			{
				TransferResources();
			});
			CargoTransferingResource component = DragingItem.GetComponent<CargoTransferingResource>();
			if (component.ToCompartment != null && (component.FromCompartment == null || component.FromCompartment == component.ToCompartment || !component.ToCompartment.IsAllowed(component.FromResource.ResourceType)))
			{
				return;
			}
			TransferSlider.onValueChanged.AddListener(UpdateValueFromSlider);
			TransferInput.onValueChanged.AddListener(UpdateValueFromInputField);
			TransferingBox.SetActive(value: true);
			From.text = Localization.From.ToUpper() + ":" + component.FromCompartment.Name.ToUpper();
			FromName.text = component.FromResource.ResourceType.ToLocalizedString().CamelCaseToSpaced();
			FromIcon.sprite = Client.Instance.SpriteManager.GetSprite(component.FromResource.ResourceType);
			TransferSlider.minValue = 0f;
			if (component.ToCompartment == null)
			{
				To.text = Localization.To.ToUpper() + ": " + Localization.Vent.ToUpper();
				TransferSlider.maxValue = component.Quantity;
			}
			else
			{
				To.text = Localization.To.ToUpper() + ": " + component.ToCompartment.Name.ToUpper();
				if (component.ToCompartment.AvailableCapacity < component.FromResource.Quantity)
				{
					TransferSlider.maxValue = component.ToCompartment.AvailableCapacity;
				}
				else
				{
					TransferSlider.maxValue = component.Quantity;
				}
			}
			CompartmentValue.text = FormatHelper.CurrentMax(TransferSlider.value, TransferSlider.maxValue);
			TransferSlider.value = TransferSlider.maxValue;
			TransferInput.text = TransferSlider.maxValue.ToString("0.#");
		}

		private void TransferValueSlider(float val)
		{
			CompartmentValue.text = FormatHelper.CurrentMax(val, TransferSlider.maxValue);
		}

		public void UpdateValueFromSlider(float value)
		{
			if ((bool)TransferInput)
			{
				TransferInput.text = value.ToString("0.#");
			}
			CompartmentValue.text = FormatHelper.CurrentMax(TransferSlider.value, TransferSlider.maxValue);
		}

		public void UpdateValueFromInputField(string value)
		{
			float.TryParse(value, out var result);
			if ((bool)TransferSlider)
			{
				TransferSlider.value = result;
			}
			CompartmentValue.text = FormatHelper.CurrentMax(result, TransferSlider.maxValue);
		}

		public void TransferResources()
		{
			CargoTransferingResource component = DragingItem.GetComponent<CargoTransferingResource>();
			ICargoCompartment fromCompartment = component.FromCompartment;
			ICargoCompartment toCompartment = component.ToCompartment;
			TransferResourceMessage transferResourceMessage = null;
			if (toCompartment != null && toCompartment.ParentCargo != null && (toCompartment.ParentCargo is SceneCargoBay || toCompartment.ParentCargo is ResourceContainer || toCompartment.ParentCargo is SubSystemRefinery || toCompartment.ParentCargo is SubSystemFabricator))
			{
				VesselComponent vesselComponent = toCompartment.ParentCargo as VesselComponent;
				TransferResourceMessage transferResourceMessage2 = new TransferResourceMessage();
				transferResourceMessage2.ToLocationType = VesselComponentLocationType(toCompartment.ParentCargo);
				transferResourceMessage2.ToVesselGuid = toCompartment.ParentCargo.ParentVessel.GUID;
				transferResourceMessage2.ToInSceneID = vesselComponent.InSceneID;
				transferResourceMessage2.ToCompartmentID = toCompartment.ID;
				transferResourceMessage2.ResourceType = component.FromResource.ResourceType;
				transferResourceMessage2.Quantity = TransferSlider.value;
				transferResourceMessage = transferResourceMessage2;
				if (fromCompartment.ParentCargo.ParentVessel == null)
				{
					transferResourceMessage.FromLocationType = ResourceLocationType.ResourcesTransferPoint;
					transferResourceMessage.FromVesselGuid = AttachPoint.ParentVessel.GUID;
					transferResourceMessage.FromInSceneID = AttachPoint.InSceneID;
					transferResourceMessage.FromCompartmentID = fromCompartment.ID;
				}
				else
				{
					VesselComponent vesselComponent2 = fromCompartment.ParentCargo as VesselComponent;
					transferResourceMessage.FromLocationType = VesselComponentLocationType(vesselComponent2 as ICargo);
					transferResourceMessage.FromVesselGuid = fromCompartment.ParentCargo.ParentVessel.GUID;
					transferResourceMessage.FromInSceneID = vesselComponent2.InSceneID;
					transferResourceMessage.FromCompartmentID = fromCompartment.ID;
				}
			}
			else if (fromCompartment.ParentCargo != null && (fromCompartment.ParentCargo is SceneCargoBay || fromCompartment.ParentCargo is ResourceContainer || fromCompartment.ParentCargo is SubSystemRefinery || fromCompartment.ParentCargo is SubSystemFabricator))
			{
				VesselComponent vesselComponent3 = fromCompartment.ParentCargo as VesselComponent;
				TransferResourceMessage transferResourceMessage2 = new TransferResourceMessage();
				transferResourceMessage2.FromLocationType = VesselComponentLocationType(fromCompartment.ParentCargo);
				transferResourceMessage2.FromVesselGuid = fromCompartment.ParentCargo.ParentVessel.GUID;
				transferResourceMessage2.FromInSceneID = vesselComponent3.InSceneID;
				transferResourceMessage2.FromCompartmentID = fromCompartment.ID;
				transferResourceMessage2.ResourceType = component.FromResource.ResourceType;
				transferResourceMessage2.Quantity = TransferSlider.value;
				transferResourceMessage = transferResourceMessage2;
				if (component.ToCompartment == null)
				{
					transferResourceMessage.ToLocationType = ResourceLocationType.None;
				}
				else if (toCompartment.ParentCargo.ParentVessel == null)
				{
					transferResourceMessage.ToLocationType = ResourceLocationType.ResourcesTransferPoint;
					transferResourceMessage.ToVesselGuid = AttachPoint.ParentVessel.GUID;
					transferResourceMessage.ToInSceneID = AttachPoint.InSceneID;
					transferResourceMessage.ToCompartmentID = toCompartment.ID;
				}
				else
				{
					VesselComponent vesselComponent4 = toCompartment.ParentCargo as VesselComponent;
					transferResourceMessage.ToLocationType = VesselComponentLocationType(toCompartment as ICargo);
					transferResourceMessage.ToVesselGuid = toCompartment.ParentCargo.ParentVessel.GUID;
					transferResourceMessage.ToInSceneID = vesselComponent4.InSceneID;
					transferResourceMessage.ToCompartmentID = toCompartment.ID;
				}
			}
			if (transferResourceMessage != null)
			{
				NetworkController.Instance.SendToGameServer(transferResourceMessage);
			}
			CancelTransfer();
			if (RefineryActive.activeInHierarchy)
			{
				UpdateRefineryResources();
			}
			if (CraftingActive.activeInHierarchy)
			{
				UpdateCraftingResources();
			}
		}

		private static ResourceLocationType VesselComponentLocationType(ICargo vc)
		{
			if (vc is SceneCargoBay)
			{
				return ResourceLocationType.CargoBay;
			}
			if (vc is SubSystemRefinery)
			{
				return ResourceLocationType.Refinery;
			}
			if (vc is SubSystemFabricator)
			{
				return ResourceLocationType.Fabricator;
			}
			return ResourceLocationType.ResourceTank;
		}

		public void OpenCrafingScreen()
		{
			CraftingItemsContent.DestroyAll<CraftingItem>();
			foreach (SubSystemFabricator.CraftableItemData data in Fabricator.CraftableItems)
			{
				if (MyPlayer.Instance.Blueprints.FirstOrDefault((ItemCompoundType m) => m.Type == data.CompoundType.Type && m.SubType == data.CompoundType.SubType && m.PartType == data.CompoundType.PartType && m.Tier == data.CompoundType.Tier) != null)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(CraftingItemUI, CraftingItemsContent);
					gameObject.transform.localScale = Vector3.one;
					CraftingItem component = gameObject.GetComponent<CraftingItem>();
					component.gameObject.transform.localScale = Vector3.one;
					component.Panel = this;
					component.Data = data;
					ItemCompoundType ict = data.CompoundType;
					if (MyPlayer.Instance.Blueprints.Count((ItemCompoundType m) => m.Type == ict.Type && m.SubType == ict.SubType && m.PartType == ict.PartType && m.Tier == ict.Tier) == 0)
					{
						component.GetComponent<Button>().interactable = false;
						component.GetComponent<Tooltip>().enabled = true;
						component.GetComponent<Image>().color = Colors.DistressRed;
					}
					else
					{
						component.GetComponent<Button>().interactable = true;
						component.GetComponent<Tooltip>().enabled = false;
						component.GetComponent<Image>().color = Colors.White;
					}
					if (ShowAvailable.isOn)
					{
						component.gameObject.Activate(component.CanFabricate());
					}
					else
					{
						component.gameObject.Activate(value: true);
					}
				}
			}
			CraftingPowerConsumption.text = FormatHelper.FormatValue(Fabricator.GetPowerConsumption(true)) + " / s";
			MainScreen.Activate(value: false);
			CraftingScreen.Activate(value: true);
		}

		public void SetItemForCrafting()
		{
			if (Fabricator == null)
			{
				return;
			}
			if (CurrentCraftingItem == null)
			{
				CraftingTime.text = "/";
				SelectItem.SetActive(value: true);
				foreach (Transform componentInChild in CurrentItemResources.GetComponentInChildren<Transform>(includeInactive: true))
				{
					UnityEngine.Object.Destroy(componentInChild.gameObject);
				}
			}
			else
			{
				SelectItem.SetActive(value: false);
				CraftingTime.text = FormatHelper.Timer(CurrentCraftingItem.GetTime());
				CurrentItemName.text = Item.GetName(CurrentCraftingItem.Data.CompoundType);
				CurrentItemName.color = Colors.Tier[CurrentCraftingItem.Data.CompoundType.Tier];
				CurrentItemIcon.sprite = Client.Instance.SpriteManager.GetSprite(CurrentCraftingItem.Data.CompoundType);
				CheckCraftingResources();
			}
			CraftingScreen.SetActive(value: false);
			MainScreen.SetActive(value: true);
			CraftingStatusUpdate();
		}

		public void CheckCraftingResources()
		{
			foreach (Transform componentInChild in CurrentItemResources.GetComponentInChildren<Transform>(includeInactive: true))
			{
				UnityEngine.Object.Destroy(componentInChild.gameObject);
			}
			foreach (KeyValuePair<ResourceType, float> resource in CurrentCraftingItem.Data.Resources)
			{
				CargoResourceForCraftingUI cargoResourceForCraftingUI = UnityEngine.Object.Instantiate(CurrentItemResourcesUI, CurrentItemResources);
				cargoResourceForCraftingUI.transform.localScale = Vector3.one;
				cargoResourceForCraftingUI.gameObject.SetActive(value: true);
				cargoResourceForCraftingUI.Icon.sprite = Client.Instance.SpriteManager.GetSprite(resource.Key);
				cargoResourceForCraftingUI.Name.text = resource.Key.ToLocalizedString().ToUpper();
				if (CraftingItems.TryGetValue(resource.Key, out var value))
				{
					cargoResourceForCraftingUI.Value.text = FormatHelper.CurrentMax(value.Quantity, resource.Value);
					if (value.Quantity < resource.Value)
					{
						cargoResourceForCraftingUI.Value.color = Colors.Red;
					}
					else
					{
						cargoResourceForCraftingUI.Value.color = Colors.White;
					}
				}
				else
				{
					cargoResourceForCraftingUI.Value.text = FormatHelper.CurrentMax(0f, resource.Value);
					cargoResourceForCraftingUI.Value.color = Colors.Red;
				}
			}
		}

		public void CloseCraftingScreen()
		{
			CurrentCraftingItem = null;
			CraftingScreen.SetActive(value: false);
			MainScreen.SetActive(value: true);
			CraftingStatusUpdate();
		}

		public void CraftItem()
		{
			Fabricator.FabricateItem(CurrentCraftingItem.Data.CompoundType.Type, CurrentCraftingItem.Data.CompoundType.SubType, CurrentCraftingItem.Data.CompoundType.PartType, CurrentCraftingItem.Data.CompoundType.Tier);
		}

		public void ToggleCancelCrafting(bool cancel)
		{
			CancelCrafingBox.Activate(cancel);
		}

		public void CancelCraftingConfirm()
		{
			Fabricator.CancelFabrication(currentItemOnly: true);
			CancelCrafingBox.Activate(value: false);
		}
	}
}
