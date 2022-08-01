using ZeroGravity.Data;
using ZeroGravity.Network;

namespace ZeroGravity.ShipComponents
{
	public class ResourceContainerAirTank : ResourceContainer
	{
		public float AirQuality = 1f;

		public override SystemAuxData GetAuxData()
		{
			AirTankAuxData airTankAuxData = new AirTankAuxData();
			airTankAuxData.AirQuality = AirQuality;
			return airTankAuxData;
		}

		public override void SetAuxDetails(IAuxDetails auxDetails)
		{
			AirQuality = (auxDetails as AirTankAuxDetails).AirQuality;
		}
	}
}
