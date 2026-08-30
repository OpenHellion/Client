using System.Collections;
using OpenHellion;
using UnityEngine;
using ZeroGravity.Math;
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

	[Range(0f, 1f)] public float Intensity;

	public bool Burning;

	private World _world;

	private void Awake()
	{
		_world = GameObject.Find("/World").GetComponent<World>();
	}

	private void OnEnable()
	{
		if ((bool)shader)
		{
			mat = new Material(shader)
			{
				name = "ImageEffectMaterial",
				hideFlags = HideFlags.HideAndDontSave
			};
		}
		else
		{
			Debug.LogWarning(gameObject.name + ": Shader is not assigned. Disabling image effect.",
				gameObject);
			enabled = false;
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
			DestroyImmediate(mat);
		}
	}

	private void Update()
	{
		if (!Burning)
		{
			try
			{
				ArtificialBody artificialBody = MyPlayer.Instance.Parent as ArtificialBody;
				Vector3D worldPosition = _world.LocalToWorldPosition(artificialBody.transform.position);
				CelestialBody parentCelesitalBody = _world.SolarSystem.GetParentCelestialBody(worldPosition);
				float num = (float)((worldPosition - parentCelesitalBody.Position).Magnitude -
				                    parentCelesitalBody.Radius);
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
