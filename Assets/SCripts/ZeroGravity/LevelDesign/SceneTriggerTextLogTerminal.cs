using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerTextLogTerminal : SceneTriggerPanels
	{
		[SerializeField]
		private bool _cancelExecuterAtSameTime;

		[SerializeField]
		private bool _isExteriorTrigger;

		public LogObject LogObject;

		private TextLogTerminal terminalUI;

		public override SceneTriggerType TriggerType
		{
			get
			{
				return SceneTriggerType.TextLogTerminal;
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
				return false;
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

		public TextLogTerminal TerminalUI
		{
			get
			{
				terminalUI = Client.Instance.InGamePanels.TextLogTerminal;
				return terminalUI;
			}
		}

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!base.Interact(player, interactWithOverlappingTriggers))
			{
				return false;
			}
			if (player.CurrentActiveItem != null && (player.Inventory == null || !player.Inventory.StoreItemInHands()))
			{
				return false;
			}
			player.AttachToPanel(this);
			TerminalUI.LogObject = LogObject;
			TerminalUI.OnInteract();
			return true;
		}

		public override void CancelInteract(MyPlayer player)
		{
			player.DetachFromPanel();
			TerminalUI.OnDetach();
		}
	}
}
