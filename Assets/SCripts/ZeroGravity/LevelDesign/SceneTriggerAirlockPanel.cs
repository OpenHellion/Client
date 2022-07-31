using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerAirlockPanel : SceneTriggerPanels
	{
		[SerializeField]
		private bool _cancelExecuterAtSameTime;

		[SerializeField]
		private bool _isExteriorTrigger;

		private AirLockControls myAirLockControls;

		private AirlockUI airlockUI;

		public override SceneTriggerType TriggerType
		{
			get
			{
				return SceneTriggerType.AirlockPanel;
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

		public AirLockControls MyAirLockControls
		{
			get
			{
				if (myAirLockControls == null)
				{
					myAirLockControls = GetComponent<AirLockControls>();
				}
				return myAirLockControls;
			}
		}

		public AirlockUI AirlockUI
		{
			get
			{
				airlockUI = Client.Instance.InGamePanels.Airlock;
				return airlockUI;
			}
		}

		private void Awake()
		{
			if (CheckAuthorizationType == AuthorizationType.none)
			{
				CheckAuthorizationType = AuthorizationType.AuthorizedOrNoSecurity;
			}
		}

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (player.CurrentActiveItem != null && (player.Inventory == null || !player.Inventory.StoreItemInHands()))
			{
				return false;
			}
			player.AttachToPanel(this);
			AirlockUI.MyAirlock = MyAirLockControls;
			AirlockUI.OnInteract();
			return true;
		}

		public override void CancelInteract(MyPlayer player)
		{
			base.CancelInteract(player);
			player.DetachFromPanel();
			AirlockUI.OnDetach();
		}
	}
}
