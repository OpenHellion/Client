using System;
using System.Collections;
using System.Collections.Generic;
using OpenHellion;
using UnityEngine;
using UnityEngine.Serialization;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class SceneDockingPort : MonoBehaviour, ISceneObject
	{
		public Transform CameraPosition;

		public string Name;

		public string DockType = string.Empty;

		[Tooltip("Whether this port control from the docking panel is disabled or not.")]
		public bool LocalyDisabled;

		[FormerlySerializedAs("Executer")] public SceneTriggerExecutor Executor;

		public string DockState;

		public string UndockState;

		public Transform SceneDockingTriggerTrans;

		public int DockingPortOrder;

		private bool _isDocked;

		[SerializeField] private int _inSceneID;

		public SceneDockingPort DockedToPort;

		[HideInInspector] public Ship ParentShip;

		public bool IsSlave;

		public bool Locked;

		[Space(5f)] public List<SceneDoor> Doors;

		public List<SceneDockingPortController> portControllers = new List<SceneDockingPortController>();

		public GameObject DockingVisualROOT;

		public bool IgnoreThisDockingPort;

		public bool LeverPulse;

		private static World _world;

		public int InSceneID
		{
			get => _inSceneID;
			set => _inSceneID = value;
		}

		private void Awake()
		{
			_world ??= GameObject.Find("/World").GetComponent<World>();
		}

		private void Start()
		{
			if (ParentShip == null)
			{
				ParentShip = GetComponentInParent<GeometryRoot>().MainObject as Ship;
			}
		}

		public void ToggleDock(bool? isDocked = null, bool isInstant = false)
		{
			_isDocked = !isDocked.HasValue ? (!_isDocked) : isDocked.Value;
			if (Executor != null)
			{
				if (isInstant)
				{
					Executor.ChangeStateImmediate((!_isDocked) ? UndockState : DockState);
				}
				else
				{
					Executor.ChangeState((!_isDocked) ? UndockState : DockState);
				}
			}

			if (!_isDocked || portControllers.Count <= 0)
			{
				return;
			}

			foreach (SceneDockingPortController portController in portControllers)
			{
				portController.ToggleLock(isLocked: false, isInstant);
			}
		}

		public void SetDetails(SceneDockingPortDetails details, bool isInitialize)
		{
			Locked = details.Locked;
			if (details.DockedToID == null)
			{
				return;
			}

			if (details.DockingStatus)
			{
				IsSlave = true;
				Ship ship = _world.GetVessel(details.DockedToID.VesselGUID) as Ship;
				if (ship == null)
				{
					if (!isInitialize)
					{
						Dbg.Error("Docking Port: There is no parent ship. ChildShip", ParentShip.GUID, "Parent Ship",
							details.DockedToID.VesselGUID);
					}

					return;
				}

				DockedToPort = ship.GetStructureObject<SceneDockingPort>(details.DockedToID.InSceneID);
				if (DockedToPort == null)
				{
					if (!isInitialize)
					{
						Dbg.Error("Docking Port: There is no docking port in ship. Child Ship", ParentShip.GUID,
							"Parent Ship", details.DockedToID.VesselGUID, "Parent Loaded", ship.SceneObjectsLoaded,
							"Docked To Port ID", details.DockedToID.InSceneID);
					}

					return;
				}

				DockedToPort.Locked = Locked;
				DockedToPort.DockedToPort = this;
				ParentShip.OnDockCompleted = OnDockCompleted;
				ParentShip.DockToShip(this, ship, DockedToPort, details, isInitialize);
				if (!isInitialize && MyPlayer.Instance.Parent == ParentShip &&
				    MyPlayer.Instance.LockedToTrigger != null && MyPlayer.Instance.LockedToTrigger.TriggerType ==
				    SceneTriggerType.DockingPanel)
				{
					_world.InWorldPanels.Docking.UpdateDockingPorts();
				}
			}
			else
			{
				IsSlave = false;
				if (DockedToPort != null)
				{
					ParentShip.OnUndockStarted = OnUndockStarted;
					ParentShip.UndockFromShip(this, DockedToPort.ParentShip, DockedToPort, details);
				}
			}
		}

		public void OnDockCompleted(bool isInitialize)
		{
			ToggleDock(true, isInitialize);
			DockedToPort.ToggleDock(true, isInitialize);
			foreach (SceneDockingPortController portController in portControllers)
			{
				portController.OnLeverStateChange();
			}
		}

		public void OnUndockStarted(bool isInitialize)
		{
			ToggleDock(false, isInitialize);
			DockedToPort.ToggleDock(false, isInitialize);
			DockedToPort.DockedToPort = null;
			DockedToPort = null;
		}

		public void UndockPort()
		{
			if (DockedToPort != null)
			{
				if (IsSlave)
				{
					ToggleDockTo(DockedToPort, toggle: false);
				}
				else
				{
					DockedToPort.ToggleDockTo(this, toggle: false);
				}
			}
		}

		public IEnumerator PulseColourOnMaterial()
		{
			float time = 0f;
			while (LeverPulse)
			{
				yield return new WaitForEndOfFrame();
				time += Time.deltaTime * 5f;
				foreach (SceneDockingPortController portController in portControllers)
				{
					portController.LeverLight.material.SetFloat("_EmissionAmount", 30f + Mathf.Sin(time) * 20f);
				}
			}
		}

		private IEnumerator ReturnLever(float time, List<SceneDockingPortController> controllers)
		{
			yield return new WaitForSeconds(time);
			foreach (SceneDockingPortController controller in controllers)
			{
				SceneTriggerExecutor getExecutor = controller.GetExecutor;
				getExecutor.ChangeState("up");
				controller.IsLocked = false;
				controller.OnLeverStateChange();
			}
		}

		public bool ToggleDockTo(SceneDockingPort sdc, bool toggle, bool checkTogglePossibility = true)
		{
			if (DockType.TagCount() >= 4 && portControllers.Count > 0)
			{
				StartCoroutine(ReturnLever(2f, portControllers));
			}

			if (!IsDockTogglePossibleWith(sdc, toggle) && checkTogglePossibility)
			{
				return false;
			}

			Ship parentShip = sdc.ParentShip;
			SceneDockingPortDetails sceneDockingPortDetails = new SceneDockingPortDetails
			{
				ID = new VesselObjectID(ParentShip.GUID, InSceneID),
				DockedToID = new VesselObjectID(parentShip.GUID, sdc.InSceneID),
				DockingStatus = toggle
			};
			Ship parentShip2 = ParentShip;
			SceneDockingPortDetails dockingPort = sceneDockingPortDetails;
			parentShip2.ChangeStats(null, null, null, null, null, null, null, null, null, dockingPort);
			return true;
		}

		public void AddPortController(SceneDockingPortController controller)
		{
			portControllers.Add(controller);
		}

		public void TryToUndock()
		{
			if (DockedToPort == null)
			{
				return;
			}

			if (DockType.TagCount() >= 4)
			{
				if (IsSlave)
				{
					ToggleDockTo(DockedToPort, toggle: false, checkTogglePossibility: false);
				}
				else
				{
					DockedToPort.ToggleDockTo(this, toggle: false, checkTogglePossibility: false);
				}
			}
			else if (DockType.ExtractTags().Contains("airlock") && DockedToPort.DockType.TagCount() < 4)
			{
				if (IsSlave)
				{
					ToggleDockTo(DockedToPort, toggle: false, checkTogglePossibility: false);
				}
				else
				{
					DockedToPort.ToggleDockTo(this, toggle: false, checkTogglePossibility: false);
				}
			}
			else if (DockedToPort == null ||
			         (portControllers.Count > 0 &&
			          portControllers.Find((SceneDockingPortController m) => !m.IsLocked) != null) ||
			         (DockedToPort.portControllers.Count > 0 &&
			          DockedToPort.portControllers.Find((SceneDockingPortController m) => !m.IsLocked) != null))
			{
				return;
			}

			if (IsSlave)
			{
				ToggleDockTo(DockedToPort, toggle: false);
			}
			else
			{
				DockedToPort.ToggleDockTo(this, toggle: false);
			}
		}

		public bool IsDockTogglePossibleWith(SceneDockingPort sdp, bool isDocking)
		{
			if (!isDocking)
			{
				if (portControllers.Count > 0 &&
				    portControllers.Find((SceneDockingPortController m) => !m.IsLocked) != null)
				{
					return false;
				}

				if (sdp.portControllers.Count > 0 &&
				    sdp.portControllers.Find((SceneDockingPortController m) => !m.IsLocked) != null)
				{
					return false;
				}
			}

			return true;
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(1f, 0.2f, 0.3f, 0.2f);
			if (SceneDockingTriggerTrans != null)
			{
				Gizmos.matrix = SceneDockingTriggerTrans.localToWorldMatrix;
				Gizmos.DrawCube(SceneDockingTriggerTrans.GetComponent<BoxCollider>().center,
					SceneDockingTriggerTrans.GetComponent<BoxCollider>().size);
			}

			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawLine(Vector3.zero, Vector3.forward);
			Gizmos.DrawLine(Vector3.forward, new Vector3(-0.2f, 0f, 0.8f));
			Gizmos.DrawLine(new Vector3(-0.2f, 0f, 0.8f), new Vector3(0.2f, 0f, 0.8f));
			Gizmos.DrawLine(new Vector3(0.2f, 0f, 0.8f), Vector3.forward);
			Gizmos.DrawLine(new Vector3(0.3f, 0f, 0f), Vector3.up * 0.5f);
			Gizmos.DrawLine(new Vector3(-0.3f, 0f, 0f), Vector3.up * 0.5f);
			Gizmos.color = new Color(1f, 0.2f, 0.3f, 0.1f);
			if (CameraPosition != null)
			{
				Gizmos.matrix = CameraPosition.localToWorldMatrix;
				Gizmos.DrawFrustum(CameraPosition.TransformPoint(Vector3.zero), 60f, 1f, 0.2f, 1.7f);
				Gizmos.DrawLine(new Vector3(0.3f, 0f, 0f) * 0.5f, Vector3.up * 0.25f);
				Gizmos.DrawLine(new Vector3(-0.3f, 0f, 0f) * 0.5f, Vector3.up * 0.25f);
			}
		}

		private void OnDestroy()
		{
			if ((bool)GetComponentInChildren<Camera>() &&
			    MyPlayer.Instance.LockedToTrigger is SceneTriggerShipControl &&
			    MyPlayer.Instance.ShipControlMode == ShipControlMode.Docking)
			{
				(MyPlayer.Instance.LockedToTrigger as SceneTriggerShipControl).CancelInteract(MyPlayer.Instance);
			}
		}
	}
}
