using TriInspector;
using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.Objects
{
	public class MilitaryCorpAssaultRifle : Weapon
	{
		[Title("UI")]
		public GameObject regular;

		public GameObject stance;

		public Text AmmoCountInStance;

		public Image FireMode;

		private new void Start()
		{
			base.Start();
			UpdateUI();
		}

		public override void UpdateUI()
		{
			base.UpdateUI();
			if (base.IsSpecialStance)
			{
				regular.SetActive(false);
				stance.SetActive(true);
				AmmoCountInStance.text = Quantity.ToString();
				FireMode.sprite = Client.Instance.SpriteManager.GetSprite(CurrentWeaponMod.ModsFireMode);
			}
			else
			{
				regular.SetActive(true);
				stance.SetActive(false);
			}
		}
	}
}
