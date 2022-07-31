using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.LevelDesign
{
	public abstract class BaseSceneAttachPoint : BaseSceneTrigger, ISceneObject, IItemSlot
	{
		[Serializable]
		public class AttachPointTransformData
		{
			public ItemType ItemType;

			public GenericItemSubType GenericItemType;

			public MachineryPartType MachineryPartType;

			public Transform AttachPoint;
		}

		[CompilerGenerated]
		private sealed class _003CGetAttachPointData_003Ec__AnonStorey1
		{
			internal Item it;

			internal bool _003C_003Em__0(AttachPointTransformData m)
			{
				return m.ItemType == it.Type && (!(it is GenericItem) || m.GenericItemType == (it as GenericItem).SubType) && (!(it is MachineryPart) || m.MachineryPartType == (it as MachineryPart).PartType);
			}
		}

		[CompilerGenerated]
		private sealed class _003CGetAttachPointData_003Ec__AnonStorey0
		{
			internal MachineryPart part;

			internal bool _003C_003Em__0(AttachPointTransformData m)
			{
				return m.ItemType == part.Type && m.MachineryPartType == part.PartType;
			}
		}

		[CompilerGenerated]
		private sealed class _003CCanAttachItemType_003Ec__AnonStorey2
		{
			internal ItemType itemType;

			internal GenericItemSubType? generic;

			internal MachineryPartType? part;

			internal bool _003C_003Em__0(AttachPointTransformData m)
			{
				return m.ItemType == itemType && (!generic.HasValue || m.GenericItemType == generic.Value) && (!part.HasValue || (m.MachineryPartType == part.GetValueOrDefault() && part.HasValue));
			}
		}

		[SerializeField]
		private int _inSceneID;

		[SerializeField]
		protected List<AttachPointTransformData> attachableTypesList;

		[SerializeField]
		private bool _isNearTrigger = true;

		[HideInInspector]
		public SpaceObjectVessel ParentVessel;

		public Collider Collider;

		public SceneTriggerExecuter Executer;

		public string OnAttachExecute;

		public string OnDetachExecute;

		[NonSerialized]
		public AttachPointSlotUI UI;

		private Item _Item;

		public int InSceneID
		{
			get
			{
				return _inSceneID;
			}
			set
			{
				_inSceneID = value;
			}
		}

		public override bool ExclusivePlayerLocking
		{
			get
			{
				return false;
			}
		}

		public override SceneTriggerType TriggerType
		{
			get
			{
				return SceneTriggerType.AttachPoint;
			}
		}

		public override PlayerHandsCheckType PlayerHandsCheck
		{
			get
			{
				return PlayerHandsCheckType.DontCheck;
			}
		}

		public override List<ItemType> PlayerHandsItemType
		{
			get
			{
				return null;
			}
		}

		public virtual string InteractionTip
		{
			get
			{
				if (attachableTypesList.Count == 1)
				{
					return Localization.SlotFor + ": " + GetItemName(attachableTypesList[0]);
				}
				if (attachableTypesList.Count > 3)
				{
					return Localization.SlotFor + ": " + Localization.MultipleItems;
				}
				if (attachableTypesList.Count > 1)
				{
					string text = string.Empty;
					foreach (AttachPointTransformData attachableTypes in attachableTypesList)
					{
						text = text + ((!(text == string.Empty)) ? ", " : string.Empty) + GetItemName(attachableTypes);
					}
					return Localization.SlotFor + ": " + text;
				}
				return null;
			}
		}

		public override bool IsNearTrigger
		{
			get
			{
				return _isNearTrigger;
			}
		}

		public override bool IsInteractable
		{
			get
			{
				return true;
			}
		}

		public override bool CameraMovementAllowed
		{
			get
			{
				return false;
			}
		}

		public virtual Item Item
		{
			get
			{
				return _Item;
			}
			protected set
			{
				if (_Item != value)
				{
					_Item = value;
					if (Collider != null)
					{
						Collider.enabled = value == null;
					}
					if (UI != null)
					{
						UI.UpdateSlot();
					}
				}
			}
		}

		public SpaceObject Parent
		{
			get
			{
				return ParentVessel;
			}
		}

		private string GetItemName(AttachPointTransformData data)
		{
			if (data.ItemType == ItemType.GenericItem)
			{
				return data.GenericItemType.ToLocalizedString();
			}
			if (data.ItemType == ItemType.MachineryPart)
			{
				return data.MachineryPartType.ToLocalizedString();
			}
			return data.ItemType.ToLocalizedString();
		}

		public Sprite GetIcon()
		{
			Sprite result = null;
			if (attachableTypesList.Count == 1)
			{
				result = ((attachableTypesList[0].ItemType == ItemType.GenericItem) ? Client.Instance.SpriteManager.GetSprite(attachableTypesList[0].GenericItemType) : ((attachableTypesList[0].ItemType != ItemType.MachineryPart) ? Client.Instance.SpriteManager.GetSprite(attachableTypesList[0].ItemType) : Client.Instance.SpriteManager.GetSprite(attachableTypesList[0].MachineryPartType)));
			}
			return result;
		}

		public abstract BaseAttachPointData GetData();

		protected void FillBaseAPData<T>(ref T data) where T : BaseAttachPointData
		{
			List<ItemType> list = new List<ItemType>();
			List<GenericItemSubType> list2 = new List<GenericItemSubType>();
			List<MachineryPartType> list3 = new List<MachineryPartType>();
			if (attachableTypesList != null && attachableTypesList.Count > 0)
			{
				foreach (AttachPointTransformData attachableTypes in attachableTypesList)
				{
					if (attachableTypes.ItemType == ItemType.GenericItem)
					{
						list2.Add(attachableTypes.GenericItemType);
					}
					else if (attachableTypes.ItemType == ItemType.MachineryPart)
					{
						list3.Add(attachableTypes.MachineryPartType);
					}
					else if (attachableTypes.ItemType != 0)
					{
						list.Add(attachableTypes.ItemType);
					}
				}
			}
			data.InSceneID = InSceneID;
			data.ItemTypes = list.Distinct().ToList();
			data.GenericSubTypes = list2.Distinct().ToList();
			data.MachineryPartTypes = list3.Distinct().ToList();
		}

		protected virtual void Awake()
		{
			if (Collider == null)
			{
				Collider = GetComponent<Collider>();
			}
		}

		protected override void Start()
		{
			base.Start();
			if (ParentVessel == null && Client.IsGameBuild)
			{
				ParentVessel = GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
			}
		}

		protected virtual AttachPointTransformData GetAttachPointData(Item it)
		{
			_003CGetAttachPointData_003Ec__AnonStorey1 _003CGetAttachPointData_003Ec__AnonStorey = new _003CGetAttachPointData_003Ec__AnonStorey1();
			_003CGetAttachPointData_003Ec__AnonStorey.it = it;
			if (this is SceneMachineryPartSlot)
			{
				_003CGetAttachPointData_003Ec__AnonStorey0 _003CGetAttachPointData_003Ec__AnonStorey2 = new _003CGetAttachPointData_003Ec__AnonStorey0();
				SceneMachineryPartSlot sceneMachineryPartSlot = this as SceneMachineryPartSlot;
				_003CGetAttachPointData_003Ec__AnonStorey2.part = _003CGetAttachPointData_003Ec__AnonStorey.it as MachineryPart;
				if (_003CGetAttachPointData_003Ec__AnonStorey2.part != null && (_003CGetAttachPointData_003Ec__AnonStorey2.part.Tier == 0 || sceneMachineryPartSlot.MaxTier == 0 || (_003CGetAttachPointData_003Ec__AnonStorey2.part.Tier >= sceneMachineryPartSlot.MinTier && _003CGetAttachPointData_003Ec__AnonStorey2.part.Tier <= sceneMachineryPartSlot.MaxTier)))
				{
					return attachableTypesList.Find(_003CGetAttachPointData_003Ec__AnonStorey2._003C_003Em__0);
				}
				return null;
			}
			return attachableTypesList.Find(_003CGetAttachPointData_003Ec__AnonStorey._003C_003Em__0);
		}

		public bool CanFitItem(Item it)
		{
			if (attachableTypesList == null || attachableTypesList.Count == 0)
			{
				return true;
			}
			return GetAttachPointData(it) != null;
		}

		public override bool Interact(MyPlayer myPlayer, bool interactWithOverlappingTriggers = true)
		{
			if (!base.Interact(myPlayer, interactWithOverlappingTriggers))
			{
				return false;
			}
			if (Item == null && myPlayer.animHelper.CanDrop)
			{
				Item item = myPlayer.Inventory.HandsSlot.Item;
				if (item != null && CanFitItem(item))
				{
					item.RequestAttach(this);
				}
			}
			else
			{
				OnDetach();
			}
			if (interactWithOverlappingTriggers)
			{
				SceneTriggerHelper.InteractWithOverlappingTriggers(base.gameObject, this, myPlayer);
			}
			return true;
		}

		protected virtual void OnAttach()
		{
			if (Executer != null && !OnAttachExecute.IsNullOrEmpty())
			{
				Executer.ChangeState(OnAttachExecute);
			}
		}

		protected virtual void OnDetach()
		{
			if (Executer != null && !OnDetachExecute.IsNullOrEmpty())
			{
				Executer.ChangeState(OnDetachExecute);
			}
		}

		public virtual Transform GetAttachPointTransform(Item itm)
		{
			if (attachableTypesList == null || attachableTypesList.Count == 0)
			{
				return base.transform;
			}
			AttachPointTransformData attachPointData = GetAttachPointData(itm);
			if (attachPointData != null && attachPointData.AttachPoint != null)
			{
				return attachPointData.AttachPoint;
			}
			return base.transform;
		}

		public virtual AttachPointDetails GetDetails()
		{
			AttachPointDetails attachPointDetails = new AttachPointDetails();
			attachPointDetails.InSceneID = InSceneID;
			return attachPointDetails;
		}

		public void AttachItem(Item item)
		{
			if (Item != null && Item != item)
			{
				Dbg.Warning("Cannot attach two items on same attach point", Item, item, base.name);
			}
			else
			{
				Item = item;
				OnAttach();
			}
		}

		public void DetachItem(Item item)
		{
			if (Item != item || Item == null)
			{
				Dbg.Warning("Cannot detach item from this slot", Item, item, base.name);
			}
			else
			{
				Item = null;
				OnDetach();
			}
		}

		public virtual bool CanAttachItemType(ItemType itemType, GenericItemSubType? generic = null, MachineryPartType? part = null, int? partTier = null)
		{
			_003CCanAttachItemType_003Ec__AnonStorey2 _003CCanAttachItemType_003Ec__AnonStorey = new _003CCanAttachItemType_003Ec__AnonStorey2();
			_003CCanAttachItemType_003Ec__AnonStorey.itemType = itemType;
			_003CCanAttachItemType_003Ec__AnonStorey.generic = generic;
			_003CCanAttachItemType_003Ec__AnonStorey.part = part;
			return attachableTypesList.FirstOrDefault(_003CCanAttachItemType_003Ec__AnonStorey._003C_003Em__0) != null;
		}

		private void OnDrawGizmos()
		{
			if (GetComponent<BoxCollider>() != null)
			{
				Gizmos.matrix = base.transform.localToWorldMatrix;
				Gizmos.color = new Color(0f, 1f, 0f, 0.05f);
				Gizmos.DrawCube(GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
				Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
				Gizmos.DrawWireCube(GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
				Gizmos.DrawIcon(base.transform.TransformPoint(GetComponent<BoxCollider>().center), "AttachPoint");
			}
		}
	}
}
