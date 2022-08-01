using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;

namespace ZeroGravity.Objects
{
	public class MeleeWeapon : Item
	{
		public float Damage;

		public float RateOfFire;

		public float Range;

		private AnimatorHelper animHelper;

		private float lastAttackTime;

		private new void Awake()
		{
			base.Awake();
			if (Client.IsGameBuild)
			{
				animHelper = MyPlayer.Instance.animHelper;
			}
		}

		public override bool PrimaryFunction()
		{
			Attack();
			return true;
		}

		private void Attack()
		{
			if (!(Time.time - lastAttackTime < RateOfFire))
			{
				lastAttackTime = Time.time;
				animHelper.SetParameterTrigger(AnimatorHelper.Triggers.Melee);
			}
		}

		public override DynamicObjectAuxData GetAuxData()
		{
			MeleeWeaponData baseAuxData = GetBaseAuxData<MeleeWeaponData>();
			baseAuxData.RateOfFire = RateOfFire;
			baseAuxData.Damage = Damage;
			baseAuxData.Range = Range;
			return baseAuxData;
		}

		public void MakeBulletHole(RaycastHit info, BulletsGoThrough.HitEffectType decalType)
		{
		}
	}
}
