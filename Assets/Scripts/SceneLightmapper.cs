using System.Collections.Generic;
using UnityEngine;

public class SceneLightmapper : MonoBehaviour
{
	public List<Texture2D> LightmapTextureList = new List<Texture2D>();

	public List<RendererLightmapParamteres> RendererList;

	private void Awake()
	{
		foreach (RendererLightmapParamteres renderer in RendererList)
		{
			if (!(renderer.renderer != null))
			{
				continue;
			}

			Material[] materials = renderer.renderer.materials;
			foreach (Material material in materials)
			{
				if (material.shader.name == "ZeroGravity/Surface/StandardLightMap" &&
				    renderer.LightmapIndex == LightmapTextureList.Count - 1)
				{
					material.SetTexture("_Lightmap", LightmapTextureList[renderer.LightmapIndex]);
					material.SetTextureScale("_Lightmap",
						new Vector2(renderer.LightmapParameters.x, renderer.LightmapParameters.y));
					material.SetTextureOffset("_Lightmap",
						new Vector2(renderer.LightmapParameters.z, renderer.LightmapParameters.w));
				}
			}
		}
	}

	private void Update()
	{
	}
}
