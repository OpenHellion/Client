using System;
using UnityEngine;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.LevelDesign
{
	public abstract class SceneTriggerPanels : BaseSceneTrigger, IVesselSystemAccessory
	{
		[SerializeField]
		private VesselSystem _BaseVesselSystem;

		public override bool ExclusivePlayerLocking
		{
			get
			{
				return false;
			}
		}

		public VesselSystem BaseVesselSystem
		{
			get
			{
				return _BaseVesselSystem;
			}
			set
			{
				_BaseVesselSystem = value;
			}
		}

		protected override void Start()
		{
			base.Start();
			if (_BaseVesselSystem == null)
			{
				_BaseVesselSystem = GetComponentInParent<SpaceObjectVessel>().VesselBaseSystem;
			}
			_BaseVesselSystem.Accessories.Add(this);
			BaseVesselSystemUpdated();
		}

		public void BaseVesselSystemUpdated()
		{
			try
			{
				if (BaseVesselSystem.Status != SystemStatus.OnLine)
				{
				}
			}
			catch (Exception ex)
			{
				Dbg.Error(ex.Message, ex.StackTrace);
			}
		}
	}
}
