using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using ZeroGravity.Data;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerCargoPanel : BaseSceneTrigger
	{
		[FormerlySerializedAs("_cancelExecuterAtSameTime")] [SerializeField] private bool _cancelExecutorAtSameTime;

		[SerializeField] private bool _isExteriorTrigger;

		private CargoPanel cargoPanel;

		public override bool ExclusivePlayerLocking => true;

		public override SceneTriggerType TriggerType => SceneTriggerType.CargoPanel;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.StoreItemInHands;

		public override List<ItemType> PlayerHandsItemType => null;

		public override bool IsNearTrigger => true;

		public override bool IsInteractable => true;

		public bool CancelExecutorAtSameTime => _cancelExecutorAtSameTime;

		public bool IsExteriorTrigger => _isExteriorTrigger;

		public override bool CameraMovementAllowed => false;

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
