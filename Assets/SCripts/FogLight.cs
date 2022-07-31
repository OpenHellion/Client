using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class FogLight : LightOverride
{
	public enum TextureSize
	{
		x256 = 0x100,
		x512 = 0x200,
		x1024 = 0x400
	}

	public bool m_ForceOnForFog;

	[Tooltip("Only one shadowed fog AreaLight at a time.")]
	[Header("Shadows")]
	public bool m_Shadows;

	[Tooltip("Always at most half the res of the AreaLight's shadowmap.")]
	public TextureSize m_ShadowmapRes = TextureSize.x256;

	[Range(0f, 3f)]
	public int m_BlurIterations;

	[MinValue(0f)]
	public float m_BlurSize = 1f;

	[MinValue(0f)]
	[Tooltip("Affects shadow softness.")]
	public float m_ESMExponent = 40f;

	public bool m_Bounded = true;

	private bool m_AddedToLightManager;

	private CommandBuffer m_BufGrabShadowmap;

	private CommandBuffer m_BufGrabShadowParams;

	private RenderTexture m_Shadowmap;

	private ComputeBuffer m_ShadowParamsCB;

	public Shader m_BlurShadowmapShader;

	private Material m_BlurShadowmapMaterial;

	public Shader m_CopyShadowParamsShader;

	private Material m_CopyShadowParamsMaterial;

	private int[] temp;

	private bool directionalShadow
	{
		get
		{
			return m_Shadows && base.type == Type.Directional;
		}
	}

	public override bool GetForceOn()
	{
		return m_ForceOnForFog;
	}

	private void AddToLightManager()
	{
		if (!m_AddedToLightManager)
		{
			m_AddedToLightManager = LightManager<FogLight>.Add(this);
		}
	}

	private void OnEnable()
	{
		AddToLightManager();
	}

	private void Update()
	{
		AddToLightManager();
	}

	private void OnDisable()
	{
		LightManager<FogLight>.Remove(this);
		m_AddedToLightManager = false;
		CleanupDirectionalShadowmap();
	}

	private void InitDirectionalShadowmap()
	{
		if (m_BufGrabShadowmap == null && directionalShadow)
		{
			Light component = GetComponent<Light>();
			m_BufGrabShadowmap = new CommandBuffer();
			m_BufGrabShadowmap.name = "Grab shadowmap for Volumetric Fog";
			component.AddCommandBuffer(LightEvent.AfterShadowMap, m_BufGrabShadowmap);
			m_BufGrabShadowParams = new CommandBuffer();
			m_BufGrabShadowParams.name = "Grab shadow params for Volumetric Fog";
			component.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, m_BufGrabShadowParams);
			m_BlurShadowmapMaterial = new Material(m_BlurShadowmapShader);
			m_BlurShadowmapMaterial.hideFlags = HideFlags.HideAndDontSave;
			m_CopyShadowParamsMaterial = new Material(m_CopyShadowParamsShader);
			m_CopyShadowParamsMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	public void UpdateDirectionalShadowmap()
	{
		InitDirectionalShadowmap();
		if (m_BufGrabShadowmap != null)
		{
			m_BufGrabShadowmap.Clear();
		}
		if (m_BufGrabShadowParams != null)
		{
			m_BufGrabShadowParams.Clear();
		}
		if (!directionalShadow)
		{
			return;
		}
		if (m_ShadowParamsCB == null)
		{
			m_ShadowParamsCB = new ComputeBuffer(1, 336);
		}
		Graphics.SetRandomWriteTarget(2, m_ShadowParamsCB);
		m_BufGrabShadowParams.DrawProcedural(Matrix4x4.identity, m_CopyShadowParamsMaterial, 0, MeshTopology.Points, 1);
		int num = 4096;
		int num2 = Mathf.Min((int)m_ShadowmapRes, num / 2);
		int num3 = (int)Mathf.Log(num / num2, 2f);
		RenderTargetIdentifier renderTargetIdentifier = BuiltinRenderTextureType.CurrentActive;
		m_BufGrabShadowmap.SetShadowSamplingMode(renderTargetIdentifier, ShadowSamplingMode.RawDepth);
		RenderTextureFormat format = RenderTextureFormat.RGHalf;
		ReleaseTemporary(ref m_Shadowmap);
		m_Shadowmap = RenderTexture.GetTemporary(num2, num2, 0, format, RenderTextureReadWrite.Linear);
		m_Shadowmap.filterMode = FilterMode.Bilinear;
		m_Shadowmap.wrapMode = TextureWrapMode.Clamp;
		if (temp == null || temp.Length != num3 - 1)
		{
			temp = new int[num3 - 1];
		}
		int i = 0;
		int num4 = num / 2;
		for (; i < num3; i++)
		{
			m_BufGrabShadowmap.SetGlobalVector("_TexelSize", new Vector4(0.5f / (float)num4, 0.5f / (float)num4, 0f, 0f));
			RenderTargetIdentifier dest;
			if (i < num3 - 1)
			{
				temp[i] = Shader.PropertyToID("ShadowmapDownscaleTemp" + i);
				m_BufGrabShadowmap.GetTemporaryRT(temp[i], num4, num4, 0, FilterMode.Bilinear, format, RenderTextureReadWrite.Linear);
				dest = new RenderTargetIdentifier(temp[i]);
			}
			else
			{
				dest = new RenderTargetIdentifier(m_Shadowmap);
			}
			if (i == 0)
			{
				m_BufGrabShadowmap.SetGlobalTexture("_DirShadowmap", renderTargetIdentifier);
				m_BufGrabShadowmap.Blit(null, dest, m_BlurShadowmapMaterial, 4);
			}
			else
			{
				m_BufGrabShadowmap.Blit(temp[i - 1], dest, m_BlurShadowmapMaterial, 5);
			}
			num4 /= 2;
		}
	}

	private void CleanupDirectionalShadowmap()
	{
		if (m_BufGrabShadowmap != null)
		{
			m_BufGrabShadowmap.Clear();
		}
		if (m_BufGrabShadowParams != null)
		{
			m_BufGrabShadowParams.Clear();
		}
		if (m_ShadowParamsCB != null)
		{
			m_ShadowParamsCB.Release();
		}
		m_ShadowParamsCB = null;
	}

	public bool SetUpDirectionalShadowmapForSampling(bool shadows, ComputeShader cs, int kernel)
	{
		if (!shadows || m_ShadowParamsCB == null || m_Shadowmap == null)
		{
			cs.SetFloat("_DirLightShadows", 0f);
			return false;
		}
		cs.SetFloat("_DirLightShadows", 1f);
		cs.SetBuffer(kernel, "_ShadowParams", m_ShadowParamsCB);
		cs.SetTexture(kernel, "_DirectionalShadowmap", m_Shadowmap);
		return true;
	}

	private void ReleaseTemporary(ref RenderTexture rt)
	{
		if (!(rt == null))
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}
}
