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
		base.gameObject.GetComponent<Animator>().SetBool(Parameter, true);
	}

	public void PlayBoolFalse()
	{
		base.gameObject.GetComponent<Animator>().SetBool(Parameter, false);
	}

	public void PlayTrigger()
	{
		base.gameObject.GetComponent<Animator>().SetTrigger(Parameter);
	}

	public void Disable()
	{
		base.gameObject.SetActive(false);
	}

	public void DisableMeshRenderer()
	{
		base.gameObject.GetComponent<MeshRenderer>().enabled = false;
	}

	public void EnableMeshRenderer()
	{
		base.gameObject.GetComponent<MeshRenderer>().enabled = true;
	}

	public void DestroyObject()
	{
		Object.Destroy(base.gameObject);
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
