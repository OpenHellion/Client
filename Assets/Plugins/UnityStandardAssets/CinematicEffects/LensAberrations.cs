using System;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Cinematic/Lens Aberrations")]
	public class LensAberrations : MonoBehaviour
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class SettingsGroup : Attribute
		{
		}

		[AttributeUsage(AttributeTargets.Field)]
		public class AdvancedSetting : Attribute
		{
		}

		[Serializable]
		public struct DistortionSettings
		{
			public bool enabled;

			[Range(-100f, 100f)]
			[Tooltip("Distortion amount.")]
			public float amount;

			[Range(-1f, 1f)]
			[Tooltip("Distortion center point (X axis).")]
			public float centerX;

			[Range(-1f, 1f)]
			[Tooltip("Distortion center point (Y axis).")]
			public float centerY;

			[Range(0f, 1f)]
			[Tooltip("Amount multiplier on X axis. Set it to 0 to disable distortion on this axis.")]
			public float amountX;

			[Range(0f, 1f)]
			[Tooltip("Amount multiplier on Y axis. Set it to 0 to disable distortion on this axis.")]
			public float amountY;

			[Range(0.01f, 5f)]
			[Tooltip("Global screen scaling.")]
			public float scale;

			public static DistortionSettings defaultSettings
			{
				get
				{
					DistortionSettings result = default(DistortionSettings);
					result.enabled = false;
					result.amount = 0f;
					result.centerX = 0f;
					result.centerY = 0f;
					result.amountX = 1f;
					result.amountY = 1f;
					result.scale = 1f;
					return result;
				}
			}
		}

		[Serializable]
		public struct VignetteSettings
		{
			public bool enabled;

			[ColorUsage(false)]
			[Tooltip("Vignette color. Use the alpha channel for transparency.")]
			public Color color;

			[Tooltip("Sets the vignette center point (screen center is [0.5,0.5]).")]
			public Vector2 center;

			[Range(0f, 3f)]
			[Tooltip("Amount of vignetting on screen.")]
			public float intensity;

			[Range(0.01f, 3f)]
			[Tooltip("Smoothness of the vignette borders.")]
			public float smoothness;

			[AdvancedSetting]
			[Range(0f, 1f)]
			[Tooltip("Lower values will make a square-ish vignette.")]
			public float roundness;

			[Range(0f, 1f)]
			[Tooltip("Blurs the corners of the screen. Leave this at 0 to disable it.")]
			public float blur;

			[Range(0f, 1f)]
			[Tooltip("Desaturate the corners of the screen. Leave this to 0 to disable it.")]
			public float desaturate;

			public static VignetteSettings defaultSettings
			{
				get
				{
					VignetteSettings result = default(VignetteSettings);
					result.enabled = false;
					result.color = new Color(0f, 0f, 0f, 1f);
					result.center = new Vector2(0.5f, 0.5f);
					result.intensity = 1.4f;
					result.smoothness = 0.8f;
					result.roundness = 1f;
					result.blur = 0f;
					result.desaturate = 0f;
					return result;
				}
			}
		}

		[Serializable]
		public struct ChromaticAberrationSettings
		{
			public bool enabled;

			[ColorUsage(false)]
			[Tooltip("Channels to apply chromatic aberration to.")]
			public Color color;

			[Range(-50f, 50f)]
			[Tooltip("Amount of tangential distortion.")]
			public float amount;

			public static ChromaticAberrationSettings defaultSettings
			{
				get
				{
					ChromaticAberrationSettings result = default(ChromaticAberrationSettings);
					result.enabled = false;
					result.color = Color.green;
					result.amount = 0f;
					return result;
				}
			}
		}

		private enum Pass
		{
			BlurPrePass = 0,
			Chroma = 1,
			Distort = 2,
			Vignette = 3,
			ChromaDistort = 4,
			ChromaVignette = 5,
			DistortVignette = 6,
			ChromaDistortVignette = 7
		}

		[SettingsGroup]
		public DistortionSettings distortion = DistortionSettings.defaultSettings;

		[SettingsGroup]
		public VignetteSettings vignette = VignetteSettings.defaultSettings;

		[SettingsGroup]
		public ChromaticAberrationSettings chromaticAberration = ChromaticAberrationSettings.defaultSettings;

		[SerializeField]
		private Shader m_Shader;

		private Material m_Material;

		private RenderTextureUtility m_RTU;

		public Shader shader
		{
			get
			{
				if (m_Shader == null)
				{
					m_Shader = Shader.Find("Hidden/LensAberrations");
				}
				return m_Shader;
			}
		}

		public Material material
		{
			get
			{
				if (m_Material == null)
				{
					m_Material = ImageEffectHelper.CheckShaderAndCreateMaterial(shader);
				}
				return m_Material;
			}
		}

		private void OnEnable()
		{
			if (!ImageEffectHelper.IsSupported(shader, false, false, this))
			{
				base.enabled = false;
			}
			m_RTU = new RenderTextureUtility();
		}

		private void OnDisable()
		{
			if (m_Material != null)
			{
				UnityEngine.Object.DestroyImmediate(m_Material);
			}
			m_Material = null;
			m_RTU.ReleaseAllTemporaryRenderTextures();
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (!vignette.enabled && !chromaticAberration.enabled && !distortion.enabled)
			{
				Graphics.Blit(source, destination);
				return;
			}
			material.shaderKeywords = null;
			if (distortion.enabled)
			{
				float val = 1.6f * Math.Max(Mathf.Abs(distortion.amount), 1f);
				float num = (float)Math.PI / 180f * Math.Min(160f, val);
				float y = 2f * Mathf.Tan(num * 0.5f);
				Vector4 value = new Vector4(distortion.centerX, distortion.centerY, Mathf.Max(distortion.amountX, 0.0001f), Mathf.Max(distortion.amountY, 0.0001f));
				Vector3 vector = new Vector3((!(distortion.amount >= 0f)) ? (1f / num) : num, y, 1f / distortion.scale);
				material.EnableKeyword((!(distortion.amount >= 0f)) ? "UNDISTORT" : "DISTORT");
				material.SetVector("_DistCenterScale", value);
				material.SetVector("_DistAmount", vector);
			}
			if (chromaticAberration.enabled)
			{
				material.EnableKeyword("CHROMATIC_ABERRATION");
				Vector4 value2 = new Vector4(chromaticAberration.color.r, chromaticAberration.color.g, chromaticAberration.color.b, chromaticAberration.amount * 0.001f);
				material.SetVector("_ChromaticAberration", value2);
			}
			if (vignette.enabled)
			{
				material.SetColor("_VignetteColor", vignette.color);
				if (vignette.blur > 0f)
				{
					int num2 = source.width / 2;
					int num3 = source.height / 2;
					RenderTexture temporaryRenderTexture = m_RTU.GetTemporaryRenderTexture(num2, num3, 0, source.format);
					RenderTexture temporaryRenderTexture2 = m_RTU.GetTemporaryRenderTexture(num2, num3, 0, source.format);
					material.SetVector("_BlurPass", new Vector2(1f / (float)num2, 0f));
					Graphics.Blit(source, temporaryRenderTexture, material, 0);
					if (distortion.enabled)
					{
						material.DisableKeyword("DISTORT");
						material.DisableKeyword("UNDISTORT");
					}
					material.SetVector("_BlurPass", new Vector2(0f, 1f / (float)num3));
					Graphics.Blit(temporaryRenderTexture, temporaryRenderTexture2, material, 0);
					material.SetVector("_BlurPass", new Vector2(1f / (float)num2, 0f));
					Graphics.Blit(temporaryRenderTexture2, temporaryRenderTexture, material, 0);
					material.SetVector("_BlurPass", new Vector2(0f, 1f / (float)num3));
					Graphics.Blit(temporaryRenderTexture, temporaryRenderTexture2, material, 0);
					material.SetTexture("_BlurTex", temporaryRenderTexture2);
					material.SetFloat("_VignetteBlur", vignette.blur * 3f);
					material.EnableKeyword("VIGNETTE_BLUR");
					if (distortion.enabled)
					{
						material.EnableKeyword((!(distortion.amount >= 0f)) ? "UNDISTORT" : "DISTORT");
					}
				}
				if (vignette.desaturate > 0f)
				{
					material.EnableKeyword("VIGNETTE_DESAT");
					material.SetFloat("_VignetteDesat", 1f - vignette.desaturate);
				}
				material.SetVector("_VignetteCenter", vignette.center);
				if (Mathf.Approximately(vignette.roundness, 1f))
				{
					material.EnableKeyword("VIGNETTE_CLASSIC");
					material.SetVector("_VignetteSettings", new Vector2(vignette.intensity, vignette.smoothness));
				}
				else
				{
					material.EnableKeyword("VIGNETTE_FILMIC");
					float z = (1f - vignette.roundness) * 6f + vignette.roundness;
					material.SetVector("_VignetteSettings", new Vector3(vignette.intensity, vignette.smoothness, z));
				}
			}
			int pass = 0;
			if (vignette.enabled && chromaticAberration.enabled && distortion.enabled)
			{
				pass = 7;
			}
			else if (vignette.enabled && chromaticAberration.enabled)
			{
				pass = 5;
			}
			else if (vignette.enabled && distortion.enabled)
			{
				pass = 6;
			}
			else if (chromaticAberration.enabled && distortion.enabled)
			{
				pass = 4;
			}
			else if (vignette.enabled)
			{
				pass = 3;
			}
			else if (chromaticAberration.enabled)
			{
				pass = 1;
			}
			else if (distortion.enabled)
			{
				pass = 2;
			}
			Graphics.Blit(source, destination, material, pass);
			m_RTU.ReleaseAllTemporaryRenderTextures();
		}
	}
}
