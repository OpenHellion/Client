using System;
using UnityEngine;

namespace UnityStandardAssets.CinematicEffects
{
	[ExecuteInEditMode]
	[AddComponentMenu("Image Effects/Cinematic/Depth Of Field")]
	[RequireComponent(typeof(Camera))]
	public class DepthOfField : MonoBehaviour
	{
		private enum Passes
		{
			BlurAlphaWeighted = 0,
			BoxBlur = 1,
			DilateFgCocFromColor = 2,
			DilateFgCoc = 3,
			CaptureCocExplicit = 4,
			VisualizeCocExplicit = 5,
			CocPrefilter = 6,
			CircleBlur = 7,
			CircleBlurWithDilatedFg = 8,
			CircleBlurLowQuality = 9,
			CircleBlowLowQualityWithDilatedFg = 10,
			MergeExplicit = 11,
			ShapeLowQuality = 12,
			ShapeLowQualityDilateFg = 13,
			ShapeLowQualityMerge = 14,
			ShapeLowQualityMergeDilateFg = 15,
			ShapeMediumQuality = 16,
			ShapeMediumQualityDilateFg = 17,
			ShapeMediumQualityMerge = 18,
			ShapeMediumQualityMergeDilateFg = 19,
			ShapeHighQuality = 20,
			ShapeHighQualityDilateFg = 21,
			ShapeHighQualityMerge = 22,
			ShapeHighQualityMergeDilateFg = 23
		}

		private enum MedianPasses
		{
			Median3 = 0,
			Median3X3 = 1
		}

		private enum BokehTexturesPasses
		{
			Apply = 0,
			Collect = 1
		}

		public enum TweakMode
		{
			Range = 0,
			Explicit = 1
		}

		public enum ApertureShape
		{
			Circular = 0,
			Hexagonal = 1,
			Octogonal = 2
		}

		public enum QualityPreset
		{
			Low = 0,
			Medium = 1,
			High = 2
		}

		public enum FilterQuality
		{
			None = 0,
			Normal = 1,
			High = 2
		}

		[Serializable]
		public struct GlobalSettings
		{
			[Tooltip("Allows to view where the blur will be applied. Yellow for near blur, blue for far blur.")]
			public bool visualizeFocus;

			[Tooltip("Setup mode. Use \"Advanced\" if you need more control on blur settings and/or want to use a bokeh texture. \"Explicit\" is the same as \"Advanced\" but makes use of \"Near Plane\" and \"Far Plane\" values instead of \"F-Stop\".")]
			public TweakMode tweakMode;

			[Tooltip("Quality presets. Use \"Custom\" for more advanced settings.")]
			public QualityPreset filteringQuality;

			[Tooltip("\"Circular\" is the fastest, followed by \"Hexagonal\" and \"Octogonal\".")]
			public ApertureShape apertureShape;

			[Range(0f, 179f)]
			[Tooltip("Rotates the aperture when working with \"Hexagonal\" and \"Ortogonal\".")]
			public float apertureOrientation;

			public static GlobalSettings defaultSettings
			{
				get
				{
					GlobalSettings result = default(GlobalSettings);
					result.visualizeFocus = false;
					result.tweakMode = TweakMode.Range;
					result.filteringQuality = QualityPreset.High;
					result.apertureShape = ApertureShape.Circular;
					result.apertureOrientation = 0f;
					return result;
				}
			}
		}

		[Serializable]
		public struct QualitySettings
		{
			[Tooltip("Enable this to get smooth bokeh.")]
			public bool prefilterBlur;

			[Tooltip("Applies a median filter for even smoother bokeh.")]
			public FilterQuality medianFilter;

			[Tooltip("Dilates near blur over in focus area.")]
			public bool dilateNearBlur;

			public static QualitySettings[] presetQualitySettings = new QualitySettings[3]
			{
				new QualitySettings
				{
					prefilterBlur = false,
					medianFilter = FilterQuality.None,
					dilateNearBlur = false
				},
				new QualitySettings
				{
					prefilterBlur = true,
					medianFilter = FilterQuality.Normal,
					dilateNearBlur = false
				},
				new QualitySettings
				{
					prefilterBlur = true,
					medianFilter = FilterQuality.High,
					dilateNearBlur = true
				}
			};
		}

		[Serializable]
		public struct FocusSettings
		{
			[Tooltip("Auto-focus on a selected transform.")]
			public Transform transform;

			[Min(0f)]
			[Tooltip("Focus distance (in world units).")]
			public float focusPlane;

			[Min(0.1f)]
			[Tooltip("Focus range (in world units). The focus plane is located in the center of the range.")]
			public float range;

			[Min(0f)]
			[Tooltip("Near focus distance (in world units).")]
			public float nearPlane;

			[Min(0f)]
			[Tooltip("Near blur falloff (in world units).")]
			public float nearFalloff;

			[Min(0f)]
			[Tooltip("Far focus distance (in world units).")]
			public float farPlane;

			[Min(0f)]
			[Tooltip("Far blur falloff (in world units).")]
			public float farFalloff;

			[Range(0f, 40f)]
			[Tooltip("Maximum blur radius for the near plane.")]
			public float nearBlurRadius;

			[Range(0f, 40f)]
			[Tooltip("Maximum blur radius for the far plane.")]
			public float farBlurRadius;

			public static FocusSettings defaultSettings
			{
				get
				{
					FocusSettings result = default(FocusSettings);
					result.transform = null;
					result.focusPlane = 20f;
					result.range = 35f;
					result.nearPlane = 2.5f;
					result.nearFalloff = 15f;
					result.farPlane = 37.5f;
					result.farFalloff = 50f;
					result.nearBlurRadius = 15f;
					result.farBlurRadius = 20f;
					return result;
				}
			}
		}

		[Serializable]
		public struct BokehTextureSettings
		{
			[Tooltip("Adding a texture to this field will enable the use of \"Bokeh Textures\". Use with care. This feature is only available on Shader Model 5 compatible-hardware and performance scale with the amount of bokeh.")]
			public Texture2D texture;

			[Range(0.01f, 10f)]
			[Tooltip("Maximum size of bokeh textures on screen.")]
			public float scale;

			[Range(0.01f, 100f)]
			[Tooltip("Bokeh brightness.")]
			public float intensity;

			[Range(0.01f, 5f)]
			[Tooltip("Controls the amount of bokeh textures. Lower values mean more bokeh splats.")]
			public float threshold;

			[Range(0.01f, 1f)]
			[Tooltip("Controls the spawn conditions. Lower values mean more visible bokeh.")]
			public float spawnHeuristic;

			public static BokehTextureSettings defaultSettings
			{
				get
				{
					BokehTextureSettings result = default(BokehTextureSettings);
					result.texture = null;
					result.scale = 1f;
					result.intensity = 50f;
					result.threshold = 2f;
					result.spawnHeuristic = 0.15f;
					return result;
				}
			}
		}

		private const float kMaxBlur = 40f;

		public GlobalSettings settings = GlobalSettings.defaultSettings;

		public FocusSettings focus = FocusSettings.defaultSettings;

		public BokehTextureSettings bokehTexture = BokehTextureSettings.defaultSettings;

		[SerializeField]
		private Shader m_FilmicDepthOfFieldShader;

		[SerializeField]
		private Shader m_MedianFilterShader;

		[SerializeField]
		private Shader m_TextureBokehShader;

		private RenderTextureUtility m_RTU = new RenderTextureUtility();

		private Material m_FilmicDepthOfFieldMaterial;

		private Material m_MedianFilterMaterial;

		private Material m_TextureBokehMaterial;

		private ComputeBuffer m_ComputeBufferDrawArgs;

		private ComputeBuffer m_ComputeBufferPoints;

		private QualitySettings m_CurrentQualitySettings;

		private float m_LastApertureOrientation;

		private Vector4 m_OctogonalBokehDirection1;

		private Vector4 m_OctogonalBokehDirection2;

		private Vector4 m_OctogonalBokehDirection3;

		private Vector4 m_OctogonalBokehDirection4;

		private Vector4 m_HexagonalBokehDirection1;

		private Vector4 m_HexagonalBokehDirection2;

		private Vector4 m_HexagonalBokehDirection3;

		public Shader filmicDepthOfFieldShader
		{
			get
			{
				if (m_FilmicDepthOfFieldShader == null)
				{
					m_FilmicDepthOfFieldShader = Shader.Find("Hidden/DepthOfField/DepthOfField");
				}
				return m_FilmicDepthOfFieldShader;
			}
		}

		public Shader medianFilterShader
		{
			get
			{
				if (m_MedianFilterShader == null)
				{
					m_MedianFilterShader = Shader.Find("Hidden/DepthOfField/MedianFilter");
				}
				return m_MedianFilterShader;
			}
		}

		public Shader textureBokehShader
		{
			get
			{
				if (m_TextureBokehShader == null)
				{
					m_TextureBokehShader = Shader.Find("Hidden/DepthOfField/BokehSplatting");
				}
				return m_TextureBokehShader;
			}
		}

		public Material filmicDepthOfFieldMaterial
		{
			get
			{
				if (m_FilmicDepthOfFieldMaterial == null)
				{
					m_FilmicDepthOfFieldMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(filmicDepthOfFieldShader);
				}
				return m_FilmicDepthOfFieldMaterial;
			}
		}

		public Material medianFilterMaterial
		{
			get
			{
				if (m_MedianFilterMaterial == null)
				{
					m_MedianFilterMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(medianFilterShader);
				}
				return m_MedianFilterMaterial;
			}
		}

		public Material textureBokehMaterial
		{
			get
			{
				if (m_TextureBokehMaterial == null)
				{
					m_TextureBokehMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(textureBokehShader);
				}
				return m_TextureBokehMaterial;
			}
		}

		public ComputeBuffer computeBufferDrawArgs
		{
			get
			{
				if (m_ComputeBufferDrawArgs == null)
				{
					m_ComputeBufferDrawArgs = new ComputeBuffer(1, 16, ComputeBufferType.IndirectArguments);
					m_ComputeBufferDrawArgs.SetData(new int[4] { 0, 1, 0, 0 });
				}
				return m_ComputeBufferDrawArgs;
			}
		}

		public ComputeBuffer computeBufferPoints
		{
			get
			{
				if (m_ComputeBufferPoints == null)
				{
					m_ComputeBufferPoints = new ComputeBuffer(90000, 28, ComputeBufferType.Append);
				}
				return m_ComputeBufferPoints;
			}
		}

		private bool shouldPerformBokeh
		{
			get
			{
				return ImageEffectHelper.supportsDX11 && bokehTexture.texture != null && (bool)textureBokehMaterial;
			}
		}

		private void OnEnable()
		{
			if (!ImageEffectHelper.IsSupported(filmicDepthOfFieldShader, true, true, this) || !ImageEffectHelper.IsSupported(medianFilterShader, true, true, this))
			{
				base.enabled = false;
				return;
			}
			if (ImageEffectHelper.supportsDX11 && !ImageEffectHelper.IsSupported(textureBokehShader, true, true, this))
			{
				base.enabled = false;
				return;
			}
			ComputeBlurDirections(true);
			GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
		}

		private void OnDisable()
		{
			ReleaseComputeResources();
			if (m_FilmicDepthOfFieldMaterial != null)
			{
				UnityEngine.Object.DestroyImmediate(m_FilmicDepthOfFieldMaterial);
			}
			if (m_TextureBokehMaterial != null)
			{
				UnityEngine.Object.DestroyImmediate(m_TextureBokehMaterial);
			}
			if (m_MedianFilterMaterial != null)
			{
				UnityEngine.Object.DestroyImmediate(m_MedianFilterMaterial);
			}
			m_FilmicDepthOfFieldMaterial = null;
			m_TextureBokehMaterial = null;
			m_MedianFilterMaterial = null;
			m_RTU.ReleaseAllTemporaryRenderTextures();
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (medianFilterMaterial == null || filmicDepthOfFieldMaterial == null)
			{
				Graphics.Blit(source, destination);
				return;
			}
			if (settings.visualizeFocus)
			{
				Vector4 blurParams;
				Vector4 blurCoe;
				ComputeCocParameters(out blurParams, out blurCoe);
				filmicDepthOfFieldMaterial.SetVector("_BlurParams", blurParams);
				filmicDepthOfFieldMaterial.SetVector("_BlurCoe", blurCoe);
				Graphics.Blit(null, destination, filmicDepthOfFieldMaterial, 5);
			}
			else
			{
				DoDepthOfField(source, destination);
			}
			m_RTU.ReleaseAllTemporaryRenderTextures();
		}

		private void DoDepthOfField(RenderTexture source, RenderTexture destination)
		{
			m_CurrentQualitySettings = QualitySettings.presetQualitySettings[(int)settings.filteringQuality];
			float num = (float)source.height / 720f;
			float num2 = num;
			float num3 = Mathf.Max(focus.nearBlurRadius, focus.farBlurRadius) * num2 * 0.75f;
			float num4 = focus.nearBlurRadius * num;
			float num5 = focus.farBlurRadius * num;
			float num6 = Mathf.Max(num4, num5);
			switch (settings.apertureShape)
			{
			case ApertureShape.Hexagonal:
				num6 *= 1.2f;
				break;
			case ApertureShape.Octogonal:
				num6 *= 1.15f;
				break;
			}
			if (num6 < 0.5f)
			{
				Graphics.Blit(source, destination);
				return;
			}
			int width = source.width / 2;
			int height = source.height / 2;
			Vector4 value = new Vector4(num4 * 0.5f, num5 * 0.5f, 0f, 0f);
			RenderTexture temporaryRenderTexture = m_RTU.GetTemporaryRenderTexture(width, height);
			RenderTexture temporaryRenderTexture2 = m_RTU.GetTemporaryRenderTexture(width, height);
			Vector4 blurParams;
			Vector4 blurCoe;
			ComputeCocParameters(out blurParams, out blurCoe);
			filmicDepthOfFieldMaterial.SetVector("_BlurParams", blurParams);
			filmicDepthOfFieldMaterial.SetVector("_BlurCoe", blurCoe);
			Graphics.Blit(source, temporaryRenderTexture2, filmicDepthOfFieldMaterial, 4);
			RenderTexture src = temporaryRenderTexture2;
			RenderTexture dst = temporaryRenderTexture;
			if (shouldPerformBokeh)
			{
				RenderTexture temporaryRenderTexture3 = m_RTU.GetTemporaryRenderTexture(width, height);
				Graphics.Blit(src, temporaryRenderTexture3, filmicDepthOfFieldMaterial, 1);
				filmicDepthOfFieldMaterial.SetVector("_Offsets", new Vector4(0f, 1.5f, 0f, 1.5f));
				Graphics.Blit(temporaryRenderTexture3, dst, filmicDepthOfFieldMaterial, 0);
				filmicDepthOfFieldMaterial.SetVector("_Offsets", new Vector4(1.5f, 0f, 0f, 1.5f));
				Graphics.Blit(dst, temporaryRenderTexture3, filmicDepthOfFieldMaterial, 0);
				textureBokehMaterial.SetTexture("_BlurredColor", temporaryRenderTexture3);
				textureBokehMaterial.SetFloat("_SpawnHeuristic", bokehTexture.spawnHeuristic);
				textureBokehMaterial.SetVector("_BokehParams", new Vector4(bokehTexture.scale * num2, bokehTexture.intensity, bokehTexture.threshold, num3));
				Graphics.SetRandomWriteTarget(1, computeBufferPoints);
				Graphics.Blit(src, dst, textureBokehMaterial, 1);
				Graphics.ClearRandomWriteTargets();
				SwapRenderTexture(ref src, ref dst);
				m_RTU.ReleaseTemporaryRenderTexture(temporaryRenderTexture3);
			}
			filmicDepthOfFieldMaterial.SetVector("_BlurParams", blurParams);
			filmicDepthOfFieldMaterial.SetVector("_BlurCoe", value);
			RenderTexture renderTexture = null;
			if (m_CurrentQualitySettings.dilateNearBlur)
			{
				RenderTexture temporaryRenderTexture4 = m_RTU.GetTemporaryRenderTexture(width, height, 0, RenderTextureFormat.RGHalf);
				renderTexture = m_RTU.GetTemporaryRenderTexture(width, height, 0, RenderTextureFormat.RGHalf);
				filmicDepthOfFieldMaterial.SetVector("_Offsets", new Vector4(0f, num4 * 0.75f, 0f, 0f));
				Graphics.Blit(src, temporaryRenderTexture4, filmicDepthOfFieldMaterial, 2);
				filmicDepthOfFieldMaterial.SetVector("_Offsets", new Vector4(num4 * 0.75f, 0f, 0f, 0f));
				Graphics.Blit(temporaryRenderTexture4, renderTexture, filmicDepthOfFieldMaterial, 3);
				m_RTU.ReleaseTemporaryRenderTexture(temporaryRenderTexture4);
				renderTexture.filterMode = FilterMode.Point;
			}
			if (m_CurrentQualitySettings.prefilterBlur)
			{
				Graphics.Blit(src, dst, filmicDepthOfFieldMaterial, 6);
				SwapRenderTexture(ref src, ref dst);
			}
			switch (settings.apertureShape)
			{
			case ApertureShape.Circular:
				DoCircularBlur(renderTexture, ref src, ref dst, num6);
				break;
			case ApertureShape.Hexagonal:
				DoHexagonalBlur(renderTexture, ref src, ref dst, num6);
				break;
			case ApertureShape.Octogonal:
				DoOctogonalBlur(renderTexture, ref src, ref dst, num6);
				break;
			}
			switch (m_CurrentQualitySettings.medianFilter)
			{
			case FilterQuality.Normal:
				medianFilterMaterial.SetVector("_Offsets", new Vector4(1f, 0f, 0f, 0f));
				Graphics.Blit(src, dst, medianFilterMaterial, 0);
				SwapRenderTexture(ref src, ref dst);
				medianFilterMaterial.SetVector("_Offsets", new Vector4(0f, 1f, 0f, 0f));
				Graphics.Blit(src, dst, medianFilterMaterial, 0);
				SwapRenderTexture(ref src, ref dst);
				break;
			case FilterQuality.High:
				Graphics.Blit(src, dst, medianFilterMaterial, 1);
				SwapRenderTexture(ref src, ref dst);
				break;
			}
			filmicDepthOfFieldMaterial.SetVector("_BlurCoe", value);
			filmicDepthOfFieldMaterial.SetVector("_Convolved_TexelSize", new Vector4(src.width, src.height, 1f / (float)src.width, 1f / (float)src.height));
			filmicDepthOfFieldMaterial.SetTexture("_SecondTex", src);
			int pass = 11;
			if (shouldPerformBokeh)
			{
				RenderTexture temporaryRenderTexture5 = m_RTU.GetTemporaryRenderTexture(source.height, source.width, 0, source.format);
				Graphics.Blit(source, temporaryRenderTexture5, filmicDepthOfFieldMaterial, pass);
				Graphics.SetRenderTarget(temporaryRenderTexture5);
				ComputeBuffer.CopyCount(computeBufferPoints, computeBufferDrawArgs, 0);
				textureBokehMaterial.SetBuffer("pointBuffer", computeBufferPoints);
				textureBokehMaterial.SetTexture("_MainTex", bokehTexture.texture);
				textureBokehMaterial.SetVector("_Screen", new Vector3(1f / (1f * (float)source.width), 1f / (1f * (float)source.height), num3));
				textureBokehMaterial.SetPass(0);
				Graphics.DrawProceduralIndirectNow(MeshTopology.Points, computeBufferDrawArgs, 0);
				Graphics.Blit(temporaryRenderTexture5, destination);
			}
			else
			{
				Graphics.Blit(source, destination, filmicDepthOfFieldMaterial, pass);
			}
		}

		private void DoHexagonalBlur(RenderTexture blurredFgCoc, ref RenderTexture src, ref RenderTexture dst, float maxRadius)
		{
			ComputeBlurDirections(false);
			int blurPass;
			int blurAndMergePass;
			GetDirectionalBlurPassesFromRadius(blurredFgCoc, maxRadius, out blurPass, out blurAndMergePass);
			filmicDepthOfFieldMaterial.SetTexture("_SecondTex", blurredFgCoc);
			RenderTexture temporaryRenderTexture = m_RTU.GetTemporaryRenderTexture(src.width, src.height, 0, src.format);
			filmicDepthOfFieldMaterial.SetVector("_Offsets", m_HexagonalBokehDirection1);
			Graphics.Blit(src, temporaryRenderTexture, filmicDepthOfFieldMaterial, blurPass);
			filmicDepthOfFieldMaterial.SetVector("_Offsets", m_HexagonalBokehDirection2);
			Graphics.Blit(temporaryRenderTexture, src, filmicDepthOfFieldMaterial, blurPass);
			filmicDepthOfFieldMaterial.SetVector("_Offsets", m_HexagonalBokehDirection3);
			filmicDepthOfFieldMaterial.SetTexture("_ThirdTex", src);
			Graphics.Blit(temporaryRenderTexture, dst, filmicDepthOfFieldMaterial, blurAndMergePass);
			m_RTU.ReleaseTemporaryRenderTexture(temporaryRenderTexture);
			SwapRenderTexture(ref src, ref dst);
		}

		private void DoOctogonalBlur(RenderTexture blurredFgCoc, ref RenderTexture src, ref RenderTexture dst, float maxRadius)
		{
			ComputeBlurDirections(false);
			int blurPass;
			int blurAndMergePass;
			GetDirectionalBlurPassesFromRadius(blurredFgCoc, maxRadius, out blurPass, out blurAndMergePass);
			filmicDepthOfFieldMaterial.SetTexture("_SecondTex", blurredFgCoc);
			RenderTexture temporaryRenderTexture = m_RTU.GetTemporaryRenderTexture(src.width, src.height, 0, src.format);
			filmicDepthOfFieldMaterial.SetVector("_Offsets", m_OctogonalBokehDirection1);
			Graphics.Blit(src, temporaryRenderTexture, filmicDepthOfFieldMaterial, blurPass);
			filmicDepthOfFieldMaterial.SetVector("_Offsets", m_OctogonalBokehDirection2);
			Graphics.Blit(temporaryRenderTexture, dst, filmicDepthOfFieldMaterial, blurPass);
			filmicDepthOfFieldMaterial.SetVector("_Offsets", m_OctogonalBokehDirection3);
			Graphics.Blit(src, temporaryRenderTexture, filmicDepthOfFieldMaterial, blurPass);
			filmicDepthOfFieldMaterial.SetVector("_Offsets", m_OctogonalBokehDirection4);
			filmicDepthOfFieldMaterial.SetTexture("_ThirdTex", dst);
			Graphics.Blit(temporaryRenderTexture, src, filmicDepthOfFieldMaterial, blurAndMergePass);
			m_RTU.ReleaseTemporaryRenderTexture(temporaryRenderTexture);
		}

		private void DoCircularBlur(RenderTexture blurredFgCoc, ref RenderTexture src, ref RenderTexture dst, float maxRadius)
		{
			int pass;
			if (blurredFgCoc != null)
			{
				filmicDepthOfFieldMaterial.SetTexture("_SecondTex", blurredFgCoc);
				pass = ((!(maxRadius > 10f)) ? 10 : 8);
			}
			else
			{
				pass = ((!(maxRadius > 10f)) ? 9 : 7);
			}
			Graphics.Blit(src, dst, filmicDepthOfFieldMaterial, pass);
			SwapRenderTexture(ref src, ref dst);
		}

		private void ComputeCocParameters(out Vector4 blurParams, out Vector4 blurCoe)
		{
			Camera component = GetComponent<Camera>();
			float num = focus.nearFalloff * 2f;
			float num2 = focus.farFalloff * 2f;
			float num3 = focus.nearPlane;
			float num4 = focus.farPlane;
			float num5;
			if (settings.tweakMode == TweakMode.Range)
			{
				num5 = ((!(focus.transform != null)) ? focus.focusPlane : component.WorldToViewportPoint(focus.transform.position).z);
				float num6 = focus.range * 0.5f;
				num3 = num5 - num6;
				num4 = num5 + num6;
			}
			num3 -= num * 0.5f;
			num4 += num2 * 0.5f;
			num5 = (num3 + num4) * 0.5f;
			float num7 = num5 / component.farClipPlane;
			float num8 = num3 / component.farClipPlane;
			float num9 = num4 / component.farClipPlane;
			float num10 = num4 - num3;
			float num11 = num9 - num8;
			float num12 = num / num10;
			float num13 = num2 / num10;
			float num14 = (1f - num12) * (num11 * 0.5f);
			float num15 = (1f - num13) * (num11 * 0.5f);
			if (num7 <= num8)
			{
				num7 = num8 + 1E-06f;
			}
			if (num7 >= num9)
			{
				num7 = num9 - 1E-06f;
			}
			if (num7 - num14 <= num8)
			{
				num14 = num7 - num8 - 1E-06f;
			}
			if (num7 + num15 >= num9)
			{
				num15 = num9 - num7 - 1E-06f;
			}
			float num16 = 1f / (num8 - num7 + num14);
			float num17 = 1f / (num9 - num7 - num15);
			float num18 = 1f - num16 * num8;
			float num19 = 1f - num17 * num9;
			blurParams = new Vector4(-1f * num16, -1f * num18, 1f * num17, 1f * num19);
			blurCoe = new Vector4(0f, 0f, (num19 - num18) / (num16 - num17), 0f);
			focus.nearPlane = num3 + num * 0.5f;
			focus.farPlane = num4 - num2 * 0.5f;
			focus.focusPlane = (focus.nearPlane + focus.farPlane) * 0.5f;
			focus.range = focus.farPlane - focus.nearPlane;
		}

		private void ReleaseComputeResources()
		{
			if (m_ComputeBufferDrawArgs != null)
			{
				m_ComputeBufferDrawArgs.Release();
			}
			if (m_ComputeBufferPoints != null)
			{
				m_ComputeBufferPoints.Release();
			}
			m_ComputeBufferDrawArgs = null;
			m_ComputeBufferPoints = null;
		}

		private void ComputeBlurDirections(bool force)
		{
			if (force || !(Math.Abs(m_LastApertureOrientation - settings.apertureOrientation) < float.Epsilon))
			{
				m_LastApertureOrientation = settings.apertureOrientation;
				float num = settings.apertureOrientation * ((float)Math.PI / 180f);
				float cosinus = Mathf.Cos(num);
				float sinus = Mathf.Sin(num);
				m_OctogonalBokehDirection1 = new Vector4(0.5f, 0f, 0f, 0f);
				m_OctogonalBokehDirection2 = new Vector4(0f, 0.5f, 1f, 0f);
				m_OctogonalBokehDirection3 = new Vector4(-0.353553f, 0.353553f, 1f, 0f);
				m_OctogonalBokehDirection4 = new Vector4(0.353553f, 0.353553f, 1f, 0f);
				m_HexagonalBokehDirection1 = new Vector4(0.5f, 0f, 0f, 0f);
				m_HexagonalBokehDirection2 = new Vector4(0.25f, 0.433013f, 1f, 0f);
				m_HexagonalBokehDirection3 = new Vector4(0.25f, -0.433013f, 1f, 0f);
				if (num > float.Epsilon)
				{
					Rotate2D(ref m_OctogonalBokehDirection1, cosinus, sinus);
					Rotate2D(ref m_OctogonalBokehDirection2, cosinus, sinus);
					Rotate2D(ref m_OctogonalBokehDirection3, cosinus, sinus);
					Rotate2D(ref m_OctogonalBokehDirection4, cosinus, sinus);
					Rotate2D(ref m_HexagonalBokehDirection1, cosinus, sinus);
					Rotate2D(ref m_HexagonalBokehDirection2, cosinus, sinus);
					Rotate2D(ref m_HexagonalBokehDirection3, cosinus, sinus);
				}
			}
		}

		private static void Rotate2D(ref Vector4 direction, float cosinus, float sinus)
		{
			Vector4 vector = direction;
			direction.x = vector.x * cosinus - vector.y * sinus;
			direction.y = vector.x * sinus + vector.y * cosinus;
		}

		private static void SwapRenderTexture(ref RenderTexture src, ref RenderTexture dst)
		{
			RenderTexture renderTexture = dst;
			dst = src;
			src = renderTexture;
		}

		private static void GetDirectionalBlurPassesFromRadius(RenderTexture blurredFgCoc, float maxRadius, out int blurPass, out int blurAndMergePass)
		{
			if (blurredFgCoc == null)
			{
				if (maxRadius > 10f)
				{
					blurPass = 20;
					blurAndMergePass = 22;
				}
				else if (maxRadius > 5f)
				{
					blurPass = 16;
					blurAndMergePass = 18;
				}
				else
				{
					blurPass = 12;
					blurAndMergePass = 14;
				}
			}
			else if (maxRadius > 10f)
			{
				blurPass = 21;
				blurAndMergePass = 23;
			}
			else if (maxRadius > 5f)
			{
				blurPass = 17;
				blurAndMergePass = 19;
			}
			else
			{
				blurPass = 13;
				blurAndMergePass = 15;
			}
		}
	}
}
