using System.Collections.Generic;
using UnityEngine.Serialization;
using ZeroGravity.Data;
using ZeroGravity.Objects;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerDockingPanel : BaseSceneTrigger
	{
		[FormerlySerializedAs("Executer")] public SceneTriggerExecutor Executor;

		public string ExecuterInteractAction;

		public string ExecuterCancelAction;

		public override bool ExclusivePlayerLocking => true;

		public override SceneTriggerType TriggerType => SceneTriggerType.DockingPanel;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.StoreItemInHands;

		public override List<ItemType> PlayerHandsItemType => null;

		public override bool IsNearTrigger => true;

		public override bool IsInteractable => true;

		public override bool CameraMovementAllowed => true;

		public DockingPanel MyDockingPanel => World.InWorldPanels.Docking;

		public Ship MyParentShip => GetComponentInParent<GeometryRoot>().MainObject as Ship;

		private void Awake()
		{
			StandardTip = Localization.StandardInteractionTip.Docking;
			if (CheckAuthorizationType == AuthorizationType.None)
			{
				CheckAuthorizationType = AuthorizationType.AuthorizedOrNoSecurity;
			}
		}

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!CheckAuthorization())
			{
				World.InGameGUI.Alert(Localization.UnauthorizedAccess.ToUpper());
				return false;
			}

			if (!base.Interact(player, interactWithOverlappingTriggers))
			{
				return false;
			}

			if (Executor != null && !ExecuterInteractAction.IsNullOrEmpty())
			{
				Executor.ChangeState(ExecuterInteractAction);
			}

			if (interactWithOverlappingTriggers)
			{
				SceneTriggerHelper.InteractWithOverlappingTriggers(gameObject, this, player);
			}

			MyDockingPanel.OnInteract(MyParentShip);
			player.AttachToPanel(this, false);
			return true;
		}

		public override void CancelInteract(MyPlayer player)
		{
			base.CancelInteract(player);
			if (Executor != null && !ExecuterCancelAction.IsNullOrEmpty())
			{
				Executor.ChangeState(ExecuterCancelAction);
			}

			MyDockingPanel.OnDetach();
			player.DetachFromPanel();
		}
	}
}
