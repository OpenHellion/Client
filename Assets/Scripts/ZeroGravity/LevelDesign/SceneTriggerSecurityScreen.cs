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
		public override bool ExclusivePlayerLocking => true;

		public override SceneTriggerType TriggerType => SceneTriggerType.SecurityScreen;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.DontCheck;

		public override List<ItemType> PlayerHandsItemType => null;

		public override bool IsNearTrigger => true;

		public override bool IsInteractable => true;

		public override bool CameraMovementAllowed => false;

		public SecurityScreen MySecurityScreen => World.InWorldPanels.Security;

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			MySecurityScreen.SecuritySystem = GetComponent<SecuritySystem>();
			if (!CheckAuthorization() && player.CurrentActiveItem != null &&
			    ItemTypeRange.IsHackingTool(player.CurrentActiveItem.Type))
			{
				MySecurityScreen.SecuritySystem.Hack();
			}
			else
			{
				if (!CheckAuthorization())
				{
					World.InGameGUI.Alert(Localization.UnauthorizedAccess.ToUpper());
					return false;
				}

				if (player.CurrentActiveItem != null &&
				    (player.Inventory == null || !player.Inventory.StoreItemInHands()))
				{
					return false;
				}
			}

			if (interactWithOverlappingTriggers)
			{
				SceneTriggerHelper.InteractWithOverlappingTriggers(gameObject, this, player);
			}

			player.AttachToPanel(this);
			MySecurityScreen.OnInteract();
			return true;
		}

		public override bool CheckAuthorization()
		{
			return IsAuthorized || (IsAuthorizedOrFreeSecurity && !ParentShip.SecurityPanelsLocked);
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
