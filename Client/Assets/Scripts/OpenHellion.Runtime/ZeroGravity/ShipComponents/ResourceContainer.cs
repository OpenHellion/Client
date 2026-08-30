using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.ShipComponents
{
	public class ResourceContainer : VesselComponent, ICargo
	{
		public DistributionSystemType DistributionSystemType;

		public bool IsInUse = true;

		public CargoCompartment CargoCompartment;

		public float NominalInput;

		public float NominalOutput;

		[NonSerialized] public float QuantityChangeRate;

		private List<ICargoCompartment> _Compartments;

		[SerializeField] private string _Name = "Resource Tank";

		public override VesselComponentType ComponentType
		{
			get { return VesselComponentType.ResourceContainer; }
		}

		public float Capacity
		{
			get { return (!(CargoCompartment != null)) ? 0f : CargoCompartment.Capacity; }
		}

		public float Quantity
		{
			get
			{
				return (!(CargoCompartment != null) || CargoCompartment.Resources == null ||
				        CargoCompartment.Resources.Count <= 0)
					? 0f
					: CargoCompartment.Resources[0].Quantity;
			}
		}

		public List<ICargoCompartment> Compartments
		{
			get { return _Compartments; }
		}

		public string Name
		{
			get { return _Name; }
		}

		private new void Start()
		{
			_ParentVessel = GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
			_Compartments = new List<ICargoCompartment> { CargoCompartment };
		}

		public ICargoCompartment GetCompartment(short? id)
		{
			return CargoCompartment;
		}

		public ResourceContainerData GetData()
		{
			ResourceContainerData resourceContainerData = new ResourceContainerData();
			resourceContainerData.InSceneID = InSceneID;
			resourceContainerData.DistributionSystemType = DistributionSystemType;
			resourceContainerData.CargoCompartment =
				((!(CargoCompartment != null)) ? null : CargoCompartment.GetData());
			resourceContainerData.NominalInput = NominalInput;
			resourceContainerData.NominalOutput = NominalOutput;
			resourceContainerData.IsInUse = IsInUse;
			resourceContainerData.AuxData = GetAuxData();
			return resourceContainerData;
		}

		public void SetDetails(ResourceContainerDetails details)
		{
			CargoCompartment.Resources = details.Resources;
			QuantityChangeRate = details.QuantityChangeRate;
			IsInUse = details.IsInUse;
			SetAuxDetails(details.AuxDetails);
		}

		public virtual void SetAuxDetails(IAuxDetails auxDetails)
		{
		}
	}
}
