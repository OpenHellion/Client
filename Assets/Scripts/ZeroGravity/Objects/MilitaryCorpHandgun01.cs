using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.Objects
{
	public class MilitaryCorpHandgun01 : Weapon
	{
		private int _BulletCount;

		private bool _IsSingleFire;

		[Title("UI")]
		[SerializeField]
		private Text AmmoCount;

		public Image FireMode;

		public int BulletCount
		{
			get
			{
				return _BulletCount;
			}
			set
			{
				_BulletCount = value;
				AmmoCount.text = _BulletCount.ToString();
			}
		}

		public bool IsSingleFire
		{
			get
			{
				return _IsSingleFire;
			}
			set
			{
				_IsSingleFire = value;
				FireMode.sprite = Client.Instance.SpriteManager.GetSprite(CurrentWeaponMod.ModsFireMode);
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
