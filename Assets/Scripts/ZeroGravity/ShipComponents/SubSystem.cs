using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.ShipComponents
{
	public abstract class SubSystem : VesselSystem
	{
		[CompilerGenerated]
		private sealed class _003CGetResourceRequirement_003Ec__AnonStorey0
		{
			internal DistributionSystemType resourceType;

			internal bool? working;

			internal SubSystem _0024this;

			internal bool _003C_003Em__0(ResourceRequirement m)
			{
				return m.ResourceType == resourceType;
			}

			internal float _003C_003Em__1(ResourceRequirement m)
			{
				return (_0024this.SecondaryStatus != SystemSecondaryStatus.Idle) ? m.Nominal : m.Standby;
			}

			internal bool _003C_003Em__2(ResourceRequirement m)
			{
				return m.ResourceType == resourceType;
			}

			internal float _003C_003Em__3(ResourceRequirement m)
			{
				return (!working.Value) ? m.Standby : m.Nominal;
			}
		}

		[Space(5f)]
		public float OperationRate = 1f;

		public bool AutoTuneOperationRate;

		[Space(5f)]
		[SerializeField]
		[FormerlySerializedAs("triggerAnimtion")]
		protected SceneTriggerAnimation triggerAnimation;

		[Space(5f)]
		public SystemSpawnSettings[] SpawnSettings = new SystemSpawnSettings[0];

		[Space(5f)]
		[Multiline]
		public string DebugInfo;

		[NonSerialized]
		public float InputFactorStandby = 1f;

		public abstract SubSystemType Type { get; }

		public abstract ResourceRequirement[] ResourceRequirements { get; }

		public override VesselComponentType ComponentType
		{
			get
			{
				return VesselComponentType.SubSystem;
			}
		}

		public float GetPowerConsumption(bool? working = null)
		{
			return GetResourceRequirement(DistributionSystemType.Power, working);
		}

		public float GetResourceConsumption(DistributionSystemType type, bool? working = null)
		{
			return GetResourceRequirement(type, working);
		}

		public float GetResourceRequirement(DistributionSystemType resourceType, bool? working = null)
		{
			_003CGetResourceRequirement_003Ec__AnonStorey0 _003CGetResourceRequirement_003Ec__AnonStorey = new _003CGetResourceRequirement_003Ec__AnonStorey0();
			_003CGetResourceRequirement_003Ec__AnonStorey.resourceType = resourceType;
			_003CGetResourceRequirement_003Ec__AnonStorey.working = working;
			_003CGetResourceRequirement_003Ec__AnonStorey._0024this = this;
			float num;
			if (!_003CGetResourceRequirement_003Ec__AnonStorey.working.HasValue)
			{
				if (Status != SystemStatus.OnLine)
				{
					return 0f;
				}
				num = ResourceRequirements.Where(_003CGetResourceRequirement_003Ec__AnonStorey._003C_003Em__0).Sum((Func<ResourceRequirement, float>)_003CGetResourceRequirement_003Ec__AnonStorey._003C_003Em__1);
			}
			else
			{
				num = ResourceRequirements.Where(_003CGetResourceRequirement_003Ec__AnonStorey._003C_003Em__2).Sum((Func<ResourceRequirement, float>)_003CGetResourceRequirement_003Ec__AnonStorey._003C_003Em__3);
			}
			if (_003CGetResourceRequirement_003Ec__AnonStorey.resourceType == DistributionSystemType.Power)
			{
				return num * PowerInputFactor;
			}
			return num * InputFactor;
		}

		protected virtual void Awake()
		{
			if (triggerAnimation == null)
			{
				triggerAnimation = GetComponent<SceneTriggerAnimation>();
			}
		}

		protected SubSystemDetails getDetails(bool? isSwitchedOn = null)
		{
			SubSystemDetails subSystemDetails = new SubSystemDetails();
			subSystemDetails.InSceneID = base.InSceneID;
			subSystemDetails.Status = ((!isSwitchedOn.HasValue) ? Status : ((!isSwitchedOn.Value) ? SystemStatus.OffLine : SystemStatus.OnLine));
			subSystemDetails.AuxDetails = GetAuxDetails();
			return subSystemDetails;
		}

		public void SendDetails()
		{
			if (Client.IsGameBuild)
			{
				SpaceObjectVessel parentVessel = _ParentVessel;
				SubSystemDetails details = getDetails();
				parentVessel.ChangeStats(null, null, null, null, details);
			}
		}

		public virtual bool IsEventFinished()
		{
			if (triggerAnimation != null)
			{
				return triggerAnimation.IsEventFinished;
			}
			return true;
		}

		public void SetParentVessel(SpaceObjectVessel vessel)
		{
			_ParentVessel = vessel;
		}

		public virtual void SetDetails(SystemStatus status, SystemSecondaryStatus secondaryStatus, float operationRate, bool instant = false)
		{
			Status = status;
			SecondaryStatus = secondaryStatus;
			OperationRate = operationRate;
			if (triggerAnimation != null)
			{
				triggerAnimation.ChangeState(Status == SystemStatus.OnLine, instant);
			}
		}

		public virtual void SetDetails(SubSystemDetails details, bool instant = false)
		{
			SetDetails(details.Status, details.SecondaryStatus, details.OperationRate, instant);
			SetAuxDetails(details.AuxDetails);
			AutoRestart = details.AutoRestart;
			InputFactor = details.InputFactor;
			PowerInputFactor = details.PowerInputFactor;
			DebugInfo = details.DebugInfo;
		}

		public virtual void SetAuxDetails(IAuxDetails auxDetails)
		{
		}

		public override void SwitchOn()
		{
			if (Status == SystemStatus.OffLine || Status == SystemStatus.CoolDown)
			{
				if (Client.IsGameBuild)
				{
					SpaceObjectVessel parentVessel = _ParentVessel;
					SubSystemDetails details = getDetails(true);
					parentVessel.ChangeStats(null, null, null, null, details);
				}
				else
				{
					SetDetails(SystemStatus.OnLine, SystemSecondaryStatus.None, OperationRate);
				}
			}
		}

		public override void SwitchOff()
		{
			if (Status == SystemStatus.OnLine || Status == SystemStatus.PowerUp)
			{
				if (Client.IsGameBuild)
				{
					SpaceObjectVessel parentVessel = _ParentVessel;
					SubSystemDetails details = getDetails(false);
					parentVessel.ChangeStats(null, null, null, null, details);
				}
				else
				{
					SetDetails(SystemStatus.OffLine, SystemSecondaryStatus.None, OperationRate);
				}
			}
		}

		public override void Toggle()
		{
			if (Client.IsGameBuild)
			{
				SpaceObjectVessel parentVessel = _ParentVessel;
				SubSystemDetails details = getDetails(!IsSwitchedOn());
				parentVessel.ChangeStats(null, null, null, null, details);
			}
			else if (IsSwitchedOn())
			{
				SwitchOff();
			}
			else
			{
				SwitchOn();
			}
		}

		public virtual SubSystemData GetData()
		{
			if (ResourceContainers.Length > 0)
			{
				ResourceContainer[] resourceContainers = ResourceContainers;
				foreach (ResourceContainer resourceContainer in resourceContainers)
				{
					if (resourceContainer == null)
					{
						throw new Exception("Reference(s) missing in Resource Containers list.");
					}
				}
			}
			List<int> list = new List<int>();
			if (MachineryPartSlots != null)
			{
				SceneMachineryPartSlot[] machineryPartSlots = MachineryPartSlots;
				foreach (SceneMachineryPartSlot sceneMachineryPartSlot in machineryPartSlots)
				{
					list.Add(sceneMachineryPartSlot.InSceneID);
				}
			}
			List<int> list2 = new List<int>();
			if (ResourceContainers != null)
			{
				ResourceContainer[] resourceContainers2 = ResourceContainers;
				foreach (ResourceContainer resourceContainer2 in resourceContainers2)
				{
					list2.Add(resourceContainer2.InSceneID);
				}
			}
			SubSystemData subSystemData = new SubSystemData();
			subSystemData.InSceneID = base.InSceneID;
			subSystemData.Type = Type;
			subSystemData.ResourceRequirements = ResourceRequirements;
			subSystemData.SpawnSettings = SpawnSettings;
			subSystemData.Status = Status;
			subSystemData.MachineryPartSlots = list;
			subSystemData.ResourceContainers = list2;
			subSystemData.OperationRate = OperationRate;
			subSystemData.AutoTuneOperationRate = AutoTuneOperationRate;
			subSystemData.AutoReactivate = AutoReactivate;
			subSystemData.CoolDownTime = CoolDownTime;
			subSystemData.PowerUpTime = PowerUpTime;
			subSystemData.AuxData = GetAuxData();
			subSystemData.RadarSignature = RadarSignature;
			subSystemData.RoomID = ((!(Room == null)) ? Room.InSceneID : (-1));
			return subSystemData;
		}

		public virtual float GetConsumption(DistributionSystemType resourceType)
		{
			if (Status != SystemStatus.OnLine)
			{
				return 0f;
			}
			ResourceRequirement[] resourceRequirements = ResourceRequirements;
			foreach (ResourceRequirement resourceRequirement in resourceRequirements)
			{
				if (resourceRequirement.ResourceType == resourceType)
				{
					return resourceRequirement.Nominal * OperationRate * InputFactor + resourceRequirement.Standby * InputFactorStandby;
				}
			}
			return 0f;
		}
	}
}
