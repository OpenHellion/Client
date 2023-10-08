using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerCargoPanel : BaseSceneTrigger
	{
		[SerializeField] private bool _cancelExecuterAtSameTime;

		[SerializeField] private bool _isExteriorTrigger;

		private CargoPanel cargoPanel;

		public override bool ExclusivePlayerLocking
		{
			get { return true; }
		}

		public override SceneTriggerType TriggerType
		{
			get { return SceneTriggerType.CargoPanel; }
		}

		public override PlayerHandsCheckType PlayerHandsCheck
		{
			get { return PlayerHandsCheckType.StoreItemInHands; }
		}

		public override List<ItemType> PlayerHandsItemType
		{
			get { return null; }
		}

		public override bool IsNearTrigger
		{
			get { return true; }
		}

		public override bool IsInteractable
		{
			get { return true; }
		}

		public bool CancelExecuterAtSameTime
		{
			get { return _cancelExecuterAtSameTime; }
		}

		public bool IsExteriorTrigger
		{
			get { return _isExteriorTrigger; }
		}

		public override bool CameraMovementAllowed
		{
			get { return false; }
		}

		public CargoPanel CargoPanel
		{
			get
			{
				cargoPanel = World.InWorldPanels.Cargo;
				return cargoPanel;
			}
		}

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!base.Interact(player, interactWithOverlappingTriggers))
			{
				return false;
			}

			if (interactWithOverlappingTriggers)
			{
				SceneTriggerHelper.InteractWithOverlappingTriggers(base.gameObject, this, player);
			}

			CargoPanel.AttachPoint = base.transform.parent.GetComponentInChildren<SceneResourcesTransferPoint>();
			player.AttachToPanel(this);
			CargoPanel.OnInteract();
			return true;
		}

		public override void CancelInteract(MyPlayer player)
		{
			base.CancelInteract(player);
			CargoPanel.AttachPoint = null;
			player.DetachFromPanel();
			CargoPanel.OnDetach();
		}
	}
}
