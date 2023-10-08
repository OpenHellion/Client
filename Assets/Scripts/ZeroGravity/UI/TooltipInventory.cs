using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class TooltipInventory : MonoBehaviour
	{
		public Text ItemName;

		public Text ItemTooltip;

		public GameObject Health;

		public GameObject Battery;

		public GameObject Ammo;

		public GameObject Damage;

		public GameObject Capacity;

		public GameObject Armor;

		public Text ItemHealth;

		public Text ItemBattery;

		public Text ItemAmmo;

		public Text ItemDamage;

		public Text ItemCapacity;

		public Text ItemArmor;

		public Transform Root;

		public GameObject StatusBar;

		public GameObject RecycleResourcesHolder;

		public Transform ResourcesTransform;

		public GameObject ResourceObj;

		public Item CurrentItem;

		public void OnDisable()
		{
			CurrentItem = null;
		}

		public void SetTooltip(Item item, bool recycle = false)
		{
			if (!(item == CurrentItem))
			{
				FillData(item, recycle);
			}
		}

		public void FillData(Item item, bool recycle)
		{
			StatusBarUI[] componentsInChildren = Root.GetComponentsInChildren<StatusBarUI>(true);
			foreach (StatusBarUI statusBarUI in componentsInChildren)
			{
				DestroyImmediate(statusBarUI.gameObject);
			}

			IEnumerator enumerator = ResourcesTransform.GetComponentInChildren<Transform>(true).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Transform transform = (Transform)enumerator.Current;
					Destroy(transform.gameObject);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}

			RecycleResourcesHolder.gameObject.SetActive(false);
			if (item == null)
			{
				return;
			}

			CurrentItem = item;
			ItemName.text = CurrentItem.Name;
			ItemName.color = Colors.Tier[item.Tier];
			Text itemName = ItemName;
			string text = itemName.text;
			itemName.text = text + " (" + Localization.Tier.ToUpper() + ": " + item.Tier + ")";
			ItemTooltip.text = CurrentItem.ToolTip;
			ClearIcons();
			if (item is MachineryPart)
			{
				MachineryPart machineryPart = item as MachineryPart;
				Text itemTooltip = ItemTooltip;
				itemTooltip.text = itemTooltip.text + " " + FormatHelper.PartTier(machineryPart);
				if (machineryPart.PartType == MachineryPartType.NaniteCore ||
				    machineryPart.PartType == MachineryPartType.MillitaryNaniteCore)
				{
					Armor.Activate(true);
					ItemArmor.text = machineryPart.AuxValue.ToString("0.0");
				}
			}
			else if (!(item is GenericItem))
			{
				if (item is Jetpack)
				{
					Jetpack jetpack = item as Jetpack;
					AddBar(Localization.Oxygen.ToUpper(), jetpack.CurrentOxygen, jetpack.MaxOxygen);
					AddBar(Localization.Fuel.ToUpper(), jetpack.CurrentFuel, jetpack.MaxFuel);
				}
				else if (item is Battery)
				{
					Battery battery = item as Battery;
					AddBar(Localization.Power.ToUpper(), battery.CurrentPower, battery.MaxPower);
				}
				else if (item is Canister)
				{
					Canister canister = item as Canister;
					AddBar(Localization.Capacity.ToUpper(), canister.Quantity, canister.MaxQuantity);
				}
				else if (item is RepairTool)
				{
					RepairTool repairTool = item as RepairTool;
					AddBar(Localization.Capacity.ToUpper(), repairTool.Quantity, repairTool.MaxQuantity);
				}
			}

			Text itemTooltip2 = ItemTooltip;
			itemTooltip2.text = itemTooltip2.text + "\n" + item.GetInfo();
			if (item.Expendable)
			{
				Health.Activate(true);
				if (item is MachineryPart)
				{
					MachineryPart machineryPart2 = item as MachineryPart;
					ItemHealth.text = FormatHelper.CurrentMax(item.Health, item.MaxHealth);
				}
				else
				{
					ItemHealth.text = FormatHelper.CurrentMax(item.Health, item.MaxHealth);
				}
			}

			if (item is Canister || item is RepairTool || item is HandDrill)
			{
				Capacity.Activate(true);
				if (item is Canister)
				{
					ItemCapacity.text =
						FormatHelper.CurrentMax((item as Canister).Quantity, (item as Canister).MaxQuantity);
				}
				else if (item is HandDrill)
				{
					ItemCapacity.text =
						FormatHelper.CurrentMax((item as HandDrill).Quantity, (item as HandDrill).MaxQuantity);
				}
				else
				{
					ItemCapacity.text =
						FormatHelper.CurrentMax((item as RepairTool).Quantity, (item as RepairTool).MaxQuantity);
				}
			}

			if (item is IBatteryConsumer || item is Battery)
			{
				Battery.Activate(true);
				if (item is IBatteryConsumer)
				{
					ItemBattery.text = FormatHelper.Percentage((item as IBatteryConsumer).BatteryPower);
				}
				else
				{
					ItemBattery.text = FormatHelper.Percentage((item as Battery).BatteryPrecentage);
				}
			}

			if (item is Outfit)
			{
				Armor.Activate(true);
				ItemArmor.text = (item as Outfit).Armor.ToString("0.0");
			}

			if (item is Weapon)
			{
				Ammo.Activate(true);
				Damage.Activate(true);
				ItemAmmo.text = (item as Weapon).QuantityCheck();
				ItemDamage.text = (item as Weapon).CurrentWeaponMod.Damage.ToString("0.0");
			}

			if (item is Magazine)
			{
				Ammo.Activate(true);
				ItemAmmo.text = (item as Magazine).QuantityCheck();
			}

			if (recycle)
			{
				GetRecycleResources(item);
			}
		}

		private void GetRecycleResources(Item itm)
		{
			IEnumerator enumerator = ResourcesTransform.GetComponentInChildren<Transform>(true).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Transform transform = (Transform)enumerator.Current;
					UnityEngine.Object.Destroy(transform.gameObject);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}

			Dictionary<ResourceType, float> recycleResources = Item.GetRecycleResources(itm);
			if (recycleResources != null)
			{
				foreach (KeyValuePair<ResourceType, float> item in recycleResources)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(ResourceObj, ResourcesTransform);
					gameObject.transform.localScale = Vector3.one;
					gameObject.SetActive(true);
					CargoResourceForCraftingUI component = gameObject.GetComponent<CargoResourceForCraftingUI>();
					component.Icon.sprite = SpriteManager.Instance.GetSprite(item.Key);
					component.Value.text = FormatHelper.FormatValue(item.Value);
					component.Name.text = item.Key.ToLocalizedString().ToUpper();
				}
			}

			RecycleResourcesHolder.transform.SetAsLastSibling();
			RecycleResourcesHolder.gameObject.SetActive(true);
		}

		private void AddBar(string name, float value, float maxValue)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(StatusBar, Root);
			gameObject.SetActive(true);
			gameObject.transform.Reset(true);
			StatusBarUI component = gameObject.GetComponent<StatusBarUI>();
			component.Name.text = name;
			component.Filler.fillAmount = value / maxValue;
			component.Value.text = FormatHelper.CurrentMax(value, maxValue);
		}

		private void ClearIcons()
		{
			Health.Activate(false);
			Battery.Activate(false);
			Ammo.Activate(false);
			Damage.Activate(false);
			Capacity.Activate(false);
			Armor.Activate(false);
		}
	}
}
