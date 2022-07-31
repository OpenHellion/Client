using System;

[Serializable]
public class SoundEvent
{
	public int EventID;

	public string EventString = string.Empty;

	public AkGameObj Source;

	public bool PlayOnAwake;

	public bool UseStringInsteadOfId;

	public bool IsPlaying;

	public bool ShowRadius;

	public float AttenuationRadius;
}
