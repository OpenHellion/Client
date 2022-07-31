namespace ZeroGravity.Data
{
	public class GeneratorCapacitorAuxData : SystemAuxData
	{
		public float NominalCapacity;

		public float Capacity;

		public override SystemAuxDataType AuxDataType
		{
			get
			{
				return SystemAuxDataType.Capacitor;
			}
		}
	}
}
