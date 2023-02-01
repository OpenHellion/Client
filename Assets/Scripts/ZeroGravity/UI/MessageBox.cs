using UnityEngine;
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
			if (InputController.GetKeyDown(KeyCode.KeypadEnter) || InputController.GetKeyDown(KeyCode.Return))
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
			GameMenu.DisableGameMenu = false;
		}

		public void PopulateMessageBox(string title, string text, bool destroyOnClose)
		{
			GameMenu.DisableGameMenu = true;
			wasCursorVisible = Cursor.visible;
			Cursor.visible = true;
			this.destroyOnClose = destroyOnClose;
			transform.Find("MessagePanel/Title").GetComponent<Text>().text = title;
			transform.Find("MessagePanel/Text").GetComponent<Text>().text = text;
		}
	}
}
