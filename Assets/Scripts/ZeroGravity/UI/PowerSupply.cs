using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class PowerSupply : AbstractPanelUI
	{
		[Title("POWER SUPPLY UI")]
		public GameObject MainScreen;

		public GameObject InfoScreen;

		public GameObject BackButton;

		public Text GeneralPowerOutput;

		public Text GeneralPowerConsumption;

		public Transform vesselListTransform;

		public GameObject VesselObject;

		public VesselObjectScript SelectedVessel;

		private static Dictionary<SpaceObjectVessel, VesselObjectScript> POVessels = new Dictionary<SpaceObjectVessel, VesselObjectScript>();

		public GameObject SelectedVesselHeader;

		public GameObject NotConnected;

		public Text SelectedVesselName;

		public Text SelectedVesselPO;

		public Image SelectedVesselIcon;

		public GameObject AuthorizationFail;

		public Transform VesselObjectsTransform;

		public Transform PowerSourcesTransform;

		public GameObject PowerGeneratorObject;

		public GameObject SolarPanelObject;

		private static Dictionary<Generator, PowerSupplyPowerGenerator> powerGenerators = new Dictionary<Generator, PowerSupplyPowerGenerator>();

		public Transform ConsumerListTransform;

		public GameObject ConsumerVessel;

		public GameObject ConsumerItem;

		private static Dictionary<VesselComponent, ConnectedVessel> ConsumerVessels = new Dictionary<VesselComponent, ConnectedVessel>();

		public Dictionary<VesselComponent, PowerSupplyPowerConsumer> powerConsumers = new Dictionary<VesselComponent, PowerSupplyPowerConsumer>();

		public GameObject CapacitorItem;

		private static Dictionary<Generator, PowerSupplyCapacitor> powerCapacitors = new Dictionary<Generator, PowerSupplyCapacitor>();

		private void Start()
		{
		}

		private void Update()
		{
			GetGeneralStats();
			BackButton.SetActive(!MainScreen.activeInHierarchy);
		}

		public override void OnInteract()
		{
			base.OnInteract();
			RefreshPowerSupply();
			MainScreen.SetActive(true);
			InfoScreen.SetActive(false);
			base.gameObject.SetActive(true);
		}

		public override void OnDetach()
		{
			base.OnDetach();
			base.gameObject.SetActive(false);
		}

		public void ToggleInfoScreen()
		{
			MainScreen.SetActive(!MainScreen.activeInHierarchy);
			InfoScreen.SetActive(!InfoScreen.activeInHierarchy);
		}

		public void RefreshPowerSupply()
		{
			POVessels.Clear();
			ConsumerVessels.Clear();
			powerGenerators.Clear();
			powerCapacitors.Clear();
			powerConsumers.Clear();
			SelectedVessel = null;
			vesselListTransform.DestroyAll<VesselObjectScript>();
			ConsumerListTransform.DestroyAll<ConnectedVessel>();
			Initialize();
			CreateVesselsAndGenerators();
			MakeConsumerList();
			if (POVessels.ContainsKey(ParentVessel))
			{
				SelectVessel(ParentVessel);
				UpdateVesselObjects(ParentVessel);
			}
			else if (POVessels.Count > 0)
			{
				SelectVessel(POVessels.Keys.First());
				UpdateVesselObjects(POVessels.Keys.First());
			}
			else
			{
				SelectedVessel = null;
				NotConnected.SetActive(true);
				SelectedVesselHeader.SetActive(false);
			}
		}

		private void CreateVesselsAndGenerators()
		{
			PowerSourcesTransform.DestroyAll<PowerSupplyPowerGenerator>();
			VesselObjectsTransform.DestroyAll<PowerSupplyCapacitor>();
			foreach (SpaceObjectVessel allVessel in AllVessels)
			{
				IPowerProvider[] componentsInChildren = allVessel.GeometryRoot.GetComponentsInChildren<IPowerProvider>();
				if (componentsInChildren != null && componentsInChildren.Length > 0)
				{
					GameObject gameObject = Object.Instantiate(VesselObject, vesselListTransform);
					gameObject.SetActive(true);
					gameObject.transform.Reset();
					VesselObjectScript component = gameObject.GetComponent<VesselObjectScript>();
					component.PowerPanel = this;
					component.Vessel = allVessel;
					POVessels[component.Vessel] = component;
				}
			}
			foreach (GeneratorCapacitor capacitor in Capacitors)
			{
				GameObject gameObject2 = Object.Instantiate(CapacitorItem, VesselObjectsTransform);
				gameObject2.transform.SetAsFirstSibling();
				gameObject2.SetActive(true);
				gameObject2.transform.Reset();
				PowerSupplyCapacitor component2 = gameObject2.GetComponent<PowerSupplyCapacitor>();
				component2.Capacitor = capacitor;
				powerCapacitors[capacitor] = component2;
				UpdateCapacitorUi(component2);
				MakePartsForCapacitor(component2);
			}
			foreach (GeneratorPower powerGenerator in PowerGenerators)
			{
				GameObject gameObject3 = Object.Instantiate(PowerGeneratorObject, PowerSourcesTransform);
				gameObject3.SetActive(true);
				gameObject3.transform.Reset();
				PowerSupplyPowerGenerator component3 = gameObject3.GetComponent<PowerSupplyPowerGenerator>();
				component3.Generator = powerGenerator;
				component3.Container = powerGenerator.ResourceContainers[0];
				powerGenerators[powerGenerator] = component3;
				UpdateGeneratorUI(component3);
				MakePartsForGenerator(component3);
			}
			foreach (GeneratorSolar solarGenerator in SolarGenerators)
			{
				GameObject gameObject4 = Object.Instantiate(SolarPanelObject, PowerSourcesTransform);
				gameObject4.SetActive(true);
				gameObject4.transform.Reset();
				PowerSupplyPowerGenerator component4 = gameObject4.GetComponent<PowerSupplyPowerGenerator>();
				component4.Generator = solarGenerator;
				powerGenerators[solarGenerator] = component4;
				UpdateGeneratorUI(component4);
				MakePartsForGenerator(component4);
			}
		}

		public void SelectVessel(SpaceObjectVessel vessel)
		{
			if (vessel == null)
			{
				NotConnected.SetActive(true);
				SelectedVesselHeader.SetActive(false);
				return;
			}
			foreach (VesselObjectScript value in POVessels.Values)
			{
				if (vessel == value.Vessel)
				{
					SelectedVessel = value;
					value.Selected.SetActive(true);
				}
				else
				{
					value.Selected.SetActive(false);
				}
			}
			foreach (KeyValuePair<Generator, PowerSupplyPowerGenerator> powerGenerator in powerGenerators)
			{
				powerGenerator.Value.gameObject.SetActive(powerGenerator.Key.ParentVessel == vessel);
			}
			foreach (KeyValuePair<Generator, PowerSupplyCapacitor> powerCapacitor in powerCapacitors)
			{
				powerCapacitor.Value.gameObject.SetActive(powerCapacitor.Key.ParentVessel == vessel);
			}
			if (SelectedVessel != null)
			{
				NotConnected.SetActive(false);
				SelectedVesselHeader.SetActive(true);
				SelectedVesselName.text = SelectedVessel.Vessel.CustomName;
				SelectedVesselIcon.sprite = Client.Instance.SpriteManager.GetSprite(SelectedVessel.Vessel);
				SelectedVesselPO.text = SelectedVessel.PS_Output.text;
				AuthorizationFail.SetActive(!SelectedVessel.IsAuthorized);
			}
			else
			{
				NotConnected.SetActive(true);
				SelectedVesselHeader.SetActive(false);
				AuthorizationFail.SetActive(false);
			}
		}

		public void UpdateVesselObjects(SpaceObjectVessel vessel)
		{
			float num = 0f;
			foreach (KeyValuePair<Generator, PowerSupplyPowerGenerator> powerGenerator in powerGenerators)
			{
				if (powerGenerator.Key.ParentVessel == vessel && powerGenerator.Value.Generator.Status == SystemStatus.OnLine)
				{
					num += powerGenerator.Value.Generator.MaxOutput;
				}
			}
			bool active = false;
			float fillAmount = 0f;
			foreach (KeyValuePair<Generator, PowerSupplyCapacitor> powerCapacitor in powerCapacitors)
			{
				if (powerCapacitor.Key.ParentVessel == vessel)
				{
					fillAmount = powerCapacitor.Value.Filler.fillAmount;
					active = true;
				}
			}
			VesselObjectScript value;
			if (POVessels.TryGetValue(vessel, out value))
			{
				if (value == null)
				{
					return;
				}
				value.PS_Output.text = FormatHelper.FormatValue(num);
				value.CapacitorHolder.SetActive(active);
				value.CapacitorFiller.fillAmount = fillAmount;
				if (SelectedVessel == value)
				{
					SelectedVesselPO.text = FormatHelper.FormatValue(num, true);
				}
			}
			GetGeneralStats();
		}

		public void MakeConsumerList()
		{
			PowerSupplyPowerConsumer[] componentsInChildren = ConsumerListTransform.GetComponentsInChildren<PowerSupplyPowerConsumer>(true);
			foreach (PowerSupplyPowerConsumer powerSupplyPowerConsumer in componentsInChildren)
			{
				Object.DestroyImmediate(powerSupplyPowerConsumer.gameObject);
			}
			foreach (SpaceObjectVessel allVessel in AllVessels)
			{
				GameObject gameObject = Object.Instantiate(ConsumerVessel, ConsumerListTransform);
				gameObject.SetActive(true);
				gameObject.transform.Reset();
				ConnectedVessel component = gameObject.GetComponent<ConnectedVessel>();
				component.PowerPanel = this;
				component.Vessel = allVessel;
				Ship ship = component.Vessel as Ship;
				component.Base = ship.VesselBaseSystem;
				component.BaseConsumption.text = FormatHelper.FormatValue(component.Base.GetPowerConsumption(true));
				component.Vc = component.Base;
				ConsumerVessels[component.Vc] = component;
				MakeConsumerForVessel(component);
				UpdateShipConsumptionUi(component);
			}
		}

		public void UpdateVesselConsumers(SpaceObjectVessel vc)
		{
			foreach (ConnectedVessel value in ConsumerVessels.Values)
			{
				if (value.Vessel == vc)
				{
					UpdateShipConsumptionUi(value);
				}
			}
		}

		private void UpdateShipConsumptionUi(ConnectedVessel vesselInfo)
		{
			float num = 0f;
			IPowerConsumer[] componentsInChildren = vesselInfo.Vessel.GeometryRoot.GetComponentsInChildren<IPowerConsumer>();
			foreach (IPowerConsumer powerConsumer in componentsInChildren)
			{
				if (powerConsumer is GeneratorCapacitor)
				{
					continue;
				}
				num += powerConsumer.GetPowerConsumption();
				PowerSupplyPowerConsumer value;
				if (powerConsumers.TryGetValue(powerConsumer as VesselComponent, out value))
				{
					if (value == null)
					{
						return;
					}
					UpdatePowerConsumers(value);
				}
			}
			if (vesselInfo.Base.Status == SystemStatus.OnLine || vesselInfo.Base.SecondaryStatus == SystemSecondaryStatus.Malfunction)
			{
				vesselInfo.ToggleStatus.transform.localPosition = new Vector3(0f, 10f, 0f);
				vesselInfo.IsOnline.SetActive(vesselInfo.Base.SecondaryStatus == SystemSecondaryStatus.Malfunction);
				vesselInfo.Consumption.text = FormatHelper.FormatValue(num);
			}
			else
			{
				vesselInfo.ToggleStatus.transform.localPosition = new Vector3(0f, -10f, 0f);
				vesselInfo.IsOnline.SetActive(true);
				vesselInfo.Consumption.text = "0";
			}
			GetGeneralStats();
		}

		public void UpdateCapacitor(Generator gen)
		{
			PowerSupplyCapacitor value;
			if (powerCapacitors.TryGetValue(gen, out value) && !(value == null))
			{
				UpdateCapacitorUi(value);
			}
		}

		private void UpdateCapacitorUi(PowerSupplyCapacitor cap)
		{
			cap.Value.text = FormatHelper.CurrentMax(cap.Capacitor.Capacity, cap.Capacitor.MaxCapacity);
			cap.Filler.fillAmount = cap.Capacitor.Capacity / cap.Capacitor.MaxCapacity;
			UpdateVesselObjects(cap.Capacitor.ParentVessel);
		}

		private void MakePartsForCapacitor(PowerSupplyCapacitor cap)
		{
			SceneMachineryPartSlot[] machineryPartSlots = cap.Capacitor.MachineryPartSlots;
			foreach (SceneMachineryPartSlot partSlot in machineryPartSlots)
			{
				GameObject gameObject = Object.Instantiate(cap.PartPref, cap.PartsTransform);
				gameObject.SetActive(true);
				gameObject.transform.Reset();
				PartSlotUI component = gameObject.GetComponent<PartSlotUI>();
				component.Panel = gameObject;
				cap.PowerPartsUI.Add(component);
				component.PartSlot = partSlot;
			}
			foreach (PartSlotUI item in cap.PowerPartsUI)
			{
				item.UpdateUI();
			}
		}

		public void UpdateGenerator(ResourceContainer container)
		{
			foreach (PowerSupplyPowerGenerator value in powerGenerators.Values)
			{
				if (value.Container == container)
				{
					UpdateGeneratorUI(value);
				}
			}
		}

		public void UpdateGenerator(Generator gen)
		{
			PowerSupplyPowerGenerator value;
			if (powerGenerators.TryGetValue(gen, out value) && !(value == null))
			{
				UpdateGeneratorUI(value);
			}
		}

		private void UpdateGeneratorUI(PowerSupplyPowerGenerator PG)
		{
			Color color;
			PG.Status.text = PG.Generator.GetStatus(out color);
			PG.Status.color = color;
			PG.IsOnline.SetActive(PG.Generator.Status == SystemStatus.OnLine);
			if (PG.DisablePattern != null)
			{
				PG.DisablePattern.SetActive(PG.Generator.Status != SystemStatus.OnLine);
			}
			if (PG.Generator.Type == GeneratorType.Power)
			{
				PG.PowerOutput.text = FormatHelper.FormatValue(PG.Generator.MaxOutput, true);
				PG.Consumption.text = FormatHelper.PerHour(PG.Generator.GetResourceConsumption(DistributionSystemType.Helium3, true));
				PG.Filler.fillAmount = PG.Container.CargoCompartment.Resources[0].Quantity / PG.Container.Capacity;
				PG.Capacity.text = FormatHelper.CurrentMax(PG.Container.CargoCompartment.Resources[0].Quantity, PG.Container.Capacity);
				PG.ToggleText.text = ((!PG.Generator.IsSwitchedOn()) ? Localization.Powerup.ToUpper() : Localization.ShutDown.ToUpper());
			}
			else if (PG.Generator.Type == GeneratorType.Solar)
			{
				GeneratorSolar generatorSolar = PG.Generator as GeneratorSolar;
				PG.Filler.fillAmount = generatorSolar.ExposureToSunlight;
				PG.Capacity.text = FormatHelper.Percentage(generatorSolar.ExposureToSunlight);
				PG.PowerOutput.text = FormatHelper.FormatValue(PG.Generator.MaxOutput, true);
				PG.ToggleText.text = ((!PG.Generator.IsSwitchedOn()) ? Localization.Deploy.ToUpper() : Localization.Retract.ToUpper());
				PG.Warning.SetActive(generatorSolar.ExposureToSunlight <= float.Epsilon);
			}
			UpdateVesselObjects(PG.Generator.ParentVessel);
		}

		private void MakePartsForGenerator(PowerSupplyPowerGenerator generator)
		{
			SceneMachineryPartSlot[] machineryPartSlots = generator.Generator.MachineryPartSlots;
			foreach (SceneMachineryPartSlot partSlot in machineryPartSlots)
			{
				GameObject gameObject = Object.Instantiate(generator.PartPref, generator.PartsTransform);
				gameObject.SetActive(true);
				gameObject.transform.Reset();
				PartSlotUI component = gameObject.GetComponent<PartSlotUI>();
				component.Panel = gameObject;
				generator.PowerPartsUI.Add(component);
				component.PartSlot = partSlot;
			}
			foreach (PartSlotUI item in generator.PowerPartsUI)
			{
				item.UpdateUI();
			}
		}

		public void RefreshParts(ref List<PartSlotUI> genParts)
		{
			foreach (PartSlotUI genPart in genParts)
			{
				genPart.UpdateUI();
			}
		}

		private void MakeConsumerForVessel(ConnectedVessel ConsumerVessel)
		{
			foreach (VesselSystem consumer in Consumers)
			{
				if (consumer.ParentVessel == ConsumerVessel.Vessel)
				{
					GameObject gameObject = Object.Instantiate(ConsumerItem, ConsumerVessel.gameObject.transform);
					gameObject.SetActive(true);
					gameObject.transform.Reset();
					PowerSupplyPowerConsumer component = gameObject.GetComponent<PowerSupplyPowerConsumer>();
					component.VesselSystem = consumer;
					UpdatePowerConsumers(component);
					powerConsumers[component.VesselSystem] = component;
				}
			}
		}

		private void UpdatePowerConsumers(PowerSupplyPowerConsumer powCon)
		{
			if (powCon.VesselSystem is SubSystem)
			{
				SubSystem subSystem = powCon.VesselSystem as SubSystem;
				powCon.Name.text = subSystem.Type.ToLocalizedString().ToUpper();
				powCon.SetPowerConsumptionText(subSystem.GetPowerConsumption(true));
			}
			else
			{
				Generator generator = powCon.VesselSystem as Generator;
				powCon.Name.text = generator.Type.ToLocalizedString().ToUpper();
				powCon.SetPowerConsumptionText(generator.GetPowerConsumption(true));
			}
			Color color;
			powCon.Status.text = powCon.VesselSystem.GetStatus(out color);
			powCon.Status.color = color;
		}

		public void GetGeneralStats()
		{
			GetPowerStatus();
			GeneralPowerOutput.text = FormatHelper.FormatValue(generalOutput, true);
			GeneralPowerConsumption.text = FormatHelper.FormatValue(generalConsumption, true);
		}
	}
}
