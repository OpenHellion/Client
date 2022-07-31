using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class VesselObjectScript : MonoBehaviour, IDropHandler, IEventSystemHandler
	{
		[NonSerialized]
		public SpaceObjectVessel Vessel;

		public Image Icon;

		public GameObject Selected;

		[NonSerialized]
		public PowerSupply PowerPanel;

		[Header("POWER SUPPLY")]
		public Text PS_Output;

		public GameObject CapacitorHolder;

		public Image CapacitorFiller;

		[NonSerialized]
		public LifeSupportPanel LifePanel;

		[Header("LIFE SUPPORT")]
		public Image AirTankFiller;

		[NonSerialized]
		public ResourceContainer AirContainer;

		[NonSerialized]
		public CargoPanel MyCargoPanel;

		[Header("CARGO")]
		public SceneCargoBay Cargo;

		public Image CapacityFiller;

		public bool IsAuthorized
		{
			get
			{
				return Vessel.IsPlayerAuthorizedOrNoSecurity(MyPlayer.Instance);
			}
		}

		public void OnDrop(PointerEventData eventData)
		{
			if (MyCargoPanel != null)
			{
				CargoTransferingResource component = MyCargoPanel.DragingItem.GetComponent<CargoTransferingResource>();
				component.ToCompartment = Cargo.CargoBayResources;
				MyCargoPanel.SetTransferBox();
			}
		}

		private void Start()
		{
			Icon.sprite = Client.Instance.SpriteManager.GetSprite(Vessel);
		}

		public void LS_UpdateAirTank()
		{
			foreach (KeyValuePair<int, ResourceContainer> resourceContainer in Vessel.ResourceContainers)
			{
				if (resourceContainer.Value.DistributionSystemType == DistributionSystemType.Air)
				{
					AirContainer = resourceContainer.Value;
				}
			}
			AirTankFiller.fillAmount = AirContainer.Quantity / AirContainer.Capacity;
		}

		public void PS_SelectVessel()
		{
			PowerPanel.SelectVessel(Vessel);
		}

		public void LS_SelectVessel()
		{
			LifePanel.SelectVessel(Vessel);
		}

		public void Cargo_SelectVessel()
		{
			MyCargoPanel.SelectVessel(Vessel);
		}

		public float CargoCapacityFillerValue()
		{
			return (Cargo.CargoBayResources.Capacity - Cargo.CargoBayResources.AvailableCapacity) / Cargo.CargoBayResources.Capacity;
		}

		public string CargoCapacityText()
		{
			return FormatHelper.CurrentMax(Cargo.CargoBayResources.Capacity - Cargo.CargoBayResources.AvailableCapacity, Cargo.CargoBayResources.Capacity);
		}
	}
}
