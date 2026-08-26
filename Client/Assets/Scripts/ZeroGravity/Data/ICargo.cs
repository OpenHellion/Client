using System.Collections.Generic;
using ZeroGravity.Objects;

namespace ZeroGravity.Data
{
	public interface ICargo
	{
		List<ICargoCompartment> Compartments { get; }

		string Name { get; }

		SpaceObjectVessel ParentVessel { get; }

		ICargoCompartment GetCompartment(short? id);
	}
}
