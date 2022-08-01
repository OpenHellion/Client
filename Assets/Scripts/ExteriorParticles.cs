using UnityEngine;

public class ExteriorParticles : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	public void UpdatePositionOnPivotReset(Vector3 positionCorrect)
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			ParticleSystem.Particle[] array = new ParticleSystem.Particle[particleSystem.main.maxParticles];
			int particles = particleSystem.GetParticles(array);
			for (int j = 0; j < particles; j++)
			{
				array[j].position += positionCorrect;
				particleSystem.SetParticles(array, particles);
			}
		}
	}
}
