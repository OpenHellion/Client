using UnityEngine;
using ZeroGravity.Objects;

public class ImpactDetector : MonoBehaviour
{
	public SoundEffect ImpactSound;

	public float VelocityThrashold = 0.4f;

	public float CooldownTime = 0.3f;

	private float lastImpactTime;

	private SpaceObjectTransferable refObject;

	private void Awake()
	{
		refObject = GetComponentInParent<SpaceObjectTransferable>();
	}

	private void OnCollisionEnter(Collision collision)
	{
		float magnitude = collision.relativeVelocity.magnitude;
		if (ImpactSound != null && magnitude > VelocityThrashold && Time.time - lastImpactTime > CooldownTime && (!(refObject is MyPlayer) || !(refObject as MyPlayer).FpsController.IsGrounded))
		{
			lastImpactTime = Time.time;
			PlayImpactSound(magnitude);
			if (refObject != null && (!(refObject is DynamicObject) || !(refObject as DynamicObject).IsKinematic))
			{
				refObject.ImpactVelocity = magnitude;
			}
		}
	}

	public void PlayImpactSound(float velocity)
	{
		ImpactSound.SetRTPCValue(SoundManager.Instance.ImpactVelocity, velocity);
		ImpactSound.Play();
	}
}
