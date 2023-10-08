using OpenHellion;
using OpenHellion.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class StartingPointOptionUI : MonoBehaviour
	{
		public StartingPointOptionData Data;

		public MainMenuGUI.StartingPointOption Type;

		public Image Background;

		public Image Active;

		public Text Heading;

		public Text Description;

		private void Start()
		{
			transform.Reset();
			Heading.text = Data.Title;
			Background.sprite = Data.Background;
			if (!GetComponent<Button>().interactable)
			{
				Description.text = Data.DisabledDescription;
			}
			else
			{
				Description.text = Data.Description;
			}
		}
	}
}
