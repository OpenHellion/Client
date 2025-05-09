using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.UI;

namespace ZeroGravity.Objects
{
	public class MilitaryCorpHandgun01 : Weapon
	{
		private int _BulletCount;

		private bool _IsSingleFire;

		[Title("UI")] [SerializeField] private Text AmmoCount;

		public Image FireMode;

		public int BulletCount
		{
			get => _BulletCount;
			set
			{
				_BulletCount = value;
				AmmoCount.text = _BulletCount.ToString();
			}
		}

		public bool IsSingleFire
		{
			get => _IsSingleFire;
			set
			{
				_IsSingleFire = value;
				FireMode.sprite = SpriteManager.Instance.GetSprite(CurrentWeaponMod.ModsFireMode);
			}
		}

		private new void Start()
		{
			base.Start();
			UpdateUI();
		}

		public override void UpdateUI()
		{
			base.UpdateUI();
			BulletCount = (int)Quantity;
			if (Mods.IndexOf(CurrentWeaponMod) == 0 && !IsSingleFire)
			{
				IsSingleFire = true;
			}
			else if (IsSingleFire && Mods.IndexOf(CurrentWeaponMod) != 0)
			{
				IsSingleFire = false;
			}
		}
	}
}
