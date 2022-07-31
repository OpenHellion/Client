using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class ConfirmMessageBox : MonoBehaviour
	{
		public delegate void OnYesDelegate();

		public delegate void OnNoDelegate();

		public OnYesDelegate OnYes;

		public OnNoDelegate OnNo;

		private bool wasCursorVisible;

		public Text Title;

		public Text Content;

		public Text Yes;

		public Text No;

		public void OnYesClicked()
		{
			if (OnYes != null)
			{
				OnYes();
			}
			HideConfirmMessageBox();
		}

		public void OnNoClicked()
		{
			if (OnNo != null)
			{
				OnNo();
			}
			HideConfirmMessageBox();
		}

		public void HideConfirmMessageBox()
		{
			Client.Instance.CanvasManager.IsConfirmBoxActive = false;
			base.gameObject.SetActive(false);
			Cursor.visible = wasCursorVisible;
			GameMenu.DisableGameMenu = false;
		}

		public void PopulateConfirmMessageBox(string title, string message, string yesText = "Ok", string noText = "Cancel")
		{
			Client.Instance.CanvasManager.IsConfirmBoxActive = true;
			GameMenu.DisableGameMenu = true;
			wasCursorVisible = Cursor.visible;
			Cursor.visible = true;
			Title.text = title;
			Content.text = message;
			Yes.text = yesText;
			No.text = noText;
		}
	}
}
