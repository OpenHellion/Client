using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class VolumetricFog : MonoBehaviour
{
	private struct Vector3i
	{
		public int x;

		public int y;

		public int z;

		public Vector3i(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
	}

	private struct PointLightParams
	{
		public Vector3 pos;

		public float range;

		public Vector3 color;

		private float padding;
	}

	private struct TubeLightParams
	{
		public Vector3 start;

		public float range;

		public Vector3 end;

		public float radius;

		public Vector3 color;

		private float padding;
	}

	private struct TubeLightShadowPlaneParams
	{
		public Vector4 plane0;

		public Vector4 plane1;

		public float feather0;

		public float feather1;

		private float padding0;

		private float padding1;
	}

	private struct AreaLightParams
	{
		public Matrix4x4 mat;

		public Vector4 pos;

		public Vector3 color;

		public float bounded;
	}

	private struct FogEllipsoidParams
	{
		public Vector3 pos;

		public float radius;

		public Vector3 axis;

		public float stretch;

		public float density;

		public float noiseAmount;

		public float noiseSpeed;

		public float noiseScale;

		public float feather;

		public float blend;

		public float padding1;

		public float padding2;
	}

	private Material m_DebugMaterial;

	[HideInInspector]
	public Shader m_DebugShader;

	[HideInInspector]
	public Shader m_ShadowmapShader;

	[HideInInspector]
	public ComputeShader m_InjectLightingAndDensity;

	[HideInInspector]
	public ComputeShader m_Scatter;

	private Material m_ApplyToOpaqueMaterial;

	[HideInInspector]
	public Shader m_ApplyToOpaqueShader;

	private Material m_BlurShadowmapMaterial;

	[HideInInspector]
	public Shader m_BlurShadowmapShader;

	[HideInInspector]
	public Texture2D m_Noise;

	[HideInInspector]
	public bool m_Debug;

	[HideInInspector]
	[Range(0f, 1f)]
	public float m_Z = 1f;

	[Header("Size")]
	[MinValue(0.1f)]
	public float m_NearClip = 0.1f;

	[MinValue(0.1f)]
	public float m_FarClipMax = 100f;

	[Header("Fog Density")]
	[FormerlySerializedAs("m_Density")]
	public float m_GlobalDensityMult = 1f;

	private Vector3i m_InjectNumThreads = new Vector3i(16, 2, 16);

	private Vector3i m_ScatterNumThreads = new Vector3i(32, 2, 1);

	private RenderTexture m_VolumeInject;

	private RenderTexture m_VolumeScatter;

	private Vector3i m_VolumeResolution = new Vector3i(160, 90, 128);

	private Camera m_Camera;

	public float m_ConstantFog;

	public float m_HeightFogAmount;

	public float m_HeightFogExponent;

	public float m_HeightFogOffset;

	[Tooltip("Noise multiplies with constant fog and height fog, but not with fog ellipsoids.")]
	[Range(0f, 1f)]
	public float m_NoiseFogAmount;

	public float m_NoiseFogScale = 1f;

	public Wind m_Wind;

	[Range(0f, 0.999f)]
	public float m_Anisotropy;

	[Header("Lights")]
	[FormerlySerializedAs("m_Intensity")]
	public float m_GlobalIntensityMult = 1f;

	[MinValue(0f)]
	public float m_AmbientLightIntensity;

	public Color m_AmbientLightColor = Color.white;

	private PointLightParams[] m_PointLightParams;

	private ComputeBuffer m_PointLightParamsCB;

	private TubeLightParams[] m_TubeLightParams;

	private ComputeBuffer m_TubeLightParamsCB;

	private TubeLightShadowPlaneParams[] m_TubeLightShadowPlaneParams;

	private ComputeBuffer m_TubeLightShadowPlaneParamsCB;

	private AreaLightParams[] m_AreaLightParams;

	private ComputeBuffer m_AreaLightParamsCB;

	private FogEllipsoidParams[] m_FogEllipsoidParams;

	private ComputeBuffer m_FogEllipsoidParamsCB;

	private TubeLightShadowPlane.Params[] sppArr;

	private FogLight m_DirectionalLight;

	private float[] m_dirLightColor;

	private float[] m_dirLightDir;

	private float[] m_fogParams;

	private float[] m_windDir;

	private float[] m_ambientLight;

	private static readonly Vector2[] frustumUVs = new Vector2[4]
	{
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(1f, 1f),
		new Vector2(0f, 1f)
	};

	private static float[] frustumRays = new float[16];

	private Camera cam
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

	private float nearClip
	{
		get
		{
			return Mathf.Max(0f, m_NearClip);
		}
	}

	private float farClip
	{
		get
		{
			return Mathf.Min(cam.farClipPlane, m_FarClipMax);
		}
	}

	private void ReleaseComputeBuffer(ref ComputeBuffer buffer)
	{
		if (buffer != null)
		{
			buffer.Release();
		}
		buffer = null;
	}

	private void OnDestroy()
	{
		Cleanup();
	}

	private void OnDisable()
	{
		Cleanup();
	}

	private void Cleanup()
	{
		Object.DestroyImmediate(m_VolumeInject);
		Object.DestroyImmediate(m_VolumeScatter);
		ReleaseComputeBuffer(ref m_PointLightParamsCB);
		ReleaseComputeBuffer(ref m_TubeLightParamsCB);
		ReleaseComputeBuffer(ref m_TubeLightShadowPlaneParamsCB);
		ReleaseComputeBuffer(ref m_AreaLightParamsCB);
		ReleaseComputeBuffer(ref m_FogEllipsoidParamsCB);
		m_VolumeInject = null;
		m_VolumeScatter = null;
	}

	private void SanitizeInput()
	{
		m_GlobalDensityMult = Mathf.Max(m_GlobalDensityMult, 0f);
		m_ConstantFog = Mathf.Max(m_ConstantFog, 0f);
		m_HeightFogAmount = Mathf.Max(m_HeightFogAmount, 0f);
	}

	private void SetUpPointLightBuffers(int kernel)
	{
		int num = ((m_PointLightParamsCB != null) ? m_PointLightParamsCB.count : 0);
		m_InjectLightingAndDensity.SetFloat("_PointLightsCount", num);
		if (num == 0)
		{
			return;
		}
		if (m_PointLightParams == null || m_PointLightParams.Length != num)
		{
			m_PointLightParams = new PointLightParams[num];
		}
		HashSet<FogLight> hashSet = LightManager<FogLight>.Get();
		int num2 = 0;
		HashSet<FogLight>.Enumerator enumerator = hashSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			FogLight current = enumerator.Current;
			if (!(current == null) && current.type == LightOverride.Type.Point && current.isOn)
			{
				Light light = current.light;
				m_PointLightParams[num2].pos = light.transform.position;
				float num3 = light.range * current.m_RangeMult;
				m_PointLightParams[num2].range = 1f / (num3 * num3);
				m_PointLightParams[num2].color = new Vector3(light.color.r, light.color.g, light.color.b) * light.intensity * current.m_IntensityMult;
				num2++;
			}
		}
		m_PointLightParamsCB.SetData(m_PointLightParams);
		m_InjectLightingAndDensity.SetBuffer(kernel, "_PointLights", m_PointLightParamsCB);
	}

	private void SetUpTubeLightBuffers(int kernel)
	{
		int num = ((m_TubeLightParamsCB != null) ? m_TubeLightParamsCB.count : 0);
		m_InjectLightingAndDensity.SetFloat("_TubeLightsCount", num);
		if (num == 0)
		{
			return;
		}
		if (m_TubeLightParams == null || m_TubeLightParams.Length != num)
		{
			m_TubeLightParams = new TubeLightParams[num];
		}
		if (m_TubeLightShadowPlaneParams == null || m_TubeLightShadowPlaneParams.Length != num)
		{
			m_TubeLightShadowPlaneParams = new TubeLightShadowPlaneParams[num];
		}
		HashSet<FogLight> hashSet = LightManager<FogLight>.Get();
		int num2 = 0;
		HashSet<FogLight>.Enumerator enumerator = hashSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			FogLight current = enumerator.Current;
			if (!(current == null) && current.type == LightOverride.Type.Tube && current.isOn)
			{
				TubeLight tubeLight = current.tubeLight;
				Transform transform = tubeLight.transform;
				Vector3 position = transform.position;
				Vector3 vector = 0.5f * transform.up * tubeLight.m_Length;
				m_TubeLightParams[num2].start = position + vector;
				m_TubeLightParams[num2].end = position - vector;
				float num3 = tubeLight.m_Range * current.m_RangeMult;
				m_TubeLightParams[num2].range = 1f / (num3 * num3);
				m_TubeLightParams[num2].color = new Vector3(tubeLight.m_Color.r, tubeLight.m_Color.g, tubeLight.m_Color.b) * tubeLight.m_Intensity * current.m_IntensityMult;
				m_TubeLightParams[num2].radius = tubeLight.m_Radius;
				TubeLightShadowPlane.Params[] shadowPlaneParams = tubeLight.GetShadowPlaneParams(ref sppArr);
				m_TubeLightShadowPlaneParams[num2].plane0 = shadowPlaneParams[0].plane;
				m_TubeLightShadowPlaneParams[num2].plane1 = shadowPlaneParams[1].plane;
				m_TubeLightShadowPlaneParams[num2].feather0 = shadowPlaneParams[0].feather;
				m_TubeLightShadowPlaneParams[num2].feather1 = shadowPlaneParams[1].feather;
				num2++;
			}
		}
		m_TubeLightParamsCB.SetData(m_TubeLightParams);
		m_InjectLightingAndDensity.SetBuffer(kernel, "_TubeLights", m_TubeLightParamsCB);
		m_TubeLightShadowPlaneParamsCB.SetData(m_TubeLightShadowPlaneParams);
		m_InjectLightingAndDensity.SetBuffer(kernel, "_TubeLightShadowPlanes", m_TubeLightShadowPlaneParamsCB);
	}

	private void SetUpAreaLightBuffers(int kernel)
	{
		int num = ((m_AreaLightParamsCB != null) ? m_AreaLightParamsCB.count : 0);
		m_InjectLightingAndDensity.SetFloat("_AreaLightsCount", num);
		if (num == 0)
		{
			return;
		}
		if (m_AreaLightParams == null || m_AreaLightParams.Length != num)
		{
			m_AreaLightParams = new AreaLightParams[num];
		}
		HashSet<FogLight> hashSet = LightManager<FogLight>.Get();
		int num2 = hashSet.Count;
		int num3 = 0;
		HashSet<FogLight>.Enumerator enumerator = hashSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			FogLight current = enumerator.Current;
			if (current == null || current.type != LightOverride.Type.Area || !current.isOn)
			{
				continue;
			}
			AreaLight areaLight = current.areaLight;
			m_AreaLightParams[num3].mat = areaLight.GetProjectionMatrix(true);
			m_AreaLightParams[num3].pos = areaLight.GetPosition();
			m_AreaLightParams[num3].color = new Vector3(areaLight.m_Color.r, areaLight.m_Color.g, areaLight.m_Color.b) * areaLight.m_Intensity * current.m_IntensityMult;
			m_AreaLightParams[num3].bounded = (current.m_Bounded ? 1 : 0);
			if (current.m_Shadows)
			{
				RenderTexture blurredShadowmap = areaLight.GetBlurredShadowmap();
				if (blurredShadowmap != null)
				{
					m_InjectLightingAndDensity.SetTexture(kernel, "_AreaLightShadowmap", blurredShadowmap);
					m_InjectLightingAndDensity.SetFloat("_ESMExponentAreaLight", current.m_ESMExponent);
					num2 = num3;
				}
			}
			num3++;
		}
		m_AreaLightParamsCB.SetData(m_AreaLightParams);
		m_InjectLightingAndDensity.SetBuffer(kernel, "_AreaLights", m_AreaLightParamsCB);
		m_InjectLightingAndDensity.SetFloat("_ShadowedAreaLightIndex", num2);
	}

	private void SetUpFogEllipsoidBuffers(int kernel)
	{
		int num = 0;
		HashSet<FogEllipsoid> hashSet = LightManager<FogEllipsoid>.Get();
		HashSet<FogEllipsoid>.Enumerator enumerator = hashSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			FogEllipsoid current = enumerator.Current;
			if (current != null && current.enabled && current.gameObject.activeSelf)
			{
				num++;
			}
		}
		m_InjectLightingAndDensity.SetFloat("_FogEllipsoidsCount", num);
		if (num == 0)
		{
			return;
		}
		if (m_FogEllipsoidParams == null || m_FogEllipsoidParams.Length != num)
		{
			m_FogEllipsoidParams = new FogEllipsoidParams[num];
		}
		int num2 = 0;
		HashSet<FogEllipsoid>.Enumerator enumerator2 = hashSet.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			FogEllipsoid current2 = enumerator2.Current;
			if (!(current2 == null) && current2.enabled && current2.gameObject.activeSelf)
			{
				Transform transform = current2.transform;
				m_FogEllipsoidParams[num2].pos = transform.position;
				m_FogEllipsoidParams[num2].radius = current2.m_Radius * current2.m_Radius;
				m_FogEllipsoidParams[num2].axis = -transform.up;
				m_FogEllipsoidParams[num2].stretch = 1f / current2.m_Stretch - 1f;
				m_FogEllipsoidParams[num2].density = current2.m_Density;
				m_FogEllipsoidParams[num2].noiseAmount = current2.m_NoiseAmount;
				m_FogEllipsoidParams[num2].noiseSpeed = current2.m_NoiseSpeed;
				m_FogEllipsoidParams[num2].noiseScale = current2.m_NoiseScale;
				m_FogEllipsoidParams[num2].feather = 1f - current2.m_Feather;
				m_FogEllipsoidParams[num2].blend = ((current2.m_Blend != 0) ? 1 : 0);
				num2++;
			}
		}
		m_FogEllipsoidParamsCB.SetData(m_FogEllipsoidParams);
		m_InjectLightingAndDensity.SetBuffer(kernel, "_FogEllipsoids", m_FogEllipsoidParamsCB);
	}

	private FogLight GetDirectionalLight()
	{
		HashSet<FogLight> hashSet = LightManager<FogLight>.Get();
		FogLight result = null;
		HashSet<FogLight>.Enumerator enumerator = hashSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			FogLight current = enumerator.Current;
			if (current == null || current.type != LightOverride.Type.Directional || !current.isOn)
			{
				continue;
			}
			result = current;
			break;
		}
		return result;
	}

	private void OnPreRender()
	{
		m_DirectionalLight = GetDirectionalLight();
		if (m_DirectionalLight != null)
		{
			m_DirectionalLight.UpdateDirectionalShadowmap();
		}
	}

	private void SetUpDirectionalLight(int kernel)
	{
		if (m_dirLightColor == null || m_dirLightColor.Length != 3)
		{
			m_dirLightColor = new float[3];
		}
		if (m_dirLightDir == null || m_dirLightDir.Length != 3)
		{
			m_dirLightDir = new float[3];
		}
		if (m_DirectionalLight == null)
		{
			m_dirLightColor[0] = 0f;
			m_dirLightColor[1] = 0f;
			m_dirLightColor[2] = 0f;
			m_InjectLightingAndDensity.SetFloats("_DirLightColor", m_dirLightColor);
			return;
		}
		m_DirectionalLight.SetUpDirectionalShadowmapForSampling(m_DirectionalLight.m_Shadows, m_InjectLightingAndDensity, kernel);
		Light light = m_DirectionalLight.light;
		Vector4 vector = light.color;
		vector *= light.intensity * m_DirectionalLight.m_IntensityMult;
		m_dirLightColor[0] = vector.x;
		m_dirLightColor[1] = vector.y;
		m_dirLightColor[2] = vector.z;
		m_InjectLightingAndDensity.SetFloats("_DirLightColor", m_dirLightColor);
		Vector3 forward = light.GetComponent<Transform>().forward;
		m_dirLightDir[0] = forward.x;
		m_dirLightDir[1] = forward.y;
		m_dirLightDir[2] = forward.z;
		m_InjectLightingAndDensity.SetFloats("_DirLightDir", m_dirLightDir);
	}

	private void SetUpForScatter(int kernel)
	{
		SanitizeInput();
		InitResources();
		SetFrustumRays();
		float num = (farClip - nearClip) * 0.01f;
		m_InjectLightingAndDensity.SetFloat("_Density", m_GlobalDensityMult * 0.128f * num);
		m_InjectLightingAndDensity.SetFloat("_Intensity", m_GlobalIntensityMult);
		m_InjectLightingAndDensity.SetFloat("_Anisotropy", m_Anisotropy);
		m_InjectLightingAndDensity.SetTexture(kernel, "_VolumeInject", m_VolumeInject);
		m_InjectLightingAndDensity.SetTexture(kernel, "_Noise", m_Noise);
		if (m_fogParams == null || m_fogParams.Length != 4)
		{
			m_fogParams = new float[4];
		}
		if (m_windDir == null || m_windDir.Length != 3)
		{
			m_windDir = new float[3];
		}
		if (m_ambientLight == null || m_ambientLight.Length != 3)
		{
			m_ambientLight = new float[3];
		}
		m_fogParams[0] = m_ConstantFog;
		m_fogParams[1] = m_HeightFogExponent;
		m_fogParams[2] = m_HeightFogOffset;
		m_fogParams[3] = m_HeightFogAmount;
		m_InjectLightingAndDensity.SetFloats("_FogParams", m_fogParams);
		m_InjectLightingAndDensity.SetFloat("_NoiseFogAmount", m_NoiseFogAmount);
		m_InjectLightingAndDensity.SetFloat("_NoiseFogScale", m_NoiseFogScale);
		m_InjectLightingAndDensity.SetFloat("_WindSpeed", (!(m_Wind == null)) ? m_Wind.m_Speed : 0f);
		Vector3 vector = ((!(m_Wind == null)) ? m_Wind.transform.forward : Vector3.forward);
		m_windDir[0] = vector.x;
		m_windDir[1] = vector.y;
		m_windDir[2] = vector.z;
		m_InjectLightingAndDensity.SetFloats("_WindDir", m_windDir);
		m_InjectLightingAndDensity.SetFloat("_Time", Time.time);
		m_InjectLightingAndDensity.SetFloat("_NearOverFarClip", nearClip / farClip);
		Color color = m_AmbientLightColor * m_AmbientLightIntensity * 0.1f;
		m_ambientLight[0] = color.r;
		m_ambientLight[1] = color.g;
		m_ambientLight[2] = color.b;
		m_InjectLightingAndDensity.SetFloats("_AmbientLight", m_ambientLight);
		SetUpPointLightBuffers(kernel);
		SetUpTubeLightBuffers(kernel);
		SetUpAreaLightBuffers(kernel);
		SetUpFogEllipsoidBuffers(kernel);
		SetUpDirectionalLight(kernel);
	}

	private void Scatter()
	{
		int num = 0;
		SetUpForScatter(num);
		m_InjectLightingAndDensity.Dispatch(num, m_VolumeResolution.x / m_InjectNumThreads.x, m_VolumeResolution.y / m_InjectNumThreads.y, m_VolumeResolution.z / m_InjectNumThreads.z);
		m_Scatter.SetTexture(0, "_VolumeInject", m_VolumeInject);
		m_Scatter.SetTexture(0, "_VolumeScatter", m_VolumeScatter);
		m_Scatter.Dispatch(0, m_VolumeResolution.x / m_ScatterNumThreads.x, m_VolumeResolution.y / m_ScatterNumThreads.y, 1);
	}

	private void DebugDisplay(RenderTexture src, RenderTexture dest)
	{
		InitMaterial(ref m_DebugMaterial, m_DebugShader);
		m_DebugMaterial.SetTexture("_VolumeInject", m_VolumeInject);
		m_DebugMaterial.SetTexture("_VolumeScatter", m_VolumeScatter);
		m_DebugMaterial.SetFloat("_Z", m_Z);
		m_DebugMaterial.SetTexture("_MainTex", src);
		Graphics.Blit(src, dest, m_DebugMaterial);
	}

	private void SetUpGlobalFogSamplingUniforms(int width, int height)
	{
		Shader.SetGlobalTexture("_VolumeScatter", m_VolumeScatter);
		Shader.SetGlobalVector("_Screen_TexelSize", new Vector4(1f / (float)width, 1f / (float)height, width, height));
		Shader.SetGlobalVector("_VolumeScatter_TexelSize", new Vector4(1f / (float)m_VolumeResolution.x, 1f / (float)m_VolumeResolution.y, 1f / (float)m_VolumeResolution.z, 0f));
		Shader.SetGlobalFloat("_CameraFarOverMaxFar", cam.farClipPlane / farClip);
		Shader.SetGlobalFloat("_NearOverFarClip", nearClip / farClip);
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (!CheckSupport())
		{
			Debug.LogError(GetUnsupportedErrorMessage());
			Graphics.Blit(src, dest);
			base.enabled = false;
			return;
		}
		if (m_Debug)
		{
			DebugDisplay(src, dest);
			return;
		}
		Scatter();
		InitMaterial(ref m_ApplyToOpaqueMaterial, m_ApplyToOpaqueShader);
		m_ApplyToOpaqueMaterial.SetTexture("_MainTex", src);
		SetUpGlobalFogSamplingUniforms(src.width, src.height);
		Graphics.Blit(src, dest, m_ApplyToOpaqueMaterial);
		VolumetricFogInForward(true);
	}

	private void OnPostRender()
	{
		VolumetricFogInForward(false);
	}

	private void VolumetricFogInForward(bool enable)
	{
		if (enable)
		{
			Shader.EnableKeyword("VOLUMETRIC_FOG");
		}
		else
		{
			Shader.DisableKeyword("VOLUMETRIC_FOG");
		}
	}

	private Vector3 ViewportToLocalPoint(Camera c, Transform t, Vector3 p)
	{
		return t.InverseTransformPoint(c.ViewportToWorldPoint(p));
	}

	private void SetFrustumRays()
	{
		float z = farClip;
		Vector3 position = cam.transform.position;
		Vector2[] array = frustumUVs;
		for (int i = 0; i < 4; i++)
		{
			Vector3 vector = cam.ViewportToWorldPoint(new Vector3(array[i].x, array[i].y, z)) - position;
			frustumRays[i * 4] = vector.x;
			frustumRays[i * 4 + 1] = vector.y;
			frustumRays[i * 4 + 2] = vector.z;
			frustumRays[i * 4 + 3] = 0f;
		}
		m_InjectLightingAndDensity.SetVector("_CameraPos", position);
		m_InjectLightingAndDensity.SetFloats("_FrustumRays", frustumRays);
	}

	private void InitVolume(ref RenderTexture volume)
	{
		if (!volume)
		{
			volume = new RenderTexture(m_VolumeResolution.x, m_VolumeResolution.y, 0, RenderTextureFormat.ARGBHalf);
			volume.volumeDepth = m_VolumeResolution.z;
			volume.dimension = TextureDimension.Tex3D;
			volume.enableRandomWrite = true;
			volume.Create();
		}
	}

	private void CreateBuffer(ref ComputeBuffer buffer, int count, int stride)
	{
		if (buffer == null || buffer.count != count)
		{
			if (buffer != null)
			{
				buffer.Release();
				buffer = null;
			}
			if (count > 0)
			{
				buffer = new ComputeBuffer(count, stride);
			}
		}
	}

	private void InitResources()
	{
		InitVolume(ref m_VolumeInject);
		InitVolume(ref m_VolumeScatter);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		HashSet<FogLight> hashSet = LightManager<FogLight>.Get();
		HashSet<FogLight>.Enumerator enumerator = hashSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			FogLight current = enumerator.Current;
			if (current == null)
			{
				continue;
			}
			bool isOn = current.isOn;
			switch (current.type)
			{
			case LightOverride.Type.Point:
				if (isOn)
				{
					num++;
				}
				break;
			case LightOverride.Type.Tube:
				if (isOn)
				{
					num2++;
				}
				break;
			case LightOverride.Type.Area:
				if (isOn)
				{
					num3++;
				}
				break;
			}
		}
		CreateBuffer(ref m_PointLightParamsCB, num, 32);
		CreateBuffer(ref m_TubeLightParamsCB, num2, 48);
		CreateBuffer(ref m_TubeLightShadowPlaneParamsCB, num2, 48);
		CreateBuffer(ref m_AreaLightParamsCB, num3, 96);
		HashSet<FogEllipsoid> hashSet2 = LightManager<FogEllipsoid>.Get();
		CreateBuffer(ref m_FogEllipsoidParamsCB, (hashSet2 != null) ? hashSet2.Count : 0, 64);
	}

	private void ReleaseTemporary(ref RenderTexture rt)
	{
		if (!(rt == null))
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}

	private void InitMaterial(ref Material material, Shader shader)
	{
		if (!material)
		{
			if (!shader)
			{
				Debug.LogError("Missing shader");
				return;
			}
			material = new Material(shader);
			material.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawFrustum(Vector3.zero, cam.fieldOfView, farClip, nearClip, cam.aspect);
	}

	public static bool CheckSupport()
	{
		return SystemInfo.supportsComputeShaders;
	}

	public static string GetUnsupportedErrorMessage()
	{
		return string.Concat("Volumetric Fog requires compute shaders and this platform doesn't support them. Disabling. \nDetected device type: ", SystemInfo.graphicsDeviceType, ", version: ", SystemInfo.graphicsDeviceVersion);
	}
}
