using UnityEngine;

[ExecuteInEditMode]
public class Attractor : MonoBehaviour
{
	public ParticleSystem pSys;

	public float attraction = 2f;

	public bool worldSpaceParticles;

	private ParticleSystem.Particle[] m_Particles;

	private void LateUpdate()
	{
		if (pSys == null)
		{
			return;
		}

		InitializeIfNeeded();
		if (m_Particles.Length >= 1)
		{
			int particles = pSys.GetParticles(m_Particles);
			Vector3 position = base.transform.position;
			if (!worldSpaceParticles)
			{
				position -= pSys.transform.position;
			}

			for (int i = 0; i < particles; i++)
			{
				m_Particles[i].position =
					Vector3.MoveTowards(m_Particles[i].position, position, Time.deltaTime * attraction);
			}

			pSys.SetParticles(m_Particles, particles);
		}
	}

	private void InitializeIfNeeded()
	{
		if (m_Particles == null || m_Particles.Length < pSys.main.maxParticles)
		{
			m_Particles = new ParticleSystem.Particle[pSys.main.maxParticles];
		}
	}
}
