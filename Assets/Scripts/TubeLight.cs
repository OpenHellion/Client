using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class TubeLight : MonoBehaviour
{
	public float m_Intensity = 0.8f;

	public Color m_Color = Color.white;

	public float m_Range = 10f;

	public float m_Radius = 0.3f;

	public float m_Length;

	[HideInInspector]
	public Mesh m_Sphere;

	[HideInInspector]
	public Mesh m_Capsule;

	[HideInInspector]
	public Shader m_ProxyShader;

	private Material m_ProxyMaterial;

	public bool m_RenderSource;

	private Renderer m_SourceRenderer;

	private Transform m_SourceTransform;

	private Mesh m_SourceMesh;

	private float m_LastLength = -1f;

	public const int maxPlanes = 2;

	[HideInInspector]
	public TubeLightShadowPlane[] m_ShadowPlanes = new TubeLightShadowPlane[2];

	private bool m_Initialized;

	private MaterialPropertyBlock m_props;

	private const float kMinRadius = 0.001f;

	private Dictionary<Camera, CommandBuffer> m_Cameras = new Dictionary<Camera, CommandBuffer>();

	private static CameraEvent kCameraEvent = CameraEvent.AfterLighting;

	private TubeLightShadowPlane.Params[] sppArr = new TubeLightShadowPlane.Params[2];

	private bool renderSource
	{
		get
		{
			return m_RenderSource && m_Radius >= 0.001f;
		}
	}

	private void Start()
	{
		if (Init())
		{
			UpdateMeshesAndBounds();
		}
	}

	private bool Init()
	{
		if (m_Initialized)
		{
			return true;
		}
		if (m_ProxyShader == null || m_Sphere == null || m_Capsule == null)
		{
			return false;
		}
		m_ProxyMaterial = new Material(m_ProxyShader);
		m_ProxyMaterial.hideFlags = HideFlags.HideAndDontSave;
		m_SourceMesh = Object.Instantiate(m_Capsule);
		m_SourceMesh.hideFlags = HideFlags.HideAndDontSave;
		MeshFilter component = base.gameObject.GetComponent<MeshFilter>();
		component.sharedMesh = m_SourceMesh;
		m_SourceRenderer = base.gameObject.GetComponent<MeshRenderer>();
		m_SourceRenderer.enabled = true;
		m_SourceTransform = base.transform;
		m_Initialized = true;
		return true;
	}

	private void OnEnable()
	{
		if (m_props == null)
		{
			m_props = new MaterialPropertyBlock();
		}
		if (Init())
		{
			UpdateMeshesAndBounds();
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
			Object.DestroyImmediate(m_ProxyMaterial);
		}
		if (m_SourceMesh != null)
		{
			Object.DestroyImmediate(m_SourceMesh);
		}
		Cleanup();
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

	private void UpdateMeshesAndBounds()
	{
		m_Range = Mathf.Max(m_Range, 0f);
		m_Radius = Mathf.Max(m_Radius, 0f);
		m_Length = Mathf.Max(m_Length, 0f);
		m_Intensity = Mathf.Max(m_Intensity, 0f);
		Vector3 vector = ((!renderSource) ? Vector3.one : (Vector3.one * m_Radius * 2f));
		if (m_SourceTransform.localScale != vector || m_Length != m_LastLength)
		{
			m_LastLength = m_Length;
			Vector3[] vertices = m_Capsule.vertices;
			for (int i = 0; i < vertices.Length; i++)
			{
				if (renderSource)
				{
					vertices[i].y += Mathf.Sign(vertices[i].y) * (-0.5f + 0.25f * m_Length / m_Radius);
				}
				else
				{
					vertices[i] = Vector3.one * 0.0001f;
				}
			}
			m_SourceMesh.vertices = vertices;
		}
		m_SourceTransform.localScale = vector;
		float num = m_Range + m_Radius;
		num += 0.5f * m_Length;
		num *= 1.02f;
		num /= m_Radius;
		m_SourceMesh.bounds = new Bounds(Vector3.zero, Vector3.one * num);
	}

	private void Update()
	{
		if (!Init())
		{
			return;
		}
		UpdateMeshesAndBounds();
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

	private Color GetColor()
	{
		if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
		{
			return m_Color * m_Intensity;
		}
		return new Color(Mathf.GammaToLinearSpace(m_Color.r * m_Intensity), Mathf.GammaToLinearSpace(m_Color.g * m_Intensity), Mathf.GammaToLinearSpace(m_Color.b * m_Intensity), 1f);
	}

	private void OnWillRenderObject()
	{
		if (!InsideShadowmapCameraRender() && Init())
		{
			m_props.SetVector("_EmissionColor", m_Color * Mathf.Sqrt(m_Intensity) * 2f);
			m_SourceRenderer.SetPropertyBlock(m_props);
			SetUpCommandBuffer();
		}
	}

	private void SetUpCommandBuffer()
	{
		Camera current = Camera.current;
		CommandBuffer commandBuffer = null;
		if (!m_Cameras.ContainsKey(current))
		{
			commandBuffer = new CommandBuffer();
			commandBuffer.name = base.gameObject.name;
			m_Cameras[current] = commandBuffer;
			current.AddCommandBuffer(kCameraEvent, commandBuffer);
			current.depthTextureMode |= DepthTextureMode.Depth;
		}
		else
		{
			commandBuffer = m_Cameras[current];
			commandBuffer.Clear();
		}
		Transform transform = base.transform;
		Vector3 up = transform.up;
		Vector3 vector = transform.position - 0.5f * up * m_Length;
		commandBuffer.SetGlobalVector("_LightPos", new Vector4(vector.x, vector.y, vector.z, 1f / (m_Range * m_Range)));
		commandBuffer.SetGlobalVector("_LightAxis", new Vector4(up.x, up.y, up.z, 0f));
		commandBuffer.SetGlobalFloat("_LightAsQuad", 0f);
		commandBuffer.SetGlobalFloat("_LightRadius", m_Radius);
		commandBuffer.SetGlobalFloat("_LightLength", m_Length);
		commandBuffer.SetGlobalVector("_LightColor", GetColor());
		SetShadowPlaneVectors(commandBuffer);
		float num = m_Range + m_Radius;
		num += 0.5f * m_Length;
		num *= 1.02f;
		num /= m_Radius;
		Matrix4x4 matrix4x = Matrix4x4.Scale(Vector3.one * num);
		commandBuffer.DrawMesh(m_Sphere, transform.localToWorldMatrix * matrix4x, m_ProxyMaterial, 0, 0);
	}

	public TubeLightShadowPlane.Params[] GetShadowPlaneParams(ref TubeLightShadowPlane.Params[] p)
	{
		if (p == null || p.Length != 2)
		{
			p = new TubeLightShadowPlane.Params[2];
		}
		for (int i = 0; i < 2; i++)
		{
			TubeLightShadowPlane tubeLightShadowPlane = m_ShadowPlanes[i];
			p[i].plane = ((!(tubeLightShadowPlane == null)) ? tubeLightShadowPlane.GetShadowPlaneVector() : new Vector4(0f, 0f, 0f, 1f));
			p[i].feather = ((!(tubeLightShadowPlane == null)) ? tubeLightShadowPlane.feather : 1f);
		}
		return p;
	}

	private void SetShadowPlaneVectors(CommandBuffer buf)
	{
		TubeLightShadowPlane.Params[] shadowPlaneParams = GetShadowPlaneParams(ref sppArr);
		int i = 0;
		for (int num = shadowPlaneParams.Length; i < num; i++)
		{
			TubeLightShadowPlane.Params @params = shadowPlaneParams[i];
			switch (i)
			{
			case 0:
				buf.SetGlobalVector("_ShadowPlane0", @params.plane);
				buf.SetGlobalFloat("_ShadowPlaneFeather0", @params.feather);
				break;
			case 1:
				buf.SetGlobalVector("_ShadowPlane1", @params.plane);
				buf.SetGlobalFloat("_ShadowPlaneFeather1", @params.feather);
				break;
			default:
				buf.SetGlobalVector("_ShadowPlane" + i, @params.plane);
				buf.SetGlobalFloat("_ShadowPlaneFeather" + i, @params.feather);
				break;
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!(m_SourceTransform == null))
		{
			Gizmos.color = Color.white;
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(m_SourceTransform.position, m_SourceTransform.rotation, Vector3.one);
			Gizmos.matrix = matrix;
			Gizmos.DrawWireSphere(Vector3.zero, m_Radius + m_Range + 0.5f * m_Length);
			Vector3 vector = 0.5f * Vector3.up * m_Length;
			Gizmos.DrawWireSphere(vector, m_Radius);
			if (m_Length != 0f)
			{
				Vector3 vector2 = -0.5f * Vector3.up * m_Length;
				Gizmos.DrawWireSphere(vector2, m_Radius);
				Vector3 vector3 = Vector3.forward * m_Radius;
				Gizmos.DrawLine(vector + vector3, vector2 + vector3);
				Gizmos.DrawLine(vector - vector3, vector2 - vector3);
				vector3 = Vector3.right * m_Radius;
				Gizmos.DrawLine(vector + vector3, vector2 + vector3);
				Gizmos.DrawLine(vector - vector3, vector2 - vector3);
			}
		}
	}

	private void OnDrawGizmos()
	{
	}

	private bool InsideShadowmapCameraRender()
	{
		RenderTexture targetTexture = Camera.current.targetTexture;
		return targetTexture != null && targetTexture.format == RenderTextureFormat.Shadowmap;
	}
}
