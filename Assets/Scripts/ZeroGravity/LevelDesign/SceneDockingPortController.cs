using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using ZeroGravity.Data;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class SceneDockingPortController : BaseSceneTrigger
	{
		[SerializeField]
		private SceneDockingPort parentPort;

		private bool isLocked;

		[SerializeField]
		private SceneTriggerExecuter executer;

		[SerializeField]
		private string lockExecuterState;

		[SerializeField]
		private string unlockExecuterState;

		[SerializeField]
		private bool _IsNearTrigger = true;

		public MeshRenderer LeverLight;

		[HideInInspector]
		public SceneTriggerExecuter GetExecuter
		{
			get
			{
				return executer;
			}
		}

		public bool IsLocked
		{
			get
			{
				return parentPort.Locked || isLocked;
			}
			set
			{
				isLocked = value;
				OnLeverStateChange();
			}
		}

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
				return SceneTriggerType.DockingPortController;
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
				return _IsNearTrigger;
			}
		}

		public override bool IsInteractable
		{
			get
			{
				return true;
			}
		}

		private bool connected
		{
			get
			{
				return parentPort.DockedToPort != null;
			}
		}

		public override bool CameraMovementAllowed
		{
			get
			{
				return false;
			}
		}

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
				parentPort.StartCoroutine("PulseColourOnMaterial");
				foreach (SceneDockingPortController portController in parentPort.DockedToPort.portControllers)
				{
					portController.LeverLight.material.SetColor("_EmColor", Colors.Blue);
					if (!portController.IsLocked)
					{
						parentPort.DockedToPort.LeverPulse = true;
						break;
					}
				}
				parentPort.DockedToPort.StartCoroutine("PulseColourOnMaterial");
			}
			else
			{
				LeverLight.material.SetColor("_EmColor", Colors.Green);
			}
		}

		public void ToggleLock(bool isLocked, bool isInstant)
		{
			IsLocked = isLocked;
			if (executer != null)
			{
				if (isInstant)
				{
					executer.ChangeStateImmediate((!IsLocked) ? unlockExecuterState : lockExecuterState);
				}
				else
				{
					executer.ChangeState((!IsLocked) ? unlockExecuterState : lockExecuterState);
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
				Dbg.Error("Scene docking port controller has no parent docking port", base.name);
			}
			else
			{
				parentPort.AddPortController(this);
			}
			if (executer != null)
			{
				LeverLight = executer.gameObject.GetComponent<MeshRenderer>();
			}
			StandardTip = Localization.StandardInteractionTip.Undock;
			if (CheckAuthorizationType == AuthorizationType.none)
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
