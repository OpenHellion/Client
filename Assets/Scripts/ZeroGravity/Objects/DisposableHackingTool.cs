using ZeroGravity.Data;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class DisposableHackingTool : Item
	{
		private new void Start()
		{
			base.Start();
		}

		public override void Special()
		{
			MyPlayer.Instance.animHelper.SetParameterTrigger(AnimatorHelper.Triggers.UseConsumable);
			SendPackage();
		}

		public void SendPackage()
		{
			DisposableHackingToolStats statsData = new DisposableHackingToolStats
			{
				Use = true
			};
			DynamicObj.SendStatsMessage(null, statsData);
		}

		public override DynamicObjectAuxData GetAuxData()
		{
			return GetBaseAuxData<DisposableHackingToolData>();
		}

		public override string QuantityCheck()
		{
			return FormatHelper.CurrentMax(Health, MaxHealth);
		}
	}
}
