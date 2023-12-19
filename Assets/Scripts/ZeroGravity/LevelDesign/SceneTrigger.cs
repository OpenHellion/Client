using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using ZeroGravity.CharacterMovement;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class SceneTrigger : BaseSceneTrigger
	{
		public int TriggerID;

		private List<ItemType> _itemInHandsTypes;

		[FormerlySerializedAs("Executer")] public SceneTriggerExecutor Executor;

		private int _executorStateID;

		private readonly List<int> _executorStateIDs = new List<int>();

		[FormerlySerializedAs("ExecuterStateName")] public string ExecutorStateName;

		private int _executorAltStateID;

		[FormerlySerializedAs("ExecuterAltStateName")] public string ExecutorAltStateName;

		[SerializeField] private bool _isNearTrigger = true;

		[SerializeField] private bool _isProximityTrigger;

		[SerializeField] private PlayerHandsCheckType playerHandsCheck = PlayerHandsCheckType.DontCheck;

		[SerializeField] private ItemType itemInHandsType;

		private bool allowNextTriggerEnter = true;

		public override bool ExclusivePlayerLocking => false;

		public override bool IsInteractable => !_isProximityTrigger;

		public bool IsProximity => _isProximityTrigger;

		public override bool IsNearTrigger => _isNearTrigger;

		public override SceneTriggerType TriggerType => SceneTriggerType.General;

		public override PlayerHandsCheckType PlayerHandsCheck => playerHandsCheck;

		public override bool CameraMovementAllowed => false;

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
			if (Executor == null)
			{
				Executor = GetComponentInParent<SceneTriggerExecutor>();
			}

			if (!(Executor != null))
			{
				return;
			}

			_executorStateID = -1;
			string[] array = ExecutorStateName.Split(';');
			foreach (string stateName in array)
			{
				int stateID = Executor.GetStateID(stateName);
				if (_executorStateID == -1)
				{
					_executorStateID = stateID;
				}

				_executorStateIDs.Add(stateID);
			}

			_executorAltStateID = Executor.GetStateID(ExecutorAltStateName);
		}

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!base.Interact(player, interactWithOverlappingTriggers))
			{
				return false;
			}

			if (Executor != null && (playerHandsCheck == PlayerHandsCheckType.DontCheck ||
			                         (playerHandsCheck == PlayerHandsCheckType.HandsMustBeEmpty &&
			                          player.Inventory.ItemInHands == null) ||
			                         (playerHandsCheck == PlayerHandsCheckType.StoreItemInHands &&
			                          player.Inventory.StoreItemInHands()) ||
			                         (playerHandsCheck == PlayerHandsCheckType.MustHaveItemInHands &&
			                          player.Inventory.ItemInHands.Type == itemInHandsType)))
			{
				Executor.ChangeStateID((!_executorStateIDs.Contains(Executor.CurrentStateID))
					? _executorStateID
					: _executorAltStateID);
				Item itemInHands = player.Inventory.ItemInHands;
				if (playerHandsCheck == PlayerHandsCheckType.MustHaveItemInHands && itemInHands != null &&
				    itemInHands.Type == ItemType.AltairDisposableHackingTool)
				{
					Executor.GetComponentInParent<SceneDoor>().ChangeStats(false);
					(itemInHands as DisposableHackingTool).Special();
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
			if (!(Executor == null))
			{
			}
		}

		protected override void OnTriggerEnter(Collider coli)
		{
			base.OnTriggerEnter(coli);
			if (!IsProximity || Executor == null)
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
					Executor.PlayerEnterTrigger(this, component.TransferableObject as MyPlayer, _executorStateID);
				}
			}
		}

		protected override void OnTriggerExit(Collider coli)
		{
			base.OnTriggerExit(coli);
			if (IsProximity && !(Executor == null))
			{
				TransitionTriggerHelper component = coli.GetComponent<TransitionTriggerHelper>();
				if (component != null && component.TransferableObject is MyPlayer)
				{
					Executor.PlayerExitTrigger(this, component.TransferableObject as MyPlayer, _executorAltStateID);
				}
			}
		}
	}
}
