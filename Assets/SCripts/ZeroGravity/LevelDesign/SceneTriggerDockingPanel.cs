using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerDockingPanel : BaseSceneTrigger
	{
		[SerializeField]
		private bool _cancelExecuterAtSameTime;

		[SerializeField]
		private bool _isExteriorTrigger = true;

		public SceneTriggerExecuter Executer;

		public string ExecuterInteractAction;

		public string ExecuterCancelAction;

		private DockingPanel myDockingPanel;

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
				return SceneTriggerType.DockingPanel;
			}
		}

		public override PlayerHandsCheckType PlayerHandsCheck
		{
			get
			{
				return PlayerHandsCheckType.StoreItemInHands;
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

		public override bool CameraMovementAllowed
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

		public bool IsExteriorTrigger
		{
			get
			{
				return _isExteriorTrigger;
			}
		}

		public DockingPanel MyDockingPanel
		{
			get
			{
				if (myDockingPanel == null)
				{
					myDockingPanel = Client.Instance.InGamePanels.Docking;
				}
				return myDockingPanel;
			}
		}

		public Ship MyParentShip
		{
			get
			{
				return GetComponentInParent<GeometryRoot>().MainObject as Ship;
			}
		}

		private void Awake()
		{
			StandardTip = Localization.StandardInteractionTip.Docking;
			if (CheckAuthorizationType == AuthorizationType.none)
			{
				CheckAuthorizationType = AuthorizationType.AuthorizedOrNoSecurity;
			}
		}

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!CheckAuthorization())
			{
				Client.Instance.CanvasManager.CanvasUI.Alert(Localization.UnauthorizedAccess.ToUpper());
				return false;
			}
			if (!base.Interact(player, interactWithOverlappingTriggers))
			{
				return false;
			}
			if (Executer != null && !ExecuterInteractAction.IsNullOrEmpty())
			{
				Executer.ChangeState(ExecuterInteractAction);
			}
			if (interactWithOverlappingTriggers)
			{
				SceneTriggerHelper.InteractWithOverlappingTriggers(base.gameObject, this, player);
			}
			MyDockingPanel.OnInteract(MyParentShip);
			player.AttachToPanel(this, false);
			return true;
		}

		public override void CancelInteract(MyPlayer player)
		{
			base.CancelInteract(player);
			if (Executer != null && !ExecuterCancelAction.IsNullOrEmpty())
			{
				Executer.ChangeState(ExecuterCancelAction);
			}
			MyDockingPanel.OnDetach();
			player.DetachFromPanel();
		}
	}
}
