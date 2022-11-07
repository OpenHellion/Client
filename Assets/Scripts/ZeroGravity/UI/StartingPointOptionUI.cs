using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.UI
{
	public class StartingPointOptionUI : MonoBehaviour
	{
		public StartingPointOptionData Data;

		public CanvasManager.StartingPointOption Type;

		public Image Background;

		public Image Active;

		public Text Heading;

		public Text Description;

		private void Start()
		{
			if (Data == null)
			{
				Data = Client.Instance.CanvasManager.StartingPointData.FirstOrDefault((StartingPointOptionData m) => m.Type == Type);
			}
			base.transform.Reset();
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
