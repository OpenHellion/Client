using UnityEngine;

public class SoundManager : MonoBehaviour
{
	public static SoundManager Instance;

	public string MasterVolume;

	public string AmbienceVolume;

	public string EffectsVolume;

	public string HelmetOn;

	public string ImpactVelocity;

	public string Pressure;

	public string Health;

	public string RepairPointIntensity;

	public string InGameVolume;

	public string SpaceAmbience;

	public string SpaceEnvironment;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}
}
