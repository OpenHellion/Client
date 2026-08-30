namespace ZeroGravity.Data
{
	public class VesselBaseSystemAuxData : SystemAuxData
	{
		public float DecayDamageMultiplier;

		public float DebrisFieldDamageMultiplier;

		public override SystemAuxDataType AuxDataType
		{
			get { return SystemAuxDataType.VesselBaseSystem; }
		}
	}
}
