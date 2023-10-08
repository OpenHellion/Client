using UnityEngine;

[ExecuteInEditMode]
public class DebrisFieldEffect : MonoBehaviour
{
	[ContextMenuItem("MoveParticles", "MoveParticles")]
	public Vector3 Direction = Vector3.zero;

	public ParticleSystem Effect;

	public ParticleSystem SmallChunks;

	public SoundEffect DebrisSound;

	[Range(0f, 1f)] [SerializeField] private float density = 1f;

	[Range(0f, 1f)] [SerializeField] private float velocity = 1f;

	public float MinDensity = 1f;

	public float MaxDensity = 20f;

	public float MinVelocity = 50f;

	public float MaxVelocity = 200f;

	private void Awake()
	{
		ParticleSystemRenderer[] componentsInChildren = GetComponentsInChildren<ParticleSystemRenderer>();
		ParticleSystemRenderer[] array = componentsInChildren;
		foreach (ParticleSystemRenderer particleSystemRenderer in array)
		{
			particleSystemRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
		}
	}

	public void UpdateEffect(bool playStop, Vector3 direction, float dens = 1f, float vel = 1f)
	{
		if (playStop)
		{
			if (!Effect.isPlaying)
			{
				Effect.Play();
				density = 0f;
				velocity = 0f;
				DebrisSound.Play(0);
			}
		}
		else if (Effect.isPlaying && density < 0.1f && velocity < 0.1f)
		{
			Effect.Stop();
			density = 0f;
			velocity = 0f;
			DebrisSound.Play(1);
		}

		if (direction.magnitude != 0f)
		{
			base.transform.rotation = Quaternion.FromToRotation(Vector3.forward, direction);
		}

		density = Mathf.Lerp(density, dens, 0.05f);
		velocity = Mathf.Lerp(velocity, vel, 0.05f);
		ParticleSystem.MainModule main = Effect.main;
		ParticleSystem.EmissionModule emission = Effect.emission;
		main.startSpeedMultiplier = Mathf.Lerp(MinVelocity, MaxVelocity, velocity);
		emission.rateOverTimeMultiplier = Mathf.Lerp(MinDensity, MaxDensity, density);
		ParticleSystem.MainModule main2 = SmallChunks.main;
		ParticleSystem.EmissionModule emission2 = SmallChunks.emission;
		main2.startSpeedMultiplier = Mathf.Lerp(MinVelocity, MaxVelocity, velocity);
		emission2.rateOverTimeMultiplier = Mathf.Lerp(MinDensity, MaxDensity * 50f, density);
	}
}
