namespace ZeroGravity.ShipComponents
{
	public interface IVesselSystemAccessory
	{
		VesselSystem BaseVesselSystem { get; set; }

		void BaseVesselSystemUpdated();
	}
}
