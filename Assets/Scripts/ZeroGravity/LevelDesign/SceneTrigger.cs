using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.CharacterMovement;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class SceneTrigger : BaseSceneTrigger
	{
		public int TriggerID;

		private List<ItemType> _itemInHandsTypes;

		public SceneTriggerExecuter Executer;

		private int executerStateID;

		private List<int> executerStateIDs = new List<int>();

		public string ExecuterStateName;

		private int executerAltStateID;

		public string ExecuterAltStateName;

		[SerializeField]
		private bool _isNearTrigger = true;

		[SerializeField]
		private bool _isProximityTrigger;

		[SerializeField]
		private PlayerHandsCheckType playerHandsCheck = PlayerHandsCheckType.DontCheck;

		[SerializeField]
		private ItemType itemInHandsType;

		private bool allowNextTriggerEnter = true;

		public override bool ExclusivePlayerLocking
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
				return !_isProximityTrigger;
			}
		}

		public bool IsProximity
		{
			get
			{
				return _isProximityTrigger;
			}
		}

		public override bool IsNearTrigger
		{
			get
			{
				return _isNearTrigger;
			}
		}

		public override SceneTriggerType TriggerType
		{
			get
			{
				return SceneTriggerType.General;
			}
		}

		public override PlayerHandsCheckType PlayerHandsCheck
		{
			get
			{
				return playerHandsCheck;
			}
		}

		public override bool CameraMovementAllowed
		{
			get
			{
				return false;
			}
		}

		public override List<ItemType> PlayerHandsItemType
		{
			get
			{
				if (_itemInHandsTypes == null)
				{
					_itemInHandsTypes = new List<ItemType> { itemInHandsType };
				}
				return _itemInHandsTypes;
			}
		}

		public void AllowNextTriggerEner(bool allow)
		{
			allowNextTriggerEnter = allow;
		}

		private void Awake()
		{
			if (Executer == null)
			{
				Executer = GetComponentInParent<SceneTriggerExecuter>();
			}
			if (!(Executer != null))
			{
				return;
			}
			executerStateID = -1;
			string[] array = ExecuterStateName.Split(';');
			foreach (string stateName in array)
			{
				int stateID = Executer.GetStateID(stateName);
				if (executerStateID == -1)
				{
					executerStateID = stateID;
				}
				executerStateIDs.Add(stateID);
			}
			executerAltStateID = Executer.GetStateID(ExecuterAltStateName);
		}

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!base.Interact(player, interactWithOverlappingTriggers))
			{
				return false;
			}
			if (Executer != null && (!Client.IsGameBuild || playerHandsCheck == PlayerHandsCheckType.DontCheck || (playerHandsCheck == PlayerHandsCheckType.HandsMustBeEmpty && player.Inventory.ItemInHands == null) || (playerHandsCheck == PlayerHandsCheckType.StoreItemInHands && player.Inventory.StoreItemInHands()) || (playerHandsCheck == PlayerHandsCheckType.MustHaveItemInHands && player.Inventory.ItemInHands.Type == itemInHandsType)))
			{
				Executer.ChangeStateID((!executerStateIDs.Contains(Executer.CurrentStateID)) ? executerStateID : executerAltStateID);
				if (Client.IsGameBuild)
				{
					Item itemInHands = player.Inventory.ItemInHands;
					if (playerHandsCheck == PlayerHandsCheckType.MustHaveItemInHands && itemInHands != null && itemInHands.Type == ItemType.AltairDisposableHackingTool)
					{
						Executer.GetComponentInParent<SceneDoor>().ChangeStats(false);
						(itemInHands as DisposableHackingTool).Special();
					}
				}
			}
			if (interactWithOverlappingTriggers)
			{
				SceneTriggerHelper.InteractWithOverlappingTriggers(base.gameObject, this, player);
			}
			return true;
		}

		public override void CancelInteract(MyPlayer player)
		{
			base.CancelInteract(player);
			if (!(Executer == null))
			{
			}
		}

		protected override void OnTriggerEnter(Collider coli)
		{
			base.OnTriggerEnter(coli);
			if (!IsProximity || Executer == null)
			{
				return;
			}
			TransitionTriggerHelper component = coli.GetComponent<TransitionTriggerHelper>();
			if (component != null && component.TransferableObject is MyPlayer)
			{
				if (!allowNextTriggerEnter)
				{
					allowNextTriggerEnter = true;
				}
				else
				{
					Executer.PlayerEnterTrigger(this, component.TransferableObject as MyPlayer, executerStateID);
				}
			}
		}

		protected override void OnTriggerExit(Collider coli)
		{
			base.OnTriggerExit(coli);
			if (IsProximity && !(Executer == null))
			{
				TransitionTriggerHelper component = coli.GetComponent<TransitionTriggerHelper>();
				if (component != null && component.TransferableObject is MyPlayer)
				{
					Executer.PlayerExitTrigger(this, component.TransferableObject as MyPlayer, executerAltStateID);
				}
			}
		}
	}
}
