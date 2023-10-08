namespace ZeroGravity.Data
{
	public class SubSystemRCSAuxData : SystemAuxData
	{
		public float Acceleration;

		public float RotationAcceleration;

		public float RotationStabilization;

		public override SystemAuxDataType AuxDataType
		{
			get { return SystemAuxDataType.RCS; }
		}
	}
}
