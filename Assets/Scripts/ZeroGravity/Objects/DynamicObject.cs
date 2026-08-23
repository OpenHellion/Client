using System;
using System.Collections.Generic;
using OpenHellion.Net;
using UnityEngine;
using ZeroGravity.CharacterMovement;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(TransitionTriggerHelper))]
	public class DynamicObject : SpaceObjectTransferable
	{
		[NonSerialized] public Rigidbody RigidBody;

		private GameObject _collisionDetector;

		// Local physics permission.
		public bool Master = true;

		private float _velocityCheckTimer;

		private float _movementReceivedTime = -1f;

		private Vector3 _movementTargetPosition;

		private Quaternion _movementTargetRotation;

		private Vector3 _movementTargetVelocity;

		private Vector3 _movementTargetAngularVelocity;

		[HideInInspector] public Item Item;

		private readonly List<Collider> _collidersWithTriggerChanged = new List<Collider>();

		public override SpaceObjectType Type => SpaceObjectType.DynamicObject;

		public float Diameter { get; private set; }

		public float Mass => RigidBody.mass;

		public override Vector3 Velocity
		{
			get => RigidBody.linearVelocity;
		}

		public Vector3 AngularVelocity
		{
			get => RigidBody.angularVelocity;
			set
			{
				if (Master)
				{
					RigidBody.angularVelocity = value;
				}
			}
		}

		public bool IsKinematic => RigidBody.isKinematic;

		public bool IsAttached =>
			Item != null && (Item.InvSlot != null || Item.AttachPoint != null || Parent is DynamicObject);

		public override SpaceObject Parent
		{
			get => base.Parent;
			set
			{
				base.Parent = value;
				if (GetParent<MyPlayer>() != null)
				{
					Master = true;
				}
			}
		}

		private void Awake()
		{
			if (TransitionTrigger == null)
			{
				TransitionTrigger = GetComponent<TransitionTriggerHelper>();
			}

			if (TransitionTrigger == null)
			{
				Debug.LogError("Transition trigger not set for dynamic object" + name + gameObject.scene);
			}

			gameObject.SetLayerRecursively(LayerMask.NameToLayer("DynamicObject"), "FirstPerson", "Triggers");
			RigidBody = GetComponent<Rigidbody>();
			RigidBody.useGravity = false;
			RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			Item = GetComponent<Item>();
			EventSystem.AddListener(typeof(DynamicObjectStatsMessage), DynamicObjectStatsMessageListener);
			EventSystem.AddListener(typeof(DynamicObjectsInfoMessage), DynamicObjectsInfoMessageListener);
		}

		private void DynamicObjectsInfoMessageListener(NetworkData data)
		{
			if (Item == null)
			{
				return;
			}

			foreach (DynamicObjectInfo info in (data as DynamicObjectsInfoMessage).Infos)
			{
				if (info.GUID == Guid)
				{
					Item.ProcesStatsData(info.Stats);
					return;
				}
			}
		}

		private void Update()
		{
			if (!IsKinematic)
			{
				// We are simulating locally (a freshly dropped/thrown/bumped item).
				if (AngularVelocity.IsEpsilonEqual(Vector3.zero, 0.5f) && Velocity.IsEpsilonEqual(Vector3.zero, 0.1f))
				{
					_velocityCheckTimer += Time.deltaTime;
					if (_velocityCheckTimer > 1f)
					{
						ToggleKinematic(value: true);
					}
				}
				else
				{
					_velocityCheckTimer = 0f;
				}
			}
			else if (_movementReceivedTime > 0f)
			{
				// Server owns this object: ease toward the last streamed position and velocity.
				float num = Time.realtimeSinceStartup - _movementReceivedTime;
				if (num < 1f)
				{
					transform.SetPositionAndRotation(
						Vector3.Lerp(transform.position, _movementTargetPosition, Mathf.Pow(num, 0.5f)),
						Quaternion.Slerp(transform.rotation, _movementTargetRotation, Mathf.Pow(num, 0.5f)));
					RigidBody.linearVelocity =
						Vector3.Lerp(RigidBody.linearVelocity, _movementTargetVelocity, Mathf.Pow(num, 0.5f));
					RigidBody.angularVelocity =
						Vector3.Lerp(RigidBody.angularVelocity, _movementTargetAngularVelocity, Mathf.Pow(num, 0.5f));
				}
			}
		}

		private void FixedUpdate()
		{
			if (IsDestroying || Guid == 0 || IsAttached)
			{
				return;
			}

			if (IsInsideSpaceObject && Gravity.IsNotEpsilonZero() && !IsKinematic)
			{
				RigidBody.linearVelocity += Gravity * Time.fixedDeltaTime;
			}
		}

		public void SendStatsMessage(DynamicObjectAttachData attachData = null, DynamicObjectStats statsData = null)
		{
			if (attachData != null || statsData != null)
			{
				DynamicObjectStatsMessage dynamicObjectStatsMessage = new DynamicObjectStatsMessage
				{
					Info = new DynamicObjectInfo
					{
						GUID = Guid
					}
				};
				if (attachData != null)
				{
					dynamicObjectStatsMessage.AttachData = attachData;
				}

				if (statsData != null)
				{
					dynamicObjectStatsMessage.Info.Stats = statsData;
				}

				NetworkController.SendAndForget(dynamicObjectStatsMessage);
			}
		}

		// The server is authoritative for any object not currently held in an inventory/attach slot.
		// Receiving a position hands control back to it: drop local ownership and follow the stream.
		public void ProcessMovementMessage(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
		{
			if (!IsAttached)
			{
				Master = false;
				ToggleKinematic(value: true);
				_movementReceivedTime = Time.realtimeSinceStartup;
				_movementTargetPosition = position;
				_movementTargetRotation = rotation;
				_movementTargetVelocity = velocity;
				_movementTargetAngularVelocity = angularVelocity;
			}
		}

		private bool AreAttachDataSame(DynamicObjectAttachData data)
		{
			return Parent.Type == data.ParentType && Parent.Guid == data.ParentGUID && IsAttached == data.IsAttached;
		}

		private void DynamicObjectStatsMessageListener(NetworkData data)
		{
			DynamicObjectStatsMessage dosm = data as DynamicObjectStatsMessage;
			if (dosm.Info.GUID != Guid)
			{
				return;
			}

			if (dosm.DestroyDynamicObject)
			{
				if (Parent is Pivot && Parent.Type == SpaceObjectType.DynamicObjectPivot)
				{
					Destroy(Parent.gameObject);
				}
				else
				{
					Destroy(gameObject);
				}

				return;
			}

			if (dosm.Info.Stats != null && Item != null)
			{
				Item.ProcesStatsData(dosm.Info.Stats);
			}

			if (dosm.AttachData == null)
			{
				return;
			}

			if ((Item != null && Item.AreAttachDataSame(dosm.AttachData)) ||
			    (Item == null && AreAttachDataSame(dosm.AttachData)))
			{
				return;
			}

			SpaceObject prevParent = Parent;
			if (dosm.AttachData.ParentType == SpaceObjectType.DynamicObjectPivot)
			{
				ArtificialBody parent = GetParent<ArtificialBody>();
				if (parent == null)
				{
					Debug.LogError("Dynamic object exited vessel but we don't know from where. " + Guid + Parent +
						dosm.AttachData.ParentType + dosm.AttachData.ParentGUID);
					return;
				}

				if (!World.TryGetSpaceObject(Guid, out Pivot pivot))
				{
					pivot = Pivot.Create(SpaceObjectType.DynamicObjectPivot, Guid, parent, isMainObject: false);
				}

				bool myPlayerIsParent = Parent is MyPlayer;
				if (Item != null)
				{
					Item.AttachToObject(pivot, sendAttachMessage: false);
				}
				else
				{
					Parent = pivot;
					SetParentTransferableObjectsRoot();
					ResetRoomTriggers();
					ToggleActive(isActive: true);
					ToggleEnabled(isEnabled: true, toggleColliders: true);
				}

				Action task = new Action(delegate
				{
					if (!myPlayerIsParent || !Master)
					{
						if (dosm.AttachData.LocalPosition != null)
						{
							transform.localPosition = dosm.AttachData.LocalPosition.ToVector3();
						}

						if (dosm.AttachData.LocalRotation != null)
						{
							transform.localRotation = dosm.AttachData.LocalRotation.ToQuaternion();
						}
					}

					if (Master)
					{
						if (dosm.AttachData.Velocity != null)
						{
							RigidBody.linearVelocity = dosm.AttachData.Velocity.ToVector3();
						}

						if (dosm.AttachData.Torque != null)
						{
							AddTorque(dosm.AttachData.Torque.ToVector3(), ForceMode.Impulse);
						}

						if (dosm.AttachData.ThrowForce != null)
						{
							Vector3 vector = dosm.AttachData.ThrowForce.ToVector3();
							if ((MyPlayer.Instance.CurrentRoomTrigger == null ||
							     !MyPlayer.Instance.CurrentRoomTrigger.UseGravity ||
							     MyPlayer.Instance.CurrentRoomTrigger.GravityForce == Vector3.zero) &&
							    prevParent == MyPlayer.Instance)
							{
								float num = MyPlayer.Instance.rigidBody.mass + Mass;
								AddForce(vector * (MyPlayer.Instance.rigidBody.mass / num), ForceMode.VelocityChange);
								MyPlayer.Instance.rigidBody.AddForce(-vector * (Mass / num), ForceMode.VelocityChange);
							}
							else
							{
								AddForce(vector, ForceMode.Impulse);
							}
						}
					}
				});
				if (Parent is MyPlayer && MyPlayer.Instance.AnimHelper.DropTask != null)
				{
					MyPlayer.Instance.AnimHelper.AfterDropTask = task;
				}
				else
				{
					task();
				}
			}
			else if (Parent is Pivot && (dosm.AttachData.ParentType == SpaceObjectType.Ship ||
			                             dosm.AttachData.ParentType == SpaceObjectType.Station ||
			                             dosm.AttachData.ParentType == SpaceObjectType.Asteroid))
			{
				if (!(Parent is Pivot))
				{
					Debug.LogError("Entered vessel but we don't know from where." + Guid + Parent +
						dosm.AttachData.ParentType + dosm.AttachData.ParentGUID);
					return;
				}

				World.RemoveArtificialBody(Parent.Guid, this);
				Destroy(Parent.gameObject);
				Parent = World.GetVessel(dosm.AttachData.ParentGUID);
				if (Item != null)
				{
					Item.AttachToObject(Parent, sendAttachMessage: false);
					return;
				}

				transform.parent = Parent.TransferableObjectsRoot.transform;
				ResetRoomTriggers();
				ToggleActive(isActive: true);
				ToggleEnabled(isEnabled: true, toggleColliders: true);
			}
			else if (Item != null)
			{
				Item.ProcessAttachData(dosm.AttachData, prevParent);
			}
		}

		public void ResetRoomTriggers()
		{
			TransitionTrigger.ResetTriggers();
		}

		public void ToggleKinematic(bool value)
		{
			if (!Master && !value)
			{
				value = true;
			}

			if (!value)
			{
				_velocityCheckTimer = 0f;
			}

			RigidBody.isKinematic = value;
		}

		public void ToggleEnabled(bool isEnabled, bool toggleColliders)
		{
			enabled = isEnabled;
			TransitionTrigger.enabled = isEnabled;
			if (toggleColliders || isEnabled)
			{
				if ((bool)OnPlatform && !isEnabled)
				{
					OnPlatform.RemoveFromPlatform(this);
				}

				if (Item != null && Item.CustomCollidereToggle(isEnabled))
				{
					return;
				}

				Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
				foreach (Collider collider in componentsInChildren)
				{
					collider.enabled = isEnabled;
				}
			}

			if (_collisionDetector != null)
			{
				_collisionDetector.SetActive(isEnabled && !IsAttached);
			}
		}

		public void ToggleTriggerColliders(bool areCollidersTrigger)
		{
			if (areCollidersTrigger)
			{
				Collider[] componentsInChildren = Item.GetComponentsInChildren<Collider>();
				foreach (Collider collider in componentsInChildren)
				{
					if (!collider.isTrigger)
					{
						if (!_collidersWithTriggerChanged.Contains(collider))
						{
							_collidersWithTriggerChanged.Add(collider);
						}

						collider.isTrigger = true;
					}
				}
			}
			else
			{
				if (_collidersWithTriggerChanged.Count <= 0)
				{
					return;
				}

				foreach (Collider item in _collidersWithTriggerChanged)
				{
					item.isTrigger = false;
				}

				_collidersWithTriggerChanged.Clear();
			}
		}

		public void ToggleActive(bool isActive)
		{
			gameObject.SetActive(isActive);
			TransitionTrigger.enabled = isActive;
			if (_collisionDetector != null)
			{
				_collisionDetector.SetActive(isActive && !IsAttached);
			}
		}

		public void SetSimulated(bool isSimulated)
		{
			if (isSimulated)
			{
				World.AddDynamicObject(Guid, this);
			}
			else
			{
				World.RemoveDynamicObject(Guid);
			}
		}

		public void AddForce(Vector3 force, ForceMode forceMode)
		{
			if (Master && !IsAttached)
			{
				if (IsKinematic)
				{
					ToggleKinematic(value: false);
				}

				RigidBody.AddForce(force, forceMode);
			}
		}

		public void AddTorque(Vector3 torque)
		{
			if (Master && !IsAttached)
			{
				if (IsKinematic)
				{
					ToggleKinematic(value: false);
				}

				RigidBody.AddTorque(torque);
			}
		}

		public void AddTorque(Vector3 torque, ForceMode forceMode)
		{
			if (Master && !IsAttached && !IsKinematic)
			{
				RigidBody.AddTorque(torque, forceMode);
			}
		}

		public static DynamicObject CreateDynamicObject(DynamicObjectDetails data)
		{
			return CreateDynamicObject(data, World.GetObject(data.AttachData.ParentGUID, data.AttachData.ParentType));
		}

		public static DynamicObject CreateDynamicObject(DynamicObjectDetails details, SpaceObject parent)
		{
			DynamicObjectData dynamicObjectData = !StaticData.DynamicObjectsDataList.ContainsKey(details.ItemID)
				? null
				: StaticData.DynamicObjectsDataList[details.ItemID];
			if (dynamicObjectData != null)
			{
				return CreateDynamicObject(details, dynamicObjectData, parent);
			}

			return null;
		}

		public static DynamicObject CreateDynamicObject(DynamicObjectDetails details, DynamicObjectData data,
			SpaceObject parent)
		{
			DynamicObject dynamicObject = World.GetDynamicObject(details.GUID);
			bool reused = dynamicObject != null;
			try
			{
				if (dynamicObject == null)
				{
					UnityEngine.Object prefab = Resources.Load(data.PrefabPath);
					if (prefab == null)
					{
						Debug.LogErrorFormat("Could not find requested prefab on path {0}", data.PrefabPath);
						return dynamicObject;
					}
					GameObject gameObject = Instantiate(prefab,
						new Vector3(20000f, 20000f, 20000f), Quaternion.identity) as GameObject;
					gameObject.SetActive(value: false);
					dynamicObject = gameObject.GetComponent<DynamicObject>();
					dynamicObject.tag = "Untagged";
					dynamicObject.Guid = details.GUID;
					dynamicObject.name = "DynamicObject_" + details.GUID;
					gameObject.SetActive(value: true);
				}

				if (dynamicObject.Item != null)
				{
					if (details.AttachData != null)
					{
						dynamicObject.Item.ProcessAttachData(details.AttachData);
					}

					if (details.StatsData != null)
					{
						dynamicObject.Item.ProcesStatsData(details.StatsData);
					}
				}

				dynamicObject.Parent = parent;
				if (!dynamicObject.IsAttached)
				{
					dynamicObject.transform.SetLocalPositionAndRotation(details.LocalPosition.ToVector3(), details.LocalRotation.ToQuaternion());
					dynamicObject.RigidBody.linearVelocity = details.Velocity.ToVector3();
					dynamicObject.RigidBody.angularVelocity = details.AngularVelocity.ToVector3();
				}

				dynamicObject.SetSimulated(parent is ArtificialBody);
				if (details.ChildObjects != null)
				{
					foreach (DynamicObjectDetails childObject in details.ChildObjects)
					{
						CreateDynamicObject(childObject, dynamicObject);
					}
				}

				return dynamicObject;
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("Failed to create dynamic object {0}, path {1}: {2}", details.GUID,
					data.PrefabPath, ex);
				if (reused)
				{
					return dynamicObject;
				}

				if (dynamicObject != null)
				{
					Destroy(dynamicObject.gameObject);
				}

				return null;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			EventSystem.RemoveListener(typeof(DynamicObjectStatsMessage), DynamicObjectStatsMessageListener);
			EventSystem.RemoveListener(typeof(DynamicObjectsInfoMessage), DynamicObjectsInfoMessageListener);
			if (World != null)
			{
				World.RemoveDynamicObject(Guid);

				// Only the object leaving the hands changes what the hands slot shows.
				if (MyPlayer.Instance != null && Item != null && Item.Slot == MyPlayer.Instance.Inventory.HandsSlot)
				{
					World.InGameGUI.HelmetHud.HandsSlotUpdate();
				}
			}

			CheckNearbyObjects();
		}

		public override void EnterVessel(SpaceObjectVessel vessel)
		{
			if (!IsAttached)
			{
				if (Parent is Pivot && Parent != vessel)
				{
					World.RemoveArtificialBody(Parent.Guid, this);
					Destroy(Parent.gameObject);
				}

				Parent = vessel;
				transform.parent = vessel.TransferableObjectsRoot.transform;
			}
		}

		/// <inheritdoc/>
		public override void ExitVessel(bool forceExit)
		{
			if (!IsAttached || forceExit)
			{
				ArtificialBody artificialBody = Parent is not SpaceObjectVessel
					? GetParent<ArtificialBody>()
					: (Parent as SpaceObjectVessel).MainVessel;
				if (artificialBody == null)
				{
					Debug.LogErrorFormat("Cannot exit vessel, cannot find parents artificial body {0}, {1}", name, Guid);
					return;
				}

				Parent = Pivot.Create(SpaceObjectType.DynamicObjectPivot, Guid, artificialBody,
					isMainObject: false);
				SetParentTransferableObjectsRoot();
				SendStatsMessage(new DynamicObjectAttachData
				{
					InventorySlotID = -1111,
					IsAttached = false,
					ParentGUID = Parent.Guid,
					ParentType = Parent.Type,
					LocalPosition = transform.localPosition.ToArray(),
					LocalRotation = transform.localRotation.ToArray()
				});
			}
		}

		public override void DockedVesselParentChanged(SpaceObjectVessel vessel)
		{
			if (IsAttached)
			{
				Debug.LogErrorFormat("Attached object changed parent {0}, {1}, {2}, {3}", Parent.Guid, Parent.Type, vessel.Guid, vessel.Type);
			}

			Parent = vessel;
			transform.parent = vessel.TransferableObjectsRoot.transform;
			SendStatsMessage(new DynamicObjectAttachData
			{
				ParentGUID = vessel.Guid,
				ParentType = vessel.Type,
				LocalPosition = transform.localPosition.ToArray(),
				LocalRotation = transform.localRotation.ToArray()
			});
		}

		public override void OnGravityChanged(Vector3 oldGravity)
		{
			if (oldGravity != Vector3.zero && Gravity.IsEpsilonEqual(Vector3.zero))
			{
				AddForce(
					new Vector3(UnityEngine.Random.Range(0.001f, 0.05f), UnityEngine.Random.Range(0.001f, 0.05f),
						UnityEngine.Random.Range(0.001f, 0.05f)), ForceMode.Impulse);
				AddTorque(new Vector3(UnityEngine.Random.Range(0.001f, 0.05f), UnityEngine.Random.Range(0.001f, 0.05f),
					UnityEngine.Random.Range(0.001f, 0.05f)));
			}
		}

		private void OnCollisionEnter(Collision coli)
		{
			if (!IsAttached && IsKinematic)
			{
				ToggleKinematic(value: false);
				SpaceObjectTransferable componentInParent =
					coli.gameObject.GetComponentInParent<SpaceObjectTransferable>();
				if (componentInParent is MyPlayer)
				{
					Master = true;
				}
				else if (componentInParent is DynamicObject && (componentInParent as DynamicObject).Master)
				{
					Master = true;
				}

				AddForce(coli.relativeVelocity, ForceMode.VelocityChange);
			}
		}

		public override void RoomChanged(SceneTriggerRoom prevRoomTrigger)
		{
			base.RoomChanged(prevRoomTrigger);
		}

		public void CheckNearbyObjects(HashSet<DynamicObject> alreadyTraversed = null)
		{
			if (alreadyTraversed == null)
			{
				alreadyTraversed = new HashSet<DynamicObject>();
			}

			if (!alreadyTraversed.Add(this))
			{
				return;
			}

			Renderer componentInChildren = GetComponentInChildren<Renderer>();
			if (componentInChildren == null)
			{
				return;
			}

			Collider[] array =
				Physics.OverlapSphere(transform.position, componentInChildren.bounds.size.magnitude);
			foreach (Collider collider in array)
			{
				DynamicObject componentInParent = collider.GetComponentInParent<DynamicObject>();
				if (componentInParent != null && !componentInParent.IsAttached && componentInParent.IsKinematic)
				{
					ToggleKinematic(value: false);
					componentInParent.CheckNearbyObjects(alreadyTraversed);
				}
			}
		}

		public void SendAttachMessage(SpaceObject newParent, IItemSlot slot, Vector3? localPosition = null,
			Quaternion? localRotation = null, Vector3? impulse = null, Vector3? angularImpulse = null,
			Vector3? velocity = null)
		{
			bool flag = slot != null || newParent is DynamicObject;
			SendStatsMessage(new DynamicObjectAttachData
			{
				IsAttached = flag,
				ParentGUID = newParent.Guid,
				ParentType = newParent.Type,
				ItemSlotID = (short)(slot is ItemSlot ? (slot as ItemSlot).ID : 0),
				InventorySlotID = (short)(!(slot is InventorySlot) ? -1111 : (slot as InventorySlot).SlotID),
				APDetails = !(slot is BaseSceneAttachPoint)
					? null
					: new AttachPointDetails
					{
						InSceneID = (slot as BaseSceneAttachPoint).InSceneID
					},
				LocalPosition = flag || !localPosition.HasValue ? null : localPosition.Value.ToArray(),
				LocalRotation = flag || !localRotation.HasValue ? null : localRotation.Value.ToArray(),
				Velocity = !velocity.HasValue ? null : velocity.Value.ToArray(),
				Torque = !angularImpulse.HasValue ? null : angularImpulse.Value.ToArray(),
				ThrowForce = !impulse.HasValue ? null : impulse.Value.ToArray()
			});
		}
	}
}
