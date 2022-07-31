using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerNavigationPanel : BaseSceneTrigger
	{
		[SerializeField]
		private bool _cancelExecuterAtSameTime;

		[SerializeField]
		private bool _isExteriorTrigger;

		private Ship myParentShip;

		public override bool ExclusivePlayerLocking
		{
			get
			{
				return false;
			}
		}

		public override SceneTriggerType TriggerType
		{
			get
			{
				return SceneTriggerType.NavigationPanel;
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

		public override bool CameraMovementAllowed
		{
			get
			{
				return false;
			}
		}

		public Ship MyParentShip
		{
			get
			{
				return GetComponentInParent<GeometryRoot>().MainObject as Ship;
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
			player.AttachToPanel(this);
			Client.Instance.Map.OnInteract(MyParentShip);
			return true;
		}

		public void InteractWithPanel()
		{
			Interact(MyPlayer.Instance);
		}

		public override void CancelInteract(MyPlayer player)
		{
			base.CancelInteract(player);
			player.DetachFromPanel();
			Client.Instance.Map.OnDetach();
		}
	}
}
