using UnityEngine;

public class MapDebrisFieldEffect : MonoBehaviour
{
	public ParticleSystem Mist;

	public ParticleSystem Debris;

	public float Radius = 1f;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetRadius(float radius, float semiMinorAxis)
	{
		ParticleSystem.MainModule main = Mist.main;
		ParticleSystem.MainModule main2 = Debris.main;
		main.startSizeMultiplier = 0.5f * radius / semiMinorAxis * 2f;
		main2.startSizeMultiplier = 0.5f * radius / semiMinorAxis * 0.05f;
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(base.transform.position, Radius);
	}
}
