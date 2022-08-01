using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Playdead/VelocityBuffer")]
public class VelocityBuffer : EffectBase
{
	public enum NeighborMaxSupport
	{
		TileSize10 = 0,
		TileSize20 = 1,
		TileSize40 = 2
	}

	private const RenderTextureFormat velocityFormat = RenderTextureFormat.RGFloat;

	public Shader velocityShader;

	private Material velocityMaterial;

	private Matrix4x4? velocityViewMatrix;

	[NonSerialized]
	[HideInInspector]
	public RenderTexture velocityBuffer;

	[NonSerialized]
	[HideInInspector]
	public RenderTexture velocityNeighborMax;

	private float timeScaleNextFrame;

	public bool neighborMaxGen;

	public NeighborMaxSupport neighborMaxSupport = NeighborMaxSupport.TileSize20;

	private Camera _camera;

	public float timeScale { get; private set; }

	private void Awake()
	{
		_camera = GetComponent<Camera>();
	}

	private void Start()
	{
		timeScaleNextFrame = Time.timeScale;
	}

	private void OnPostRender()
	{
		EnsureMaterial(ref velocityMaterial, velocityShader);
		if (_camera.orthographic || _camera.depthTextureMode == DepthTextureMode.None || velocityMaterial == null)
		{
			if (_camera.depthTextureMode == DepthTextureMode.None)
			{
				_camera.depthTextureMode = DepthTextureMode.Depth;
			}
			return;
		}
		timeScale = timeScaleNextFrame;
		timeScaleNextFrame = ((Time.timeScale != 0f) ? Time.timeScale : timeScaleNextFrame);
		int pixelWidth = _camera.pixelWidth;
		int pixelHeight = _camera.pixelHeight;
		EnsureRenderTarget(ref velocityBuffer, pixelWidth, pixelHeight, RenderTextureFormat.RGFloat, FilterMode.Point, 16);
		EnsureKeyword(velocityMaterial, "TILESIZE_10", neighborMaxSupport == NeighborMaxSupport.TileSize10);
		EnsureKeyword(velocityMaterial, "TILESIZE_20", neighborMaxSupport == NeighborMaxSupport.TileSize20);
		EnsureKeyword(velocityMaterial, "TILESIZE_40", neighborMaxSupport == NeighborMaxSupport.TileSize40);
		Matrix4x4 projectionMatrix = _camera.projectionMatrix;
		Matrix4x4 worldToCameraMatrix = _camera.worldToCameraMatrix;
		Matrix4x4 value = projectionMatrix * worldToCameraMatrix;
		Matrix4x4? matrix4x = velocityViewMatrix;
		if (!matrix4x.HasValue)
		{
			velocityViewMatrix = worldToCameraMatrix;
		}
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = velocityBuffer;
		GL.Clear(true, true, Color.black);
		FrustumJitter component = GetComponent<FrustumJitter>();
		if (component != null)
		{
			velocityMaterial.SetVector("_Corner", _camera.GetPerspectiveProjectionCornerRay(component.activeSample.x, component.activeSample.y));
		}
		else
		{
			velocityMaterial.SetVector("_Corner", _camera.GetPerspectiveProjectionCornerRay());
		}
		velocityMaterial.SetMatrix("_CurrV", worldToCameraMatrix);
		velocityMaterial.SetMatrix("_CurrVP", value);
		velocityMaterial.SetMatrix("_PrevVP", projectionMatrix * velocityViewMatrix.Value);
		velocityMaterial.SetPass(0);
		FullScreenQuad();
		List<VelocityBufferTag> activeObjects = VelocityBufferTag.activeObjects;
		int i = 0;
		for (int count = activeObjects.Count; i != count; i++)
		{
			VelocityBufferTag velocityBufferTag = activeObjects[i];
			if (velocityBufferTag != null && !velocityBufferTag.sleeping && velocityBufferTag.mesh != null)
			{
				velocityMaterial.SetMatrix("_CurrM", velocityBufferTag.localToWorldCurr);
				velocityMaterial.SetMatrix("_PrevM", velocityBufferTag.localToWorldPrev);
				velocityMaterial.SetPass((!velocityBufferTag.useSkinnedMesh) ? 1 : 2);
				for (int j = 0; j != velocityBufferTag.mesh.subMeshCount; j++)
				{
					Graphics.DrawMeshNow(velocityBufferTag.mesh, Matrix4x4.identity, j);
				}
			}
		}
		if (neighborMaxGen)
		{
			int num = 1;
			switch (neighborMaxSupport)
			{
			case NeighborMaxSupport.TileSize10:
				num = 10;
				break;
			case NeighborMaxSupport.TileSize20:
				num = 20;
				break;
			case NeighborMaxSupport.TileSize40:
				num = 40;
				break;
			}
			int num2 = pixelWidth / num;
			int num3 = pixelHeight / num;
			EnsureRenderTarget(ref velocityNeighborMax, num2, num3, RenderTextureFormat.RGFloat, FilterMode.Bilinear);
			RenderTexture renderTexture = (RenderTexture.active = RenderTexture.GetTemporary(num2, num3, 0, RenderTextureFormat.RGFloat));
			velocityMaterial.SetTexture("_VelocityTex", velocityBuffer);
			velocityMaterial.SetVector("_VelocityTex_TexelSize", new Vector4(1f / (float)pixelWidth, 1f / (float)pixelHeight, 0f, 0f));
			velocityMaterial.SetPass(3);
			FullScreenQuad();
			RenderTexture.active = velocityNeighborMax;
			velocityMaterial.SetTexture("_VelocityTex", renderTexture);
			velocityMaterial.SetVector("_VelocityTex_TexelSize", new Vector4(1f / (float)num2, 1f / (float)num3, 0f, 0f));
			velocityMaterial.SetPass(4);
			FullScreenQuad();
			RenderTexture.ReleaseTemporary(renderTexture);
		}
		else if (velocityNeighborMax != null)
		{
			RenderTexture.ReleaseTemporary(velocityNeighborMax);
			velocityNeighborMax = null;
		}
		RenderTexture.active = active;
		velocityViewMatrix = worldToCameraMatrix;
	}
}
