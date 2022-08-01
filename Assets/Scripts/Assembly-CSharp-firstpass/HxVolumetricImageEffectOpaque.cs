using UnityEngine;

[ExecuteInEditMode]
public class HxVolumetricImageEffectOpaque : HxVolumetricRenderCallback
{
	private void OnEnable()
	{
		RenderOrder = HxVolumetricCamera.hxRenderOrder.ImageEffectOpaque;
		if (volumetricCamera == null)
		{
			volumetricCamera = GetComponent<HxVolumetricCamera>();
		}
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (volumetricCamera == null)
		{
			volumetricCamera = GetComponent<HxVolumetricCamera>();
		}
		if (volumetricCamera == null)
		{
			Graphics.Blit(src, dest);
		}
		else
		{
			volumetricCamera.EventOnRenderImage(src, dest);
		}
	}
}
