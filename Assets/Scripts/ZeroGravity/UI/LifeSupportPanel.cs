using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class LifeSupportPanel : AbstractPanelUI
	{
		[Title("LIFE SUPPORT UI")] public GameObject MainScreen;

		public GameObject InfoScreen;

		public GameObject BackButton;

		public Transform vesselListTransform;

		public GameObject VesselObject;

		public VesselObjectScript SelectedVessel;

		private static Dictionary<SpaceObjectVessel, VesselObjectScript> LSVessels =
			new Dictionary<SpaceObjectVessel, VesselObjectScript>();

		public GameObject SelectedVesselHeader;

		public GameObject NotConnected;

		public Text SelectedVesselName;

		public Image SelectedVesselIcon;

		public GameObject SelectedVesselTank;

		public Text SelectedVesselAirTankValue;

		public Image SelectedVesselAirTankFiller;

		public GameObject AuthorizationFail;

		public Text AirTankValue;

		public Image AirTankFiller;

		public float CurrentAirTank;

		public float MaxAirTank;

		public GameObject AirTankDanger;

		public Text TankStatus;

		public GameObject TankNotConnected;

		public Transform VesselObjectsTransform;

		public Transform GeneratorsTransform;

		public GameObject AirGeneratorObject;

		public GameObject AirFilterObject;

		private static Dictionary<Generator, LSGeneratorUI> LSGenerators = new Dictionary<Generator, LSGeneratorUI>();

		public Transform ConnectedListTransform;

		public GameObject ConnectedVesselObject;

		public GameObject RoomObject;

		private static Dictionary<SceneTriggerRoom, LSRoomsUI> LSRooms = new Dictionary<SceneTriggerRoom, LSRoomsUI>();

		private void Start()
		{
		}

		private void Update()
		{
			GetPowerStatus();
			UpdateAirTank();
			BackButton.SetActive(!MainScreen.activeInHierarchy);
		}

		public override void OnInteract()
		{
			base.OnInteract();
			RefreshLifeSupport();
			MainScreen.SetActive(value: true);
			InfoScreen.SetActive(value: false);
			base.gameObject.SetActive(value: true);
		}

		public override void OnDetach()
		{
			base.OnDetach();
			base.gameObject.SetActive(value: false);
		}

		public void RefreshLifeSupport()
		{
			LSVessels.Clear();
			LSGenerators.Clear();
			LSRooms.Clear();
			SelectedVessel = null;
			VesselObjectScript[] componentsInChildren =
				vesselListTransform.GetComponentsInChildren<VesselObjectScript>(includeInactive: true);
			foreach (VesselObjectScript vesselObjectScript in componentsInChildren)
			{
				DestroyImmediate(vesselObjectScript.gameObject);
			}

			Initialize();
			CreateVesselsAndGenerators();
			if (LSVessels.ContainsKey(ParentVessel))
			{
				SelectVessel(ParentVessel);
			}
			else if (LSVessels.Count > 0)
			{
				SelectVessel(LSVessels.Keys.First());
			}
			else
			{
				SelectVessel(null);
			}

			CreateConnectedVessels();
			UpdateAirTank();
			foreach (KeyValuePair<SceneTriggerRoom, LSRoomsUI> lSRoom in LSRooms)
			{
				lSRoom.Value.RefreshUI();
			}
		}

		private void CreateVesselsAndGenerators()
		{
			LSResourceTank[] componentsInChildren =
				VesselObjectsTransform.GetComponentsInChildren<LSResourceTank>(includeInactive: true);
			foreach (LSResourceTank lSResourceTank in componentsInChildren)
			{
				DestroyImmediate(lSResourceTank.gameObject);
			}

			LSGeneratorUI[] componentsInChildren2 =
				GeneratorsTransform.GetComponentsInChildren<LSGeneratorUI>(includeInactive: true);
			foreach (LSGeneratorUI lSGeneratorUI in componentsInChildren2)
			{
				DestroyImmediate(lSGeneratorUI.gameObject);
			}

			foreach (SpaceObjectVessel allVessel in AllVessels)
			{
				ILifeProvider[] componentsInChildren3 = allVessel.GeometryRoot.GetComponentsInChildren<ILifeProvider>();
				if (componentsInChildren3 != null && componentsInChildren3.Length > 0)
				{
					GameObject gameObject = Instantiate(VesselObject, vesselListTransform);
					gameObject.SetActive(value: true);
					gameObject.transform.Reset();
					VesselObjectScript component = gameObject.GetComponent<VesselObjectScript>();
					component.LifePanel = this;
					component.Vessel = allVessel;
					LSVessels[component.Vessel] = component;
				}
			}

			foreach (Generator lifeGenerator in LifeGenerators)
			{
				if (lifeGenerator.Type == GeneratorType.Air)
				{
					GameObject gameObject2 = Instantiate(AirGeneratorObject, GeneratorsTransform);
					gameObject2.SetActive(value: true);
					gameObject2.transform.Reset();
					LSGeneratorUI component2 = gameObject2.GetComponent<LSGeneratorUI>();
					component2.Generator = lifeGenerator;
					LSGenerators[lifeGenerator] = component2;
					UpdateGeneratorUI(component2);
					MakePartsForGenerator(component2);
				}
				else
				{
					GameObject gameObject3 = Instantiate(AirFilterObject, GeneratorsTransform);
					gameObject3.SetActive(value: true);
					gameObject3.transform.Reset();
					LSGeneratorUI component3 = gameObject3.GetComponent<LSGeneratorUI>();
					component3.Generator = lifeGenerator;
					LSGenerators[lifeGenerator] = component3;
					UpdateGeneratorUI(component3);
					MakePartsForGenerator(component3);
				}
			}
		}

		public void UpdateAirTank()
		{
			if (AirTanks.Count == 0)
			{
				TankNotConnected.SetActive(value: true);
				AirTankDanger.SetActive(value: false);
				return;
			}

			TankNotConnected.SetActive(value: false);
			float num = 0f;
			float num2 = 0f;
			foreach (ResourceContainer airTank in AirTanks)
			{
				num += airTank.Capacity;
				num2 += airTank.Quantity;
			}

			CurrentAirTank = num2;
			MaxAirTank = num;
			AirTankFiller.fillAmount = num2 / num;
			AirTankValue.text = FormatHelper.CurrentMax(CurrentAirTank, MaxAirTank);
			AirTankDanger.SetActive(CurrentAirTank == 0f || CurrentAirTank == MaxAirTank);
			if (CurrentAirTank == 0f)
			{
				TankStatus.text = Localization.AirTank.ToUpper() + " " + Localization.Empty.ToUpper() +
				                  " - <color=#AE1515>" + Localization.UnableToPressurize.ToUpper() + "</color>";
			}
			else if (CurrentAirTank == MaxAirTank)
			{
				TankStatus.text = Localization.AirTank.ToUpper() + " " + Localization.Full.ToUpper() +
				                  " - <color=#AE1515>" + Localization.UnableToDepressurize.ToUpper() + "</color>";
			}
			else
			{
				TankStatus.text = Localization.AirTank.ToUpper();
			}

			SelectedVesselAirTankValue.text = FormatHelper.CurrentMax(SelectedVessel.AirContainer.Quantity,
				SelectedVessel.AirContainer.Capacity);
			SelectedVesselAirTankFiller.fillAmount =
				SelectedVessel.AirContainer.Quantity / SelectedVessel.AirContainer.Capacity;
		}

		public void CreateConnectedVessels()
		{
			ConnectedVessel[] componentsInChildren =
				ConnectedListTransform.GetComponentsInChildren<ConnectedVessel>(includeInactive: true);
			foreach (ConnectedVessel connectedVessel in componentsInChildren)
			{
				DestroyImmediate(connectedVessel.gameObject);
			}

			foreach (SpaceObjectVessel allVessel in AllVessels)
			{
				GameObject gameObject = Instantiate(ConnectedVesselObject, ConnectedListTransform);
				gameObject.SetActive(value: true);
				gameObject.transform.Reset();
				ConnectedVessel component = gameObject.GetComponent<ConnectedVessel>();
				component.LifePanel = this;
				component.Vessel = allVessel;
				CreateRoom(component);
			}
		}

		private void CreateRoom(ConnectedVessel ConsumerVessel)
		{
			SceneTriggerRoom[] componentsInChildren =
				ConsumerVessel.Vessel.GeometryRoot.GetComponentsInChildren<SceneTriggerRoom>();
			float val = componentsInChildren.Sum((SceneTriggerRoom m) => m.Volume);
			SceneTriggerRoom[] array = componentsInChildren;
			foreach (SceneTriggerRoom sceneTriggerRoom in array)
			{
				if (!sceneTriggerRoom.RoomName.IsNullOrEmpty())
				{
					GameObject gameObject = Instantiate(RoomObject, ConsumerVessel.gameObject.transform);
					gameObject.SetActive(value: true);
					gameObject.transform.Reset();
					LSRoomsUI component = gameObject.GetComponent<LSRoomsUI>();
					component.Panel = this;
					component.Room = sceneTriggerRoom;
					component.Vessel = ConsumerVessel.Vessel;
					LSRooms[sceneTriggerRoom] = component;
					component.RefreshUI();
				}
			}

			ConsumerVessel.Volume.text = FormatHelper.FormatValue(val);
		}

		public void UpdateRoom(SceneTriggerRoom room)
		{
			if (LSRooms.TryGetValue(room, out var value) && !(value == null))
			{
				value.RefreshUI();
			}
		}

		public void UpdateVesselObjects(SpaceObjectVessel vessel)
		{
			if (LSVessels.TryGetValue(vessel, out var value))
			{
				if (value == null)
				{
					return;
				}

				value.LS_UpdateAirTank();
			}

			GetPowerStatus();
		}

		public void SelectVessel(SpaceObjectVessel vessel)
		{
			if (vessel == null)
			{
				NotConnected.SetActive(value: true);
				SelectedVesselTank.SetActive(value: false);
				SelectedVesselHeader.SetActive(value: false);
				return;
			}

			foreach (VesselObjectScript value in LSVessels.Values)
			{
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

			foreach (KeyValuePair<Generator, LSGeneratorUI> lSGenerator in LSGenerators)
			{
				lSGenerator.Value.gameObject.SetActive(lSGenerator.Key.ParentVessel == vessel);
			}

			if (SelectedVessel != null)
			{
				NotConnected.SetActive(value: false);
				SelectedVesselTank.SetActive(value: true);
				SelectedVesselHeader.SetActive(value: true);
				SelectedVesselName.text = SelectedVessel.Vessel.CustomName;
				SelectedVesselIcon.sprite = SpriteManager.Instance.GetSprite(SelectedVessel.Vessel);
				AuthorizationFail.SetActive(!SelectedVessel.IsAuthorized);
				SelectedVesselAirTankValue.text = FormatHelper.CurrentMax(SelectedVessel.AirContainer.Quantity,
					SelectedVessel.AirContainer.Capacity);
				SelectedVesselAirTankFiller.fillAmount =
					SelectedVessel.AirContainer.Quantity / SelectedVessel.AirContainer.Capacity;
			}
			else
			{
				NotConnected.SetActive(value: true);
				SelectedVesselTank.SetActive(value: false);
				SelectedVesselHeader.SetActive(value: false);
				AuthorizationFail.SetActive(value: false);
			}

			UpdateVesselObjects(vessel);
		}

		public void UpdateGenerator(Generator gen)
		{
			if (LSGenerators.TryGetValue(gen, out var value) && !(value == null))
			{
				UpdateGeneratorUI(value);
			}
		}

		private void UpdateGeneratorUI(LSGeneratorUI LG)
		{
			LG.Status.text = LG.Generator.GetStatus(out var color);
			LG.Status.color = color;
			LG.IsOnline.SetActive(LG.Generator.Status == SystemStatus.Online);
			if (LG.DisablePattern != null)
			{
				LG.DisablePattern.SetActive(LG.Generator.Status != SystemStatus.Online);
			}

			LG.ToggleText.text = ((!LG.Generator.IsSwitchedOn())
				? Localization.Powerup.ToUpper()
				: Localization.ShutDown.ToUpper());
			LG.Consumption.text = FormatHelper.FormatValue(LG.Generator.GetPowerConsumption(true));
			LG.Output.text = LG.Generator.MaxOutput.ToString();
			LG.UpdateContainers();
			UpdateVesselObjects(LG.Generator.ParentVessel);
		}

		private void MakePartsForGenerator(LSGeneratorUI generator)
		{
			SceneMachineryPartSlot[] machineryPartSlots = generator.Generator.MachineryPartSlots;
			foreach (SceneMachineryPartSlot partSlot in machineryPartSlots)
			{
				GameObject gameObject = Instantiate(generator.PartPref, generator.PartsTransform);
				gameObject.SetActive(value: true);
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

		public void ToggleInfoScreen()
		{
			MainScreen.SetActive(!MainScreen.activeInHierarchy);
			InfoScreen.SetActive(!InfoScreen.activeInHierarchy);
		}

		public void UpdateConnectedVesselsHealth()
		{
			ConnectedVessel[] componentsInChildren =
				ConnectedListTransform.GetComponentsInChildren<ConnectedVessel>(includeInactive: true);
			foreach (ConnectedVessel connectedVessel in componentsInChildren)
			{
				connectedVessel.HealthAndArmorUpdate();
			}
		}
	}
}
