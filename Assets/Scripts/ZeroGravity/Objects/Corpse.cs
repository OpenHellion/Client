using System;
using System.Collections.Generic;
using System.Linq;
using OpenHellion.Net;
using UnityEngine;
using ZeroGravity.CharacterMovement;
using ZeroGravity.LevelDesign;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class Corpse : SpaceObjectTransferable, ISlotContainer
	{
		private class CorpsePart
		{
			public Rigidbody RBody;

			public Transform Trans;
		}

		public static float SendMovementInterval = 0.1f;

		private float sendMovementTime;

		public Inventory Inventory;

		[SerializeField]
		private RagdollHelper ragdollComponent;

		[SerializeField]
		private AnimatorHelper animHelper;

		[SerializeField]
		private SkinnedMeshRenderer headSkin;

		[SerializeField]
		private Transform outfitTransform;

		[SerializeField]
		private Transform basicOutfitHolder;

		[SerializeField]
		private Transform centerOfMass;

		private Collider takeoverTrigger;

		private byte hipsKey = byte.MaxValue;

		private Dictionary<byte, CorpsePart> corpseParts = new Dictionary<byte, CorpsePart>();

		private float velocityCheckTimer;

		private float takeoverTimer;

		private float movementReceivedTime = -1f;

		private Vector3 movementTargetLocalPosition;

		private Quaternion movementTargetLocalRotation;

		private Vector3 movementVelocity;

		private Vector3 movementAngularVelocity;

		private Gender Gender;

		public override SpaceObjectType Type => SpaceObjectType.Corpse;

		public Outfit CurrentOutfit { get; private set; }

		public Rigidbody RigidBody => corpseParts[hipsKey].RBody;

		public bool IsKinematic => RigidBody.isKinematic;

		public override SpaceObject Parent
		{
			get
			{
				return base.Parent;
			}
			set
			{
				base.Parent = value;
				SetParentTransferableObjectsRoot();
			}
		}

		public static Corpse SpawnCorpse(SpawnObjectResponseData data)
		{
			SpawnCorpseResponseData spawnCorpseResponseData = data as SpawnCorpseResponseData;
			return SpawnCorpse(spawnCorpseResponseData.Details, null);
		}

		public static Corpse SpawnCorpse(Corpse template)
		{
			GameObject gameObject = ((template.Gender != 0) ? (UnityEngine.Object.Instantiate(Resources.Load("Models/Units/Characters/CharacterCorpseFemale"), new Vector3(20000f, 20000f, 20000f), Quaternion.identity) as GameObject) : (UnityEngine.Object.Instantiate(Resources.Load("Models/Units/Characters/CharacterCorpse"), new Vector3(20000f, 20000f, 20000f), Quaternion.identity) as GameObject));
			gameObject.SetActive(value: false);
			Corpse component = gameObject.GetComponent<Corpse>();
			component.Gender = template.Gender;
			component.GUID = template.GUID;
			component.animHelper.CreateRig();
			component.InitializeInventory();
			component.ReconnectAllBones();
			component.gameObject.name = "Corpse_" + component.GUID;
			Client.Instance.AddCorpse(component.GUID, component);
			component.Parent = template.Parent;
			component.transform.position = template.transform.position;
			component.transform.rotation = template.transform.rotation;
			foreach (KeyValuePair<AnimatorHelper.HumanBones, Transform> bone in template.animHelper.GetBones())
			{
				if (component.corpseParts.ContainsKey((byte)bone.Key))
				{
					component.corpseParts[(byte)bone.Key].Trans.rotation = bone.Value.rotation;
				}
			}
			component.SetKinematic(toggle: false);
			gameObject.SetActive(value: true);
			component.ragdollComponent.ToggleRagdoll(enabled: true, component);
			return component;
		}

		public static Corpse SpawnCorpse(CorpseDetails details, OtherPlayer playerThatDied)
		{
			if (Client.Instance.GetCorpse(details.GUID) != null)
			{
				return null;
			}
			SpaceObject @object = Client.Instance.GetObject(details.ParentGUID, details.ParentType);
			if (@object == null)
			{
				Dbg.Error("Cannot spawn corpse because there is no corpse parent", details.GUID, details.ParentGUID, details.ParentType);
				return null;
			}
			GameObject gameObject = ((details.Gender != 0) ? (UnityEngine.Object.Instantiate(Resources.Load("Models/Units/Characters/CharacterCorpseFemale"), new Vector3(20000f, 20000f, 20000f), Quaternion.identity) as GameObject) : (UnityEngine.Object.Instantiate(Resources.Load("Models/Units/Characters/CharacterCorpse"), new Vector3(20000f, 20000f, 20000f), Quaternion.identity) as GameObject));
			gameObject.SetActive(value: false);
			Corpse component = gameObject.GetComponent<Corpse>();
			component.Gender = details.Gender;
			component.GUID = details.GUID;
			component.animHelper.CreateRig();
			component.InitializeInventory();
			component.ReconnectAllBones();
			component.gameObject.name = "Corpse_" + component.GUID;
			Client.Instance.AddCorpse(component.GUID, component);
			if (details.DynamicObjectData != null && details.DynamicObjectData.Count > 0)
			{
				DynamicObjectDetails dynamicObjectDetails = details.DynamicObjectData.Find((DynamicObjectDetails x) => x.AttachData.IsAttached && x.AttachData.InventorySlotID == -2);
				if (dynamicObjectDetails != null)
				{
					DynamicObject.SpawnDynamicObject(dynamicObjectDetails, component);
				}
				foreach (DynamicObjectDetails dynamicObjectDatum in details.DynamicObjectData)
				{
					if (dynamicObjectDatum != dynamicObjectDetails)
					{
						DynamicObject.SpawnDynamicObject(dynamicObjectDatum, component);
					}
				}
			}
			component.Parent = @object;
			component.transform.localPosition = details.LocalPosition.ToVector3();
			component.transform.localRotation = details.LocalRotation.ToQuaternion();
			if (playerThatDied != null)
			{
				component.CopyPositionFromPlayer(playerThatDied);
				component.SetGravity(playerThatDied.Gravity);
			}
			component.SetKinematic(toggle: false);
			gameObject.SetActive(value: true);
			component.ragdollComponent.ToggleRagdoll(enabled: true, component);
			if (component.Inventory.ItemInHands != null)
			{
				Vector3 value = component.transform.parent.InverseTransformPoint(component.transform.position);
				component.Inventory.ItemInHands.DynamicObj.SendAttachMessage(MyPlayer.Instance.Parent, null, value, Quaternion.identity, Vector3.zero, Vector3.zero, MyPlayer.Instance.rigidBody.velocity);
			}
			return component;
		}

		public void Connect()
		{
			EventSystem.AddListener(typeof(CorpseStatsMessage), CorpseStatsMessageListener);
		}

		public void Disconnect()
		{
			EventSystem.RemoveListener(typeof(CorpseStatsMessage), CorpseStatsMessageListener);
		}

		private void Awake()
		{
			InitializeInventory();
			Connect();
		}

		private void FixedUpdate()
		{
			if (Client.IsGameBuild && !IsDestroying && !IsKinematic && sendMovementTime + SendMovementInterval <= Time.fixedTime)
			{
				sendMovementTime = Time.fixedTime;
				SendMovementMessage();
			}
		}

		private void SendMovementMessage()
		{
			CorpseMovementMessage corpseMovementMessage = new CorpseMovementMessage();
			corpseMovementMessage.GUID = base.GUID;
			corpseMovementMessage.LocalPosition = base.transform.localPosition.ToArray();
			corpseMovementMessage.LocalRotation = base.transform.localRotation.ToArray();
			corpseMovementMessage.Velocity = corpseParts[hipsKey].RBody.velocity.ToArray();
			corpseMovementMessage.AngularVelocity = corpseParts[hipsKey].RBody.angularVelocity.ToArray();
			corpseMovementMessage.Timestamp = Time.time;
			corpseMovementMessage.IsInsideSpaceObject = base.IsInsideSpaceObject;
			corpseMovementMessage.RagdollDataList = new Dictionary<byte, RagdollItemData>();
			CorpseMovementMessage corpseMovementMessage2 = corpseMovementMessage;
			CorpsePart corpsePart = corpseParts[hipsKey];
			if (corpsePart.Trans.hasChanged)
			{
				corpseMovementMessage2.RagdollDataList.Add(hipsKey, new RagdollItemData
				{
					Position = corpsePart.Trans.localPosition.ToArray(),
					LocalRotation = corpsePart.Trans.localRotation.ToArray(),
					Velocity = corpsePart.RBody.velocity.ToArray(),
					AngularVelocity = corpsePart.RBody.angularVelocity.ToArray()
				});
				corpsePart.Trans.hasChanged = false;
			}
			NetworkController.Instance.SendToGameServer(corpseMovementMessage2);
		}

		public void CopyPositionFromPlayer(OtherPlayer player)
		{
			foreach (KeyValuePair<AnimatorHelper.HumanBones, Transform> bone in player.AnimHelper.GetBones())
			{
				if (corpseParts.ContainsKey((byte)bone.Key))
				{
					corpseParts[(byte)bone.Key].Trans.position = bone.Value.position;
					corpseParts[(byte)bone.Key].Trans.rotation = bone.Value.rotation;
				}
			}
		}

		public void InitializeInventory()
		{
			if (Inventory == null)
			{
				Inventory = new Inventory(this, animHelper);
			}
		}

		public void ReconnectAllBones()
		{
			corpseParts.Clear();
			corpseParts = new Dictionary<byte, CorpsePart>();
			foreach (KeyValuePair<AnimatorHelper.HumanBones, Transform> bone in animHelper.GetBones())
			{
				RagdollCollider component = bone.Value.GetComponent<RagdollCollider>();
				Rigidbody rigidbody = null;
				if (component != null)
				{
					if (hipsKey == byte.MaxValue && bone.Key == AnimatorHelper.HumanBones.Hips)
					{
						hipsKey = (byte)bone.Key;
					}
					component.enabled = true;
					component.CorpseObject = this;
					rigidbody = bone.Value.GetComponent<Rigidbody>();
					if (rigidbody == null)
					{
						Dbg.Error("Missing rigidbody on ragdoll colliders", component.name, base.GUID);
						return;
					}
					rigidbody.useGravity = false;
					rigidbody.isKinematic = true;
					Collider component2 = bone.Value.gameObject.GetComponent<Collider>();
					component2.isTrigger = false;
				}
				corpseParts.Add((byte)bone.Key, new CorpsePart
				{
					RBody = rigidbody,
					Trans = bone.Value.transform
				});
			}
			DynamicObject[] componentsInChildren = GetComponentsInChildren<DynamicObject>(includeInactive: true);
			foreach (DynamicObject dynamicObject in componentsInChildren)
			{
				dynamicObject.ToggleKinematic(value: true);
				dynamicObject.ToggleEnabled(isEnabled: true, toggleColliders: true);
			}
			if (Gender == Gender.Female)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Models/Units/Characters/Hairs/Female/Hair1")) as GameObject;
				gameObject.transform.parent = animHelper.GetBone(AnimatorHelper.HumanBones.Head);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localScale = Vector3.one;
			}
			SetKinematic(toggle: true);
		}

		private void SetKinematic(bool toggle)
		{
			foreach (KeyValuePair<byte, CorpsePart> corpsePart in corpseParts)
			{
				if (corpsePart.Value.RBody != null)
				{
					corpsePart.Value.RBody.isKinematic = toggle;
				}
			}
		}

		private void RemoveOutfit()
		{
			if (CurrentOutfit != null)
			{
				CurrentOutfit.SetOutfitParent(outfitTransform.GetChildren(), CurrentOutfit.OutfitTrans, activateGeometry: false);
				CurrentOutfit.FoldedOutfitTrans.gameObject.SetActive(value: true);
				return;
			}
			foreach (Transform child in outfitTransform.GetChildren())
			{
				child.parent = basicOutfitHolder;
				child.gameObject.SetActive(value: false);
			}
		}

		public void SetOutfitParent(List<Transform> children, Transform parentTransform, bool activeGeometry)
		{
			foreach (Transform child in children)
			{
				child.parent = parentTransform;
				child.localScale = Vector3.one;
				child.localPosition = Vector3.zero;
				child.localRotation = Quaternion.Euler((!(child.name == "Root")) ? Vector3.zero : new Vector3(0f, 90f, -90f));
				child.gameObject.SetActive(activeGeometry);
			}
		}

		public void EquipOutfit(Outfit o)
		{
			o.FoldedOutfitTrans.gameObject.SetActive(value: false);
			o.transform.parent = base.transform;
			RemoveOutfit();
			CurrentOutfit = o;
			SetOutfitParent(o.OutfitTrans.GetChildren(), outfitTransform, activeGeometry: true);
			RefreshOutfitData();
			Inventory.SetOutfit(o);
		}

		public void TakeOffOutfit()
		{
			ragdollComponent.ToggleRagdoll(enabled: false, this);
			RemoveOutfit();
			UnityEngine.Object.Destroy(base.gameObject);
			SpawnCorpse(this);
		}

		private void RefreshHeadBones()
		{
			Transform[] array = new Transform[headSkin.bones.Length];
			Transform bone = animHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
			for (int i = 0; i < headSkin.bones.Length; i++)
			{
				array[i] = bone.FindChildByName(headSkin.bones[i].name);
			}
			headSkin.bones = array;
		}

		public void RefreshOutfitData()
		{
			animHelper.CreateRig();
			ragdollComponent.RefreshRagdollVariables();
			ReconnectAllBones();
			centerOfMass.transform.parent = animHelper.GetBone(AnimatorHelper.HumanBones.Spine2);
			centerOfMass.localScale = Vector3.one;
			centerOfMass.transform.localPosition = new Vector3(-0.133f, 0.014f, 0.001f);
			centerOfMass.transform.localRotation = Quaternion.Euler(97.33099f, -90f, 0.2839966f);
			RefreshHeadBones();
		}

		private void SendStatsMessage()
		{
			CorpseStatsMessage corpseStatsMessage = new CorpseStatsMessage();
			corpseStatsMessage.GUID = base.GUID;
			corpseStatsMessage.ParentGUID = Parent.GUID;
			corpseStatsMessage.ParentType = Parent.Type;
			corpseStatsMessage.LocalPosition = base.transform.localPosition.ToArray();
			corpseStatsMessage.LocalRotation = base.transform.localRotation.ToArray();
			CorpseStatsMessage data = corpseStatsMessage;
			NetworkController.Instance.SendToGameServer(data);
		}

		public InventorySlot GetInventorySlot(short attachedToID)
		{
			return Inventory.GetSlotByID(attachedToID);
		}

		public void AddForce(Vector3 force, ForceMode forceMode)
		{
			if (corpseParts.ContainsKey(hipsKey) && corpseParts[hipsKey].RBody != null)
			{
				AddForce(corpseParts[hipsKey].RBody, force, forceMode);
			}
		}

		public void AddTorque(Vector3 torque, ForceMode forceMode)
		{
			if (corpseParts.ContainsKey(hipsKey) && corpseParts[hipsKey].RBody != null)
			{
				AddTorque(corpseParts[hipsKey].RBody, torque, forceMode);
			}
		}

		public void AddForce(Rigidbody rbody, Vector3 force, ForceMode forceMode)
		{
			rbody.isKinematic = false;
			rbody.AddForce(force, forceMode);
		}

		public void AddTorque(Rigidbody rbody, Vector3 torque, ForceMode forceMode)
		{
			rbody.isKinematic = false;
			rbody.AddTorque(torque, forceMode);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (Client.IsGameBuild)
			{
				Disconnect();
				Client.Instance.RemoveCorpse(base.GUID);
			}
		}

		public void ProcessMoveCorpseObectMessage(CorpseMovementMessage mm)
		{
			ToggleKinematic(value: true);
			movementReceivedTime = Time.time;
			movementTargetLocalPosition = mm.LocalPosition.ToVector3();
			movementTargetLocalRotation = mm.LocalRotation.ToQuaternion();
			movementVelocity = mm.Velocity.ToVector3();
			movementAngularVelocity = mm.AngularVelocity.ToVector3();
		}

		private void CorpseStatsMessageListener(NetworkData data)
		{
			CorpseStatsMessage corpseStatsMessage = data as CorpseStatsMessage;
			if (corpseStatsMessage.GUID != base.GUID)
			{
				return;
			}
			if (corpseStatsMessage.DestroyCorpse)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else if (!(Parent is Pivot) && corpseStatsMessage.ParentType == SpaceObjectType.CorpsePivot)
			{
				ArtificialBody artificialBody = Parent as ArtificialBody;
				if (artificialBody == null)
				{
					Dbg.Error("Corpse exited vessel but we don't know from where.", base.GUID, Parent, corpseStatsMessage.ParentType, corpseStatsMessage.ParentGUID);
				}
				else
				{
					Pivot pivot = Client.Instance.SolarSystem.GetArtificialBody(base.GUID) as Pivot;
					if (pivot == null)
					{
						pivot = Pivot.Create(SpaceObjectType.CorpsePivot, base.GUID, artificialBody, isMainObject: false);
					}
					Parent = pivot;
				}
			}
			else if (Parent is Pivot && (corpseStatsMessage.ParentType == SpaceObjectType.Ship || corpseStatsMessage.ParentType == SpaceObjectType.Station || corpseStatsMessage.ParentType == SpaceObjectType.Asteroid))
			{
				Client.Instance.SolarSystem.RemoveArtificialBody(Parent as Pivot);
				UnityEngine.Object.Destroy(Parent.gameObject);
				Parent = Client.Instance.GetVessel(corpseStatsMessage.ParentGUID);
				TransitionTrigger.ResetTriggers();
			}
		}

		public override void OnGravityChanged(Vector3 oldGravity)
		{
			if (!(oldGravity != Vector3.zero) || !base.Gravity.IsEpsilonEqual(Vector3.zero))
			{
				return;
			}
			foreach (KeyValuePair<byte, CorpsePart> corpsePart in corpseParts)
			{
				if (corpsePart.Value.RBody != null)
				{
					corpsePart.Value.RBody.AddForce(new Vector3(UnityEngine.Random.Range(0.001f, 0.05f), UnityEngine.Random.Range(0.001f, 0.05f), UnityEngine.Random.Range(0.001f, 0.05f)), ForceMode.Impulse);
					corpsePart.Value.RBody.AddTorque(new Vector3(UnityEngine.Random.Range(0.001f, 0.05f), UnityEngine.Random.Range(0.001f, 0.05f), UnityEngine.Random.Range(0.001f, 0.05f)));
				}
			}
		}

		public override void DockedVesselParentChanged(SpaceObjectVessel vessel)
		{
			Parent = vessel;
			SendStatsMessage();
		}

		public override void RoomChanged(SceneTriggerRoom prevRoomTrigger)
		{
			base.RoomChanged(prevRoomTrigger);
		}

		public override void EnterVessel(SpaceObjectVessel vessel)
		{
			if (!IsKinematic && !(vessel == null))
			{
				if (Parent is Pivot && Parent != vessel)
				{
					Client.Instance.SolarSystem.RemoveArtificialBody(Parent as Pivot);
					UnityEngine.Object.Destroy(Parent.gameObject);
				}
				Parent = vessel;
				SendStatsMessage();
			}
		}

		public override void ExitVessel(bool forceExit)
		{
			if (!IsKinematic)
			{
				ArtificialBody artificialBody = ((!(Parent is SpaceObjectVessel)) ? (Parent as ArtificialBody) : (Parent as SpaceObjectVessel).MainVessel);
				if (artificialBody == null)
				{
					Dbg.Error("Corpse cannot exit vessel, cannot find parents artificial body", base.name, base.GUID);
				}
				else
				{
					Pivot pivot = (Pivot)(Parent = Pivot.Create(SpaceObjectType.CorpsePivot, base.GUID, artificialBody, isMainObject: false));
					SendStatsMessage();
				}
			}
		}

		public List<Item> AllItems()
		{
			List<Item> list = new List<Item>();
			if (Inventory != null)
			{
				if (Inventory.Outfit != null)
				{
					foreach (KeyValuePair<short, InventorySlot> inventorySlot in Inventory.Outfit.InventorySlots)
					{
						if (inventorySlot.Value.Item != null)
						{
							list.Add(inventorySlot.Value.Item);
						}
					}
				}
				if (Inventory.HandsSlot != null && Inventory.HandsSlot.Item != null)
				{
					list.Add(Inventory.HandsSlot.Item);
				}
			}
			if (list.Count == 0)
			{
				return null;
			}
			return list;
		}

		private void Update()
		{
			Transform parent = corpseParts[hipsKey].Trans.parent;
			corpseParts[hipsKey].Trans.parent = null;
			base.transform.position = corpseParts[hipsKey].Trans.position;
			base.transform.rotation = corpseParts[hipsKey].Trans.rotation;
			corpseParts[hipsKey].Trans.parent = parent;
			float num = Time.time - movementReceivedTime;
			if (!IsKinematic)
			{
				if (RigidBody.velocity.IsEpsilonEqual(Vector3.zero, 0.1f) && RigidBody.angularVelocity.IsEpsilonEqual(Vector3.zero, 0.5f))
				{
					velocityCheckTimer += Time.deltaTime;
					if (velocityCheckTimer > 1f)
					{
						SendMovementMessage();
						ToggleKinematic(value: true);
					}
				}
				else
				{
					velocityCheckTimer = 0f;
				}
			}
			else if (movementReceivedTime > 0f && num < 1f)
			{
				base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, movementTargetLocalPosition, Mathf.Pow(num, 0.5f));
				base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, movementTargetLocalRotation, Mathf.Pow(num, 0.5f));
			}
			else if (num > 1f && num - Time.deltaTime <= 1f)
			{
				ToggleKinematic(value: false);
				RigidBody.velocity = movementVelocity;
				RigidBody.angularVelocity = movementAngularVelocity;
			}
			takeoverTimer += Time.deltaTime;
		}

		public void ToggleKinematic(bool value)
		{
			if (value)
			{
				if (takeoverTrigger == null)
				{
					takeoverTrigger = base.gameObject.AddComponent<SphereCollider>();
					takeoverTrigger.isTrigger = true;
				}
			}
			else
			{
				if (takeoverTrigger != null)
				{
					GameObject.Destroy(takeoverTrigger);
				}
				velocityCheckTimer = 0f;
			}
			RigidBody.isKinematic = value;
		}

		public void PushedByMyPlayer(Vector3 relativeVelocity)
		{
			if (IsKinematic)
			{
				takeoverTimer = 0f;
				AddForce(relativeVelocity, ForceMode.VelocityChange);
			}
		}

		public InventorySlot GetSlotByID(short id)
		{
			switch (id)
			{
			case -1:
				return Inventory.HandsSlot;
			case -2:
				return Inventory.OutfitSlot;
			default:
				if (CurrentOutfit != null)
				{
					return CurrentOutfit.GetSlotByID(id);
				}
				return null;
			}
		}

		public Dictionary<short, InventorySlot> GetAllSlots()
		{
			InventorySlot inventorySlot = CurrentOutfit.InventorySlots.Values.FirstOrDefault((InventorySlot m) => m.SlotGroup == InventorySlot.Group.Jetpack);
			InventorySlot inventorySlot2 = CurrentOutfit.InventorySlots.Values.FirstOrDefault((InventorySlot m) => m.SlotGroup == InventorySlot.Group.Helmet);
			Dictionary<short, InventorySlot> dictionary = new Dictionary<short, InventorySlot>();
			dictionary.Add(-1, Inventory.HandsSlot);
			dictionary.Add(-2, Inventory.OutfitSlot);
			dictionary.Add(inventorySlot.SlotID, inventorySlot);
			dictionary.Add(inventorySlot2.SlotID, inventorySlot2);
			return dictionary;
		}

		public Dictionary<short, InventorySlot> GetSlotsByGroup(InventorySlot.Group group)
		{
			return (from m in GetAllSlots()
				where m.Value.SlotGroup == @group
				select m).ToDictionary((KeyValuePair<short, InventorySlot> k) => k.Key, (KeyValuePair<short, InventorySlot> v) => v.Value);
		}
	}
}
