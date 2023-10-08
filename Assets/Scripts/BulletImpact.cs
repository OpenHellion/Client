using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.LevelDesign;

public class BulletImpact : MonoBehaviour
{
	public enum BulletImpactType
	{
		Metal = 0,
		Flesh = 1,
		Object = 2
	}

	public float OnGravityValue = 1f;

	public float OffGravityValue;

	public GameObject Decal;

	public SoundEffect SoundEffect;

	[CompilerGenerated] private static Func<Collider, bool> _003C_003Ef__am_0024cache0;

	[CompilerGenerated] private static Func<Collider, bool> _003C_003Ef__am_0024cache1;

	public void Play()
	{
		Collider[] source =
			Physics.OverlapSphere(base.transform.position, 0.2f, 1 << LayerMask.NameToLayer("Triggers"));
		if (_003C_003Ef__am_0024cache0 == null)
		{
			_003C_003Ef__am_0024cache0 = _003CPlay_003Em__0;
		}

		Collider collider = source.FirstOrDefault(_003C_003Ef__am_0024cache0);
		SceneTriggerRoom sceneTriggerRoom = null;
		if (collider != null)
		{
			sceneTriggerRoom = collider.GetComponent<SceneTriggerRoom>();
		}
		else
		{
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CPlay_003Em__1;
			}

			collider = source.FirstOrDefault(_003C_003Ef__am_0024cache1);
			if (collider != null)
			{
				sceneTriggerRoom = collider.GetComponent<SceneTriggerRoomSegment>().BaseRoom;
			}
		}

		if (sceneTriggerRoom != null)
		{
			SoundEffect.SetEnvironment(sceneTriggerRoom.EnvironmentReverb);
			ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particleSystem in componentsInChildren)
			{
				ParticleSystem.MainModule main = particleSystem.main;
				main.gravityModifier = ((!sceneTriggerRoom.UseGravity) ? OffGravityValue : OnGravityValue);
			}
		}
		else
		{
			SoundEffect.SetEnvironment("None");
			ParticleSystem[] componentsInChildren2 = GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particleSystem2 in componentsInChildren2)
			{
				ParticleSystem.MainModule main2 = particleSystem2.main;
				main2.gravityModifier = OffGravityValue;
			}
		}
	}

	public void DestroyBulletImpact()
	{
		if (Decal != null)
		{
			UnityEngine.Object.Destroy(Decal);
		}

		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void Update()
	{
	}

	[CompilerGenerated]
	private static bool _003CPlay_003Em__0(Collider m)
	{
		return m.GetComponent<SceneTriggerRoom>() != null;
	}

	[CompilerGenerated]
	private static bool _003CPlay_003Em__1(Collider m)
	{
		return m.GetComponent<SceneTriggerRoomSegment>();
	}
}
