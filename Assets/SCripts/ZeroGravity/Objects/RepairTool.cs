using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.UI;

namespace ZeroGravity.Objects
{
	public class RepairTool : Item, ICargo
	{
		public float RepairAmount;

		public float UsageCooldown;

		public float Range;

		public bool UseAnimatorHelperTrigger;

		public AnimatorHelper.Triggers AnimatorHelperTrigger;

		public MyPlayer.PlayerStance ActiveStance = MyPlayer.PlayerStance.Passive;

		public float StanceChangeSpeedMultiplier = 1f;

		[Space(10f)]
		public ParticleSystem RepairEffect;

		public SoundEffect RepairSoundEffect;

		[Space(10f)]
		public CargoCompartment FuelCompartment;

		[Tooltip("Units per HP")]
		public float FuelConsumption = 0.001f;

		private AnimatorHelper animHelper;

		private float lastUsageTime;

		private bool active;

		private int raycastMask;

		private List<ICargoCompartment> _Compartments;

		[CompilerGenerated]
		private static Func<CargoResourceData, float> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Func<RaycastHit, float> _003C_003Ef__am_0024cache1;

		string ICargo.Name
		{
			get
			{
				return Localization.RepairTool;
			}
		}

		public List<ICargoCompartment> Compartments
		{
			get
			{
				return _Compartments;
			}
		}

		public SpaceObjectVessel ParentVessel
		{
			get
			{
				return null;
			}
		}

		public override float MaxQuantity
		{
			get
			{
				return FuelCompartment.Capacity;
			}
		}

		public override float Quantity
		{
			get
			{
				float result;
				if (FuelCompartment.Resources != null)
				{
					List<CargoResourceData> resources = FuelCompartment.Resources;
					if (_003C_003Ef__am_0024cache0 == null)
					{
						_003C_003Ef__am_0024cache0 = _003Cget_Quantity_003Em__0;
					}
					result = resources.Sum(_003C_003Ef__am_0024cache0);
				}
				else
				{
					result = 0f;
				}
				return result;
			}
		}

		public float ResourcePercentage
		{
			get
			{
				return Quantity / MaxQuantity;
			}
		}

		private new void Awake()
		{
			base.Awake();
			if (Client.IsGameBuild)
			{
				animHelper = MyPlayer.Instance.animHelper;
			}
			raycastMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("RepairTriggers")) | (1 << LayerMask.NameToLayer("DynamicObject")) | 1;
			_Compartments = new List<ICargoCompartment> { FuelCompartment };
		}

		public override bool PrimaryFunction()
		{
			active = true;
			MyPlayer.Instance.ChangeStance(ActiveStance, StanceChangeSpeedMultiplier);
			return true;
		}

		public override void PrimaryReleased()
		{
			active = false;
			StopRepairEffect(true);
			MyPlayer.Instance.ChangeStance(MyPlayer.PlayerStance.Passive, StanceChangeSpeedMultiplier);
		}

		private void PlayRepairEffect(bool sendStats = false)
		{
			if (!(RepairEffect != null) || !RepairEffect.isStopped)
			{
				return;
			}
			RepairEffect.Play();
			if (sendStats)
			{
				DynamicObject dynamicObj = DynamicObj;
				RepairToolStats statsData = new RepairToolStats
				{
					Active = true
				};
				dynamicObj.SendStatsMessage(null, statsData);
				if (RepairSoundEffect != null)
				{
					RepairSoundEffect.Play(0);
				}
			}
		}

		private void StopRepairEffect(bool sendStats = false)
		{
			if (!(RepairEffect != null) || !RepairEffect.isPlaying)
			{
				return;
			}
			RepairEffect.Stop();
			if (sendStats)
			{
				DynamicObject dynamicObj = DynamicObj;
				RepairToolStats statsData = new RepairToolStats
				{
					Active = false
				};
				dynamicObj.SendStatsMessage(null, statsData);
				if (RepairSoundEffect != null)
				{
					RepairSoundEffect.Play(1);
				}
			}
		}

		public override DynamicObjectAuxData GetAuxData()
		{
			RepairToolData baseAuxData = GetBaseAuxData<RepairToolData>();
			baseAuxData.UsageCooldown = UsageCooldown;
			baseAuxData.RepairAmount = RepairAmount;
			baseAuxData.Range = Range;
			baseAuxData.FuelCompartment = FuelCompartment.GetData();
			baseAuxData.FuelConsumption = FuelConsumption;
			return baseAuxData;
		}

		private void Update()
		{
			if (active && Type == ItemType.FireExtinguisher && FuelCompartment.Resources[0].Quantity > float.Epsilon && MyPlayer.Instance.FpsController.IsZeroG && !InputManager.GetButton(InputManager.AxisNames.LeftShift))
			{
				MyPlayer.Instance.FpsController.AddForce(-(MyPlayer.Instance.FpsController.MainCamera.transform.rotation * Vector3.forward).normalized * 0.03f, ForceMode.VelocityChange);
			}
			if (!active || Time.time - lastUsageTime < UsageCooldown || (base.InvSlot != null && !(base.InvSlot.Parent is MyPlayer)))
			{
				return;
			}
			bool flag = false;
			Vector3 position = MyPlayer.Instance.FpsController.MainCamera.transform.position;
			Vector3 direction = MyPlayer.Instance.FpsController.MainCamera.transform.rotation * Vector3.forward;
			RaycastHit[] source = Physics.RaycastAll(position, direction, Range, raycastMask, QueryTriggerInteraction.Collide);
			if (_003C_003Ef__am_0024cache1 == null)
			{
				_003C_003Ef__am_0024cache1 = _003CUpdate_003Em__1;
			}
			RaycastHit[] array = source.OrderBy(_003C_003Ef__am_0024cache1).ToArray();
			RaycastHit[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit = array2[i];
				Item componentInParent = raycastHit.collider.gameObject.GetComponentInParent<Item>();
				if (Type != ItemType.FireExtinguisher && componentInParent != null && componentInParent.Repairable && componentInParent.MaxHealth - componentInParent.Health > float.Epsilon && FuelCompartment.Resources[0].Quantity > float.Epsilon)
				{
					Client.Instance.NetworkController.SendToGameServer(new RepairItemMessage
					{
						GUID = componentInParent.GUID
					});
					flag = true;
					Client.Instance.ChangeStatsByIfNotAdmin(SteamStatID.repair_time, UsageCooldown / 60f);
					break;
				}
				VesselRepairPoint componentInParent2 = raycastHit.collider.gameObject.GetComponentInParent<VesselRepairPoint>();
				if (componentInParent2 != null && componentInParent2.MaxHealth - componentInParent2.Health > float.Epsilon && FuelCompartment.Resources[0].Quantity > float.Epsilon && (Type != ItemType.FireExtinguisher || (Type == ItemType.FireExtinguisher && componentInParent2.DamageType == RepairPointDamageType.Fire && componentInParent2.SecondaryDamageActive)))
				{
					Client.Instance.NetworkController.SendToGameServer(new RepairVesselMessage
					{
						ID = new VesselObjectID(componentInParent2.ParentVessel.GUID, componentInParent2.InSceneID)
					});
					flag = true;
					if (Type == ItemType.FireExtinguisher)
					{
						Client.Instance.ChangeStatsByIfNotAdmin(SteamStatID.firefighting_time, UsageCooldown / 60f);
					}
					else
					{
						Client.Instance.ChangeStatsByIfNotAdmin(SteamStatID.repair_time, UsageCooldown / 60f);
					}
					break;
				}
			}
			if (!flag && Type == ItemType.FireExtinguisher && FuelCompartment.Resources[0].Quantity > float.Epsilon)
			{
				Client.Instance.NetworkController.SendToGameServer(new RepairItemMessage
				{
					GUID = -1L
				});
				flag = true;
			}
			if (flag)
			{
				lastUsageTime = Time.time;
				if (UseAnimatorHelperTrigger)
				{
					animHelper.SetParameterTrigger(AnimatorHelperTrigger);
				}
				PlayRepairEffect(true);
			}
			else
			{
				StopRepairEffect(true);
			}
		}

		public override void ProcesStatsData(DynamicObjectStats dos)
		{
			base.ProcesStatsData(dos);
			RepairToolStats repairToolStats = dos as RepairToolStats;
			if (repairToolStats.Active.HasValue && base.InvSlot != null && base.InvSlot.Parent is Player && !(base.InvSlot.Parent is MyPlayer) && base.InvSlot.SlotType == InventorySlot.Type.Hands)
			{
				if (repairToolStats.Active.Value)
				{
					PlayRepairEffect();
				}
				if (!repairToolStats.Active.Value)
				{
					StopRepairEffect();
				}
			}
			if (repairToolStats.FuelResource != null)
			{
				FuelCompartment.Resources[0] = repairToolStats.FuelResource;
				UpdateHealthIndicator(repairToolStats.FuelResource.Quantity, FuelCompartment.Capacity);
			}
			if (base.AttachPoint != null && MyPlayer.Instance.IsLockedToTrigger && MyPlayer.Instance.LockedToTrigger is SceneTriggerCargoPanel)
			{
				SceneTriggerCargoPanel sceneTriggerCargoPanel = MyPlayer.Instance.LockedToTrigger as SceneTriggerCargoPanel;
				sceneTriggerCargoPanel.CargoPanel.RefreshAttachedItemResources();
			}
		}

		public ICargoCompartment GetCompartment(short? id)
		{
			throw new NotImplementedException();
		}

		public override string QuantityCheck()
		{
			return FormatHelper.Percentage(Quantity / MaxQuantity);
		}

		[CompilerGenerated]
		private static float _003Cget_Quantity_003Em__0(CargoResourceData m)
		{
			return m.Quantity;
		}

		[CompilerGenerated]
		private static float _003CUpdate_003Em__1(RaycastHit x)
		{
			return x.distance;
		}
	}
}
