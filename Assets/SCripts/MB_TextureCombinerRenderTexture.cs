using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB_TextureCombinerRenderTexture
{
	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	private Material mat;

	private RenderTexture _destinationTexture;

	private Camera myCamera;

	private int _padding;

	private bool _isNormalMap;

	private bool _fixOutOfBoundsUVs;

	private bool _doRenderAtlas;

	private Rect[] rs;

	private List<MB3_TextureCombiner.MB_TexSet> textureSets;

	private int indexOfTexSetToRender;

	private ShaderTextureProperty _texPropertyName;

	private TextureBlender _resultMaterialTextureBlender;

	private Texture2D targTex;

	private MB3_TextureCombiner combiner;

	public Texture2D DoRenderAtlas(GameObject gameObject, int width, int height, int padding, Rect[] rss, List<MB3_TextureCombiner.MB_TexSet> textureSetss, int indexOfTexSetToRenders, ShaderTextureProperty texPropertyname, TextureBlender resultMaterialTextureBlender, bool isNormalMap, bool fixOutOfBoundsUVs, bool considerNonTextureProperties, MB3_TextureCombiner texCombiner, MB2_LogLevel LOG_LEV)
	{
		LOG_LEVEL = LOG_LEV;
		textureSets = textureSetss;
		indexOfTexSetToRender = indexOfTexSetToRenders;
		_texPropertyName = texPropertyname;
		_padding = padding;
		_isNormalMap = isNormalMap;
		_fixOutOfBoundsUVs = fixOutOfBoundsUVs;
		_resultMaterialTextureBlender = resultMaterialTextureBlender;
		combiner = texCombiner;
		rs = rss;
		Shader shader = ((!_isNormalMap) ? Shader.Find("MeshBaker/AlbedoShader") : Shader.Find("MeshBaker/NormalMapShader"));
		if (shader == null)
		{
			UnityEngine.Debug.LogError("Could not find shader for RenderTexture. Try reimporting mesh baker");
			return null;
		}
		mat = new Material(shader);
		_destinationTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
		_destinationTexture.filterMode = FilterMode.Point;
		myCamera = gameObject.GetComponent<Camera>();
		myCamera.orthographic = true;
		myCamera.orthographicSize = height >> 1;
		myCamera.aspect = width / height;
		myCamera.targetTexture = _destinationTexture;
		myCamera.clearFlags = CameraClearFlags.Color;
		Transform component = myCamera.GetComponent<Transform>();
		component.localPosition = new Vector3((float)width / 2f, (float)height / 2f, 3f);
		component.localRotation = Quaternion.Euler(0f, 180f, 180f);
		_doRenderAtlas = true;
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log(string.Format("Begin Camera.Render destTex w={0} h={1} camPos={2}", width, height, component.localPosition));
		}
		myCamera.Render();
		_doRenderAtlas = false;
		MB_Utility.Destroy(mat);
		MB_Utility.Destroy(_destinationTexture);
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("Finished Camera.Render ");
		}
		Texture2D result = targTex;
		targTex = null;
		return result;
	}

	public void OnRenderObject()
	{
		if (!_doRenderAtlas)
		{
			return;
		}
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		for (int i = 0; i < rs.Length; i++)
		{
			MB3_TextureCombiner.MeshBakerMaterialTexture meshBakerMaterialTexture = textureSets[i].ts[indexOfTexSetToRender];
			if (LOG_LEVEL >= MB2_LogLevel.trace && meshBakerMaterialTexture.t != null)
			{
				UnityEngine.Debug.Log(string.Concat("Added ", meshBakerMaterialTexture.t, " to atlas w=", meshBakerMaterialTexture.t.width, " h=", meshBakerMaterialTexture.t.height, " offset=", meshBakerMaterialTexture.matTilingRect.min, " scale=", meshBakerMaterialTexture.matTilingRect.size, " rect=", rs[i], " padding=", _padding));
				_printTexture(meshBakerMaterialTexture.t);
			}
			CopyScaledAndTiledToAtlas(textureSets[i], meshBakerMaterialTexture, textureSets[i].obUVoffset, textureSets[i].obUVscale, rs[i], _texPropertyName, _resultMaterialTextureBlender);
		}
		stopwatch.Stop();
		stopwatch.Start();
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("Total time for Graphics.DrawTexture calls " + stopwatch.ElapsedMilliseconds.ToString("f5"));
		}
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("Copying RenderTexture to Texture2D.");
		}
		Texture2D texture2D = new Texture2D(_destinationTexture.width, _destinationTexture.height, TextureFormat.ARGB32, true);
		int num = _destinationTexture.width / 512;
		int num2 = _destinationTexture.height / 512;
		if (num == 0 || num2 == 0)
		{
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log("Copying all in one shot");
			}
			RenderTexture.active = _destinationTexture;
			texture2D.ReadPixels(new Rect(0f, 0f, _destinationTexture.width, _destinationTexture.height), 0, 0, true);
			RenderTexture.active = null;
		}
		else if (IsOpenGL())
		{
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log("OpenGL copying blocks");
			}
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < num2; k++)
				{
					RenderTexture.active = _destinationTexture;
					texture2D.ReadPixels(new Rect(j * 512, k * 512, 512f, 512f), j * 512, k * 512, true);
					RenderTexture.active = null;
				}
			}
		}
		else
		{
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log("Not OpenGL copying blocks");
			}
			for (int l = 0; l < num; l++)
			{
				for (int m = 0; m < num2; m++)
				{
					RenderTexture.active = _destinationTexture;
					texture2D.ReadPixels(new Rect(l * 512, _destinationTexture.height - 512 - m * 512, 512f, 512f), l * 512, m * 512, true);
					RenderTexture.active = null;
				}
			}
		}
		texture2D.Apply();
		myCamera.targetTexture = null;
		RenderTexture.active = null;
		targTex = texture2D;
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("Total time to copy RenderTexture to Texture2D " + stopwatch.ElapsedMilliseconds.ToString("f5"));
		}
	}

	private Color32 ConvertNormalFormatFromUnity_ToStandard(Color32 c)
	{
		Vector3 zero = Vector3.zero;
		zero.x = (float)(int)c.a * 2f - 1f;
		zero.y = (float)(int)c.g * 2f - 1f;
		zero.z = Mathf.Sqrt(1f - zero.x * zero.x - zero.y * zero.y);
		Color32 result = default(Color32);
		result.a = 1;
		result.r = (byte)((zero.x + 1f) * 0.5f);
		result.g = (byte)((zero.y + 1f) * 0.5f);
		result.b = (byte)((zero.z + 1f) * 0.5f);
		return result;
	}

	private bool IsOpenGL()
	{
		string graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion;
		return graphicsDeviceVersion.StartsWith("OpenGL");
	}

	private void CopyScaledAndTiledToAtlas(MB3_TextureCombiner.MB_TexSet texSet, MB3_TextureCombiner.MeshBakerMaterialTexture source, Vector2 obUVoffset, Vector2 obUVscale, Rect rec, ShaderTextureProperty texturePropertyName, TextureBlender resultMatTexBlender)
	{
		Rect rect = rec;
		if (resultMatTexBlender != null)
		{
			myCamera.backgroundColor = resultMatTexBlender.GetColorIfNoTexture(texSet.mats[0].mat, texturePropertyName);
		}
		else
		{
			myCamera.backgroundColor = MB3_TextureCombiner.GetColorIfNoTexture(texturePropertyName);
		}
		if (source.t == null)
		{
			source.t = combiner._createTemporaryTexture(16, 16, TextureFormat.ARGB32, true);
		}
		rect.y = 1f - (rect.y + rect.height);
		rect.x *= _destinationTexture.width;
		rect.y *= _destinationTexture.height;
		rect.width *= _destinationTexture.width;
		rect.height *= _destinationTexture.height;
		Rect rect2 = rect;
		rect2.x -= _padding;
		rect2.y -= _padding;
		rect2.width += _padding * 2;
		rect2.height += _padding * 2;
		Rect r = source.matTilingRect.GetRect();
		Rect screenRect = default(Rect);
		if (_fixOutOfBoundsUVs)
		{
			Rect r2 = new Rect(obUVoffset.x, obUVoffset.y, obUVscale.x, obUVscale.y);
			r = MB3_UVTransformUtility.CombineTransforms(ref r, ref r2);
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log("Fixing out of bounds UVs for tex " + source.t);
			}
		}
		Texture2D t = source.t;
		TextureWrapMode wrapMode = t.wrapMode;
		if (r.width == 1f && r.height == 1f && r.x == 0f && r.y == 0f)
		{
			t.wrapMode = TextureWrapMode.Clamp;
		}
		else
		{
			t.wrapMode = TextureWrapMode.Repeat;
		}
		if (LOG_LEVEL >= MB2_LogLevel.trace)
		{
			UnityEngine.Debug.Log(string.Concat("DrawTexture tex=", t.name, " destRect=", rect, " srcRect=", r, " Mat=", mat));
		}
		Rect sourceRect = default(Rect);
		sourceRect.x = r.x;
		sourceRect.y = r.y + 1f - 1f / (float)t.height;
		sourceRect.width = r.width;
		sourceRect.height = 1f / (float)t.height;
		screenRect.x = rect.x;
		screenRect.y = rect2.y;
		screenRect.width = rect.width;
		screenRect.height = _padding;
		Graphics.DrawTexture(screenRect, t, sourceRect, 0, 0, 0, 0, mat);
		sourceRect.x = r.x;
		sourceRect.y = r.y;
		sourceRect.width = r.width;
		sourceRect.height = 1f / (float)t.height;
		screenRect.x = rect.x;
		screenRect.y = rect.y + rect.height;
		screenRect.width = rect.width;
		screenRect.height = _padding;
		Graphics.DrawTexture(screenRect, t, sourceRect, 0, 0, 0, 0, mat);
		sourceRect.x = r.x;
		sourceRect.y = r.y;
		sourceRect.width = 1f / (float)t.width;
		sourceRect.height = r.height;
		screenRect.x = rect2.x;
		screenRect.y = rect.y;
		screenRect.width = _padding;
		screenRect.height = rect.height;
		Graphics.DrawTexture(screenRect, t, sourceRect, 0, 0, 0, 0, mat);
		sourceRect.x = r.x + 1f - 1f / (float)t.width;
		sourceRect.y = r.y;
		sourceRect.width = 1f / (float)t.width;
		sourceRect.height = r.height;
		screenRect.x = rect.x + rect.width;
		screenRect.y = rect.y;
		screenRect.width = _padding;
		screenRect.height = rect.height;
		Graphics.DrawTexture(screenRect, t, sourceRect, 0, 0, 0, 0, mat);
		sourceRect.x = r.x;
		sourceRect.y = r.y + 1f - 1f / (float)t.height;
		sourceRect.width = 1f / (float)t.width;
		sourceRect.height = 1f / (float)t.height;
		screenRect.x = rect2.x;
		screenRect.y = rect2.y;
		screenRect.width = _padding;
		screenRect.height = _padding;
		Graphics.DrawTexture(screenRect, t, sourceRect, 0, 0, 0, 0, mat);
		sourceRect.x = r.x + 1f - 1f / (float)t.width;
		sourceRect.y = r.y + 1f - 1f / (float)t.height;
		sourceRect.width = 1f / (float)t.width;
		sourceRect.height = 1f / (float)t.height;
		screenRect.x = rect.x + rect.width;
		screenRect.y = rect2.y;
		screenRect.width = _padding;
		screenRect.height = _padding;
		Graphics.DrawTexture(screenRect, t, sourceRect, 0, 0, 0, 0, mat);
		sourceRect.x = r.x;
		sourceRect.y = r.y;
		sourceRect.width = 1f / (float)t.width;
		sourceRect.height = 1f / (float)t.height;
		screenRect.x = rect2.x;
		screenRect.y = rect.y + rect.height;
		screenRect.width = _padding;
		screenRect.height = _padding;
		Graphics.DrawTexture(screenRect, t, sourceRect, 0, 0, 0, 0, mat);
		sourceRect.x = r.x + 1f - 1f / (float)t.width;
		sourceRect.y = r.y;
		sourceRect.width = 1f / (float)t.width;
		sourceRect.height = 1f / (float)t.height;
		screenRect.x = rect.x + rect.width;
		screenRect.y = rect.y + rect.height;
		screenRect.width = _padding;
		screenRect.height = _padding;
		Graphics.DrawTexture(screenRect, t, sourceRect, 0, 0, 0, 0, mat);
		Graphics.DrawTexture(rect, t, r, 0, 0, 0, 0, mat);
		t.wrapMode = wrapMode;
	}

	private void _printTexture(Texture2D t)
	{
		if (t.width * t.height > 100)
		{
			UnityEngine.Debug.Log("Not printing texture too large.");
		}
		try
		{
			Color32[] pixels = t.GetPixels32();
			string text = string.Empty;
			for (int i = 0; i < t.height; i++)
			{
				for (int j = 0; j < t.width; j++)
				{
					text = string.Concat(text, pixels[i * t.width + j], ", ");
				}
				text += "\n";
			}
			UnityEngine.Debug.Log(text);
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.Log("Could not print texture. texture may not be readable." + ex.ToString());
		}
	}
}
