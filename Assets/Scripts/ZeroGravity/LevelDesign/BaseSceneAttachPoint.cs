using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
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

		[SerializeField] private int _inSceneID;

		[SerializeField] protected List<AttachPointTransformData> attachableTypesList;

		[SerializeField] private bool _isNearTrigger = true;

		[HideInInspector] public SpaceObjectVessel ParentVessel;

		public Collider Collider;

		[FormerlySerializedAs("Executer")] public SceneTriggerExecutor Executor;

		public string OnAttachExecute;

		public string OnDetachExecute;

		[NonSerialized] public AttachPointSlotUI UI;

		private Item _item;

		public int InSceneID
		{
			get => _inSceneID;
			set => _inSceneID = value;
		}

		public override bool ExclusivePlayerLocking => false;

		public override SceneTriggerType TriggerType => SceneTriggerType.AttachPoint;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.DontCheck;

		public override List<ItemType> PlayerHandsItemType => null;

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

		public override bool IsNearTrigger => _isNearTrigger;

		public override bool IsInteractable => true;

		public override bool CameraMovementAllowed => false;

		public virtual Item Item
		{
			get => _item;
			protected set
			{
				if (_item != value)
				{
					_item = value;
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

		public SpaceObject Parent => ParentVessel;

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
				result = (attachableTypesList[0].ItemType == ItemType.GenericItem)
					? SpriteManager.Instance.GetSprite(attachableTypesList[0].GenericItemType)
					: ((attachableTypesList[0].ItemType != ItemType.MachineryPart)
						? SpriteManager.Instance.GetSprite(attachableTypesList[0].ItemType)
						: SpriteManager.Instance.GetSprite(attachableTypesList[0].MachineryPartType));
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
			if (ParentVessel == null)
			{
				ParentVessel = GetComponentInParent<GeometryRoot>().MainObject as SpaceObjectVessel;
			}
		}

		protected virtual AttachPointTransformData GetAttachPointData(Item it)
		{
			if (this is SceneMachineryPartSlot)
			{
				SceneMachineryPartSlot sceneMachineryPartSlot = this as SceneMachineryPartSlot;
				MachineryPart part = it as MachineryPart;
				if (part != null && (part.Tier == 0 || sceneMachineryPartSlot.MaxTier == 0 ||
				                     (part.Tier >= sceneMachineryPartSlot.MinTier &&
				                      part.Tier <= sceneMachineryPartSlot.MaxTier)))
				{
					return attachableTypesList.Find((AttachPointTransformData m) =>
						m.ItemType == part.Type && m.MachineryPartType == part.PartType);
				}

				return null;
			}

			return attachableTypesList.Find((AttachPointTransformData m) => m.ItemType == it.Type &&
			                                                                (!(it is GenericItem) ||
			                                                                 m.GenericItemType ==
			                                                                 (it as GenericItem).SubType) &&
			                                                                (!(it is MachineryPart) ||
			                                                                 m.MachineryPartType ==
			                                                                 (it as MachineryPart).PartType));
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
				SceneTriggerHelper.InteractWithOverlappingTriggers(gameObject, this, myPlayer);
			}

			return true;
		}

		protected virtual void OnAttach()
		{
			if (Executor != null && !OnAttachExecute.IsNullOrEmpty())
			{
				Executor.ChangeState(OnAttachExecute);
			}
		}

		protected virtual void OnDetach()
		{
			if (Executor != null && !OnDetachExecute.IsNullOrEmpty())
			{
				Executor.ChangeState(OnDetachExecute);
			}
		}

		public virtual Transform GetAttachPointTransform(Item itm)
		{
			if (attachableTypesList == null || attachableTypesList.Count == 0)
			{
				return transform;
			}

			AttachPointTransformData attachPointData = GetAttachPointData(itm);
			if (attachPointData != null && attachPointData.AttachPoint != null)
			{
				return attachPointData.AttachPoint;
			}

			return transform;
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
				Debug.LogWarningFormat("Cannot attach two items on same attach point {0} and {1}. Original item name {2}.", Item, item, base.name);
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
				Debug.LogWarningFormat("Cannot detach item from this slot {0} and {1}. Name {2}.", Item, item, name);
			}
			else
			{
				Item = null;
				OnDetach();
			}
		}

		public virtual bool CanAttachItemType(ItemType itemType, GenericItemSubType? generic = null,
			MachineryPartType? part = null, int? partTier = null)
		{
			return attachableTypesList.FirstOrDefault((AttachPointTransformData m) =>
				m.ItemType == itemType && (!generic.HasValue || m.GenericItemType == generic.Value) &&
				(!part.HasValue || (m.MachineryPartType == part.GetValueOrDefault() && part.HasValue))) != null;
		}

		private void OnDrawGizmos()
		{
			if (GetComponent<BoxCollider>() != null)
			{
				Gizmos.matrix = transform.localToWorldMatrix;
				Gizmos.color = new Color(0f, 1f, 0f, 0.05f);
				Gizmos.DrawCube(GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
				Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
				Gizmos.DrawWireCube(GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
				Gizmos.DrawIcon(transform.TransformPoint(GetComponent<BoxCollider>().center), "AttachPoint", false);
			}
		}
	}
}
