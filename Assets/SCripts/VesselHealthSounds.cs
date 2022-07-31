using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VesselHealthSounds : MonoBehaviour
{
	public List<SoundEffect> DamageSounds;

	private void Awake()
	{
		if (DamageSounds.Count == 0)
		{
			DamageSounds = GetComponentsInChildren<SoundEffect>().ToList();
		}
	}

	public void PlaySounds()
	{
		foreach (SoundEffect damageSound in DamageSounds)
		{
			damageSound.Play(0, true);
		}
	}
}
