using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityStandardAssets.CinematicEffects
{
	[ExecuteInEditMode]
	[ImageEffectAllowedInSceneView]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Cinematic Image Effects/Screen Space Reflections")]
	public class ScreenSpaceReflection : MonoBehaviour
	{
		public enum SSRResolution
		{
			High = 0,
			Low = 2
		}

		public enum SSRReflectionBlendType
		{
			PhysicallyBased = 0,
			Additive = 1
		}

		[Serializable]
		public struct SSRSettings
		{
			[AttributeUsage(AttributeTargets.Field)]
			public class LayoutAttribute : PropertyAttribute
			{
			}

			[Layout]
			public ReflectionSettings reflectionSettings;

			[Layout]
			public IntensitySettings intensitySettings;

			[Layout]
			public ScreenEdgeMask screenEdgeMask;

			private static readonly SSRSettings s_Default = new SSRSettings
			{
				reflectionSettings = new ReflectionSettings
				{
					blendType = SSRReflectionBlendType.PhysicallyBased,
					reflectionQuality = SSRResolution.High,
					maxDistance = 100f,
					iterationCount = 256,
					stepSize = 3,
					widthModifier = 0.5f,
					reflectionBlur = 1f,
					reflectBackfaces = true
				},
				intensitySettings = new IntensitySettings
				{
					reflectionMultiplier = 1f,
					fadeDistance = 100f,
					fresnelFade = 1f,
					fresnelFadePower = 1f
				},
				screenEdgeMask = new ScreenEdgeMask
				{
					intensity = 0.03f
				}
			};

			public static SSRSettings defaultSettings
			{
				get
				{
					return s_Default;
				}
			}
		}

		[Serializable]
		public struct IntensitySettings
		{
			[Tooltip("Nonphysical multiplier for the SSR reflections. 1.0 is physically based.")]
			[Range(0f, 2f)]
			public float reflectionMultiplier;

			[Tooltip("How far away from the maxDistance to begin fading SSR.")]
			[Range(0f, 1000f)]
			public float fadeDistance;

			[Tooltip("Amplify Fresnel fade out. Increase if floor reflections look good close to the surface and bad farther 'under' the floor.")]
			[Range(0f, 1f)]
			public float fresnelFade;

			[Tooltip("Higher values correspond to a faster Fresnel fade as the reflection changes from the grazing angle.")]
			[Range(0.1f, 10f)]
			public float fresnelFadePower;
		}

		[Serializable]
		public struct ReflectionSettings
		{
			[Tooltip("How the reflections are blended into the render.")]
			public SSRReflectionBlendType blendType;

			[Tooltip("Half resolution SSRR is much faster, but less accurate.")]
			public SSRResolution reflectionQuality;

			[Tooltip("Maximum reflection distance in world units.")]
			[Range(0.1f, 300f)]
			public float maxDistance;

			[Tooltip("Max raytracing length.")]
			[Range(16f, 1024f)]
			public int iterationCount;

			[Tooltip("Log base 2 of ray tracing coarse step size. Higher traces farther, lower gives better quality silhouettes.")]
			[Range(1f, 16f)]
			public int stepSize;

			[Tooltip("Typical thickness of columns, walls, furniture, and other objects that reflection rays might pass behind.")]
			[Range(0.01f, 10f)]
			public float widthModifier;

			[Tooltip("Blurriness of reflections.")]
			[Range(0.1f, 8f)]
			public float reflectionBlur;

			[Tooltip("Enable for a performance gain in scenes where most glossy objects are horizontal, like floors, water, and tables. Leave on for scenes with glossy vertical objects.")]
			public bool reflectBackfaces;
		}

		[Serializable]
		public struct ScreenEdgeMask
		{
			[Tooltip("Higher = fade out SSRR near the edge of the screen so that reflections don't pop under camera motion.")]
			[Range(0f, 1f)]
			public float intensity;
		}

		private enum PassIndex
		{
			RayTraceStep = 0,
			CompositeFinal = 1,
			Blur = 2,
			CompositeSSR = 3,
			MinMipGeneration = 4,
			HitPointToReflections = 5,
			BilateralKeyPack = 6,
			BlitDepthAsCSZ = 7,
			PoissonBlur = 8
		}

		[SerializeField]
		public SSRSettings settings = SSRSettings.defaultSettings;

		[Tooltip("Enable to limit the effect a few bright pixels can have on rougher surfaces")]
		private bool highlightSuppression;

		[Tooltip("Enable to allow rays to pass behind objects. This can lead to more screen-space reflections, but the reflections are more likely to be wrong.")]
		private bool traceBehindObjects = true;

		[Tooltip("Enable to force more surfaces to use reflection probes if you see streaks on the sides of objects or bad reflections of their backs.")]
		private bool treatBackfaceHitAsMiss;

		[Tooltip("Drastically improves reflection reconstruction quality at the expense of some performance.")]
		private bool bilateralUpsample = true;

		[SerializeField]
		private Shader m_Shader;

		private Material m_Material;

		private Camera m_Camera;

		private CommandBuffer m_CommandBuffer;

		private static int kNormalAndRoughnessTexture;

		private static int kHitPointTexture;

		private static int[] kReflectionTextures;

		private static int kFilteredReflections;

		private static int kBlurTexture;

		private static int kFinalReflectionTexture;

		private static int kTempTexture;

		public Shader shader
		{
			get
			{
				if (m_Shader == null)
				{
					m_Shader = Shader.Find("Hidden/ScreenSpaceReflection");
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

		public Camera camera_
		{
			get
			{
				if (m_Camera == null)
				{
					m_Camera = GetComponent<Camera>();
				}
				return m_Camera;
			}
		}

		private void OnEnable()
		{
			if (!ImageEffectHelper.IsSupported(shader, false, true, this))
			{
				base.enabled = false;
				return;
			}
			camera_.depthTextureMode |= DepthTextureMode.Depth;
			kReflectionTextures = new int[5];
			kNormalAndRoughnessTexture = Shader.PropertyToID("_NormalAndRoughnessTexture");
			kHitPointTexture = Shader.PropertyToID("_HitPointTexture");
			kReflectionTextures[0] = Shader.PropertyToID("_ReflectionTexture0");
			kReflectionTextures[1] = Shader.PropertyToID("_ReflectionTexture1");
			kReflectionTextures[2] = Shader.PropertyToID("_ReflectionTexture2");
			kReflectionTextures[3] = Shader.PropertyToID("_ReflectionTexture3");
			kReflectionTextures[4] = Shader.PropertyToID("_ReflectionTexture4");
			kBlurTexture = Shader.PropertyToID("_BlurTexture");
			kFilteredReflections = Shader.PropertyToID("_FilteredReflections");
			kFinalReflectionTexture = Shader.PropertyToID("_FinalReflectionTexture");
			kTempTexture = Shader.PropertyToID("_TempTexture");
		}

		private void OnDisable()
		{
			if ((bool)m_Material)
			{
				UnityEngine.Object.DestroyImmediate(m_Material);
			}
			m_Material = null;
			if (camera_ != null)
			{
				if (m_CommandBuffer != null)
				{
					camera_.RemoveCommandBuffer(CameraEvent.AfterFinalPass, m_CommandBuffer);
				}
				m_CommandBuffer = null;
			}
		}

		public void OnPreRender()
		{
			if (material == null || Camera.current.actualRenderingPath != RenderingPath.DeferredShading)
			{
				return;
			}
			int num = ((settings.reflectionSettings.reflectionQuality == SSRResolution.High) ? 1 : 2);
			int num2 = camera_.pixelWidth / num;
			int num3 = camera_.pixelHeight / num;
			float num4 = camera_.pixelWidth;
			float num5 = camera_.pixelHeight;
			float num6 = num4 / 2f;
			float num7 = num5 / 2f;
			RenderTextureFormat format = (camera_.allowHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32);
			material.SetInt("_RayStepSize", settings.reflectionSettings.stepSize);
			material.SetInt("_AdditiveReflection", (settings.reflectionSettings.blendType == SSRReflectionBlendType.Additive) ? 1 : 0);
			material.SetInt("_BilateralUpsampling", bilateralUpsample ? 1 : 0);
			material.SetInt("_TreatBackfaceHitAsMiss", treatBackfaceHitAsMiss ? 1 : 0);
			material.SetInt("_AllowBackwardsRays", settings.reflectionSettings.reflectBackfaces ? 1 : 0);
			material.SetInt("_TraceBehindObjects", traceBehindObjects ? 1 : 0);
			material.SetInt("_MaxSteps", settings.reflectionSettings.iterationCount);
			material.SetInt("_FullResolutionFiltering", 0);
			material.SetInt("_HalfResolution", (settings.reflectionSettings.reflectionQuality != 0) ? 1 : 0);
			material.SetInt("_HighlightSuppression", highlightSuppression ? 1 : 0);
			float value = num4 / (-2f * (float)Math.Tan((double)camera_.fieldOfView / 180.0 * Math.PI * 0.5));
			material.SetFloat("_PixelsPerMeterAtOneMeter", value);
			material.SetFloat("_ScreenEdgeFading", settings.screenEdgeMask.intensity);
			material.SetFloat("_ReflectionBlur", settings.reflectionSettings.reflectionBlur);
			material.SetFloat("_MaxRayTraceDistance", settings.reflectionSettings.maxDistance);
			material.SetFloat("_FadeDistance", settings.intensitySettings.fadeDistance);
			material.SetFloat("_LayerThickness", settings.reflectionSettings.widthModifier);
			material.SetFloat("_SSRMultiplier", settings.intensitySettings.reflectionMultiplier);
			material.SetFloat("_FresnelFade", settings.intensitySettings.fresnelFade);
			material.SetFloat("_FresnelFadePower", settings.intensitySettings.fresnelFadePower);
			Matrix4x4 projectionMatrix = camera_.projectionMatrix;
			Vector4 value2 = new Vector4(-2f / (num4 * projectionMatrix[0]), -2f / (num5 * projectionMatrix[5]), (1f - projectionMatrix[2]) / projectionMatrix[0], (1f + projectionMatrix[6]) / projectionMatrix[5]);
			Vector3 vector = ((!float.IsPositiveInfinity(camera_.farClipPlane)) ? new Vector3(camera_.nearClipPlane * camera_.farClipPlane, camera_.nearClipPlane - camera_.farClipPlane, camera_.farClipPlane) : new Vector3(camera_.nearClipPlane, -1f, 1f));
			material.SetVector("_ReflectionBufferSize", new Vector2(num2, num3));
			material.SetVector("_ScreenSize", new Vector2(num4, num5));
			material.SetVector("_InvScreenSize", new Vector2(1f / num4, 1f / num5));
			material.SetVector("_ProjInfo", value2);
			material.SetVector("_CameraClipInfo", vector);
			Matrix4x4 matrix4x = default(Matrix4x4);
			matrix4x.SetRow(0, new Vector4(num6, 0f, 0f, num6));
			matrix4x.SetRow(1, new Vector4(0f, num7, 0f, num7));
			matrix4x.SetRow(2, new Vector4(0f, 0f, 1f, 0f));
			matrix4x.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
			Matrix4x4 value3 = matrix4x * projectionMatrix;
			material.SetMatrix("_ProjectToPixelMatrix", value3);
			material.SetMatrix("_WorldToCameraMatrix", camera_.worldToCameraMatrix);
			material.SetMatrix("_CameraToWorldMatrix", camera_.worldToCameraMatrix.inverse);
			if (m_CommandBuffer == null)
			{
				m_CommandBuffer = new CommandBuffer();
				m_CommandBuffer.name = "Screen Space Reflections";
				m_CommandBuffer.GetTemporaryRT(kNormalAndRoughnessTexture, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
				m_CommandBuffer.GetTemporaryRT(kHitPointTexture, num2, num3, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
				for (int i = 0; i < 5; i++)
				{
					m_CommandBuffer.GetTemporaryRT(kReflectionTextures[i], num2 >> i, num3 >> i, 0, FilterMode.Bilinear, format);
				}
				m_CommandBuffer.GetTemporaryRT(kFilteredReflections, num2, num3, 0, (!bilateralUpsample) ? FilterMode.Bilinear : FilterMode.Point, format);
				m_CommandBuffer.GetTemporaryRT(kFinalReflectionTexture, num2, num3, 0, FilterMode.Point, format);
				m_CommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, kNormalAndRoughnessTexture, material, 6);
				m_CommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, kHitPointTexture, material, 0);
				m_CommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, kFilteredReflections, material, 5);
				m_CommandBuffer.Blit(kFilteredReflections, kReflectionTextures[0], material, 8);
				for (int j = 1; j < 5; j++)
				{
					int num8 = kReflectionTextures[j - 1];
					int num9 = j;
					m_CommandBuffer.GetTemporaryRT(kBlurTexture, num2 >> num9, num3 >> num9, 0, FilterMode.Bilinear, format);
					m_CommandBuffer.SetGlobalVector("_Axis", new Vector4(1f, 0f, 0f, 0f));
					m_CommandBuffer.SetGlobalFloat("_CurrentMipLevel", (float)j - 1f);
					m_CommandBuffer.Blit(num8, kBlurTexture, material, 2);
					m_CommandBuffer.SetGlobalVector("_Axis", new Vector4(0f, 1f, 0f, 0f));
					num8 = kReflectionTextures[j];
					m_CommandBuffer.Blit(kBlurTexture, num8, material, 2);
					m_CommandBuffer.ReleaseTemporaryRT(kBlurTexture);
				}
				m_CommandBuffer.Blit(kReflectionTextures[0], kFinalReflectionTexture, material, 3);
				m_CommandBuffer.GetTemporaryRT(kTempTexture, camera_.pixelWidth, camera_.pixelHeight, 0, FilterMode.Bilinear, format);
				m_CommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, kTempTexture, material, 1);
				m_CommandBuffer.Blit(kTempTexture, BuiltinRenderTextureType.CameraTarget);
				m_CommandBuffer.ReleaseTemporaryRT(kTempTexture);
				camera_.AddCommandBuffer(CameraEvent.AfterFinalPass, m_CommandBuffer);
			}
		}
	}
}
