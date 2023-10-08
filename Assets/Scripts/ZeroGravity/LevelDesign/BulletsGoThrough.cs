using UnityEngine;

namespace ZeroGravity.LevelDesign
{
	public class BulletsGoThrough : MonoBehaviour
	{
		public enum HitEffectType
		{
			None = 0,
			Flesh = 1,
			DefaultMetal = 2
		}

		public HitEffectType LeaveDecal;

		[HideInInspector] public float DamageMultiply = 1f;

		[HideInInspector] public float DamageTreshold;

		private void Start()
		{
		}

		public void Modify(ref Vector3 direction, ref float range)
		{
		}

		private void Update()
		{
		}
	}
}
