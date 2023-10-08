using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ZeroGravity.UI
{
	public class InventoryCharacterRotate : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler,
		IEventSystemHandler
	{
		private const float RotSpeed = 5f;

		private bool canZoom;

		private void Update()
		{
			if (canZoom && Mouse.current.scroll.y.ReadValue().IsNotEpsilonZero())
			{
				float axis = Mouse.current.scroll.y.ReadValue();
				if (axis > 0f)
				{
					InventoryCharacterPreview.instance.ZoomCamera(0.1f);
				}
				else if (axis < 0f)
				{
					InventoryCharacterPreview.instance.ZoomCamera(-0.1f);
				}
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			InventoryCharacterPreview.instance.RotateCharacter(Mouse.current.delta.x.ReadValue() * RotSpeed);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			canZoom = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			canZoom = false;
		}
	}
}
