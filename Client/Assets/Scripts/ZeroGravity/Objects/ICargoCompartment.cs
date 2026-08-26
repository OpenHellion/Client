using System.Collections.Generic;
using ZeroGravity.Data;

namespace ZeroGravity.Objects
{
	public interface ICargoCompartment
	{
		CargoCompartmentType CompartmentType { get; }

		short ID { get; set; }

		float Capacity { get; set; }

		List<CargoResourceData> Resources { get; set; }

		float AvailableCapacity { get; }

		string Name { get; }

		ICargo ParentCargo { get; }

		bool IsAllowed(ResourceType resourceType);
	}
}
