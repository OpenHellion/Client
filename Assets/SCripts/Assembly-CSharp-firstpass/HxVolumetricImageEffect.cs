using UnityEngine;

[ExecuteInEditMode]
public class HxVolumetricImageEffect : HxVolumetricRenderCallback
{
	private void OnEnable()
	{
		RenderOrder = HxVolumetricCamera.hxRenderOrder.ImageEffect;
		if (volumetricCamera == null)
		{
			volumetricCamera = GetComponent<HxVolumetricCamera>();
		}
	}

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
