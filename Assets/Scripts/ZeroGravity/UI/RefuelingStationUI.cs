using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class RefuelingStationUI : MonoBehaviour
	{
		public GameObject refuelingSubSystemPrefab;

		public Text RefuelingStationText;

		private static Dictionary<Type, string> namesOfSystems = new Dictionary<Type, string>
		{
			{
				typeof(SubSystemRCS),
				Localization.RCS
			},
			{
				typeof(SubSystemEngine),
				Localization.Engine
			},
			{
				typeof(GeneratorAir),
				Localization.AirGenerator
			},
			{
				typeof(GeneratorPower),
				Localization.FusionReactor
			}
		};

		private Dictionary<CargoCompartment, RefuelingSubSystemPrefab> resourceContainers =
			new Dictionary<CargoCompartment, RefuelingSubSystemPrefab>();

		private SpaceObjectVessel parentVessel;

		private void Start()
		{
			if (parentVessel == null && GetComponentInParent<GeometryRoot>() != null)
			{
				parentVessel = GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
			}

			RefuelingStationText.text = "<color=#ec5500>AltCorp</color>" + Localization.RefuelingStation;
			Invoke("MakeResourceContainers", 3f);
		}

		public void UpdateResourceContainer(ResourceContainer rcv)
		{
			MakeResourceContainers();
			if (rcv == null)
			{
				return;
			}

			foreach (CargoCompartment compartment in rcv.Compartments)
			{
				RefuelingSubSystemPrefab value;
				if (resourceContainers.TryGetValue(compartment, out value))
				{
					value.ValueOfCompartment.text = "<color=#989898>" +
					                                (compartment.Capacity - compartment.AvailableCapacity).ToString(
						                                "f1") + "</color> / " + compartment.Capacity.ToString("f1");
					value.Filler.fillAmount =
						(compartment.Capacity - compartment.AvailableCapacity) / compartment.Capacity;
				}
			}
		}

		private void MakeResourceContainers()
		{
			if (resourceContainers.Count > 0)
			{
				return;
			}

			VesselSystem[] componentsInChildren = parentVessel.GeometryRoot.GetComponentsInChildren<VesselSystem>();
			foreach (VesselSystem vesselSystem in componentsInChildren)
			{
				if (!(vesselSystem is SubSystemRCS) && !(vesselSystem is SubSystemEngine) &&
				    !(vesselSystem is GeneratorAir) && !(vesselSystem is GeneratorPower))
				{
					continue;
				}

				ResourceContainer[] array = vesselSystem.ResourceContainers;
				foreach (ResourceContainer resourceContainer in array)
				{
					foreach (CargoCompartment compartment in resourceContainer.Compartments)
					{
						try
						{
							GameObject gameObject = UnityEngine.Object.Instantiate(refuelingSubSystemPrefab,
								refuelingSubSystemPrefab.transform.parent);
							RefuelingSubSystemPrefab component = gameObject.GetComponent<RefuelingSubSystemPrefab>();
							string value = vesselSystem.name;
							namesOfSystems.TryGetValue(vesselSystem.GetType(), out value);
							component.Name.text = value;
							component.NameOfCompartment.text = compartment.Name;
							component.ValueOfCompartment.text = "<color=#989898>" +
							                                    (compartment.Capacity - compartment.AvailableCapacity)
							                                    .ToString("f1") + "</color> / " +
							                                    compartment.Capacity.ToString("f1");
							component.Filler.fillAmount = (compartment.Capacity - compartment.AvailableCapacity) /
							                              compartment.Capacity;
							gameObject.SetActive(true);
							resourceContainers.Add(compartment, component);
						}
						catch (Exception)
						{
						}
					}
				}
			}
		}
	}
}
