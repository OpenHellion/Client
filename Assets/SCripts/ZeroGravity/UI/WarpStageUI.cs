using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class WarpStageUI : MonoBehaviour
	{
		public NavigationPanel Panel;

		public Image Selected;

		public Text Stage;

		public int Value;

		private void Start()
		{
			Stage.text = ((Value != 0) ? Value.ToString() : "E");
		}

		public void SelectStage()
		{
			Panel.SelectWarpStage(Value);
		}
	}
}
