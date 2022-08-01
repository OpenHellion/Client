using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.UI
{
	public class PowerSupplyPowerConsumer : MonoBehaviour
	{
		public PowerSupply MyPowerSupply;

		public VesselSystem VesselSystem;

		public Text Name;

		public Text Status;

		public Text Consumption;

		public void SetPowerConsumptionText(float value)
		{
			if (Consumption != null)
			{
				Consumption.text = FormatHelper.FormatValue(value, true);
			}
		}
	}
}
