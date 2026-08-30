using UnityEngine;
using UnityEngine.EventSystems;

namespace ZeroGravity.UI
{
	public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
	{
		public GameObject Object;

		private void OnEnable()
		{
			Object.Activate(false);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			Object.Activate(true);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			Object.Activate(false);
		}
	}
}
