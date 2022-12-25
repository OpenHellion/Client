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
			DynamicObject dynamicObj = DynamicObj;
			DisposableHackingToolStats statsData = new DisposableHackingToolStats
			{
				Use = true
			};
			dynamicObj.SendStatsMessage(null, statsData);
		}

		public override DynamicObjectAuxData GetAuxData()
		{
			return GetBaseAuxData<DisposableHackingToolData>();
		}

		public override void ProcesStatsData(DynamicObjectStats dos)
		{
			base.ProcesStatsData(dos);
			DisposableHackingToolStats disposableHackingToolStats = dos as DisposableHackingToolStats;
			if (disposableHackingToolStats.Use && DynamicObj.Parent is MyPlayer)
			{
				Client.Instance.ChangeStatsByIfNotAdmin(ProviderStatID.hacked_stuff, 1);
			}
		}

		public override string QuantityCheck()
		{
			return FormatHelper.CurrentMax(base.Health, base.MaxHealth);
		}
	}
}
