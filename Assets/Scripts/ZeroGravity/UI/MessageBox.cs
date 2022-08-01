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
			if (InputManager.GetKeyDown(KeyCode.KeypadEnter) || InputManager.GetKeyDown(KeyCode.Return))
			{
				HideMessageBox();
			}
		}

		public void HideMessageBox()
		{
			base.gameObject.SetActive(false);
			if (destroyOnClose)
			{
				Object.Destroy(base.gameObject);
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
			base.transform.Find("MessagePanel/Title").GetComponent<Text>().text = title;
			base.transform.Find("MessagePanel/Text").GetComponent<Text>().text = text;
		}
	}
}
