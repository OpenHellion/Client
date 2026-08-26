namespace ZeroGravity.Data
{
	public class SubSystemACDeviceAuxData : SystemAuxData
	{
		public float HeatingCapacity;

		public float CoolingCapacity;

		public override SystemAuxDataType AuxDataType
		{
			get { return SystemAuxDataType.ACDevice; }
		}
	}
}
