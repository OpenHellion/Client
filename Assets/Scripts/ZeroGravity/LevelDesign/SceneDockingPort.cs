using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace ZeroGravity.LevelDesign
{
	public class SceneDockingPort : MonoBehaviour, ISceneObject
	{
		public const bool DockLeverState = false;

		public const bool UndockLeverState = true;

		public Transform CameraPosition;

		public string Name;

		public string DockType = string.Empty;

		[Tooltip("Wether this port control from the docking panel is disabled or not.")]
		public bool LocalyDisabled;

		public SceneTriggerExecuter Executer;

		public string DockState;

		public string UndockState;

		public Transform SceneDockingTriggerTrans;

		public int DockingPortOrder;

		private bool _isDocked;

		[SerializeField]
		private int _inSceneID;

		private bool dockingStatus;

		public SceneDockingPort DockedToPort;

		[HideInInspector]
		public Ship ParentShip;

		public bool IsSlave;

		public bool Locked;

		[Space(5f)]
		[SerializeField]
		public List<SceneTriggerExecuter> mergeExecuters;

		[SerializeField]
		public float mergeExecutersDistance = 1f;

		[Space(5f)]
		public List<SceneDoor> Doors;

		public float DoorPairingDistance = 0.5f;

		public List<SceneDockingPortController> portControllers = new List<SceneDockingPortController>();

		public GameObject DockingVisualROOT;

		public bool IgnoreThisDockingPort;

		public bool LeverPulse;

		public const string AirlockTag = "airlock";

		[CompilerGenerated]
		private static Predicate<SceneDockingPortController> _003C_003Ef__am_0024cache0;

		[CompilerGenerated]
		private static Predicate<SceneDockingPortController> _003C_003Ef__am_0024cache1;

		[CompilerGenerated]
		private static Predicate<SceneDockingPortController> _003C_003Ef__am_0024cache2;

		[CompilerGenerated]
		private static Predicate<SceneDockingPortController> _003C_003Ef__am_0024cache3;

		public int InSceneID
		{
			get
			{
				return _inSceneID;
			}
			set
			{
				_inSceneID = value;
			}
		}

		private void Start()
		{
			if (Client.IsGameBuild && ParentShip == null)
			{
				ParentShip = GetComponentInParent<GeometryRoot>().MainObject as Ship;
			}
		}

		public void ToggleDock(bool? isDocked = null, bool isInstant = false)
		{
			_isDocked = ((!isDocked.HasValue) ? (!_isDocked) : isDocked.Value);
			if (Executer != null)
			{
				if (isInstant)
				{
					Executer.ChangeStateImmediate((!_isDocked) ? UndockState : DockState);
				}
				else
				{
					Executer.ChangeState((!_isDocked) ? UndockState : DockState);
				}
			}
			if (!_isDocked || portControllers.Count <= 0)
			{
				return;
			}
			foreach (SceneDockingPortController portController in portControllers)
			{
				portController.ToggleLock(false, isInstant);
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
				Ship ship = Client.Instance.GetVessel(details.DockedToID.VesselGUID) as Ship;
				if (ship == null)
				{
					if (!isInitialize)
					{
						Dbg.Error("Docking Port: There is no parent ship. ChildShip", ParentShip.GUID, "Parent Ship", details.DockedToID.VesselGUID);
					}
					return;
				}
				DockedToPort = ship.GetStructureObject<SceneDockingPort>(details.DockedToID.InSceneID);
				if (DockedToPort == null)
				{
					if (!isInitialize)
					{
						Dbg.Error("Docking Port: There is no docking port in ship. Child Ship", ParentShip.GUID, "Parent Ship", details.DockedToID.VesselGUID, "Parent Loaded", ship.SceneObjectsLoaded, "Docked To Port ID", details.DockedToID.InSceneID);
					}
					return;
				}
				DockedToPort.Locked = Locked;
				dockingStatus = details.DockingStatus;
				DockedToPort.DockedToPort = this;
				DockedToPort.dockingStatus = details.DockingStatus;
				ParentShip.OnDockCompleted = OnDockCompleted;
				ParentShip.DockToShip(this, ship, DockedToPort, details, isInitialize);
				if (!isInitialize && MyPlayer.Instance.Parent == ParentShip && MyPlayer.Instance.LockedToTrigger != null && MyPlayer.Instance.LockedToTrigger.TriggerType == SceneTriggerType.DockingPanel)
				{
					Client.LogCustomEvent("dock_vessel", new Dictionary<string, object>
					{
						{ "name", ParentShip.Name },
						{ "to_vessel_name", ship.Name },
						{
							"main_vessel_name",
							ParentShip.MainVessel.Name
						},
						{
							"total_vessels_count",
							ParentShip.MainVessel.AllDockedVessels.Count + 1
						}
					});
					Client.Instance.InGamePanels.Docking.UpdateDockingPorts();
				}
			}
			else
			{
				IsSlave = false;
				if (DockedToPort != null)
				{
					dockingStatus = details.DockingStatus;
					DockedToPort.dockingStatus = details.DockingStatus;
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
			if (Client.IsGameBuild && DockedToPort != null)
			{
				if (IsSlave)
				{
					ToggleDockTo(DockedToPort, false);
				}
				else
				{
					DockedToPort.ToggleDockTo(this, false);
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
				SceneTriggerExecuter getExecuter = controller.GetExecuter;
				getExecuter.ChangeState("up");
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
			SceneDockingPortDetails sceneDockingPortDetails = new SceneDockingPortDetails();
			sceneDockingPortDetails.ID = new VesselObjectID(ParentShip.GUID, InSceneID);
			sceneDockingPortDetails.DockedToID = new VesselObjectID(parentShip.GUID, sdc.InSceneID);
			sceneDockingPortDetails.DockingStatus = toggle;
			Ship parentShip2 = ParentShip;
			SceneDockingPortDetails dockingPort = sceneDockingPortDetails;
			parentShip2.ChangeStats(null, null, null, null, null, null, null, null, null, dockingPort);
			Client.Instance.ChangeStatsByIfNotAdmin(SteamStatID.docked_vessels, 1);
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
					ToggleDockTo(DockedToPort, false, false);
				}
				else
				{
					DockedToPort.ToggleDockTo(this, false, false);
				}
			}
			else if (DockType.ExtractTags().Contains("airlock") && DockedToPort.DockType.TagCount() < 4)
			{
				if (IsSlave)
				{
					ToggleDockTo(DockedToPort, false, false);
				}
				else
				{
					DockedToPort.ToggleDockTo(this, false, false);
				}
			}
			else
			{
				if (DockedToPort == null)
				{
					return;
				}
				if (portControllers.Count > 0)
				{
					List<SceneDockingPortController> list = portControllers;
					if (_003C_003Ef__am_0024cache0 == null)
					{
						_003C_003Ef__am_0024cache0 = _003CTryToUndock_003Em__0;
					}
					if (list.Find(_003C_003Ef__am_0024cache0) != null)
					{
						return;
					}
				}
				if (DockedToPort.portControllers.Count > 0)
				{
					List<SceneDockingPortController> list2 = DockedToPort.portControllers;
					if (_003C_003Ef__am_0024cache1 == null)
					{
						_003C_003Ef__am_0024cache1 = _003CTryToUndock_003Em__1;
					}
					if (list2.Find(_003C_003Ef__am_0024cache1) != null)
					{
						return;
					}
				}
			}
			if (IsSlave)
			{
				ToggleDockTo(DockedToPort, false);
			}
			else
			{
				DockedToPort.ToggleDockTo(this, false);
			}
		}

		public bool IsDockTogglePossibleWith(SceneDockingPort sdp, bool isDocking)
		{
			if (!isDocking)
			{
				if (portControllers.Count > 0)
				{
					List<SceneDockingPortController> list = portControllers;
					if (_003C_003Ef__am_0024cache2 == null)
					{
						_003C_003Ef__am_0024cache2 = _003CIsDockTogglePossibleWith_003Em__2;
					}
					if (list.Find(_003C_003Ef__am_0024cache2) != null)
					{
						return false;
					}
				}
				if (sdp.portControllers.Count > 0)
				{
					List<SceneDockingPortController> list2 = sdp.portControllers;
					if (_003C_003Ef__am_0024cache3 == null)
					{
						_003C_003Ef__am_0024cache3 = _003CIsDockTogglePossibleWith_003Em__3;
					}
					if (list2.Find(_003C_003Ef__am_0024cache3) != null)
					{
						return false;
					}
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
				Gizmos.DrawCube(SceneDockingTriggerTrans.GetComponent<BoxCollider>().center, SceneDockingTriggerTrans.GetComponent<BoxCollider>().size);
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
			if ((bool)GetComponentInChildren<Camera>() && MyPlayer.Instance.LockedToTrigger is SceneTriggerShipControl2 && MyPlayer.Instance.ShipControlMode == ShipControlMode.Docking)
			{
				(MyPlayer.Instance.LockedToTrigger as SceneTriggerShipControl2).CancelInteract(MyPlayer.Instance);
			}
		}

		[CompilerGenerated]
		private static bool _003CTryToUndock_003Em__0(SceneDockingPortController m)
		{
			return !m.IsLocked;
		}

		[CompilerGenerated]
		private static bool _003CTryToUndock_003Em__1(SceneDockingPortController m)
		{
			return !m.IsLocked;
		}

		[CompilerGenerated]
		private static bool _003CIsDockTogglePossibleWith_003Em__2(SceneDockingPortController m)
		{
			return !m.IsLocked;
		}

		[CompilerGenerated]
		private static bool _003CIsDockTogglePossibleWith_003Em__3(SceneDockingPortController m)
		{
			return !m.IsLocked;
		}
	}
}
