using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class Magazine : Item
	{
		[SerializeField]
		private int bulletCount;

		[SerializeField]
		private int maxBulletCount;

		public override float Quantity
		{
			get
			{
				return bulletCount;
			}
		}

		public override float MaxQuantity
		{
			get
			{
				return maxBulletCount;
			}
		}

		public override void ChangeQuantity(float amount)
		{
			bulletCount += (int)amount;
			if (bulletCount <= 0)
			{
				bulletCount = 0;
			}
			else if (bulletCount > maxBulletCount)
			{
				bulletCount = maxBulletCount;
			}
		}

		private static void SplitMagazines(Magazine fromMag, Magazine toMag)
		{
			int num = toMag.maxBulletCount - toMag.bulletCount;
			if (toMag.bulletCount == 0)
			{
				num = fromMag.bulletCount / 2;
			}
			else if (fromMag.bulletCount < num)
			{
				num = fromMag.bulletCount;
			}
			toMag.bulletCount += num;
			fromMag.bulletCount -= num;
			DynamicObject dynamicObj = fromMag.DynamicObj;
			MagazineStats statsData = new MagazineStats
			{
				BulletsFrom = fromMag.GUID,
				BulletsTo = toMag.GUID
			};
			dynamicObj.SendStatsMessage(null, statsData);
		}

		public override bool ProcessSlotChange(Inventory inv, InventorySlot mySlot, InventorySlot nextSlot)
		{
			if (nextSlot != null && nextSlot.CanFitItem(this) && nextSlot.Item != null && nextSlot.Item.Type == Type)
			{
				Magazine toMag = nextSlot.Item as Magazine;
				SplitMagazines(this, toMag);
				return true;
			}
			return false;
		}

		public override void ProcesStatsData(DynamicObjectStats dos)
		{
			base.ProcesStatsData(dos);
			MagazineStats magazineStats = dos as MagazineStats;
			if (magazineStats.BulletCount.HasValue)
			{
				bulletCount = magazineStats.BulletCount.Value;
				if (base.InvSlot != null && base.InvSlot.UI != null)
				{
					base.InvSlot.UI.UpdateSlot();
				}
				else if (DynamicObj.Parent is DynamicObject && (DynamicObj.Parent as DynamicObject).Item is Weapon)
				{
					((DynamicObj.Parent as DynamicObject).Item as Weapon).UpdateUI();
				}
			}
		}

		public override DynamicObjectAuxData GetAuxData()
		{
			MagazineData baseAuxData = GetBaseAuxData<MagazineData>();
			baseAuxData.BulletCount = bulletCount;
			baseAuxData.MaxBulletCount = maxBulletCount;
			return baseAuxData;
		}

		public override string QuantityCheck()
		{
			return FormatHelper.CurrentMax(Quantity, MaxQuantity);
		}
	}
}
