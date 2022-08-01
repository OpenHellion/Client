using UnityEngine;

[ExecuteInEditMode]
public class CameraDepth : MonoBehaviour
{
	private Camera curCam;

	private void Start()
	{
	}

	private void Update()
	{
		if (!curCam)
		{
			curCam = GetComponent<Camera>();
		}
		else if (curCam.depthTextureMode == DepthTextureMode.None)
		{
			curCam.depthTextureMode = DepthTextureMode.Depth;
		}
	}
}
