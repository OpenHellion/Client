using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(FrustumJitter))]
[RequireComponent(typeof(VelocityBuffer))]
[AddComponentMenu("Playdead/TemporalReprojection")]
public class TemporalReprojection : EffectBase
{
	public enum Neighborhood
	{
		MinMax3x3 = 0,
		MinMax3x3Rounded = 1,
		MinMax4TapVarying = 2
	}

	private static RenderBuffer[] mrt = new RenderBuffer[2];

	public Shader reprojectionShader;

	private Material reprojectionMaterial;

	private Matrix4x4[] reprojectionMatrix;

	private RenderTexture[] reprojectionBuffer;

	private int reprojectionIndex;

	public Neighborhood neighborhood = Neighborhood.MinMax3x3Rounded;

	public bool unjitterColorSamples = true;

	public bool unjitterNeighborhood;

	public bool unjitterReprojection;

	public bool useYCoCg;

	public bool useClipping = true;

	public bool useDilation = true;

	public bool useMotionBlur = true;

	public bool useOptimizations = true;

	[Range(0f, 1f)]
	public float feedbackMin = 0.88f;

	[Range(0f, 1f)]
	public float feedbackMax = 0.97f;

	public float motionBlurStrength = 1f;

	public bool motionBlurIgnoreFF;

	private Camera _camera;

	private FrustumJitter _jitter;

	private VelocityBuffer _velocity;

	private void Awake()
	{
		_camera = GetComponent<Camera>();
		_jitter = GetComponent<FrustumJitter>();
		_velocity = GetComponent<VelocityBuffer>();
	}

	private void Resolve(RenderTexture source, RenderTexture destination)
	{
		EnsureMaterial(ref reprojectionMaterial, reprojectionShader);
		if (_camera.orthographic || _camera.depthTextureMode == DepthTextureMode.None || reprojectionMaterial == null)
		{
			Graphics.Blit(source, destination);
			if (_camera.depthTextureMode == DepthTextureMode.None)
			{
				_camera.depthTextureMode = DepthTextureMode.Depth;
			}
			return;
		}
		if (reprojectionMatrix == null || reprojectionMatrix.Length != 2)
		{
			reprojectionMatrix = new Matrix4x4[2];
		}
		if (reprojectionBuffer == null || reprojectionBuffer.Length != 2)
		{
			reprojectionBuffer = new RenderTexture[2];
		}
		int width = source.width;
		int height = source.height;
		EnsureRenderTarget(ref reprojectionBuffer[0], width, height, RenderTextureFormat.ARGB32, FilterMode.Bilinear);
		EnsureRenderTarget(ref reprojectionBuffer[1], width, height, RenderTextureFormat.ARGB32, FilterMode.Bilinear);
		EnsureKeyword(reprojectionMaterial, "MINMAX_3X3", neighborhood == Neighborhood.MinMax3x3);
		EnsureKeyword(reprojectionMaterial, "MINMAX_3X3_ROUNDED", neighborhood == Neighborhood.MinMax3x3Rounded);
		EnsureKeyword(reprojectionMaterial, "MINMAX_4TAP_VARYING", neighborhood == Neighborhood.MinMax4TapVarying);
		EnsureKeyword(reprojectionMaterial, "UNJITTER_COLORSAMPLES", unjitterColorSamples);
		EnsureKeyword(reprojectionMaterial, "UNJITTER_NEIGHBORHOOD", unjitterNeighborhood);
		EnsureKeyword(reprojectionMaterial, "UNJITTER_REPROJECTION", unjitterReprojection);
		EnsureKeyword(reprojectionMaterial, "USE_YCOCG", useYCoCg);
		EnsureKeyword(reprojectionMaterial, "USE_CLIPPING", useClipping);
		EnsureKeyword(reprojectionMaterial, "USE_DILATION", useDilation);
		EnsureKeyword(reprojectionMaterial, "USE_MOTION_BLUR", useMotionBlur);
		EnsureKeyword(reprojectionMaterial, "USE_MOTION_BLUR_NEIGHBORMAX", _velocity.velocityNeighborMax != null);
		EnsureKeyword(reprojectionMaterial, "USE_OPTIMIZATIONS", useOptimizations);
		Matrix4x4 perspectiveProjection = _camera.GetPerspectiveProjection();
		Matrix4x4 matrix4x = perspectiveProjection * _camera.worldToCameraMatrix;
		float num = Mathf.Tan((float)Math.PI / 360f * _camera.fieldOfView);
		float x = num * _camera.aspect;
		if (reprojectionIndex == -1)
		{
			reprojectionIndex = 0;
			reprojectionMatrix[reprojectionIndex] = matrix4x;
			Graphics.Blit(source, reprojectionBuffer[reprojectionIndex]);
		}
		int num2 = reprojectionIndex;
		int num3 = (reprojectionIndex + 1) % 2;
		reprojectionMaterial.SetTexture("_VelocityBuffer", _velocity.velocityBuffer);
		reprojectionMaterial.SetTexture("_VelocityNeighborMax", _velocity.velocityNeighborMax);
		reprojectionMaterial.SetVector("_Corner", new Vector4(x, num, 0f, 0f));
		reprojectionMaterial.SetVector("_Jitter", _jitter.activeSample);
		reprojectionMaterial.SetMatrix("_PrevVP", reprojectionMatrix[num2]);
		reprojectionMaterial.SetTexture("_MainTex", source);
		reprojectionMaterial.SetTexture("_PrevTex", reprojectionBuffer[num2]);
		reprojectionMaterial.SetFloat("_FeedbackMin", feedbackMin);
		reprojectionMaterial.SetFloat("_FeedbackMax", feedbackMax);
		reprojectionMaterial.SetFloat("_MotionScale", motionBlurStrength * ((!motionBlurIgnoreFF) ? 1f : Mathf.Min(1f, 1f / _velocity.timeScale)));
		mrt[0] = reprojectionBuffer[num3].colorBuffer;
		mrt[1] = destination.colorBuffer;
		Graphics.SetRenderTarget(mrt, source.depthBuffer);
		reprojectionMaterial.SetPass(0);
		FullScreenQuad();
		reprojectionMatrix[num3] = matrix4x;
		reprojectionIndex = num3;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (destination != null)
		{
			Resolve(source, destination);
			return;
		}
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
		Resolve(source, temporary);
		Graphics.Blit(temporary, destination);
		RenderTexture.ReleaseTemporary(temporary);
	}
}
