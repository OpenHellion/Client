using System.Collections.Generic;
using UnityEngine;

public class RepairPointParticleEffect : MonoBehaviour
{
	public ParticleEffectIntensity Effect;

	[Range(0f, 1f)] public float HealthThreshold = 0.01f;

	[Range(0f, 1f)] [ContextMenuItem("Test Intensity", "Test")] [ContextMenuItem("Test Gravity", "TestGravity")]
	public float Intensity = 1f;

	public bool PlayOnce;

	[Tooltip("Child particles on which to change gravity.")]
	public List<ParticleEffectIntensity> ChildParticles;

	public SoundEffect SoundEffect;

	private bool testGravity;

	public void Update()
	{
	}

	public void Test()
	{
		SetIntensity(Intensity);
	}

	public void TestGravity()
	{
		testGravity = !testGravity;
		SetGravity(testGravity);
	}

	public void Awake()
	{
		Effect.Particle = GetComponent<ParticleSystem>();
		Intensity = 0f;
	}

	public void SetIntensity(float intensity)
	{
		if (intensity < HealthThreshold)
		{
			Stop();
			return;
		}

		Play();
		if (SoundEffect != null)
		{
			SoundEffect.SetRTPCValue(SoundManager.Instance.RepairPointIntensity, intensity);
		}

		ParticleSystem.MainModule main = Effect.Particle.main;
		ParticleSystem.EmissionModule emission = Effect.Particle.emission;
		foreach (ParticleIntensityParameter intensityParameter in Effect.IntensityParameters)
		{
			if (intensityParameter.Parameter == ParticleIntensityParameter.ParameterType.Speed)
			{
				main.startSpeedMultiplier =
					Mathf.Lerp(intensityParameter.MinValue, intensityParameter.MaxValue, intensity);
			}

			if (intensityParameter.Parameter == ParticleIntensityParameter.ParameterType.Size)
			{
				main.startSizeMultiplier =
					Mathf.Lerp(intensityParameter.MinValue, intensityParameter.MaxValue, intensity);
			}

			if (intensityParameter.Parameter == ParticleIntensityParameter.ParameterType.Emission)
			{
				emission.rateOverTimeMultiplier =
					Mathf.Lerp(intensityParameter.MinValue, intensityParameter.MaxValue, intensity);
			}

			if (intensityParameter.Parameter == ParticleIntensityParameter.ParameterType.Life)
			{
				main.startLifetimeMultiplier =
					Mathf.Lerp(intensityParameter.MinValue, intensityParameter.MaxValue, intensity);
			}
		}

		foreach (ParticleEffectIntensity childParticle in ChildParticles)
		{
			ParticleSystem.MainModule main2 = childParticle.Particle.main;
			ParticleSystem.EmissionModule emission2 = childParticle.Particle.emission;
			foreach (ParticleIntensityParameter intensityParameter2 in childParticle.IntensityParameters)
			{
				if (intensityParameter2.Parameter == ParticleIntensityParameter.ParameterType.Speed)
				{
					main2.startSpeedMultiplier = Mathf.Lerp(intensityParameter2.MinValue, intensityParameter2.MaxValue,
						intensity);
				}

				if (intensityParameter2.Parameter == ParticleIntensityParameter.ParameterType.Size)
				{
					main2.startSizeMultiplier = Mathf.Lerp(intensityParameter2.MinValue, intensityParameter2.MaxValue,
						intensity);
				}

				if (intensityParameter2.Parameter == ParticleIntensityParameter.ParameterType.Emission)
				{
					emission2.rateOverTimeMultiplier = Mathf.Lerp(intensityParameter2.MinValue,
						intensityParameter2.MaxValue, intensity);
				}

				if (intensityParameter2.Parameter == ParticleIntensityParameter.ParameterType.Life)
				{
					main2.startLifetimeMultiplier = Mathf.Lerp(intensityParameter2.MinValue,
						intensityParameter2.MaxValue, intensity);
				}
			}
		}
	}

	public void SetGravity(bool gravity)
	{
		ParticleSystem.MainModule main = Effect.Particle.main;
		if (gravity)
		{
			main.gravityModifier = Effect.Gravity;
		}
		else
		{
			main.gravityModifier = 0f;
		}

		foreach (ParticleEffectIntensity childParticle in ChildParticles)
		{
			ParticleSystem.MainModule main2 = childParticle.Particle.main;
			if (gravity)
			{
				main2.gravityModifier = childParticle.Gravity;
			}
			else
			{
				main2.gravityModifier = 0f;
			}
		}
	}

	public void Play()
	{
		Effect.Particle.Play();
	}

	public void Stop()
	{
		if (Effect.Particle.isPlaying)
		{
			Effect.Particle.Stop();
		}
	}

	public bool IsPlaying()
	{
		return Effect.Particle.isPlaying;
	}
}
