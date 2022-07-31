using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ParticleEffectIntensity
{
	public ParticleSystem Particle;

	public float Gravity;

	public List<ParticleIntensityParameter> IntensityParameters;
}
