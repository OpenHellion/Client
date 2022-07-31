using System.Collections.Generic;
using UnityEngine;

public class EngineThrusters : MonoBehaviour
{
	public List<Light> Lights;

	public List<ParticleSystem> Particles;

	public List<GameObject> Flares;

	public float LerpValue;

	[SerializeField]
	private bool onOff;

	public float Speed = 1f;

	public bool OnOff
	{
		get
		{
			return onOff;
		}
		set
		{
			onOff = value;
			if (value)
			{
				base.gameObject.SetActive(true);
			}
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (LerpValue < 1f && onOff)
		{
			LerpValue += Time.deltaTime * Speed;
			foreach (Light light in Lights)
			{
				light.intensity = LerpValue * 8f;
			}
			foreach (GameObject flare in Flares)
			{
				flare.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_GlobalIntensity", LerpValue);
			}
			foreach (ParticleSystem particle in Particles)
			{
				if (!particle.isPlaying)
				{
					particle.Play();
				}
			}
		}
		if (!(LerpValue > 0f) || onOff)
		{
			return;
		}
		LerpValue -= Time.deltaTime * Speed;
		foreach (Light light2 in Lights)
		{
			light2.intensity = LerpValue * 8f;
		}
		foreach (GameObject flare2 in Flares)
		{
			flare2.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_GlobalIntensity", LerpValue);
		}
		foreach (ParticleSystem particle2 in Particles)
		{
			if (particle2.isPlaying)
			{
				particle2.Stop();
			}
		}
		if (LerpValue < 0f)
		{
			base.gameObject.SetActive(false);
		}
	}
}
