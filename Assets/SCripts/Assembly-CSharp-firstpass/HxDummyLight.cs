using UnityEngine;

[RequireComponent(typeof(HxVolumetricLight))]
public class HxDummyLight : MonoBehaviour
{
	public LightType type = LightType.Point;

	public float range = 10f;

	[Range(0f, 179f)]
	public float spotAngle = 40f;

	public Color color = Color.white;

	[Range(0f, 8f)]
	public float intensity = 1f;

	public Texture cookie;

	public void Update()
	{
	}
}
