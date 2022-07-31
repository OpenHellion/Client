using UnityEngine;

public class DrillEffectScript : MonoBehaviour
{
	public Light effectLight;

	public ParticleSystem Sparks;

	public ParticleSystem Dust;

	[HideInInspector]
	public bool isEffectOn = true;

	private void Awake()
	{
		ToggleEffect(false);
	}

	public void ToggleEffect(bool? isActive)
	{
		if (!isActive.HasValue)
		{
			isActive = !isEffectOn;
		}
		if (isActive.GetValueOrDefault() != isEffectOn || !isActive.HasValue)
		{
			if (isActive.Value)
			{
				effectLight.enabled = true;
				Sparks.Play();
				Dust.Play();
			}
			else
			{
				effectLight.enabled = false;
				Sparks.Stop();
				Dust.Stop();
			}
			isEffectOn = isActive.Value;
		}
	}
}
