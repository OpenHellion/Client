using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class ConnectedVessel : MonoBehaviour
	{
		[NonSerialized]
		public SpaceObjectVessel Vessel;

		public Text VesselName;

		public Image Icon;

		public GameObject Authorized;

		[NonSerialized]
		public PowerSupply PowerPanel;

		[NonSerialized]
		public VesselComponent Vc;

		[NonSerialized]
		public VesselBaseSystem Base;

		[Title("POWER SUPPLY")]
		public Text BaseConsumption;

		public Text Consumption;

		public GameObject ToggleStatus;

		public GameObject IsOnline;

		[NonSerialized]
		public LifeSupportPanel LifePanel;

		[Title("LIFE SUPPORT")]
		public Text Volume;

		public Text ArmorValue;

		public Image HealthFiller;

		public Text HealthValue;

		private List<SceneMachineryPartSlot> ArmorSlots;

		public Image NaniteFiller;

		public Text NaniteValue;

		[NonSerialized]
		public CargoPanel CargoPanel;

		public bool IsAuthorized => Vessel.IsPlayerAuthorizedOrNoSecurity(MyPlayer.Instance);

		private void Start()
		{
			Text[] componentsInChildren = GetComponentsInChildren<Text>(includeInactive: true);
			foreach (Text text in componentsInChildren)
			{
				string value = null;
				if (Localization.PanelsLocalization.TryGetValue(text.name, out value))
				{
					text.text = value;
				}
			}
			VesselName.text = Vessel.CustomName;
			Icon.sprite = Client.Instance.SpriteManager.GetSprite(Vessel);
			Authorized.SetActive(!IsAuthorized);
			ArmorSlots = Vessel.VesselBaseSystem.MachineryPartSlots.Where((SceneMachineryPartSlot m) => m.Scope == MachineryPartSlotScope.Armor).ToList();
			if (LifePanel != null)
			{
				HealthAndArmorUpdate();
			}
		}

		public void Update()
		{
		}

		public void ToggleVessel()
		{
			Base.Toggle();
		}

		public void ToggleRooms()
		{
			LSRoomsUI[] componentsInChildren = GetComponentsInChildren<LSRoomsUI>(includeInactive: true);
			foreach (LSRoomsUI lSRoomsUI in componentsInChildren)
			{
				lSRoomsUI.gameObject.SetActive(!lSRoomsUI.gameObject.activeInHierarchy);
			}
		}

		public void ToggleConsumers()
		{
			PowerSupplyPowerConsumer[] componentsInChildren = GetComponentsInChildren<PowerSupplyPowerConsumer>(includeInactive: true);
			foreach (PowerSupplyPowerConsumer powerSupplyPowerConsumer in componentsInChildren)
			{
				powerSupplyPowerConsumer.gameObject.SetActive(!powerSupplyPowerConsumer.gameObject.activeInHierarchy);
			}
		}

		public void HealthAndArmorUpdate()
		{
			HealthFiller.fillAmount = Vessel.Health / Vessel.MaxHealth;
			HealthValue.text = FormatHelper.Percentage(Vessel.Health / Vessel.MaxHealth);
			ArmorValue.text = FormatHelper.FormatValue(Vessel.Armor);
			float num = 0f;
			float num2 = 0f;
			if (ArmorSlots != null)
			{
				foreach (SceneMachineryPartSlot armorSlot in ArmorSlots)
				{
					if (armorSlot?.Item != null)
					{
						num += armorSlot.Item.MaxHealth;
						num2 += armorSlot.Item.Health;
					}
				}
			}
			NaniteFiller.fillAmount = num2 / num;
			NaniteValue.text = FormatHelper.FormatValue(num2);
		}
	}
}
