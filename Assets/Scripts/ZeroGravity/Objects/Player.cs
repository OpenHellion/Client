using OpenHellion.ProviderSystem;
using Steamworks;
using UnityEngine;
using ZeroGravity.LevelDesign;

namespace ZeroGravity.Objects
{
	public abstract class Player : SpaceObjectTransferable
	{
		public string PlayerName;

		public string SteamId;

		private BaseSceneTrigger _LockedToTrigger;

		public Inventory Inventory;

		public virtual bool IsLockedToTrigger
		{
			get
			{
				return LockedToTrigger != null;
			}
		}

		public virtual BaseSceneTrigger LockedToTrigger
		{
			get
			{
				return _LockedToTrigger;
			}
			set
			{
				_LockedToTrigger = value;
			}
		}

		public Texture Avatar
		{
			get
			{
				return ProviderManager.MainProvider.GetAvatar(SteamId);
			}
		}

		public bool IsUsingItemInHands { get; set; }

		public Item CurrentActiveItem
		{
			get
			{
				if (Inventory != null)
				{
					return Inventory.ItemInHands;
				}
				return null;
			}
		}

		public AnimatorHelper AnimHelper
		{
			get
			{
				if (this is MyPlayer)
				{
					return (this as MyPlayer).animHelper;
				}
				if (this is OtherPlayer)
				{
					return (this as OtherPlayer).tpsController.animHelper;
				}
				return null;
			}
		}

		public override SpaceObject Parent
		{
			get
			{
				return base.Parent;
			}
			set
			{
				base.Parent = value;
				SetParentTransferableObjectsRoot();
			}
		}

		public void RefreshTargetingPoints()
		{
			TargetingPoint[] componentsInChildren = GetComponentsInChildren<TargetingPoint>(true);
			foreach (TargetingPoint targetingPoint in componentsInChildren)
			{
				targetingPoint.MainObject = this;
			}
		}

		public static Texture GetAvatar(string SteamID)
		{
			return ProviderManager.MainProvider.GetAvatar(SteamID);
		}
	}
}
