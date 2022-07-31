using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
		[CompilerGenerated]
		private sealed class _003CGetCompartment_003Ec__AnonStorey0
		{
			internal short? id;

			internal bool _003C_003Em__0(ICargoCompartment m)
			{
				return m.ID == id.Value;
			}
		}

		public CargoCompartment CargoCompartment;

		private List<ICargoCompartment> _Compartments;

		public HandDrill handDrill;

		public GameObject CanisterEmpty;

		public GameObject StatusHolder;

		public Image ResourceBarFiller;

		private int currentHandDrillResourceIndex;

		public Text ResourceName;

		public Text ResourceValue;

		public Text NoOfElements;

		public bool IsLookingAtPanel;

		private bool shouldInjectResource;

		[CompilerGenerated]
		private static Func<CargoResourceData, float> _003C_003Ef__am_0024cache0;

		public bool HasSpace
		{
			get
			{
				return MaxQuantity - Quantity > float.Epsilon;
			}
		}

		public override float MaxQuantity
		{
			get
			{
				return CargoCompartment.Capacity;
			}
		}

		public override float Quantity
		{
			get
			{
				float result;
				if (CargoCompartment.Resources != null)
				{
					List<CargoResourceData> resources = CargoCompartment.Resources;
					if (_003C_003Ef__am_0024cache0 == null)
					{
						_003C_003Ef__am_0024cache0 = _003Cget_Quantity_003Em__0;
					}
					result = resources.Sum(_003C_003Ef__am_0024cache0);
				}
				else
				{
					result = 0f;
				}
				return result;
			}
		}

		public float ResourcePercentage
		{
			get
			{
				return Quantity / MaxQuantity;
			}
		}

		public new string Name
		{
			get
			{
				return base.Name;
			}
		}

		public List<ICargoCompartment> Compartments
		{
			get
			{
				return _Compartments;
			}
		}

		public SpaceObjectVessel ParentVessel
		{
			get
			{
				return null;
			}
		}

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
			if (_Compartments == null)
			{
				_Compartments = new List<ICargoCompartment> { CargoCompartment };
			}
			CargoCompartment.Capacity = canisterStats.Capacity;
			CargoCompartment.Resources = canisterStats.Resources;
			ResourceBarFiller.fillAmount = ResourcePercentage;
			if (base.AttachPoint != null && MyPlayer.Instance.IsLockedToTrigger && MyPlayer.Instance.LockedToTrigger is SceneTriggerCargoPanel)
			{
				SceneTriggerCargoPanel sceneTriggerCargoPanel = MyPlayer.Instance.LockedToTrigger as SceneTriggerCargoPanel;
				sceneTriggerCargoPanel.CargoPanel.RefreshAttachedItemResources();
			}
			UpdateResources();
		}

		protected override void Awake()
		{
			base.Awake();
			_Compartments = new List<ICargoCompartment> { CargoCompartment };
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

		private void Update()
		{
		}

		public ICargoCompartment GetCompartment(short? id = null)
		{
			_003CGetCompartment_003Ec__AnonStorey0 _003CGetCompartment_003Ec__AnonStorey = new _003CGetCompartment_003Ec__AnonStorey0();
			_003CGetCompartment_003Ec__AnonStorey.id = id;
			if (_003CGetCompartment_003Ec__AnonStorey.id.HasValue)
			{
				return _Compartments.Find(_003CGetCompartment_003Ec__AnonStorey._003C_003Em__0);
			}
			return _Compartments[0];
		}

		private void UpdateResources()
		{
			if (Type == ItemType.AltairResourceContainer)
			{
				if (CargoCompartment != null && CargoCompartment.Resources != null && CargoCompartment.Resources.Count != 0 && currentHandDrillResourceIndex < CargoCompartment.Resources.Count)
				{
					CanisterEmpty.Activate(false);
					StatusHolder.Activate(true);
					ResourceName.text = CargoCompartment.Resources[currentHandDrillResourceIndex].ResourceType.ToString().CamelCaseToSpaced().ToUpper();
					ResourceValue.text = CargoCompartment.Resources[currentHandDrillResourceIndex].Quantity.ToString("f0");
					ResourceBarFiller.fillAmount = ResourcePercentage;
				}
				else
				{
					CanisterEmpty.Activate(true);
					ResourceName.text = Localization.Empty.ToUpper();
					ResourceBarFiller.fillAmount = 0f;
					ResourceValue.text = string.Empty;
				}
			}
			else if (Type == ItemType.AltairHandDrillCanister || Type == ItemType.AltairRefinedCanister)
			{
				if (CargoCompartment != null && CargoCompartment.Resources != null && currentHandDrillResourceIndex < CargoCompartment.Resources.Count && CargoCompartment.Resources[currentHandDrillResourceIndex].Quantity != 0f)
				{
					CanisterEmpty.Activate(false);
					StatusHolder.Activate(true);
					ResourceName.text = CargoCompartment.Resources[currentHandDrillResourceIndex].ResourceType.ToString().CamelCaseToSpaced().ToUpper();
					ResourceValue.text = CargoCompartment.Resources[currentHandDrillResourceIndex].Quantity.ToString("f0");
					NoOfElements.text = currentHandDrillResourceIndex + 1 + "/" + CargoCompartment.Resources.Count;
				}
				else
				{
					StatusHolder.Activate(false);
					CanisterEmpty.Activate(true);
					ResourceName.text = string.Empty;
					ResourceValue.text = string.Empty;
					NoOfElements.text = string.Empty;
				}
			}
			if (MyPlayer.Instance != null && MyPlayer.Instance.Inventory != null && (base.Slot == MyPlayer.Instance.Inventory.HandsSlot || (DynamicObj.Parent is DynamicObject && (DynamicObj.Parent as DynamicObject).Item.Slot == MyPlayer.Instance.Inventory.HandsSlot)))
			{
				Client.Instance.CanvasManager.CanvasUI.HelmetHud.HandsSlotUpdate();
			}
		}

		public override bool PrimaryFunction()
		{
			if (Type == ItemType.AltairResourceContainer && GetComponentInParent<MyPlayer>() != null && MyPlayer.Instance.FpsController.CurrentJetpack != null)
			{
				if (MyPlayer.Instance.animHelper.IsConsumableInUse)
				{
					return false;
				}
				shouldInjectResource = false;
				foreach (ICargoCompartment compartment in Compartments)
				{
					if (compartment.Resources == null)
					{
						continue;
					}
					foreach (CargoResourceData resource in compartment.Resources)
					{
						if ((resource.ResourceType == ResourceType.Nitro && resource.Quantity > 0f && MyPlayer.Instance.FpsController.CurrentJetpack.CurrentFuel < MyPlayer.Instance.FpsController.CurrentJetpack.MaxFuel) || (resource.ResourceType == ResourceType.Oxygen && resource.Quantity > 0f && MyPlayer.Instance.FpsController.CurrentJetpack.CurrentOxygen < MyPlayer.Instance.FpsController.CurrentJetpack.MaxOxygen))
						{
							shouldInjectResource = true;
						}
					}
				}
				if (shouldInjectResource)
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
					Client.Instance.CanvasManager.ShowInteractionCanvasMessage(Localization.CanisterIsEmpty);
				}
				else
				{
					Client.Instance.CanvasManager.ShowInteractionCanvasMessage(Localization.ResourcesAreAlreadyFull);
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
					string text2 = text;
					text = text2 + "\n" + resource.ResourceType.ToLocalizedString() + " " + resource.Quantity.ToString("f1");
				}
				return text;
			}
			return text;
		}

		public override string QuantityCheck()
		{
			return FormatHelper.Percentage(Quantity / MaxQuantity);
		}

		[CompilerGenerated]
		private static float _003Cget_Quantity_003Em__0(CargoResourceData m)
		{
			return m.Quantity;
		}
	}
}
