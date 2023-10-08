using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class MessageBox : MonoBehaviour
	{
		public delegate void OnCloseDelegate();

		public OnCloseDelegate OnClose;

		private bool destroyOnClose;

		private bool wasCursorVisible;

		private void Update()
		{
			if (Keyboard.current.enterKey.wasPressedThisFrame)
			{
				HideMessageBox();
			}
		}

		public void HideMessageBox()
		{
			gameObject.SetActive(false);
			if (destroyOnClose)
			{
				Destroy(gameObject);
			}

			Cursor.visible = wasCursorVisible;
			PauseMenu.DisableGameMenu = false;
		}

		public void PopulateMessageBox(string title, string text, bool destroyOnClose)
		{
			PauseMenu.DisableGameMenu = true;
			wasCursorVisible = Cursor.visible;
			Cursor.visible = true;
			this.destroyOnClose = destroyOnClose;
			transform.Find("MessagePanel/Title").GetComponent<Text>().text = title;
			transform.Find("MessagePanel/Text").GetComponent<Text>().text = text;
		}
	}
}
