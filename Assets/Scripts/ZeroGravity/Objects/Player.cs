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

		private Texture _Avatar;

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
				if (_Avatar == null)
				{
					return GetAvatar(SteamId);
				}
				return _Avatar;
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
			for (int i = 0; i < 20; i++)
			{
				int largeFriendAvatar = SteamFriends.GetLargeFriendAvatar(new CSteamID(ulong.Parse(SteamID)));
				uint pnWidth;
				uint pnHeight;
				if (SteamUtils.GetImageSize(largeFriendAvatar, out pnWidth, out pnHeight) && pnWidth != 0 && pnHeight != 0)
				{
					byte[] array = new byte[pnWidth * pnHeight * 4];
					Texture2D texture2D = new Texture2D((int)pnWidth, (int)pnHeight, TextureFormat.RGBA32, false, false);
					if (SteamUtils.GetImageRGBA(largeFriendAvatar, array, (int)(pnWidth * pnHeight * 4)))
					{
						texture2D.LoadRawTextureData(array);
						texture2D.Apply();
					}
					return texture2D;
				}
			}
			return Resources.Load<Texture2D>("UI/default_avatar");
		}
	}
}
