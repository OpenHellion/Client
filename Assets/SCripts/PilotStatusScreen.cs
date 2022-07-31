using System;
using System.Linq;
using System.Runtime.CompilerServices;
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

	[Header("Warinngs")]
	public GameObject WarningActive;

	public Text WarningLabel;

	public GameObject DebrisWarning;

	public GameObject Breach;

	public GameObject Fire;

	public GameObject Gravity;

	public GameObject System;

	[CompilerGenerated]
	private static Func<SceneMachineryPartSlot, bool> _003C_003Ef__am_0024cache0;

	public PilotOverlayUI ParentPilot
	{
		get
		{
			return Client.Instance.InGamePanels.Pilot;
		}
	}

	private void Start()
	{
		NotActive.SetActive(true);
		WarningLabel.text = Localization.Warning.ToUpper();
		EngNotAvailableLabel.text = Localization.EngineNotAvailable.ToUpper();
	}

	public void UpdateSystemsInfo()
	{
		if (ArmorSlot == null)
		{
			SceneMachineryPartSlot[] machineryPartSlots = ParentPilot.ParentShip.VesselBaseSystem.MachineryPartSlots;
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CUpdateSystemsInfo_003Em__0;
			}
			ArmorSlot = machineryPartSlots.Where(_003C_003Ef__am_0024cache0).FirstOrDefault();
		}
		OffTargetAssistant.Activate(Client.Instance.OffSpeedHelper);
		float num = ParentPilot.ParentShip.Health / ParentPilot.ParentShip.MaxHealth;
		HealthValue.text = FormatHelper.Percentage(num);
		HealthFiller.fillAmount = num;
		HPDanger.Activate(num < 0.2f);
		if (ParentPilot.ParentShip.RCS != null)
		{
			float num2 = ParentPilot.ParentShip.RCS.ResourceContainers[0].Compartments[0].Capacity - ParentPilot.ParentShip.RCS.ResourceContainers[0].Compartments[0].AvailableCapacity;
			float capacity = ParentPilot.ParentShip.RCS.ResourceContainers[0].Compartments[0].Capacity;
			float num3 = num2 / capacity;
			RCSValue.text = FormatHelper.Percentage(num3);
			RCSFuel.fillAmount = num3;
			RCSFuel.color = ((!(num3 < 0.2f)) ? Colors.Orange : Colors.Red);
			RCSStatus.color = ((ParentPilot.ParentShip.RCS.Status != SystemStatus.OnLine) ? Colors.GrayDefault : Colors.White);
		}
		if (ParentPilot.ParentShip.Engine != null)
		{
			float num4 = ParentPilot.ParentShip.Engine.ResourceContainers[0].Compartments[0].Capacity - ParentPilot.ParentShip.Engine.ResourceContainers[0].Compartments[0].AvailableCapacity;
			float capacity2 = ParentPilot.ParentShip.Engine.ResourceContainers[0].Compartments[0].Capacity;
			float val = num4 / capacity2;
			EngineValue.text = FormatHelper.Percentage(val);
			Color color;
			EngineStatus.text = ParentPilot.ParentShip.Engine.GetStatus(out color);
			EngineStatus.color = color;
			ENGStatus.color = ((ParentPilot.ParentShip.Engine.Status != SystemStatus.OnLine) ? Colors.GrayDefault : Colors.White);
			EngPowerUp.SetActive(ParentPilot.ParentShip.Engine.Status == SystemStatus.PowerUp);
			EngineNotAvailable.Activate(false);
		}
		else
		{
			EngineStatus.text = string.Empty;
			EngineNotAvailable.Activate(true);
		}
		if (ParentPilot.ParentShip.Capacitor != null)
		{
			float num5 = ParentPilot.ParentShip.Capacitor.Capacity / ParentPilot.ParentShip.Capacitor.MaxCapacity;
			PowerValue.text = FormatHelper.Percentage(num5);
			Power.fillAmount = num5;
			PowerDanger.SetActive(num5 < 0.2f);
		}
		WarningsUpdate();
		if ((double)ParentPilot.ParentShip.ExposureDamage * SpaceObjectVessel.VesselDecayRateMultiplier * 3600.0 > (double)(ParentPilot.ParentShip.Armor * 3600f))
		{
			Armor.color = Colors.FormatedRed;
		}
		else
		{
			Armor.color = Colors.ArmorActive;
		}
		if ((((object)ArmorSlot != null) ? ArmorSlot.Item : null) != null)
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

	public void WarningsUpdate()
	{
		Breach.Activate(false);
		Fire.Activate(false);
		Gravity.Activate(false);
		CheckSystems();
		if (ParentPilot.ParentShip != null)
		{
			SceneTriggerRoom[] componentsInChildren = ParentPilot.ParentShip.MainVessel.GetComponentsInChildren<SceneTriggerRoom>();
			foreach (SceneTriggerRoom sceneTriggerRoom in componentsInChildren)
			{
				if (sceneTriggerRoom.Breach)
				{
					Breach.Activate(true);
				}
				if (sceneTriggerRoom.Fire)
				{
					Fire.Activate(true);
				}
				if (sceneTriggerRoom.GravityMalfunction)
				{
					Gravity.Activate(true);
				}
			}
		}
		DebrisWarning.Activate(MyPlayer.Instance.InDebrisField != null);
		WarningActive.Activate(Breach.activeSelf || Fire.activeSelf || Gravity.activeSelf || DebrisWarning.activeSelf || System.activeSelf);
		if (WarningActive.activeInHierarchy && WarningLabel.color != Colors.White)
		{
			WarningLabel.color = Colors.White;
		}
		else if (!WarningActive.activeInHierarchy && WarningLabel.color != Colors.Gray)
		{
			WarningLabel.color = Colors.Gray;
		}
	}

	public void CheckSystems()
	{
		bool value = false;
		foreach (SubSystem value2 in ParentPilot.ParentShip.SubSystems.Values)
		{
			if (value2.SecondaryStatus == SystemSecondaryStatus.Defective)
			{
				value = true;
			}
		}
		foreach (Generator value3 in ParentPilot.ParentShip.Generators.Values)
		{
			if (value3.SecondaryStatus == SystemSecondaryStatus.Defective)
			{
				value = true;
			}
		}
		System.Activate(value);
	}

	[CompilerGenerated]
	private static bool _003CUpdateSystemsInfo_003Em__0(SceneMachineryPartSlot m)
	{
		return m.Scope == MachineryPartSlotScope.Armor;
	}
}
