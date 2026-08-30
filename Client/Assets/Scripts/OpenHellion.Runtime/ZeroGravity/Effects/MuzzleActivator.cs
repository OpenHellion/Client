using UnityEngine;

namespace ZeroGravity.Effects
{
	public class MuzzleActivator : MonoBehaviour
	{
		public GameObject currentMuzzle;

		public GameObject currentSmoke;

		private float timer;

		public float destroyTime;

		private void Update()
		{
			if (Time.time - timer >= destroyTime && currentMuzzle.activeInHierarchy)
			{
				currentMuzzle.SetActive(false);
			}
		}

		public void ActivateMuzzle()
		{
			timer = Time.time;
			currentMuzzle.SetActive(true);
			if (currentSmoke != null)
			{
				ParticleSystem[] componentsInChildren = currentSmoke.GetComponentsInChildren<ParticleSystem>();
				foreach (ParticleSystem particleSystem in componentsInChildren)
				{
					particleSystem.Play();
				}
			}
		}
	}
}
