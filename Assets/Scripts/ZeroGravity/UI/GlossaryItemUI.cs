using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class GlossaryItemUI : MonoBehaviour
	{
		public GlossaryMenu Menu;

		public GlosseryItem GlossaryItm;

		public Image Image;

		public Text ItemName;

		public Sprite Sprite;

		public void ShowFullDescription()
		{
			Menu.ShowLongDescription(this);
		}
	}
}
