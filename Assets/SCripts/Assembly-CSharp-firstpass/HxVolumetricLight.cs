using System;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class HxVolumetricLight : MonoBehaviour
{
	private static float ShadowDistanceExtra = 0.75f;

	private Light myLight;

	private HxDummyLight myDummyLight;

	public Texture3D NoiseTexture3D;

	private CommandBuffer BufferRender;

	private CommandBuffer BufferCopy;

	private Projector myProjector;

	public Vector3 NoiseScale = new Vector3(0.5f, 0.5f, 0.5f);

	public Vector3 NoiseVelocity = new Vector3(1f, 1f, 0f);

	private bool dirty = true;

	public Texture LightFalloff;

	public float NearPlane;

	public bool NoiseEnabled;

	public bool CustomMieScatter;

	public bool CustomExtinction;

	public bool CustomExtinctionEffect;

	public bool CustomDensity;

	public bool CustomSampleCount;

	public bool CustomColor;

	public bool CustomNoiseEnabled;

	public bool CustomNoiseTexture;

	public bool CustomNoiseScale;

	public bool CustomNoiseVelocity;

	public bool CustomNoiseContrast;

	public bool CustomFogHeightEnabled;

	public bool CustomFogHeight;

	public bool CustomFogTransitionSize;

	public bool CustomAboveFogPercent;

	public bool CustomSunSize;

	public bool CustomSunBleed;

	public bool ShadowCasting = true;

	public bool CustomStrength;

	public bool CustomIntensity;

	public bool CustomTintMode;

	public bool CustomTintColor;

	public bool CustomTintColor2;

	public bool CustomTintGradient;

	public bool CustomTintIntensity;

	public bool CustomMaxLightDistance;

	[Range(0f, 4f)]
	public float NoiseContrast = 1f;

	public HxVolumetricCamera.HxTintMode TintMode;

	public Color TintColor = Color.red;

	public Color TintColor2 = Color.blue;

	public float TintIntensity = 0.2f;

	[Range(0f, 1f)]
	public float TintGradient = 0.2f;

	[Range(0f, 8f)]
	public float Intensity = 1f;

	[Range(0f, 1f)]
	public float Strength = 1f;

	public Color Color = Color.white;

	[Range(0f, 0.9999f)]
	[Tooltip("0 for even scattering, 1 for forward scattering")]
	public float MieScattering = 0.05f;

	[Range(0f, 1f)]
	[Tooltip("Create a sun using mie scattering")]
	public float SunSize;

	[Tooltip("Allows the sun to bleed over the edge of objects (recommend using bloom)")]
	public bool SunBleed = true;

	[Range(0f, 10f)]
	[Tooltip("dimms results over distance")]
	public float Extinction = 0.01f;

	[Range(0f, 1f)]
	[Tooltip("Density of air")]
	public float Density = 0.2f;

	[Range(0f, 1f)]
	[Tooltip("Useful when you want a light to have slightly more density")]
	public float ExtraDensity;

	[Range(2f, 64f)]
	[Tooltip("How many samples per pixel, Recommended 4-8 for point, 6 - 16 for Directional")]
	public int SampleCount = 4;

	[Tooltip("Ray marching Shadows can be expensive, save some frames by not marching shadows")]
	public bool Shadows = true;

	public bool FogHeightEnabled;

	public float FogHeight = 5f;

	public float FogTransitionSize = 5f;

	public float MaxLightDistance = 128f;

	public float AboveFogPercent = 0.1f;

	private bool OffsetUpdated;

	public Vector3 Offset = Vector3.zero;

	private static MaterialPropertyBlock propertyBlock;

	private bool bufferBuilt;

	public static int VolumetricBMVPPID;

	public static int VolumetricMVPPID;

	public static int VolumetricMVP2PID;

	public static int VolumetricMVPID;

	private static int LightColourPID;

	private static int LightColour2PID;

	private static int FogHeightsPID;

	private static int PhasePID;

	private static int _LightParamsPID;

	private static int DensityPID;

	private static int ShadowBiasPID;

	private static int _CustomLightPositionPID;

	private static int hxNearPlanePID;

	private static int NoiseScalePID;

	private static int NoiseOffsetPID;

	private static int _SpotLightParamsPID;

	private static int _LightTexture0PID;

	private static int _hxProjectorTexturePID;

	private static int _hxProjectorFalloffTexturePID;

	private bool LastBufferDirectional;

	private float LastSpotAngle;

	private float LastRange;

	private float LastAspect;

	private LightType lastType = LightType.Area;

	private Matrix4x4 LightMatrix;

	private Bounds lastBounds = default(Bounds);

	private Vector3 minBounds;

	private Vector3 maxBounds;

	private HxOctreeNode<HxVolumetricLight>.NodeObject octreeNode;

	private Vector4 TopFrustumNormal;

	private Vector4 BottomFrustumNormal;

	private Vector4 LeftFrustumNormal;

	private Vector4 RightFrustumNormal;

	private static Matrix4x4[] VolumeMatrixArrays = new Matrix4x4[50];

	private static Vector4[] VolumeSettingsArrays = new Vector4[50];

	private static Matrix4x4[] VolumeMatrixArraysOld = new Matrix4x4[10];

	private static Vector4[] VolumeSettingsArraysOld = new Vector4[10];

	private float LastOrthoSize;

	private bool LastOrtho;

	private bool matrixReconstruct = true;

	public Light LightSafe()
	{
		if (myLight == null)
		{
			myLight = GetComponent<Light>();
		}
		return myLight;
	}

	public HxDummyLight DummyLightSafe()
	{
		if (myDummyLight == null)
		{
			myDummyLight = GetComponent<HxDummyLight>();
		}
		return myDummyLight;
	}

	private LightType GetLightType()
	{
		if (myLight != null)
		{
			return myLight.type;
		}
		if (myDummyLight != null)
		{
			return myDummyLight.type;
		}
		return LightType.Area;
	}

	private LightShadows LightShadow()
	{
		if (myLight != null)
		{
			return myLight.shadows;
		}
		return LightShadows.None;
	}

	private bool HasLight()
	{
		if (myLight != null)
		{
			return true;
		}
		if (myDummyLight != null)
		{
			return true;
		}
		return false;
	}

	private Texture LightCookie()
	{
		if (myLight != null)
		{
			return myLight.cookie;
		}
		if (myDummyLight != null)
		{
			return myDummyLight.cookie;
		}
		return null;
	}

	private Texture LightFalloffTexture()
	{
		if (LightFalloff != null)
		{
			return LightFalloff;
		}
		return HxVolumetricCamera.Active.LightFalloff;
	}

	private float LightShadowBias()
	{
		if (myLight != null)
		{
			return myLight.shadowBias * 1.05f;
		}
		return 0.1f;
	}

	private Color LightColor()
	{
		if (myLight != null)
		{
			return (QualitySettings.activeColorSpace != 0) ? myLight.color.linear : myLight.color;
		}
		if (myDummyLight != null)
		{
			return (QualitySettings.activeColorSpace != 0) ? myDummyLight.color.linear : myDummyLight.color;
		}
		if (myProjector != null)
		{
			return (QualitySettings.activeColorSpace != 0) ? myProjector.material.GetColor("_Color").linear : myProjector.material.GetColor("_Color");
		}
		return Color.white;
	}

	private float LightSpotAngle()
	{
		if (myLight != null)
		{
			return myLight.spotAngle;
		}
		if (myDummyLight != null)
		{
			return myDummyLight.spotAngle;
		}
		if (myProjector != null)
		{
			return myProjector.fieldOfView;
		}
		return 1f;
	}

	private bool LightEnabled()
	{
		if (myLight != null)
		{
			return myLight.enabled;
		}
		if (myDummyLight != null)
		{
			return myDummyLight.enabled;
		}
		if (myProjector != null)
		{
			return myProjector.enabled;
		}
		myLight = GetComponent<Light>();
		if (myLight != null)
		{
			return myLight.enabled;
		}
		myDummyLight = GetComponent<HxDummyLight>();
		if (myDummyLight != null)
		{
			return myDummyLight.enabled;
		}
		myProjector = GetComponent<Projector>();
		if (myProjector != null)
		{
			return myProjector.enabled;
		}
		return false;
	}

	private float LightRange()
	{
		if (myLight != null)
		{
			return myLight.range;
		}
		if (myDummyLight != null)
		{
			return myDummyLight.range;
		}
		if (myProjector != null)
		{
			return myProjector.farClipPlane;
		}
		return 0f;
	}

	private float LightShadowStrength()
	{
		if (myLight != null)
		{
			return myLight.shadowStrength;
		}
		return 1f;
	}

	private float LightIntensity()
	{
		if (myLight != null)
		{
			return myLight.intensity;
		}
		if (myDummyLight != null)
		{
			return myDummyLight.intensity;
		}
		if (myProjector != null)
		{
			return 1f;
		}
		return 0f;
	}

	private void OnEnable()
	{
		myLight = GetComponent<Light>();
		myDummyLight = GetComponent<HxDummyLight>();
		myProjector = GetComponent<Projector>();
		HxVolumetricCamera.AllVolumetricLight.Add(this);
		UpdatePosition(true);
		if (GetLightType() != LightType.Directional)
		{
			octreeNode = HxVolumetricCamera.AddLightOctree(this, minBounds, maxBounds);
		}
		else
		{
			HxVolumetricCamera.ActiveDirectionalLights.Add(this);
		}
	}

	private void OnDisable()
	{
		HxVolumetricCamera.AllVolumetricLight.Remove(this);
		if (GetLightType() != LightType.Directional)
		{
			HxVolumetricCamera.RemoveLightOctree(this);
			octreeNode = null;
		}
		else
		{
			HxVolumetricCamera.ActiveDirectionalLights.Remove(this);
		}
	}

	private void OnDestroy()
	{
		HxVolumetricCamera.AllVolumetricLight.Remove(this);
		if (lastType == LightType.Directional)
		{
			HxVolumetricCamera.ActiveDirectionalLights.Remove(this);
			return;
		}
		HxVolumetricCamera.RemoveLightOctree(this);
		octreeNode = null;
	}

	private void Start()
	{
		myLight = GetComponent<Light>();
		myDummyLight = GetComponent<HxDummyLight>();
	}

	public void BuildBuffer(CommandBuffer CameraBuffer)
	{
		if (!LightEnabled() || !(LightIntensity() > 0f))
		{
			return;
		}
		switch (GetLightType())
		{
		case LightType.Directional:
			BuildDirectionalBuffer(CameraBuffer);
			LastBufferDirectional = true;
			break;
		case LightType.Spot:
			BuildSpotLightBuffer(CameraBuffer);
			LastBufferDirectional = false;
			break;
		case LightType.Point:
			BuildPointBuffer(CameraBuffer);
			LastBufferDirectional = false;
			break;
		case LightType.Area:
			if (myProjector != null)
			{
				BuildProjectorLightBuffer(CameraBuffer);
				LastBufferDirectional = false;
			}
			break;
		}
	}

	public void ReleaseBuffer()
	{
		if (myLight != null && bufferBuilt)
		{
			if (LastBufferDirectional)
			{
				myLight.RemoveCommandBuffer(LightEvent.AfterShadowMap, BufferCopy);
				myLight.RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, BufferRender);
			}
			else
			{
				myLight.RemoveCommandBuffer(LightEvent.AfterShadowMap, BufferRender);
			}
			bufferBuilt = false;
		}
	}

	public static void CreatePID()
	{
		_hxProjectorTexturePID = Shader.PropertyToID("_ShadowTex");
		_hxProjectorFalloffTexturePID = Shader.PropertyToID("_FalloffTex");
		hxNearPlanePID = Shader.PropertyToID("hxNearPlane");
		VolumetricBMVPPID = Shader.PropertyToID("VolumetricBMVP");
		VolumetricMVPPID = Shader.PropertyToID("VolumetricMVP");
		VolumetricMVP2PID = Shader.PropertyToID("VolumetricMVP2");
		LightColourPID = Shader.PropertyToID("LightColour");
		LightColour2PID = Shader.PropertyToID("LightColour2");
		VolumetricMVPID = Shader.PropertyToID("VolumetricMV");
		FogHeightsPID = Shader.PropertyToID("FogHeights");
		PhasePID = Shader.PropertyToID("Phase");
		_LightParamsPID = Shader.PropertyToID("_LightParams");
		DensityPID = Shader.PropertyToID("Density");
		ShadowBiasPID = Shader.PropertyToID("ShadowBias");
		_CustomLightPositionPID = Shader.PropertyToID("_CustomLightPosition");
		NoiseScalePID = Shader.PropertyToID("NoiseScale");
		NoiseOffsetPID = Shader.PropertyToID("NoiseOffset");
		_SpotLightParamsPID = Shader.PropertyToID("_SpotLightParams");
		_LightTexture0PID = Shader.PropertyToID("_LightTexture0");
	}

	private float LightNearPlane()
	{
		if (myLight != null)
		{
			return myLight.shadowNearPlane;
		}
		return 0.1f;
	}

	private int DirectionalPass(CommandBuffer buffer)
	{
		if (HxVolumetricCamera.Active.Ambient == HxVolumetricCamera.HxAmbientMode.UseRenderSettings)
		{
			if (RenderSettings.ambientMode == AmbientMode.Flat)
			{
				buffer.SetGlobalVector("AmbientSkyColor", RenderSettings.ambientSkyColor * RenderSettings.ambientIntensity);
				return 0;
			}
			if (RenderSettings.ambientMode == AmbientMode.Trilight)
			{
				buffer.SetGlobalVector("AmbientSkyColor", RenderSettings.ambientSkyColor * RenderSettings.ambientIntensity);
				buffer.SetGlobalVector("AmbientEquatorColor", RenderSettings.ambientEquatorColor * RenderSettings.ambientIntensity);
				buffer.SetGlobalVector("AmbientGroundColor", RenderSettings.ambientGroundColor * RenderSettings.ambientIntensity);
				return 1;
			}
			return 2;
		}
		if (HxVolumetricCamera.Active.Ambient == HxVolumetricCamera.HxAmbientMode.Color)
		{
			buffer.SetGlobalVector("AmbientSkyColor", HxVolumetricCamera.Active.AmbientSky * HxVolumetricCamera.Active.AmbientIntensity);
			return 0;
		}
		if (HxVolumetricCamera.Active.Ambient == HxVolumetricCamera.HxAmbientMode.Gradient)
		{
			buffer.SetGlobalVector("AmbientSkyColor", HxVolumetricCamera.Active.AmbientSky * HxVolumetricCamera.Active.AmbientIntensity);
			buffer.SetGlobalVector("AmbientEquatorColor", HxVolumetricCamera.Active.AmbientEquator * HxVolumetricCamera.Active.AmbientIntensity);
			buffer.SetGlobalVector("AmbientGroundColor", HxVolumetricCamera.Active.AmbientGround * HxVolumetricCamera.Active.AmbientIntensity);
			return 1;
		}
		return 2;
	}

	private float getContrast()
	{
		if (CustomNoiseContrast)
		{
			return NoiseContrast;
		}
		return HxVolumetricCamera.Active.NoiseContrast;
	}

	private bool ShaderModel4()
	{
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2)
		{
			return false;
		}
		return false;
	}

	private void BuildDirectionalBuffer(CommandBuffer CameraBuffer)
	{
		bool flag = LightShadow() != 0 && Shadows && QualitySettings.shadows != ShadowQuality.Disable;
		if (!dirty)
		{
			return;
		}
		if (flag)
		{
			if (BufferCopy == null)
			{
				BufferCopy = new CommandBuffer();
				BufferCopy.name = "ShadowCopy";
				BufferCopy.SetGlobalTexture(HxVolumetricCamera.ShadowMapTexturePID, BuiltinRenderTextureType.CurrentActive);
			}
			if (BufferRender == null)
			{
				BufferRender = new CommandBuffer();
				BufferRender.name = "VolumetricRender";
			}
			bufferBuilt = true;
			CameraBuffer = BufferRender;
			BufferRender.Clear();
		}
		if (flag && HxVolumetricCamera.Active.ShadowFix)
		{
			Graphics.DrawMesh(HxVolumetricCamera.BoxMesh, HxVolumetricCamera.Active.transform.position, HxVolumetricCamera.Active.transform.rotation, HxVolumetricCamera.ShadowMaterial, 0, HxVolumetricCamera.ActiveCamera, 0, null, true);
		}
		Vector3 forward = base.transform.forward;
		if ((!CustomFogHeightEnabled) ? HxVolumetricCamera.Active.FogHeightEnabled : FogHeightEnabled)
		{
			CameraBuffer.SetGlobalVector(FogHeightsPID, new Vector3(((!CustomFogHeight) ? HxVolumetricCamera.Active.FogHeight : FogHeight) - ((!CustomFogTransitionSize) ? HxVolumetricCamera.Active.FogTransitionSize : FogTransitionSize), (!CustomFogHeight) ? HxVolumetricCamera.Active.FogHeight : FogHeight, (!CustomAboveFogPercent) ? HxVolumetricCamera.Active.AboveFogPercent : AboveFogPercent));
		}
		float fogDensity = GetFogDensity();
		CameraBuffer.SetGlobalVector("MaxRayDistance", new Vector2(Mathf.Min(QualitySettings.shadowDistance, (!CustomMaxLightDistance) ? HxVolumetricCamera.Active.MaxDirectionalRayDistance : MaxLightDistance), (!CustomMaxLightDistance) ? HxVolumetricCamera.Active.MaxDirectionalRayDistance : MaxLightDistance));
		float num = ((!CustomMieScatter) ? HxVolumetricCamera.Active.MieScattering : MieScattering);
		Vector4 value = new Vector4(1f / (4f * (float)Math.PI), 1f - num * num, 1f + num * num, 2f * num);
		float num2 = ((!CustomSunSize) ? HxVolumetricCamera.Active.SunSize : SunSize);
		CameraBuffer.SetGlobalVector("SunSize", new Vector2((num2 != 0f) ? 1 : 0, ((!CustomSunBleed) ? HxVolumetricCamera.Active.SunBleed : SunBleed) ? 1 : 0));
		num2 = Mathf.Lerp(0.9999f, 0.995f, Mathf.Pow(num2, 4f));
		LoadVolumeData();
		LoadVolumeDateIntoBuffer(CameraBuffer);
		Vector4 value2 = new Vector4(1f / (4f * (float)Math.PI), 1f - num2 * num2, 1f + num2 * num2, 2f * num2);
		CameraBuffer.SetGlobalVector("Phase2", value2);
		CameraBuffer.SetGlobalVector(PhasePID, value);
		SetColors(CameraBuffer);
		CameraBuffer.SetGlobalVector(_LightParamsPID, new Vector4((!CustomStrength) ? LightShadowStrength() : Strength, 0f, 0f, (!CustomIntensity) ? LightIntensity() : Intensity));
		CameraBuffer.SetGlobalVector(DensityPID, new Vector4(fogDensity, GetSampleCount(flag), 0f, (!CustomExtinction) ? HxVolumetricCamera.Active.Extinction : Extinction));
		CameraBuffer.SetGlobalVector(ShadowBiasPID, new Vector3(LightShadowBias(), LightNearPlane(), (1f - ((!CustomStrength) ? LightShadowStrength() : Strength)) * value.x * (value.y / Mathf.Pow(value.z - value.w * -1f, 1.5f))));
		CameraBuffer.SetGlobalVector(_SpotLightParamsPID, new Vector4(forward.x, forward.y, forward.z, 0f));
		Vector3 vector = ((!CustomNoiseScale) ? HxVolumetricCamera.Active.NoiseScale : NoiseScale);
		vector = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z) / 32f;
		CameraBuffer.SetGlobalVector(NoiseScalePID, vector);
		if (!OffsetUpdated)
		{
			OffsetUpdated = true;
			Offset += NoiseVelocity * Time.deltaTime;
		}
		CameraBuffer.SetGlobalVector(NoiseOffsetPID, (!CustomNoiseVelocity) ? HxVolumetricCamera.Active.Offset : Offset);
		CameraBuffer.SetGlobalFloat("FirstLight", HxVolumetricCamera.FirstDirectional ? 1 : 0);
		CameraBuffer.SetGlobalFloat("AmbientStrength", HxVolumetricCamera.Active.AmbientLightingStrength);
		HxVolumetricCamera.FirstDirectional = false;
		if (flag)
		{
			if (HxVolumetricCamera.Active.TransparencySupport)
			{
				CameraBuffer.SetRenderTarget(HxVolumetricCamera.VolumetricTransparency[(int)HxVolumetricCamera.Active.compatibleTBuffer()], HxVolumetricCamera.ScaledDepthTextureRTID[(int)HxVolumetricCamera.Active.resolution]);
			}
			else
			{
				CameraBuffer.SetRenderTarget(HxVolumetricCamera.VolumetricTextureRTID, HxVolumetricCamera.ScaledDepthTextureRTID[(int)HxVolumetricCamera.Active.resolution]);
			}
		}
		CameraBuffer.SetGlobalMatrix(VolumetricMVPPID, HxVolumetricCamera.BlitMatrixMVP);
		CameraBuffer.SetGlobalFloat("ExtinctionEffect", HxVolumetricCamera.Active.ExtinctionEffect);
		int mid = MID(flag, HxVolumetricCamera.ActiveFull());
		if ((!CustomNoiseEnabled) ? HxVolumetricCamera.Active.NoiseEnabled : NoiseEnabled)
		{
			if (propertyBlock == null)
			{
				propertyBlock = new MaterialPropertyBlock();
			}
			Texture3D noiseTexture = GetNoiseTexture();
			if (noiseTexture != null)
			{
				propertyBlock.SetFloat("hxNoiseContrast", getContrast());
				propertyBlock.SetTexture("NoiseTexture3D", noiseTexture);
			}
		}
		CameraBuffer.DrawMesh(HxVolumetricCamera.QuadMesh, HxVolumetricCamera.BlitMatrix, HxVolumetricCamera.GetDirectionalMaterial(mid), 0, DirectionalPass(CameraBuffer), propertyBlock);
		if (flag)
		{
			myLight.AddCommandBuffer(LightEvent.AfterShadowMap, BufferCopy);
			myLight.AddCommandBuffer(LightEvent.AfterScreenspaceMask, BufferRender);
		}
	}

	private void LoadVolumeDateIntoBuffer(CommandBuffer buffer)
	{
		if (ShaderModel4())
		{
			buffer.SetGlobalMatrixArray("hxVolumeMatrix", VolumeMatrixArrays);
			buffer.SetGlobalVectorArray("hxVolumeSettings", VolumeSettingsArrays);
		}
		else
		{
			buffer.SetGlobalMatrixArray("hxVolumeMatrixOld", VolumeMatrixArraysOld);
			buffer.SetGlobalVectorArray("hxVolumeSettingsOld", VolumeSettingsArraysOld);
		}
	}

	private float CalcLightInstensityDistance(float distance)
	{
		return Mathf.InverseLerp((!CustomMaxLightDistance) ? HxVolumetricCamera.Active.MaxLightDistance : MaxLightDistance, ((!CustomMaxLightDistance) ? HxVolumetricCamera.Active.MaxLightDistance : MaxLightDistance) * 0.8f, distance);
	}

	private void BuildSpotLightBuffer(CommandBuffer cameraBuffer)
	{
		float num = ClosestDistanceToCone(HxVolumetricCamera.Active.transform.position);
		float num2 = CalcLightInstensityDistance(num);
		if (!(num2 > 0f))
		{
			return;
		}
		bool flag = LightShadow() != 0 && Shadows && QualitySettings.shadows != ShadowQuality.Disable;
		if (flag && num > QualitySettings.shadowDistance - ShadowDistanceExtra)
		{
			flag = false;
		}
		if (!dirty)
		{
			return;
		}
		if (flag)
		{
			if (BufferRender == null)
			{
				BufferRender = new CommandBuffer();
				BufferRender.name = "VolumetricRender";
			}
			bufferBuilt = true;
			cameraBuffer = BufferRender;
			BufferRender.Clear();
		}
		cameraBuffer.SetGlobalTexture(HxVolumetricCamera.ShadowMapTexturePID, BuiltinRenderTextureType.CurrentActive);
		SetColors(cameraBuffer, num2);
		if (flag)
		{
			if (HxVolumetricCamera.Active.TransparencySupport)
			{
				cameraBuffer.SetRenderTarget(HxVolumetricCamera.VolumetricTransparency[(int)HxVolumetricCamera.Active.compatibleTBuffer()], HxVolumetricCamera.ScaledDepthTextureRTID[(int)HxVolumetricCamera.Active.resolution]);
			}
			else
			{
				cameraBuffer.SetRenderTarget(HxVolumetricCamera.VolumetricTextureRTID, HxVolumetricCamera.ScaledDepthTextureRTID[(int)HxVolumetricCamera.Active.resolution]);
			}
		}
		if ((!CustomFogHeightEnabled) ? HxVolumetricCamera.Active.FogHeightEnabled : FogHeightEnabled)
		{
			cameraBuffer.SetGlobalVector(FogHeightsPID, new Vector3(((!CustomFogHeight) ? HxVolumetricCamera.Active.FogHeight : FogHeight) - ((!CustomFogTransitionSize) ? HxVolumetricCamera.Active.FogTransitionSize : FogTransitionSize), (!CustomFogHeight) ? HxVolumetricCamera.Active.FogHeight : FogHeight, (!CustomAboveFogPercent) ? HxVolumetricCamera.Active.AboveFogPercent : AboveFogPercent));
		}
		LoadVolumeDataBounds();
		LoadVolumeDateIntoBuffer(cameraBuffer);
		float fogDensity = GetFogDensity();
		cameraBuffer.SetGlobalMatrix(VolumetricMVPPID, HxVolumetricCamera.Active.MatrixVP * LightMatrix);
		cameraBuffer.SetGlobalMatrix(VolumetricMVPID, HxVolumetricCamera.Active.MatrixV * LightMatrix);
		float num3 = ((!CustomMieScatter) ? HxVolumetricCamera.Active.MieScattering : MieScattering);
		Vector4 value = new Vector4(1f / (4f * (float)Math.PI), 1f - num3 * num3, 1f + num3 * num3, 2f * num3);
		cameraBuffer.SetGlobalVector(PhasePID, value);
		cameraBuffer.SetGlobalVector(_CustomLightPositionPID, base.transform.position);
		cameraBuffer.SetGlobalVector(_LightParamsPID, new Vector4((!CustomStrength) ? LightShadowStrength() : Strength, 1f / LightRange(), LightRange(), (!CustomIntensity) ? LightIntensity() : Intensity));
		cameraBuffer.SetGlobalVector(DensityPID, new Vector4(fogDensity, GetSampleCount(flag), 0f, (!CustomExtinction) ? HxVolumetricCamera.Active.Extinction : Extinction));
		if (flag)
		{
			Graphics.DrawMesh(HxVolumetricCamera.SpotLightMesh, LightMatrix, HxVolumetricCamera.ShadowMaterial, 0, HxVolumetricCamera.ActiveCamera, 0, null, ShadowCastingMode.ShadowsOnly);
		}
		float x = (1f - LightRange() / LightNearPlane()) / LightRange();
		float y = LightRange() / LightNearPlane() / LightRange();
		cameraBuffer.SetGlobalVector(ShadowBiasPID, new Vector4(x, y, (1f - ((!CustomStrength) ? LightShadowStrength() : Strength)) * value.x * (value.y / Mathf.Pow(value.z - value.w * -1f, 1.5f)), LightShadowBias()));
		Vector3 vector = ((!CustomNoiseScale) ? HxVolumetricCamera.Active.NoiseScale : NoiseScale);
		vector = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z) / 32f;
		cameraBuffer.SetGlobalFloat(hxNearPlanePID, NearPlane);
		cameraBuffer.SetGlobalVector(NoiseScalePID, vector);
		if (!OffsetUpdated)
		{
			OffsetUpdated = true;
			Offset += NoiseVelocity * Time.deltaTime;
		}
		cameraBuffer.SetGlobalVector(NoiseOffsetPID, (!CustomNoiseVelocity) ? HxVolumetricCamera.Active.Offset : Offset);
		Vector3 forward = base.transform.forward;
		cameraBuffer.SetGlobalVector(_SpotLightParamsPID, new Vector4(forward.x, forward.y, forward.z, (LightSpotAngle() + 0.01f) / 2f * ((float)Math.PI / 180f)));
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
		propertyBlock.SetTexture(_LightTexture0PID, (!(LightCookie() == null)) ? LightCookie() : HxVolumetricCamera.Active.SpotLightCookie);
		propertyBlock.SetTexture(_hxProjectorFalloffTexturePID, LightFalloffTexture());
		cameraBuffer.SetGlobalVector("TopFrustumNormal", TopFrustumNormal);
		cameraBuffer.SetGlobalVector("BottomFrustumNormal", BottomFrustumNormal);
		cameraBuffer.SetGlobalVector("LeftFrustumNormal", LeftFrustumNormal);
		cameraBuffer.SetGlobalVector("RightFrustumNormal", RightFrustumNormal);
		int mid = MID(flag, HxVolumetricCamera.ActiveFull());
		if ((!CustomNoiseEnabled) ? HxVolumetricCamera.Active.NoiseEnabled : NoiseEnabled)
		{
			if (propertyBlock == null)
			{
				propertyBlock = new MaterialPropertyBlock();
			}
			Texture3D noiseTexture = GetNoiseTexture();
			if (noiseTexture != null)
			{
				propertyBlock.SetFloat("hxNoiseContrast", getContrast());
				propertyBlock.SetTexture("NoiseTexture3D", noiseTexture);
			}
		}
		if (lastBounds.SqrDistance(HxVolumetricCamera.Active.transform.position) < HxVolumetricCamera.ActiveCamera.nearClipPlane * 2f * (HxVolumetricCamera.ActiveCamera.nearClipPlane * 2f))
		{
			cameraBuffer.DrawMesh(HxVolumetricCamera.QuadMesh, LightMatrix, HxVolumetricCamera.GetSpotMaterial(mid), 0, 0, propertyBlock);
		}
		else
		{
			cameraBuffer.DrawMesh(HxVolumetricCamera.SpotLightMesh, LightMatrix, HxVolumetricCamera.GetSpotMaterial(mid), 0, 1, propertyBlock);
		}
		if (flag)
		{
			myLight.AddCommandBuffer(LightEvent.AfterShadowMap, BufferRender);
		}
	}

	private void BuildProjectorLightBuffer(CommandBuffer cameraBuffer)
	{
		float num = Mathf.Sqrt(lastBounds.SqrDistance(HxVolumetricCamera.ActiveCamera.transform.position));
		float num2 = CalcLightInstensityDistance(num);
		if (!(num2 > 0f) || !dirty)
		{
			return;
		}
		SetColors(cameraBuffer, num2);
		cameraBuffer.SetGlobalTexture(HxVolumetricCamera.ShadowMapTexturePID, BuiltinRenderTextureType.CurrentActive);
		if ((!CustomFogHeightEnabled) ? HxVolumetricCamera.Active.FogHeightEnabled : FogHeightEnabled)
		{
			cameraBuffer.SetGlobalVector(FogHeightsPID, new Vector3(((!CustomFogHeight) ? HxVolumetricCamera.Active.FogHeight : FogHeight) - ((!CustomFogTransitionSize) ? HxVolumetricCamera.Active.FogTransitionSize : FogTransitionSize), (!CustomFogHeight) ? HxVolumetricCamera.Active.FogHeight : FogHeight, (!CustomAboveFogPercent) ? HxVolumetricCamera.Active.AboveFogPercent : AboveFogPercent));
		}
		LoadVolumeDataBounds();
		LoadVolumeDateIntoBuffer(cameraBuffer);
		float fogDensity = GetFogDensity();
		cameraBuffer.SetGlobalMatrix(VolumetricMVPPID, HxVolumetricCamera.Active.MatrixVP * LightMatrix);
		cameraBuffer.SetGlobalMatrix(VolumetricMVPID, HxVolumetricCamera.Active.MatrixV * LightMatrix);
		float num3 = ((!CustomMieScatter) ? HxVolumetricCamera.Active.MieScattering : MieScattering);
		cameraBuffer.SetGlobalVector(value: new Vector4(1f / (4f * (float)Math.PI), 1f - num3 * num3, 1f + num3 * num3, 2f * num3), nameID: PhasePID);
		cameraBuffer.SetGlobalVector(_CustomLightPositionPID, base.transform.position);
		cameraBuffer.SetGlobalVector(_LightParamsPID, new Vector4((!CustomStrength) ? LightShadowStrength() : Strength, 1f / LightRange(), LightRange(), (!CustomIntensity) ? LightIntensity() : Intensity));
		cameraBuffer.SetGlobalVector(DensityPID, new Vector4(fogDensity, GetSampleCount(false), 0f, (!CustomExtinction) ? HxVolumetricCamera.Active.Extinction : Extinction));
		Vector3 vector = ((!CustomNoiseScale) ? HxVolumetricCamera.Active.NoiseScale : NoiseScale);
		vector = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z) / 32f;
		cameraBuffer.SetGlobalFloat(hxNearPlanePID, NearPlane);
		cameraBuffer.SetGlobalVector(NoiseScalePID, vector);
		if (!OffsetUpdated)
		{
			OffsetUpdated = true;
			Offset += NoiseVelocity * Time.deltaTime;
		}
		cameraBuffer.SetGlobalVector(NoiseOffsetPID, (!CustomNoiseVelocity) ? HxVolumetricCamera.Active.Offset : Offset);
		Vector3 forward = base.transform.forward;
		cameraBuffer.SetGlobalVector(_SpotLightParamsPID, new Vector4(forward.x, forward.y, forward.z, (LightSpotAngle() + 0.01f) / 2f * ((float)Math.PI / 180f)));
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
		propertyBlock.SetTexture(_hxProjectorTexturePID, myProjector.material.GetTexture(_hxProjectorTexturePID));
		propertyBlock.SetTexture(_hxProjectorFalloffTexturePID, (!(LightFalloff != null)) ? myProjector.material.GetTexture(_hxProjectorFalloffTexturePID) : LightFalloff);
		cameraBuffer.SetGlobalVector("TopFrustumNormal", TopFrustumNormal);
		cameraBuffer.SetGlobalVector("BottomFrustumNormal", BottomFrustumNormal);
		cameraBuffer.SetGlobalVector("LeftFrustumNormal", LeftFrustumNormal);
		cameraBuffer.SetGlobalVector("RightFrustumNormal", RightFrustumNormal);
		cameraBuffer.SetGlobalFloat("OrthoLight", myProjector.orthographic ? 1 : 0);
		cameraBuffer.SetGlobalVector("UpFrustumOffset", base.transform.up * ((!myProjector.orthographic) ? 0f : myProjector.orthographicSize));
		cameraBuffer.SetGlobalVector("RightFrustumOffset", base.transform.right * ((!myProjector.orthographic) ? 0f : (myProjector.orthographicSize * myProjector.aspectRatio)));
		int mid = MID(false, HxVolumetricCamera.ActiveFull());
		if ((!CustomNoiseEnabled) ? HxVolumetricCamera.Active.NoiseEnabled : NoiseEnabled)
		{
			if (propertyBlock == null)
			{
				propertyBlock = new MaterialPropertyBlock();
			}
			Texture3D noiseTexture = GetNoiseTexture();
			if (noiseTexture != null)
			{
				propertyBlock.SetFloat("hxNoiseContrast", getContrast());
				propertyBlock.SetTexture("NoiseTexture3D", noiseTexture);
			}
		}
		if (myProjector.orthographic)
		{
			cameraBuffer.DrawMesh(HxVolumetricCamera.OrthoProjectorMesh, LightMatrix, HxVolumetricCamera.GetProjectorMaterial(mid), 0, (!(num < HxVolumetricCamera.ActiveCamera.nearClipPlane)) ? 1 : 0, propertyBlock);
		}
		else
		{
			cameraBuffer.DrawMesh(HxVolumetricCamera.SpotLightMesh, LightMatrix, HxVolumetricCamera.GetProjectorMaterial(mid), 0, (!(num < HxVolumetricCamera.ActiveCamera.nearClipPlane)) ? 1 : 0, propertyBlock);
		}
	}

	private void SetColors(CommandBuffer buffer, float distanceLerp)
	{
		Vector4 vector = ((!CustomColor) ? LightColor() : Color) * ((!CustomIntensity) ? LightIntensity() : Intensity) * distanceLerp;
		if ((!CustomTintMode) ? (HxVolumetricCamera.Active.TintMode == HxVolumetricCamera.HxTintMode.Off) : (TintMode == HxVolumetricCamera.HxTintMode.Off))
		{
			buffer.SetGlobalVector(LightColourPID, vector);
			buffer.SetGlobalVector(LightColour2PID, vector);
		}
		else if ((!CustomTintMode) ? (HxVolumetricCamera.Active.TintMode == HxVolumetricCamera.HxTintMode.Color) : (TintMode == HxVolumetricCamera.HxTintMode.Color))
		{
			buffer.SetGlobalVector(LightColourPID, CalcTintColor(vector));
			buffer.SetGlobalVector(LightColour2PID, CalcTintColor(vector));
		}
		else if ((!CustomTintMode) ? (HxVolumetricCamera.Active.TintMode == HxVolumetricCamera.HxTintMode.Edge) : (TintMode == HxVolumetricCamera.HxTintMode.Edge))
		{
			buffer.SetGlobalVector(LightColourPID, vector);
			buffer.SetGlobalVector(LightColour2PID, CalcTintColor(vector));
			buffer.SetGlobalFloat("TintPercent", 1f / ((!CustomTintGradient) ? HxVolumetricCamera.Active.TintGradient : TintGradient) / 2f);
		}
		else if ((!CustomTintMode) ? (HxVolumetricCamera.Active.TintMode == HxVolumetricCamera.HxTintMode.Gradient) : (TintMode == HxVolumetricCamera.HxTintMode.Gradient))
		{
			buffer.SetGlobalVector(LightColourPID, CalcTintColor(vector));
			buffer.SetGlobalVector(LightColour2PID, CalcTintColorEdge(vector));
			buffer.SetGlobalFloat("TintPercent", 1f / ((!CustomTintGradient) ? HxVolumetricCamera.Active.TintGradient : TintGradient) / 2f);
		}
	}

	private void SetColors(CommandBuffer buffer)
	{
		Vector4 vector = ((!CustomColor) ? LightColor() : Color) * ((!CustomIntensity) ? LightIntensity() : Intensity);
		if (HxVolumetricCamera.Active.TintMode == HxVolumetricCamera.HxTintMode.Off)
		{
			buffer.SetGlobalVector(LightColourPID, vector);
			buffer.SetGlobalVector(LightColour2PID, vector);
		}
		else if (HxVolumetricCamera.Active.TintMode == HxVolumetricCamera.HxTintMode.Color)
		{
			buffer.SetGlobalVector(LightColourPID, CalcTintColor(vector));
			buffer.SetGlobalVector(LightColour2PID, CalcTintColor(vector));
		}
		else if (HxVolumetricCamera.Active.TintMode == HxVolumetricCamera.HxTintMode.Edge)
		{
			buffer.SetGlobalVector(LightColourPID, vector);
			buffer.SetGlobalVector(LightColour2PID, CalcTintColor(vector));
			buffer.SetGlobalFloat("TintPercent", 1f / HxVolumetricCamera.Active.TintGradient / 2f);
		}
		else if (HxVolumetricCamera.Active.TintMode == HxVolumetricCamera.HxTintMode.Gradient)
		{
			buffer.SetGlobalVector(LightColourPID, CalcTintColor(vector));
			buffer.SetGlobalVector(LightColour2PID, CalcTintColorEdge(vector));
			buffer.SetGlobalFloat("TintPercent", 1f / HxVolumetricCamera.Active.TintGradient / 2f);
		}
	}

	private Vector3 CalcTintColor(Vector4 c)
	{
		Vector3 vector = new Vector3(c.x, c.y, c.z);
		float magnitude = vector.magnitude;
		if (CustomTintColor)
		{
			vector += new Vector3(((QualitySettings.activeColorSpace != 0) ? TintColor.linear : TintColor).r, ((QualitySettings.activeColorSpace != 0) ? TintColor.linear : TintColor).g, ((QualitySettings.activeColorSpace != 0) ? TintColor.linear : TintColor).b) * ((!CustomTintIntensity) ? HxVolumetricCamera.Active.TintIntensity : TintIntensity);
		}
		else
		{
			vector += HxVolumetricCamera.Active.CurrentTint;
		}
		return vector.normalized * magnitude;
	}

	private Vector3 CalcTintColorEdge(Vector4 c)
	{
		Vector3 vector = new Vector3(c.x, c.y, c.z);
		float magnitude = vector.magnitude;
		if (CustomTintColor2)
		{
			vector += new Vector3(((QualitySettings.activeColorSpace != 0) ? TintColor2.linear : TintColor2).r, ((QualitySettings.activeColorSpace != 0) ? TintColor2.linear : TintColor2).g, ((QualitySettings.activeColorSpace != 0) ? TintColor2.linear : TintColor2).b) * ((!CustomTintIntensity) ? HxVolumetricCamera.Active.TintIntensity : TintIntensity);
		}
		else
		{
			vector += HxVolumetricCamera.Active.CurrentTintEdge;
		}
		return vector.normalized * magnitude;
	}

	private void BuildPointBuffer(CommandBuffer cameraBuffer)
	{
		float num = Mathf.Max(Vector3.Distance(HxVolumetricCamera.Active.transform.position, base.transform.position) - LightRange(), 0f);
		float num2 = CalcLightInstensityDistance(num);
		if (!(num2 > 0f))
		{
			return;
		}
		bool flag = LightShadow() != 0 && Shadows && QualitySettings.shadows != ShadowQuality.Disable;
		if (flag && num >= QualitySettings.shadowDistance - ShadowDistanceExtra)
		{
			flag = false;
		}
		if (!dirty)
		{
			return;
		}
		if (flag)
		{
			if (BufferRender == null)
			{
				BufferRender = new CommandBuffer();
				BufferRender.name = "VolumetricRender";
			}
			bufferBuilt = true;
			cameraBuffer = BufferRender;
			BufferRender.Clear();
		}
		cameraBuffer.SetGlobalTexture(HxVolumetricCamera.ShadowMapTexturePID, BuiltinRenderTextureType.CurrentActive);
		SetColors(cameraBuffer, num2);
		if ((!CustomFogHeightEnabled) ? HxVolumetricCamera.Active.FogHeightEnabled : FogHeightEnabled)
		{
			cameraBuffer.SetGlobalVector(FogHeightsPID, new Vector3(((!CustomFogHeight) ? HxVolumetricCamera.Active.FogHeight : FogHeight) - ((!CustomFogTransitionSize) ? HxVolumetricCamera.Active.FogTransitionSize : FogTransitionSize), (!CustomFogHeight) ? HxVolumetricCamera.Active.FogHeight : FogHeight, (!CustomAboveFogPercent) ? HxVolumetricCamera.Active.AboveFogPercent : AboveFogPercent));
		}
		if (flag)
		{
			if (HxVolumetricCamera.Active.TransparencySupport)
			{
				cameraBuffer.SetRenderTarget(HxVolumetricCamera.VolumetricTransparency[(int)HxVolumetricCamera.Active.compatibleTBuffer()], HxVolumetricCamera.ScaledDepthTextureRTID[(int)HxVolumetricCamera.Active.resolution]);
			}
			else
			{
				cameraBuffer.SetRenderTarget(HxVolumetricCamera.VolumetricTextureRTID, HxVolumetricCamera.ScaledDepthTextureRTID[(int)HxVolumetricCamera.Active.resolution]);
			}
		}
		LoadVolumeDataBounds();
		LoadVolumeDateIntoBuffer(cameraBuffer);
		float fogDensity = GetFogDensity();
		cameraBuffer.SetGlobalMatrix(VolumetricMVPPID, HxVolumetricCamera.Active.MatrixVP * LightMatrix);
		cameraBuffer.SetGlobalMatrix(VolumetricMVPID, HxVolumetricCamera.Active.MatrixV * LightMatrix);
		float num3 = ((!CustomMieScatter) ? HxVolumetricCamera.Active.MieScattering : MieScattering);
		Vector4 value = new Vector4(1f / (4f * (float)Math.PI), 1f - num3 * num3, 1f + num3 * num3, 2f * num3);
		cameraBuffer.SetGlobalVector(PhasePID, value);
		cameraBuffer.SetGlobalVector(_CustomLightPositionPID, base.transform.position);
		cameraBuffer.SetGlobalVector(_LightParamsPID, new Vector4((!CustomStrength) ? LightShadowStrength() : Strength, 1f / LightRange(), LightRange(), (!CustomIntensity) ? LightIntensity() : Intensity));
		cameraBuffer.SetGlobalVector(DensityPID, new Vector4(fogDensity, GetSampleCount(flag), 0f, (!CustomExtinction) ? HxVolumetricCamera.Active.Extinction : Extinction));
		cameraBuffer.SetGlobalVector(ShadowBiasPID, new Vector3(LightShadowBias(), LightNearPlane(), (1f - ((!CustomStrength) ? LightShadowStrength() : Strength)) * value.x * (value.y / Mathf.Pow(value.z - value.w * -1f, 1.5f))));
		Vector3 vector = ((!CustomNoiseScale) ? HxVolumetricCamera.Active.NoiseScale : NoiseScale);
		vector = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z) / 32f;
		cameraBuffer.SetGlobalVector(NoiseScalePID, vector);
		if (!OffsetUpdated)
		{
			OffsetUpdated = true;
			Offset += NoiseVelocity * Time.deltaTime;
		}
		cameraBuffer.SetGlobalVector(NoiseOffsetPID, (!CustomNoiseVelocity) ? HxVolumetricCamera.Active.Offset : Offset);
		if (flag && HxVolumetricCamera.Active.ShadowFix)
		{
			Graphics.DrawMesh(HxVolumetricCamera.BoxMesh, LightMatrix, HxVolumetricCamera.ShadowMaterial, 0, HxVolumetricCamera.ActiveCamera, 0, null, ShadowCastingMode.ShadowsOnly);
		}
		int num4 = ((!(num <= (LightRange() + LightRange() * 0.09f) / 2f + HxVolumetricCamera.ActiveCamera.nearClipPlane * 2f)) ? 1 : 0);
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
		int mid = MID(flag, HxVolumetricCamera.ActiveFull());
		if ((!CustomNoiseEnabled) ? HxVolumetricCamera.Active.NoiseEnabled : NoiseEnabled)
		{
			Texture3D noiseTexture = GetNoiseTexture();
			if (noiseTexture != null)
			{
				propertyBlock.SetFloat("hxNoiseContrast", getContrast());
				propertyBlock.SetTexture("NoiseTexture3D", noiseTexture);
			}
		}
		propertyBlock.SetTexture(_hxProjectorFalloffTexturePID, LightFalloffTexture());
		if (LightCookie() != null)
		{
			propertyBlock.SetTexture(Shader.PropertyToID("PointCookieTexture"), LightCookie());
			cameraBuffer.DrawMesh((num4 != 0) ? HxVolumetricCamera.SphereMesh : HxVolumetricCamera.QuadMesh, LightMatrix, HxVolumetricCamera.GetPointMaterial(mid), 0, num4, propertyBlock);
		}
		else
		{
			cameraBuffer.DrawMesh((num4 != 0) ? HxVolumetricCamera.SphereMesh : HxVolumetricCamera.QuadMesh, LightMatrix, HxVolumetricCamera.GetPointMaterial(mid), 0, num4, propertyBlock);
		}
		if (flag)
		{
			myLight.AddCommandBuffer(LightEvent.AfterShadowMap, BufferRender);
		}
	}

	public int MID(bool RenderShadows, bool full)
	{
		int num = 0;
		if (RenderShadows)
		{
			num++;
		}
		if (LightCookie() != null)
		{
			num += 2;
		}
		if ((!CustomNoiseEnabled) ? HxVolumetricCamera.Active.NoiseEnabled : NoiseEnabled)
		{
			num += 4;
		}
		if ((!CustomFogHeight) ? HxVolumetricCamera.Active.FogHeightEnabled : FogHeightEnabled)
		{
			num += 8;
		}
		if (HxVolumetricCamera.Active.renderDensityParticleCheck())
		{
			num += 16;
		}
		if (HxVolumetricCamera.Active.TransparencySupport)
		{
			num += 32;
		}
		if (full)
		{
			num += 64;
		}
		return num;
	}

	private void Update()
	{
		OffsetUpdated = false;
	}

	private float GetFogDensity()
	{
		if (CustomDensity)
		{
			return Density + ExtraDensity;
		}
		return HxVolumetricCamera.Active.Density + ExtraDensity;
	}

	private Texture3D GetNoiseTexture()
	{
		if (CustomNoiseTexture)
		{
			if (NoiseTexture3D == null)
			{
				return HxVolumetricCamera.Active.NoiseTexture3D;
			}
			return NoiseTexture3D;
		}
		return HxVolumetricCamera.Active.NoiseTexture3D;
	}

	private int GetSampleCount(bool RenderShadows)
	{
		int b = (CustomSampleCount ? SampleCount : ((GetLightType() == LightType.Directional) ? HxVolumetricCamera.Active.DirectionalSampleCount : HxVolumetricCamera.Active.SampleCount));
		return Mathf.Max(2, b);
	}

	public static Vector3 ClosestPointOnLine(Vector3 vA, Vector3 vB, Vector3 vPoint)
	{
		Vector3 rhs = vPoint - vA;
		Vector3 normalized = (vB - vA).normalized;
		float num = Vector3.Distance(vA, vB);
		float num2 = Vector3.Dot(normalized, rhs);
		if (num2 <= 0f)
		{
			return vA;
		}
		if (num2 >= num)
		{
			return vB;
		}
		Vector3 vector = normalized * num2;
		return vA + vector;
	}

	private float ClosestDistanceToCone(Vector3 Point)
	{
		Vector3 vector = base.transform.forward * LightRange();
		Vector3 vector2 = base.transform.position + vector;
		float num = Vector3.Dot(base.transform.forward, Point - vector2);
		if (num == 0f)
		{
			return 0f;
		}
		Vector3 vector3 = Point - num * base.transform.forward;
		float num2 = Mathf.Tan(LightSpotAngle() / 2f * ((float)Math.PI / 180f)) * LightRange();
		Vector3 vector4 = vector3 - vector2;
		if (num > 0f)
		{
			vector3 = vector2 + vector4.normalized * Mathf.Min(vector4.magnitude, num2);
			return Vector3.Distance(Point, vector3);
		}
		float num3 = (float)Math.PI / 180f * LightSpotAngle();
		float num4 = Mathf.Acos(Vector3.Dot(Point - base.transform.position, -vector) / ((Point - base.transform.position).magnitude * LightRange()));
		if (Mathf.Abs(num4 - num3) >= (float)Math.PI / 2f)
		{
			return 0f;
		}
		vector3 = vector2 + vector4.normalized * num2;
		vector3 = ClosestPointOnLine(vector3, base.transform.position, Point);
		return Vector3.Distance(Point, vector3);
	}

	private void UpdateLightMatrix()
	{
		LastRange = LightRange();
		LastSpotAngle = LightSpotAngle();
		lastType = GetLightType();
		if (GetLightType() == LightType.Point)
		{
			LightMatrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, new Vector3(LightRange() * 2f, LightRange() * 2f, LightRange() * 2f));
			matrixReconstruct = false;
		}
		else if (GetLightType() == LightType.Spot)
		{
			float num = Mathf.Tan(LightSpotAngle() / 2f * ((float)Math.PI / 180f)) * LightRange();
			LightMatrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, new Vector3(num * 2f, num * 2f, LightRange()));
		}
		else if (GetLightType() == LightType.Area && myProjector != null)
		{
			if (myProjector.orthographic)
			{
				LightMatrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, new Vector3(myProjector.orthographicSize * myProjector.aspectRatio * 2f, myProjector.orthographicSize * 2f, LightRange()));
			}
			else
			{
				float num2 = Mathf.Tan(LightSpotAngle() / 2f * ((float)Math.PI / 180f)) * LightRange();
				LightMatrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, new Vector3(num2 * 2f, num2 * 2f, LightRange()));
			}
		}
		base.transform.hasChanged = false;
		matrixReconstruct = false;
	}

	private void CheckLightType()
	{
		if (lastType != GetLightType())
		{
			if (lastType == LightType.Directional)
			{
				octreeNode = HxVolumetricCamera.AddLightOctree(this, minBounds, maxBounds);
				HxVolumetricCamera.ActiveDirectionalLights.Remove(this);
			}
			else if (GetLightType() == LightType.Directional && lastType != LightType.Directional)
			{
				HxVolumetricCamera.RemoveLightOctree(this);
				octreeNode = null;
				HxVolumetricCamera.ActiveDirectionalLights.Add(this);
			}
		}
		lastType = GetLightType();
	}

	private void LoadVolumeData()
	{
		if (ShaderModel4())
		{
			int num = 0;
			for (int i = 0; i < HxVolumetricCamera.ActiveVolumes.Count; i++)
			{
				if (HxVolumetricCamera.ActiveVolumes[i].enabled && (HxVolumetricCamera.ActiveVolumes[i].BlendMode != HxDensityVolume.DensityBlendMode.Add || HxVolumetricCamera.ActiveVolumes[i].BlendMode != HxDensityVolume.DensityBlendMode.Sub) && HxVolumetricCamera.ActiveVolumes[i].Density != 0f)
				{
					VolumeMatrixArrays[num] = HxVolumetricCamera.ActiveVolumes[i].ToLocalSpace;
					VolumeSettingsArrays[num] = new Vector2(HxVolumetricCamera.ActiveVolumes[i].Density, (float)(HxVolumetricCamera.ActiveVolumes[i].BlendMode + (int)HxVolumetricCamera.ActiveVolumes[i].Shape * 4));
					num++;
				}
				if (num >= 50)
				{
					break;
				}
			}
			if (num < 49)
			{
				VolumeSettingsArrays[num] = new Vector2(0f, -1f);
			}
			return;
		}
		int num2 = 0;
		for (int j = 0; j < HxVolumetricCamera.ActiveVolumes.Count; j++)
		{
			if (HxVolumetricCamera.ActiveVolumes[j].enabled && (HxVolumetricCamera.ActiveVolumes[j].BlendMode != HxDensityVolume.DensityBlendMode.Add || HxVolumetricCamera.ActiveVolumes[j].BlendMode != HxDensityVolume.DensityBlendMode.Sub) && HxVolumetricCamera.ActiveVolumes[j].Density != 0f)
			{
				VolumeMatrixArraysOld[num2] = HxVolumetricCamera.ActiveVolumes[j].ToLocalSpace;
				VolumeSettingsArraysOld[num2] = new Vector2(HxVolumetricCamera.ActiveVolumes[j].Density, (float)(HxVolumetricCamera.ActiveVolumes[j].BlendMode + (int)HxVolumetricCamera.ActiveVolumes[j].Shape * 4));
				num2++;
			}
			if (num2 >= 10)
			{
				break;
			}
		}
		if (num2 < 9)
		{
			VolumeSettingsArraysOld[num2] = new Vector2(0f, -1f);
		}
	}

	private bool BoundsIntersect(HxDensityVolume vol)
	{
		return minBounds.x <= vol.maxBounds.x && maxBounds.x >= vol.minBounds.x && minBounds.y <= vol.maxBounds.y && maxBounds.y >= vol.minBounds.y && minBounds.z <= vol.maxBounds.z && maxBounds.z >= vol.minBounds.z;
	}

	private void LoadVolumeDataBounds()
	{
		if (ShaderModel4())
		{
			int num = 0;
			for (int i = 0; i < HxVolumetricCamera.ActiveVolumes.Count; i++)
			{
				if (HxVolumetricCamera.ActiveVolumes[i].enabled && (HxVolumetricCamera.ActiveVolumes[i].BlendMode != HxDensityVolume.DensityBlendMode.Add || HxVolumetricCamera.ActiveVolumes[i].BlendMode != HxDensityVolume.DensityBlendMode.Sub) && HxVolumetricCamera.ActiveVolumes[i].Density != 0f && BoundsIntersect(HxVolumetricCamera.ActiveVolumes[i]))
				{
					VolumeMatrixArrays[num] = HxVolumetricCamera.ActiveVolumes[i].ToLocalSpace;
					VolumeSettingsArrays[num] = new Vector2(HxVolumetricCamera.ActiveVolumes[i].Density, (float)(HxVolumetricCamera.ActiveVolumes[i].BlendMode + (int)HxVolumetricCamera.ActiveVolumes[i].Shape * 4));
					num++;
				}
				if (num >= 50)
				{
					break;
				}
			}
			if (num < 49)
			{
				VolumeSettingsArrays[num] = new Vector2(0f, -1f);
			}
			return;
		}
		int num2 = 0;
		for (int j = 0; j < HxVolumetricCamera.ActiveVolumes.Count; j++)
		{
			if (HxVolumetricCamera.ActiveVolumes[j].enabled && (HxVolumetricCamera.ActiveVolumes[j].BlendMode != HxDensityVolume.DensityBlendMode.Add || HxVolumetricCamera.ActiveVolumes[j].BlendMode != HxDensityVolume.DensityBlendMode.Sub) && HxVolumetricCamera.ActiveVolumes[j].Density != 0f && BoundsIntersect(HxVolumetricCamera.ActiveVolumes[j]))
			{
				VolumeMatrixArraysOld[num2] = HxVolumetricCamera.ActiveVolumes[j].ToLocalSpace;
				VolumeSettingsArraysOld[num2] = new Vector2(HxVolumetricCamera.ActiveVolumes[j].Density, (float)(HxVolumetricCamera.ActiveVolumes[j].BlendMode + (int)HxVolumetricCamera.ActiveVolumes[j].Shape * 4));
				num2++;
			}
			if (num2 >= 10)
			{
				break;
			}
		}
		if (num2 < 9)
		{
			VolumeSettingsArraysOld[num2] = new Vector2(0f, -1f);
		}
	}

	private Vector4 NormalOfTriangle(Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 vector = Vector3.Cross(b - a, c - a);
		vector.Normalize();
		return new Vector4(vector.x, vector.y, vector.z, 0f);
	}

	private void DrawIntersect()
	{
		Vector3 forward = base.transform.forward;
		Vector3 forward2 = HxVolumetricCamera.Active.transform.forward;
		Vector3 position = base.transform.position;
		Vector3 position2 = HxVolumetricCamera.Active.transform.position;
		Vector3 vector = default(Vector3);
		vector.x = Vector3.Dot(forward, forward2);
		Vector3 vector2 = default(Vector3);
		vector2.x = Vector3.Dot(position + forward * LightRange() - position2, forward);
		vector.y = Vector3.Dot(BottomFrustumNormal, forward2);
		vector2.y = Vector3.Dot(position - position2, BottomFrustumNormal);
		vector.z = Vector3.Dot(LeftFrustumNormal, forward2);
		vector2.z = Vector3.Dot(position - position2, LeftFrustumNormal);
		Vector3 vector3 = default(Vector3);
		vector3.x = Vector3.Dot(-forward, forward2);
		Vector3 vector4 = default(Vector3);
		vector4.x = Vector3.Dot(position + forward * NearPlane - position2, -forward);
		vector3.y = Vector3.Dot(TopFrustumNormal, forward2);
		vector4.y = Vector3.Dot(position - position2, TopFrustumNormal);
		vector3.z = Vector3.Dot(RightFrustumNormal, forward2);
		vector4.z = Vector3.Dot(position - position2, RightFrustumNormal);
		float num = 0f;
		float num2 = 100000f;
		if (vector3.x > 0f)
		{
			num2 = Mathf.Min(num2, vector4.x / vector3.x);
		}
		else if (vector3.x < 0f)
		{
			num = Mathf.Max(num, vector4.x / vector3.x);
		}
		if (vector3.y > 0f)
		{
			num2 = Mathf.Min(num2, vector4.y / vector3.y);
		}
		else if (vector3.y < 0f)
		{
			num = Mathf.Max(num, vector4.y / vector3.y);
		}
		if (vector3.z > 0f)
		{
			num2 = Mathf.Min(num2, vector4.z / vector3.z);
		}
		else if (vector3.z < 0f)
		{
			num = Mathf.Max(num, vector4.z / vector3.z);
		}
		if (vector.x > 0f)
		{
			num2 = Mathf.Min(num2, vector2.x / vector.x);
		}
		else if (vector.x < 0f)
		{
			num = Mathf.Max(num, vector2.x / vector.x);
		}
		if (vector.y > 0f)
		{
			num2 = Mathf.Min(num2, vector2.y / vector.y);
		}
		else if (vector.y < 0f)
		{
			num = Mathf.Max(num, vector2.y / vector.y);
		}
		if (vector.z > 0f)
		{
			num2 = Mathf.Min(num2, vector2.z / vector.z);
		}
		else if (vector.z < 0f)
		{
			num = Mathf.Max(num, vector2.z / vector.z);
		}
		Debug.DrawLine(position2, position2 + (Vector3)RightFrustumNormal);
		Debug.DrawLine(position2 + forward2 * num, position2 + forward2 * num + Vector3.up);
		Debug.DrawLine(position2 + forward2 * num, position2 + forward2 * num2);
	}

	private float GetAspect()
	{
		if (myProjector != null)
		{
			return myProjector.aspectRatio;
		}
		return 0f;
	}

	private float GetOrthoSize()
	{
		if (myProjector != null)
		{
			return myProjector.orthographicSize;
		}
		return 0f;
	}

	private bool GetOrtho()
	{
		if (myProjector != null)
		{
			return myProjector.orthographic;
		}
		return false;
	}

	public void UpdatePosition(bool first = false)
	{
		if (!first && !base.transform.hasChanged && !matrixReconstruct && LastRange == LightRange() && LastSpotAngle == LightSpotAngle() && lastType == GetLightType() && (GetLightType() != LightType.Area || (LastAspect == GetAspect() && LastOrthoSize != GetOrthoSize() && LastOrtho != GetOrtho())))
		{
			return;
		}
		if (GetLightType() == LightType.Point)
		{
			Vector3 vector = new Vector3(LightRange(), LightRange(), LightRange());
			minBounds = base.transform.position - vector;
			maxBounds = base.transform.position + vector;
			lastBounds.SetMinMax(minBounds, maxBounds);
			if (!first)
			{
				CheckLightType();
				HxVolumetricCamera.LightOctree.Move(octreeNode, minBounds, maxBounds);
			}
			else
			{
				lastType = GetLightType();
			}
			UpdateLightMatrix();
			return;
		}
		if (GetLightType() == LightType.Spot)
		{
			Vector3 position = base.transform.position;
			Vector3 forward = base.transform.forward;
			Vector3 right = base.transform.right;
			Vector3 up = base.transform.up;
			Vector3 vector2 = position + forward * LightRange();
			float num = Mathf.Tan(LightSpotAngle() * ((float)Math.PI / 180f) / 2f) * LightRange();
			Vector3 vector3 = vector2 + up * num - right * num;
			Vector3 vector4 = vector2 + up * num + right * num;
			Vector3 vector5 = vector2 - up * num - right * num;
			Vector3 vector6 = vector2 - up * num + right * num;
			TopFrustumNormal = NormalOfTriangle(position, vector3, vector4);
			BottomFrustumNormal = NormalOfTriangle(position, vector6, vector5);
			LeftFrustumNormal = NormalOfTriangle(position, vector5, vector3);
			RightFrustumNormal = NormalOfTriangle(position, vector4, vector6);
			minBounds = new Vector3(Mathf.Min(vector3.x, Mathf.Min(vector4.x, Mathf.Min(vector5.x, Mathf.Min(vector6.x, position.x)))), Mathf.Min(vector3.y, Mathf.Min(vector4.y, Mathf.Min(vector5.y, Mathf.Min(vector6.y, position.y)))), Mathf.Min(vector3.z, Mathf.Min(vector4.z, Mathf.Min(vector5.z, Mathf.Min(vector6.z, position.z)))));
			maxBounds = new Vector3(Mathf.Max(vector3.x, Mathf.Max(vector4.x, Mathf.Max(vector5.x, Mathf.Max(vector6.x, position.x)))), Mathf.Max(vector3.y, Mathf.Max(vector4.y, Mathf.Max(vector5.y, Mathf.Max(vector6.y, position.y)))), Mathf.Max(vector3.z, Mathf.Max(vector4.z, Mathf.Max(vector5.z, Mathf.Max(vector6.z, position.z)))));
			lastBounds.SetMinMax(minBounds, maxBounds);
			if (!first)
			{
				CheckLightType();
				HxVolumetricCamera.LightOctree.Move(octreeNode, minBounds, maxBounds);
			}
			else
			{
				lastType = GetLightType();
			}
			UpdateLightMatrix();
			return;
		}
		if (myProjector != null)
		{
			Vector3 position2 = base.transform.position;
			Vector3 forward2 = base.transform.forward;
			Vector3 right2 = base.transform.right;
			Vector3 up2 = base.transform.up;
			Vector3 vector7 = position2 + forward2 * LightRange();
			Vector3 vector8 = position2 + forward2 * myProjector.nearClipPlane;
			float num2;
			float num3;
			if (myProjector.orthographic)
			{
				num2 = myProjector.orthographicSize;
				num3 = myProjector.orthographicSize;
			}
			else
			{
				num2 = Mathf.Tan(LightSpotAngle() * ((float)Math.PI / 180f) / 2f) * LightRange();
				num3 = Mathf.Tan(LightSpotAngle() * ((float)Math.PI / 180f) / 2f) * myProjector.nearClipPlane;
			}
			Vector3 vector9 = vector7 + up2 * num2 - right2 * (num2 * myProjector.aspectRatio);
			Vector3 vector10 = vector7 + up2 * num2 + right2 * (num2 * myProjector.aspectRatio);
			Vector3 vector11 = vector7 - up2 * num2 - right2 * (num2 * myProjector.aspectRatio);
			Vector3 vector12 = vector7 - up2 * num2 + right2 * (num2 * myProjector.aspectRatio);
			Vector3 vector13 = vector8 + up2 * num3 - right2 * (num3 * myProjector.aspectRatio);
			Vector3 vector14 = vector8 + up2 * num3 + right2 * (num3 * myProjector.aspectRatio);
			Vector3 vector15 = vector8 - up2 * num3 - right2 * (num3 * myProjector.aspectRatio);
			Vector3 vector16 = vector8 - up2 * num3 + right2 * (num3 * myProjector.aspectRatio);
			TopFrustumNormal = NormalOfTriangle(vector13, vector9, vector10);
			BottomFrustumNormal = NormalOfTriangle(vector16, vector12, vector11);
			LeftFrustumNormal = NormalOfTriangle(vector15, vector11, vector9);
			RightFrustumNormal = NormalOfTriangle(vector14, vector10, vector12);
			LastOrtho = GetOrtho();
			minBounds = Vector3.Min(vector9, Vector3.Min(vector10, Vector3.Min(vector11, Vector3.Min(vector12, Vector3.Min(vector13, Vector3.Min(vector14, Vector3.Min(vector15, vector16)))))));
			maxBounds = Vector3.Max(vector9, Vector3.Max(vector10, Vector3.Max(vector11, Vector3.Max(vector12, Vector3.Max(vector13, Vector3.Max(vector14, Vector3.Max(vector15, vector16)))))));
			LastOrthoSize = GetOrthoSize();
			lastBounds.SetMinMax(minBounds, maxBounds);
			if (!first)
			{
				CheckLightType();
				HxVolumetricCamera.LightOctree.Move(octreeNode, minBounds, maxBounds);
			}
			else
			{
				lastType = GetLightType();
			}
			LastAspect = GetAspect();
			UpdateLightMatrix();
		}
		if (!first)
		{
			CheckLightType();
		}
		else
		{
			lastType = GetLightType();
		}
	}

	public void DrawBounds()
	{
		if (GetLightType() != LightType.Directional)
		{
			Debug.DrawLine(new Vector3(minBounds.x, minBounds.y, minBounds.z), new Vector3(maxBounds.x, minBounds.y, minBounds.z), LightColor());
			Debug.DrawLine(new Vector3(maxBounds.x, minBounds.y, minBounds.z), new Vector3(maxBounds.x, minBounds.y, maxBounds.z), LightColor());
			Debug.DrawLine(new Vector3(maxBounds.x, minBounds.y, maxBounds.z), new Vector3(minBounds.x, minBounds.y, maxBounds.z), LightColor());
			Debug.DrawLine(new Vector3(minBounds.x, minBounds.y, maxBounds.z), new Vector3(minBounds.x, minBounds.y, minBounds.z), LightColor());
			Debug.DrawLine(new Vector3(minBounds.x, maxBounds.y, minBounds.z), new Vector3(maxBounds.x, maxBounds.y, minBounds.z), LightColor());
			Debug.DrawLine(new Vector3(maxBounds.x, maxBounds.y, minBounds.z), new Vector3(maxBounds.x, maxBounds.y, maxBounds.z), LightColor());
			Debug.DrawLine(new Vector3(maxBounds.x, maxBounds.y, maxBounds.z), new Vector3(minBounds.x, maxBounds.y, maxBounds.z), LightColor());
			Debug.DrawLine(new Vector3(minBounds.x, maxBounds.y, maxBounds.z), new Vector3(minBounds.x, maxBounds.y, minBounds.z), LightColor());
			Debug.DrawLine(new Vector3(minBounds.x, minBounds.y, minBounds.z), new Vector3(minBounds.x, maxBounds.y, minBounds.z), LightColor());
			Debug.DrawLine(new Vector3(maxBounds.x, minBounds.y, minBounds.z), new Vector3(maxBounds.x, maxBounds.y, minBounds.z), LightColor());
			Debug.DrawLine(new Vector3(maxBounds.x, minBounds.y, maxBounds.z), new Vector3(maxBounds.x, maxBounds.y, maxBounds.z), LightColor());
			Debug.DrawLine(new Vector3(minBounds.x, minBounds.y, maxBounds.z), new Vector3(minBounds.x, maxBounds.y, maxBounds.z), LightColor());
		}
	}
}
