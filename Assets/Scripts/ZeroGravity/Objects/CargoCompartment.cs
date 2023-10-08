using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Data;

namespace ZeroGravity.Objects
{
	public class CargoCompartment : MonoBehaviour, ICargoCompartment
	{
		private static Dictionary<CargoCompartmentType, List<ResourceType>> allowedResourcesDict =
			new Dictionary<CargoCompartmentType, List<ResourceType>>
			{
				{
					CargoCompartmentType.None,
					new List<ResourceType>()
				},
				{
					CargoCompartmentType.CargoBayResources,
					new List<ResourceType>
					{
						ResourceType.Reserved,
						ResourceType.Ice,
						ResourceType.Regolith,
						ResourceType.DryIce,
						ResourceType.NitrateMinerals,
						ResourceType.Oxygen,
						ResourceType.Hydrogen,
						ResourceType.Helium3,
						ResourceType.Nitro,
						ResourceType.Nitrogen,
						ResourceType.CarbonFibers,
						ResourceType.Alloys,
						ResourceType.Circuits,
						ResourceType.Air,
						ResourceType.Ferrite,
						ResourceType.MaficRock,
						ResourceType.RareCompounds,
						ResourceType.Titanite,
						ResourceType.Superconductors,
						ResourceType.NickelIron,
						ResourceType.Silicates,
						ResourceType.RareElements,
						ResourceType.Titanium
					}
				},
				{
					CargoCompartmentType.RawResources,
					new List<ResourceType>
					{
						ResourceType.Ice,
						ResourceType.Regolith,
						ResourceType.DryIce,
						ResourceType.NitrateMinerals,
						ResourceType.Ferrite,
						ResourceType.MaficRock,
						ResourceType.RareCompounds,
						ResourceType.Titanite
					}
				},
				{
					CargoCompartmentType.RefinedResources,
					new List<ResourceType>
					{
						ResourceType.Oxygen,
						ResourceType.Hydrogen,
						ResourceType.Helium3,
						ResourceType.Nitro,
						ResourceType.Nitrogen,
						ResourceType.Reserved
					}
				},
				{
					CargoCompartmentType.CraftingResources,
					new List<ResourceType>
					{
						ResourceType.CarbonFibers,
						ResourceType.Alloys,
						ResourceType.Circuits,
						ResourceType.Superconductors,
						ResourceType.NickelIron,
						ResourceType.Silicates,
						ResourceType.RareElements,
						ResourceType.Titanium,
						ResourceType.Reserved
					}
				},
				{
					CargoCompartmentType.JetpackPropellant,
					new List<ResourceType> { ResourceType.Nitro }
				},
				{
					CargoCompartmentType.JetpackOxygen,
					new List<ResourceType> { ResourceType.Oxygen }
				},
				{
					CargoCompartmentType.RefinedCanister,
					new List<ResourceType>
					{
						ResourceType.Helium3,
						ResourceType.Hydrogen,
						ResourceType.Nitrogen,
						ResourceType.Oxygen,
						ResourceType.Nitro,
						ResourceType.CarbonFibers,
						ResourceType.Alloys,
						ResourceType.Circuits,
						ResourceType.Superconductors,
						ResourceType.NickelIron,
						ResourceType.Silicates,
						ResourceType.RareElements,
						ResourceType.Titanium
					}
				},
				{
					CargoCompartmentType.Canister,
					new List<ResourceType>
					{
						ResourceType.Ice,
						ResourceType.Regolith,
						ResourceType.DryIce,
						ResourceType.NitrateMinerals,
						ResourceType.Ferrite,
						ResourceType.MaficRock,
						ResourceType.RareCompounds,
						ResourceType.Titanite
					}
				},
				{
					CargoCompartmentType.RCS,
					new List<ResourceType> { ResourceType.Nitro }
				},
				{
					CargoCompartmentType.Engine,
					new List<ResourceType> { ResourceType.Hydrogen }
				},
				{
					CargoCompartmentType.PowerGenerator,
					new List<ResourceType> { ResourceType.Helium3 }
				},
				{
					CargoCompartmentType.AirGeneratorOxygen,
					new List<ResourceType> { ResourceType.Oxygen }
				},
				{
					CargoCompartmentType.AirGeneratorNitrogen,
					new List<ResourceType> { ResourceType.Nitrogen }
				},
				{
					CargoCompartmentType.AirTank,
					new List<ResourceType> { ResourceType.Air }
				},
				{
					CargoCompartmentType.WelderFuel,
					new List<ResourceType> { ResourceType.Hydrogen }
				},
				{
					CargoCompartmentType.FireRetardant,
					new List<ResourceType> { ResourceType.Nitrogen }
				}
			};

		[SerializeField] private CargoCompartmentType _CompartmentType;

		[SerializeField] private short _ID;

		[SerializeField] private string _Name;

		[SerializeField] private List<CargoResourceData> _Resources;

		[SerializeField] private float _Capacity;

		private ICargo _ParentCargo;

		[CompilerGenerated] private static Func<CargoResourceData, float> _003C_003Ef__am_0024cache0;

		public CargoCompartmentType CompartmentType
		{
			get { return _CompartmentType; }
			set { _CompartmentType = value; }
		}

		public short ID
		{
			get { return _ID; }
			set { _ID = value; }
		}

		public string Name
		{
			get
			{
				_Name = CompartmentType.ToLocalizedString();
				return _Name;
			}
			set { _Name = value; }
		}

		private List<ResourceType> allowedResources
		{
			get { return allowedResourcesDict[CompartmentType]; }
		}

		public bool AllowOnlyOneType
		{
			get
			{
				if (CompartmentType == CargoCompartmentType.CargoBayResources ||
				    CompartmentType == CargoCompartmentType.RawResources ||
				    CompartmentType == CargoCompartmentType.RefinedResources ||
				    CompartmentType == CargoCompartmentType.CraftingResources ||
				    CompartmentType == CargoCompartmentType.Canister ||
				    CompartmentType == CargoCompartmentType.CargoBayResources ||
				    CompartmentType == CargoCompartmentType.RefinedCanister)
				{
					return false;
				}

				return true;
			}
		}

		public List<CargoResourceData> Resources
		{
			get { return _Resources; }
			set { _Resources = value; }
		}

		public float Capacity
		{
			get { return _Capacity; }
			set { _Capacity = value; }
		}

		public ICargo ParentCargo
		{
			get { return _ParentCargo; }
		}

		public float AvailableCapacity
		{
			get
			{
				if (Resources == null)
				{
					return Capacity;
				}

				float capacity = Capacity;
				List<CargoResourceData> resources = Resources;
				if (_003C_003Ef__am_0024cache0 == null)
				{
					_003C_003Ef__am_0024cache0 = _003Cget_AvailableCapacity_003Em__0;
				}

				return capacity - resources.Sum(_003C_003Ef__am_0024cache0);
			}
		}

		public static bool IsRawResource(ResourceType resourceType)
		{
			return resourceType > ResourceType.None && resourceType < ResourceType.Oxygen;
		}

		public static bool IsRefinedResource(ResourceType resourceType)
		{
			return resourceType >= ResourceType.Oxygen && resourceType < ResourceType.Alloys;
		}

		public static bool IsCraftingResource(ResourceType resourceType)
		{
			return resourceType >= ResourceType.Alloys && resourceType < (ResourceType)300;
		}

		public static bool IsOther(ResourceType resourceType)
		{
			return resourceType >= (ResourceType)300;
		}

		private void Start()
		{
			_ParentCargo = GetComponentInParent<ICargo>();
		}

		public CargoCompartmentData GetData()
		{
			CargoCompartmentData cargoCompartmentData = new CargoCompartmentData();
			cargoCompartmentData.Type = CompartmentType;
			cargoCompartmentData.ID = ID;
			cargoCompartmentData.Capacity = Capacity;
			cargoCompartmentData.Name = Name;
			cargoCompartmentData.AllowedResources = allowedResources;
			cargoCompartmentData.AllowOnlyOneType = AllowOnlyOneType;
			cargoCompartmentData.Resources = Resources;
			return cargoCompartmentData;
		}

		public bool IsAllowed(ResourceType resourceType)
		{
			return allowedResources.Contains(resourceType);
		}

		public static bool IsAllowed(ResourceType resourceType, CargoCompartmentType compartmentType)
		{
			return allowedResourcesDict[compartmentType].Contains(resourceType);
		}

		[CompilerGenerated]
		private static float _003Cget_AvailableCapacity_003Em__0(CargoResourceData m)
		{
			return m.Quantity;
		}
	}
}
