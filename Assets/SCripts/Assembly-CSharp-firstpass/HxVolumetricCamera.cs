using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

[ExecuteInEditMode]
public class HxVolumetricCamera : MonoBehaviour
{
	public enum hxRenderOrder
	{
		ImageEffect = 0,
		ImageEffectOpaque = 1
	}

	public enum TransparencyQualities
	{
		Low = 0,
		Medium = 1,
		High = 2,
		VeryHigh = 3
	}

	public enum DensityParticleQualities
	{
		Low = 0,
		Medium = 1,
		High = 2,
		VeryHigh = 3
	}

	public enum HxAmbientMode
	{
		UseRenderSettings = 0,
		Color = 1,
		Gradient = 2
	}

	public enum HxTintMode
	{
		Off = 0,
		Color = 1,
		Edge = 2,
		Gradient = 3
	}

	public enum Resolution
	{
		full = 0,
		half = 1,
		quarter = 2
	}

	public enum DensityResolution
	{
		full = 0,
		half = 1,
		quarter = 2,
		eighth = 3
	}

	private struct TriangleIndices
	{
		public int v1;

		public int v2;

		public int v3;

		public TriangleIndices(int v1, int v2, int v3)
		{
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;
		}
	}

	public hxRenderOrder RenderOrder;

	public HxVolumetricRenderCallback callBackImageEffect;

	public HxVolumetricRenderCallback callBackImageEffectOpaque;

	public bool ShadowFix = true;

	private bool TemporalFirst = true;

	public bool TemporalSampling = true;

	[Range(0f, 1f)]
	public float DitherSpeed = 0.6256256f;

	[Range(0f, 1f)]
	public float LuminanceFeedback = 0.8f;

	[Range(0f, 1f)]
	public float MaxFeedback = 0.9f;

	[Range(0f, 4f)]
	public float NoiseContrast = 1f;

	private static Shader directionalShader;

	private static Shader pointShader;

	private static Shader spotShader;

	private static Shader ProjectorShader;

	[NonSerialized]
	public bool FullUsed;

	[NonSerialized]
	public bool LowResUsed;

	[NonSerialized]
	public bool HeightFogUsed;

	[NonSerialized]
	public bool HeightFogOffUsed;

	[NonSerialized]
	public bool NoiseUsed;

	[NonSerialized]
	public bool NoiseOffUsed;

	[NonSerialized]
	public bool TransparencyUsed;

	[NonSerialized]
	public bool TransparencyOffUsed;

	[NonSerialized]
	public bool DensityParticlesUsed;

	[NonSerialized]
	public bool PointUsed;

	[NonSerialized]
	public bool SpotUsed;

	[NonSerialized]
	public bool ProjectorUsed;

	[NonSerialized]
	public bool DirectionalUsed;

	[NonSerialized]
	public bool SinglePassStereoUsed;

	public static TransparencyQualities TransparencyBufferDepth = TransparencyQualities.Medium;

	public static DensityParticleQualities DensityBufferDepth = DensityParticleQualities.High;

	private int EnumBufferDepthLength = 4;

	private Matrix4x4 CurrentView;

	private Matrix4x4 CurrentProj;

	private Matrix4x4 CurrentInvers;

	private Matrix4x4 CurrentView2;

	private Matrix4x4 CurrentProj2;

	private Matrix4x4 CurrentInvers2;

	private RenderTexture TemporalTexture;

	private RenderTargetIdentifier TemporalTextureRTID;

	private static RenderTexture VolumetricTexture;

	private static RenderTexture FullBlurRT;

	private static RenderTargetIdentifier FullBlurRTID;

	private static RenderTexture downScaledBlurRT;

	private static RenderTargetIdentifier downScaledBlurRTID;

	private static RenderTexture FullBlurRT2;

	private static RenderTargetIdentifier FullBlurRT2ID;

	private static RenderTargetIdentifier[] VolumetricUpsampledBlurTextures = new RenderTargetIdentifier[2];

	private static RenderTexture[] VolumetricDensityTextures = new RenderTexture[8];

	private static int[] VolumetricDensityPID = new int[4];

	private static int[] VolumetricTransparencyPID = new int[4];

	private static RenderTexture[] VolumetricTransparencyTextures = new RenderTexture[8];

	public static RenderTargetIdentifier[][] VolumetricDensity = new RenderTargetIdentifier[8][]
	{
		new RenderTargetIdentifier[1] { default(RenderTargetIdentifier) },
		new RenderTargetIdentifier[2]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[3]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[4]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[5]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[6]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[7]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[8]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		}
	};

	public static RenderTargetIdentifier[][] VolumetricTransparency = new RenderTargetIdentifier[8][]
	{
		new RenderTargetIdentifier[2]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[3]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[4]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[5]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[6]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[7]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[8]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[9]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		}
	};

	public static RenderTargetIdentifier[][] VolumetricTransparencyI = new RenderTargetIdentifier[8][]
	{
		new RenderTargetIdentifier[1] { default(RenderTargetIdentifier) },
		new RenderTargetIdentifier[2]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[3]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[4]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[5]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[6]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[7]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		},
		new RenderTargetIdentifier[8]
		{
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier),
			default(RenderTargetIdentifier)
		}
	};

	private static RenderTexture[] ScaledDepthTexture = new RenderTexture[4];

	private static ShaderVariantCollection CollectionAll;

	public static Texture2D Tile5x5;

	private static int VolumetricTexturePID;

	private static int ScaledDepthTexturePID;

	public static int ShadowMapTexturePID;

	public static RenderTargetIdentifier VolumetricTextureRTID;

	public static RenderTargetIdentifier[] ScaledDepthTextureRTID = new RenderTargetIdentifier[4];

	[NonSerialized]
	public static Material DownSampleMaterial;

	[NonSerialized]
	public static Material VolumeBlurMaterial;

	[NonSerialized]
	public static Material TransparencyBlurMaterial;

	[NonSerialized]
	public static Material ApplyMaterial;

	[NonSerialized]
	public static Material ApplyDirectMaterial;

	[NonSerialized]
	public static Material ApplyQueueMaterial;

	public Texture3D NoiseTexture3D;

	public static Matrix4x4 BlitMatrix;

	public static Matrix4x4 BlitMatrixMV;

	public static Matrix4x4 BlitMatrixMVP;

	public static Vector3 BlitScale;

	[Tooltip("Rending resolution, Lower for more speed, higher for better quality")]
	public Resolution resolution = Resolution.half;

	[Tooltip("How many samples per pixel, Recommended 4-8 for point, 6 - 16 for Directional")]
	[Range(2f, 64f)]
	public int SampleCount = 4;

	[Tooltip("How many samples per pixel, Recommended 4-8 for point, 6 - 16 for Directional")]
	[Range(2f, 64f)]
	public int DirectionalSampleCount = 8;

	[Tooltip("Max distance the directional light gets raymarched.")]
	public float MaxDirectionalRayDistance = 128f;

	[Tooltip("Any point of spot lights passed this point will not render.")]
	public float MaxLightDistance = 128f;

	[Range(0f, 1f)]
	[Tooltip("Density of air")]
	public float Density = 0.05f;

	[Range(0f, 2f)]
	public float AmbientLightingStrength = 0.5f;

	[Tooltip("0 for even scattering, 1 for forward scattering")]
	[Range(0f, 0.995f)]
	public float MieScattering = 0.4f;

	[Range(0f, 1f)]
	[Tooltip("Create a sun using mie Scattering")]
	public float SunSize;

	[Tooltip("Allows the sun to bleed over the edge of objects (recommend using bloom)")]
	public bool SunBleed = true;

	[Range(0f, 0.5f)]
	[Tooltip("dimms results over distance")]
	public float Extinction = 0.05f;

	[Tooltip("Tone down Extinction effect on FinalColor")]
	[Range(0f, 1f)]
	public float ExtinctionEffect;

	public bool FogHeightEnabled;

	public float FogHeight = 5f;

	public float FogTransitionSize = 5f;

	public float AboveFogPercent = 0.1f;

	[Tooltip("Ambient Mode - Use unitys or overide your own")]
	public HxAmbientMode Ambient;

	public Color AmbientSky = Color.white;

	public Color AmbientEquator = Color.white;

	public Color AmbientGround = Color.white;

	[Range(0f, 1f)]
	public float AmbientIntensity = 1f;

	public HxTintMode TintMode;

	public Color TintColor = Color.red;

	public Color TintColor2 = Color.blue;

	public float TintIntensity = 0.2f;

	[Range(0f, 1f)]
	public float TintGradient = 0.2f;

	public Vector3 CurrentTint;

	public Vector3 CurrentTintEdge;

	[Tooltip("Use 3D noise")]
	public bool NoiseEnabled;

	[Tooltip("The scale of the noise texture")]
	public Vector3 NoiseScale = new Vector3(0.1f, 0.1f, 0.1f);

	[Tooltip("Used to simulate some wind")]
	public Vector3 NoiseVelocity = new Vector3(1f, 0f, 1f);

	[Tooltip("Allows particles to modulate the air density")]
	public bool ParticleDensitySupport;

	[Tooltip("Rending resolution of density, Lower for more speed, higher for more detailed dust")]
	public DensityResolution densityResolution = DensityResolution.eighth;

	[Tooltip("Max Distance of density particles")]
	public float densityDistance = 64f;

	private float densityBias = 1.7f;

	[Tooltip("Enabling Transparency support has a cost - disable if you dont need it")]
	public bool TransparencySupport;

	[Tooltip("Max Distance for transparency Support - lower distance will give greater resilts")]
	public float transparencyDistance = 64f;

	[Tooltip("Cost a little extra but can remove the grainy look on Transparent objects when sample count is low")]
	[Range(0f, 4f)]
	public int BlurTransparency = 1;

	private float transparencyBias = 1.5f;

	[Range(0f, 4f)]
	[Tooltip("Blur results of volumetric pass")]
	public int blurCount = 1;

	[Tooltip("Used in final blur pass, Higher number will retain silhouette")]
	public float BlurDepthFalloff = 5f;

	[Tooltip("Used in Downsample blur pass, Higher number will retain silhouette")]
	public float DownsampledBlurDepthFalloff = 5f;

	[Range(0f, 4f)]
	[Tooltip("Blur bad results after upscaling")]
	public int UpSampledblurCount;

	[Tooltip("If depth is with-in this threshold, bilinearly sample result")]
	public float DepthThreshold = 0.06f;

	[Tooltip("Use gaussian weights - makes blur less blurry but can make it more splotchy")]
	public bool GaussianWeights;

	[HideInInspector]
	[Tooltip("Only enable if you arnt using tonemapping and HDR mode")]
	public bool MapToLDR;

	[Tooltip("A small amount of noise can be added to remove and color banding from the volumetric effect")]
	public bool RemoveColorBanding = true;

	[NonSerialized]
	public Vector3 Offset = Vector3.zero;

	private static int DepthThresholdPID;

	private static int BlurDepthFalloffPID;

	private static int VolumeScalePID;

	private static int InverseViewMatrixPID;

	private static int InverseProjectionMatrixPID;

	private static int InverseProjectionMatrix2PID;

	private static int NoiseOffsetPID;

	private static int ShadowDistancePID;

	private static HxVolumetricShadersUsed UsedShaderSettings;

	private static List<string> ShaderVariantList = new List<string>(10);

	[HideInInspector]
	public static List<HxDensityVolume> ActiveVolumes = new List<HxDensityVolume>();

	public static List<HxVolumetricLight> ActiveLights = new List<HxVolumetricLight>();

	public static List<HxVolumetricParticleSystem> ActiveParticleSystems = new List<HxVolumetricParticleSystem>();

	public static HxOctree<HxVolumetricLight> LightOctree;

	public static HxOctree<HxVolumetricParticleSystem> ParticleOctree;

	public static HashSet<HxDensityVolume> AllDensityVolumes = new HashSet<HxDensityVolume>();

	public static HashSet<HxVolumetricLight> AllVolumetricLight = new HashSet<HxVolumetricLight>();

	public static HashSet<HxVolumetricParticleSystem> AllParticleSystems = new HashSet<HxVolumetricParticleSystem>();

	private bool test;

	public static Mesh QuadMesh;

	public static Mesh BoxMesh;

	public static Mesh SphereMesh;

	public static Mesh SpotLightMesh;

	public static Mesh OrthoProjectorMesh;

	[HideInInspector]
	private Camera Mycamera;

	private static float[] ResolutionScale = new float[4] { 1f, 0.5f, 0.25f, 0.125f };

	public static float[] SampleScale = new float[4] { 1f, 4f, 16f, 32f };

	private CommandBuffer BufferSetup;

	private CommandBuffer BufferRender;

	private CommandBuffer BufferRenderLights;

	private CommandBuffer BufferFinalize;

	private bool dirty = true;

	[NonSerialized]
	public static bool PIDCreated = false;

	[NonSerialized]
	private static Dictionary<int, Material> DirectionalMaterial = new Dictionary<int, Material>();

	[NonSerialized]
	private static Dictionary<int, Material> PointMaterial = new Dictionary<int, Material>();

	[NonSerialized]
	private static Dictionary<int, Material> SpotMaterial = new Dictionary<int, Material>();

	[NonSerialized]
	private static Dictionary<int, Material> ProjectorMaterial = new Dictionary<int, Material>();

	public static ShaderVariantCollection.ShaderVariant[] DirectionalVariant = new ShaderVariantCollection.ShaderVariant[128];

	public static ShaderVariantCollection.ShaderVariant[] PointVariant = new ShaderVariantCollection.ShaderVariant[128];

	public static ShaderVariantCollection.ShaderVariant[] SpotVariant = new ShaderVariantCollection.ShaderVariant[128];

	public static Material ShadowMaterial;

	public static Material DensityMaterial;

	[HideInInspector]
	public Matrix4x4 MatrixVP;

	public Matrix4x4 LastMatrixVP;

	public Matrix4x4 LastMatrixVPInv;

	public Matrix4x4 LastMatrixVP2;

	public Matrix4x4 LastMatrixVPInv2;

	[HideInInspector]
	public Matrix4x4 MatrixV;

	private bool OffsetUpdated;

	[HideInInspector]
	private static Texture2D _SpotLightCookie;

	[HideInInspector]
	private static Texture2D _LightFalloff;

	private int ParticleDensityRenderCount;

	private static Matrix4x4 particleMatrix;

	public static HxVolumetricCamera Active;

	public static Camera ActiveCamera;

	private CameraEvent LightRenderEvent = CameraEvent.AfterLighting;

	private CameraEvent SetupEvent = CameraEvent.AfterDepthNormalsTexture;

	private CameraEvent RenderEvent = CameraEvent.BeforeLighting;

	private CameraEvent FinalizeEvent = CameraEvent.AfterLighting;

	public static List<HxVolumetricLight> ActiveDirectionalLights = new List<HxVolumetricLight>();

	private static Vector3 MinBounds;

	private static Vector3 MaxBounds;

	private static Plane[] CameraPlanes = new Plane[6]
	{
		default(Plane),
		default(Plane),
		default(Plane),
		default(Plane),
		default(Plane),
		default(Plane)
	};

	private bool preCullEventAdded;

	private bool BuffersBuilt;

	private bool LightBufferAdded;

	private bool SetupBufferAdded;

	private bool SetupBufferDirty;

	private bool FinalizeBufferAdded;

	private bool FinalizeBufferDirty;

	private CameraEvent lastApply;

	private CameraEvent lastRender;

	private CameraEvent lastSetup;

	private CameraEvent lastFinalize;

	private CameraEvent lastLightRender;

	private bool LastPlaying;

	[NonSerialized]
	private static int lastRes = -1;

	[NonSerialized]
	private int lastBlurCount = -1;

	[NonSerialized]
	private int lastupSampleBlurCount;

	[NonSerialized]
	private int lastLDR = -1;

	[NonSerialized]
	private int lastBanding = -1;

	[NonSerialized]
	private int lastH = -1;

	[NonSerialized]
	private int lastW = -1;

	[NonSerialized]
	private int lastPath = -1;

	[NonSerialized]
	private int lastGaussian = -1;

	[NonSerialized]
	private int lastTransparency = -1;

	[NonSerialized]
	private int lastDensity = -1;

	[NonSerialized]
	private int lastDensityRes = -1;

	[NonSerialized]
	private float lastDepthFalloff = -1f;

	[NonSerialized]
	private float lastDownDepthFalloff = -1f;

	private float currentDitherOffset;

	private float MaxLightDistanceUsed;

	public static bool FirstDirectional = true;

	private static int[] Tile5x5int = new int[25]
	{
		8, 18, 22, 0, 13, 4, 14, 9, 19, 21,
		16, 23, 1, 12, 6, 10, 7, 15, 24, 3,
		20, 2, 11, 5, 17
	};

	[CompilerGenerated]
	private static Comparison<HxDensityVolume> _003C_003Ef__am_0024cache0;

	[HideInInspector]
	public Texture2D SpotLightCookie
	{
		get
		{
			if (_SpotLightCookie == null)
			{
				_SpotLightCookie = (Texture2D)Resources.Load("LightSoftCookie");
				if (_SpotLightCookie == null)
				{
					Debug.Log("couldnt find default cookie");
				}
			}
			return _SpotLightCookie;
		}
		set
		{
			_SpotLightCookie = value;
		}
	}

	[HideInInspector]
	public Texture2D LightFalloff
	{
		get
		{
			if (_LightFalloff == null)
			{
				_LightFalloff = (Texture2D)Resources.Load("HxFallOff");
				if (_LightFalloff == null)
				{
					Debug.Log("couldnt find default Falloff");
				}
			}
			return _LightFalloff;
		}
		set
		{
			_LightFalloff = value;
		}
	}

	private void SetUpRenderOrder()
	{
		if (callBackImageEffect != null)
		{
			if (TransparencySupport || RenderOrder == hxRenderOrder.ImageEffectOpaque)
			{
				callBackImageEffect.enabled = false;
			}
			else
			{
				callBackImageEffect.enabled = true;
			}
		}
		if (callBackImageEffectOpaque != null)
		{
			if (TransparencySupport || RenderOrder == hxRenderOrder.ImageEffectOpaque)
			{
				callBackImageEffectOpaque.enabled = true;
			}
			else
			{
				callBackImageEffectOpaque.enabled = false;
			}
		}
		if (callBackImageEffectOpaque == null && (TransparencySupport || RenderOrder == hxRenderOrder.ImageEffectOpaque))
		{
			callBackImageEffectOpaque = base.gameObject.GetComponent<HxVolumetricImageEffectOpaque>();
			if (callBackImageEffectOpaque == null)
			{
				callBackImageEffectOpaque = base.gameObject.AddComponent<HxVolumetricImageEffectOpaque>();
				callBackImageEffectOpaque.RenderOrder = hxRenderOrder.ImageEffectOpaque;
			}
		}
		if (callBackImageEffect == null && !TransparencySupport && RenderOrder != hxRenderOrder.ImageEffectOpaque)
		{
			callBackImageEffect = base.gameObject.GetComponent<HxVolumetricImageEffect>();
			if (callBackImageEffect == null)
			{
				callBackImageEffect = base.gameObject.AddComponent<HxVolumetricImageEffect>();
				callBackImageEffect.RenderOrder = hxRenderOrder.ImageEffect;
			}
		}
	}

	public static Material GetDirectionalMaterial(int mid)
	{
		Material value;
		if (!DirectionalMaterial.TryGetValue(mid, out value))
		{
			if (directionalShader == null)
			{
				directionalShader = Shader.Find("Hidden/HxVolumetricDirectionalLight");
			}
			CreateShader(directionalShader, mid, out value, false);
			DirectionalMaterial.Add(mid, value);
		}
		return value;
	}

	public static Material GetProjectorMaterial(int mid)
	{
		Material value;
		if (!ProjectorMaterial.TryGetValue(mid, out value))
		{
			if (ProjectorShader == null)
			{
				ProjectorShader = Shader.Find("Hidden/HxVolumetricProjector");
			}
			CreateShader(ProjectorShader, mid, out value, false);
			ProjectorMaterial.Add(mid, value);
		}
		return value;
	}

	public static Material GetSpotMaterial(int mid)
	{
		Material value;
		if (!SpotMaterial.TryGetValue(mid, out value))
		{
			if (spotShader == null)
			{
				spotShader = Shader.Find("Hidden/HxVolumetricSpotLight");
			}
			CreateShader(spotShader, mid, out value, false);
			SpotMaterial.Add(mid, value);
		}
		return value;
	}

	public static Material GetPointMaterial(int mid)
	{
		Material value;
		if (!PointMaterial.TryGetValue(mid, out value))
		{
			if (pointShader == null)
			{
				pointShader = Shader.Find("Hidden/HxVolumetricPointLight");
			}
			CreateShader(pointShader, mid, out value);
			PointMaterial.Add(mid, value);
		}
		return value;
	}

	public TransparencyQualities compatibleTBuffer()
	{
		if (TransparencyBufferDepth > TransparencyQualities.Medium && SystemInfo.graphicsDeviceType != GraphicsDeviceType.Direct3D11 && SystemInfo.graphicsDeviceType != GraphicsDeviceType.Direct3D12 && SystemInfo.graphicsDeviceType != GraphicsDeviceType.PlayStation4)
		{
			return TransparencyQualities.High;
		}
		return TransparencyBufferDepth;
	}

	private bool IsRenderBoth()
	{
		if (Mycamera.stereoTargetEye == StereoTargetEyeMask.Both && Application.isPlaying && XRSettings.enabled && HxUtil.isPresent())
		{
			return true;
		}
		return false;
	}

	private DensityParticleQualities compatibleDBuffer()
	{
		return DensityBufferDepth;
	}

	private void MyPreCull(Camera cam)
	{
		if (cam != ActiveCamera)
		{
			ReleaseLightBuffers();
			SetUpRenderOrder();
		}
	}

	public bool renderDensityParticleCheck()
	{
		return ParticleDensityRenderCount > 0;
	}

	private void WarmUp()
	{
		if (CollectionAll == null)
		{
			UnityEngine.Object[] array = Resources.LoadAll("HxUsedShaderVariantCollection");
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] as ShaderVariantCollection != null)
				{
					CollectionAll = array[i] as ShaderVariantCollection;
					break;
				}
			}
			if (CollectionAll != null)
			{
				CollectionAll.WarmUp();
			}
		}
		if (UsedShaderSettings == null)
		{
			UsedShaderSettings = (HxVolumetricShadersUsed)Resources.Load("HxUsedShaders");
			if (UsedShaderSettings != null)
			{
				TransparencyBufferDepth = UsedShaderSettings.LastTransperencyQuality;
				DensityBufferDepth = UsedShaderSettings.LastDensityParticleQuality;
			}
		}
	}

	private void CreateShaderVariant(Shader source, int i, ref Material[] material, ref ShaderVariantCollection.ShaderVariant[] Variant, bool point = true)
	{
		ShaderVariantList.Clear();
		int num = i;
		int num2 = 0;
		if (num >= 64)
		{
			material[i].EnableKeyword("FULL_ON");
			ShaderVariantList.Add("FULL_ON");
			num -= 64;
			num2++;
		}
		if (num >= 32)
		{
			material[i].EnableKeyword("VTRANSPARENCY_ON");
			ShaderVariantList.Add("VTRANSPARENCY_ON");
			num -= 32;
			num2++;
		}
		if (num >= 16)
		{
			material[i].EnableKeyword("DENSITYPARTICLES_ON");
			ShaderVariantList.Add("DENSITYPARTICLES_ON");
			num -= 16;
			num2++;
		}
		if (num >= 8)
		{
			material[i].EnableKeyword("HEIGHTFOG_ON");
			ShaderVariantList.Add("HEIGHTFOG_ON");
			num -= 8;
			num2++;
		}
		if (num >= 4)
		{
			material[i].EnableKeyword("NOISE_ON");
			ShaderVariantList.Add("NOISE_ON");
			num -= 4;
			num2++;
		}
		if (num >= 2)
		{
			if (point)
			{
				material[i].EnableKeyword("POINT_COOKIE");
				ShaderVariantList.Add("POINT_COOKIE");
				num2++;
			}
			num -= 2;
		}
		if (num >= 1)
		{
			num--;
		}
		else
		{
			material[i].EnableKeyword("SHADOWS_OFF");
			ShaderVariantList.Add("SHADOWS_OFF");
			num2++;
		}
		if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Shadowmap))
		{
			material[i].EnableKeyword("SHADOWS_NATIVE");
		}
		string[] array = new string[num2];
		ShaderVariantList.CopyTo(array);
		Variant[i] = new ShaderVariantCollection.ShaderVariant(source, PassType.Normal, array);
	}

	private static void CreateShader(Shader source, int i, out Material outMaterial, bool point = true)
	{
		outMaterial = new Material(source);
		outMaterial.hideFlags = HideFlags.DontSave;
		bool flag = false;
		int num = i;
		int num2 = 0;
		if (num >= 64)
		{
			outMaterial.EnableKeyword("FULL_ON");
			num -= 64;
			num2++;
		}
		if (num >= 32)
		{
			outMaterial.EnableKeyword("VTRANSPARENCY_ON");
			num -= 32;
			num2++;
		}
		if (num >= 16)
		{
			outMaterial.EnableKeyword("DENSITYPARTICLES_ON");
			num -= 16;
			num2++;
		}
		if (num >= 8)
		{
			outMaterial.EnableKeyword("HEIGHTFOG_ON");
			num -= 8;
			num2++;
		}
		if (num >= 4)
		{
			outMaterial.EnableKeyword("NOISE_ON");
			num -= 4;
			num2++;
		}
		if (num >= 2)
		{
			if (point)
			{
				outMaterial.EnableKeyword("POINT_COOKIE");
				num2++;
			}
			num -= 2;
		}
		if (num >= 1)
		{
			num--;
			flag = true;
		}
		else
		{
			outMaterial.EnableKeyword("SHADOWS_OFF");
			num2++;
		}
		if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Shadowmap) && flag)
		{
			outMaterial.EnableKeyword("SHADOWS_NATIVE");
		}
	}

	private void CreatePIDs()
	{
		if (NoiseTexture3D == null)
		{
			Create3DNoiseTexture();
		}
		bool flag = false;
		if (!PIDCreated)
		{
			flag = true;
			PIDCreated = true;
			VolumetricTexturePID = Shader.PropertyToID("VolumetricTexture");
			ScaledDepthTexturePID = Shader.PropertyToID("VolumetricDepth");
			ShadowMapTexturePID = Shader.PropertyToID("_ShadowMapTexture");
			DepthThresholdPID = Shader.PropertyToID("DepthThreshold");
			BlurDepthFalloffPID = Shader.PropertyToID("BlurDepthFalloff");
			VolumeScalePID = Shader.PropertyToID("VolumeScale");
			InverseViewMatrixPID = Shader.PropertyToID("InverseViewMatrix");
			InverseProjectionMatrixPID = Shader.PropertyToID("InverseProjectionMatrix");
			InverseProjectionMatrix2PID = Shader.PropertyToID("InverseProjectionMatrix2");
			NoiseOffsetPID = Shader.PropertyToID("NoiseOffset");
			ShadowDistancePID = Shader.PropertyToID("ShadowDistance");
			for (int i = 0; i < EnumBufferDepthLength; i++)
			{
				VolumetricDensityPID[i] = Shader.PropertyToID("VolumetricDensityTexture" + i);
				VolumetricTransparencyPID[i] = Shader.PropertyToID("VolumetricTransparencyTexture" + i);
			}
			HxVolumetricLight.CreatePID();
		}
		if (Tile5x5 == null)
		{
			CreateTileTexture();
		}
		if (DownSampleMaterial == null)
		{
			DownSampleMaterial = new Material(Shader.Find("Hidden/HxVolumetricDownscaleDepth"));
			DownSampleMaterial.hideFlags = HideFlags.DontSave;
		}
		if (TransparencyBlurMaterial == null)
		{
			TransparencyBlurMaterial = new Material(Shader.Find("Hidden/HxTransparencyBlur"));
			TransparencyBlurMaterial.hideFlags = HideFlags.DontSave;
		}
		if (DensityMaterial == null)
		{
			DensityMaterial = new Material(Shader.Find("Hidden/HxDensityShader"));
			DensityMaterial.hideFlags = HideFlags.DontSave;
		}
		if (VolumeBlurMaterial == null)
		{
			VolumeBlurMaterial = new Material(Shader.Find("Hidden/HxVolumetricDepthAwareBlur"));
			VolumeBlurMaterial.hideFlags = HideFlags.DontSave;
		}
		if (ApplyMaterial == null)
		{
			ApplyMaterial = new Material(Shader.Find("Hidden/HxVolumetricApply"));
			ApplyMaterial.hideFlags = HideFlags.DontSave;
		}
		if (ApplyDirectMaterial == null)
		{
			ApplyDirectMaterial = new Material(Shader.Find("Hidden/HxVolumetricApplyDirect"));
			ApplyDirectMaterial.hideFlags = HideFlags.DontSave;
		}
		if (ApplyQueueMaterial == null)
		{
			ApplyQueueMaterial = new Material(Shader.Find("Hidden/HxVolumetricApplyRenderQueue"));
			ApplyQueueMaterial.hideFlags = HideFlags.DontSave;
		}
		if (QuadMesh == null)
		{
			QuadMesh = CreateQuad();
			QuadMesh.hideFlags = HideFlags.DontSave;
		}
		if (BoxMesh == null)
		{
			BoxMesh = CreateBox();
		}
		if (SphereMesh == null)
		{
			SphereMesh = CreateIcoSphere(1, 0.56f);
			SphereMesh.hideFlags = HideFlags.DontSave;
		}
		if (SpotLightMesh == null)
		{
			SpotLightMesh = CreateCone(4, false);
			SpotLightMesh.hideFlags = HideFlags.DontSave;
		}
		if (OrthoProjectorMesh == null)
		{
			OrthoProjectorMesh = CreateOrtho(4, false);
			OrthoProjectorMesh.hideFlags = HideFlags.DontSave;
		}
		if (directionalShader == null)
		{
			directionalShader = Shader.Find("Hidden/HxVolumetricDirectionalLight");
		}
		if (pointShader == null)
		{
			pointShader = Shader.Find("Hidden/HxVolumetricPointLight");
		}
		if (spotShader == null)
		{
			spotShader = Shader.Find("Hidden/HxVolumetricSpotLight");
		}
		if (flag)
		{
			WarmUp();
		}
		if (ShadowMaterial == null)
		{
			ShadowMaterial = new Material(Shader.Find("Hidden/HxShadowCasterFix"));
			ShadowMaterial.hideFlags = HideFlags.DontSave;
		}
	}

	public static bool ActiveFull()
	{
		return Active.resolution == Resolution.full;
	}

	private void DefineFull()
	{
	}

	private static void UpdateLight(HxOctreeNode<HxVolumetricLight>.NodeObject node, Vector3 boundsMin, Vector3 boundsMax)
	{
		LightOctree.Move(node, boundsMin, boundsMax);
	}

	public static HxOctreeNode<HxVolumetricLight>.NodeObject AddLightOctree(HxVolumetricLight light, Vector3 boundsMin, Vector3 boundsMax)
	{
		if (LightOctree == null)
		{
			LightOctree = new HxOctree<HxVolumetricLight>(Vector3.zero, 100f, 0.1f, 10f);
		}
		return LightOctree.Add(light, boundsMin, boundsMax);
	}

	public static HxOctreeNode<HxVolumetricParticleSystem>.NodeObject AddParticleOctree(HxVolumetricParticleSystem particle, Vector3 boundsMin, Vector3 boundsMax)
	{
		if (ParticleOctree == null)
		{
			ParticleOctree = new HxOctree<HxVolumetricParticleSystem>(Vector3.zero, 100f, 0.1f, 10f);
		}
		return ParticleOctree.Add(particle, boundsMin, boundsMax);
	}

	public static void RemoveLightOctree(HxVolumetricLight light)
	{
		if (LightOctree != null)
		{
			LightOctree.Remove(light);
		}
	}

	public static void RemoveParticletOctree(HxVolumetricParticleSystem Particle)
	{
		if (ParticleOctree != null)
		{
			ParticleOctree.Remove(Particle);
		}
	}

	private void OnApplicationQuit()
	{
		PIDCreated = false;
	}

	public Camera GetCamera()
	{
		if (Mycamera == null)
		{
			Mycamera = GetComponent<Camera>();
		}
		return Mycamera;
	}

	private Vector4 CalculateDensityDistance(int i)
	{
		float num = (int)(compatibleDBuffer() + 1) * 4 - 1;
		return new Vector4(densityDistance * Mathf.Pow((float)(i + 1) / num, densityBias) - densityDistance * Mathf.Pow((float)i / num, densityBias), densityDistance * Mathf.Pow((float)(i + 2) / num, densityBias) - densityDistance * Mathf.Pow((float)(i + 1) / num, densityBias), densityDistance * Mathf.Pow((float)(i + 3) / num, densityBias) - densityDistance * Mathf.Pow((float)(i + 2) / num, densityBias), densityDistance * Mathf.Pow((float)(i + 4) / num, densityBias) - densityDistance * Mathf.Pow((float)(i + 3) / num, densityBias));
	}

	private Vector4 CalculateTransparencyDistance(int i)
	{
		float num = (int)(compatibleTBuffer() + 1) * 4 - 1;
		return new Vector4(transparencyDistance * Mathf.Pow((float)(i + 1) / num, transparencyBias) - transparencyDistance * Mathf.Pow((float)i / num, transparencyBias), transparencyDistance * Mathf.Pow((float)(i + 2) / num, transparencyBias) - transparencyDistance * Mathf.Pow((float)(i + 1) / num, transparencyBias), transparencyDistance * Mathf.Pow((float)(i + 3) / num, transparencyBias) - transparencyDistance * Mathf.Pow((float)(i + 2) / num, transparencyBias), transparencyDistance * Mathf.Pow((float)(i + 4) / num, transparencyBias) - transparencyDistance * Mathf.Pow((float)(i + 3) / num, transparencyBias));
	}

	private void RenderParticles()
	{
		ParticleDensityRenderCount = 0;
		if (ParticleDensitySupport)
		{
			BufferRender.SetGlobalVector("DensitySliceDistance0", CalculateDensityDistance(0));
			BufferRender.SetGlobalVector("DensitySliceDistance1", CalculateDensityDistance(1));
			BufferRender.SetGlobalVector("DensitySliceDistance2", CalculateDensityDistance(2));
			BufferRender.SetGlobalVector("DensitySliceDistance3", CalculateDensityDistance(3));
			ConstructPlanes(Mycamera, 0f, Mathf.Max(MaxDirectionalRayDistance, MaxLightDistanceUsed));
			FindActiveParticleSystems();
			ParticleDensityRenderCount += RenderSlices();
			if (ParticleDensityRenderCount > 0)
			{
				Shader.EnableKeyword("DENSITYPARTICLES_ON");
				BufferRender.SetGlobalVector("SliceSettings", new Vector4(densityDistance, 1f / densityBias, (int)(compatibleDBuffer() + 1) * 4, 0f));
				for (int i = 0; i < (int)(compatibleDBuffer() + 1); i++)
				{
					BufferRender.SetGlobalTexture(VolumetricDensityPID[i], VolumetricDensity[(int)compatibleDBuffer()][i]);
				}
			}
			else
			{
				Shader.DisableKeyword("DENSITYPARTICLES_ON");
			}
		}
		else
		{
			Shader.DisableKeyword("DENSITYPARTICLES_ON");
		}
		if (TransparencySupport)
		{
			Shader.EnableKeyword("VTRANSPARENCY_ON");
			BufferRender.SetGlobalVector("TransparencySliceSettings", new Vector4(transparencyDistance, 1f / transparencyBias, (int)(compatibleTBuffer() + 1) * 4, 1f / transparencyDistance));
			for (int j = 0; j < (int)(compatibleTBuffer() + 1); j++)
			{
				BufferRender.SetGlobalTexture(VolumetricTransparencyPID[j], VolumetricTransparencyI[(int)compatibleTBuffer()][j]);
			}
		}
		else
		{
			Shader.DisableKeyword("VTRANSPARENCY_ON");
		}
	}

	private void OnPostRender()
	{
		Shader.DisableKeyword("VTRANSPARENCY_ON");
	}

	private int RenderSlices()
	{
		BufferRender.SetRenderTarget(VolumetricDensity[(int)compatibleDBuffer()], VolumetricDensity[(int)compatibleDBuffer()][0]);
		BufferRender.ClearRenderTarget(false, true, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		BufferRender.SetGlobalVector("SliceSettings", new Vector4(densityDistance, 1f / densityBias, (int)(compatibleDBuffer() + 1) * 4, 0f));
		int num = 0;
		for (int i = 0; i < ActiveParticleSystems.Count; i++)
		{
			if (ActiveParticleSystems[i].BlendMode == HxVolumetricParticleSystem.ParticleBlendMode.Max)
			{
				BufferRender.SetGlobalFloat("particleDensity", ActiveParticleSystems[i].DensityStrength);
				DensityMaterial.CopyPropertiesFromMaterial(ActiveParticleSystems[i].particleRenderer.sharedMaterial);
				BufferRender.DrawRenderer(ActiveParticleSystems[i].particleRenderer, DensityMaterial, 0, (int)ActiveParticleSystems[i].BlendMode);
				num++;
			}
		}
		for (int j = 0; j < ActiveParticleSystems.Count; j++)
		{
			if (ActiveParticleSystems[j].BlendMode == HxVolumetricParticleSystem.ParticleBlendMode.Add)
			{
				BufferRender.SetGlobalFloat("particleDensity", ActiveParticleSystems[j].DensityStrength);
				DensityMaterial.CopyPropertiesFromMaterial(ActiveParticleSystems[j].particleRenderer.sharedMaterial);
				BufferRender.DrawRenderer(ActiveParticleSystems[j].particleRenderer, DensityMaterial, 0, (int)ActiveParticleSystems[j].BlendMode);
				num++;
			}
		}
		for (int k = 0; k < ActiveParticleSystems.Count; k++)
		{
			if (ActiveParticleSystems[k].BlendMode == HxVolumetricParticleSystem.ParticleBlendMode.Min)
			{
				BufferRender.SetGlobalFloat("particleDensity", ActiveParticleSystems[k].DensityStrength);
				DensityMaterial.CopyPropertiesFromMaterial(ActiveParticleSystems[k].particleRenderer.sharedMaterial);
				BufferRender.DrawRenderer(ActiveParticleSystems[k].particleRenderer, DensityMaterial, 0, (int)ActiveParticleSystems[k].BlendMode);
				num++;
			}
		}
		for (int l = 0; l < ActiveParticleSystems.Count; l++)
		{
			if (ActiveParticleSystems[l].BlendMode == HxVolumetricParticleSystem.ParticleBlendMode.Sub)
			{
				BufferRender.SetGlobalFloat("particleDensity", ActiveParticleSystems[l].DensityStrength);
				DensityMaterial.CopyPropertiesFromMaterial(ActiveParticleSystems[l].particleRenderer.sharedMaterial);
				BufferRender.DrawRenderer(ActiveParticleSystems[l].particleRenderer, DensityMaterial, 0, (int)ActiveParticleSystems[l].BlendMode);
				num++;
			}
		}
		return num;
	}

	private int GetCamPixelHeight()
	{
		if (Mycamera.stereoTargetEye != 0 && Application.isPlaying && XRSettings.enabled && HxUtil.isPresent())
		{
			return XRSettings.eyeTextureHeight;
		}
		return Mycamera.pixelHeight;
	}

	private int GetCamPixelWidth()
	{
		if (Mycamera.stereoTargetEye != 0 && Application.isPlaying && XRSettings.enabled && HxUtil.isPresent())
		{
			return XRSettings.eyeTextureWidth + ((Mycamera.stereoTargetEye == StereoTargetEyeMask.Both) ? (XRSettings.eyeTextureWidth + Mathf.CeilToInt(48f * XRSettings.eyeTextureResolutionScale)) : 0);
		}
		return Mycamera.pixelWidth;
	}

	private void CreateTempTextures()
	{
		int width = Mathf.CeilToInt((float)GetCamPixelWidth() * ResolutionScale[(int)resolution]);
		int height = Mathf.CeilToInt((float)GetCamPixelHeight() * ResolutionScale[(int)resolution]);
		if (resolution != 0 && FullBlurRT == null)
		{
			FullBlurRT = RenderTexture.GetTemporary(GetCamPixelWidth(), GetCamPixelHeight(), 16, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
			FullBlurRTID = new RenderTargetIdentifier(FullBlurRT);
			FullBlurRT.filterMode = FilterMode.Bilinear;
			FullBlurRT.hideFlags = HideFlags.DontSave;
		}
		if (VolumetricTexture == null)
		{
			VolumetricTexture = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
			VolumetricTexture.filterMode = FilterMode.Bilinear;
			VolumetricTexture.hideFlags = HideFlags.DontSave;
			VolumetricTextureRTID = new RenderTargetIdentifier(VolumetricTexture);
		}
		if (ScaledDepthTexture[(int)resolution] == null)
		{
			ScaledDepthTexture[(int)resolution] = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
			ScaledDepthTexture[(int)resolution].filterMode = FilterMode.Point;
			ScaledDepthTextureRTID[(int)resolution] = new RenderTargetIdentifier(ScaledDepthTexture[(int)resolution]);
			ScaledDepthTexture[(int)resolution].hideFlags = HideFlags.DontSave;
		}
		if (TransparencySupport)
		{
			for (int i = 0; i < EnumBufferDepthLength; i++)
			{
				VolumetricTransparency[i][0] = VolumetricTextureRTID;
			}
			for (int j = 0; j < (int)(compatibleTBuffer() + 1); j++)
			{
				if (VolumetricTransparencyTextures[j] == null)
				{
					VolumetricTransparencyTextures[j] = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
					VolumetricTransparencyTextures[j].hideFlags = HideFlags.DontSave;
					VolumetricTransparencyTextures[j].filterMode = FilterMode.Bilinear;
					RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(VolumetricTransparencyTextures[j]);
					for (int k = Mathf.Max(j, 0); k < EnumBufferDepthLength; k++)
					{
						VolumetricTransparency[k][j + 1] = renderTargetIdentifier;
						VolumetricTransparencyI[k][j] = renderTargetIdentifier;
					}
				}
			}
		}
		if (downScaledBlurRT == null && (blurCount > 0 || ((BlurTransparency > 0 || MapToLDR) && TransparencySupport)) && resolution != 0)
		{
			downScaledBlurRT = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
			downScaledBlurRT.filterMode = FilterMode.Bilinear;
			downScaledBlurRTID = new RenderTargetIdentifier(downScaledBlurRT);
			downScaledBlurRT.hideFlags = HideFlags.DontSave;
		}
		if (FullBlurRT2 == null && ((resolution != 0 && UpSampledblurCount > 0) || (resolution == Resolution.full && (blurCount > 0 || ((BlurTransparency > 0 || MapToLDR) && TransparencySupport) || TemporalSampling)) || MapToLDR))
		{
			FullBlurRT2 = RenderTexture.GetTemporary(GetCamPixelWidth(), GetCamPixelHeight(), 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
			FullBlurRT2.hideFlags = HideFlags.DontSave;
			FullBlurRT2.filterMode = FilterMode.Bilinear;
			FullBlurRT2ID = new RenderTargetIdentifier(FullBlurRT2);
			if (resolution != 0)
			{
				VolumetricUpsampledBlurTextures[0] = FullBlurRTID;
				VolumetricUpsampledBlurTextures[1] = FullBlurRT2ID;
			}
		}
		width = Mathf.CeilToInt((float)GetCamPixelWidth() * ResolutionScale[Mathf.Max((int)resolution, (int)densityResolution)]);
		height = Mathf.CeilToInt((float)GetCamPixelHeight() * ResolutionScale[Mathf.Max((int)resolution, (int)densityResolution)]);
		if (!ParticleDensitySupport)
		{
			return;
		}
		for (int l = 0; l < (int)(compatibleDBuffer() + 1); l++)
		{
			if (VolumetricDensityTextures[l] == null)
			{
				VolumetricDensityTextures[l] = RenderTexture.GetTemporary(width, height, (l == 0) ? 16 : 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
				VolumetricDensityTextures[l].hideFlags = HideFlags.DontSave;
				VolumetricDensityTextures[l].filterMode = FilterMode.Bilinear;
				RenderTargetIdentifier renderTargetIdentifier2 = new RenderTargetIdentifier(VolumetricDensityTextures[l]);
				for (int m = Mathf.Max(l, 0); m < EnumBufferDepthLength; m++)
				{
					VolumetricDensity[m][l] = renderTargetIdentifier2;
				}
			}
		}
	}

	public static void ConstructPlanes(Camera cam, float near, float far)
	{
		Vector3 position = cam.transform.position;
		Vector3 forward = cam.transform.forward;
		Vector3 right = cam.transform.right;
		Vector3 up = cam.transform.up;
		Vector3 vector = position + forward * far;
		Vector3 vector2 = position + forward * near;
		float num = Mathf.Tan(cam.fieldOfView * ((float)Math.PI / 180f) / 2f) * far;
		float num2 = num * cam.aspect;
		float num3 = Mathf.Tan(cam.fieldOfView * ((float)Math.PI / 180f) / 2f) * near;
		float num4 = num * cam.aspect;
		Vector3 vector3 = vector + up * num - right * num2;
		Vector3 vector4 = vector + up * num + right * num2;
		Vector3 vector5 = vector - up * num - right * num2;
		Vector3 vector6 = vector - up * num + right * num2;
		Vector3 a = vector2 + up * num3 - right * num4;
		Vector3 b = vector2 + up * num3 + right * num4;
		Vector3 c = vector2 - up * num3 - right * num4;
		CameraPlanes[0] = new Plane(vector5, vector3, vector4);
		CameraPlanes[1] = new Plane(a, b, c);
		CameraPlanes[2] = new Plane(position, vector3, vector5);
		CameraPlanes[3] = new Plane(position, vector6, vector4);
		CameraPlanes[4] = new Plane(position, vector5, vector6);
		CameraPlanes[5] = new Plane(position, vector4, vector3);
		MinBounds = new Vector3(Mathf.Min(vector3.x, Mathf.Min(vector4.x, Mathf.Min(vector5.x, Mathf.Min(vector6.x, position.x)))), Mathf.Min(vector3.y, Mathf.Min(vector4.y, Mathf.Min(vector5.y, Mathf.Min(vector6.y, position.y)))), Mathf.Min(vector3.z, Mathf.Min(vector4.z, Mathf.Min(vector5.z, Mathf.Min(vector6.z, position.z)))));
		MaxBounds = new Vector3(Mathf.Max(vector3.x, Mathf.Max(vector4.x, Mathf.Max(vector5.x, Mathf.Max(vector6.x, position.x)))), Mathf.Max(vector3.y, Mathf.Max(vector4.y, Mathf.Max(vector5.y, Mathf.Max(vector6.y, position.y)))), Mathf.Max(vector3.z, Mathf.Max(vector4.z, Mathf.Max(vector5.z, Mathf.Max(vector6.z, position.z)))));
	}

	private void FindActiveLights()
	{
		ActiveLights.Clear();
		ActiveVolumes.Clear();
		if (LightOctree != null)
		{
			LightOctree.GetObjectsBoundsPlane(ref CameraPlanes, MinBounds, MaxBounds, ActiveLights);
		}
		for (int i = 0; i < ActiveDirectionalLights.Count; i++)
		{
			ActiveLights.Add(ActiveDirectionalLights[i]);
		}
		if (HxDensityVolume.DensityOctree != null)
		{
			HxDensityVolume.DensityOctree.GetObjectsBoundsPlane(ref CameraPlanes, MinBounds, MaxBounds, ActiveVolumes);
			List<HxDensityVolume> activeVolumes = ActiveVolumes;
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CFindActiveLights_003Em__0;
			}
			activeVolumes.Sort(_003C_003Ef__am_0024cache0);
		}
	}

	private void FindActiveParticleSystems()
	{
		ActiveParticleSystems.Clear();
		if (ParticleOctree != null)
		{
			ParticleOctree.GetObjectsBoundsPlane(ref CameraPlanes, MinBounds, MaxBounds, ActiveParticleSystems);
		}
	}

	public void Update()
	{
		OffsetUpdated = false;
		if (Mycamera == null)
		{
			Mycamera = GetComponent<Camera>();
		}
		if (Mycamera != null)
		{
			if (BoxMesh == null)
			{
				BoxMesh = CreateBox();
			}
			if (ShadowMaterial == null)
			{
				ShadowMaterial = new Material(Shader.Find("Hidden/HxShadowCasterFix"));
				ShadowMaterial.hideFlags = HideFlags.DontSave;
			}
			if (ShadowFix)
			{
				Graphics.DrawMesh(BoxMesh, Matrix4x4.TRS(base.transform.position, Quaternion.identity, new Vector3(Mathf.Max(MaxDirectionalRayDistance, MaxLightDistance), Mathf.Max(MaxDirectionalRayDistance, MaxLightDistance), Mathf.Max(MaxDirectionalRayDistance, MaxLightDistance)) * 2f), ShadowMaterial, 0);
			}
		}
		else
		{
			base.enabled = false;
		}
	}

	private void Start()
	{
		FinalizeBufferDirty = true;
		SetupBufferDirty = true;
		if (preCullEventAdded)
		{
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(MyPreCull));
			preCullEventAdded = true;
		}
	}

	private void OnEnable()
	{
		FinalizeBufferDirty = true;
		SetupBufferDirty = true;
		if (preCullEventAdded)
		{
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(MyPreCull));
			preCullEventAdded = true;
		}
	}

	private void CreateApplyBuffer()
	{
	}

	private void CreateSetupBuffer()
	{
		if (SetupBufferDirty && SetupBufferAdded)
		{
			Mycamera.RemoveCommandBuffer(lastSetup, BufferSetup);
			SetupBufferAdded = false;
			SetupBufferDirty = false;
		}
		if (!SetupBufferAdded)
		{
			if (BufferSetup == null)
			{
				BufferSetup = new CommandBuffer();
				BufferSetup.name = "VolumetricSetup";
			}
			else
			{
				BufferSetup.Clear();
			}
			if (TransparencySupport)
			{
				BufferSetup.SetRenderTarget(VolumetricTransparencyI[(int)compatibleTBuffer()], ScaledDepthTextureRTID[(int)Active.resolution]);
				BufferSetup.ClearRenderTarget(false, true, new Color32(0, 0, 0, 0));
			}
			BufferSetup.SetRenderTarget(ScaledDepthTextureRTID[(int)resolution]);
			BufferSetup.DrawMesh(QuadMesh, Matrix4x4.identity, DownSampleMaterial, 0, (int)resolution);
			BufferSetup.SetGlobalTexture(ScaledDepthTexturePID, ScaledDepthTextureRTID[(int)resolution]);
			lastSetup = SetupEvent;
			Mycamera.AddCommandBuffer(SetupEvent, BufferSetup);
			SetupBufferAdded = true;
		}
	}

	private bool CheckBufferDirty()
	{
		bool flag = true;
		if (TemporalSampling && TemporalFirst)
		{
			flag = true;
		}
		if (lastDownDepthFalloff != DownsampledBlurDepthFalloff)
		{
			flag = true;
			lastDownDepthFalloff = DownsampledBlurDepthFalloff;
		}
		if (lastDepthFalloff != BlurDepthFalloff)
		{
			flag = true;
			lastDepthFalloff = BlurDepthFalloff;
		}
		if (lastDensityRes != (int)densityResolution)
		{
			flag = true;
			lastDensityRes = (int)densityResolution;
		}
		if (lastTransparency != (TransparencySupport ? 1 : 0))
		{
			flag = true;
			lastTransparency = (TransparencySupport ? 1 : 0);
		}
		if (lastDensity != (ParticleDensitySupport ? 1 : 0))
		{
			flag = true;
			lastDensity = (ParticleDensitySupport ? 1 : 0);
		}
		if (lastGaussian != (GaussianWeights ? 1 : 0))
		{
			flag = true;
			lastGaussian = (GaussianWeights ? 1 : 0);
		}
		if (lastPath != (int)Mycamera.actualRenderingPath)
		{
			flag = true;
			lastPath = (int)Mycamera.actualRenderingPath;
		}
		if (lastBanding != (RemoveColorBanding ? 1 : 0))
		{
			flag = true;
			lastBanding = (RemoveColorBanding ? 1 : 0);
		}
		if (GetCamPixelHeight() != lastH)
		{
			flag = true;
			lastH = GetCamPixelHeight();
		}
		if (GetCamPixelWidth() != lastW)
		{
			flag = true;
			lastW = GetCamPixelWidth();
		}
		if (lastLDR != (MapToLDR ? 1 : 0))
		{
			flag = true;
			lastLDR = (MapToLDR ? 1 : 0);
		}
		if (lastupSampleBlurCount != UpSampledblurCount)
		{
			lastupSampleBlurCount = UpSampledblurCount;
			flag = true;
		}
		if (lastBlurCount != blurCount)
		{
			lastBlurCount = blurCount;
			flag = true;
		}
		if (lastRes != (int)resolution)
		{
			lastRes = (int)resolution;
			flag = true;
		}
		if (Application.isPlaying)
		{
			if (!LastPlaying)
			{
				flag = true;
			}
			LastPlaying = true;
		}
		else
		{
			if (LastPlaying)
			{
				flag = true;
			}
			LastPlaying = false;
		}
		if (flag)
		{
			FinalizeBufferDirty = true;
			SetupBufferDirty = true;
			return true;
		}
		return false;
	}

	private void CreateFinalizeBuffer()
	{
		if (FinalizeBufferAdded)
		{
			return;
		}
		bool flag = true;
		bool flag2 = true;
		if ((BlurTransparency > 0 || MapToLDR) && TransparencySupport)
		{
			int num = (int)compatibleTBuffer();
			int num2 = Mathf.Max(BlurTransparency, 1);
			for (int i = 0; i < num + 1; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					BufferFinalize.SetRenderTarget((resolution != 0) ? downScaledBlurRTID : FullBlurRT2ID);
					BufferFinalize.SetGlobalTexture("_MainTex", VolumetricTransparencyI[num][i]);
					BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, TransparencyBlurMaterial, 0, 0);
					BufferFinalize.SetRenderTarget(VolumetricTransparencyI[num][i]);
					BufferFinalize.SetGlobalTexture("_MainTex", (resolution != 0) ? downScaledBlurRTID : FullBlurRT2ID);
					BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, TransparencyBlurMaterial, 0, (!MapToLDR || j != num2 - 1) ? 1 : 2);
				}
			}
		}
		if (blurCount > 0 && resolution != 0)
		{
			BufferFinalize.SetGlobalFloat(BlurDepthFalloffPID, DownsampledBlurDepthFalloff);
			for (int k = 0; k < blurCount; k++)
			{
				if (flag)
				{
					BufferFinalize.SetRenderTarget(downScaledBlurRTID);
					BufferFinalize.SetGlobalTexture("_MainTex", VolumetricTextureRTID);
					BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, VolumeBlurMaterial, 0, GaussianWeights ? 2 : 0);
				}
				else
				{
					BufferFinalize.SetRenderTarget(VolumetricTextureRTID);
					BufferFinalize.SetGlobalTexture("_MainTex", downScaledBlurRTID);
					BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, VolumeBlurMaterial, 0, GaussianWeights ? 2 : 0);
				}
				flag = !flag;
			}
		}
		if (resolution != 0)
		{
			if (TemporalSampling)
			{
				BufferFinalize.SetGlobalTexture("hxLastVolumetric", TemporalTextureRTID);
				BufferFinalize.SetGlobalVector("hxTemporalSettings", new Vector4(LuminanceFeedback, MaxFeedback, 0f, 0f));
			}
			if (UpSampledblurCount == 0)
			{
				BufferFinalize.SetRenderTarget(FullBlurRT);
				BufferFinalize.SetGlobalTexture("_MainTex", (!flag) ? downScaledBlurRTID : VolumetricTextureRTID);
				BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, ApplyDirectMaterial, 0, (TemporalSampling && !TemporalFirst) ? 6 : 0);
			}
			else
			{
				BufferFinalize.SetGlobalTexture("_MainTex", (!flag) ? downScaledBlurRTID : VolumetricTextureRTID);
				BufferFinalize.SetRenderTarget(VolumetricUpsampledBlurTextures, FullBlurRT);
				BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, ApplyDirectMaterial, 0, (!TemporalSampling || TemporalFirst) ? 5 : 7);
			}
			if (UpSampledblurCount > 0)
			{
				BufferFinalize.SetGlobalFloat(BlurDepthFalloffPID, BlurDepthFalloff);
				if (UpSampledblurCount % 2 != 0)
				{
					flag2 = false;
				}
				for (int l = 0; l < UpSampledblurCount; l++)
				{
					if (flag2)
					{
						BufferFinalize.SetRenderTarget(FullBlurRT2ID);
						BufferFinalize.SetGlobalTexture("_MainTex", FullBlurRTID);
						BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, VolumeBlurMaterial, 0, 1);
					}
					else
					{
						BufferFinalize.SetRenderTarget(FullBlurRTID);
						BufferFinalize.SetGlobalTexture("_MainTex", FullBlurRT2ID);
						BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, VolumeBlurMaterial, 0, 1);
					}
					flag2 = !flag2;
				}
			}
			if (MapToLDR)
			{
				BufferFinalize.SetGlobalTexture(VolumetricTexturePID, FullBlurRTID);
				BufferFinalize.SetRenderTarget(TemporalTexture);
				BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, ApplyDirectMaterial, 0, 8);
				BufferFinalize.SetRenderTarget(FullBlurRT2);
				BufferFinalize.SetGlobalTexture("_MainTex", FullBlurRTID);
				BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, TransparencyBlurMaterial, 0, 3);
				BufferFinalize.SetGlobalTexture(VolumetricTexturePID, FullBlurRT2);
			}
			else
			{
				BufferFinalize.SetGlobalTexture(VolumetricTexturePID, FullBlurRT);
			}
		}
		else if (blurCount > 0 || TemporalSampling)
		{
			if (TemporalSampling)
			{
				BufferFinalize.SetGlobalTexture("hxLastVolumetric", TemporalTextureRTID);
				BufferFinalize.SetGlobalVector("hxTemporalSettings", new Vector4(LuminanceFeedback, MaxFeedback, 0f, 0f));
			}
			BufferFinalize.SetGlobalFloat(BlurDepthFalloffPID, BlurDepthFalloff);
			flag2 = true;
			for (int m = 0; m < blurCount; m++)
			{
				if (flag2)
				{
					BufferFinalize.SetRenderTarget(FullBlurRT2ID);
					BufferFinalize.SetGlobalTexture("_MainTex", VolumetricTextureRTID);
					BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, VolumeBlurMaterial, 0, (!GaussianWeights) ? 4 : 5);
				}
				else
				{
					BufferFinalize.SetRenderTarget(VolumetricTextureRTID);
					BufferFinalize.SetGlobalTexture("_MainTex", FullBlurRT2ID);
					BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, VolumeBlurMaterial, 0, (!GaussianWeights) ? 4 : 5);
				}
				flag2 = !flag2;
			}
			if (!flag2)
			{
				if (MapToLDR)
				{
					if (TemporalSampling && !TemporalFirst)
					{
						BufferFinalize.SetRenderTarget(VolumetricTextureRTID);
						BufferFinalize.SetGlobalTexture("_MainTex", FullBlurRT2);
						BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, TransparencyBlurMaterial, 0, 5);
						BufferFinalize.SetGlobalTexture(VolumetricTexturePID, VolumetricTextureRTID);
						TemporalFirst = false;
						BufferFinalize.SetRenderTarget(TemporalTexture);
						BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, ApplyDirectMaterial, 0, 8);
						BufferFinalize.SetRenderTarget(FullBlurRT2);
						BufferFinalize.SetGlobalTexture("_MainTex", VolumetricTextureRTID);
						BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, TransparencyBlurMaterial, 0, (!TemporalSampling || TemporalFirst) ? 3 : 4);
						BufferFinalize.SetGlobalTexture(VolumetricTexturePID, FullBlurRT2);
					}
					else
					{
						BufferFinalize.SetRenderTarget(VolumetricTextureRTID);
						BufferFinalize.SetGlobalTexture("_MainTex", FullBlurRT2ID);
						BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, TransparencyBlurMaterial, 0, 3);
						BufferFinalize.SetGlobalTexture(VolumetricTexturePID, VolumetricTexture);
					}
				}
				else if (TemporalSampling && !TemporalFirst)
				{
					BufferFinalize.SetRenderTarget(VolumetricTextureRTID);
					BufferFinalize.SetGlobalTexture("_MainTex", FullBlurRT2);
					BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, TransparencyBlurMaterial, 0, 5);
					BufferFinalize.SetGlobalTexture(VolumetricTexturePID, VolumetricTextureRTID);
				}
				else
				{
					BufferFinalize.SetGlobalTexture(VolumetricTexturePID, FullBlurRT2);
				}
			}
			else if (MapToLDR)
			{
				if (TemporalSampling && !TemporalFirst)
				{
					BufferFinalize.SetRenderTarget(FullBlurRT2);
					BufferFinalize.SetGlobalTexture("_MainTex", VolumetricTextureRTID);
					BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, TransparencyBlurMaterial, 0, 5);
					BufferFinalize.SetGlobalTexture(VolumetricTexturePID, FullBlurRT2);
					BufferFinalize.SetRenderTarget(TemporalTexture);
					BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, ApplyDirectMaterial, 0, 8);
					BufferFinalize.SetRenderTarget(VolumetricTextureRTID);
					BufferFinalize.SetGlobalTexture("_MainTex", FullBlurRT2ID);
					BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, TransparencyBlurMaterial, 0, 3);
					BufferFinalize.SetGlobalTexture(VolumetricTexturePID, VolumetricTexture);
				}
				else
				{
					BufferFinalize.SetRenderTarget(FullBlurRT2);
					BufferFinalize.SetGlobalTexture("_MainTex", VolumetricTextureRTID);
					BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, TransparencyBlurMaterial, 0, (!TemporalSampling || TemporalFirst) ? 3 : 4);
					BufferFinalize.SetGlobalTexture(VolumetricTexturePID, FullBlurRT2);
				}
			}
			else if (TemporalSampling && !TemporalFirst)
			{
				BufferFinalize.SetRenderTarget(FullBlurRT2);
				BufferFinalize.SetGlobalTexture("_MainTex", VolumetricTextureRTID);
				BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, TransparencyBlurMaterial, 0, 5);
				BufferFinalize.SetGlobalTexture(VolumetricTexturePID, FullBlurRT2);
			}
			else
			{
				BufferFinalize.SetGlobalTexture(VolumetricTexturePID, VolumetricTextureRTID);
			}
		}
		else if (MapToLDR)
		{
			BufferFinalize.SetRenderTarget(FullBlurRT2);
			BufferFinalize.SetGlobalTexture("_MainTex", VolumetricTextureRTID);
			BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, TransparencyBlurMaterial, 0, 3);
			BufferFinalize.SetGlobalTexture(VolumetricTexturePID, FullBlurRT2);
		}
		else
		{
			BufferFinalize.SetGlobalTexture(VolumetricTexturePID, VolumetricTextureRTID);
		}
		if (TemporalSampling)
		{
			if (MapToLDR)
			{
				TemporalFirst = false;
			}
			else
			{
				TemporalFirst = false;
				BufferFinalize.SetRenderTarget(TemporalTexture);
				BufferFinalize.DrawMesh(QuadMesh, Matrix4x4.identity, ApplyDirectMaterial, 0, 8);
			}
		}
		else
		{
			TemporalFirst = true;
		}
		lastFinalize = FinalizeEvent;
		lastRender = RenderEvent;
		lastLightRender = LightRenderEvent;
		lastFinalize = FinalizeEvent;
		Mycamera.AddCommandBuffer(FinalizeEvent, BufferFinalize);
		FinalizeBufferAdded = true;
	}

	private void BuildBuffer()
	{
		if (BuffersBuilt)
		{
			if (BufferRender != null)
			{
				Mycamera.RemoveCommandBuffer(lastRender, BufferRender);
			}
			BuffersBuilt = false;
		}
		CreatePIDs();
		CalculateEvent();
		DefineFull();
		CheckTemporalTextures();
		if (CheckBufferDirty())
		{
			ReleaseTempTextures();
		}
		CreateTempTextures();
		Active = this;
		ActiveCamera = Mycamera;
		if (FinalizeBufferDirty && FinalizeBufferAdded)
		{
			Mycamera.RemoveCommandBuffer(lastFinalize, BufferFinalize);
			FinalizeBufferAdded = false;
			FinalizeBufferDirty = false;
		}
		if (!FinalizeBufferAdded)
		{
			if (BufferFinalize == null)
			{
				BufferFinalize = new CommandBuffer();
				BufferFinalize.name = "VolumetricFinalize";
			}
			else
			{
				BufferFinalize.Clear();
			}
		}
		CreateSetupBuffer();
		CreateApplyBuffer();
		if (resolution == Resolution.full)
		{
			FullUsed = true;
		}
		else
		{
			LowResUsed = true;
		}
		CurrentTint = new Vector3(((QualitySettings.activeColorSpace != 0) ? TintColor.linear : TintColor).r, ((QualitySettings.activeColorSpace != 0) ? TintColor.linear : TintColor).g, ((QualitySettings.activeColorSpace != 0) ? TintColor.linear : TintColor).b) * TintIntensity;
		CurrentTintEdge = new Vector3(((QualitySettings.activeColorSpace != 0) ? TintColor2.linear : TintColor2).r, ((QualitySettings.activeColorSpace != 0) ? TintColor2.linear : TintColor2).g, ((QualitySettings.activeColorSpace != 0) ? TintColor2.linear : TintColor2).b) * TintIntensity;
		if (!dirty)
		{
			return;
		}
		if (BufferRender == null)
		{
			BufferRender = new CommandBuffer();
			BufferRender.name = "VolumetricRender";
		}
		else
		{
			BufferRender.Clear();
		}
		if (TemporalSampling)
		{
			Matrix4x4 currentView = CurrentView;
			Matrix4x4 gPUProjectionMatrix = GL.GetGPUProjectionMatrix(CurrentProj, false);
			LastMatrixVP = gPUProjectionMatrix * currentView;
			BufferRender.SetGlobalMatrix("hxLastVP", LastMatrixVP);
			if (IsRenderBoth())
			{
				currentView = CurrentView2;
				gPUProjectionMatrix = GL.GetGPUProjectionMatrix(CurrentProj2, false);
				LastMatrixVP = gPUProjectionMatrix * currentView;
				BufferRender.SetGlobalMatrix("hxLastVP2", LastMatrixVP);
			}
		}
		if (Mycamera.stereoTargetEye != 0 && Application.isPlaying && XRSettings.enabled && HxUtil.isPresent())
		{
			Camera.StereoscopicEye stereoscopicEye = Camera.StereoscopicEye.Right;
			if (!IsRenderBoth())
			{
				stereoscopicEye = ((Mycamera.stereoTargetEye == StereoTargetEyeMask.Right) ? Camera.StereoscopicEye.Right : Camera.StereoscopicEye.Left);
			}
			else
			{
				SinglePassStereoUsed = true;
			}
			CurrentProj = Mycamera.GetStereoProjectionMatrix(stereoscopicEye);
			CurrentView = Mycamera.GetStereoViewMatrix(stereoscopicEye);
			CurrentInvers = CurrentProj.inverse;
			if (IsRenderBoth())
			{
				CurrentProj2 = Mycamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
				CurrentView2 = Mycamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
			}
			if (IsRenderBoth())
			{
				Matrix4x4 worldToCameraMatrix = Mycamera.worldToCameraMatrix;
				Matrix4x4 worldToCameraMatrix2 = Mycamera.worldToCameraMatrix;
				worldToCameraMatrix[12] += Mycamera.stereoSeparation / 2f;
				worldToCameraMatrix2[12] -= Mycamera.stereoSeparation / 2f;
				BufferRender.SetGlobalMatrix("hxCameraToWorld", worldToCameraMatrix.inverse);
				BufferRender.SetGlobalMatrix("hxCameraToWorld2", worldToCameraMatrix2.inverse);
			}
			else
			{
				Matrix4x4 worldToCameraMatrix3 = Mycamera.worldToCameraMatrix;
				worldToCameraMatrix3[12] += Mycamera.stereoSeparation / 2f * (float)((stereoscopicEye != 0) ? 1 : (-1));
				BufferRender.SetGlobalMatrix("hxCameraToWorld", worldToCameraMatrix3.inverse);
			}
		}
		else
		{
			CurrentView = Mycamera.worldToCameraMatrix;
			BufferRender.SetGlobalMatrix("hxCameraToWorld", Mycamera.cameraToWorldMatrix);
			CurrentProj = Mycamera.projectionMatrix;
			CurrentInvers = Mycamera.projectionMatrix.inverse;
		}
		Matrix4x4 gPUProjectionMatrix2 = GL.GetGPUProjectionMatrix(CurrentProj, true);
		MatrixVP = gPUProjectionMatrix2 * CurrentView;
		MatrixV = CurrentView;
		Matrix4x4 currentView2 = CurrentView;
		Matrix4x4 gPUProjectionMatrix3 = GL.GetGPUProjectionMatrix(CurrentProj, false);
		Matrix4x4 inverse = (gPUProjectionMatrix3 * currentView2).inverse;
		BufferRender.SetGlobalMatrix("_InvViewProj", inverse);
		BlitScale.z = ActiveCamera.nearClipPlane + 1f;
		BlitScale.y = (ActiveCamera.nearClipPlane + 1f) * Mathf.Tan((float)Math.PI / 180f * ActiveCamera.fieldOfView * 0.51f);
		BlitScale.x = BlitScale.y * ActiveCamera.aspect;
		BlitMatrix = Matrix4x4.TRS(Active.transform.position, Active.transform.rotation, BlitScale);
		BlitMatrixMVP = Active.MatrixVP * BlitMatrix;
		BlitMatrixMV = Active.MatrixV * BlitMatrix;
		if (TemporalSampling)
		{
			currentDitherOffset += DitherSpeed;
			if (currentDitherOffset > 1f)
			{
				currentDitherOffset -= 1f;
			}
			BufferRender.SetGlobalFloat("hxRayOffset", currentDitherOffset);
		}
		BufferRender.SetGlobalMatrix(HxVolumetricLight.VolumetricMVPPID, BlitMatrixMVP);
		BufferRender.SetGlobalMatrix(HxVolumetricLight.VolumetricMVPID, BlitMatrixMV);
		Matrix4x4 gPUProjectionMatrix4 = GL.GetGPUProjectionMatrix(Mycamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left), true);
		Matrix4x4 stereoViewMatrix = Mycamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
		BufferRender.SetGlobalMatrix(HxVolumetricLight.VolumetricMVP2PID, gPUProjectionMatrix4 * stereoViewMatrix * BlitMatrix);
		BufferRender.SetGlobalMatrix(InverseProjectionMatrix2PID, Mycamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left).inverse);
		BufferRender.SetGlobalMatrix("InverseProjectionMatrix1", Mycamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right).inverse);
		BufferRender.SetGlobalMatrix("hxInverseP1", GL.GetGPUProjectionMatrix(Mycamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left), false).inverse);
		BufferRender.SetGlobalMatrix("hxInverseP2", GL.GetGPUProjectionMatrix(Mycamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right), false).inverse);
		RenderParticles();
		BufferRender.SetRenderTarget(VolumetricTextureRTID, ScaledDepthTextureRTID[(int)Active.resolution]);
		BufferRender.ClearRenderTarget(false, true, new Color(0f, 0f, 0f, 0f));
		BufferRender.SetGlobalFloat(DepthThresholdPID, DepthThreshold);
		BufferRender.SetGlobalVector("CameraFoward", base.transform.forward);
		BufferRender.SetGlobalFloat(BlurDepthFalloffPID, BlurDepthFalloff);
		BufferRender.SetGlobalFloat(VolumeScalePID, ResolutionScale[(int)resolution]);
		BufferRender.SetGlobalMatrix(InverseViewMatrixPID, Mycamera.cameraToWorldMatrix);
		BufferRender.SetGlobalMatrix(InverseProjectionMatrixPID, CurrentInvers);
		if (!OffsetUpdated)
		{
			OffsetUpdated = true;
			Offset += NoiseVelocity * Time.deltaTime;
		}
		BufferRender.SetGlobalVector(NoiseOffsetPID, Offset);
		BufferRender.SetGlobalFloat(ShadowDistancePID, QualitySettings.shadowDistance);
		CreateLightbuffers();
		CreateFinalizeBuffer();
		BuffersBuilt = true;
		Mycamera.AddCommandBuffer(RenderEvent, BufferRender);
		Mycamera.AddCommandBuffer(LightRenderEvent, BufferRenderLights);
	}

	private void OnDestroy()
	{
		if (TemporalTexture != null)
		{
			RenderTexture.ReleaseTemporary(TemporalTexture);
			TemporalTexture = null;
		}
		if (!preCullEventAdded)
		{
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(MyPreCull));
			preCullEventAdded = false;
		}
		if (Active == this)
		{
			Active.ReleaseLightBuffers();
			ReleaseTempTextures();
		}
		if (BuffersBuilt)
		{
			if (BufferRenderLights != null && LightBufferAdded)
			{
				Mycamera.RemoveCommandBuffer(lastLightRender, BufferRenderLights);
				LightBufferAdded = false;
			}
			if (BufferSetup != null && SetupBufferAdded)
			{
				Mycamera.RemoveCommandBuffer(lastSetup, BufferSetup);
				SetupBufferAdded = false;
			}
			if (BufferRender != null)
			{
				Mycamera.RemoveCommandBuffer(lastRender, BufferRender);
			}
			if (BufferFinalize != null && FinalizeBufferAdded)
			{
				Mycamera.RemoveCommandBuffer(lastFinalize, BufferFinalize);
				FinalizeBufferAdded = false;
			}
			BuffersBuilt = false;
		}
		SaveUsedShaderVarience();
		if (callBackImageEffect != null)
		{
			callBackImageEffect.enabled = false;
		}
		if (callBackImageEffectOpaque != null)
		{
			callBackImageEffectOpaque.enabled = false;
		}
	}

	private void SaveUsedShaderVarience()
	{
	}

	private void OnDisable()
	{
		if (TemporalTexture != null)
		{
			RenderTexture.ReleaseTemporary(TemporalTexture);
			TemporalTexture = null;
			TemporalFirst = true;
		}
		if (!preCullEventAdded)
		{
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(MyPreCull));
			preCullEventAdded = false;
		}
		if (Active == this)
		{
			Active.ReleaseLightBuffers();
			ReleaseTempTextures();
		}
		if (BuffersBuilt)
		{
			if (BufferRenderLights != null && LightBufferAdded)
			{
				Mycamera.RemoveCommandBuffer(lastLightRender, BufferRenderLights);
				LightBufferAdded = false;
			}
			if (BufferSetup != null && SetupBufferAdded)
			{
				Mycamera.RemoveCommandBuffer(lastSetup, BufferSetup);
				SetupBufferAdded = false;
			}
			if (BufferRender != null)
			{
				Mycamera.RemoveCommandBuffer(lastRender, BufferRender);
			}
			if (BufferFinalize != null && FinalizeBufferAdded)
			{
				Mycamera.RemoveCommandBuffer(lastFinalize, BufferFinalize);
				FinalizeBufferAdded = false;
			}
			BuffersBuilt = false;
		}
		if (callBackImageEffect != null)
		{
			callBackImageEffect.enabled = false;
		}
		if (callBackImageEffectOpaque != null)
		{
			callBackImageEffectOpaque.enabled = false;
		}
	}

	private void CalculateEvent()
	{
		switch (Mycamera.actualRenderingPath)
		{
		case RenderingPath.DeferredLighting:
			SetupEvent = CameraEvent.BeforeLighting;
			RenderEvent = CameraEvent.BeforeLighting;
			LightRenderEvent = CameraEvent.BeforeLighting;
			FinalizeEvent = CameraEvent.AfterLighting;
			break;
		case RenderingPath.DeferredShading:
			SetupEvent = CameraEvent.BeforeLighting;
			RenderEvent = CameraEvent.BeforeLighting;
			LightRenderEvent = CameraEvent.BeforeLighting;
			FinalizeEvent = CameraEvent.AfterLighting;
			break;
		case RenderingPath.Forward:
			if (Mycamera.depthTextureMode == DepthTextureMode.None)
			{
				Mycamera.depthTextureMode = DepthTextureMode.Depth;
			}
			if (Mycamera.depthTextureMode == DepthTextureMode.Depth || Mycamera.depthTextureMode == (DepthTextureMode.Depth | DepthTextureMode.MotionVectors))
			{
				RenderEvent = CameraEvent.BeforeDepthTexture;
				SetupEvent = CameraEvent.AfterDepthTexture;
			}
			else
			{
				RenderEvent = CameraEvent.BeforeDepthNormalsTexture;
				SetupEvent = CameraEvent.AfterDepthNormalsTexture;
			}
			FinalizeEvent = CameraEvent.AfterForwardOpaque;
			LightRenderEvent = CameraEvent.BeforeForwardOpaque;
			break;
		}
	}

	public void EventOnRenderImage(RenderTexture src, RenderTexture dest)
	{
		Graphics.Blit(src, dest, ApplyMaterial, ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? 1 : 2) + ((!RemoveColorBanding) ? 2 : 0));
	}

	private int ScalePass()
	{
		if (resolution == Resolution.half)
		{
			return 0;
		}
		if (resolution == Resolution.quarter)
		{
			return 1;
		}
		return 2;
	}

	private void DownSampledFullBlur(RenderTexture mainColor, RenderBuffer NewColor, RenderBuffer depth, int pass)
	{
		Graphics.SetRenderTarget(NewColor, depth);
		VolumeBlurMaterial.SetTexture("_MainTex", mainColor);
		GL.PushMatrix();
		VolumeBlurMaterial.SetPass(pass);
		GL.LoadOrtho();
		GL.Begin(7);
		GL.Color(Color.red);
		GL.Vertex3(0f, 0f, 0f);
		GL.Vertex3(1f, 0f, 0f);
		GL.Vertex3(1f, 1f, 0f);
		GL.Vertex3(0f, 1f, 0f);
		GL.End();
		GL.PopMatrix();
	}

	private void CheckTemporalTextures()
	{
		if (TemporalSampling)
		{
			if (TemporalTexture != null && (TemporalTexture.width != GetCamPixelWidth() || TemporalTexture.height != GetCamPixelHeight()))
			{
				RenderTexture.ReleaseTemporary(TemporalTexture);
				TemporalTexture = null;
				TemporalFirst = true;
			}
			if (TemporalTexture == null)
			{
				TemporalTexture = RenderTexture.GetTemporary(GetCamPixelWidth(), GetCamPixelHeight(), 16, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
				TemporalTextureRTID = new RenderTargetIdentifier(TemporalTexture);
				TemporalTexture.hideFlags = HideFlags.DontSave;
			}
		}
		else if (TemporalTexture != null)
		{
			RenderTexture.ReleaseTemporary(TemporalTexture);
			TemporalTexture = null;
			TemporalFirst = true;
		}
	}

	public static void ReleaseTempTextures()
	{
		if (VolumetricTexture != null)
		{
			RenderTexture.ReleaseTemporary(VolumetricTexture);
			VolumetricTexture = null;
		}
		if (FullBlurRT != null)
		{
			RenderTexture.ReleaseTemporary(FullBlurRT);
			FullBlurRT = null;
		}
		for (int i = 0; i < VolumetricTransparencyTextures.Length; i++)
		{
			if (VolumetricTransparencyTextures[i] != null)
			{
				RenderTexture.ReleaseTemporary(VolumetricTransparencyTextures[i]);
				VolumetricTransparencyTextures[i] = null;
			}
		}
		for (int j = 0; j < VolumetricDensityTextures.Length; j++)
		{
			if (VolumetricDensityTextures[j] != null)
			{
				RenderTexture.ReleaseTemporary(VolumetricDensityTextures[j]);
				VolumetricDensityTextures[j] = null;
			}
		}
		if (downScaledBlurRT != null)
		{
			RenderTexture.ReleaseTemporary(downScaledBlurRT);
			downScaledBlurRT = null;
		}
		if (FullBlurRT2 != null)
		{
			RenderTexture.ReleaseTemporary(FullBlurRT2);
			FullBlurRT2 = null;
		}
		if (ScaledDepthTexture[0] != null)
		{
			RenderTexture.ReleaseTemporary(ScaledDepthTexture[0]);
			ScaledDepthTexture[0] = null;
		}
		if (ScaledDepthTexture[1] != null)
		{
			RenderTexture.ReleaseTemporary(ScaledDepthTexture[1]);
			ScaledDepthTexture[1] = null;
		}
		if (ScaledDepthTexture[2] != null)
		{
			RenderTexture.ReleaseTemporary(ScaledDepthTexture[2]);
			ScaledDepthTexture[2] = null;
		}
		if (ScaledDepthTexture[3] != null)
		{
			RenderTexture.ReleaseTemporary(ScaledDepthTexture[3]);
			ScaledDepthTexture[3] = null;
		}
	}

	private void OnPreCull()
	{
		SetUpRenderOrder();
		ReleaseLightBuffers();
		MaxLightDistanceUsed = MaxLightDistance;
		ConstructPlanes(Mycamera, 0f, MaxLightDistanceUsed);
		UpdateLightPoistions();
		UpdateParticlePoistions();
		FindActiveLights();
		BuildBuffer();
	}

	private void UpdateLightPoistions()
	{
		MaxLightDistanceUsed = MaxLightDistance;
		HashSet<HxVolumetricLight>.Enumerator enumerator = AllVolumetricLight.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.CustomMaxLightDistance)
			{
				MaxLightDistanceUsed = Mathf.Max(enumerator.Current.MaxLightDistance, MaxLightDistanceUsed);
			}
			enumerator.Current.UpdatePosition();
		}
		if (LightOctree != null)
		{
			LightOctree.TryShrink();
		}
		HashSet<HxDensityVolume>.Enumerator enumerator2 = AllDensityVolumes.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			enumerator2.Current.UpdateVolume();
		}
		if (HxDensityVolume.DensityOctree != null)
		{
			HxDensityVolume.DensityOctree.TryShrink();
		}
	}

	private void UpdateParticlePoistions()
	{
		if (ParticleDensitySupport)
		{
			HashSet<HxVolumetricParticleSystem>.Enumerator enumerator = AllParticleSystems.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.UpdatePosition();
			}
			if (ParticleOctree != null)
			{
				ParticleOctree.TryShrink();
			}
		}
	}

	private void Awake()
	{
		if (_SpotLightCookie == null)
		{
			_SpotLightCookie = (Texture2D)Resources.Load("LightSoftCookie");
		}
		CreatePIDs();
		Mycamera = GetComponent<Camera>();
	}

	private void start()
	{
		Mycamera = GetComponent<Camera>();
	}

	public void ReleaseLightBuffers()
	{
		for (int i = 0; i < ActiveLights.Count; i++)
		{
			ActiveLights[i].ReleaseBuffer();
		}
		ActiveLights.Clear();
	}

	private void CreateLightbuffers()
	{
		if (BufferRenderLights == null)
		{
			BufferRenderLights = new CommandBuffer();
			BufferRenderLights.name = "renderLights";
		}
		else
		{
			BufferRenderLights.Clear();
		}
		if (LightBufferAdded)
		{
			ActiveCamera.RemoveCommandBuffer(lastLightRender, BufferRenderLights);
			LightBufferAdded = false;
		}
		if (Active.TransparencySupport)
		{
			BufferRenderLights.SetRenderTarget(VolumetricTransparency[(int)Active.compatibleTBuffer()], ScaledDepthTextureRTID[(int)Active.resolution]);
		}
		else
		{
			BufferRenderLights.SetRenderTarget(VolumetricTextureRTID, ScaledDepthTextureRTID[(int)Active.resolution]);
		}
		FirstDirectional = true;
		for (int i = 0; i < ActiveLights.Count; i++)
		{
			ActiveLights[i].BuildBuffer(BufferRenderLights);
		}
		LightBufferAdded = true;
	}

	private static void CreateTileTexture()
	{
		Tile5x5 = Resources.Load("HxOffsetTile") as Texture2D;
		if (Tile5x5 == null)
		{
			Tile5x5 = new Texture2D(5, 5, TextureFormat.RFloat, false, true);
			Tile5x5.hideFlags = HideFlags.DontSave;
			Tile5x5.filterMode = FilterMode.Point;
			Tile5x5.wrapMode = TextureWrapMode.Repeat;
			Color[] array = new Color[25];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Color((float)Tile5x5int[i] * 0.04f, 0f, 0f, 0f);
			}
			Tile5x5.SetPixels(array);
			Tile5x5.Apply();
			Shader.SetGlobalTexture("Tile5x5", Tile5x5);
			Shader.SetGlobalFloat("HxTileSize", 5f);
		}
		else
		{
			Shader.SetGlobalTexture("Tile5x5", Tile5x5);
			Shader.SetGlobalFloat("HxTileSize", Tile5x5.width);
		}
	}

	public static Mesh CreateOrtho(int sides, bool inner = true)
	{
		Vector3[] vertices = new Vector3[8]
		{
			new Vector3(-0.5f, -0.5f, 0f),
			new Vector3(0.5f, -0.5f, 0f),
			new Vector3(0.5f, 0.5f, 0f),
			new Vector3(-0.5f, 0.5f, 0f),
			new Vector3(-0.5f, 0.5f, 1f),
			new Vector3(0.5f, 0.5f, 1f),
			new Vector3(0.5f, -0.5f, 1f),
			new Vector3(-0.5f, -0.5f, 1f)
		};
		int[] triangles = new int[36]
		{
			0, 2, 1, 0, 3, 2, 2, 3, 4, 2,
			4, 5, 1, 2, 5, 1, 5, 6, 0, 7,
			4, 0, 4, 3, 5, 4, 7, 5, 7, 6,
			0, 6, 7, 0, 1, 6
		};
		Mesh mesh = new Mesh();
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		return mesh;
	}

	public static Mesh CreateCone(int sides, bool inner = true)
	{
		Mesh mesh = new Mesh();
		Vector3[] array = new Vector3[sides + 1];
		int[] array2 = new int[sides * 3 + (sides - 2) * 3];
		float num = ((!inner) ? 1f : Mathf.Cos((float)Math.PI / (float)sides));
		float num2 = num * Mathf.Tan((float)Math.PI / (float)sides);
		Vector3 vector = new Vector3(0.5f - (1f - num) / 2f, 0f, 0f);
		Vector3 vector2 = new Vector3(0f, 0f, num2);
		vector += new Vector3(0f, 0f, num2 / 2f);
		Quaternion quaternion = Quaternion.Euler(new Vector3(0f, 360f / (float)sides, 0f));
		Quaternion quaternion2 = Quaternion.Euler(new Vector3(-90f, 0f, 0f));
		array[0] = new Vector3(0f, 0f, 0f);
		for (int i = 1; i < sides + 1; i++)
		{
			array[i] = quaternion2 * (vector - Vector3.up);
			vector -= vector2;
			vector2 = quaternion * vector2;
		}
		int num3 = 0;
		for (int j = 0; j < sides - 1; j++)
		{
			num3 = j * 3;
			array2[num3] = 0;
			array2[num3 + 1] = j + 1;
			array2[num3 + 2] = j + 2;
		}
		num3 = (sides - 1) * 3;
		array2[num3] = 0;
		array2[num3 + 1] = sides;
		array2[num3 + 2] = 1;
		num3 += 3;
		for (int k = 0; k < sides - 2; k++)
		{
			array2[num3] = 1;
			array2[num3 + 2] = k + 2;
			array2[num3 + 1] = k + 3;
			num3 += 3;
		}
		mesh.vertices = array;
		mesh.triangles = array2;
		mesh.uv = new Vector2[array.Length];
		mesh.colors = new Color[0];
		mesh.bounds = new Bounds(Vector3.zero, Vector3.one);
		mesh.RecalculateNormals();
		return mesh;
	}

	public static Mesh CreateQuad()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[4]
		{
			new Vector3(-1f, -1f, 1f),
			new Vector3(-1f, 1f, 1f),
			new Vector3(1f, -1f, 1f),
			new Vector3(1f, 1f, 1f)
		};
		mesh.triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
		mesh.RecalculateBounds();
		return mesh;
	}

	public static Mesh CreateBox()
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		Mesh sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(gameObject);
		}
		return sharedMesh;
	}

	public static Mesh CreateIcoSphere(int recursionLevel, float radius)
	{
		Mesh mesh = new Mesh();
		mesh.Clear();
		List<Vector3> vertices = new List<Vector3>();
		Dictionary<long, int> cache = new Dictionary<long, int>();
		float num = (1f + Mathf.Sqrt(5f)) / 2f;
		vertices.Add(new Vector3(-1f, num, 0f).normalized * radius);
		vertices.Add(new Vector3(1f, num, 0f).normalized * radius);
		vertices.Add(new Vector3(-1f, 0f - num, 0f).normalized * radius);
		vertices.Add(new Vector3(1f, 0f - num, 0f).normalized * radius);
		vertices.Add(new Vector3(0f, -1f, num).normalized * radius);
		vertices.Add(new Vector3(0f, 1f, num).normalized * radius);
		vertices.Add(new Vector3(0f, -1f, 0f - num).normalized * radius);
		vertices.Add(new Vector3(0f, 1f, 0f - num).normalized * radius);
		vertices.Add(new Vector3(num, 0f, -1f).normalized * radius);
		vertices.Add(new Vector3(num, 0f, 1f).normalized * radius);
		vertices.Add(new Vector3(0f - num, 0f, -1f).normalized * radius);
		vertices.Add(new Vector3(0f - num, 0f, 1f).normalized * radius);
		List<TriangleIndices> list = new List<TriangleIndices>();
		list.Add(new TriangleIndices(0, 11, 5));
		list.Add(new TriangleIndices(0, 5, 1));
		list.Add(new TriangleIndices(0, 1, 7));
		list.Add(new TriangleIndices(0, 7, 10));
		list.Add(new TriangleIndices(0, 10, 11));
		list.Add(new TriangleIndices(1, 5, 9));
		list.Add(new TriangleIndices(5, 11, 4));
		list.Add(new TriangleIndices(11, 10, 2));
		list.Add(new TriangleIndices(10, 7, 6));
		list.Add(new TriangleIndices(7, 1, 8));
		list.Add(new TriangleIndices(3, 9, 4));
		list.Add(new TriangleIndices(3, 4, 2));
		list.Add(new TriangleIndices(3, 2, 6));
		list.Add(new TriangleIndices(3, 6, 8));
		list.Add(new TriangleIndices(3, 8, 9));
		list.Add(new TriangleIndices(4, 9, 5));
		list.Add(new TriangleIndices(2, 4, 11));
		list.Add(new TriangleIndices(6, 2, 10));
		list.Add(new TriangleIndices(8, 6, 7));
		list.Add(new TriangleIndices(9, 8, 1));
		for (int i = 0; i < recursionLevel; i++)
		{
			List<TriangleIndices> list2 = new List<TriangleIndices>();
			foreach (TriangleIndices item in list)
			{
				int middlePoint = getMiddlePoint(item.v1, item.v2, ref vertices, ref cache, radius);
				int middlePoint2 = getMiddlePoint(item.v2, item.v3, ref vertices, ref cache, radius);
				int middlePoint3 = getMiddlePoint(item.v3, item.v1, ref vertices, ref cache, radius);
				list2.Add(new TriangleIndices(item.v1, middlePoint, middlePoint3));
				list2.Add(new TriangleIndices(item.v2, middlePoint2, middlePoint));
				list2.Add(new TriangleIndices(item.v3, middlePoint3, middlePoint2));
				list2.Add(new TriangleIndices(middlePoint, middlePoint2, middlePoint3));
			}
			list = list2;
		}
		mesh.vertices = vertices.ToArray();
		List<int> list3 = new List<int>();
		for (int j = 0; j < list.Count; j++)
		{
			list3.Add(list[j].v1);
			list3.Add(list[j].v2);
			list3.Add(list[j].v3);
		}
		mesh.triangles = list3.ToArray();
		mesh.uv = new Vector2[vertices.Count];
		Vector3[] array = new Vector3[vertices.Count];
		for (int k = 0; k < array.Length; k++)
		{
			array[k] = vertices[k].normalized;
		}
		mesh.normals = array;
		mesh.bounds = new Bounds(Vector3.zero, Vector3.one);
		return mesh;
	}

	private static int getMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float radius)
	{
		bool flag = p1 < p2;
		long num = ((!flag) ? p2 : p1);
		long num2 = ((!flag) ? p1 : p2);
		long key = (num << 32) + num2;
		int value;
		if (cache.TryGetValue(key, out value))
		{
			return value;
		}
		Vector3 vector = vertices[p1];
		Vector3 vector2 = vertices[p2];
		Vector3 vector3 = new Vector3((vector.x + vector2.x) / 2f, (vector.y + vector2.y) / 2f, (vector.z + vector2.z) / 2f);
		int count = vertices.Count;
		vertices.Add(vector3.normalized * radius);
		cache.Add(key, count);
		return count;
	}

	public void Create3DNoiseTexture()
	{
		NoiseTexture3D = Resources.Load("NoiseTexture") as Texture3D;
		Shader.SetGlobalTexture("NoiseTexture3D", NoiseTexture3D);
	}

	private int PostoIndex(Vector3 pos)
	{
		if (pos.x >= 32f)
		{
			pos.x = 0f;
		}
		else if (pos.x < 0f)
		{
			pos.x = 31f;
		}
		if (pos.y >= 32f)
		{
			pos.y = 0f;
		}
		else if (pos.y < 0f)
		{
			pos.y = 31f;
		}
		if (pos.z >= 32f)
		{
			pos.z = 0f;
		}
		else if (pos.z < 0f)
		{
			pos.z = 31f;
		}
		return (int)(pos.z * 32f * 32f + pos.y * 32f + pos.x);
	}

	[CompilerGenerated]
	private static int _003CFindActiveLights_003Em__0(HxDensityVolume a, HxDensityVolume b)
	{
		int blendMode = (int)a.BlendMode;
		return blendMode.CompareTo((int)b.BlendMode);
	}
}
