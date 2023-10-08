using System;
using System.Collections.Generic;
using System.Linq;
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

		public bool isActive = true;

		public int SlotIndex;

		public VesselComponent ParentVesselComponent;

		public override BaseAttachPointData GetData()
		{
			MachineryPartSlotData machineryPartSlotData = new MachineryPartSlotData
			{
				ItemTypes = new List<ItemType> { ItemType.MachineryPart },
				Scope = Scope,
				MaxTier = MaxTier,
				MinTier = MinTier,
				PartDecay = PartDecay,
				IsActive = isActive,
				SlotIndex = SlotIndex
			};
			MachineryPartSlotData data = machineryPartSlotData;
			FillBaseAPData(ref data);
			return data;
		}

		protected override void Awake()
		{
			AttachPointTransformData attachPointTransformData =
				attachableTypesList?.Find((AttachPointTransformData m) => ItemTypeRange.IsMachineryPart(m.ItemType));
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
			if (Executor == null)
			{
				SetActive(state: true);
			}

			ParentVesselComponent.MachineryPartAttached(this);
		}

		protected override void OnDetach()
		{
			base.OnDetach();
			if (Executor == null)
			{
				SetActive(state: false);
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
			return attachableTypesList.Select((AttachPointTransformData m) => m.MachineryPartType)
				.FirstOrDefault((MachineryPartType m) => m != MachineryPartType.None);
		}

		public override bool CanAttachItemType(ItemType itemType, GenericItemSubType? generic = null,
			MachineryPartType? part = null, int? partTier = null)
		{
			return base.CanAttachItemType(itemType, generic, part, partTier) &&
			       (!partTier.HasValue || (partTier.Value >= MinTier && partTier.Value <= MaxTier));
		}
	}
}
