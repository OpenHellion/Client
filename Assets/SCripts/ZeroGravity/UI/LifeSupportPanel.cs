using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
		[Header("LIFE SUPPORT UI")]
		public GameObject MainScreen;

		public GameObject InfoScreen;

		public GameObject BackButton;

		public Transform vesselListTransform;

		public GameObject VesselObject;

		public VesselObjectScript SelectedVessel;

		private static Dictionary<SpaceObjectVessel, VesselObjectScript> LSVessels = new Dictionary<SpaceObjectVessel, VesselObjectScript>();

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

		[CompilerGenerated]
		private static Func<SceneTriggerRoom, float> _003C_003Ef__am_0024cache0;

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
			MainScreen.SetActive(true);
			InfoScreen.SetActive(false);
			base.gameObject.SetActive(true);
		}

		public override void OnDetach()
		{
			base.OnDetach();
			base.gameObject.SetActive(false);
		}

		public void RefreshLifeSupport()
		{
			LSVessels.Clear();
			LSGenerators.Clear();
			LSRooms.Clear();
			SelectedVessel = null;
			VesselObjectScript[] componentsInChildren = vesselListTransform.GetComponentsInChildren<VesselObjectScript>(true);
			foreach (VesselObjectScript vesselObjectScript in componentsInChildren)
			{
				UnityEngine.Object.DestroyImmediate(vesselObjectScript.gameObject);
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
			LSResourceTank[] componentsInChildren = VesselObjectsTransform.GetComponentsInChildren<LSResourceTank>(true);
			foreach (LSResourceTank lSResourceTank in componentsInChildren)
			{
				UnityEngine.Object.DestroyImmediate(lSResourceTank.gameObject);
			}
			LSGeneratorUI[] componentsInChildren2 = GeneratorsTransform.GetComponentsInChildren<LSGeneratorUI>(true);
			foreach (LSGeneratorUI lSGeneratorUI in componentsInChildren2)
			{
				UnityEngine.Object.DestroyImmediate(lSGeneratorUI.gameObject);
			}
			foreach (SpaceObjectVessel allVessel in AllVessels)
			{
				ILifeProvider[] componentsInChildren3 = allVessel.GeometryRoot.GetComponentsInChildren<ILifeProvider>();
				if (componentsInChildren3 != null && componentsInChildren3.Length > 0)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(VesselObject, vesselListTransform);
					gameObject.SetActive(true);
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
					GameObject gameObject2 = UnityEngine.Object.Instantiate(AirGeneratorObject, GeneratorsTransform);
					gameObject2.SetActive(true);
					gameObject2.transform.Reset();
					LSGeneratorUI component2 = gameObject2.GetComponent<LSGeneratorUI>();
					component2.Generator = lifeGenerator;
					LSGenerators[lifeGenerator] = component2;
					UpdateGeneratorUI(component2);
					MakePartsForGenerator(component2);
				}
				else
				{
					GameObject gameObject3 = UnityEngine.Object.Instantiate(AirFilterObject, GeneratorsTransform);
					gameObject3.SetActive(true);
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
				TankNotConnected.SetActive(true);
				AirTankDanger.SetActive(false);
				return;
			}
			TankNotConnected.SetActive(false);
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
				TankStatus.text = Localization.AirTank.ToUpper() + " " + Localization.Empty.ToUpper() + " - <color=#AE1515>" + Localization.UnableToPressurize.ToUpper() + "</color>";
			}
			else if (CurrentAirTank == MaxAirTank)
			{
				TankStatus.text = Localization.AirTank.ToUpper() + " " + Localization.Full.ToUpper() + " - <color=#AE1515>" + Localization.UnableToDepressurize.ToUpper() + "</color>";
			}
			else
			{
				TankStatus.text = Localization.AirTank.ToUpper();
			}
			SelectedVesselAirTankValue.text = FormatHelper.CurrentMax(SelectedVessel.AirContainer.Quantity, SelectedVessel.AirContainer.Capacity);
			SelectedVesselAirTankFiller.fillAmount = SelectedVessel.AirContainer.Quantity / SelectedVessel.AirContainer.Capacity;
		}

		public void CreateConnectedVessels()
		{
			ConnectedVessel[] componentsInChildren = ConnectedListTransform.GetComponentsInChildren<ConnectedVessel>(true);
			foreach (ConnectedVessel connectedVessel in componentsInChildren)
			{
				UnityEngine.Object.DestroyImmediate(connectedVessel.gameObject);
			}
			foreach (SpaceObjectVessel allVessel in AllVessels)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(ConnectedVesselObject, ConnectedListTransform);
				gameObject.SetActive(true);
				gameObject.transform.Reset();
				ConnectedVessel component = gameObject.GetComponent<ConnectedVessel>();
				component.LifePanel = this;
				component.Vessel = allVessel;
				CreateRoom(component);
			}
		}

		private void CreateRoom(ConnectedVessel ConsumerVessel)
		{
			SceneTriggerRoom[] componentsInChildren = ConsumerVessel.Vessel.GeometryRoot.GetComponentsInChildren<SceneTriggerRoom>();
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CCreateRoom_003Em__0;
			}
			float val = componentsInChildren.Sum(_003C_003Ef__am_0024cache0);
			SceneTriggerRoom[] array = componentsInChildren;
			foreach (SceneTriggerRoom sceneTriggerRoom in array)
			{
				if (!sceneTriggerRoom.RoomName.IsNullOrEmpty())
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(RoomObject, ConsumerVessel.gameObject.transform);
					gameObject.SetActive(true);
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
			LSRoomsUI value;
			if (LSRooms.TryGetValue(room, out value) && !(value == null))
			{
				value.RefreshUI();
			}
		}

		public void UpdateVesselObjects(SpaceObjectVessel vessel)
		{
			VesselObjectScript value;
			if (LSVessels.TryGetValue(vessel, out value))
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
				NotConnected.SetActive(true);
				SelectedVesselTank.SetActive(false);
				SelectedVesselHeader.SetActive(false);
				return;
			}
			foreach (VesselObjectScript value in LSVessels.Values)
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
			foreach (KeyValuePair<Generator, LSGeneratorUI> lSGenerator in LSGenerators)
			{
				lSGenerator.Value.gameObject.SetActive(lSGenerator.Key.ParentVessel == vessel);
			}
			if (SelectedVessel != null)
			{
				NotConnected.SetActive(false);
				SelectedVesselTank.SetActive(true);
				SelectedVesselHeader.SetActive(true);
				SelectedVesselName.text = SelectedVessel.Vessel.CustomName;
				SelectedVesselIcon.sprite = Client.Instance.SpriteManager.GetSprite(SelectedVessel.Vessel);
				AuthorizationFail.SetActive(!SelectedVessel.IsAuthorized);
				SelectedVesselAirTankValue.text = FormatHelper.CurrentMax(SelectedVessel.AirContainer.Quantity, SelectedVessel.AirContainer.Capacity);
				SelectedVesselAirTankFiller.fillAmount = SelectedVessel.AirContainer.Quantity / SelectedVessel.AirContainer.Capacity;
			}
			else
			{
				NotConnected.SetActive(true);
				SelectedVesselTank.SetActive(false);
				SelectedVesselHeader.SetActive(false);
				AuthorizationFail.SetActive(false);
			}
			UpdateVesselObjects(vessel);
		}

		public void UpdateGenerator(Generator gen)
		{
			LSGeneratorUI value;
			if (LSGenerators.TryGetValue(gen, out value) && !(value == null))
			{
				UpdateGeneratorUI(value);
			}
		}

		private void UpdateGeneratorUI(LSGeneratorUI LG)
		{
			Color color;
			LG.Status.text = LG.Generator.GetStatus(out color);
			LG.Status.color = color;
			LG.IsOnline.SetActive(LG.Generator.Status == SystemStatus.OnLine);
			if (LG.DisablePattern != null)
			{
				LG.DisablePattern.SetActive(LG.Generator.Status != SystemStatus.OnLine);
			}
			LG.ToggleText.text = ((!LG.Generator.IsSwitchedOn()) ? Localization.Powerup.ToUpper() : Localization.ShutDown.ToUpper());
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
				GameObject gameObject = UnityEngine.Object.Instantiate(generator.PartPref, generator.PartsTransform);
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

		public void ToggleInfoScreen()
		{
			MainScreen.SetActive(!MainScreen.activeInHierarchy);
			InfoScreen.SetActive(!InfoScreen.activeInHierarchy);
		}

		public void UpdateConnectedVesselsHealth()
		{
			ConnectedVessel[] componentsInChildren = ConnectedListTransform.GetComponentsInChildren<ConnectedVessel>(true);
			foreach (ConnectedVessel connectedVessel in componentsInChildren)
			{
				connectedVessel.HealthAndArmorUpdate();
			}
		}

		[CompilerGenerated]
		private static float _003CCreateRoom_003Em__0(SceneTriggerRoom m)
		{
			return m.Volume;
		}
	}
}
