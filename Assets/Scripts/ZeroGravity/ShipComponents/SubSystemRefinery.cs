using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.ShipComponents
{
	public class SubSystemRefinery : SubSystem, IPowerConsumer, ICargo
	{
		[CompilerGenerated]
		private sealed class _003CGetCompartment_003Ec__AnonStorey0
		{
			internal short? id;

			internal bool _003C_003Em__0(ICargoCompartment m)
			{
				return m.ID == id.Value;
			}
		}

		[CompilerGenerated]
		private sealed class _003CSetAuxDetails_003Ec__AnonStorey1
		{
			internal CargoCompartmentDetails ccd;

			internal bool _003C_003Em__0(ICargoCompartment m)
			{
				return m.ID == ccd.ID;
			}
		}

		[SerializeField] private string _Name = "Refinery";

		private List<ICargoCompartment> _Compartments;

		[SerializeField] private ResourceRequirement[] _ResourceRequirements = new ResourceRequirement[1]
		{
			new ResourceRequirement
			{
				ResourceType = DistributionSystemType.Power
			}
		};

		public float Capacity = 1000f;

		public CargoCompartment CargoResources;

		[Tooltip("Full capacity processing duration in seconds.")]
		public float ProcessingTime;

		public List<RefinedResourcesData> Resources;

		[NonSerialized] public float CurrentProgress;

		public override SubSystemType Type
		{
			get { return SubSystemType.Refinery; }
		}

		public string Name
		{
			get { return _Name; }
		}

		public List<ICargoCompartment> Compartments
		{
			get { return _Compartments; }
		}

		public override ResourceRequirement[] ResourceRequirements
		{
			get { return _ResourceRequirements; }
		}

		protected override void Start()
		{
			base.Start();
			_Compartments = new List<ICargoCompartment> { CargoResources };
		}

		public override SystemAuxData GetAuxData()
		{
			SubSystemRefineryAuxData subSystemRefineryAuxData = new SubSystemRefineryAuxData();
			subSystemRefineryAuxData.Capacity = Capacity;
			subSystemRefineryAuxData.ProcessingTime = ProcessingTime;
			subSystemRefineryAuxData.Resources = Resources;
			subSystemRefineryAuxData.CargoCompartments = new List<CargoCompartmentData>
				{ (!(CargoResources == null)) ? CargoResources.GetData() : null };
			return subSystemRefineryAuxData;
		}

		public ICargoCompartment GetCompartment(short? id = null)
		{
			_003CGetCompartment_003Ec__AnonStorey0 _003CGetCompartment_003Ec__AnonStorey =
				new _003CGetCompartment_003Ec__AnonStorey0();
			_003CGetCompartment_003Ec__AnonStorey.id = id;
			if (_003CGetCompartment_003Ec__AnonStorey.id.HasValue)
			{
				return _Compartments.Find(_003CGetCompartment_003Ec__AnonStorey._003C_003Em__0);
			}

			return _Compartments[0];
		}

		public override void SetAuxDetails(IAuxDetails auxDetails)
		{
			RefineryAuxDetails refineryAuxDetails = auxDetails as RefineryAuxDetails;
			if (_Compartments == null)
			{
				_Compartments = new List<ICargoCompartment> { CargoResources };
			}

			using (List<CargoCompartmentDetails>.Enumerator enumerator =
			       refineryAuxDetails.CargoCompartments.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					_003CSetAuxDetails_003Ec__AnonStorey1 _003CSetAuxDetails_003Ec__AnonStorey =
						new _003CSetAuxDetails_003Ec__AnonStorey1();
					_003CSetAuxDetails_003Ec__AnonStorey.ccd = enumerator.Current;
					ICargoCompartment cargoCompartment =
						_Compartments.Find(_003CSetAuxDetails_003Ec__AnonStorey._003C_003Em__0);
					if (cargoCompartment != null)
					{
						cargoCompartment.Resources = _003CSetAuxDetails_003Ec__AnonStorey.ccd.Resources;
					}
				}
			}
		}
	}
}
