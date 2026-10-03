using OpenHellion.Net.Message;
using ZeroGravity.Data;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class Medpack : Item
	{
		public float RegenRate;

		public float MaxHp;

		public bool isUsed;

		public override bool PrimaryFunction()
		{
			if (!isUsed)
			{
				MyPlayer.Instance.AnimHelper.SetParameterTrigger(AnimatorHelper.Triggers.UseConsumable);
				SendPackage();
				isUsed = true;
			}

			return false;
		}

		public void SendPackage()
		{
			World.SolarSystem.SendCommand(StateUpdateRequest.CommandType.UseMedpack, GUID);
		}

		public override DynamicObjectAuxData GetAuxData()
		{
			MedpackData baseAuxData = GetBaseAuxData<MedpackData>();
			baseAuxData.MaxHP = MaxHp;
			baseAuxData.RegenRate = RegenRate;
			return baseAuxData;
		}

		public override void ProcesStatsData(DynamicObjectStats dos)
		{
			base.ProcesStatsData(dos);
		}
	}
}
