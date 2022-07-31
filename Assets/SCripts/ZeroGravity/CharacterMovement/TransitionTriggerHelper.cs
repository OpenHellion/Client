using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

namespace ZeroGravity.CharacterMovement
{
	public class TransitionTriggerHelper : MonoBehaviour
	{
		[SerializeField]
		private SpaceObjectTransferable transferableObj;

		[SerializeField]
		private List<Transform> extraCheckPoints = new List<Transform>();

		[HideInInspector]
		public bool Enabled = true;

		private SphereCollider sphereCollider;

		private Dictionary<SceneTriggerRoom, HashSet<Collider>> roomTriggersDict = new Dictionary<SceneTriggerRoom, HashSet<Collider>>();

		public SpaceObjectTransferable TransferableObject
		{
			get
			{
				return transferableObj;
			}
		}

		private SceneTriggerRoom GetRoomTrigger(Collider coli)
		{
			SceneTriggerRoom sceneTriggerRoom = coli.GetComponent<SceneTriggerRoom>();
			if (sceneTriggerRoom == null)
			{
				SceneTriggerRoomSegment component = coli.GetComponent<SceneTriggerRoomSegment>();
				if (component != null)
				{
					sceneTriggerRoom = component.BaseRoom;
				}
			}
			return sceneTriggerRoom;
		}

		private void Awake()
		{
			if (transferableObj == null)
			{
				Dbg.Error("Transition trigger transferable object not set", base.name);
			}
		}

		private void Start()
		{
			if (transferableObj is MyPlayer)
			{
				sphereCollider = GetComponentInParent<SphereCollider>();
				this.InvokeRepeating(AuxCheckRoomTriggers, 0f, 5f);
			}
		}

		private void OnTriggerEnter(Collider coli)
		{
			if (!Enabled)
			{
				return;
			}
			bool flag = transferableObj.CurrentRoomTrigger == null || roomTriggersDict.Count == 0;
			SceneTriggerRoom roomTrigger = GetRoomTrigger(coli);
			if (TransferableObject is DynamicObject && roomTrigger == MyPlayer.Instance.MyRoomTrigger)
			{
				return;
			}
			if (roomTrigger != null)
			{
				if (roomTriggersDict.Count == 0)
				{
					if (flag && Client.IsGameBuild)
					{
						transferableObj.EnterVesselRoomTrigger = roomTrigger;
						transferableObj.EnterVessel(roomTrigger.ParentVessel);
					}
					SceneTriggerRoom currentRoomTrigger = transferableObj.CurrentRoomTrigger;
					transferableObj.CurrentRoomTrigger = roomTrigger;
					transferableObj.RoomChanged(currentRoomTrigger);
				}
				else if (transferableObj.CurrentRoomTrigger != null && transferableObj.CurrentRoomTrigger.ParentVessel is Asteroid && roomTrigger.ParentVessel is Ship)
				{
					transferableObj.EnterVesselRoomTrigger = roomTrigger;
					transferableObj.EnterVessel(roomTrigger.ParentVessel);
					SceneTriggerRoom currentRoomTrigger2 = transferableObj.CurrentRoomTrigger;
					transferableObj.CurrentRoomTrigger = roomTrigger;
					transferableObj.RoomChanged(currentRoomTrigger2);
				}
				else
				{
					transferableObj.EnterVesselRoomTrigger = null;
				}
				if (!roomTriggersDict.ContainsKey(roomTrigger))
				{
					roomTriggersDict.Add(roomTrigger, new HashSet<Collider>());
				}
				roomTriggersDict[roomTrigger].Add(coli);
			}
			else if (transferableObj is MyPlayer && coli.GetComponentInParent<SceneTriggerLadder>() != null)
			{
				SceneTriggerLadder componentInParent = coli.GetComponentInParent<SceneTriggerLadder>();
				if (componentInParent != null)
				{
					MyPlayer myPlayer = transferableObj as MyPlayer;
					myPlayer.InLadderTrigger = true;
					myPlayer.LadderTrigger = componentInParent;
				}
			}
		}

		private void OnTriggerExit(Collider coli)
		{
			if (!Enabled)
			{
				return;
			}
			SceneTriggerRoom roomTrigger = GetRoomTrigger(coli);
			if (roomTrigger != null && roomTriggersDict.ContainsKey(roomTrigger))
			{
				roomTriggersDict[roomTrigger].Remove(coli);
				if (roomTriggersDict[roomTrigger].Count != 0)
				{
					return;
				}
				roomTriggersDict.Remove(roomTrigger);
				SceneTriggerRoom sceneTriggerRoom = null;
				foreach (SceneTriggerRoom key in roomTriggersDict.Keys)
				{
					if (sceneTriggerRoom == null)
					{
						sceneTriggerRoom = key;
					}
					if (sceneTriggerRoom.ParentVessel is Ship)
					{
						break;
					}
					if (sceneTriggerRoom != key && !(key.ParentVessel is Asteroid))
					{
						sceneTriggerRoom = key;
					}
				}
				if (sceneTriggerRoom != null)
				{
					SceneTriggerRoom currentRoomTrigger = transferableObj.CurrentRoomTrigger;
					if (transferableObj.Parent != sceneTriggerRoom.ParentVessel)
					{
						if (transferableObj.Parent is SpaceObjectVessel && transferableObj.Parent != sceneTriggerRoom.ParentVessel)
						{
							SpaceObjectVessel spaceObjectVessel = transferableObj.Parent as SpaceObjectVessel;
							if (spaceObjectVessel.IsDocked)
							{
								spaceObjectVessel = spaceObjectVessel.DockedToMainVessel;
							}
							if (spaceObjectVessel == sceneTriggerRoom.ParentVessel || spaceObjectVessel.AllDockedVessels.Contains(sceneTriggerRoom.ParentVessel))
							{
								transferableObj.DockedVesselParentChanged(sceneTriggerRoom.ParentVessel);
							}
							else
							{
								transferableObj.EnterVessel(sceneTriggerRoom.ParentVessel);
							}
						}
						else
						{
							Dbg.Error("How did this happen, player exited room trigger and its parent is not vessel");
						}
					}
					transferableObj.CurrentRoomTrigger = sceneTriggerRoom;
					transferableObj.RoomChanged(currentRoomTrigger);
				}
				else
				{
					if (transferableObj.CurrentRoomTrigger != null)
					{
						SceneTriggerRoom currentRoomTrigger2 = transferableObj.CurrentRoomTrigger;
						transferableObj.CurrentRoomTrigger = null;
						transferableObj.RoomChanged(currentRoomTrigger2);
						transferableObj.ExitVessel(false);
					}
					checkMyPlayerCanGrabWall();
				}
			}
			else
			{
				if (!(transferableObj is MyPlayer) || !(coli.GetComponentInParent<SceneTriggerLadder>() != null))
				{
					return;
				}
				SceneTriggerLadder componentInParent = coli.GetComponentInParent<SceneTriggerLadder>();
				if (componentInParent != null)
				{
					MyPlayer myPlayer = transferableObj as MyPlayer;
					if (myPlayer.FpsController.IsOnLadder)
					{
						componentInParent.LadderDetach(MyPlayer.Instance);
					}
					myPlayer.InLadderTrigger = false;
					myPlayer.LadderTrigger = null;
				}
			}
		}

		public void ResetTriggers()
		{
			roomTriggersDict = new Dictionary<SceneTriggerRoom, HashSet<Collider>>();
		}

		public void ExitTriggers(Collider[] colliders)
		{
			foreach (Collider coli in colliders)
			{
				SceneTriggerRoom roomTrigger = GetRoomTrigger(coli);
				if (roomTrigger != null && roomTriggersDict.ContainsKey(roomTrigger))
				{
					OnTriggerExit(coli);
				}
			}
		}

		public void SetTransferableObject(SpaceObjectTransferable transObj)
		{
			transferableObj = transObj;
		}

		public void checkMyPlayerCanGrabWall()
		{
			MyPlayer.Instance.FpsController.ToggleInPlayerCollider(Physics.OverlapSphere(base.transform.position, 0.5f, LayerMask.NameToLayer("Default") | LayerMask.NameToLayer("Player")).Length > 1);
		}

		private void AuxCheckRoomTriggers()
		{
			if (sphereCollider == null || !Enabled)
			{
				return;
			}
			Collider[] source = Physics.OverlapSphere(base.transform.position, sphereCollider.radius);
			Collider collider = source.FirstOrDefault(_003CAuxCheckRoomTriggers_003Em__0);
			if (collider == null)
			{
				if (TransferableObject.CurrentRoomTrigger != null || !(transferableObj.Parent is Pivot))
				{
					TransferableObject.ExitVessel(true);
					ResetTriggers();
				}
				else if (roomTriggersDict.Count > 0)
				{
					ResetTriggers();
				}
			}
			else if (TransferableObject.CurrentRoomTrigger == null)
			{
				OnTriggerEnter(collider);
			}
		}

		[CompilerGenerated]
		private bool _003CAuxCheckRoomTriggers_003Em__0(Collider m)
		{
			return GetRoomTrigger(m) != null;
		}
	}
}
