using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ZeroGravity.Data;

namespace ZeroGravity.UI
{
	public class CargoRefine : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler, IEventSystemHandler
	{
		private List<ResourceType> RefinableResources = new List<ResourceType>
		{
			ResourceType.DryIce,
			ResourceType.Regolith,
			ResourceType.Ice,
			ResourceType.NitrateMinerals
		};

		public CargoPanel MyCargoPanel
		{
			get
			{
				return GetComponentInParent<CargoPanel>();
			}
		}

		public void OnDrop(PointerEventData data)
		{
			CargoTransferingResource component = MyCargoPanel.DragingItem.GetComponent<CargoTransferingResource>();
			component.ToCompartment = MyCargoPanel.Refinery.CargoResources;
			if (component.FromCompartment != null)
			{
				MyCargoPanel.SetTransferBox();
			}
		}

		public void OnPointerEnter(PointerEventData data)
		{
		}

		public void OnPointerExit(PointerEventData data)
		{
		}
	}
}
