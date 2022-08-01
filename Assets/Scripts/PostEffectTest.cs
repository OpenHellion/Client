using UnityEngine;

[ExecuteInEditMode]
public class PostEffectTest : MonoBehaviour
{
	public Material mat;

	public GameObject sun;

	public Vector3 screenPos;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		screenPos = GetComponent<Camera>().WorldToScreenPoint(sun.transform.position);
		mat.SetFloat("_sunX", screenPos.x);
		mat.SetFloat("_sunY", screenPos.y);
		mat.SetFloat("_sunZ", screenPos.z);
		Graphics.Blit(source, destination, mat);
	}
}
