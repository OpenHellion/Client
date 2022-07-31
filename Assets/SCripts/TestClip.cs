using ThreeEyedGames;
using UnityEngine;

public class TestClip : MonoBehaviour
{
	public Decalicious Decalicious;

	[Range(0f, 1f)]
	public float Clip;

	private void Awake()
	{
		Decalicious.Material = Object.Instantiate(Decalicious.Material);
	}

	private void Update()
	{
		Decalicious.Material.SetFloat("_MaskClip", Mathf.Clamp01(Clip));
	}
}
