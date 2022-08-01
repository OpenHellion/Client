using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;
using ZeroGravity.UI;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerSecurityScreen : BaseSceneTrigger
	{
		[SerializeField]
		private bool _cancelExecuterAtSameTime;

		private SecurityScreen mySecurityScreen;

		public override bool ExclusivePlayerLocking
		{
			get
			{
				return true;
			}
		}

		public override SceneTriggerType TriggerType
		{
			get
			{
				return SceneTriggerType.SecurityScreen;
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

		public override bool IsNearTrigger
		{
			get
			{
				return true;
			}
		}

		public override bool IsInteractable
		{
			get
			{
				return true;
			}
		}

		public bool CancelExecuterAtSameTime
		{
			get
			{
				return _cancelExecuterAtSameTime;
			}
		}

		public override bool CameraMovementAllowed
		{
			get
			{
				return false;
			}
		}

		public SecurityScreen MySecurityScreen
		{
			get
			{
				if (mySecurityScreen == null)
				{
					mySecurityScreen = Client.Instance.InGamePanels.Security;
				}
				return mySecurityScreen;
			}
		}

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			MySecurityScreen.SecuritySystem = GetComponent<SecuritySystem>();
			if (!CheckAuthorization() && player.CurrentActiveItem != null && ItemTypeRange.IsHackingTool(player.CurrentActiveItem.Type))
			{
				MySecurityScreen.SecuritySystem.Hack();
			}
			else
			{
				if (!CheckAuthorization())
				{
					Client.Instance.CanvasManager.CanvasUI.Alert(Localization.UnauthorizedAccess.ToUpper());
					return false;
				}
				if (player.CurrentActiveItem != null && (player.Inventory == null || !player.Inventory.StoreItemInHands()))
				{
					return false;
				}
			}
			if (interactWithOverlappingTriggers)
			{
				SceneTriggerHelper.InteractWithOverlappingTriggers(base.gameObject, this, player);
			}
			player.AttachToPanel(this);
			MySecurityScreen.OnInteract();
			return true;
		}

		public override bool CheckAuthorization()
		{
			return base.IsAuthorized || (base.IsAuthorizedOrFreeSecurity && !base.ParentShip.SecurityPanelsLocked);
		}

		public override void CancelInteract(MyPlayer player)
		{
			base.CancelInteract(player);
			player.DetachFromPanel();
			MySecurityScreen.OnDetach();
		}

		public void InteractWithPanel()
		{
			Interact(MyPlayer.Instance);
		}
	}
}
