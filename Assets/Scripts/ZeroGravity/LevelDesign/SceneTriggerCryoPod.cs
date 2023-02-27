using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.LevelDesign
{
	public class SceneTriggerCryoPod : BaseSceneTrigger
	{
		[SerializeField]
		private bool _cancelExecuterAtSameTime;

		[SerializeField]
		private bool _isExteriorTrigger;

		public SceneSpawnPoint SpawnPoint;

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
				return SceneTriggerType.CryoPodPanel;
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

		public CryoPodUI MyCryoPod
		{
			get
			{
				return Client.Instance.InGamePanels.Cryo;
			}
		}

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!base.Interact(player, interactWithOverlappingTriggers))
			{
				return false;
			}
			MyCryoPod.SpawnPoint = SpawnPoint;
			if (player.CurrentActiveItem != null)
			{
				if (player.CurrentActiveItem is DisposableHackingTool)
				{
					MyCryoPod.SpawnPoint.HackSpawnPoint();
				}
				else if (player.Inventory == null || !player.Inventory.StoreItemInHands())
				{
					return false;
				}
			}
			player.AttachToPanel(this);
			MyCryoPod.ToggleCanvas(true);
			return true;
		}

		public override void CancelInteract(MyPlayer player)
		{
			base.CancelInteract(player);
			player.DetachFromPanel();
			MyCryoPod.ToggleCanvas(false);
		}
	}
}
