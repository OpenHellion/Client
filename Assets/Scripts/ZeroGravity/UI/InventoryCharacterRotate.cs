using UnityEngine;
using UnityEngine.EventSystems;

namespace ZeroGravity.UI
{
	public class InventoryCharacterRotate : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
	{
		private float rotSpeed = 5f;

		private bool canZoom;

		private void Update()
		{
			if (canZoom && InputController.GetAxis(InputController.AxisNames.MouseWheel).IsNotEpsilonZero())
			{
				float axis = InputController.GetAxis(InputController.AxisNames.MouseWheel);
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
			InventoryCharacterPreview.instance.RotateCharacter(InputController.GetAxis(InputController.AxisNames.LookHorizontal) * rotSpeed);
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
