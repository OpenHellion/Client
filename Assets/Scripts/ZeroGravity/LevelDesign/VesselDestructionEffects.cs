using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class VesselDestructionEffects : MonoBehaviour
	{
		public List<ParticleSystem> ExplosionEffects;

		private SpaceObjectVessel parentVessel;

		private bool isActive;

		private void Awake()
		{
			GeometryRoot componentInParent = GetComponentInParent<GeometryRoot>();
			if (componentInParent != null)
			{
				parentVessel = GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
			}
		}

		private void OnEnable()
		{
			foreach (ParticleSystem explosionEffect in ExplosionEffects)
			{
				if (explosionEffect != null)
				{
					explosionEffect.Play();
				}
			}

			isActive = true;
		}

		private void Update()
		{
			if (!isActive)
			{
				return;
			}

			bool flag = true;
			foreach (ParticleSystem explosionEffect in ExplosionEffects)
			{
				if (explosionEffect != null && explosionEffect.IsAlive(true))
				{
					flag = false;
				}
			}

			if (flag)
			{
				isActive = false;
				Object.Destroy(base.gameObject);
			}
		}
	}
}
