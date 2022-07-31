using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.Objects
{
	public class AltCorpRifle : Weapon
	{
		[Header("UI")]
		public Image FireMode;

		public Text BulletCountText;

		private new void Start()
		{
			base.Start();
			UpdateUI();
		}

		public override void UpdateUI()
		{
			base.UpdateUI();
			FireMode.sprite = Client.Instance.SpriteManager.GetSprite(CurrentWeaponMod.ModsFireMode);
			AmmoUIByBulletCount();
		}

		public void AmmoUIByBulletCount()
		{
			if (!(BulletCountText != null))
			{
				return;
			}
			if (Quantity > 0f)
			{
				if (!BulletCountText.gameObject.activeInHierarchy)
				{
					BulletCountText.gameObject.SetActive(true);
				}
				BulletCountText.text = Quantity.ToString();
				if (Quantity <= 5f)
				{
					BulletCountText.color = new Color(1f, 0f, 0f, 0.3f);
				}
				else
				{
					BulletCountText.color = new Color(1f, 1f, 1f);
				}
			}
			else
			{
				BulletCountText.gameObject.SetActive(true);
				BulletCountText.color = new Color(1f, 0f, 0f, 0.3f);
				BulletCountText.text = "0";
			}
		}
	}
}
