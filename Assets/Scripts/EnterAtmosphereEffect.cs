using System.Collections;
using UnityEngine;
using ZeroGravity.Objects;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class EnterAtmosphereEffect : MonoBehaviour
{
	private Material mat;

	public Shader shader;

	public Texture noise;

	public Color heat;

	public float MinDistance = 10000f;

	public float MaxDistance = 100000f;

	[Range(0f, 1f)]
	public float Intensity;

	public bool Burning;

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
			Debug.LogWarning(base.gameObject.name + ": Shader is not assigned. Disabling image effect.", base.gameObject);
			base.enabled = false;
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		if ((bool)shader && (bool)mat)
		{
			mat.SetTexture("_noise", noise);
			mat.SetColor("_heat", heat);
			mat.SetFloat("_height", Intensity);
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

	private void Update()
	{
		if (!Burning)
		{
			try
			{
				ArtificialBody artificialBody = MyPlayer.Instance.Parent as ArtificialBody;
				CelestialBody parentCelesitalBody = artificialBody.ParentCelesitalBody;
				float num = (float)((artificialBody.Position - parentCelesitalBody.Position).Magnitude - parentCelesitalBody.Radius);
				Intensity = 1f - Mathf.Clamp01((num - MinDistance) / (MaxDistance - MinDistance));
			}
			catch
			{
				Intensity = 0f;
			}
		}
	}

	public void BurnEffect(float time)
	{
		StartCoroutine(Burn(time));
	}

	private IEnumerator Burn(float time)
	{
		float BurningTime = time;
		while (BurningTime > 0f || Intensity > 0f)
		{
			Intensity = Mathf.Lerp(Intensity, BurningTime / time, Time.deltaTime);
			BurningTime -= Time.deltaTime;
			Burning = true;
			yield return new WaitForEndOfFrame();
		}
		Burning = false;
		Intensity = 0f;
	}
}
