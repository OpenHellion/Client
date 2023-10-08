using UnityEngine;

public class Lens : MonoBehaviour
{
	public Shader shader;

	public float ratio = 1f;

	public float radius;

	public GameObject BH;

	private Material _material;

	protected Material material
	{
		get
		{
			if (_material == null)
			{
				_material = new Material(shader);
				_material.hideFlags = HideFlags.HideAndDontSave;
			}

			return _material;
		}
	}

	protected virtual void OnDisable()
	{
		if ((bool)_material)
		{
			Object.DestroyImmediate(_material);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if ((bool)shader && (bool)material && BH != null)
		{
			Vector3 vector = GetComponent<Camera>().WorldToScreenPoint(BH.transform.position);
			material.SetVector("_Position",
				new Vector2(vector.x / (float)GetComponent<Camera>().pixelWidth,
					vector.y / (float)GetComponent<Camera>().pixelHeight));
			material.SetFloat("_Ratio", ratio);
			material.SetFloat("_Rad", (!(vector.z > 0f)) ? 0f : radius);
			material.SetFloat("_Distance", Vector3.Distance(BH.transform.position, base.transform.position));
			Graphics.Blit(source, destination, material);
		}
	}
}
