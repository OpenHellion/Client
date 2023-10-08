using UnityEngine;

namespace ZeroGravity.Effects
{
	public class AirDoorParticleToggler : MonoBehaviour
	{
		private bool tmpBool;

		private ParticleSystem emiter;

		private void Start()
		{
			emiter = GetComponent<ParticleSystem>();
		}

		public void ToggleParticle(bool? isActive)
		{
			if (isActive.HasValue)
			{
				tmpBool = isActive.Value;
			}
			else
			{
				tmpBool = !tmpBool;
			}

			if (tmpBool)
			{
				emiter.Play();
			}
			else
			{
				emiter.Stop();
			}
		}

		public void SetScale(Vector3 scale)
		{
			if (emiter == null)
			{
				emiter = GetComponent<ParticleSystem>();
			}

			ParticleSystem.ShapeModule shape = emiter.shape;
			shape.scale = new Vector3(scale.x, scale.y, 1f);
		}
	}
}
