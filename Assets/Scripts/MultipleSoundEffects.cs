using System.Collections.Generic;
using UnityEngine;

public class MultipleSoundEffects : MonoBehaviour
{
	public List<AkAmbient> AmbientList;

	public GameObject SoundSourceObject;

	private void Awake()
	{
		if (SoundSourceObject == null)
		{
			SoundSourceObject = base.gameObject;
		}
	}

	public void PlaySound(int index)
	{
		if (AmbientList.Count > index)
		{
			AkSoundEngine.PostEvent((uint)AmbientList[index].eventID, SoundSourceObject);
		}
	}
}
