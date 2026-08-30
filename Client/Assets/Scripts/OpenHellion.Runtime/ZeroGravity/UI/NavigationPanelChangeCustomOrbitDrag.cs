using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	[RequireComponent(typeof(Image))]
	public class NavigationPanelChangeCustomOrbitDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
		IEventSystemHandler
	{
		private RectTransform m_DraggingPlane;

		public int Selector;

		public NavigationPanel NavPanel;

		public void OnBeginDrag(PointerEventData eventData)
		{
		}

		public void OnDrag(PointerEventData eventData)
		{
		}

		public void OnEndDrag(PointerEventData eventData)
		{
		}
	}
}
