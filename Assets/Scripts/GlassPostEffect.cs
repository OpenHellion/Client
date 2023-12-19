using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class GlassPostEffect : MonoBehaviour
{
	public Shader shader;

	public Material mat;

	public Texture2D DisplacementTexture;

	public Texture2D DirtTexture;

	public Color GlassTint;

	public float RefractionIntensity;

	public float RefractionStrength;

	public float ReflectionContrast;

	public float ReflectionIntensity;

	public float DirtIntensity;

	public float ReflectionSaturation;

	[Space(20f)] [ContextMenuItem("Raise", "Raise")] [ContextMenuItem("Lower", "Lower")] [Range(0f, 1f)]
	public float LowerVisor;

	private void OnEnable()
	{
		if ((bool)shader)
		{
			mat = new Material(shader);
			mat.name = "ImageEffectMaterial";
			mat.hideFlags = HideFlags.HideAndDontSave;
		}
		else
		{
			Debug.LogWarning(base.gameObject.name + ": Shader is not assigned. Disabling image effect.",
				base.gameObject);
			base.enabled = false;
		}

		LowerVisor = 0f;
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		if ((bool)shader && (bool)mat)
		{
			mat.SetTexture("_DisplacementTexture", DisplacementTexture);
			mat.SetTexture("_DirtTexture", DirtTexture);
			mat.SetColor("_GlassTint", GlassTint);
			mat.SetFloat("_RefractionIntensity", RefractionIntensity);
			mat.SetFloat("_RefractionStrength", RefractionStrength);
			mat.SetFloat("_ReflectionCap", ReflectionContrast);
			mat.SetFloat("_ReflectionIntensity", ReflectionIntensity);
			mat.SetFloat("_DirtIntensity", DirtIntensity);
			mat.SetFloat("_ReflectionSaturation", ReflectionSaturation);
			mat.SetFloat("_LowerHelmet", LowerVisor);
			Graphics.Blit(src, dst, mat);
		}
		else
		{
			Graphics.Blit(src, dst);
		}
	}

	private void OnDisable()
	{
		if ((bool)mat)
		{
			Object.DestroyImmediate(mat);
		}
	}

	public void Raise()
	{
		StartCoroutine("RaiseLowerVisor", true);
	}

	public void Lower()
	{
		StartCoroutine("RaiseLowerVisor", false);
	}

	private IEnumerator RaiseLowerVisor(bool raise)
	{
		while ((raise && LowerVisor > 0f) || (!raise && LowerVisor < 1f))
		{
			if (raise && LowerVisor > 0f)
			{
				LowerVisor -= 5f * Time.deltaTime;
			}
			else if (!raise && LowerVisor < 1f)
			{
				LowerVisor += 5f * Time.deltaTime;
			}

			AkSoundEngine.SetRTPCValue(SoundManager.HelmetOn, LowerVisor);
			yield return new WaitForEndOfFrame();
		}

		if (raise)
		{
			LowerVisor = 0f;
		}
		else
		{
			LowerVisor = 1f;
		}

		AkSoundEngine.SetRTPCValue(SoundManager.HelmetOn, LowerVisor);
	}
}
