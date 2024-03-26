using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;
using ZeroGravity.Data;
using ZeroGravity.Objects;
using Debug = UnityEngine.Debug;

namespace ZeroGravity.LevelDesign
{
	public class SceneDockingPortController : BaseSceneTrigger
	{
		[SerializeField] private SceneDockingPort parentPort;

		private bool _isLocked;

		[FormerlySerializedAs("executer")] [SerializeField]
		private SceneTriggerExecutor _executor;

		[FormerlySerializedAs("lockExecuterState")] [SerializeField] private string _lockExecutorState;

		[FormerlySerializedAs("unlockExecuterState")] [SerializeField] private string _unlockExecutorState;

		[FormerlySerializedAs("_IsNearTrigger")] [SerializeField] private bool _isNearTrigger = true;

		public MeshRenderer LeverLight;

		public SceneTriggerExecutor GetExecutor => _executor;

		public bool IsLocked
		{
			get => parentPort.Locked || _isLocked;
			set
			{
				_isLocked = value;
				OnLeverStateChange();
			}
		}

		public override bool ExclusivePlayerLocking => true;

		public override SceneTriggerType TriggerType => SceneTriggerType.DockingPortController;

		public override PlayerHandsCheckType PlayerHandsCheck => PlayerHandsCheckType.DontCheck;

		public override List<ItemType> PlayerHandsItemType => null;

		public override bool IsNearTrigger => _isNearTrigger;

		public override bool IsInteractable => true;

		private bool connected => parentPort.DockedToPort != null;

		public override bool CameraMovementAllowed => false;

		public void OnLeverStateChange()
		{
			StackTrace stackTrace = new StackTrace(true);
			parentPort.LeverPulse = false;
			if (!IsLocked)
			{
				LeverLight.material.SetColor("_EmColor", Colors.Blue);
			}
			else if (connected)
			{
				LeverLight.material.SetColor("_EmColor", Colors.Red);
				parentPort.LeverPulse = true;
				parentPort.StartCoroutine(nameof(SceneDockingPort.PulseColourOnMaterial));
				foreach (SceneDockingPortController portController in parentPort.DockedToPort.portControllers)
				{
					portController.LeverLight.material.SetColor("_EmColor", Colors.Blue);
					if (!portController.IsLocked)
					{
						parentPort.DockedToPort.LeverPulse = true;
						break;
					}
				}

				parentPort.DockedToPort.StartCoroutine(nameof(SceneDockingPort.PulseColourOnMaterial));
			}
			else
			{
				LeverLight.material.SetColor("_EmColor", Colors.Green);
			}
		}

		public void ToggleLock(bool isLocked, bool isInstant)
		{
			IsLocked = isLocked;
			if (_executor != null)
			{
				if (isInstant)
				{
					_executor.ChangeStateImmediate((!IsLocked) ? _unlockExecutorState : _lockExecutorState);
				}
				else
				{
					_executor.ChangeState((!IsLocked) ? _unlockExecutorState : _lockExecutorState);
				}
			}

			if (isLocked && parentPort != null)
			{
				parentPort.TryToUndock();
			}
		}

		private void Awake()
		{
			if (parentPort == null)
			{
				parentPort = GetComponentInParent<SceneDockingPort>();
			}

			if (parentPort == null)
			{
				Debug.LogError("Scene docking port controller has no parent docking port " + base.name);
			}
			else
			{
				parentPort.AddPortController(this);
			}

			if (_executor != null)
			{
				LeverLight = _executor.gameObject.GetComponent<MeshRenderer>();
			}

			StandardTip = Localization.StandardInteractionTip.Undock;
			if (CheckAuthorizationType == AuthorizationType.None)
			{
				CheckAuthorizationType = AuthorizationType.AuthorizedOrNoSecurity;
			}
		}

		public override bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!base.Interact(player, interactWithOverlappingTriggers))
			{
				return false;
			}

			ToggleLock(!IsLocked, false);
			if (interactWithOverlappingTriggers)
			{
				SceneTriggerHelper.InteractWithOverlappingTriggers(base.gameObject, this, player);
			}

			return true;
		}
	}
}
