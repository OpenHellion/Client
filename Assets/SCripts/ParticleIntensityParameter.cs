using System;

[Serializable]
public class ParticleIntensityParameter
{
	public enum ParameterType
	{
		none = 0,
		Speed = 1,
		Emission = 2,
		Size = 3,
		Life = 4
	}

	public ParameterType Parameter;

	public float MinValue;

	public float MaxValue = 1f;
}
