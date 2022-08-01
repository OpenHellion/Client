using System;
using UnityEngine;

public static class GraphicsOptions
{
	public enum ShadowQlty
	{
		None = -1,
		Low = 0,
		Medium = 1,
		High = 2,
		VeryHigh = 3,
		NoShadows = 4
	}

	public enum TextureQlty
	{
		None = -1,
		Low = 0,
		Medium = 1,
		High = 2
	}

	public static bool AntiAliasing;

	public static bool AmbientOcclusion;

	public static bool DepthOfField;

	public static bool MotionBlur;

	public static bool EyeAdaptation;

	public static bool Bloom;

	public static bool ChromaticAberration;

	public static bool Shadows = true;

	public static float ShadowDistance;

	public static TextureQlty TextureQuality;

	public static ShadowQlty ShadowQuality;

	public static void ChangeQuality(bool? antiAliasing = null, bool? ambientOcclusion = null, bool? depthOfField = null, bool? motionBlur = null, bool? eyeAdaptation = null, bool? bloom = null, bool? chromaticAberration = null, TextureQlty? texQual = null, ShadowQlty? shadQual = null, float? shadowDistance = null)
	{
		if (antiAliasing.HasValue)
		{
			AntiAliasing = antiAliasing.Value;
		}
		if (ambientOcclusion.HasValue)
		{
			AmbientOcclusion = ambientOcclusion.Value;
		}
		if (depthOfField.HasValue)
		{
			DepthOfField = depthOfField.Value;
		}
		if (motionBlur.HasValue)
		{
			MotionBlur = motionBlur.Value;
		}
		if (eyeAdaptation.HasValue)
		{
			EyeAdaptation = eyeAdaptation.Value;
		}
		if (bloom.HasValue)
		{
			Bloom = bloom.Value;
		}
		if (chromaticAberration.HasValue)
		{
			ChromaticAberration = chromaticAberration.Value;
		}
		if (texQual.HasValue)
		{
			QualitySettings.masterTextureLimit = (int)(Enum.GetValues(typeof(TextureQlty)).Length - 2 - texQual.Value);
		}
		if (shadQual.HasValue && shadQual.Value != ShadowQlty.None)
		{
			if (shadQual.Value == ShadowQlty.NoShadows)
			{
				Shadows = false;
			}
			else
			{
				Shadows = true;
				QualitySettings.shadowResolution = (ShadowResolution)shadQual.Value;
			}
		}
		if (shadowDistance.HasValue)
		{
			QualitySettings.shadowDistance = shadowDistance.Value;
		}
	}
}
