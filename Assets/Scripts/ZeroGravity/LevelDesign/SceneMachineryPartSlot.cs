using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.LevelDesign
{
	public class SceneMachineryPartSlot : BaseSceneAttachPoint
	{
		public MachineryPartSlotScope Scope;

		public string PartDescription;

		public int MinTier;

		public int MaxTier;

		[Tooltip("Part rate of decay [HP/hr]")]
		public float PartDecay;

		[SerializeField]
		public bool isActive = true;

		public int SlotIndex;

		public VesselComponent ParentVesselComponent;

		[CompilerGenerated]
		private static Predicate<AttachPointTransformData> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<AttachPointTransformData, MachineryPartType> _003C_003Ef__am_0024cache1;

		[CompilerGenerated]
		private static Func<MachineryPartType, bool> _003C_003Ef__am_0024cache2;

		public override BaseAttachPointData GetData()
		{
			MachineryPartSlotData machineryPartSlotData = new MachineryPartSlotData();
			machineryPartSlotData.ItemTypes = new List<ItemType> { ItemType.MachineryPart };
			machineryPartSlotData.Scope = Scope;
			machineryPartSlotData.MaxTier = MaxTier;
			machineryPartSlotData.MinTier = MinTier;
			machineryPartSlotData.PartDecay = PartDecay;
			machineryPartSlotData.IsActive = isActive;
			machineryPartSlotData.SlotIndex = SlotIndex;
			MachineryPartSlotData data = machineryPartSlotData;
			FillBaseAPData(ref data);
			return data;
		}

		protected override void Awake()
		{
			object obj;
			if (attachableTypesList != null)
			{
				List<AttachPointTransformData> list = attachableTypesList;
				if (_003C_003Ef__am_0024cache0 == null)
				{
					_003C_003Ef__am_0024cache0 = _003CAwake_003Em__0;
				}
				obj = list.Find(_003C_003Ef__am_0024cache0);
			}
			else
			{
				obj = null;
			}
			AttachPointTransformData attachPointTransformData = (AttachPointTransformData)obj;
			base.Awake();
		}

		public void SetActive(bool state, bool changeStats = true)
		{
			if (state != isActive)
			{
				isActive = state;
				if (changeStats)
				{
					SpaceObjectVessel parentVessel = ParentVessel;
					AttachPointDetails details = GetDetails();
					parentVessel.ChangeStats(null, null, null, null, null, null, null, null, null, null, details);
				}
			}
		}

		public override AttachPointDetails GetDetails()
		{
			AttachPointDetails details = base.GetDetails();
			details.AuxDetails = new MachineryPartSlotAuxDetails
			{
				IsActive = isActive
			};
			return details;
		}

		protected override void OnAttach()
		{
			base.OnAttach();
			if (Executer == null)
			{
				SetActive(true);
			}
			ParentVesselComponent.MachineryPartAttached(this);
		}

		protected override void OnDetach()
		{
			base.OnDetach();
			if (Executer == null)
			{
				SetActive(false);
			}
			ParentVesselComponent.MachineryPartDetached(this);
		}

		public string GetDescription()
		{
			MachineryPartType machineryPartType = GetMachineryPartType();
			if (Localization.MachineryPartsDescriptions.ContainsKey(machineryPartType))
			{
				return Localization.MachineryPartsDescriptions[machineryPartType];
			}
			if (!PartDescription.IsNullOrEmpty())
			{
				return PartDescription;
			}
			return string.Empty;
		}

		public MachineryPartType GetMachineryPartType()
		{
			List<AttachPointTransformData> source = attachableTypesList;
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CGetMachineryPartType_003Em__1;
			}
			IEnumerable<MachineryPartType> source2 = source.Select(_003C_003Ef__am_0024cache1);
			if (_003C_003Ef__am_0024cache2 == null)
			{
				_003C_003Ef__am_0024cache2 = _003CGetMachineryPartType_003Em__2;
			}
			return source2.FirstOrDefault(_003C_003Ef__am_0024cache2);
		}

		public override bool CanAttachItemType(ItemType itemType, GenericItemSubType? generic = null, MachineryPartType? part = null, int? partTier = null)
		{
			return base.CanAttachItemType(itemType, generic, part, partTier) && (!partTier.HasValue || (partTier.Value >= MinTier && partTier.Value <= MaxTier));
		}

		[CompilerGenerated]
		private static bool _003CAwake_003Em__0(AttachPointTransformData m)
		{
			return ItemTypeRange.IsMachineryPart(m.ItemType);
		}

		[CompilerGenerated]
		private static MachineryPartType _003CGetMachineryPartType_003Em__1(AttachPointTransformData m)
		{
			return m.MachineryPartType;
		}

		[CompilerGenerated]
		private static bool _003CGetMachineryPartType_003Em__2(MachineryPartType m)
		{
			return m != MachineryPartType.None;
		}
	}
}
