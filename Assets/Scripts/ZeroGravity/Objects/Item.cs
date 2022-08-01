using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.UI;

namespace ZeroGravity.Objects
{
	[RequireComponent(typeof(DynamicObject))]
	public class Item : MonoBehaviour, IDamageable
	{
		[Serializable]
		public class ItemIngredient
		{
			public ResourceType ResourceType;

			public float Quantity;
		}

		[Serializable]
		public class ItemIngredientLists
		{
			public ItemIngredient[] Recycle;

			public ItemIngredient[] Craft;
		}

		public enum SizeType
		{
			None = 0,
			Rifle = 1,
			Pistol = 2,
			SmallItem = 3,
			Barrel = 4,
			Box = 5
		}

		public enum EquipType
		{
			None = 0,
			Hands = 1,
			EquipInventory = 2,
			Inventory = 3
		}

		public enum Hand
		{
			Left = 1,
			Right = 2
		}

		public enum ItemAnimationType
		{
			Equip = 1,
			Unequip = 2
		}

		public enum ItemSound
		{
			Reload = 0
		}

		[CompilerGenerated]
		private sealed class _003CAttachToObjectImpl_003Ec__AnonStorey1
		{
			internal Transform attachToTrans;

			internal SpaceObject obj;

			internal BaseSceneAttachPoint attachPoint;

			internal IItemSlot slot;

			internal bool hideObject;

			internal Item _0024this;

			internal void _003C_003Em__0()
			{
				IItemSlot itemSlot = _0024this.Slot;
				bool flag = attachToTrans != null;
				if (_0024this.DynamicObj.Parent is Pivot && _0024this.DynamicObj.Parent != obj)
				{
					Client.Instance.SolarSystem.RemoveArtificialBody(_0024this.DynamicObj.Parent as Pivot);
					UnityEngine.Object.Destroy(_0024this.DynamicObj.Parent.gameObject);
				}
				if (_0024this.AttachPoint != null && _0024this.AttachPoint != attachPoint)
				{
					_0024this.AttachPoint.DetachItem(_0024this);
					_0024this.AttachPoint = null;
				}
				if (_0024this.DynamicObj.Parent is Player && (attachPoint != null || !flag))
				{
					Player player = _0024this.DynamicObj.Parent as Player;
					if (player.Inventory.ItemInHands == _0024this)
					{
						player.Inventory.RemoveItemFromHands(true);
					}
					_0024this.ChangeEquip(EquipType.None, player);
				}
				if (_0024this.InvSlot != null && _0024this.InvSlot != slot)
				{
					if (_0024this.InvSlot.Item == _0024this)
					{
						if (_0024this.InvSlot.Inventory != null && _0024this.InvSlot.Inventory.Parent is Player && _0024this.InvSlot.SlotType == InventorySlot.Type.Hands)
						{
							_0024this.InvSlot.Inventory.RemoveItemFromHands(true);
						}
						else
						{
							_0024this.InvSlot.SetItem(null);
						}
					}
					_0024this.InvSlot = null;
				}
				if (attachPoint != null)
				{
					_0024this.AttachPoint = attachPoint;
				}
				if (slot != null)
				{
					if (slot is InventorySlot)
					{
						_0024this.InvSlot = slot as InventorySlot;
						if ((slot as InventorySlot).Inventory != null && (slot as InventorySlot).Inventory.Parent is SpaceObjectTransferable)
						{
							SpaceObjectTransferable spaceObjectTransferable = (slot as InventorySlot).Inventory.Parent as SpaceObjectTransferable;
						}
					}
					else if (slot is ItemSlot)
					{
						(slot as ItemSlot).FitItem(_0024this);
					}
				}
				SpaceObject parent = _0024this.DynamicObj.Parent;
				_0024this.DynamicObj.Parent = obj;
				_0024this.DynamicObj.ResetRoomTriggers();
				_0024this.DynamicObj.ToggleKinematic(flag || parent is OtherPlayer);
				_0024this.DynamicObj.ToggleActive(!hideObject);
				_0024this.DynamicObj.ToggleTriggerColliders(_0024this.DynamicObj.Parent is Corpse);
				_0024this.DynamicObj.ToggleEnabled(!flag || _0024this.AttachPoint != null || _0024this.DynamicObj.Parent is Corpse, true);
				if (!flag)
				{
					_0024this.DynamicObj.CheckRoomTrigger(null);
				}
				if (_0024this.InvSlot != null && _0024this.InvSlot.SlotType == InventorySlot.Type.Hands)
				{
					_0024this.InvSlot.Inventory.ItemAddedToHands(_0024this);
					if (_0024this.PickupSound != null)
					{
						_0024this.PickupSound.Play();
					}
				}
				try
				{
					if (_0024this.AttachPoint != null)
					{
						_0024this.AttachPoint.AttachItem(_0024this);
					}
				}
				catch (Exception ex)
				{
					Dbg.Error("Item attach point exception", ex.Message, ex.StackTrace);
				}
				if (_0024this.DynamicObj.Parent is DynamicObject)
				{
				}
				if (flag)
				{
					_0024this.transform.SetParent(attachToTrans);
					_0024this.transform.Reset(true);
					_0024this.OnAttach(_0024this.AttachPoint == null, _0024this.DynamicObj.Parent is Player);
				}
				else
				{
					_0024this.OnAttach(true, _0024this.DynamicObj.Parent is Player);
					if (_0024this.DynamicObj.Parent is SpaceObjectVessel)
					{
						_0024this.transform.SetParent(_0024this.DynamicObj.Parent.TransferableObjectsRoot.transform);
						_0024this.DynamicObj.CheckRoomTrigger(null);
					}
					else if (_0024this.DynamicObj.Parent is Pivot && _0024this.DynamicObj.Parent.Type == SpaceObjectType.DynamicObjectPivot)
					{
						_0024this.transform.SetParent(_0024this.DynamicObj.Parent.TransferableObjectsRoot.transform);
					}
					else
					{
						Dbg.Error("Dynamic object cannot be attached to DynamicObjectRoot");
					}
				}
				Client.Instance.CanvasManager.CanvasUI.HelmetHud.HandsSlotUpdate();
				if (itemSlot != null && itemSlot is BaseSceneAttachPoint && _0024this.Slot != null && _0024this.Slot.Parent.GetComponentInParent<MyPlayer>() != null)
				{
					SceneQuestTrigger.Check((itemSlot as BaseSceneAttachPoint).gameObject, SceneQuestTriggerEvent.DetachItem);
				}
				else if (itemSlot != null && itemSlot.Parent.GetComponentInParent<MyPlayer>() != null && _0024this.Slot != null && _0024this.Slot is BaseSceneAttachPoint)
				{
					SceneQuestTrigger.Check((_0024this.Slot as BaseSceneAttachPoint).gameObject, SceneQuestTriggerEvent.AttachItem);
				}
			}
		}

		[CompilerGenerated]
		private sealed class _003CProcessAttachData_003Ec__AnonStorey2
		{
			internal bool myPlayerIsParent;

			internal DynamicObjectAttachData data;

			internal SpaceObject prevParent;

			internal Item _0024this;

			internal void _003C_003Em__0()
			{
				if (_0024this.DynamicObj.IsAttached)
				{
					return;
				}
				if (!myPlayerIsParent || !_0024this.DynamicObj.Master)
				{
					if (data.LocalPosition != null)
					{
						_0024this.transform.localPosition = data.LocalPosition.ToVector3();
					}
					if (data.LocalRotation != null)
					{
						_0024this.transform.localRotation = data.LocalRotation.ToQuaternion();
					}
				}
				if (_0024this.DynamicObj.Master)
				{
					if (data.Velocity != null)
					{
						_0024this.DynamicObj.rigidBody.velocity = data.Velocity.ToVector3();
					}
					if (data.Torque != null)
					{
						_0024this.AddTorque(data.Torque.ToVector3(), ForceMode.Impulse);
					}
					if (data.ThrowForce != null)
					{
						Vector3 vector = data.ThrowForce.ToVector3();
						if ((MyPlayer.Instance.CurrentRoomTrigger == null || !MyPlayer.Instance.CurrentRoomTrigger.UseGravity || MyPlayer.Instance.CurrentRoomTrigger.GravityForce == Vector3.zero) && prevParent == MyPlayer.Instance)
						{
							float num = MyPlayer.Instance.rigidBody.mass + _0024this.DynamicObj.Mass;
							_0024this.AddForce(vector * (MyPlayer.Instance.rigidBody.mass / num), ForceMode.VelocityChange);
							MyPlayer.Instance.rigidBody.AddForce(-vector * (_0024this.DynamicObj.Mass / num), ForceMode.VelocityChange);
						}
						else
						{
							_0024this.AddForce(vector, ForceMode.Impulse);
						}
					}
				}
				if (_0024this.ThrowSound != null)
				{
					_0024this.ThrowSound.Play();
				}
			}
		}

		[CompilerGenerated]
		private sealed class _003CGetRecycleResources_003Ec__AnonStorey3
		{
			internal ItemType itemType;

			internal GenericItemSubType subType;

			internal MachineryPartType partType;

			internal int tier;

			internal bool _003C_003Em__0(ItemIngredientsData m)
			{
				return m.Type == itemType && m.SubType == subType && m.PartType == partType;
			}

			internal bool _003C_003Em__1(KeyValuePair<int, ItemIngredientsTierData> m)
			{
				return m.Key <= tier;
			}
		}

		[CompilerGenerated]
		private sealed class _003CGetCraftingResources_003Ec__AnonStorey4
		{
			internal ItemType itemType;

			internal GenericItemSubType subType;

			internal MachineryPartType partType;

			internal int tier;

			internal bool _003C_003Em__0(ItemIngredientsData m)
			{
				return m.Type == itemType && m.SubType == subType && m.PartType == partType;
			}

			internal bool _003C_003Em__1(KeyValuePair<int, ItemIngredientsTierData> m)
			{
				return m.Key <= tier;
			}
		}

		[CompilerGenerated]
		private sealed class _003CRequestAttach_003Ec__AnonStorey5
		{
			internal Item swapItem;

			internal bool _003C_003Em__0(ItemSlot m)
			{
				return m.CanFitItem(swapItem);
			}
		}

		public bool DefaultBlueprint;

		public ItemCategory Category;

		[SerializeField]
		public ItemType Type;

		public List<ItemType> ReloadWithTypes;

		[Header("General")]
		public string Name = string.Empty;

		[HideInInspector]
		public string ToolTip = string.Empty;

		public bool CanGoDirectlyToInventory = true;

		public Dictionary<short, ItemSlot> Slots = new Dictionary<short, ItemSlot>();

		public bool IsSlotContainer;

		private IItemSlot _Slot;

		[SerializeField]
		[Range(1f, 4f)]
		private int _Tier = 1;

		public float[] TierMultipliers;

		public float[] AuxValues;

		[ContextMenuItem("apply", "ApplyTierColor")]
		public Color[] TierColors;

		[ContextMenuItem("apply", "ApplyTierColor")]
		public GameObject[] TierGameObjects;

		public ItemIngredientLists[] Ingredients;

		[SerializeField]
		[Space(10f)]
		public string ShaderProperty = "_Mat1Color1";

		[HideInInspector]
		public DynamicObject DynamicObj;

		public float PassiveSpeedMultiplier = 1f;

		public float ActiveSpeedMultiplier = 1f;

		public float SpecialSpeedMultiplier = 1f;

		public bool HasActiveStance;

		public bool HasSpecialStance;

		public bool NeedsFullAnimOverride;

		public bool useIkForTargeting;

		public bool forceAnimOverride;

		public bool useSwayAnimations;

		public bool HasMelee;

		public float MeleeDamage;

		public float MeleeRange;

		[Header("ITEM DESTRUCTION")]
		public bool DestructAt0HP;

		[FormerlySerializedAs("BlastRadius")]
		public float ExplosionRadius;

		[FormerlySerializedAs("Damage")]
		public float ExplosionDamage;

		[FormerlySerializedAs("DamageType")]
		public TypeOfDamage ExplosionDamageType;

		[FormerlySerializedAs("BlastImpulse")]
		public float ExplosionImpulse;

		[FormerlySerializedAs("Effects")]
		public ParticleSystem ExplosionEffects;

		public SoundEffect ExplosionSound;

		[Space(10f)]
		public Transform ikTargetingPoint;

		[SerializeField]
		private float _MaxHealth = 100f;

		[SerializeField]
		private float _Health = 100f;

		[SerializeField]
		private float _Armor;

		[SerializeField]
		private bool _Damageable;

		[SerializeField]
		private bool _Expendable;

		[SerializeField]
		private bool _Repairable;

		[SerializeField]
		[Tooltip("HP per second")]
		private float _UsageWear;

		[Space(10f)]
		public Renderer HealthIndicator;

		public int MaterialIndex;

		public Color FullHealthColor = new Color(1f, 1f, 1f, 1f);

		public float FullHealthIntensity = 20f;

		public Color MidHealthColor = new Color(0.5f, 0.5f, 0.5f, 1f);

		public float MidHealthIntensity = 10f;

		public Color NoHealthColor = new Color(0f, 0f, 0f, 1f);

		public float NoHealthIntensity;

		public AnimationCurve HealthIndicatorCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[Range(0f, 1f)]
		public float TestHealthPercentage = 1f;

		[Header("SOUND")]
		public SoundEffect ThrowSound;

		public SoundEffect PickupSound;

		private Vector3? sendVelocity;

		private Vector3? sendTorque;

		private Vector3? sendThrowForce;

		[Header("ANIMATIONS")]
		public ItemAnimations ItemAnimations;

		[HideInInspector]
		public ItemAnimations.FPSAnimations fpsAnimations;

		[HideInInspector]
		public ItemAnimations.TPSAnimations tpsAnimations;

		private float prevQuantity;

		private bool exploded;

		private int explosionRaycastMask;

		[CompilerGenerated]
		private static Func<ItemSlot, short> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<ItemSlot, ItemSlot> _003C_003Ef__am_0024cache1;

		[CompilerGenerated]
		private static Func<KeyValuePair<short, ItemSlot>, bool> _003C_003Ef__am_0024cache2;

		[CompilerGenerated]
		private static Func<ItemIngredient, bool> _003C_003Ef__am_0024cache3;

		[CompilerGenerated]
		private static Func<ItemIngredient, bool> _003C_003Ef__am_0024cache4;

		[CompilerGenerated]
		private static Func<Renderer, bool> _003C_003Ef__am_0024cache5;

		[CompilerGenerated]
		private static Func<Renderer, bool> _003C_003Ef__am_0024cache6;

		[CompilerGenerated]
		private static Func<Renderer, bool> _003C_003Ef__am_0024cache7;

		[CompilerGenerated]
		private static Func<Renderer, bool> _003C_003Ef__am_0024cache8;

		[CompilerGenerated]
		private static Func<Renderer, bool> _003C_003Ef__am_0024cache9;

		[CompilerGenerated]
		private static Func<Renderer, bool> _003C_003Ef__am_0024cacheA;

		[CompilerGenerated]
		private static Func<KeyValuePair<int, ItemIngredientsTierData>, int> _003C_003Ef__am_0024cacheB;

		[CompilerGenerated]
		private static Func<KeyValuePair<ResourceType, float>, bool> _003C_003Ef__am_0024cacheC;

		[CompilerGenerated]
		private static Func<KeyValuePair<ResourceType, float>, ResourceType> _003C_003Ef__am_0024cacheD;

		[CompilerGenerated]
		private static Func<KeyValuePair<ResourceType, float>, float> _003C_003Ef__am_0024cacheE;

		[CompilerGenerated]
		private static Func<KeyValuePair<int, ItemIngredientsTierData>, int> _003C_003Ef__am_0024cacheF;

		[CompilerGenerated]
		private static Func<KeyValuePair<ResourceType, float>, bool> _003C_003Ef__am_0024cache10;

		[CompilerGenerated]
		private static Func<KeyValuePair<ResourceType, float>, ResourceType> _003C_003Ef__am_0024cache11;

		[CompilerGenerated]
		private static Func<KeyValuePair<ResourceType, float>, float> _003C_003Ef__am_0024cache12;

		[CompilerGenerated]
		private static Func<ItemIngredientsData, bool> _003C_003Ef__am_0024cache13;

		[CompilerGenerated]
		private static Func<SceneQuestTrigger, bool> _003C_003Ef__am_0024cache14;

		[CompilerGenerated]
		private static Func<SceneQuestTrigger, bool> _003C_003Ef__am_0024cache15;

		[CompilerGenerated]
		private static Func<SceneQuestTrigger, bool> _003C_003Ef__am_0024cache16;

		[CompilerGenerated]
		private static Func<KeyValuePair<int, VesselRepairPoint>, VesselRepairPoint> _003C_003Ef__am_0024cache17;

		[CompilerGenerated]
		private static Func<RaycastHit, float> _003C_003Ef__am_0024cache18;

		[CompilerGenerated]
		private static Func<RaycastHit, float> _003C_003Ef__am_0024cache19;

		[CompilerGenerated]
		private static Func<RaycastHit, float> _003C_003Ef__am_0024cache1A;

		[CompilerGenerated]
		private static Func<KeyValuePair<int, ItemIngredientsTierData>, bool> _003C_003Ef__am_0024cache1B;

		public Sprite Icon
		{
			get
			{
				if (this is GenericItem)
				{
					return Client.Instance.SpriteManager.GetSprite((this as GenericItem).SubType);
				}
				if (this is MachineryPart)
				{
					return Client.Instance.SpriteManager.GetSprite((this as MachineryPart).PartType);
				}
				return Client.Instance.SpriteManager.GetSprite(Type);
			}
		}

		public virtual float Quantity
		{
			get
			{
				return 0f;
			}
		}

		public virtual float MaxQuantity
		{
			get
			{
				return 0f;
			}
		}

		public IItemSlot Slot
		{
			get
			{
				return _Slot;
			}
			set
			{
				UpdateUI();
				_Slot = value;
				UpdateUI();
			}
		}

		public int Tier
		{
			get
			{
				return _Tier;
			}
			set
			{
				_Tier = value;
				ApplyTierColor();
			}
		}

		public float AuxValue
		{
			get
			{
				if (Tier < 1 || Tier > AuxValues.Length)
				{
					return 1f;
				}
				return AuxValues[Tier - 1];
			}
		}

		public float TierMultiplier
		{
			get
			{
				if (Tier < 1 || Tier > TierMultipliers.Length)
				{
					return 1f;
				}
				return TierMultipliers[Tier - 1];
			}
		}

		public long GUID
		{
			get
			{
				return DynamicObj.GUID;
			}
		}

		public bool IsInsideSpaceObject
		{
			get
			{
				return DynamicObj.IsInsideSpaceObject;
			}
		}

		public BaseSceneAttachPoint AttachPoint
		{
			get
			{
				return (!(_Slot is BaseSceneAttachPoint)) ? null : (_Slot as BaseSceneAttachPoint);
			}
			private set
			{
				Slot = value;
			}
		}

		public InventorySlot InvSlot
		{
			get
			{
				return (!(_Slot is InventorySlot)) ? null : (_Slot as InventorySlot);
			}
			private set
			{
				Slot = value;
			}
		}

		public short InvSlotID
		{
			get
			{
				return (short)((InvSlot == null) ? (-1111) : InvSlot.SlotID);
			}
		}

		public virtual Transform TipOfItem
		{
			get
			{
				return null;
			}
		}

		public virtual bool IsInvetoryEquipable
		{
			get
			{
				return false;
			}
		}

		public virtual EquipType EquipTo
		{
			get
			{
				return EquipType.Hands;
			}
		}

		public float MaxHealth
		{
			get
			{
				return _MaxHealth;
			}
			set
			{
				_MaxHealth = ((!(value < 0f)) ? value : 0f);
			}
		}

		public float Health
		{
			get
			{
				return _Health;
			}
			set
			{
				_Health = ((value > MaxHealth) ? MaxHealth : ((!(value < 0f)) ? value : 0f));
			}
		}

		public float Armor
		{
			get
			{
				return _Armor;
			}
			set
			{
				_Armor = ((!(value < 0f)) ? value : 0f);
			}
		}

		public bool Damageable
		{
			get
			{
				return _Damageable;
			}
		}

		public bool Expendable
		{
			get
			{
				return _Expendable;
			}
		}

		public bool Repairable
		{
			get
			{
				return _Repairable;
			}
		}

		public float UsageWear
		{
			get
			{
				return _UsageWear;
			}
			set
			{
				_UsageWear = ((!(value < 0f)) ? value : 0f);
			}
		}

		public string TypeName
		{
			get
			{
				if (this is GenericItem)
				{
					return (this as GenericItem).SubType.ToString();
				}
				if (this is MachineryPart)
				{
					return (this as MachineryPart).PartType.ToString();
				}
				return Type.ToString();
			}
		}

		public ItemCompoundType CompoundType
		{
			get
			{
				ItemCompoundType itemCompoundType;
				if (Type == ItemType.GenericItem)
				{
					itemCompoundType = new ItemCompoundType();
					itemCompoundType.Type = Type;
					itemCompoundType.SubType = (this as GenericItem).SubType;
					itemCompoundType.PartType = MachineryPartType.None;
					itemCompoundType.Tier = Tier;
					return itemCompoundType;
				}
				if (Type == ItemType.MachineryPart)
				{
					itemCompoundType = new ItemCompoundType();
					itemCompoundType.Type = Type;
					itemCompoundType.SubType = GenericItemSubType.None;
					itemCompoundType.PartType = (this as MachineryPart).PartType;
					itemCompoundType.Tier = Tier;
					return itemCompoundType;
				}
				itemCompoundType = new ItemCompoundType();
				itemCompoundType.Type = Type;
				itemCompoundType.SubType = GenericItemSubType.None;
				itemCompoundType.PartType = MachineryPartType.None;
				itemCompoundType.Tier = Tier;
				return itemCompoundType;
			}
		}

		public virtual void UpdateUI()
		{
			if (_Slot is BaseSceneAttachPoint)
			{
				if ((_Slot as BaseSceneAttachPoint).UI != null)
				{
					(_Slot as BaseSceneAttachPoint).UI.UpdateSlot();
				}
			}
			else if (_Slot is InventorySlot && (Slot as InventorySlot).Parent is MyPlayer && Client.Instance.CanvasManager.PlayerOverview.Inventory.gameObject.activeInHierarchy)
			{
				(_Slot as InventorySlot).UI.UpdateSlot();
			}
		}

		protected virtual void Awake()
		{
			ItemSlot[] componentsInChildren = GetComponentsInChildren<ItemSlot>();
			if (_003C_003Ef__am_0024cache0 == null)
			{
				_003C_003Ef__am_0024cache0 = _003CAwake_003Em__0;
			}
			Func<ItemSlot, short> keySelector = _003C_003Ef__am_0024cache0;
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CAwake_003Em__1;
			}
			Slots = componentsInChildren.ToDictionary(keySelector, _003C_003Ef__am_0024cache1);
			foreach (ItemSlot value in Slots.Values)
			{
				value.Parent = DynamicObj;
			}
			if (this is IBatteryConsumer)
			{
				IBatteryConsumer obj = this as IBatteryConsumer;
				Dictionary<short, ItemSlot> slots = Slots;
				if (_003C_003Ef__am_0024cache2 == null)
				{
					_003C_003Ef__am_0024cache2 = _003CAwake_003Em__2;
				}
				obj.BatterySlot = slots.FirstOrDefault(_003C_003Ef__am_0024cache2).Value;
			}
			DynamicObj = GetComponent<DynamicObject>();
			if (Type == ItemType.MachineryPart)
			{
				MachineryPart machineryPart = this as MachineryPart;
				Name = machineryPart.PartName;
				ToolTip = machineryPart.PartDescription;
			}
			else if (Type == ItemType.GenericItem)
			{
				GenericItem genericItem = this as GenericItem;
				Name = genericItem.ItemName;
				ToolTip = genericItem.ItemDescription;
			}
			else
			{
				Name = Type.ToLocalizedString();
				Localization.ItemsDescriptions.TryGetValue(Type, out ToolTip);
				if (ToolTip == null)
				{
					ToolTip = Name;
				}
			}
			if (ItemAnimations != null)
			{
				tpsAnimations = ItemAnimations.TPS;
				fpsAnimations = ItemAnimations.FPS;
			}
		}

		protected virtual void Start()
		{
			explosionRaycastMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("DynamicObject")) | (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("RepairTriggers"));
		}

		public virtual void AttachToObject(InventorySlot slot, bool sendAttachMessage)
		{
			InventorySlot.AttachData attachPoint = slot.GetAttachPoint(this);
			if (attachPoint != null)
			{
				AttachToObjectImpl(slot.Parent, attachPoint.Point.transform, slot, null, attachPoint.HideAttachedObject, sendAttachMessage);
				return;
			}
			Dbg.Error("Cannot attach item to slot because there is no attach point for it", base.name, slot.SlotID);
		}

		public virtual void AttachToObject(BaseSceneAttachPoint attachPoint, bool hideObject, bool sendAttachMessage)
		{
			AttachToObjectImpl(attachPoint.ParentVessel, attachPoint.GetAttachPointTransform(this), null, attachPoint, hideObject, sendAttachMessage);
		}

		public virtual void AttachToObject(ItemSlot slot, bool sendAttachMessage)
		{
			AttachToObjectImpl(slot.Parent, slot.ItemPlacement, slot, null, slot.ItemPlacement == slot.transform, sendAttachMessage);
		}

		public virtual void AttachToObject(SpaceObject obj, bool sendAttachMessage)
		{
			if (obj != null && (obj is Pivot || obj is SpaceObjectVessel))
			{
				AttachToObjectImpl(obj, null, null, null, false, sendAttachMessage);
				return;
			}
			Dbg.Error("Cannot attach item to object", base.name, GUID, obj, obj.GUID);
		}

		public virtual void AttachToBone(Player pl, AnimatorHelper.HumanBones bone, bool resetTransform = true, bool hide = false)
		{
			OnAttach(true, true);
			DynamicObj.transform.parent = pl.AnimHelper.GetBone(bone);
			DynamicObj.ToggleKinematic(true);
			DynamicObj.ToggleActive(!hide);
			DynamicObj.ToggleEnabled(false, true);
			if (resetTransform)
			{
				DynamicObj.transform.Reset(true);
			}
			else
			{
				DynamicObj.transform.localScale = Vector3.one;
			}
		}

		protected virtual void AttachToObjectImpl(SpaceObject obj, Transform attachToTrans, IItemSlot slot, BaseSceneAttachPoint attachPoint, bool hideObject, bool sendAttachMessage)
		{
			_003CAttachToObjectImpl_003Ec__AnonStorey1 _003CAttachToObjectImpl_003Ec__AnonStorey = new _003CAttachToObjectImpl_003Ec__AnonStorey1();
			_003CAttachToObjectImpl_003Ec__AnonStorey.attachToTrans = attachToTrans;
			_003CAttachToObjectImpl_003Ec__AnonStorey.obj = obj;
			_003CAttachToObjectImpl_003Ec__AnonStorey.attachPoint = attachPoint;
			_003CAttachToObjectImpl_003Ec__AnonStorey.slot = slot;
			_003CAttachToObjectImpl_003Ec__AnonStorey.hideObject = hideObject;
			_003CAttachToObjectImpl_003Ec__AnonStorey._0024this = this;
			bool flag = DynamicObj.Parent is Player && (DynamicObj.Parent as Player).Inventory.ItemInHands == this;
			Task task = new Task(_003CAttachToObjectImpl_003Ec__AnonStorey._003C_003Em__0);
			if (flag && _003CAttachToObjectImpl_003Ec__AnonStorey.slot == null && _003CAttachToObjectImpl_003Ec__AnonStorey.attachPoint == null && DynamicObj.Parent is MyPlayer)
			{
				MyPlayer.Instance.AnimHelper.SetDropTask(task);
			}
			else
			{
				task.RunSynchronously();
			}
			if (sendAttachMessage)
			{
				DynamicObj.SendStatsMessage(new DynamicObjectAttachData
				{
					IsAttached = DynamicObj.IsAttached,
					ParentGUID = DynamicObj.Parent.GUID,
					ParentType = DynamicObj.Parent.Type,
					InventorySlotID = InvSlotID,
					APDetails = ((!(AttachPoint == null)) ? new AttachPointDetails
					{
						InSceneID = AttachPoint.InSceneID
					} : null),
					LocalPosition = ((!DynamicObj.IsAttached) ? base.transform.localPosition.ToArray() : null),
					LocalRotation = ((!DynamicObj.IsAttached) ? base.transform.localPosition.ToArray() : null),
					Velocity = ((!sendVelocity.HasValue) ? null : sendVelocity.Value.ToArray()),
					Torque = ((!sendTorque.HasValue) ? null : sendTorque.Value.ToArray()),
					ThrowForce = ((!sendThrowForce.HasValue) ? null : sendThrowForce.Value.ToArray())
				});
				sendVelocity = null;
				sendTorque = null;
				sendThrowForce = null;
			}
		}

		public bool AreAttachDataSame(DynamicObjectAttachData data)
		{
			SpaceObject spaceObject = ((Slot is InventorySlot && InvSlot.Outfit != null) ? InvSlot.Outfit.DynamicObj.Parent : ((!(Slot is ItemSlot)) ? DynamicObj.Parent : Slot.Parent));
			return spaceObject.Type == data.ParentType && spaceObject.GUID == data.ParentGUID && Slot is ItemSlot && (Slot as ItemSlot).ID == data.ItemSlotID && DynamicObj.IsAttached == data.IsAttached && InvSlotID == data.InventorySlotID && ((AttachPoint == null && data.APDetails == null) || (AttachPoint != null && data.APDetails != null && AttachPoint.InSceneID == data.APDetails.InSceneID));
		}

		public void ProcessAttachData(DynamicObjectAttachData data, SpaceObject prevParent = null)
		{
			_003CProcessAttachData_003Ec__AnonStorey2 _003CProcessAttachData_003Ec__AnonStorey = new _003CProcessAttachData_003Ec__AnonStorey2();
			_003CProcessAttachData_003Ec__AnonStorey.data = data;
			_003CProcessAttachData_003Ec__AnonStorey.prevParent = prevParent;
			_003CProcessAttachData_003Ec__AnonStorey._0024this = this;
			SpaceObject @object = Client.Instance.GetObject(_003CProcessAttachData_003Ec__AnonStorey.data.ParentGUID, _003CProcessAttachData_003Ec__AnonStorey.data.ParentType);
			if (@object == null)
			{
				Dbg.Error("Could not find space object to attach item to.", base.name, _003CProcessAttachData_003Ec__AnonStorey.data.ParentGUID, _003CProcessAttachData_003Ec__AnonStorey.data.ParentType);
				return;
			}
			if (@object is OtherPlayer)
			{
				DynamicObj.Master = false;
				DynamicObj.ToggleKinematic(true);
			}
			if (Slot != null && Slot is ItemSlot && Slot.Item == this)
			{
				(Slot as ItemSlot).RemoveItem();
			}
			if (this is Outfit && (InvSlotID != _003CProcessAttachData_003Ec__AnonStorey.data.InventorySlotID || DynamicObj.Parent != @object))
			{
				if (_003CProcessAttachData_003Ec__AnonStorey.data.InventorySlotID == -2)
				{
					if (@object is MyPlayer)
					{
						(this as Outfit).EquipOutfit(MyPlayer.Instance, false);
					}
					else if (@object is OtherPlayer)
					{
						(@object as OtherPlayer).EquipOutfit(this as Outfit);
					}
					else if (@object is Corpse)
					{
						(@object as Corpse).EquipOutfit(this as Outfit);
					}
					return;
				}
				if (InvSlotID == -2)
				{
					if (DynamicObj.Parent is MyPlayer)
					{
						(this as Outfit).TakeOffOutfit(MyPlayer.Instance, false);
					}
					else if (DynamicObj.Parent is OtherPlayer && (DynamicObj.Parent as OtherPlayer).Inventory.Outfit == this)
					{
						(DynamicObj.Parent as OtherPlayer).TakeOffOutfit();
					}
					else if (DynamicObj.Parent is Corpse && (DynamicObj.Parent as Corpse).Inventory.Outfit == this)
					{
						(DynamicObj.Parent as Corpse).TakeOffOutfit();
					}
				}
			}
			if (_003CProcessAttachData_003Ec__AnonStorey.data.InventorySlotID != -1111 && _003CProcessAttachData_003Ec__AnonStorey.data.InventorySlotID != -2)
			{
				InventorySlot inventorySlot = null;
				if (@object is Player)
				{
					inventorySlot = (@object as Player).Inventory.GetSlotByID(_003CProcessAttachData_003Ec__AnonStorey.data.InventorySlotID);
				}
				else if (@object is DynamicObject && (@object as DynamicObject).Item is Outfit)
				{
					inventorySlot = ((@object as DynamicObject).Item as Outfit).GetSlotByID(_003CProcessAttachData_003Ec__AnonStorey.data.InventorySlotID);
				}
				else if (@object is Corpse && (@object as Corpse).Inventory != null)
				{
					inventorySlot = (@object as Corpse).Inventory.GetSlotByID(_003CProcessAttachData_003Ec__AnonStorey.data.InventorySlotID);
				}
				if (inventorySlot != null)
				{
					inventorySlot.SetItem(this, false);
					return;
				}
			}
			if (_003CProcessAttachData_003Ec__AnonStorey.data.APDetails != null && @object is SpaceObjectVessel)
			{
				BaseSceneAttachPoint structureObject = (@object as SpaceObjectVessel).GetStructureObject<BaseSceneAttachPoint>(_003CProcessAttachData_003Ec__AnonStorey.data.APDetails.InSceneID);
				if (structureObject != null)
				{
					AttachToObject(structureObject, false, false);
					return;
				}
			}
			if (@object is DynamicObject && (@object as DynamicObject).Item != null)
			{
				ItemSlot value;
				if ((@object as DynamicObject).Item.Slots.Count > 0 && (@object as DynamicObject).Item.Slots.TryGetValue(_003CProcessAttachData_003Ec__AnonStorey.data.ItemSlotID, out value))
				{
					AttachToObject(value, false);
				}
				(@object as DynamicObject).Item.UpdateUI();
				return;
			}
			AttachToObject(@object, false);
			_003CProcessAttachData_003Ec__AnonStorey.myPlayerIsParent = DynamicObj.Parent is MyPlayer;
			Task task = new Task(_003CProcessAttachData_003Ec__AnonStorey._003C_003Em__0);
			if (DynamicObj.Parent is MyPlayer && MyPlayer.Instance.AnimHelper.DropTask != null)
			{
				MyPlayer.Instance.AnimHelper.AfterDropTask = task;
			}
			else
			{
				task.RunSynchronously();
			}
		}

		public void ToggleTriggersEnabled(bool value)
		{
		}

		public void ResetRoomTriggers()
		{
			DynamicObj.ResetRoomTriggers();
		}

		public virtual void AddForce(Vector3 force, ForceMode forceMode)
		{
			DynamicObj.AddForce(force, forceMode);
		}

		public void AddTorque(Vector3 torque, ForceMode forceMode)
		{
			DynamicObj.AddTorque(torque, forceMode);
		}

		public virtual bool PrimaryFunction()
		{
			return false;
		}

		public virtual bool SecondaryFunction()
		{
			return false;
		}

		public virtual void Reload(Item newMagazine)
		{
		}

		public virtual void Special()
		{
		}

		public virtual void ChangeQuantity(float amount)
		{
		}

		public virtual void ChangeEquip(EquipType type, Player pl)
		{
		}

		public virtual void ProcesStatsData(DynamicObjectStats dos)
		{
			prevQuantity = Quantity;
			if (dos.Tier.HasValue)
			{
				Tier = dos.Tier.Value;
			}
			if (dos.Armor.HasValue)
			{
				Armor = dos.Armor.Value;
			}
			if (dos.MaxHealth.HasValue)
			{
				MaxHealth = dos.MaxHealth.Value;
			}
			if (dos.Health.HasValue)
			{
				Health = dos.Health.Value;
				if (Health <= 0f && DestructAt0HP)
				{
					Explode();
				}
			}
			if (dos.Damages != null)
			{
				TakeDamage(dos.Damages);
			}
			IItemSlot slot = Slot;
			Inventory inventory = MyPlayer.Instance.Inventory;
			if (slot == ((inventory != null) ? inventory.HandsSlot : null) || (DynamicObj.Parent is DynamicObject && (DynamicObj.Parent as DynamicObject).Item.Slot == MyPlayer.Instance.Inventory.HandsSlot))
			{
				Client.Instance.CanvasManager.CanvasUI.HelmetHud.HandsSlotUpdate();
			}
			if (Slot is BaseSceneAttachPoint && MyPlayer.Instance.IsLockedToTrigger && MyPlayer.Instance.LockedToTrigger is SceneTriggerCargoPanel)
			{
				StartCoroutine(CheckQuantityCoroutine());
			}
		}

		private IEnumerator CheckQuantityCoroutine()
		{
			yield return null;
			if (Quantity < prevQuantity)
			{
				SceneQuestTrigger.Check((Slot as BaseSceneAttachPoint).gameObject, SceneQuestTriggerEvent.DecreaseQuantity);
			}
			else if (Quantity > prevQuantity)
			{
				SceneQuestTrigger.Check((Slot as BaseSceneAttachPoint).gameObject, SceneQuestTriggerEvent.IncreaseQuantity);
			}
		}

		public virtual bool ProcessSlotChange(Inventory inv, InventorySlot mySlot, InventorySlot nextSlot)
		{
			return false;
		}

		public virtual DynamicObjectAuxData GetAuxData()
		{
			return null;
		}

		public T GetBaseAuxData<T>() where T : DynamicObjectAuxData
		{
			List<ItemSlotData> list = new List<ItemSlotData>();
			ItemSlot[] componentsInChildren = GetComponentsInChildren<ItemSlot>(true);
			foreach (ItemSlot itemSlot in componentsInChildren)
			{
				list.Add(itemSlot.GetData());
			}
			DynamicObjectAuxData dynamicObjectAuxData = Activator.CreateInstance(typeof(T)) as DynamicObjectAuxData;
			dynamicObjectAuxData.Category = Category;
			dynamicObjectAuxData.ItemType = Type;
			dynamicObjectAuxData.MaxHealth = MaxHealth;
			dynamicObjectAuxData.Health = Health;
			dynamicObjectAuxData.Armor = Armor;
			dynamicObjectAuxData.Damageable = Damageable;
			dynamicObjectAuxData.Repairable = Repairable;
			dynamicObjectAuxData.UsageWear = UsageWear;
			dynamicObjectAuxData.MeleeDamage = MeleeDamage;
			dynamicObjectAuxData.Tier = Tier;
			dynamicObjectAuxData.TierMultipliers = TierMultipliers;
			dynamicObjectAuxData.AuxValues = AuxValues;
			dynamicObjectAuxData.Slots = list;
			dynamicObjectAuxData.ExplosionDamage = ExplosionDamage;
			dynamicObjectAuxData.ExplosionDamageType = ExplosionDamageType;
			dynamicObjectAuxData.ExplosionRadius = ExplosionRadius;
			return dynamicObjectAuxData as T;
		}

		public ItemIngredientsData GetIngredientsData()
		{
			ItemIngredientsData itemIngredientsData = new ItemIngredientsData();
			itemIngredientsData.SubType = GenericItemSubType.None;
			itemIngredientsData.PartType = MachineryPartType.None;
			ItemIngredientsData itemIngredientsData2 = itemIngredientsData;
			itemIngredientsData2.Type = Type;
			if (this is GenericItem)
			{
				itemIngredientsData2.SubType = (this as GenericItem).SubType;
				itemIngredientsData2.Name = itemIngredientsData2.SubType.ToString();
			}
			else if (this is MachineryPart)
			{
				itemIngredientsData2.PartType = (this as MachineryPart).PartType;
				itemIngredientsData2.Name = itemIngredientsData2.PartType.ToString();
			}
			else
			{
				itemIngredientsData2.Name = Type.ToString();
			}
			itemIngredientsData2.IngredientsTiers = new Dictionary<int, ItemIngredientsTierData>();
			for (int i = 0; i < Ingredients.Length; i++)
			{
				ItemIngredientLists itemIngredientLists = Ingredients[i];
				ItemIngredientsTierData itemIngredientsTierData = new ItemIngredientsTierData();
				itemIngredientsTierData.Craft = null;
				itemIngredientsTierData.Recycle = null;
				ItemIngredientsTierData itemIngredientsTierData2 = itemIngredientsTierData;
				if (itemIngredientLists.Recycle.Length > 0)
				{
					itemIngredientsTierData2.Recycle = new Dictionary<ResourceType, float>();
					ItemIngredient[] recycle = itemIngredientLists.Recycle;
					if (_003C_003Ef__am_0024cache3 == null)
					{
						_003C_003Ef__am_0024cache3 = _003CGetIngredientsData_003Em__3;
					}
					foreach (ItemIngredient item in recycle.Where(_003C_003Ef__am_0024cache3))
					{
						itemIngredientsTierData2.Recycle[item.ResourceType] = item.Quantity;
					}
				}
				if (itemIngredientLists.Craft.Length > 0)
				{
					itemIngredientsTierData2.Craft = new Dictionary<ResourceType, float>();
					ItemIngredient[] craft = itemIngredientLists.Craft;
					if (_003C_003Ef__am_0024cache4 == null)
					{
						_003C_003Ef__am_0024cache4 = _003CGetIngredientsData_003Em__4;
					}
					foreach (ItemIngredient item2 in craft.Where(_003C_003Ef__am_0024cache4))
					{
						itemIngredientsTierData2.Craft[item2.ResourceType] = item2.Quantity;
					}
				}
				itemIngredientsData2.IngredientsTiers[i + 1] = itemIngredientsTierData2;
			}
			return itemIngredientsData2;
		}

		public virtual bool CustomCollidereToggle(bool isEnabled)
		{
			return false;
		}

		public virtual bool CanReloadOnInteract(Item item)
		{
			return false;
		}

		public virtual void PrimaryReleased()
		{
		}

		public virtual void ReloadStepComplete(Player pl, AnimatorHelper.ReloadStepType reloadStepType, ref Item currentReloadItem, ref Item newReloadItem)
		{
		}

		public virtual void TriggerItemAnimation(AnimatorHelper.ReloadType rType, string triggerName, string blendTreeParamName)
		{
		}

		public virtual void MakeHitEffect(RaycastHit info, BulletsGoThrough.HitEffectType hitType)
		{
		}

		public virtual void GetReloadType(out ItemType type, out bool getHighestQuantity)
		{
			type = ItemType.None;
			getHighestQuantity = true;
		}

		public void DropThrow(Vector3 dropPosition, Vector3 velocity, Vector3 throwForce)
		{
			if (!(DynamicObj.Parent is MyPlayer) && (!(DynamicObj.Parent is DynamicObject) || !(DynamicObj.Parent.Parent is MyPlayer)))
			{
				Dbg.Error("Cannot drop/throw item if parent is not player", DynamicObj, DynamicObj.Parent, DynamicObj.Parent.Parent);
				return;
			}
			base.transform.position = dropPosition;
			ResetRoomTriggers();
			ToggleTriggersEnabled(true);
			Vector3 vector = new Vector3(UnityEngine.Random.Range(0.001f, 0.01f), UnityEngine.Random.Range(0.001f, 0.01f), UnityEngine.Random.Range(0.001f, 0.01f));
			sendVelocity = velocity;
			sendTorque = vector;
			if (throwForce.IsNotEpsilonZero())
			{
				sendThrowForce = throwForce;
			}
			base.transform.localRotation = Quaternion.identity;
			if (MyPlayer.Instance.Parent is Pivot)
			{
				DynamicObj.ExitVessel(true);
			}
			else
			{
				AttachToObject(MyPlayer.Instance.Parent, true);
			}
			AddForce(velocity, ForceMode.VelocityChange);
			AddTorque(vector, ForceMode.Impulse);
			if (throwForce.IsNotEpsilonZero())
			{
				AddForce(throwForce, ForceMode.Impulse);
			}
			if (ThrowSound != null)
			{
				ThrowSound.Play();
			}
		}

		public virtual void SetPositionOfActiveEffect()
		{
			if (DynamicObj.Parent is MyPlayer && Client.IsGameBuild && TipOfItem != null && MyPlayer.Instance.CurrentActiveItem == this)
			{
				MyPlayer.Instance.MuzzleFlashTransform.position = TipOfItem.position;
				MyPlayer.Instance.MuzzleFlashTransform.rotation = TipOfItem.rotation;
			}
		}

		public virtual void StartItemAnimation(ItemAnimationType type, InventorySlot destinationSlot, bool isDrop)
		{
			if (DynamicObj.Parent is MyPlayer)
			{
				MyPlayer myPlayer = DynamicObj.Parent as MyPlayer;
				if (type == ItemAnimationType.Equip && destinationSlot != null && destinationSlot.SlotType == InventorySlot.Type.Equip)
				{
					myPlayer.Inventory.SetAnimationItem(this, InvSlot, destinationSlot, isDrop);
					myPlayer.animHelper.SetParameterTrigger(AnimatorHelper.Triggers.EquipItem);
					myPlayer.FpsController.IsEquippingAnimationTriggered = true;
					myPlayer.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, AnimatorHelper.EquipOrDeEquip.Equip);
					AnimatorHelper animHelper = myPlayer.animHelper;
					ItemType? equipItemId = Type;
					animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, equipItemId);
				}
				else if (type == ItemAnimationType.Unequip)
				{
					myPlayer.Inventory.SetAnimationItem(this, InvSlot, destinationSlot, isDrop);
					myPlayer.animHelper.SetParameterTrigger(AnimatorHelper.Triggers.EquipItem);
					myPlayer.FpsController.IsEquippingAnimationTriggered = true;
					myPlayer.animHelper.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, AnimatorHelper.EquipOrDeEquip.DeEquip);
					AnimatorHelper animHelper2 = myPlayer.animHelper;
					ItemType? equipItemId = Type;
					animHelper2.SetParameter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, equipItemId);
				}
			}
		}

		public virtual void TakeDamage(Dictionary<TypeOfDamage, float> damages)
		{
		}

		public virtual void AttackWithItem()
		{
			SpaceObject spaceObject = ((!(MyPlayer.Instance.Parent is SpaceObjectVessel)) ? MyPlayer.Instance.Parent : (MyPlayer.Instance.Parent as SpaceObjectVessel).MainVessel);
			ShotData shotData = new ShotData();
			shotData.Position = (Quaternion.LookRotation(spaceObject.Forward, spaceObject.Up) * MyPlayer.Instance.FpsController.MainCamera.transform.position).ToArray();
			shotData.Orientation = (Quaternion.LookRotation(spaceObject.Forward, spaceObject.Up) * MyPlayer.Instance.FpsController.MainCamera.transform.forward.normalized).ToArray();
			shotData.parentGUID = spaceObject.GUID;
			shotData.parentType = spaceObject.Type;
			shotData.Range = MeleeRange;
			shotData.IsMeleeAttack = true;
			ShotData shotData2 = shotData;
			MyPlayer.Instance.Attack(shotData2, this, 0f, 0f);
		}

		public virtual void OnAttach(bool isAttached, bool isOnPlayer)
		{
		}

		public virtual bool CanPlayerPickUp(Player pl)
		{
			return true;
		}

		public void UpdateHealthIndicator(float value, float maxValue)
		{
			if (HealthIndicator != null && HealthIndicator.materials.Length >= MaterialIndex + 1)
			{
				float num = HealthIndicatorCurve.Evaluate((!(maxValue > 0f)) ? 0f : (value / maxValue));
				float num2;
				Color color;
				if (num < 0.5f)
				{
					num2 = Mathf.Lerp(NoHealthIntensity, MidHealthIntensity, num * 2f);
					color = Color.Lerp(NoHealthColor, MidHealthColor, num * 2f);
				}
				else
				{
					num2 = Mathf.Lerp(MidHealthIntensity, FullHealthIntensity, (num - 0.5f) * 2f);
					color = Color.Lerp(MidHealthColor, FullHealthColor, (num - 0.5f) * 2f);
				}
				Material material = HealthIndicator.materials[MaterialIndex];
				if (material.shader.name.StartsWith("Standard"))
				{
					material.SetColor("_EmissionColor", color * num2);
					return;
				}
				material.SetFloat("_EmissionAmount", num2);
				material.SetColor("_EmColor", color);
			}
		}

		public void ApplyTierColor()
		{
			int? num = ((TierColors != null) ? new int?(TierColors.Length) : null);
			if (num.HasValue && num.GetValueOrDefault() > 0)
			{
				Color value = Color.white;
				if (Tier > 0 && Tier <= TierColors.Length)
				{
					value = TierColors[Tier - 1];
				}
				List<Renderer> list = GetComponentsInChildren<Renderer>(true).Where(_003CApplyTierColor_003Em__5).ToList();
				if (this is Outfit)
				{
					if (DynamicObj.Parent is MyPlayer && MyPlayer.Instance.CurrentOutfit == this)
					{
						Renderer[] componentsInChildren = MyPlayer.Instance.Outfit.GetComponentsInChildren<Renderer>(true);
						if (_003C_003Ef__am_0024cache5 == null)
						{
							_003C_003Ef__am_0024cache5 = _003CApplyTierColor_003Em__6;
						}
						list.AddRange(componentsInChildren.Where(_003C_003Ef__am_0024cache5).ToList());
						Renderer[] componentsInChildren2 = MyPlayer.Instance.CurrentOutfit.FoldedOutfitTrans.GetComponentsInChildren<Renderer>(true);
						if (_003C_003Ef__am_0024cache6 == null)
						{
							_003C_003Ef__am_0024cache6 = _003CApplyTierColor_003Em__7;
						}
						list.AddRange(componentsInChildren2.Where(_003C_003Ef__am_0024cache6).ToList());
						Renderer[] componentsInChildren3 = MyPlayer.Instance.CurrentOutfit.OutfitTrans.GetComponentsInChildren<Renderer>(true);
						if (_003C_003Ef__am_0024cache7 == null)
						{
							_003C_003Ef__am_0024cache7 = _003CApplyTierColor_003Em__8;
						}
						list.AddRange(componentsInChildren3.Where(_003C_003Ef__am_0024cache7).ToList());
					}
					else if (DynamicObj.Parent is OtherPlayer && (DynamicObj.Parent as OtherPlayer).tpsController.CurrentOutfit == this)
					{
						Renderer[] componentsInChildren4 = (DynamicObj.Parent as OtherPlayer).tpsController.Outfit.GetComponentsInChildren<Renderer>(true);
						if (_003C_003Ef__am_0024cache8 == null)
						{
							_003C_003Ef__am_0024cache8 = _003CApplyTierColor_003Em__9;
						}
						list.AddRange(componentsInChildren4.Where(_003C_003Ef__am_0024cache8).ToList());
						Renderer[] componentsInChildren5 = (DynamicObj.Parent as OtherPlayer).tpsController.CurrentOutfit.FoldedOutfitTrans.GetComponentsInChildren<Renderer>(true);
						if (_003C_003Ef__am_0024cache9 == null)
						{
							_003C_003Ef__am_0024cache9 = _003CApplyTierColor_003Em__A;
						}
						list.AddRange(componentsInChildren5.Where(_003C_003Ef__am_0024cache9).ToList());
						Renderer[] componentsInChildren6 = (DynamicObj.Parent as OtherPlayer).tpsController.CurrentOutfit.OutfitTrans.GetComponentsInChildren<Renderer>(true);
						if (_003C_003Ef__am_0024cacheA == null)
						{
							_003C_003Ef__am_0024cacheA = _003CApplyTierColor_003Em__B;
						}
						list.AddRange(componentsInChildren6.Where(_003C_003Ef__am_0024cacheA).ToList());
					}
				}
				foreach (Renderer item in list.Distinct())
				{
					Material material = item.material;
					if (material.shader.name.StartsWith("Standard"))
					{
						material.SetColor("_Color", value);
					}
					else
					{
						material.SetColor(ShaderProperty, value);
					}
				}
			}
			int? num2 = ((TierGameObjects != null) ? new int?(TierGameObjects.Length) : null);
			if (!num2.HasValue || num2.GetValueOrDefault() <= 0)
			{
				return;
			}
			for (int i = 0; i < TierGameObjects.Length; i++)
			{
				GameObject gameObject = TierGameObjects[i];
				if (gameObject != null)
				{
					if (i == Tier - 1)
					{
						gameObject.Activate(true);
					}
					else
					{
						UnityEngine.Object.Destroy(gameObject);
					}
				}
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawIcon(base.transform.position, "Item");
		}

		public static Dictionary<ResourceType, float> GetRecycleResources(Item item)
		{
			if (item is GenericItem)
			{
				return GetRecycleResources(item.Type, (item as GenericItem).SubType, MachineryPartType.None, item.Tier);
			}
			if (item is MachineryPart)
			{
				return GetRecycleResources(item.Type, GenericItemSubType.None, (item as MachineryPart).PartType, item.Tier);
			}
			return GetRecycleResources(item.Type, GenericItemSubType.None, MachineryPartType.None, item.Tier);
		}

		public static Dictionary<ResourceType, float> GetRecycleResources(ItemType itemType, GenericItemSubType subType, MachineryPartType partType, int tier)
		{
			_003CGetRecycleResources_003Ec__AnonStorey3 _003CGetRecycleResources_003Ec__AnonStorey = new _003CGetRecycleResources_003Ec__AnonStorey3();
			_003CGetRecycleResources_003Ec__AnonStorey.itemType = itemType;
			_003CGetRecycleResources_003Ec__AnonStorey.subType = subType;
			_003CGetRecycleResources_003Ec__AnonStorey.partType = partType;
			_003CGetRecycleResources_003Ec__AnonStorey.tier = tier;
			ItemIngredientsData itemIngredientsData = Client.Instance.ItemsIngredients.FirstOrDefault(_003CGetRecycleResources_003Ec__AnonStorey._003C_003Em__0);
			if (itemIngredientsData != null)
			{
				Dictionary<int, ItemIngredientsTierData> ingredientsTiers = itemIngredientsData.IngredientsTiers;
				if (_003C_003Ef__am_0024cacheB == null)
				{
					_003C_003Ef__am_0024cacheB = _003CGetRecycleResources_003Em__C;
				}
				KeyValuePair<int, ItemIngredientsTierData>? keyValuePair = ingredientsTiers.OrderBy(_003C_003Ef__am_0024cacheB).Reverse().FirstOrDefault(_003CGetRecycleResources_003Ec__AnonStorey._003C_003Em__1);
				if (keyValuePair.HasValue && keyValuePair.Value.Value.Recycle != null && keyValuePair.Value.Value.Recycle.Count > 0)
				{
					Dictionary<ResourceType, float> recycle = keyValuePair.Value.Value.Recycle;
					if (_003C_003Ef__am_0024cacheC == null)
					{
						_003C_003Ef__am_0024cacheC = _003CGetRecycleResources_003Em__D;
					}
					IEnumerable<KeyValuePair<ResourceType, float>> source = recycle.Where(_003C_003Ef__am_0024cacheC);
					if (_003C_003Ef__am_0024cacheD == null)
					{
						_003C_003Ef__am_0024cacheD = _003CGetRecycleResources_003Em__E;
					}
					Func<KeyValuePair<ResourceType, float>, ResourceType> keySelector = _003C_003Ef__am_0024cacheD;
					if (_003C_003Ef__am_0024cacheE == null)
					{
						_003C_003Ef__am_0024cacheE = _003CGetRecycleResources_003Em__F;
					}
					return source.ToDictionary(keySelector, _003C_003Ef__am_0024cacheE);
				}
			}
			return null;
		}

		public static Dictionary<ResourceType, float> GetCraftingResources(ItemIngredientsData item, int tier)
		{
			return GetCraftingResources(item.Type, item.SubType, item.PartType, tier);
		}

		public static Dictionary<ResourceType, float> GetCraftingResources(ItemCompoundType item)
		{
			return GetCraftingResources(item.Type, item.SubType, item.PartType, item.Tier);
		}

		public static Dictionary<ResourceType, float> GetCraftingResources(ItemType itemType, GenericItemSubType subType, MachineryPartType partType, int tier)
		{
			_003CGetCraftingResources_003Ec__AnonStorey4 _003CGetCraftingResources_003Ec__AnonStorey = new _003CGetCraftingResources_003Ec__AnonStorey4();
			_003CGetCraftingResources_003Ec__AnonStorey.itemType = itemType;
			_003CGetCraftingResources_003Ec__AnonStorey.subType = subType;
			_003CGetCraftingResources_003Ec__AnonStorey.partType = partType;
			_003CGetCraftingResources_003Ec__AnonStorey.tier = tier;
			ItemIngredientsData itemIngredientsData = Client.Instance.ItemsIngredients.FirstOrDefault(_003CGetCraftingResources_003Ec__AnonStorey._003C_003Em__0);
			if (itemIngredientsData != null)
			{
				Dictionary<int, ItemIngredientsTierData> ingredientsTiers = itemIngredientsData.IngredientsTiers;
				if (_003C_003Ef__am_0024cacheF == null)
				{
					_003C_003Ef__am_0024cacheF = _003CGetCraftingResources_003Em__10;
				}
				KeyValuePair<int, ItemIngredientsTierData>? keyValuePair = ingredientsTiers.OrderBy(_003C_003Ef__am_0024cacheF).Reverse().FirstOrDefault(_003CGetCraftingResources_003Ec__AnonStorey._003C_003Em__1);
				if (keyValuePair.HasValue && keyValuePair.Value.Value.Craft != null && keyValuePair.Value.Value.Craft.Count > 0)
				{
					Dictionary<ResourceType, float> craft = keyValuePair.Value.Value.Craft;
					if (_003C_003Ef__am_0024cache10 == null)
					{
						_003C_003Ef__am_0024cache10 = _003CGetCraftingResources_003Em__11;
					}
					IEnumerable<KeyValuePair<ResourceType, float>> source = craft.Where(_003C_003Ef__am_0024cache10);
					if (_003C_003Ef__am_0024cache11 == null)
					{
						_003C_003Ef__am_0024cache11 = _003CGetCraftingResources_003Em__12;
					}
					Func<KeyValuePair<ResourceType, float>, ResourceType> keySelector = _003C_003Ef__am_0024cache11;
					if (_003C_003Ef__am_0024cache12 == null)
					{
						_003C_003Ef__am_0024cache12 = _003CGetCraftingResources_003Em__13;
					}
					return source.ToDictionary(keySelector, _003C_003Ef__am_0024cache12);
				}
			}
			return null;
		}

		public static List<ItemIngredientsData> GetCraftableItems()
		{
			List<ItemIngredientsData> itemsIngredients = Client.Instance.ItemsIngredients;
			if (_003C_003Ef__am_0024cache13 == null)
			{
				_003C_003Ef__am_0024cache13 = _003CGetCraftableItems_003Em__14;
			}
			return itemsIngredients.Where(_003C_003Ef__am_0024cache13).ToList();
		}

		public static string GetName(ItemCompoundType item)
		{
			return GetName(item.Type, item.SubType, item.PartType);
		}

		public static string GetName(ItemType itemType, GenericItemSubType subType, MachineryPartType partType, bool localized = true)
		{
			switch (itemType)
			{
			case ItemType.GenericItem:
				if (localized)
				{
					return subType.ToLocalizedString();
				}
				return subType.ToString();
			case ItemType.MachineryPart:
				if (localized)
				{
					return partType.ToLocalizedString();
				}
				return partType.ToString();
			default:
				if (localized)
				{
					return itemType.ToLocalizedString();
				}
				return itemType.ToString();
			}
		}

		public static string GetDescription(ItemIngredientsData item)
		{
			return GetDescription(item.Type, item.SubType, item.PartType);
		}

		public static string GetDescription(ItemCompoundType compoundType)
		{
			return GetDescription(compoundType.Type, compoundType.SubType, compoundType.PartType);
		}

		public static string GetDescription(ItemType itemType, GenericItemSubType subType, MachineryPartType partType)
		{
			string value = string.Empty;
			switch (itemType)
			{
			case ItemType.GenericItem:
				Localization.GenericItemsDescriptions.TryGetValue(subType, out value);
				break;
			case ItemType.MachineryPart:
				Localization.MachineryPartsDescriptions.TryGetValue(partType, out value);
				break;
			default:
				Localization.ItemsDescriptions.TryGetValue(itemType, out value);
				break;
			}
			return value ?? string.Empty;
		}

		public void RequestPickUp(bool handsFirst = false)
		{
			if (Slot is BaseSceneAttachPoint && (Slot as BaseSceneAttachPoint).SceneQuestTriggers != null)
			{
				List<SceneQuestTrigger> sceneQuestTriggers = (Slot as BaseSceneAttachPoint).SceneQuestTriggers;
				if (_003C_003Ef__am_0024cache14 == null)
				{
					_003C_003Ef__am_0024cache14 = _003CRequestPickUp_003Em__15;
				}
				SceneQuestTrigger sceneQuestTrigger = sceneQuestTriggers.FirstOrDefault(_003C_003Ef__am_0024cache14);
				if (sceneQuestTrigger != null && sceneQuestTrigger.QuestTrigger.Status != QuestStatus.Active && sceneQuestTrigger.QuestTrigger.Status != QuestStatus.Completed)
				{
					return;
				}
			}
			InventorySlot inventorySlot = null;
			InventorySlot slotByID = MyPlayer.Instance.Inventory.GetSlotByID(-2);
			inventorySlot = ((!(this is Outfit) || slotByID == null || !(slotByID.Item == null) || !(MyPlayer.Instance.Inventory.HandsSlot.Item == null)) ? MyPlayer.Instance.Inventory.FindEmptyOutfitSlot(this) : slotByID);
			if (MyPlayer.Instance.Inventory.HandsSlot.Item == null && (handsFirst || inventorySlot == null))
			{
				inventorySlot = MyPlayer.Instance.Inventory.HandsSlot;
			}
			if (inventorySlot != null)
			{
				DynamicObj.SendAttachMessage(MyPlayer.Instance, inventorySlot);
				if (inventorySlot.SlotGroup == InventorySlot.Group.Hands)
				{
					MyPlayer.Instance.ItemAddedToHands(this);
				}
			}
		}

		public void RequestDrop(float throwStrength = 0f)
		{
			if (Slot is BaseSceneAttachPoint && (Slot as BaseSceneAttachPoint).SceneQuestTriggers != null)
			{
				List<SceneQuestTrigger> sceneQuestTriggers = (Slot as BaseSceneAttachPoint).SceneQuestTriggers;
				if (_003C_003Ef__am_0024cache15 == null)
				{
					_003C_003Ef__am_0024cache15 = _003CRequestDrop_003Em__16;
				}
				SceneQuestTrigger sceneQuestTrigger = sceneQuestTriggers.FirstOrDefault(_003C_003Ef__am_0024cache15);
				if (sceneQuestTrigger != null && sceneQuestTrigger.QuestTrigger.Status != QuestStatus.Active && sceneQuestTrigger.QuestTrigger.Status != QuestStatus.Completed)
				{
					return;
				}
			}
			Vector3 value = MyPlayer.Instance.transform.parent.InverseTransformPoint(MyPlayer.Instance.FpsController.MainCamera.transform.position + MyPlayer.Instance.FpsController.MainCamera.transform.forward);
			Vector3 vector = MyPlayer.Instance.FpsController.CameraForward * MathHelper.ProportionalValue(throwStrength, 0f, Client.DROP_MAX_TIME, 0f, Client.DROP_MAX_FORCE);
			Vector3 value2 = ((!(vector == Vector3.zero)) ? new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f)) : Vector3.zero);
			DynamicObj.SendAttachMessage(MyPlayer.Instance.Parent, null, value, Quaternion.identity, vector, value2, MyPlayer.Instance.rigidBody.velocity);
			if (DynamicObj.Parent is MyPlayer)
			{
				MyPlayer.Instance.AnimHelper.SetParameterTrigger(AnimatorHelper.Triggers.Drop);
			}
		}

		public void RequestAttach(IItemSlot slot)
		{
			_003CRequestAttach_003Ec__AnonStorey5 _003CRequestAttach_003Ec__AnonStorey = new _003CRequestAttach_003Ec__AnonStorey5();
			if (slot is InventorySlot && (slot as InventorySlot).SlotType == InventorySlot.Type.Hands && (slot as InventorySlot).Inventory.Parent is MyPlayer)
			{
				((slot as InventorySlot).Inventory.Parent as MyPlayer).ChangeStance(MyPlayer.PlayerStance.Passive, 1f);
			}
			if (Slot is BaseSceneAttachPoint && (Slot as BaseSceneAttachPoint).SceneQuestTriggers != null)
			{
				List<SceneQuestTrigger> sceneQuestTriggers = (Slot as BaseSceneAttachPoint).SceneQuestTriggers;
				if (_003C_003Ef__am_0024cache16 == null)
				{
					_003C_003Ef__am_0024cache16 = _003CRequestAttach_003Em__17;
				}
				SceneQuestTrigger sceneQuestTrigger = sceneQuestTriggers.FirstOrDefault(_003C_003Ef__am_0024cache16);
				if (sceneQuestTrigger != null && sceneQuestTrigger.QuestTrigger.Status != QuestStatus.Active && sceneQuestTrigger.QuestTrigger.Status != QuestStatus.Completed)
				{
					return;
				}
			}
			_003CRequestAttach_003Ec__AnonStorey.swapItem = slot.Item;
			IItemSlot slot2 = Slot;
			if (slot.Item != null && slot2 != null)
			{
				if (!slot2.CanFitItem(_003CRequestAttach_003Ec__AnonStorey.swapItem) && slot2.Parent is DynamicObject)
				{
					ItemSlot itemSlot = (slot2.Parent as DynamicObject).Item.Slots.Values.FirstOrDefault(_003CRequestAttach_003Ec__AnonStorey._003C_003Em__0);
					if (itemSlot == null)
					{
						if (MyPlayer.Instance.Inventory.CanAddToInventory(_003CRequestAttach_003Ec__AnonStorey.swapItem))
						{
							_003CRequestAttach_003Ec__AnonStorey.swapItem.RequestPickUp();
						}
						else
						{
							_003CRequestAttach_003Ec__AnonStorey.swapItem.RequestDrop();
						}
					}
				}
				else
				{
					_003CRequestAttach_003Ec__AnonStorey.swapItem.DynamicObj.SendAttachMessage(slot2.Parent, slot2);
				}
			}
			else if (_003CRequestAttach_003Ec__AnonStorey.swapItem != null && slot2 == null)
			{
				if (MyPlayer.Instance.Inventory.CanAddToInventory(_003CRequestAttach_003Ec__AnonStorey.swapItem))
				{
					_003CRequestAttach_003Ec__AnonStorey.swapItem.RequestPickUp();
				}
				else
				{
					_003CRequestAttach_003Ec__AnonStorey.swapItem.RequestDrop();
				}
			}
			DynamicObj.SendAttachMessage(slot.Parent, slot);
		}

		public virtual string GetInfo()
		{
			return string.Empty;
		}

		public virtual string QuantityCheck()
		{
			return null;
		}

		public void CalculateRecycle()
		{
			for (int i = 0; i < ((TierMultipliers.Length < 1) ? 1 : TierMultipliers.Length); i++)
			{
				Ingredients[i].Recycle = ObjectCopier.DeepCopy(Ingredients[i].Craft);
				ItemIngredient[] recycle = Ingredients[i].Recycle;
				foreach (ItemIngredient ii in recycle)
				{
					ApplyRecMulty(ii);
				}
			}
		}

		private static void ApplyRecMulty(ItemIngredient ii)
		{
			if (ii.ResourceType == ResourceType.NickelIron)
			{
				ii.Quantity *= 0.25f;
			}
			else if (ii.ResourceType == ResourceType.Silicates)
			{
				ii.Quantity *= 0.2f;
			}
			else if (ii.ResourceType == ResourceType.Titanium)
			{
				ii.Quantity *= 0.15f;
			}
			else if (ii.ResourceType == ResourceType.RareElements)
			{
				ii.Quantity *= 0.1f;
			}
			else if (ii.ResourceType == ResourceType.CarbonFibers)
			{
				ii.Quantity *= 0.25f;
			}
			else if (ii.ResourceType == ResourceType.Alloys)
			{
				ii.Quantity *= 0.2f;
			}
			else if (ii.ResourceType == ResourceType.Circuits)
			{
				ii.Quantity *= 0.15f;
			}
			else if (ii.ResourceType == ResourceType.Superconductors)
			{
				ii.Quantity *= 0.1f;
			}
		}

		public virtual bool Explode()
		{
			if (exploded)
			{
				return false;
			}
			HashSet<long> hashSet = new HashSet<long>();
			HashSet<VesselObjectID> hashSet2 = new HashSet<VesselObjectID>();
			if (ExplosionRadius > 0f)
			{
				Collider[] array = Physics.OverlapSphere(base.transform.position, ExplosionRadius);
				Collider[] array2 = array;
				foreach (Collider colliderOverlaped in array2)
				{
					if (!CheckDamageables(hashSet, colliderOverlaped) && !CheckPlayers(hashSet, colliderOverlaped))
					{
						CheckRepairPoints(hashSet2, colliderOverlaped);
					}
				}
				if (DynamicObj.Parent is SpaceObjectVessel)
				{
					IOrderedEnumerable<KeyValuePair<int, VesselRepairPoint>> source = (DynamicObj.Parent as SpaceObjectVessel).RepairPoints.Where(_003CExplode_003Em__18).OrderBy(_003CExplode_003Em__19);
					if (_003C_003Ef__am_0024cache17 == null)
					{
						_003C_003Ef__am_0024cache17 = _003CExplode_003Em__1A;
					}
					foreach (VesselRepairPoint item in source.Select(_003C_003Ef__am_0024cache17))
					{
						hashSet2.Add(new VesselObjectID(item.ParentVessel.GUID, item.InSceneID));
					}
				}
			}
			Client.Instance.NetworkController.SendToGameServer(new ExplosionMessage
			{
				AffectedGUIDs = hashSet.ToArray(),
				ItemGUID = GUID,
				RepairPointIDs = hashSet2.ToArray()
			});
			if (ExplosionEffects != null)
			{
				ExplosionEffects.Play();
				ExplosionEffects.transform.parent = null;
			}
			if (ExplosionSound != null)
			{
				ExplosionSound.Play();
			}
			base.gameObject.Activate(false);
			exploded = true;
			if (Slot != null)
			{
				if (Slot is BaseSceneAttachPoint)
				{
					(Slot as BaseSceneAttachPoint).DetachItem(this);
				}
				else if (Slot is InventorySlot)
				{
					(Slot as InventorySlot).SetItem(null);
					if ((Slot as InventorySlot).SlotGroup == InventorySlot.Group.Hands)
					{
						(Slot as InventorySlot).Inventory.ExitCombatStance();
					}
				}
				else if (Slot is ItemSlot)
				{
					(Slot as ItemSlot).RemoveItem();
				}
			}
			return true;
		}

		private bool CheckDamageables(HashSet<long> hitGUIDs, Collider colliderOverlaped)
		{
			Item componentInParent = colliderOverlaped.GetComponentInParent<Item>();
			if (componentInParent != null && componentInParent != this && (componentInParent.AttachPoint == null || componentInParent.Damageable))
			{
				foreach (Vector3 target in GetTargets(colliderOverlaped.transform.position, 0.1f))
				{
					Debug.DrawRay(base.transform.position, target - base.transform.position, Color.green, 100f);
					RaycastHit[] source = Physics.RaycastAll(base.transform.position, target - base.transform.position, ExplosionRadius, explosionRaycastMask, QueryTriggerInteraction.Collide);
					if (_003C_003Ef__am_0024cache18 == null)
					{
						_003C_003Ef__am_0024cache18 = _003CCheckDamageables_003Em__1B;
					}
					RaycastHit[] array = source.OrderBy(_003C_003Ef__am_0024cache18).ToArray();
					RaycastHit[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						RaycastHit raycastHit = array2[i];
						Item componentInParent2 = raycastHit.collider.GetComponentInParent<Item>();
						if (componentInParent2 == componentInParent)
						{
							if (componentInParent.Damageable)
							{
								hitGUIDs.Add(componentInParent.GUID);
							}
							Vector3 vector = base.transform.position - target;
							colliderOverlaped.GetComponentInParent<DynamicObject>().AddForce(vector.normalized * (1f - ExplosionRadius / vector.magnitude) * ExplosionImpulse, ForceMode.Impulse);
							return true;
						}
						if (!raycastHit.collider.isTrigger)
						{
							break;
						}
					}
				}
			}
			return false;
		}

		private bool CheckPlayers(HashSet<long> hitGUIDs, Collider colliderOverlaped)
		{
			Player componentInParent = colliderOverlaped.GetComponentInParent<Player>();
			if (componentInParent != null)
			{
				TargetingPoint[] componentsInChildren = componentInParent.GetComponentsInChildren<TargetingPoint>();
				TargetingPoint[] array = componentsInChildren;
				foreach (TargetingPoint targetingPoint in array)
				{
					RaycastHit[] source = Physics.RaycastAll(base.transform.position, targetingPoint.transform.position - base.transform.position, ExplosionRadius, explosionRaycastMask, QueryTriggerInteraction.Collide);
					if (_003C_003Ef__am_0024cache19 == null)
					{
						_003C_003Ef__am_0024cache19 = _003CCheckPlayers_003Em__1C;
					}
					RaycastHit[] array2 = source.OrderBy(_003C_003Ef__am_0024cache19).ToArray();
					RaycastHit[] array3 = array2;
					for (int j = 0; j < array3.Length; j++)
					{
						RaycastHit raycastHit = array3[j];
						if (raycastHit.collider.GetComponentInParent<Player>() != null)
						{
							hitGUIDs.Add(componentInParent.GUID);
							if (componentInParent is MyPlayer && ExplosionDamageType == TypeOfDamage.Impact && !MyPlayer.Instance.IsLockedToTrigger)
							{
								if (!MyPlayer.Instance.FpsController.IsZeroG && !MyPlayer.Instance.FpsController.HasTumbled && (!InputManager.GetButton(InputManager.AxisNames.LeftShift) || !MyPlayer.Instance.FpsController.IsGrounded))
								{
									MyPlayer.Instance.FpsController.Tumble();
								}
								Vector3 vector = base.transform.position - MyPlayer.Instance.transform.position;
								MyPlayer.Instance.FpsController.AddForce(vector.normalized * (1f - ExplosionRadius / vector.magnitude) * ExplosionImpulse, ForceMode.Impulse);
							}
							return true;
						}
						if (!raycastHit.collider.isTrigger)
						{
							break;
						}
					}
				}
			}
			return false;
		}

		private void CheckRepairPoints(HashSet<VesselObjectID> rpts, Collider colliderOverlaped)
		{
			VesselRepairPoint componentInParent = colliderOverlaped.GetComponentInParent<VesselRepairPoint>();
			if (!(componentInParent != null) || !(componentInParent.Health > float.Epsilon))
			{
				return;
			}
			foreach (Vector3 target in GetTargets(colliderOverlaped.transform.position, 0.35f))
			{
				RaycastHit[] source = Physics.RaycastAll(base.transform.position, target - base.transform.position, ExplosionRadius, explosionRaycastMask, QueryTriggerInteraction.Collide);
				if (_003C_003Ef__am_0024cache1A == null)
				{
					_003C_003Ef__am_0024cache1A = _003CCheckRepairPoints_003Em__1D;
				}
				RaycastHit[] array = source.OrderBy(_003C_003Ef__am_0024cache1A).ToArray();
				RaycastHit[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					RaycastHit raycastHit = array2[i];
					if (raycastHit.collider.GetComponentInParent<VesselRepairPoint>() == componentInParent)
					{
						rpts.Add(new VesselObjectID(componentInParent.ParentVessel.GUID, componentInParent.InSceneID));
					}
					else if (!raycastHit.collider.isTrigger)
					{
						break;
					}
				}
			}
		}

		private List<Vector3> GetTargets(Vector3 targetPos, float radius)
		{
			List<Vector3> list = new List<Vector3>();
			Quaternion quaternion = Quaternion.FromToRotation(Vector3.forward, (targetPos - base.transform.position).normalized);
			Quaternion quaternion2 = Quaternion.Euler(0f, 0f, MathHelper.RandomRange(0, 90));
			list.Add(targetPos);
			list.Add(targetPos + quaternion * quaternion2 * new Vector3(radius, radius, 0f));
			list.Add(targetPos + quaternion * quaternion2 * new Vector3(radius, 0f - radius, 0f));
			list.Add(targetPos + quaternion * quaternion2 * new Vector3(0f - radius, radius, 0f));
			list.Add(targetPos + quaternion * quaternion2 * new Vector3(0f - radius, 0f - radius, 0f));
			return list;
		}

		[CompilerGenerated]
		private static short _003CAwake_003Em__0(ItemSlot k)
		{
			return k.ID;
		}

		[CompilerGenerated]
		private static ItemSlot _003CAwake_003Em__1(ItemSlot v)
		{
			return v;
		}

		[CompilerGenerated]
		private static bool _003CAwake_003Em__2(KeyValuePair<short, ItemSlot> m)
		{
			return m.Value.ItemTypes.Contains(ItemType.AltairHandDrillBattery);
		}

		[CompilerGenerated]
		private static bool _003CGetIngredientsData_003Em__3(ItemIngredient m)
		{
			return m.Quantity > 0f && m.ResourceType != ResourceType.None;
		}

		[CompilerGenerated]
		private static bool _003CGetIngredientsData_003Em__4(ItemIngredient m)
		{
			return m.Quantity > 0f && m.ResourceType != ResourceType.None;
		}

		[CompilerGenerated]
		private bool _003CApplyTierColor_003Em__5(Renderer m)
		{
			return m.GetComponentInParent<Item>() == this;
		}

		[CompilerGenerated]
		private static bool _003CApplyTierColor_003Em__6(Renderer m)
		{
			return m.GetComponentInParent<Item>() == null;
		}

		[CompilerGenerated]
		private static bool _003CApplyTierColor_003Em__7(Renderer m)
		{
			return m.GetComponentInParent<Item>() == null;
		}

		[CompilerGenerated]
		private static bool _003CApplyTierColor_003Em__8(Renderer m)
		{
			return m.GetComponentInParent<Item>() == null;
		}

		[CompilerGenerated]
		private static bool _003CApplyTierColor_003Em__9(Renderer m)
		{
			return m.GetComponentInParent<Item>() == null;
		}

		[CompilerGenerated]
		private static bool _003CApplyTierColor_003Em__A(Renderer m)
		{
			return m.GetComponentInParent<Item>() == null;
		}

		[CompilerGenerated]
		private static bool _003CApplyTierColor_003Em__B(Renderer m)
		{
			return m.GetComponentInParent<Item>() == null;
		}

		[CompilerGenerated]
		private static int _003CGetRecycleResources_003Em__C(KeyValuePair<int, ItemIngredientsTierData> m)
		{
			return m.Key;
		}

		[CompilerGenerated]
		private static bool _003CGetRecycleResources_003Em__D(KeyValuePair<ResourceType, float> m)
		{
			return m.Key != 0 && m.Value > 0f;
		}

		[CompilerGenerated]
		private static ResourceType _003CGetRecycleResources_003Em__E(KeyValuePair<ResourceType, float> k)
		{
			return k.Key;
		}

		[CompilerGenerated]
		private static float _003CGetRecycleResources_003Em__F(KeyValuePair<ResourceType, float> v)
		{
			return v.Value;
		}

		[CompilerGenerated]
		private static int _003CGetCraftingResources_003Em__10(KeyValuePair<int, ItemIngredientsTierData> m)
		{
			return m.Key;
		}

		[CompilerGenerated]
		private static bool _003CGetCraftingResources_003Em__11(KeyValuePair<ResourceType, float> m)
		{
			return m.Key != 0 && m.Value > 0f;
		}

		[CompilerGenerated]
		private static ResourceType _003CGetCraftingResources_003Em__12(KeyValuePair<ResourceType, float> k)
		{
			return k.Key;
		}

		[CompilerGenerated]
		private static float _003CGetCraftingResources_003Em__13(KeyValuePair<ResourceType, float> v)
		{
			return v.Value;
		}

		[CompilerGenerated]
		private static bool _003CGetCraftableItems_003Em__14(ItemIngredientsData m)
		{
			int result;
			if (m.IngredientsTiers != null)
			{
				Dictionary<int, ItemIngredientsTierData> ingredientsTiers = m.IngredientsTiers;
				if (_003C_003Ef__am_0024cache1B == null)
				{
					_003C_003Ef__am_0024cache1B = _003CGetCraftableItems_003Em__1E;
				}
				result = ((ingredientsTiers.Count(_003C_003Ef__am_0024cache1B) > 0) ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}

		[CompilerGenerated]
		private static bool _003CRequestPickUp_003Em__15(SceneQuestTrigger m)
		{
			return m.TriggerEvent == SceneQuestTriggerEvent.DetachItem;
		}

		[CompilerGenerated]
		private static bool _003CRequestDrop_003Em__16(SceneQuestTrigger m)
		{
			return m.TriggerEvent == SceneQuestTriggerEvent.DetachItem;
		}

		[CompilerGenerated]
		private static bool _003CRequestAttach_003Em__17(SceneQuestTrigger m)
		{
			return m.TriggerEvent == SceneQuestTriggerEvent.DetachItem;
		}

		[CompilerGenerated]
		private bool _003CExplode_003Em__18(KeyValuePair<int, VesselRepairPoint> m)
		{
			return (base.transform.position - m.Value.transform.position).magnitude <= ExplosionRadius;
		}

		[CompilerGenerated]
		private float _003CExplode_003Em__19(KeyValuePair<int, VesselRepairPoint> m)
		{
			return (base.transform.position - m.Value.transform.position).magnitude;
		}

		[CompilerGenerated]
		private static VesselRepairPoint _003CExplode_003Em__1A(KeyValuePair<int, VesselRepairPoint> kvp)
		{
			return kvp.Value;
		}

		[CompilerGenerated]
		private static float _003CCheckDamageables_003Em__1B(RaycastHit m)
		{
			return m.distance;
		}

		[CompilerGenerated]
		private static float _003CCheckPlayers_003Em__1C(RaycastHit m)
		{
			return m.distance;
		}

		[CompilerGenerated]
		private static float _003CCheckRepairPoints_003Em__1D(RaycastHit m)
		{
			return m.distance;
		}

		[CompilerGenerated]
		private static bool _003CGetCraftableItems_003Em__1E(KeyValuePair<int, ItemIngredientsTierData> n)
		{
			return n.Value.Recycle != null && n.Value.Recycle.Count > 0;
		}
	}
}
