using UnityEngine;

namespace RootMotion.Demos
{
	public class WeaponRifle : WeaponBase
	{
		[Header("Shooting")]
		public Transform shootFrom;

		public float range = 300f;

		public LayerMask hitLayers;

		[Header("FX")]
		public ParticleSystem muzzleFlash;

		public ParticleSystem muzzleSmoke;

		public Transform bulletHole;

		public ParticleSystem bulletHit;

		public float smokeFadeOutSpeed = 5f;

		private float smokeEmission;

		public override void Fire()
		{
			muzzleFlash.Emit(1);
			smokeEmission = 10f;
			RaycastHit hitInfo;
			if (Physics.Raycast(shootFrom.position, shootFrom.forward, out hitInfo, range, hitLayers))
			{
				Vector3 position = hitInfo.point + hitInfo.normal * 0.01f;
				Object.Instantiate(bulletHole, position, Quaternion.LookRotation(-hitInfo.normal));
				bulletHit.transform.position = position;
				bulletHit.Emit(20);
			}
		}

		private void Update()
		{
			smokeEmission = Mathf.Max(smokeEmission - Time.deltaTime * smokeFadeOutSpeed, 0f);
			ParticleSystem.EmissionModule emission = muzzleSmoke.emission;
			emission.enabled = smokeEmission > 0f;
			emission.rate = new ParticleSystem.MinMaxCurve(smokeEmission);
		}
	}
}
