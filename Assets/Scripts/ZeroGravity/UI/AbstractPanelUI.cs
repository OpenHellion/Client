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
	public abstract class AbstractPanelUI : MonoBehaviour
	{
		protected SpaceObjectVessel ParentVessel;

		protected List<SpaceObjectVessel> AllVessels = new List<SpaceObjectVessel>();

		protected List<GeneratorPower> PowerGenerators = new List<GeneratorPower>();

		protected List<GeneratorSolar> SolarGenerators = new List<GeneratorSolar>();

		protected List<Generator> LifeGenerators = new List<Generator>();

		protected List<VesselComponent> Consumers = new List<VesselComponent>();

		protected List<GeneratorCapacitor> Capacitors = new List<GeneratorCapacitor>();

		protected List<ResourceContainer> AirTanks = new List<ResourceContainer>();

		protected List<SceneCargoBay> Cargos = new List<SceneCargoBay>();

		protected float generalOutput;

		protected float generalConsumption;

		protected float curCap;

		protected float maxCap;

		protected float powDiff;

		public Text GeneralCapacity;

		public Image GeneralCapacityFiller;

		public Text PowerDiff;

		public virtual void OnInteract()
		{
			Client.Instance.CanvasManager.QuickTipHolder.Activate(value: false);
		}

		public virtual void OnDetach()
		{
		}

		public void FillAllVessels()
		{
			AllVessels.Clear();
			if ((object)MyPlayer.Instance.CurrentRoomTrigger.ParentVessel != null)
			{
				ParentVessel = MyPlayer.Instance.CurrentRoomTrigger.ParentVessel;
			}
			AllVessels.Add(ParentVessel.MainVessel);
			foreach (SpaceObjectVessel allDockedVessel in ParentVessel.MainVessel.AllDockedVessels)
			{
				AllVessels.Add(allDockedVessel);
			}
		}

		public void Initialize()
		{
			Cargos.Clear();
			AirTanks.Clear();
			PowerGenerators.Clear();
			SolarGenerators.Clear();
			LifeGenerators.Clear();
			Consumers.Clear();
			Capacitors.Clear();
			FillAllVessels();
			foreach (SpaceObjectVessel allVessel in AllVessels)
			{
				IPowerProvider[] componentsInChildren = allVessel.GeometryRoot.GetComponentsInChildren<IPowerProvider>();
				if (componentsInChildren != null && componentsInChildren.Length > 0)
				{
					foreach (GeneratorCapacitor item7 in componentsInChildren.Where((IPowerProvider m) => m is GeneratorCapacitor))
					{
						Capacitors.Add(item7);
					}
					foreach (GeneratorPower item8 in componentsInChildren.Where((IPowerProvider m) => m is GeneratorPower))
					{
						PowerGenerators.Add(item8);
					}
					foreach (GeneratorSolar item9 in componentsInChildren.Where((IPowerProvider m) => m is GeneratorSolar))
					{
						SolarGenerators.Add(item9);
					}
				}
				ILifeProvider[] componentsInChildren2 = allVessel.GeometryRoot.GetComponentsInChildren<ILifeProvider>();
				if (componentsInChildren2 != null && componentsInChildren2.Length > 0)
				{
					ILifeProvider[] array = componentsInChildren2;
					for (int i = 0; i < array.Length; i++)
					{
						Generator item4 = (Generator)array[i];
						LifeGenerators.Add(item4);
					}
				}
				foreach (IPowerConsumer item10 in from m in allVessel.GeometryRoot.GetComponentsInChildren<IPowerConsumer>()
					where !(m is VesselBaseSystem)
					select m)
				{
					if (!(item10 is GeneratorCapacitor))
					{
						VesselComponent item5 = item10 as VesselComponent;
						Consumers.Add(item5);
					}
				}
				foreach (ResourceContainer item11 in from m in allVessel.GeometryRoot.GetComponentsInChildren<ResourceContainer>()
					where m.DistributionSystemType == DistributionSystemType.Air
					select m)
				{
					AirTanks.Add(item11);
				}
				SceneCargoBay[] componentsInChildren3 = allVessel.GeometryRoot.GetComponentsInChildren<SceneCargoBay>();
				foreach (SceneCargoBay item6 in componentsInChildren3)
				{
					Cargos.Add(item6);
				}
			}
			GetPowerStatus();
		}

		public void GetPowerStatus()
		{
			generalOutput = 0f;
			generalConsumption = 0f;
			curCap = 0f;
			maxCap = 0f;
			powDiff = 0f;
			foreach (GeneratorPower powerGenerator in PowerGenerators)
			{
				if (powerGenerator.Status == SystemStatus.Online)
				{
					generalOutput += powerGenerator.MaxOutput;
				}
			}
			foreach (GeneratorSolar solarGenerator in SolarGenerators)
			{
				if (solarGenerator.Status == SystemStatus.Online)
				{
					generalOutput += solarGenerator.MaxOutput;
				}
			}
			foreach (GeneratorCapacitor capacitor in Capacitors)
			{
				curCap += capacitor.Capacity;
				maxCap += capacitor.MaxCapacity;
			}
			foreach (VesselSystem consumer in Consumers)
			{
				if (consumer.Status == SystemStatus.Online)
				{
					generalConsumption += (consumer as IPowerConsumer).GetPowerConsumption();
				}
			}
			foreach (SpaceObjectVessel allVessel in AllVessels)
			{
				Ship ship = allVessel as Ship;
				if (ship.VesselBaseSystem.IsSwitchedOn() && ship.VesselBaseSystem.SecondaryStatus != SystemSecondaryStatus.Malfunction)
				{
					generalConsumption += ship.VesselBaseSystem.GetPowerConsumption();
				}
			}
			if (GeneralCapacityFiller != null)
			{
				GeneralCapacityFiller.fillAmount = curCap / maxCap;
			}
			powDiff = generalOutput - generalConsumption;
			if (GeneralCapacity != null && PowerDiff != null)
			{
				GeneralCapacity.text = FormatHelper.CurrentMax(curCap, maxCap);
				if (powDiff >= 0f)
				{
					PowerDiff.color = Colors.Green;
					PowerDiff.text = "+" + FormatHelper.FormatValue(powDiff, round: true);
				}
				else
				{
					PowerDiff.color = Colors.PowerRed;
					PowerDiff.text = FormatHelper.FormatValue(powDiff, round: true);
				}
			}
		}

		public void PanelExit()
		{
			base.gameObject.SetActive(value: false);
			MyPlayer.Instance.DetachFromPanel();
		}
	}
}
