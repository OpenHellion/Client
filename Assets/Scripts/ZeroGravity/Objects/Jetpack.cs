using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class Jetpack : Item, ICargo
	{
		[Serializable]
		public class Nozzle
		{
			public ParticleSystem NozzleEffect;

			public List<NozzleDirection> directions;

			[HideInInspector] public GameObject nozzle;

			public void ToggleNozzle(bool activate)
			{
				if (activate)
				{
					NozzleEffect.Play();
				}
				else
				{
					NozzleEffect.Stop();
				}
			}
		}

		public enum NozzleDirection
		{
			Forward = 1,
			Backwards = 2,
			Left = 3,
			Right = 4,
			Up = 5,
			Down = 6,
			LeanRight = 7,
			LeanLeft = 8
		}

		public bool IsActive;

		public GameObject nozzlesEffect;

		private List<ICargoCompartment> _Compartments;

		private Helmet helmet;

		[HideInInspector] public sbyte[] NozzleDir = new sbyte[4];

		public CargoCompartment OxygenCompartment;

		public CargoCompartment PropellantCompartment;

		public float OxygenConsumption = 0.008f;

		public float PropellantConsumption = 0.005f;

		public float MaxAngularVelocity = 20f;

		public bool OldJetpackActiveState;

		public SoundEffect JetpackSounds;

		public SoundEffect MyJetpackSounds;

		[SerializeField] private List<Nozzle> nozzles;

		public Image FuelFillerImage;

		public GameObject FuelLow;

		public Image OxyImageFiller;

		public GameObject OxyLow;

		public float CurrentFuel
		{
			get
			{
				return (PropellantCompartment.Resources == null || PropellantCompartment.Resources.Count <= 0)
					? 0f
					: PropellantCompartment.Resources[0].Quantity;
			}
		}

		public float MaxFuel
		{
			get { return PropellantCompartment.Capacity; }
		}

		public float CurrentOxygen
		{
			get
			{
				return (OxygenCompartment.Resources == null || OxygenCompartment.Resources.Count <= 0)
					? 0f
					: OxygenCompartment.Resources[0].Quantity;
			}
		}

		public float MaxOxygen
		{
			get { return OxygenCompartment.Capacity; }
		}

		public float PropFuel
		{
			get { return CurrentFuel / MaxFuel; }
		}

		public float PropOxygen
		{
			get { return CurrentOxygen / MaxOxygen; }
		}

		public List<ICargoCompartment> Compartments
		{
			get { return _Compartments; }
		}

		public new string Name
		{
			get { return base.Name; }
		}

		public Helmet Helmet
		{
			get { return helmet; }
			set
			{
				helmet = value;
				UpdateUI();
				if (helmet != null)
				{
					helmet.HudUI.UpdateUI();
				}
			}
		}

		public override bool IsInvetoryEquipable
		{
			get { return true; }
		}

		public override EquipType EquipTo
		{
			get { return EquipType.EquipInventory; }
		}

		public SpaceObjectVessel ParentVessel
		{
			get { return null; }
		}

		protected override void Awake()
		{
			base.Awake();
			_Compartments = new List<ICargoCompartment> { OxygenCompartment, PropellantCompartment };
		}

		private new void Start()
		{
			base.Start();
			UpdateUI();
		}

		public override void UpdateUI()
		{
			base.UpdateUI();
			FuelFillerImage.fillAmount = CurrentFuel / MaxFuel;
			FuelLow.Activate(CurrentFuel / MaxFuel < 0.2f);
			OxyImageFiller.fillAmount = CurrentOxygen / MaxOxygen;
			OxyLow.Activate(CurrentOxygen / MaxOxygen < 0.2f);
			if (helmet != null)
			{
				helmet.HudUI.UpdateUI();
			}
		}

		public void StartNozzles(Vector4 direction, bool myJetpack = false)
		{
			SoundEffect soundEffect = ((!myJetpack) ? JetpackSounds : MyJetpackSounds);
			if (soundEffect != null)
			{
				if (direction.z >= 0.01f)
				{
					soundEffect.Play(3);
					NozzleDir[2] = 1;
				}
				else
				{
					soundEffect.Play(9);
					NozzleDir[2] = 0;
				}

				if (direction.z <= -0.01f)
				{
					soundEffect.Play(2);
					NozzleDir[2] = -1;
				}
				else
				{
					soundEffect.Play(8);
				}

				if (direction.x >= 0.01f)
				{
					soundEffect.Play(0);
					NozzleDir[0] = 1;
				}
				else
				{
					soundEffect.Play(6);
					NozzleDir[0] = 0;
				}

				if (direction.x <= -0.01f)
				{
					soundEffect.Play(1);
					NozzleDir[0] = -1;
				}
				else
				{
					soundEffect.Play(7);
				}

				if (direction.y >= 0.01f)
				{
					soundEffect.Play(5);
					NozzleDir[1] = 1;
				}
				else
				{
					soundEffect.Play(11);
					NozzleDir[1] = 0;
				}

				if (direction.y <= -0.01f)
				{
					soundEffect.Play(4);
					NozzleDir[1] = -1;
				}
				else
				{
					soundEffect.Play(10);
				}

				if (direction.w >= 0.01f)
				{
					soundEffect.Play(12);
					NozzleDir[3] = 1;
				}
				else
				{
					soundEffect.Play(14);
					NozzleDir[3] = 0;
				}

				if (direction.w <= -0.01f)
				{
					soundEffect.Play(13);
					NozzleDir[3] = -1;
				}
				else
				{
					soundEffect.Play(15);
				}
			}

			foreach (Nozzle nozzle in nozzles)
			{
				if (direction.z >= 0.01f && nozzle.directions.Contains(NozzleDirection.Forward))
				{
					nozzle.ToggleNozzle(true);
				}
				else if (direction.z <= -0.01f && nozzle.directions.Contains(NozzleDirection.Backwards))
				{
					nozzle.ToggleNozzle(true);
				}
				else if (direction.x >= 0.01f && nozzle.directions.Contains(NozzleDirection.Right))
				{
					nozzle.ToggleNozzle(true);
				}
				else if (direction.x <= -0.01f && nozzle.directions.Contains(NozzleDirection.Left))
				{
					nozzle.ToggleNozzle(true);
				}
				else if (direction.y >= 0.01f && nozzle.directions.Contains(NozzleDirection.Up))
				{
					nozzle.ToggleNozzle(true);
				}
				else if (direction.y <= -0.01f && nozzle.directions.Contains(NozzleDirection.Down))
				{
					nozzle.ToggleNozzle(true);
				}
				else if (direction.w >= 0.01f && nozzle.directions.Contains(NozzleDirection.LeanRight))
				{
					nozzle.ToggleNozzle(true);
				}
				else if (direction.w <= -0.01f && nozzle.directions.Contains(NozzleDirection.LeanLeft))
				{
					nozzle.ToggleNozzle(true);
				}
				else
				{
					nozzle.ToggleNozzle(false);
				}
			}
		}

		public void StopNozzles()
		{
			foreach (Nozzle nozzle in nozzles)
			{
				nozzle.ToggleNozzle(false);
			}
		}

		public override void Special()
		{
			InventorySlot inventorySlot = MyPlayer.Instance.Inventory.FindEmptyOutfitSlot(this, true);
			if (inventorySlot == null ||
			    !MyPlayer.Instance.Inventory.AddToInventory(this, inventorySlot, MyPlayer.Instance.Inventory.HandsSlot))
			{
				string empty = string.Empty;
				empty = ((!(MyPlayer.Instance.Inventory.Outfit == null))
					? Localization.AlreadyEquipped.ToUpper()
					: Localization.EquipSuitFirst.ToUpper());
				World.InGameGUI.ShowInteractionCanvasMessage(empty);
			}
		}

		public override void ChangeEquip(EquipType type, Player pl)
		{
			if (pl is OtherPlayer)
			{
				(pl as OtherPlayer).CurrentJetpack = ((type != EquipTo) ? null : this);
			}
			else
			{
				if (!(pl is MyPlayer))
				{
					return;
				}

				MyPlayer myPlayer = pl as MyPlayer;
				if (type == EquipTo)
				{
					myPlayer.FpsController.CurrentJetpack = this;
					if (myPlayer.CurrentHelmet != null)
					{
						Helmet = myPlayer.CurrentHelmet;
						Helmet.Jetpack = this;
					}

					myPlayer.FpsController.RefreshMaxAngularVelocity();

					MyPlayer.Instance.CheckEquipmentAchievement();
				}
				else if (myPlayer.FpsController.CurrentJetpack == this)
				{
					myPlayer.FpsController.CurrentJetpack = null;
					OldJetpackActiveState = false;
					IsActive = false;
					if (Helmet != null)
					{
						Helmet.Jetpack = null;
						Helmet.HudUI.UpdateUI();
						Helmet = null;
					}

					StartNozzles(Vector4.zero);
					myPlayer.FpsController.RefreshMaxAngularVelocity();
				}
			}
		}

		public override void ProcesStatsData(DynamicObjectStats dos)
		{
			base.ProcesStatsData(dos);
			JetpackStats jetpackStats = dos as JetpackStats;
			if (_Compartments == null)
			{
				_Compartments = new List<ICargoCompartment> { OxygenCompartment, PropellantCompartment };
			}

			PropellantCompartment.Capacity = jetpackStats.PropellantCapacity;
			PropellantCompartment.Resources = new List<CargoResourceData>();
			if (jetpackStats.Propellant != null)
			{
				PropellantCompartment.Resources.Add(jetpackStats.Propellant);
			}

			OxygenCompartment.Capacity = jetpackStats.OxygenCapacity;
			OxygenCompartment.Resources = new List<CargoResourceData>();
			if (jetpackStats.Oxygen != null)
			{
				OxygenCompartment.Resources.Add(jetpackStats.Oxygen);
			}

			if (base.AttachPoint != null && MyPlayer.Instance.IsLockedToTrigger &&
			    MyPlayer.Instance.LockedToTrigger is SceneTriggerCargoPanel)
			{
				SceneTriggerCargoPanel sceneTriggerCargoPanel =
					MyPlayer.Instance.LockedToTrigger as SceneTriggerCargoPanel;
				sceneTriggerCargoPanel.CargoPanel.CreateAttachItemUI();
			}

			if (Helmet != null)
			{
				Helmet.HudUI.UpdateUI();
			}

			if (CurrentFuel <= float.Epsilon)
			{
				IsActive = false;
				StopNozzles();
			}

			UpdateUI();
		}

		public override DynamicObjectAuxData GetAuxData()
		{
			JetpackData baseAuxData = GetBaseAuxData<JetpackData>();
			baseAuxData.OxygenCompartment = OxygenCompartment.GetData();
			baseAuxData.PropellantCompartment = PropellantCompartment.GetData();
			baseAuxData.OxygenConsumption = OxygenConsumption;
			baseAuxData.PropellantConsumption = PropellantConsumption;
			return baseAuxData;
		}

		public ICargoCompartment GetCompartment(short? id)
		{
			throw new NotImplementedException();
		}

		public override bool ProcessSlotChange(Inventory inv, InventorySlot mySlot, InventorySlot nextSlot)
		{
			if (mySlot.SlotType == InventorySlot.Type.Hands && nextSlot.Item != null)
			{
				return true;
			}

			if (mySlot.SlotType == InventorySlot.Type.Equip && nextSlot.Item != null)
			{
				return true;
			}

			return false;
		}
	}
}
