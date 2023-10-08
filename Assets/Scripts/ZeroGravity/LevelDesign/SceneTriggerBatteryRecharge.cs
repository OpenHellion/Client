using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerBatteryRecharge : BaseSceneAttachPoint
	{
		private bool isInUse;

		public GameObject chargeIcon;

		public Text RechargeStationText;

		public bool IsInUse
		{
			get { return isInUse; }
		}

		public override string InteractionTip
		{
			get { return Localization.PowerRechargeStation; }
		}

		public override BaseAttachPointData GetData()
		{
			BatteryRechargePointData data = new BatteryRechargePointData();
			FillBaseAPData(ref data);
			return data;
		}

		protected override void Awake()
		{
			if (attachableTypesList == null || attachableTypesList.Count == 0)
			{
				attachableTypesList = new List<AttachPointTransformData>
				{
					new AttachPointTransformData
					{
						AttachPoint = base.transform,
						ItemType = ItemType.AltairHandDrillBattery
					}
				};
			}

			chargeIcon.SetActive(isInUse);
			RechargeStationText.text = Localization.RechargeStation.ToUpper();
			base.Awake();
		}

		protected override void OnAttach()
		{
			isInUse = true;
			chargeIcon.SetActive(true);
			base.OnAttach();
		}

		protected override void OnDetach()
		{
			isInUse = false;
			chargeIcon.SetActive(false);
			base.OnDetach();
		}
	}
}
