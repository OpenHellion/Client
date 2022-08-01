using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class PowerSupplySolarPanel : MonoBehaviour
	{
		public GeneratorSolar SolarPanel;

		public Text Status;

		public Text PowerOutput;

		public Text EfficiencyPercentage;

		public Image EfficiencyFiller;

		public Text SolarToggleText;

		public Transform PartsTransform;

		public GameObject PartPref;

		public List<PartSlotUI> PowerPartsUI = new List<PartSlotUI>();

		public void Toggle()
		{
			SolarPanel.Toggle();
		}
	}
}
