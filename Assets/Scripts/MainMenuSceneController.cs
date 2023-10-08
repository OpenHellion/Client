using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class MainMenuSceneController : MonoBehaviour
{
	public PostProcessProfile PostProcessProfile;

	public AnimationCurve FadeInCurve;

	public float FadeInTime = 1f;

	public AnimationCurve VolumeFadeInCurve;

	public AnimationCurve FadeOutCurve;

	public float FadeOutTime = 1f;

	public AnimationCurve VolumeFadeOutCurve;

	public AnimationCurve ChangeFocusCurve;

	public float ChangeFocusTime = 1f;

	[ContextMenuItem("ToggleFocus", "ToggleFocus")]
	public bool FocusState = true;

	private void OnEnable()
	{
		Initialize();
	}

	public void Initialize()
	{
		StartCoroutine(FadeIn());
		DepthOfField outSetting;
		PostProcessProfile.TryGetSettings<DepthOfField>(out outSetting);
		if (FocusState)
		{
			outSetting.focusDistance.value = ChangeFocusCurve.Evaluate(0f);
		}
		else
		{
			outSetting.focusDistance.value = ChangeFocusCurve.Evaluate(1f);
		}
	}

	public void Disable()
	{
		StartCoroutine(FadeOut());
	}

	public void ToggleFocus()
	{
		FocusState = !FocusState;
		OutOfFocus(FocusState);
	}

	public void OutOfFocus(bool state)
	{
		if (state)
		{
			StartCoroutine(Defocus());
		}
		else
		{
			StartCoroutine(Focus());
		}
	}

	public IEnumerator Defocus()
	{
		float focusTime = ChangeFocusTime;
		DepthOfField dof;
		PostProcessProfile.TryGetSettings<DepthOfField>(out dof);
		while (focusTime > 0f)
		{
			dof.focusDistance.value = ChangeFocusCurve.Evaluate(Mathf.Clamp01(focusTime / ChangeFocusTime));
			focusTime -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		dof.focusDistance.value = ChangeFocusCurve.Evaluate(0f);
	}

	public IEnumerator Focus()
	{
		float focusTime = ChangeFocusTime;
		DepthOfField dof;
		PostProcessProfile.TryGetSettings<DepthOfField>(out dof);
		while (focusTime > 0f)
		{
			dof.focusDistance.value = ChangeFocusCurve.Evaluate(1f - Mathf.Clamp01(focusTime / ChangeFocusTime));
			focusTime -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		dof.focusDistance.value = ChangeFocusCurve.Evaluate(1f);
	}

	public IEnumerator FadeIn()
	{
		float fadeTime = FadeInTime;
		ColorGrading colorGrading;
		PostProcessProfile.TryGetSettings<ColorGrading>(out colorGrading);
		while (fadeTime > 0f)
		{
			colorGrading.postExposure.value = FadeInCurve.Evaluate(1f - Mathf.Clamp01(fadeTime / FadeInTime));
			fadeTime -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		colorGrading.postExposure.value = FadeInCurve.Evaluate(1f);
	}

	public IEnumerator FadeOut()
	{
		float fadeTime = FadeOutTime;
		ColorGrading colorGrading;
		PostProcessProfile.TryGetSettings<ColorGrading>(out colorGrading);
		while (fadeTime > 0f)
		{
			colorGrading.postExposure.value = FadeOutCurve.Evaluate(1f - Mathf.Clamp01(fadeTime / FadeOutTime));
			fadeTime -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		colorGrading.postExposure.value = FadeOutCurve.Evaluate(1f);
		base.gameObject.SetActive(false);
	}
}
