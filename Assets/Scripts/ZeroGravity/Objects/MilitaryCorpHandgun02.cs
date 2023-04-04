using UnityEngine;
using UnityEngine.UI;

namespace ZeroGravity.Objects
{
	public class MilitaryCorpHandgun02 : Weapon
	{
		private int _BulletCount;

		[Title("UI")]
		[SerializeField]
		private Text AmmoCount;

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

		private new void Start()
		{
			base.Start();
			UpdateUI();
		}

		public override void UpdateUI()
		{
			base.UpdateUI();
			BulletCount = (int)Quantity;
		}
	}
}
