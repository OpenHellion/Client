using UnityEngine;

namespace ZeroGravity.Effects
{
	public class Stardust : MonoBehaviour
	{
		public Vector3 RelativeVelocity;

		[SerializeField]
		private ParticleSystem stardust;

		private void Update()
		{
			ParticleSystem.Particle[] array = new ParticleSystem.Particle[stardust.particleCount];
			int particles = stardust.GetParticles(array);
			if (RelativeVelocity.magnitude > 100f)
			{
				RelativeVelocity = RelativeVelocity.normalized * 100f;
			}
			for (int i = 0; i < particles; i++)
			{
				if (array[i].position.magnitude > 50f)
				{
					array[i].remainingLifetime = 0f;
				}
				array[i].velocity = RelativeVelocity;
			}
			stardust.SetParticles(array, particles);
		}
	}
}
