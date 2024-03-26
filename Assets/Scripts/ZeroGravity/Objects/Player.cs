using System;
using OpenHellion.Social.RichPresence;
using UnityEngine;
using ZeroGravity.LevelDesign;

namespace ZeroGravity.Objects
{
	public abstract class Player : SpaceObjectTransferable
	{
		[NonSerialized] public string PlayerName;

		[NonSerialized] public string PlayerId;

		[NonSerialized] public Inventory Inventory;

		public virtual bool IsLockedToTrigger => LockedToTrigger is not null;

		public virtual BaseSceneTrigger LockedToTrigger { get; set; }

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
			get => base.Parent;
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

		public static Texture GetAvatar(string playerId)
		{
			return Resources.Load<Texture2D>("UI/default_avatar");
		}
	}
}
