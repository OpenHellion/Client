using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZeroGravity.LevelDesign;

namespace ZeroGravity.UI
{
	[RequireComponent(typeof(Image))]
	public class CargoConnectedCargos : MonoBehaviour, IDropHandler, IEventSystemHandler
	{
		public SceneCargoBay Cargo;

		public float Capacity;

		public Text Name;

		public GameObject Selected;

		public CargoPanel MyCargoPanel
		{
			get
			{
				return GetComponentInParent<CargoPanel>();
			}
		}

		public void OnDrop(PointerEventData eventData)
		{
			CargoTransferingResource component = MyCargoPanel.DragingItem.GetComponent<CargoTransferingResource>();
			component.ToCompartment = Cargo.CargoBayResources;
			MyCargoPanel.SetTransferBox();
		}
	}
}
