using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class Magazine : Item
	{
		[SerializeField] private int bulletCount;

		[SerializeField] private int maxBulletCount;

		public override float Quantity => bulletCount;

		public override float MaxQuantity => maxBulletCount;

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

		public override void ProcesStatsData(DynamicObjectStats dos)
		{
			base.ProcesStatsData(dos);
			MagazineStats magazineStats = dos as MagazineStats;
			if (magazineStats.BulletCount.HasValue)
			{
				bulletCount = magazineStats.BulletCount.Value;
				if (InvSlot != null && InvSlot.UI != null)
				{
					InvSlot.UI.UpdateSlot();
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
