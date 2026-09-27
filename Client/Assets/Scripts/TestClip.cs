using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TestClip : MonoBehaviour
{
	public DecalProjector Decal;

	[Range(0f, 1f)] public float Clip;

	private void Awake()
	{
		Decal.material = Instantiate(Decal.material);
	}

	private void Update()
	{
		Decal.material.SetFloat("_ClipThreshold", Mathf.Clamp01(Clip));
	}
}
