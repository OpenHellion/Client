using UnityEngine;

public class RepairPointMeshEffect : MonoBehaviour
{
	public MeshRenderer Mesh;

	public AnimationCurve Curve;

	[Range(0f, 1f)]
	public float Intensity;

	private void Awake()
	{
		Mesh = GetComponent<MeshRenderer>();
	}

	public void Update()
	{
		if (!Application.isPlaying)
		{
			Mesh.sharedMaterial.SetFloat("_Health", Curve.Evaluate(Intensity));
		}
	}
}
