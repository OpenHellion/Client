using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerLifeSupportPanel : BaseSceneTrigger
	{
		[SerializeField]
		private bool _cancelExecuterAtSameTime;

		[SerializeField]
		private bool _isExteriorTrigger;

		private LifeSupportPanel myLifeSupport;

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
				return SceneTriggerType.LifeSupportPanel;
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

		public LifeSupportPanel MyLifeSupport
		{
			get
			{
				myLifeSupport = Client.Instance.InGamePanels.LifeSupport;
				return myLifeSupport;
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
			MyLifeSupport.OnInteract();
			return true;
		}

		public override void CancelInteract(MyPlayer player)
		{
			base.CancelInteract(player);
			player.DetachFromPanel();
			MyLifeSupport.OnDetach();
		}
	}
}
