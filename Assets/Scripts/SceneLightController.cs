using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[ExecuteInEditMode]
public class SceneLightController : MonoBehaviour
{
	public enum LightState
	{
		Normal = 0,
		ToxicAtmosphere = 1,
		LowPressure = 2
	}

	private Light light;

	private HxVolumetricLight volumelight;

	[Space(20f)]
	public LightState State;

	public bool OnOff = true;

	private bool previousOnOff = true;

	private LightState tempState;

	[ContextMenuItem("On", "test")]
	[ContextMenuItem("Off", "test2")]
	public float LightChangeDuration = 1f;

	public CurveHelper CurveHelper;

	[Space(20f)]
	[Tooltip("Should the ligth component be disabled when this state is active")]
	public bool OnDisable;

	[Range(0f, 10f)]
	public float OnIntensity;

	public Color OnColor;

	[Range(0f, 10f)]
	public float OnVolumetricIntensity;

	[Space(20f)]
	[Tooltip("Should the ligth component be disabled when this state is active")]
	public bool OffDisable;

	[Range(0f, 10f)]
	public float OffIntensity;

	public Color OffColor;

	[Range(0f, 10f)]
	public float OffVolumetricIntensity;

	[Space(20f)]
	[Tooltip("Should the ligth component be disabled when this state is active")]
	public bool ToxicDisable;

	[Range(0f, 10f)]
	public float ToxicIntensity;

	public Color ToxicColor;

	[Range(0f, 10f)]
	public float ToxicVolumetricIntensity;

	[Space(20f)]
	[Tooltip("Should the ligth component be disabled when this state is active")]
	public bool PressureDisable;

	[Range(0f, 10f)]
	public float PressureIntensity;

	public Color PressureColor;

	public List<LightEmission> LightEmissions;

	public List<MeshRenderer> EmissionRenderers;

	private void Reset()
	{
		if (GetComponent<HxVolumetricLight>() != null)
		{
			volumelight = GetComponent<HxVolumetricLight>();
			volumelight.CustomIntensity = true;
		}
		light = GetComponent<Light>();
		OnIntensity = light.intensity;
		OnColor = light.color;
		OffIntensity = light.intensity * 0.1f;
		OffColor = Color.blue;
		ToxicIntensity = light.intensity;
		ToxicColor = new Color(1f, 0.5f, 0f);
		PressureIntensity = light.intensity;
		PressureColor = Color.red;
	}

	private void OnEnable()
	{
		if (GetComponent<HxVolumetricLight>() != null)
		{
			volumelight = GetComponent<HxVolumetricLight>();
			volumelight.CustomIntensity = true;
		}
		if (light == null)
		{
			light = GetComponent<Light>();
		}
		if (Application.isPlaying)
		{
			foreach (LightEmission lightEmission in LightEmissions)
			{
				if (lightEmission != null)
				{
					lightEmission.GetDefaultColor();
				}
			}
		}
		light.cullingMask = ~((1 << LayerMask.NameToLayer("Sun")) | (1 << LayerMask.NameToLayer("Planets")) | (1 << LayerMask.NameToLayer("Map")) | (1 << LayerMask.NameToLayer("InventoryCharacter")));
		if (Application.isPlaying)
		{
			foreach (MeshRenderer emissionRenderer in EmissionRenderers)
			{
				Material[] materials = emissionRenderer.materials;
				foreach (Material material in materials)
				{
					material.SetColor("_ToxicColor", ToxicColor * ToxicIntensity);
					material.SetColor("_LowPressureColor", PressureColor * PressureIntensity);
				}
			}
		}
		CurveHelper = Resources.Load("ScriptableObjects/Helpers/LightCurves") as CurveHelper;
	}

	public void test()
	{
		SwitchOnOff(true);
	}

	public void test2()
	{
		SwitchOnOff(false);
	}

	public void SwitchOnOff(bool onOff)
	{
		OnOff = onOff;
		SwitchStateTo(State);
	}

	private IEnumerator OnOffLerp()
	{
		float startIntensity = light.intensity;
		float endIntensity = 0f;
		Color startColor = light.color;
		Color endColor2 = Color.white;
		float startVolume = 0f;
		float endVolume = 0f;
		float emissionState = 0f;
		if (volumelight != null)
		{
			startVolume = volumelight.Intensity;
		}
		if (OnOff)
		{
			if (State == LightState.Normal)
			{
				endIntensity = OnIntensity;
				endColor2 = OnColor;
				if (volumelight != null)
				{
					endVolume = OnVolumetricIntensity;
				}
				emissionState = 0f;
			}
			else if (State == LightState.ToxicAtmosphere)
			{
				endIntensity = ToxicIntensity;
				endColor2 = ToxicColor;
				if (volumelight != null)
				{
					endVolume = ToxicVolumetricIntensity;
				}
				emissionState = 1f;
			}
			else
			{
				endIntensity = PressureIntensity;
				endColor2 = PressureColor;
				if (volumelight != null)
				{
					endVolume = OffVolumetricIntensity;
				}
				emissionState = 2f;
			}
		}
		else
		{
			endIntensity = OffIntensity;
			endColor2 = OffColor;
			if (volumelight != null)
			{
				endVolume = OffVolumetricIntensity;
			}
		}
		int curveIndex = 0;
		if (CurveHelper != null)
		{
			curveIndex = Random.Range(0, CurveHelper.Curves.Count);
		}
		float time = LightChangeDuration;
		while (time > 0f)
		{
			if (CurveHelper != null)
			{
				light.intensity = Mathf.Lerp(startIntensity, endIntensity, CurveHelper.Curves[curveIndex].Evaluate(1f - time / LightChangeDuration));
				light.color = Color.Lerp(startColor, endColor2, CurveHelper.Curves[curveIndex].Evaluate(1f - time / LightChangeDuration));
				if (volumelight != null)
				{
					volumelight.Intensity = Mathf.Lerp(startVolume, endVolume, CurveHelper.Curves[curveIndex].Evaluate(1f - time / LightChangeDuration));
				}
				previousOnOff = OnOff;
			}
			else
			{
				light.intensity = Mathf.Lerp(startIntensity, endIntensity, 1f - time / LightChangeDuration);
				light.color = Color.Lerp(startColor, endColor2, 1f - time / LightChangeDuration);
				if (volumelight != null)
				{
					volumelight.Intensity = Mathf.Lerp(startVolume, endVolume, 1f - time / LightChangeDuration);
				}
			}
			time -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		light.intensity = endIntensity;
		foreach (MeshRenderer emissionRenderer in EmissionRenderers)
		{
			Material[] materials = emissionRenderer.materials;
			foreach (Material material in materials)
			{
				material.SetFloat("_EmissionControl", (!OnOff) ? 0f : 1f);
				material.SetFloat("_EmissionState", emissionState);
			}
		}
	}

	public void SwitchStateTo(LightState state)
	{
		State = state;
		StartCoroutine("OnOffLerp");
	}
}
