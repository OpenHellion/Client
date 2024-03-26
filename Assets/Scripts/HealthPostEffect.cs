using System;
using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class HealthPostEffect : MonoBehaviour
{
	public enum side
	{
		Front = 0,
		Back = 1,
		Left = 2,
		Right = 3
	}

	public bool isInCorutine;

	public Shader m_Shader;

	public Texture BloodTexture;

	public Texture BloodMask;

	public Texture Vignette;

	public Texture Veins;

	public Texture VeinsMask;

	[Range(0f, 1f)] public float VeinsAppear;

	public float LerpedVeins;

	public float HitFront;

	public float HitBack;

	public float HitLeft;

	public float HitRight;

	public float FadeSpeed = 1f;

	public float DamageMultiplier = 0.01f;

	public float VeinFadeSpeed = 1f;

	public float VeinDamageMultiplier = 0.1f;

	[Range(0f, 100f)] public float Health = 100f;

	public float LerpedHealth = 1f;

	[SerializeField] private Material material;

	[Range(0f, 100f)] public int BlurIterations;

	public float BlurAmount;

	public float Suffocation;

	public float SuffocationFadeSpeed = 0.3f;

	public float SuffocationDamageMultiplier = 0.01f;

	public Material m_Material
	{
		get { return material; }
	}

	private void OnEnable()
	{
		if ((bool)m_Shader)
		{
			material = new Material(m_Shader);
			material.name = "ImageEffectMaterial";
			material.hideFlags = HideFlags.HideAndDontSave;
			material.SetTexture("_BloodTex", BloodTexture);
			material.SetTexture("_BloodMask", BloodMask);
			material.SetTexture("_Vignette", Vignette);
			material.SetTexture("_Veins", Veins);
			material.SetTexture("_VeinsMask", VeinsMask);
		}
		else
		{
			Debug.LogWarning(base.gameObject.name + ": Shader is not assigned. Disabling image effect.",
				base.gameObject);
			base.enabled = false;
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		if ((bool)m_Shader && (bool)material)
		{
			material.SetFloat("_HitFront", HitFront);
			material.SetFloat("_HitBack", HitBack);
			material.SetFloat("_HitLeft", HitLeft);
			material.SetFloat("_HitRight", HitRight);
			material.SetFloat("_Health", LerpedHealth / 100f);
			material.SetFloat("_VeinsAppear", LerpedVeins);
			material.SetFloat("_BlurAmount", BlurAmount);
			material.SetFloat("_BlurIterations", BlurIterations);
			Graphics.Blit(src, dst, material);
		}
		else
		{
			Graphics.Blit(src, dst);
		}
	}

	private void OnDisable()
	{
		if ((bool)material)
		{
			UnityEngine.Object.DestroyImmediate(material);
		}
	}

	private void Update()
	{
		if (HitFront != 0f || HitBack != 0f || HitLeft != 0f || HitRight != 0f)
		{
			if (HitFront > 0f)
			{
				HitFront -= Time.deltaTime * FadeSpeed;
			}
			else
			{
				HitFront = 0f;
			}

			if (HitBack > 0f)
			{
				HitBack -= Time.deltaTime * FadeSpeed;
			}
			else
			{
				HitBack = 0f;
			}

			if (HitLeft > 0f)
			{
				HitLeft -= Time.deltaTime * FadeSpeed;
			}
			else
			{
				HitLeft = 0f;
			}

			if (HitRight > 0f)
			{
				HitRight -= Time.deltaTime * FadeSpeed;
			}
			else
			{
				HitRight = 0f;
			}
		}

		if (Mathf.Abs(LerpedVeins - VeinsAppear) > 0.01)
		{
			LerpedVeins = Mathf.Lerp(LerpedVeins, VeinsAppear, Time.deltaTime * 2f);
		}

		if (VeinsAppear != 0f)
		{
			if (VeinsAppear > 0f)
			{
				VeinsAppear -= Time.deltaTime * VeinFadeSpeed;
			}
			else
			{
				VeinsAppear = 0f;
			}
		}

		if (Mathf.Abs(Health - LerpedHealth) > 0.01f)
		{
			LerpedHealth = Mathf.Lerp(LerpedHealth, Health, Time.deltaTime * 2f);
		}

		if (Mathf.Abs(BlurAmount - Suffocation) > 0.01)
		{
			BlurAmount = Mathf.Lerp(BlurAmount, Suffocation, Time.deltaTime * 2f);
		}

		if (Suffocation != 0f && Suffocation > 0f)
		{
			Suffocation -= Time.deltaTime * SuffocationFadeSpeed;
		}
	}

	public void Hit(float Damage, side Side)
	{
		if (Side == side.Front && HitFront < 2f)
		{
			HitFront += Damage * DamageMultiplier;
		}

		if (Side == side.Back && HitBack < 2f)
		{
			HitBack += Damage * DamageMultiplier;
		}

		if (Side == side.Left && HitLeft < 2f)
		{
			HitLeft += Damage * DamageMultiplier;
		}

		if (Side == side.Right && HitRight < 2f)
		{
			HitRight += Damage * DamageMultiplier;
		}
	}

	public void LowPressureHit(float Damage)
	{
		if (VeinsAppear < 2f)
		{
			VeinsAppear += Damage * VeinDamageMultiplier;
		}
	}

	public void SuffocationHit(float Damage)
	{
		if (Suffocation < 2f)
		{
			Suffocation += Damage * SuffocationDamageMultiplier;
		}
	}

	public IEnumerator HitPlayer()
	{
		if (isInCorutine)
		{
			yield return null;
		}

		isInCorutine = true;
		float speed = (float)Math.PI;
		for (float val = 0f; val <= (float)Math.PI; val += speed * Time.deltaTime)
		{
			material.SetFloat("_Hurt", Mathf.Abs(Mathf.Sin(val)));
			yield return new WaitForEndOfFrame();
		}

		isInCorutine = false;
		yield return null;
	}
}
