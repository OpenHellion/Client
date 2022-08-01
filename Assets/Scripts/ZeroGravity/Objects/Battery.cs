using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class Battery : Item
	{
		public float MaxPower;

		public float CurrentPower;

		public HandDrill handDrill;

		public Image fill;

		public float BatteryPrecentage
		{
			get
			{
				return CurrentPower / MaxPower;
			}
		}

		private new void Start()
		{
			base.Start();
			UpdateUI();
		}

		public override void ChangeQuantity(float amount)
		{
			CurrentPower += amount;
		}

		public override DynamicObjectAuxData GetAuxData()
		{
			BatteryData baseAuxData = GetBaseAuxData<BatteryData>();
			baseAuxData.CurrentPower = CurrentPower;
			baseAuxData.MaxPower = MaxPower;
			return baseAuxData;
		}

		public override void ProcesStatsData(DynamicObjectStats dos)
		{
			base.ProcesStatsData(dos);
			BatteryStats batteryStats = dos as BatteryStats;
			CurrentPower = batteryStats.CurrentPower;
			MaxPower = batteryStats.MaxPower;
			UpdateUI();
		}

		public override void UpdateUI()
		{
			base.UpdateUI();
			if (fill != null)
			{
				fill.fillAmount = BatteryPrecentage;
			}
			if (CurrentPower <= MaxPower * 0.2f)
			{
				fill.color = Colors.Red;
			}
			else
			{
				fill.color = Colors.Green;
			}
		}

		public override string QuantityCheck()
		{
			return FormatHelper.Percentage(CurrentPower / MaxPower);
		}
	}
}
