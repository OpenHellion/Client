using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class AreaLight : MonoBehaviour
{
	public enum TextureSize
	{
		x512 = 0x200,
		x1024 = 0x400,
		x2048 = 0x800,
		x4096 = 0x1000
	}

	public bool m_RenderSource = true;

	public Vector3 m_Size = new Vector3(1f, 1f, 2f);

	[Range(0f, 179f)] public float m_Angle;

	[MinValue(0f)] public float m_Intensity = 0.8f;

	public Color m_Color = Color.white;

	[Title("Shadows")] public bool m_Shadows;

	public LayerMask m_ShadowCullingMask = -1;

	public TextureSize m_ShadowmapRes = TextureSize.x2048;

	[MinValue(0f)] public float m_ReceiverSearchDistance = 24f;

	[MinValue(0f)] public float m_ReceiverDistanceScale = 5f;

	[MinValue(0f)] public float m_LightNearSize = 4f;

	[MinValue(0f)] public float m_LightFarSize = 22f;

	[Range(0f, 0.1f)] public float m_ShadowBias = 0.001f;

	private MeshRenderer m_SourceRenderer;

	private Mesh m_SourceMesh;

	[HideInInspector] public Mesh m_Quad;

	private Vector2 m_CurrentQuadSize = Vector2.zero;

	private Vector3 m_CurrentSize = Vector3.zero;

	private float m_CurrentAngle = -1f;

	private bool m_Initialized;

	private MaterialPropertyBlock m_props;

	private static Vector3[] vertices = new Vector3[4];

	[HideInInspector] public Mesh m_Cube;

	[HideInInspector] public Shader m_ProxyShader;

	private Material m_ProxyMaterial;

	private static Texture2D s_TransformInvTexture_Specular;

	private static Texture2D s_TransformInvTexture_Diffuse;

	private static Texture2D s_AmpDiffAmpSpecFresnel;

	private Dictionary<Camera, CommandBuffer> m_Cameras = new Dictionary<Camera, CommandBuffer>();

	private static CameraEvent kCameraEvent = CameraEvent.AfterLighting;

	private static readonly float[,] offsets = new float[4, 2]
	{
		{ 1f, 1f },
		{ 1f, -1f },
		{ -1f, -1f },
		{ -1f, 1f }
	};

	private Camera m_ShadowmapCamera;

	private Transform m_ShadowmapCameraTransform;

	[HideInInspector] public Shader m_ShadowmapShader;

	[HideInInspector] public Shader m_BlurShadowmapShader;

	private Material m_BlurShadowmapMaterial;

	private RenderTexture m_Shadowmap;

	private RenderTexture m_BlurredShadowmap;

	private Texture2D m_ShadowmapDummy;

	private int m_ShadowmapRenderTime = -1;

	private int m_BlurredShadowmapRenderTime = -1;

	private FogLight m_FogLight;

	private RenderTexture[] temp;

	private void Awake()
	{
		if (Init())
		{
			UpdateSourceMesh();
		}
	}

	private bool Init()
	{
		if (m_Initialized)
		{
			return true;
		}

		if (m_Quad == null || !InitDirect())
		{
			return false;
		}

		m_SourceRenderer = GetComponent<MeshRenderer>();
		m_SourceRenderer.enabled = true;
		m_SourceMesh = Instantiate(m_Quad);
		m_SourceMesh.hideFlags = HideFlags.HideAndDontSave;
		MeshFilter component = gameObject.GetComponent<MeshFilter>();
		component.sharedMesh = m_SourceMesh;
		Transform transform = base.transform;
		if (transform.localScale != Vector3.one)
		{
			transform.localScale = Vector3.one;
		}

		SetUpLUTs();
		m_Initialized = true;
		return true;
	}

	private void OnEnable()
	{
		m_props = new MaterialPropertyBlock();
		if (Init())
		{
			UpdateSourceMesh();
		}
	}

	private void OnDisable()
	{
		if (!Application.isPlaying)
		{
			Cleanup();
			return;
		}

		Dictionary<Camera, CommandBuffer>.Enumerator enumerator = m_Cameras.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value != null)
			{
				enumerator.Current.Value.Clear();
			}
		}
	}

	private void OnDestroy()
	{
		if (m_ProxyMaterial != null)
		{
			DestroyImmediate(m_ProxyMaterial);
		}

		if (m_SourceMesh != null)
		{
			DestroyImmediate(m_SourceMesh);
		}

		Cleanup();
	}

	private void UpdateSourceMesh()
	{
		m_Size.x = Mathf.Max(m_Size.x, 0f);
		m_Size.y = Mathf.Max(m_Size.y, 0f);
		m_Size.z = Mathf.Max(m_Size.z, 0f);
		Vector2 vector =
			((!m_RenderSource || !enabled) ? (Vector2.one * 0.0001f) : new Vector2(m_Size.x, m_Size.y));
		if (vector != m_CurrentQuadSize)
		{
			float num = vector.x * 0.5f;
			float num2 = vector.y * 0.5f;
			float newZ = -0.001f;
			vertices[0].Set(0f - num, num2, newZ);
			vertices[1].Set(num, 0f - num2, newZ);
			vertices[2].Set(num, num2, newZ);
			vertices[3].Set(0f - num, 0f - num2, newZ);
			m_SourceMesh.vertices = vertices;
			m_CurrentQuadSize = vector;
		}

		if (m_Size != m_CurrentSize || m_Angle != m_CurrentAngle)
		{
			m_SourceMesh.bounds = GetFrustumBounds();
		}
	}

	private void Update()
	{
		if (!gameObject.activeInHierarchy || !enabled)
		{
			Cleanup();
		}
		else
		{
			if (!Init())
			{
				return;
			}

			UpdateSourceMesh();
			if (!Application.isPlaying)
			{
				return;
			}

			Dictionary<Camera, CommandBuffer>.Enumerator enumerator = m_Cameras.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Value != null)
				{
					enumerator.Current.Value.Clear();
				}
			}
		}
	}

	private void OnWillRenderObject()
	{
		if (Init())
		{
			Color color = new Color(Mathf.GammaToLinearSpace(m_Color.r), Mathf.GammaToLinearSpace(m_Color.g),
				Mathf.GammaToLinearSpace(m_Color.b), 1f);
			m_props.SetVector("_EmissionColor", color * m_Intensity);
			m_SourceRenderer.SetPropertyBlock(m_props);
			SetUpCommandBuffer();
		}
	}

	private float GetNearToCenter()
	{
		if (m_Angle == 0f)
		{
			return 0f;
		}

		return m_Size.y * 0.5f / Mathf.Tan(m_Angle * 0.5f * ((float)Math.PI / 180f));
	}

	private Matrix4x4 GetOffsetMatrix(float zOffset)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		identity.SetColumn(3, new Vector4(0f, 0f, zOffset, 1f));
		return identity;
	}

	public Matrix4x4 GetProjectionMatrix(bool linearZ = false)
	{
		Matrix4x4 matrix4x;
		if (m_Angle == 0f)
		{
			matrix4x = Matrix4x4.Ortho(-0.5f * m_Size.x, 0.5f * m_Size.x, -0.5f * m_Size.y, 0.5f * m_Size.y, 0f,
				0f - m_Size.z);
		}
		else
		{
			float nearToCenter = GetNearToCenter();
			if (linearZ)
			{
				matrix4x = PerspectiveLinearZ(m_Angle, m_Size.x / m_Size.y, nearToCenter, nearToCenter + m_Size.z);
			}
			else
			{
				matrix4x = Matrix4x4.Perspective(m_Angle, m_Size.x / m_Size.y, nearToCenter, nearToCenter + m_Size.z);
				matrix4x *= Matrix4x4.Scale(new Vector3(1f, 1f, -1f));
			}

			matrix4x *= GetOffsetMatrix(nearToCenter);
		}

		return matrix4x * transform.worldToLocalMatrix;
	}

	public Vector4 MultiplyPoint(Matrix4x4 m, Vector3 v)
	{
		Vector4 result = default(Vector4);
		result.x = m.m00 * v.x + m.m01 * v.y + m.m02 * v.z + m.m03;
		result.y = m.m10 * v.x + m.m11 * v.y + m.m12 * v.z + m.m13;
		result.z = m.m20 * v.x + m.m21 * v.y + m.m22 * v.z + m.m23;
		result.w = m.m30 * v.x + m.m31 * v.y + m.m32 * v.z + m.m33;
		return result;
	}

	private Matrix4x4 PerspectiveLinearZ(float fov, float aspect, float near, float far)
	{
		float f = (float)Math.PI / 180f * fov * 0.5f;
		float num = Mathf.Cos(f) / Mathf.Sin(f);
		float num2 = 1f / (far - near);
		Matrix4x4 result = default(Matrix4x4);
		result.m00 = num / aspect;
		result.m01 = 0f;
		result.m02 = 0f;
		result.m03 = 0f;
		result.m10 = 0f;
		result.m11 = num;
		result.m12 = 0f;
		result.m13 = 0f;
		result.m20 = 0f;
		result.m21 = 0f;
		result.m22 = 2f * num2;
		result.m23 = (0f - (far + near)) * num2;
		result.m30 = 0f;
		result.m31 = 0f;
		result.m32 = 1f;
		result.m33 = 0f;
		return result;
	}

	public Vector4 GetPosition()
	{
		Transform transform = base.transform;
		if (m_Angle == 0f)
		{
			Vector3 vector = -transform.forward;
			return new Vector4(vector.x, vector.y, vector.z, 0f);
		}

		Vector3 vector2 = transform.position - GetNearToCenter() * transform.forward;
		return new Vector4(vector2.x, vector2.y, vector2.z, 1f);
	}

	private Bounds GetFrustumBounds()
	{
		if (m_Angle == 0f)
		{
			return new Bounds(Vector3.zero, m_Size);
		}

		float num = Mathf.Tan(m_Angle * 0.5f * ((float)Math.PI / 180f));
		float num2 = m_Size.y * 0.5f / num;
		float z = m_Size.z;
		float num3 = (num2 + m_Size.z) * num * 2f;
		float x = m_Size.x * num3 / m_Size.y;
		return new Bounds(Vector3.forward * m_Size.z * 0.5f, new Vector3(x, num3, z));
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		if (m_Angle == 0f)
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireCube(new Vector3(0f, 0f, 0.5f * m_Size.z), m_Size);
			return;
		}

		float nearToCenter = GetNearToCenter();
		Gizmos.matrix = transform.localToWorldMatrix * GetOffsetMatrix(0f - nearToCenter);
		Gizmos.DrawFrustum(Vector3.zero, m_Angle, nearToCenter + m_Size.z, nearToCenter, m_Size.x / m_Size.y);
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = Color.yellow;
		Bounds frustumBounds = GetFrustumBounds();
		Gizmos.DrawWireCube(frustumBounds.center, frustumBounds.size);
	}

	private bool InitDirect()
	{
		if (m_ProxyShader == null || m_Cube == null)
		{
			return false;
		}

		m_ProxyMaterial = new Material(m_ProxyShader);
		m_ProxyMaterial.hideFlags = HideFlags.HideAndDontSave;
		return true;
	}

	private void SetUpLUTs()
	{
		if (s_TransformInvTexture_Diffuse == null)
		{
			s_TransformInvTexture_Diffuse = AreaLightLUT.LoadLUT(AreaLightLUT.LUTType.TransformInv_DisneyDiffuse);
		}

		if (s_TransformInvTexture_Specular == null)
		{
			s_TransformInvTexture_Specular = AreaLightLUT.LoadLUT(AreaLightLUT.LUTType.TransformInv_GGX);
		}

		if (s_AmpDiffAmpSpecFresnel == null)
		{
			s_AmpDiffAmpSpecFresnel = AreaLightLUT.LoadLUT(AreaLightLUT.LUTType.AmpDiffAmpSpecFresnel);
		}

		m_ProxyMaterial.SetTexture("_TransformInv_Diffuse", s_TransformInvTexture_Diffuse);
		m_ProxyMaterial.SetTexture("_TransformInv_Specular", s_TransformInvTexture_Specular);
		m_ProxyMaterial.SetTexture("_AmpDiffAmpSpecFresnel", s_AmpDiffAmpSpecFresnel);
	}

	private void Cleanup()
	{
		Dictionary<Camera, CommandBuffer>.Enumerator enumerator = m_Cameras.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<Camera, CommandBuffer> current = enumerator.Current;
			if (current.Key != null && current.Value != null)
			{
				current.Key.RemoveCommandBuffer(kCameraEvent, current.Value);
			}
		}

		m_Cameras.Clear();
	}

	private CommandBuffer GetOrCreateCommandBuffer(Camera cam)
	{
		if (cam == null)
		{
			return null;
		}

		CommandBuffer commandBuffer = null;
		if (!m_Cameras.ContainsKey(cam))
		{
			commandBuffer = new CommandBuffer();
			commandBuffer.name = gameObject.name;
			m_Cameras[cam] = commandBuffer;
			cam.AddCommandBuffer(kCameraEvent, commandBuffer);
			cam.depthTextureMode |= DepthTextureMode.Depth;
		}
		else
		{
			commandBuffer = m_Cameras[cam];
			commandBuffer.Clear();
		}

		return commandBuffer;
	}

	public void SetUpCommandBuffer()
	{
		if (!InsideShadowmapCameraRender())
		{
			Camera current = Camera.current;
			CommandBuffer orCreateCommandBuffer = GetOrCreateCommandBuffer(current);
			orCreateCommandBuffer.SetGlobalVector("_LightPos", base.transform.position);
			orCreateCommandBuffer.SetGlobalVector("_LightColor", GetColor());
			SetUpLUTs();
			orCreateCommandBuffer.SetGlobalFloat("_LightAsQuad", 0f);
			float z = 0.01f;
			Transform transform = base.transform;
			Matrix4x4 value = default(Matrix4x4);
			for (int i = 0; i < 4; i++)
			{
				value.SetRow(i,
					transform.TransformPoint(new Vector3(m_Size.x * offsets[i, 0], m_Size.y * offsets[i, 1], z) *
					                         0.5f));
			}

			orCreateCommandBuffer.SetGlobalMatrix("_LightVerts", value);
			if (m_Shadows)
			{
				SetUpShadowmapForSampling(orCreateCommandBuffer);
			}

			Matrix4x4 matrix4x = Matrix4x4.TRS(new Vector3(0f, 0f, 10f), Quaternion.identity, Vector3.one * 20f);
			orCreateCommandBuffer.DrawMesh(m_Cube, transform.localToWorldMatrix * matrix4x, m_ProxyMaterial, 0,
				(!m_Shadows) ? 1 : 0);
		}
	}

	private void SetKeyword(string keyword, bool on)
	{
		if (on)
		{
			m_ProxyMaterial.EnableKeyword(keyword);
		}
		else
		{
			m_ProxyMaterial.DisableKeyword(keyword);
		}
	}

	private void ReleaseTemporary(ref RenderTexture rt)
	{
		if (!(rt == null))
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}

	private Color GetColor()
	{
		if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
		{
			return m_Color * m_Intensity;
		}

		return new Color(Mathf.GammaToLinearSpace(m_Color.r * m_Intensity),
			Mathf.GammaToLinearSpace(m_Color.g * m_Intensity), Mathf.GammaToLinearSpace(m_Color.b * m_Intensity), 1f);
	}

	private void UpdateShadowmap(int res)
	{
		if (m_Shadowmap != null && m_ShadowmapRenderTime == Time.renderedFrameCount)
		{
			return;
		}

		if (m_ShadowmapCamera == null)
		{
			if (m_ShadowmapShader == null)
			{
				Debug.LogError("AreaLight's shadowmap shader not assigned.", this);
				return;
			}

			GameObject gameObject = new GameObject("Shadowmap Camera");
			gameObject.AddComponent(typeof(Camera));
			m_ShadowmapCamera = gameObject.GetComponent<Camera>();
			gameObject.hideFlags = HideFlags.HideAndDontSave;
			m_ShadowmapCamera.enabled = false;
			m_ShadowmapCamera.clearFlags = CameraClearFlags.Color;
			m_ShadowmapCamera.renderingPath = RenderingPath.Forward;
			m_ShadowmapCamera.backgroundColor = Color.white;
			m_ShadowmapCameraTransform = gameObject.transform;
			m_ShadowmapCameraTransform.parent = transform;
			m_ShadowmapCameraTransform.localRotation = Quaternion.identity;
		}

		if (m_Angle == 0f)
		{
			m_ShadowmapCamera.orthographic = true;
			m_ShadowmapCameraTransform.localPosition = Vector3.zero;
			m_ShadowmapCamera.nearClipPlane = 0f;
			m_ShadowmapCamera.farClipPlane = m_Size.z;
			m_ShadowmapCamera.orthographicSize = 0.5f * m_Size.y;
			m_ShadowmapCamera.aspect = m_Size.x / m_Size.y;
		}
		else
		{
			m_ShadowmapCamera.orthographic = false;
			float nearToCenter = GetNearToCenter();
			m_ShadowmapCameraTransform.localPosition = (0f - nearToCenter) * Vector3.forward;
			m_ShadowmapCamera.nearClipPlane = nearToCenter;
			m_ShadowmapCamera.farClipPlane = nearToCenter + m_Size.z;
			m_ShadowmapCamera.fieldOfView = m_Angle;
			m_ShadowmapCamera.aspect = m_Size.x / m_Size.y;
		}

		ReleaseTemporary(ref m_Shadowmap);
		m_Shadowmap = RenderTexture.GetTemporary(res, res, 24, RenderTextureFormat.Shadowmap);
		m_Shadowmap.name = "AreaLight Shadowmap";
		m_Shadowmap.filterMode = FilterMode.Bilinear;
		m_Shadowmap.wrapMode = TextureWrapMode.Clamp;
		m_ShadowmapCamera.targetTexture = m_Shadowmap;
		m_ShadowmapCamera.cullingMask = 0;
		m_ShadowmapCamera.Render();
		m_ShadowmapCamera.cullingMask = m_ShadowCullingMask;
		bool invertCulling = GL.invertCulling;
		GL.invertCulling = false;
		m_ShadowmapCamera.RenderWithShader(m_ShadowmapShader, "RenderType");
		GL.invertCulling = invertCulling;
		m_ShadowmapRenderTime = Time.renderedFrameCount;
	}

	public RenderTexture GetBlurredShadowmap()
	{
		UpdateBlurredShadowmap();
		return m_BlurredShadowmap;
	}

	private void UpdateBlurredShadowmap()
	{
		if (m_BlurredShadowmap != null && m_BlurredShadowmapRenderTime == Time.renderedFrameCount)
		{
			return;
		}

		InitFogLight();
		int num = (int)m_ShadowmapRes;
		int num2 = (int)m_FogLight.m_ShadowmapRes;
		if (isActiveAndEnabled && m_Shadows)
		{
			num2 = Mathf.Min(num2, num / 2);
		}
		else
		{
			num = 2 * num;
		}

		UpdateShadowmap(num);
		RenderTexture active = RenderTexture.active;
		ReleaseTemporary(ref m_BlurredShadowmap);
		InitMaterial(ref m_BlurShadowmapMaterial, m_BlurShadowmapShader);
		int num3 = (int)Mathf.Log(num / num2, 2f);
		if (temp == null || temp.Length != num3)
		{
			temp = new RenderTexture[num3];
		}

		RenderTextureFormat format = RenderTextureFormat.RGHalf;
		int i = 0;
		int num4 = num / 2;
		for (; i < num3; i++)
		{
			temp[i] = RenderTexture.GetTemporary(num4, num4, 0, format, RenderTextureReadWrite.Linear);
			temp[i].name = "AreaLight Shadow Downsample";
			temp[i].filterMode = FilterMode.Bilinear;
			temp[i].wrapMode = TextureWrapMode.Clamp;
			m_BlurShadowmapMaterial.SetVector("_TexelSize",
				new Vector4(0.5f / num4, 0.5f / num4, 0f, 0f));
			if (i == 0)
			{
				m_BlurShadowmapMaterial.SetTexture("_Shadowmap", m_Shadowmap);
				InitShadowmapDummy();
				m_BlurShadowmapMaterial.SetTexture("_ShadowmapDummy", m_ShadowmapDummy);
				m_BlurShadowmapMaterial.SetVector("_ZParams", GetZParams());
				m_BlurShadowmapMaterial.SetFloat("_ESMExponent", m_FogLight.m_ESMExponent);
				Blur(m_Shadowmap, temp[i], 0);
			}
			else
			{
				m_BlurShadowmapMaterial.SetTexture("_MainTex", temp[i - 1]);
				Blur(temp[i - 1], temp[i], 1);
			}

			num4 /= 2;
		}

		for (int j = 0; j < num3 - 1; j++)
		{
			RenderTexture.ReleaseTemporary(temp[j]);
		}

		m_BlurredShadowmap = temp[num3 - 1];
		if (m_FogLight.m_BlurIterations > 0)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(num2, num2, 0, format, RenderTextureReadWrite.Linear);
			temporary.name = "AreaLight Shadow Blur";
			temporary.filterMode = FilterMode.Bilinear;
			temporary.wrapMode = TextureWrapMode.Clamp;
			m_BlurShadowmapMaterial.SetVector("_MainTex_TexelSize",
				new Vector4(1f / num2, 1f / num2, 0f, 0f));
			float num5 = m_FogLight.m_BlurSize;
			for (int k = 0; k < m_FogLight.m_BlurIterations; k++)
			{
				m_BlurShadowmapMaterial.SetFloat("_BlurSize", num5);
				Blur(m_BlurredShadowmap, temporary, 2);
				Blur(temporary, m_BlurredShadowmap, 3);
				num5 *= 1.2f;
			}

			RenderTexture.ReleaseTemporary(temporary);
		}

		RenderTexture.active = active;
		m_BlurredShadowmapRenderTime = Time.renderedFrameCount;
	}

	private void Blur(RenderTexture src, RenderTexture dst, int pass)
	{
		RenderTexture.active = dst;
		m_BlurShadowmapMaterial.SetTexture("_MainTex", src);
		m_BlurShadowmapMaterial.SetPass(pass);
		RenderQuad();
	}

	private void RenderQuad()
	{
		GL.Begin(7);
		GL.TexCoord2(0f, 0f);
		GL.Vertex3(-1f, 1f, 0f);
		GL.TexCoord2(0f, 1f);
		GL.Vertex3(-1f, -1f, 0f);
		GL.TexCoord2(1f, 1f);
		GL.Vertex3(1f, -1f, 0f);
		GL.TexCoord2(1f, 0f);
		GL.Vertex3(1f, 1f, 0f);
		GL.End();
	}

	private void SetUpShadowmapForSampling(CommandBuffer buf)
	{
		UpdateShadowmap((int)m_ShadowmapRes);
		buf.SetGlobalTexture("_Shadowmap", m_Shadowmap);
		InitShadowmapDummy();
		m_ProxyMaterial.SetTexture("_ShadowmapDummy", m_ShadowmapDummy);
		buf.SetGlobalMatrix("_ShadowProjectionMatrix", GetProjectionMatrix());
		float num = (float)m_ShadowmapRes;
		float num2 = num / 2048f;
		buf.SetGlobalFloat("_ShadowReceiverWidth", num2 * m_ReceiverSearchDistance / num);
		buf.SetGlobalFloat("_ShadowReceiverDistanceScale", m_ReceiverDistanceScale * 0.5f / 10f);
		Vector2 vector = new Vector2(m_LightNearSize, m_LightFarSize) * num2 / num;
		buf.SetGlobalVector("_ShadowLightWidth", vector);
		buf.SetGlobalFloat("_ShadowBias", m_ShadowBias);
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

	private void InitShadowmapDummy()
	{
		if (!(m_ShadowmapDummy != null))
		{
			m_ShadowmapDummy = new Texture2D(1, 1, TextureFormat.Alpha8, false, true);
			m_ShadowmapDummy.filterMode = FilterMode.Point;
			m_ShadowmapDummy.SetPixel(0, 0, new Color(0f, 0f, 0f, 0f));
			m_ShadowmapDummy.Apply(false, true);
		}
	}

	private void InitFogLight()
	{
		if (!(m_FogLight != null))
		{
			m_FogLight = GetComponent<FogLight>();
		}
	}

	private bool InsideShadowmapCameraRender()
	{
		RenderTexture targetTexture = Camera.current.targetTexture;
		return targetTexture != null && targetTexture.format == RenderTextureFormat.Shadowmap;
	}

	private Vector4 GetZParams()
	{
		float nearToCenter = GetNearToCenter();
		float num = nearToCenter + m_Size.z;
		return new Vector4(nearToCenter / (nearToCenter - num), (nearToCenter + num) / (nearToCenter - num), 0f, 0f);
	}
}
