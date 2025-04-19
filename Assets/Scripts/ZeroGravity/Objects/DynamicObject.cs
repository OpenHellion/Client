using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
		public static float SendMovementInterval = 0.1f;

		public Rigidbody _rigidBody;

		private GameObject _collisionDetector;

		public bool Master = true;

		private float _takeoverTimer;

		private Vector3 _movementVelocity;

		private Vector3 _movementAngularVelocity;

		private Collider _takeoverTrigger;

		[HideInInspector] public Item Item;

		private readonly List<Collider> _collidersWithTriggerChanged = new List<Collider>();

		public override SpaceObjectType Type => SpaceObjectType.DynamicObject;

		public float Diameter { get; private set; }

		public float Mass => _rigidBody.mass;

		public new Vector3 Velocity
		{
			get => _rigidBody.velocity;
			set
			{
				if (Master)
				{
					_rigidBody.velocity = value;
				}
			}
		}

		public new Vector3 AngularVelocity
		{
			get => _rigidBody.angularVelocity;
			set
			{
				if (Master)
				{
					_rigidBody.angularVelocity = value;
				}
			}
		}

		public bool IsKinematic => _rigidBody.isKinematic;

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

				NetworkController.Send(dynamicObjectStatsMessage);
			}
		}

		public void ProcessDynamicObectMovementMessage(DynamicObjectMovementMessage mm)
		{
			if (!IsAttached && !(_takeoverTimer < 1f))
			{
				Master = false;
				ToggleKinematic(value: true);
				_movementVelocity = mm.Velocity.ToVector3();
				_movementAngularVelocity = mm.AngularVelocity.ToVector3();
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

				Pivot pivot = World.SolarSystem.GetArtificialBody(Guid) as Pivot;
				if (pivot == null)
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

				Task task = new Task(delegate
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
							_rigidBody.velocity = dosm.AttachData.Velocity.ToVector3();
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
					task.RunSynchronously();
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

				World.SolarSystem.RemoveArtificialBody(Parent as Pivot);
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

		private void SendMovementMessage()
		{
			DynamicObjectMovementMessage message = new DynamicObjectMovementMessage
			{
				GUID = Guid,
				LocalPosition = transform.localPosition.ToArray(),
				LocalRotation = transform.localRotation.ToArray(),
				Velocity = Velocity.ToArray(),
				AngularVelocity = AngularVelocity.ToArray(),
				ImpactVelocity = ImpactVelocity,
				Timestamp = Time.fixedTime
			};
			NetworkController.Send(message);
			ImpactVelocity = 0f;
			transform.hasChanged = false;
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
				if (_takeoverTrigger != null)
				{
					Destroy(_takeoverTrigger);
				}
			}

			_rigidBody.isKinematic = value;
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

		public void AddForce(Vector3 force, ForceMode forceMode)
		{
			if (Master && !IsAttached)
			{
				if (IsKinematic)
				{
					ToggleKinematic(value: false);
				}

				_rigidBody.AddForce(force, forceMode);
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

				_rigidBody.AddTorque(torque);
			}
		}

		public void AddTorque(Vector3 torque, ForceMode forceMode)
		{
			if (Master && !IsAttached && !IsKinematic)
			{
				_rigidBody.AddTorque(torque, forceMode);
			}
		}

		public static DynamicObject SpawnDynamicObject(SpawnObjectResponseData data)
		{
			SpawnDynamicObjectResponseData spawnDynamicObjectResponseData = data as SpawnDynamicObjectResponseData;
			return SpawnDynamicObject(spawnDynamicObjectResponseData.Details,
				World.GetObject(spawnDynamicObjectResponseData.Details.AttachData.ParentGUID,
					spawnDynamicObjectResponseData.Details.AttachData.ParentType));
		}

		public static DynamicObject SpawnDynamicObject(DynamicObjectDetails details, SpaceObject parent)
		{
			DynamicObjectData dynamicObjectData = !StaticData.DynamicObjectsDataList.ContainsKey(details.ItemID)
				? null
				: StaticData.DynamicObjectsDataList[details.ItemID];
			if (dynamicObjectData != null)
			{
				return SpawnDynamicObject(details, dynamicObjectData, parent);
			}

			return null;
		}

		public static DynamicObject SpawnDynamicObject(DynamicObjectDetails details, DynamicObjectData data,
			SpaceObject parent)
		{
			DynamicObject dynamicObject = World.GetDynamicObject(details.GUID);
			try
			{
				if (dynamicObject == null)
				{
					GameObject gameObject = Instantiate(Resources.Load(data.PrefabPath),
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
					dynamicObject.transform.localPosition = details.LocalPosition.ToVector3();
					dynamicObject.transform.localRotation = details.LocalRotation.ToQuaternion();
					dynamicObject._rigidBody.velocity = details.Velocity.ToVector3();
					dynamicObject._rigidBody.angularVelocity = details.AngularVelocity.ToVector3();
				}

				World.AddDynamicObject(dynamicObject.Guid, dynamicObject);
				if (details.ChildObjects != null)
				{
					if (details.ChildObjects.Count > 0)
					{
						foreach (DynamicObjectDetails childObject in details.ChildObjects)
						{
							SpawnDynamicObject(childObject, dynamicObject);
						}

						return dynamicObject;
					}

					return dynamicObject;
				}

				return dynamicObject;
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
				Debug.LogErrorFormat("Could not find dynamic object with GUID {0}, path {1}", details.GUID, data.PrefabPath);
				return dynamicObject;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			EventSystem.RemoveListener(typeof(DynamicObjectStatsMessage), DynamicObjectStatsMessageListener);
			World.RemoveDynamicObject(Guid);
			if (MyPlayer.Instance.Inventory != null &&
			    ((object)Item != null ? Item.Slot : null) == MyPlayer.Instance.Inventory.HandsSlot)
			{
				World.InGameGUI.HelmetHud.HandsSlotUpdate();
			}

			CheckNearbyObjects();
		}

		public override void EnterVessel(SpaceObjectVessel vessel)
		{
			if (!IsAttached)
			{
				if (Parent is Pivot && Parent != vessel)
				{
					World.SolarSystem.RemoveArtificialBody(Parent as Pivot);
					Destroy(Parent.gameObject);
				}

				Parent = vessel;
				transform.parent = vessel.TransferableObjectsRoot.transform;
			}
		}

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

		public override void RoomChanged(SceneTriggerRoom prevRoomTrigger)
		{
			base.RoomChanged(prevRoomTrigger);
		}

		private void ForceActivate()
		{
			Master = true;
			ToggleKinematic(value: false);
			Velocity = _movementVelocity;
			AngularVelocity = _movementAngularVelocity;
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
