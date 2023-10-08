using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenHellion;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	[DisallowMultipleComponent]
	public class Canister : Item, ICargo
	{
		public CargoCompartment CargoCompartment;

		private List<ICargoCompartment> _compartments;

		public GameObject CanisterEmpty;

		public GameObject StatusHolder;

		public Image ResourceBarFiller;

		private int _currentHandDrillResourceIndex;

		public Text ResourceName;

		public Text ResourceValue;

		public Text NoOfElements;

		public bool IsLookingAtPanel;

		private bool _shouldInjectResource;

		public bool HasSpace => MaxQuantity - Quantity > float.Epsilon;

		public override float MaxQuantity => CargoCompartment.Capacity;

		public override float Quantity => CargoCompartment.Resources?.Sum((CargoResourceData m) => m.Quantity) ?? 0f;

		public float ResourcePercentage => Quantity / MaxQuantity;

		public new string Name => base.Name;

		public List<ICargoCompartment> Compartments => _compartments;

		public SpaceObjectVessel ParentVessel => null;

		public override DynamicObjectAuxData GetAuxData()
		{
			CanisterData baseAuxData = GetBaseAuxData<CanisterData>();
			baseAuxData.CargoCompartment = CargoCompartment.GetData();
			return baseAuxData;
		}

		public override void ProcesStatsData(DynamicObjectStats dos)
		{
			base.ProcesStatsData(dos);
			CanisterStats canisterStats = dos as CanisterStats;
			if (_compartments == null)
			{
				_compartments = new List<ICargoCompartment> { CargoCompartment };
			}

			CargoCompartment.Capacity = canisterStats.Capacity;
			CargoCompartment.Resources = canisterStats.Resources;
			ResourceBarFiller.fillAmount = ResourcePercentage;
			if (AttachPoint is not null && MyPlayer.Instance.IsLockedToTrigger &&
			    MyPlayer.Instance.LockedToTrigger is SceneTriggerCargoPanel)
			{
				SceneTriggerCargoPanel sceneTriggerCargoPanel =
					MyPlayer.Instance.LockedToTrigger as SceneTriggerCargoPanel;
				sceneTriggerCargoPanel.CargoPanel.RefreshAttachedItemResources();
			}

			UpdateResources();
		}

		protected override void Awake()
		{
			base.Awake();
			_compartments = new List<ICargoCompartment> { CargoCompartment };
		}

		private new void Start()
		{
			base.Start();
			if (CargoCompartment.Resources == null)
			{
				CargoCompartment.Resources = new List<CargoResourceData>();
			}

			UpdateResources();
			if (MyPlayer.Instance.IsAlive)
			{
				GetComponentInChildren<Canvas>().worldCamera = MyPlayer.Instance.FpsController.MainCamera;
			}
		}

		public ICargoCompartment GetCompartment(short? id = null)
		{
			if (id.HasValue)
			{
				return _compartments.Find((ICargoCompartment m) => m.ID == id.Value);
			}

			return _compartments[0];
		}

		private void UpdateResources()
		{
			if (Type == ItemType.AltairResourceContainer)
			{
				if (CargoCompartment is not null && CargoCompartment.Resources != null &&
				    CargoCompartment.Resources.Count != 0 &&
				    _currentHandDrillResourceIndex < CargoCompartment.Resources.Count)
				{
					CanisterEmpty.Activate(value: false);
					StatusHolder.Activate(value: true);
					ResourceName.text = CargoCompartment.Resources[_currentHandDrillResourceIndex].ResourceType
						.ToString().CamelCaseToSpaced().ToUpper();
					ResourceValue.text = CargoCompartment.Resources[_currentHandDrillResourceIndex].Quantity
						.ToString("f0");
					ResourceBarFiller.fillAmount = ResourcePercentage;
				}
				else
				{
					CanisterEmpty.Activate(value: true);
					ResourceName.text = Localization.Empty.ToUpper();
					ResourceBarFiller.fillAmount = 0f;
					ResourceValue.text = string.Empty;
				}
			}
			else if (Type == ItemType.AltairHandDrillCanister || Type == ItemType.AltairRefinedCanister)
			{
				if (CargoCompartment is not null && CargoCompartment.Resources != null &&
				    _currentHandDrillResourceIndex < CargoCompartment.Resources.Count &&
				    CargoCompartment.Resources[_currentHandDrillResourceIndex].Quantity != 0f)
				{
					CanisterEmpty.Activate(value: false);
					StatusHolder.Activate(value: true);
					ResourceName.text = CargoCompartment.Resources[_currentHandDrillResourceIndex].ResourceType
						.ToString().CamelCaseToSpaced().ToUpper();
					ResourceValue.text = CargoCompartment.Resources[_currentHandDrillResourceIndex].Quantity
						.ToString("f0");
					NoOfElements.text = _currentHandDrillResourceIndex + 1 + "/" + CargoCompartment.Resources.Count;
				}
				else
				{
					StatusHolder.Activate(value: false);
					CanisterEmpty.Activate(value: true);
					ResourceName.text = string.Empty;
					ResourceValue.text = string.Empty;
					NoOfElements.text = string.Empty;
				}
			}

			if (MyPlayer.Instance is not null && MyPlayer.Instance.Inventory != null &&
			    (base.Slot == MyPlayer.Instance.Inventory.HandsSlot || (DynamicObj.Parent is DynamicObject &&
			                                                            (DynamicObj.Parent as DynamicObject).Item
			                                                            .Slot ==
			                                                            MyPlayer.Instance.Inventory.HandsSlot)))
			{
				World.InGameGUI.HelmetHud.HandsSlotUpdate();
			}
		}

		public override bool PrimaryFunction()
		{
			if (Type == ItemType.AltairResourceContainer && GetComponentInParent<MyPlayer>() is not null &&
			    MyPlayer.Instance.FpsController.CurrentJetpack is not null)
			{
				if (MyPlayer.Instance.animHelper.IsConsumableInUse)
				{
					return false;
				}

				_shouldInjectResource = false;
				foreach (ICargoCompartment compartment in Compartments)
				{
					if (compartment.Resources == null)
					{
						continue;
					}

					foreach (CargoResourceData resource in compartment.Resources)
					{
						if ((resource.ResourceType == ResourceType.Nitro && resource.Quantity > 0f &&
						     MyPlayer.Instance.FpsController.CurrentJetpack.CurrentFuel <
						     MyPlayer.Instance.FpsController.CurrentJetpack.MaxFuel) ||
						    (resource.ResourceType == ResourceType.Oxygen && resource.Quantity > 0f &&
						     MyPlayer.Instance.FpsController.CurrentJetpack.CurrentOxygen <
						     MyPlayer.Instance.FpsController.CurrentJetpack.MaxOxygen))
						{
							_shouldInjectResource = true;
						}
					}
				}

				if (_shouldInjectResource)
				{
					DynamicObject dynamicObj = DynamicObj;
					CanisterStats statsData = new CanisterStats
					{
						UseCanister = true
					};
					dynamicObj.SendStatsMessage(null, statsData);
					MyPlayer.Instance.animHelper.SetParameterTrigger(AnimatorHelper.Triggers.UseConsumable);
				}
				else if (Quantity <= 0f)
				{
					World.InGameGUI.ShowInteractionCanvasMessage(Localization.CanisterIsEmpty);
				}
				else
				{
					World.InGameGUI.ShowInteractionCanvasMessage(Localization.ResourcesAreAlreadyFull);
				}
			}

			return false;
		}

		public override string GetInfo()
		{
			string text = string.Empty;
			if (ResourcePercentage > 0f)
			{
				foreach (CargoResourceData resource in CargoCompartment.Resources)
				{
					text = text + "\n" + resource.ResourceType.ToLocalizedString() + " " +
					       resource.Quantity.ToString("f1");
				}

				return text;
			}

			return text;
		}

		public override string QuantityCheck()
		{
			return FormatHelper.Percentage(Quantity / MaxQuantity);
		}
	}
}
