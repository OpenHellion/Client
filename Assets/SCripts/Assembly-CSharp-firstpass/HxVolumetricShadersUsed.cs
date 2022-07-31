using UnityEngine;

public class HxVolumetricShadersUsed : ScriptableObject
{
	public HxVolumetricCamera.TransparencyQualities TransperencyQuality = HxVolumetricCamera.TransparencyQualities.Medium;

	public HxVolumetricCamera.DensityParticleQualities DensityParticleQuality = HxVolumetricCamera.DensityParticleQualities.High;

	[HideInInspector]
	public HxVolumetricCamera.DensityParticleQualities LastDensityParticleQuality = HxVolumetricCamera.DensityParticleQualities.High;

	[HideInInspector]
	public HxVolumetricCamera.TransparencyQualities LastTransperencyQuality = HxVolumetricCamera.TransparencyQualities.Medium;

	private static HxVolumetricShadersUsed instance;

	public bool Full;

	public bool LowRes;

	public bool HeightFog;

	public bool HeightFogOff;

	public bool Noise;

	public bool NoiseOff;

	public bool Transparency;

	public bool TransparencyOff;

	public bool DensityParticles;

	public bool Point;

	public bool Spot;

	public bool Directional;

	public bool SinglePassStereo;

	public bool Projector;

	[HideInInspector]
	public bool FullLast;

	[HideInInspector]
	public bool LowResLast;

	[HideInInspector]
	public bool HeightFogLast;

	[HideInInspector]
	public bool HeightFogOffLast;

	[HideInInspector]
	public bool NoiseLast;

	[HideInInspector]
	public bool NoiseOffLast;

	[HideInInspector]
	public bool TransparencyLast;

	[HideInInspector]
	public bool TransparencyOffLast;

	[HideInInspector]
	public bool DensityParticlesLast;

	[HideInInspector]
	public bool PointLast;

	[HideInInspector]
	public bool SpotLast;

	[HideInInspector]
	public bool DirectionalLast;

	[HideInInspector]
	public bool SinglePassStereoLast;

	[HideInInspector]
	public bool ProjectorLast;

	private bool CheckDirty()
	{
		bool result = false;
		if (Resources.Load("HxUsedShaders") == null)
		{
			result = true;
		}
		if (Resources.Load("HxUsedShaderVariantCollection") == null)
		{
			result = true;
		}
		if (Full != FullLast)
		{
			result = true;
			FullLast = Full;
		}
		if (LowRes != LowResLast)
		{
			result = true;
			LowResLast = LowRes;
		}
		if (HeightFog != HeightFogLast)
		{
			result = true;
			HeightFogLast = HeightFog;
		}
		if (HeightFogOff != HeightFogOffLast)
		{
			result = true;
			HeightFogOffLast = HeightFogOff;
		}
		if (Noise != NoiseLast)
		{
			result = true;
			NoiseLast = Noise;
		}
		if (NoiseOff != NoiseOffLast)
		{
			result = true;
			NoiseOffLast = NoiseOff;
		}
		if (Transparency != TransparencyLast)
		{
			result = true;
			TransparencyLast = Transparency;
		}
		if (TransparencyOff != TransparencyOffLast)
		{
			result = true;
			TransparencyOffLast = TransparencyOff;
		}
		if (DensityParticles != DensityParticlesLast)
		{
			result = true;
			DensityParticlesLast = DensityParticles;
		}
		if (Point != PointLast)
		{
			result = true;
			PointLast = Point;
		}
		if (Spot != SpotLast)
		{
			result = true;
			SpotLast = Spot;
		}
		if (Directional != DirectionalLast)
		{
			result = true;
			DirectionalLast = Directional;
		}
		if (SinglePassStereo != SinglePassStereoLast)
		{
			result = true;
			SinglePassStereoLast = SinglePassStereo;
		}
		if (Projector != ProjectorLast)
		{
			result = true;
			ProjectorLast = Projector;
		}
		return result;
	}
}
