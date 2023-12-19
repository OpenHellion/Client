using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.LevelDesign
{
	public class SceneCargoBay : VesselComponent, ICargo
	{
		[CompilerGenerated]
		private sealed class _003CSetDetails_003Ec__AnonStorey0
		{
			internal CargoCompartmentDetails ccd;

			internal bool _003C_003Em__0(ICargoCompartment m)
			{
				return m.ID == ccd.ID;
			}
		}

		[CompilerGenerated]
		private sealed class _003CGetCompartment_003Ec__AnonStorey1
		{
			internal short? id;

			internal bool _003C_003Em__0(ICargoCompartment m)
			{
				return m.ID == id.Value;
			}
		}

		private List<ICargoCompartment> _Compartments;

		[SerializeField] private string _Name = "Cargo Bay";

		public CargoCompartment CargoBayResources;

		public List<ICargoCompartment> Compartments => _Compartments;

		public string Name => _Name;

		public override VesselComponentType ComponentType => VesselComponentType.CargoBay;

		private new void Start()
		{
			_ParentVessel = GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
			_Compartments = new List<ICargoCompartment> { CargoBayResources };
		}

		public CargoBayData GetData()
		{
			CargoBayData cargoBayData = new CargoBayData();
			cargoBayData.InSceneID = base.InSceneID;
			cargoBayData.CargoCompartments = new List<CargoCompartmentData>
				{ (!(CargoBayResources == null)) ? CargoBayResources.GetData() : null };
			return cargoBayData;
		}

		public void SetDetails(CargoBayDetails details)
		{
			if (_Compartments == null)
			{
				_Compartments = new List<ICargoCompartment> { CargoBayResources };
			}

			using (List<CargoCompartmentDetails>.Enumerator enumerator = details.CargoCompartments.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					_003CSetDetails_003Ec__AnonStorey0 _003CSetDetails_003Ec__AnonStorey =
						new _003CSetDetails_003Ec__AnonStorey0();
					_003CSetDetails_003Ec__AnonStorey.ccd = enumerator.Current;
					ICargoCompartment cargoCompartment =
						_Compartments.Find(_003CSetDetails_003Ec__AnonStorey._003C_003Em__0);
					if (cargoCompartment != null)
					{
						cargoCompartment.Resources = _003CSetDetails_003Ec__AnonStorey.ccd.Resources;
					}
				}
			}
		}

		public ICargoCompartment GetCompartment(short? id)
		{
			_003CGetCompartment_003Ec__AnonStorey1 _003CGetCompartment_003Ec__AnonStorey =
				new _003CGetCompartment_003Ec__AnonStorey1();
			_003CGetCompartment_003Ec__AnonStorey.id = id;
			if (_003CGetCompartment_003Ec__AnonStorey.id.HasValue)
			{
				return _Compartments.Find(_003CGetCompartment_003Ec__AnonStorey._003C_003Em__0);
			}

			return _Compartments[0];
		}
	}
}
