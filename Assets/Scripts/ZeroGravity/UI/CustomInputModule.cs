using TeamUtility.IO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ZeroGravity.UI
{
	public class CustomInputModule : TeamUtility.IO.StandaloneInputModule
	{
		private Vector2 customCursorPos;

		private readonly MouseState mouseState = new MouseState();

		private GameObject overGameObject;

		public bool UseCustomCursorPosition { get; set; }

		public void UpdateCursorPosition(Vector2 cursorPos)
		{
			if (UseCustomCursorPosition)
			{
				customCursorPos = cursorPos;
			}
		}

		protected override MouseState GetMousePointerEventData(int id)
		{
			PointerEventData data;
			bool pointerData = GetPointerData(-1, out data, true);
			data.Reset();
			if (pointerData)
			{
				if (UseCustomCursorPosition)
				{
					data.position = customCursorPos;
				}
				else
				{
					data.position = Input.mousePosition;
				}
			}
			Vector2 vector = Input.mousePosition;
			if (UseCustomCursorPosition)
			{
				vector = customCursorPos;
			}
			data.delta = vector - data.position;
			data.position = vector;
			data.scrollDelta = Input.mouseScrollDelta;
			data.button = PointerEventData.InputButton.Left;
			base.eventSystem.RaycastAll(data, m_RaycastResultCache);
			RaycastResult raycastResult2 = (data.pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache));
			m_RaycastResultCache.Clear();
			overGameObject = raycastResult2.gameObject;
			PointerEventData data2;
			GetPointerData(-2, out data2, true);
			CopyFromTo(data, data2);
			data2.button = PointerEventData.InputButton.Right;
			PointerEventData data3;
			GetPointerData(-3, out data3, true);
			CopyFromTo(data, data3);
			data3.button = PointerEventData.InputButton.Middle;
			mouseState.SetButtonState(PointerEventData.InputButton.Left, StateForMouseButton(0), data);
			mouseState.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), data2);
			mouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), data3);
			return mouseState;
		}

		public GameObject OverGameObject()
		{
			return overGameObject;
		}

		public void ToggleCustomCursorPosition(bool val)
		{
			if (val)
			{
				Client.Instance.InputModule.UseCustomCursorPosition = true;
			}
			else
			{
				Invoke("DelayTurnOff", 0.1f);
			}
		}

		private void DelayTurnOff()
		{
			Client.Instance.InputModule.UseCustomCursorPosition = false;
		}
	}
}
