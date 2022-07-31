using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityStandardAssets.CinematicEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Cinematic/Ambient Occlusion")]
	[ImageEffectAllowedInSceneView]
	public class AmbientOcclusion : MonoBehaviour
	{
		private struct PropertyObserver
		{
			private bool _downsampling;

			private OcclusionSource _occlusionSource;

			private bool _ambientOnly;

			private bool _debug;

			private int _pixelWidth;

			private int _pixelHeight;

			public bool CheckNeedsReset(Settings setting, Camera camera)
			{
				return _downsampling != setting.downsampling || _occlusionSource != setting.occlusionSource || _ambientOnly != setting.ambientOnly || _debug != setting.debug || _pixelWidth != camera.pixelWidth || _pixelHeight != camera.pixelHeight;
			}

			public void Update(Settings setting, Camera camera)
			{
				_downsampling = setting.downsampling;
				_occlusionSource = setting.occlusionSource;
				_ambientOnly = setting.ambientOnly;
				_debug = setting.debug;
				_pixelWidth = camera.pixelWidth;
				_pixelHeight = camera.pixelHeight;
			}
		}

		public enum SampleCount
		{
			Lowest = 0,
			Low = 1,
			Medium = 2,
			High = 3,
			Variable = 4
		}

		public enum OcclusionSource
		{
			DepthTexture = 0,
			DepthNormalsTexture = 1,
			GBuffer = 2
		}

		[Serializable]
		public class Settings
		{
			[SerializeField]
			[Range(0f, 4f)]
			[Tooltip("Degree of darkness produced by the effect.")]
			public float intensity;

			[SerializeField]
			[Tooltip("Radius of sample points, which affects extent of darkened areas.")]
			public float radius;

			[SerializeField]
			[Tooltip("Number of sample points, which affects quality and performance.")]
			public SampleCount sampleCount;

			[SerializeField]
			[Tooltip("Determines the sample count when SampleCount.Variable is used.")]
			public int sampleCountValue;

			[SerializeField]
			[Tooltip("Halves the resolution of the effect to increase performance.")]
			public bool downsampling;

			[SerializeField]
			[Tooltip("If checked, the effect only affects ambient lighting.")]
			public bool ambientOnly;

			[SerializeField]
			[Tooltip("Source buffer on which the occlusion estimator is based.")]
			public OcclusionSource occlusionSource;

			[SerializeField]
			[Tooltip("Displays occlusion for debug purpose.")]
			public bool debug;

			public static Settings defaultSettings
			{
				get
				{
					Settings settings = new Settings();
					settings.intensity = 1f;
					settings.radius = 0.3f;
					settings.sampleCount = SampleCount.Medium;
					settings.sampleCountValue = 24;
					settings.downsampling = false;
					settings.ambientOnly = false;
					settings.occlusionSource = OcclusionSource.DepthNormalsTexture;
					return settings;
				}
			}
		}

		[SerializeField]
		public Settings settings = Settings.defaultSettings;

		[SerializeField]
		private Shader _aoShader;

		private Material _aoMaterial;

		private CommandBuffer _aoCommands;

		[SerializeField]
		private Mesh _quadMesh;

		public bool isAmbientOnlySupported
		{
			get
			{
				return targetCamera.allowHDR && occlusionSource == OcclusionSource.GBuffer;
			}
		}

		public bool isGBufferAvailable
		{
			get
			{
				return targetCamera.actualRenderingPath == RenderingPath.DeferredShading;
			}
		}

		private float intensity
		{
			get
			{
				return settings.intensity;
			}
		}

		private float radius
		{
			get
			{
				return Mathf.Max(settings.radius, 0.0001f);
			}
		}

		private SampleCount sampleCount
		{
			get
			{
				return settings.sampleCount;
			}
		}

		private int sampleCountValue
		{
			get
			{
				switch (settings.sampleCount)
				{
				case SampleCount.Lowest:
					return 3;
				case SampleCount.Low:
					return 6;
				case SampleCount.Medium:
					return 12;
				case SampleCount.High:
					return 20;
				default:
					return Mathf.Clamp(settings.sampleCountValue, 1, 256);
				}
			}
		}

		private OcclusionSource occlusionSource
		{
			get
			{
				if (settings.occlusionSource == OcclusionSource.GBuffer && !isGBufferAvailable)
				{
					return OcclusionSource.DepthNormalsTexture;
				}
				return settings.occlusionSource;
			}
		}

		private bool downsampling
		{
			get
			{
				return settings.downsampling;
			}
		}

		private bool ambientOnly
		{
			get
			{
				return settings.ambientOnly && !settings.debug && isAmbientOnlySupported;
			}
		}

		private RenderTextureFormat aoTextureFormat
		{
			get
			{
				if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8))
				{
					return RenderTextureFormat.R8;
				}
				return RenderTextureFormat.Default;
			}
		}

		private Shader aoShader
		{
			get
			{
				if (_aoShader == null)
				{
					_aoShader = Shader.Find("Hidden/Image Effects/Cinematic/AmbientOcclusion");
				}
				return _aoShader;
			}
		}

		private Material aoMaterial
		{
			get
			{
				if (_aoMaterial == null)
				{
					_aoMaterial = ImageEffectHelper.CheckShaderAndCreateMaterial(aoShader);
				}
				return _aoMaterial;
			}
		}

		private CommandBuffer aoCommands
		{
			get
			{
				if (_aoCommands == null)
				{
					_aoCommands = new CommandBuffer();
					_aoCommands.name = "AmbientOcclusion";
				}
				return _aoCommands;
			}
		}

		private Camera targetCamera
		{
			get
			{
				return GetComponent<Camera>();
			}
		}

		private PropertyObserver propertyObserver { get; set; }

		private Mesh quadMesh
		{
			get
			{
				return _quadMesh;
			}
		}

		private void BuildAOCommands()
		{
			CommandBuffer commandBuffer = aoCommands;
			int pixelWidth = targetCamera.pixelWidth;
			int pixelHeight = targetCamera.pixelHeight;
			int num = ((!downsampling) ? 1 : 2);
			RenderTextureFormat format = aoTextureFormat;
			RenderTextureReadWrite readWrite = RenderTextureReadWrite.Linear;
			FilterMode filter = FilterMode.Bilinear;
			Material material = aoMaterial;
			int num2 = Shader.PropertyToID("_OcclusionTexture");
			commandBuffer.GetTemporaryRT(num2, pixelWidth / num, pixelHeight / num, 0, filter, format, readWrite);
			commandBuffer.Blit(null, num2, material, 2);
			int num3 = Shader.PropertyToID("_OcclusionBlurTexture");
			commandBuffer.GetTemporaryRT(num3, pixelWidth, pixelHeight, 0, filter, format, readWrite);
			commandBuffer.SetGlobalVector("_BlurVector", Vector2.right * 2f);
			commandBuffer.Blit(num2, num3, material, 4);
			commandBuffer.ReleaseTemporaryRT(num2);
			commandBuffer.GetTemporaryRT(num2, pixelWidth, pixelHeight, 0, filter, format, readWrite);
			commandBuffer.SetGlobalVector("_BlurVector", Vector2.up * 2f * num);
			commandBuffer.Blit(num3, num2, material, 4);
			commandBuffer.ReleaseTemporaryRT(num3);
			commandBuffer.GetTemporaryRT(num3, pixelWidth, pixelHeight, 0, filter, format, readWrite);
			commandBuffer.SetGlobalVector("_BlurVector", Vector2.right * num);
			commandBuffer.Blit(num2, num3, material, 6);
			commandBuffer.ReleaseTemporaryRT(num2);
			commandBuffer.GetTemporaryRT(num2, pixelWidth, pixelHeight, 0, filter, format, readWrite);
			commandBuffer.SetGlobalVector("_BlurVector", Vector2.up * num);
			commandBuffer.Blit(num3, num2, material, 6);
			commandBuffer.ReleaseTemporaryRT(num3);
			RenderTargetIdentifier[] colors = new RenderTargetIdentifier[2]
			{
				BuiltinRenderTextureType.GBuffer0,
				BuiltinRenderTextureType.CameraTarget
			};
			commandBuffer.SetRenderTarget(colors, BuiltinRenderTextureType.CameraTarget);
			commandBuffer.SetGlobalTexture("_OcclusionTexture", num2);
			commandBuffer.DrawMesh(quadMesh, Matrix4x4.identity, material, 0, 8);
			commandBuffer.ReleaseTemporaryRT(num2);
		}

		private void ExecuteAOPass(RenderTexture source, RenderTexture destination)
		{
			int width = source.width;
			int height = source.height;
			int num = ((!downsampling) ? 1 : 2);
			RenderTextureFormat format = aoTextureFormat;
			RenderTextureReadWrite readWrite = RenderTextureReadWrite.Linear;
			bool flag = settings.occlusionSource == OcclusionSource.GBuffer;
			Material material = aoMaterial;
			RenderTexture temporary = RenderTexture.GetTemporary(width / num, height / num, 0, format, readWrite);
			Graphics.Blit(null, temporary, material, (int)occlusionSource);
			RenderTexture temporary2 = RenderTexture.GetTemporary(width, height, 0, format, readWrite);
			material.SetVector("_BlurVector", Vector2.right * 2f);
			Graphics.Blit(temporary, temporary2, material, (!flag) ? 3 : 4);
			RenderTexture.ReleaseTemporary(temporary);
			temporary = RenderTexture.GetTemporary(width, height, 0, format, readWrite);
			material.SetVector("_BlurVector", Vector2.up * 2f * num);
			Graphics.Blit(temporary2, temporary, material, (!flag) ? 3 : 4);
			RenderTexture.ReleaseTemporary(temporary2);
			temporary2 = RenderTexture.GetTemporary(width, height, 0, format, readWrite);
			material.SetVector("_BlurVector", Vector2.right * num);
			Graphics.Blit(temporary, temporary2, material, (!flag) ? 5 : 6);
			RenderTexture.ReleaseTemporary(temporary);
			temporary = RenderTexture.GetTemporary(width, height, 0, format, readWrite);
			material.SetVector("_BlurVector", Vector2.up * num);
			Graphics.Blit(temporary2, temporary, material, (!flag) ? 5 : 6);
			RenderTexture.ReleaseTemporary(temporary2);
			material.SetTexture("_OcclusionTexture", temporary);
			if (!settings.debug)
			{
				Graphics.Blit(source, destination, material, 7);
			}
			else
			{
				Graphics.Blit(source, destination, material, 9);
			}
			RenderTexture.ReleaseTemporary(temporary);
		}

		private void UpdateMaterialProperties()
		{
			Material material = aoMaterial;
			material.SetFloat("_Intensity", intensity);
			material.SetFloat("_Radius", radius);
			material.SetFloat("_TargetScale", (!downsampling) ? 1f : 0.5f);
			material.SetInt("_SampleCount", sampleCountValue);
		}

		private void OnEnable()
		{
			if (!ImageEffectHelper.IsSupported(aoShader, true, false, this))
			{
				base.enabled = false;
				return;
			}
			if (ambientOnly)
			{
				targetCamera.AddCommandBuffer(CameraEvent.BeforeReflections, aoCommands);
			}
			if (occlusionSource == OcclusionSource.DepthTexture)
			{
				targetCamera.depthTextureMode |= DepthTextureMode.Depth;
			}
			if (occlusionSource != OcclusionSource.GBuffer)
			{
				targetCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
			}
		}

		private void OnDisable()
		{
			if (_aoMaterial != null)
			{
				UnityEngine.Object.DestroyImmediate(_aoMaterial);
			}
			_aoMaterial = null;
			if (_aoCommands != null)
			{
				targetCamera.RemoveCommandBuffer(CameraEvent.BeforeReflections, _aoCommands);
			}
			_aoCommands = null;
		}

		private void Update()
		{
			if (propertyObserver.CheckNeedsReset(settings, targetCamera))
			{
				OnDisable();
				OnEnable();
				if (ambientOnly)
				{
					aoCommands.Clear();
					BuildAOCommands();
				}
				propertyObserver.Update(settings, targetCamera);
			}
			if (ambientOnly)
			{
				UpdateMaterialProperties();
			}
		}

		[ImageEffectOpaque]
		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (ambientOnly)
			{
				Graphics.Blit(source, destination);
				return;
			}
			UpdateMaterialProperties();
			ExecuteAOPass(source, destination);
		}
	}
}
