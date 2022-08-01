using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

[ExecuteInEditMode]
public class MB3_AtlasPackerRenderTexture : MonoBehaviour
{
	private MB_TextureCombinerRenderTexture fastRenderer;

	private bool _doRenderAtlas;

	public int width;

	public int height;

	public int padding;

	public bool isNormalMap;

	public bool fixOutOfBoundsUVs;

	public bool considerNonTextureProperties;

	public TextureBlender resultMaterialTextureBlender;

	public Rect[] rects;

	public Texture2D tex1;

	public List<MB3_TextureCombiner.MB_TexSet> textureSets;

	public int indexOfTexSetToRender;

	public ShaderTextureProperty texPropertyName;

	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	public Texture2D testTex;

	public Material testMat;

	public Texture2D OnRenderAtlas(MB3_TextureCombiner combiner)
	{
		fastRenderer = new MB_TextureCombinerRenderTexture();
		_doRenderAtlas = true;
		Texture2D result = fastRenderer.DoRenderAtlas(base.gameObject, width, height, padding, rects, textureSets, indexOfTexSetToRender, texPropertyName, resultMaterialTextureBlender, isNormalMap, fixOutOfBoundsUVs, considerNonTextureProperties, combiner, LOG_LEVEL);
		_doRenderAtlas = false;
		return result;
	}

	private void OnRenderObject()
	{
		if (_doRenderAtlas)
		{
			fastRenderer.OnRenderObject();
			_doRenderAtlas = false;
		}
	}
}
