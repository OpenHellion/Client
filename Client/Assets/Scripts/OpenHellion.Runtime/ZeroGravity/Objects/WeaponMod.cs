using System;
using UnityEngine;

namespace ZeroGravity.Objects
{
	[Serializable]
	public class WeaponMod
	{
		public enum FireMode
		{
			Single = 0,
			Auto = 1
		}

		public FireMode ModsFireMode;

		public float PowerConsuption;

		public float Damage;

		public float RateOfFire;

		public float Range;

		[SerializeField] public Vector2 SpecialRecoil;

		[SerializeField] public Vector2 NormalRecoil;

		public float RotationPerShot;

		public float ThrustPerShot;
	}
}
