using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationHelper : MonoBehaviour
{
	[ContextMenuItem("True", "PlayBoolTrue")]
	[ContextMenuItem("False", "PlayBoolFalse")]
	[ContextMenuItem("Trigger", "PlayTrigger")]
	public string Parameter;

	public List<ParticleSystem> Particles;

	public List<SoundEffect> SoundEffects;

	public Animator Animator;

	public List<UnityEvent> Events;

	public void TriggerEvents()
	{
		foreach (UnityEvent @event in Events)
		{
			@event.Invoke();
		}
	}

	public void TriggerEventWithIndex(int index)
	{
		if (index >= 0 && index < Events.Count)
		{
			Events[index].Invoke();
		}
	}

	private void Awake()
	{
		Animator = GetComponent<Animator>();
	}

	public void PlayParticles(int id)
	{
		Particles[id].Play();
	}

	public void StopParticles(int id)
	{
		Particles[id].Stop();
	}

	public void PlayBoolTrue()
	{
		gameObject.GetComponent<Animator>().SetBool(Parameter, true);
	}

	public void PlayBoolFalse()
	{
		gameObject.GetComponent<Animator>().SetBool(Parameter, false);
	}

	public void PlayTrigger()
	{
		gameObject.GetComponent<Animator>().SetTrigger(Parameter);
	}

	public void Disable()
	{
		gameObject.SetActive(false);
	}

	public void DisableMeshRenderer()
	{
		gameObject.GetComponent<MeshRenderer>().enabled = false;
	}

	public void EnableMeshRenderer()
	{
		gameObject.GetComponent<MeshRenderer>().enabled = true;
	}

	public void DestroyObject()
	{
		Destroy(gameObject);
	}

	public void PlaySound(AnimationEvent ev)
	{
		if (!(ev.animatorClipInfo.weight > 0.5f))
		{
			return;
		}

		foreach (SoundEffect soundEffect in SoundEffects)
		{
			if (soundEffect != null && (!(ev.stringParameter == "DontPlayIfPlaying") || !soundEffect.IsPlaying))
			{
				soundEffect.Play(ev.intParameter);
			}
		}
	}

	public void OnParticleSystemStopped()
	{
		TriggerEvents();
	}
}
