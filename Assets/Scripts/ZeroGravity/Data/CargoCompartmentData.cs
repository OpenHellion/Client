using System.Collections.Generic;

namespace ZeroGravity.Data
{
	public class CargoCompartmentData : ISceneData
	{
		public short ID;

		public List<ResourceType> AllowedResources;

		public bool AllowOnlyOneType;

		public float Capacity;

		public string Name;

		public CargoCompartmentType Type;

		public List<CargoResourceData> Resources;
	}
}
