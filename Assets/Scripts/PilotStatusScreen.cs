using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

public class PilotStatusScreen : MonoBehaviour
{
	public GameObject InfoScreen;

	public Image EngineFuel;

	public Text EngineValue;

	public Text EngineStatus;

	public Image ENGStatus;

	public GameObject EngPowerUp;

	public GameObject EngineNotAvailable;

	public Text EngNotAvailableLabel;

	public Image RCSFuel;

	public Text RCSValue;

	public Image RCSStatus;

	public Text HealthValue;

	public Image HealthFiller;

	public GameObject HPDanger;

	public Image Power;

	public Text PowerValue;

	public GameObject PowerDanger;

	public GameObject NotActive;

	public GameObject OffTargetAssistant;

	private SceneMachineryPartSlot ArmorSlot;

	public Image Armor;

	public Image NaniteFiller;

	[Title("Warnings")] public GameObject WarningActive;

	public Text WarningLabel;

	public GameObject DebrisWarning;

	public GameObject Breach;

	public GameObject Fire;

	public GameObject Gravity;

	public GameObject System;

	private void Start()
	{
		NotActive.SetActive(value: true);
		WarningLabel.text = Localization.Warning.ToUpper();
		EngNotAvailableLabel.text = Localization.EngineNotAvailable.ToUpper();
	}

	public void UpdateSystemsInfo(Ship parentShip)
	{
		if (ArmorSlot == null)
		{
			ArmorSlot = parentShip.VesselBaseSystem.MachineryPartSlots
				.FirstOrDefault((SceneMachineryPartSlot m) => m.Scope == MachineryPartSlotScope.Armor);
		}

		OffTargetAssistant.Activate(parentShip.OffSpeedHelper);
		float num = parentShip.Health / parentShip.MaxHealth;
		HealthValue.text = FormatHelper.Percentage(num);
		HealthFiller.fillAmount = num;
		HPDanger.Activate(num < 0.2f);
		if (parentShip.RCS != null)
		{
			float num2 = parentShip.RCS.ResourceContainers[0].Compartments[0].Capacity -
			             parentShip.RCS.ResourceContainers[0].Compartments[0].AvailableCapacity;
			float capacity = parentShip.RCS.ResourceContainers[0].Compartments[0].Capacity;
			float num3 = num2 / capacity;
			RCSValue.text = FormatHelper.Percentage(num3);
			RCSFuel.fillAmount = num3;
			RCSFuel.color = num3 < 0.2f ? Colors.Red : Colors.Orange;
			RCSStatus.color = parentShip.RCS.Status != SystemStatus.Online
				? Colors.GrayDefault
				: Colors.White;
		}

		if (parentShip.Engine != null)
		{
			float num4 = parentShip.Engine.ResourceContainers[0].Compartments[0].Capacity -
			             parentShip.Engine.ResourceContainers[0].Compartments[0].AvailableCapacity;
			float capacity2 = parentShip.Engine.ResourceContainers[0].Compartments[0].Capacity;
			float val = num4 / capacity2;
			EngineValue.text = FormatHelper.Percentage(val);
			EngineStatus.text = parentShip.Engine.GetStatus(out var color);
			EngineStatus.color = color;
			ENGStatus.color = (parentShip.Engine.Status != SystemStatus.Online)
				? Colors.GrayDefault
				: Colors.White;
			EngPowerUp.SetActive(parentShip.Engine.Status == SystemStatus.Powerup);
			EngineNotAvailable.Activate(value: false);
		}
		else
		{
			EngineStatus.text = string.Empty;
			EngineNotAvailable.Activate(value: true);
		}

		if (parentShip.Capacitor != null)
		{
			float num5 = parentShip.Capacitor.Capacity / parentShip.Capacitor.MaxCapacity;
			PowerValue.text = FormatHelper.Percentage(num5);
			Power.fillAmount = num5;
			PowerDanger.SetActive(num5 < 0.2f);
		}

		WarningsUpdate(parentShip);
		if (parentShip.ExposureDamage * SpaceObjectVessel.VesselDecayRateMultiplier * 3600.0 >
		    parentShip.Armor * 3600f)
		{
			Armor.color = Colors.FormatedRed;
		}
		else
		{
			Armor.color = Colors.ArmorActive;
		}

		if (((ArmorSlot is not null) ? ArmorSlot.Item : null) is not null)
		{
			NaniteFiller.fillAmount = ArmorSlot.Item.Health / ArmorSlot.Item.MaxHealth;
		}
		else
		{
			NaniteFiller.fillAmount = 0f;
		}
	}

	public void ToggleStatusScreen(bool toggle)
	{
		NotActive.SetActive(toggle);
	}

	public void WarningsUpdate(Ship parentShip)
	{
		Breach.Activate(value: false);
		Fire.Activate(value: false);
		Gravity.Activate(value: false);
		CheckSystems(parentShip);
		if (parentShip != null)
		{
			SceneTriggerRoom[] componentsInChildren =
				parentShip.MainVessel.GetComponentsInChildren<SceneTriggerRoom>();
			foreach (SceneTriggerRoom sceneTriggerRoom in componentsInChildren)
			{
				if (sceneTriggerRoom.Breach)
				{
					Breach.Activate(value: true);
				}

				if (sceneTriggerRoom.Fire)
				{
					Fire.Activate(value: true);
				}

				if (sceneTriggerRoom.GravityMalfunction)
				{
					Gravity.Activate(value: true);
				}
			}
		}

		DebrisWarning.Activate(MyPlayer.Instance.InDebrisField != null);
		WarningActive.Activate(Breach.activeSelf || Fire.activeSelf || Gravity.activeSelf || DebrisWarning.activeSelf ||
		                       System.activeSelf);
		if (WarningActive.activeInHierarchy && WarningLabel.color != Colors.White)
		{
			WarningLabel.color = Colors.White;
		}
		else if (!WarningActive.activeInHierarchy && WarningLabel.color != Colors.Gray)
		{
			WarningLabel.color = Colors.Gray;
		}
	}

	public void CheckSystems(Ship parentShip)
	{
		bool value = false;
		foreach (SubSystem value2 in parentShip.SubSystems.Values)
		{
			if (value2.SecondaryStatus == SystemSecondaryStatus.Defective)
			{
				value = true;
			}
		}

		foreach (Generator value3 in parentShip.Generators.Values)
		{
			if (value3.SecondaryStatus == SystemSecondaryStatus.Defective)
			{
				value = true;
			}
		}

		System.Activate(value);
	}
}
