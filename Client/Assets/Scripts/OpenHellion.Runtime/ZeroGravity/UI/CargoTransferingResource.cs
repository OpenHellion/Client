using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class CargoTransferingResource : MonoBehaviour
	{
		public ICargoCompartment FromCompartment;

		public CargoResourceData FromResource;

		public ICargoCompartment ToCompartment;

		public Image Icon;

		public float Quantity;
	}
}
